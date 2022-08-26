
using System;
using System.Collections.Generic;
using System.Linq;
using nWins.Lib.Core;
using nWins.Lib.Factory;
using nWins.Lib.Settings;
using nWins.Lib.Storage;

namespace nWins.Lib.Agent;

/// <summary>
/// A trainable agent using the dynamic Q learning algorithm for training.
/// </summary>
public class DynaQLAgent : ITrainableAgent
{
    /// <summary>
    /// Creat a simple Q-learning agent from settings.
    /// </summary>
    /// <param name="settings">The settings used for agent creation.</param>
    /// <param name="side">The side the agent is acting for.</param>
    public DynaQLAgent(ITrainableAgentSettings settings, GameSide side)
    {
        _side = side;
        LoadFromSettings(settings);
    }

    private const string PARAM_ALPHA = "alpha";
    private const string PARAM_GAMMA = "gamma";
    private const string PARAM_DYNAQ_STEPS = "dynaq_steps";
    private const string PARAM_DYNAQ_MEMORY_SIZE = "dynaq_memory_size";
    private const string PARAM_DYNAQ_MIN_MEMORY = "dynaq_min_memory";
    private static readonly Random _random = new Random();

    private GameSide _side;
    private ITrainableAgentSettings _settings;
    private AgentMode _mode = AgentMode.Training;

    private double _alpha;
    private double _gamma;
    private IExplorationStrategy _expStrategy;
    private IExplorationStrategy _infStrategy;
    private IExplorationStrategy _semiGreedyStrategy;

    private QTable _table = new QTable();

    // experience memory as ring buffer (fade out too old memories with a queue)
    private RingBuffer<ActionLog> _experienceMemory;

    private int _dynaQSteps;
    private int _dynaQMemorySize;
    private int _dynaQMinMemory;

    public void LoadFromSettings(ITrainableAgentSettings settings)
    {
        _settings = settings;
        _mode = settings.IsTrainable ? AgentMode.Training : AgentMode.Inference;
        _infStrategy = new GreedyStrategy();
        _semiGreedyStrategy = new SemiGreedyInferenceStrategy(settings.epsilon);

        if (settings.IsTrainable)
        {
            // make sure that alpha and gamma are specified
            if (!settings.Params.ContainsKey(PARAM_ALPHA)) { throw new ArgumentException("Invalid arguments! Missing alpha on trainable agent settings!"); }
            if (!settings.Params.ContainsKey(PARAM_GAMMA)) { throw new ArgumentException("Invalid arguments! Missing gamma on trainable agent settings!"); }

            // make sure that DynaQ Memory Size, Steps and MinMemory are specified
            if (!settings.Params.ContainsKey(PARAM_DYNAQ_MEMORY_SIZE)) { throw new ArgumentException("Invalid arguments! Missing DynaQ memory size on trainable agent settings!"); }
            if (!settings.Params.ContainsKey(PARAM_DYNAQ_STEPS)) { throw new ArgumentException("Invalid arguments! Missing DynaQ steps on trainable agent settings!"); }
            if (!settings.Params.ContainsKey(PARAM_DYNAQ_MIN_MEMORY)) { throw new ArgumentException("Invalid arguments! Missing DynaQ minimum memory on trainable agent settings!"); }

            // initialize learning hyper-params
            _alpha = (double)settings.Params[PARAM_ALPHA];
            _gamma = (double)settings.Params[PARAM_GAMMA];
            _expStrategy = ExplorationStrategyFactory.CreateStrategy(settings.Params);

            // initialize experience memory
            _dynaQMemorySize = (int)(long)settings.Params[PARAM_DYNAQ_MEMORY_SIZE];
            _dynaQSteps = (int)(long)settings.Params[PARAM_DYNAQ_STEPS];
            _dynaQMinMemory = (int)(long)settings.Params[PARAM_DYNAQ_MIN_MEMORY];
            _experienceMemory = new RingBuffer<ActionLog>(_dynaQMemorySize);
            Console.WriteLine($"experience memory settings: memorysize={ _dynaQMemorySize }, steps={ _dynaQSteps }, minMemory={ _dynaQMinMemory }");
        }

        Console.WriteLine($"agent settings: side={ _side }, alpha={ _alpha }, gamma={ _gamma }");
    }

    public void SetMode(AgentMode mode) => _mode = mode;

    #region IAgent

    public GameAction ChooseAction(IGameState state, GameAction? oppAction)
    {
        // make sure that the state exists in Q table
        if (!_table.ContainsKey(state)) { initQValue(state); }

        // choose an action using the exploration strategy
        var strategy = _expStrategy;
        switch (_mode)
        {
            case AgentMode.Training:
                strategy = _expStrategy;
                break;
            case AgentMode.Inference:
                strategy = _infStrategy;
                break;
            case AgentMode.SemiGreedy:
                strategy = _semiGreedyStrategy;
                break;
        }
        return strategy.ChooseAction(_table[state]);
    }

    public void OnGameOver(IGameSummary summary, GameAction? oppAction = null)
    {
        // abort learning step if agent is not in training mode
        if (!_settings.IsTrainable || _mode == AgentMode.Inference || _mode == AgentMode.SemiGreedy) { return; }

        // get logs from summary
        var logs = _side == GameSide.SideA ? summary.ActionsSideA : summary.ActionsSideB;

        // loop through all action logs
        for (int i = 0; i < logs.Count(); i++)
        {
            var log = logs.ElementAt(i);
            // make sure that the Q tables are initialized for the log's states
            if (!_table.ContainsKey(log.OldState)) { initQValue(log.OldState); }

            // make sure the Q tables are initialized for the new state (after any opponent action)
            var nextState = logs.ElementAtOrDefault(i + 1).OldState ?? logs.ElementAt(i).NewState;
            if (!_table.ContainsKey(nextState)) { initQValue(nextState); }

            // update Q table using the Q learning formula
            updateQTable(log, nextState);

            // update experience memory
            updateExperienceMemory(log, nextState);
        }
        // perform Dyna-Q learning steps using the experience memory
        if (_experienceMemory.Count >= _dynaQMinMemory)
        {
            // get random batch from experience memory and perform a Q table update
            var learnBatch = _experienceMemory.RandBatch(_dynaQSteps).ToList();
            learnBatch.ForEach(log => updateQTable(log, log.NewState));
        }

        // update strategy
        _expStrategy.NextEpisode(summary);
    }

    private void updateExperienceMemory(ActionLog log, IGameState nextState)
    {
        _experienceMemory.AddItem(new ActionLog(log.OldState, nextState, log.Action, log.Reward, log.IsTerminal));
    }


    private void initQValue(IGameState state)
    {
        // initialize the state with uniform random Q value for all possible actions
        var entry = new Dictionary<GameAction, double>();
        foreach (var action in state.GetPossibleActions()) { entry.Add(action, _random.NextDouble()); }
        _table.Add(state, entry);
    }

    private void updateQTable(ActionLog log, IGameState nextState)
    {
        // apply the Q formula to the old state
        double Q_sa = _table[log.OldState][log.Action];
        double Q_maxNextState = log.IsTerminal ? 0 : _table[nextState].Values.Max();
        double newQValue = Q_sa + _alpha * (log.Reward + _gamma * Q_maxNextState - Q_sa);
        _table[log.OldState][log.Action] = newQValue;
    }

    #endregion IAgent

    #region ISerializableAgent

    public void LoadModel(string filePath)
    {
        _table = QTableCSVSerializer.Deserialize(filePath);
    }

    public void StoreModel(string filePath)
    {
        QTableCSVSerializer.Serialize(_table, filePath);
    }

    #endregion ISerializableAgent
}
