
using System;
using System.Collections.Generic;
using System.Linq;
using nWins.Lib.Core;
using nWins.Lib.Factory;
using nWins.Lib.Settings;
using nWins.Lib.Storage;

namespace nWins.Lib.Agent;

/// <summary>
/// A trainable agent using the SARSA-Lambda Q learning algorithm (incl. eligibility traces) for training.
/// </summary>
public class SarsaLambdaAgent : ITrainableAgent
{
    /// <summary>
    /// Creat a simple Q-learning agent from settings.
    /// </summary>
    /// <param name="settings">The settings used for agent creation.</param>
    /// <param name="side">The side the agent is acting for.</param>
    public SarsaLambdaAgent(ITrainableAgentSettings settings, GameSide side)
    {
        _side = side;
        LoadFromSettings(settings);
    }

    private const string PARAM_ALPHA = "alpha";
    private const string PARAM_GAMMA = "gamma";
    private const string PARAM_LAMBDA = "lambda";

    private static readonly Random _random = new Random();

    private GameSide _side;
    private ITrainableAgentSettings _settings;
    private AgentMode _mode = AgentMode.Training;

    private double _alpha;
    private double _gamma;
    private double _lambda;
    private IExplorationStrategy _expStrategy;
    private IExplorationStrategy _infStrategy;
    private IExplorationStrategy _semiGreedyStrategy;

    private QTable _table = new QTable();

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
            if (!settings.Params.ContainsKey(PARAM_LAMBDA)) { throw new ArgumentException("Invalid arguments! Missing lambda on trainable agent settings!"); }

            // initialize learning hyper-params
            _alpha = (double)settings.Params[PARAM_ALPHA];
            _gamma = (double)settings.Params[PARAM_GAMMA];
            _lambda = (double)settings.Params[PARAM_LAMBDA];
            _expStrategy = ExplorationStrategyFactory.CreateStrategy(settings.Params);
        }
        Console.WriteLine($"agent settings: side={ _side }, alpha={ _alpha }, gamma={ _gamma }, lambda={ _lambda }");
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
        
        // Initialize eligibility traces
        var eligibilityTraces = new QTable();

        // get logs from summary
        var logs = _side == GameSide.SideA ? summary.ActionsSideA : summary.ActionsSideB;

        // loop through all action logs
        for (int i = 0; i < logs.Count(); i++)
        {
            var log = logs.ElementAt(i);
            // make sure that the Q table is initialized for the log's state
            if (!_table.ContainsKey(log.OldState)) { initQValue(log.OldState); }

            // make sure eligibility traces is initialized for the log's states
            if (!eligibilityTraces.ContainsKey(log.OldState)) { initEligibilityTrace(eligibilityTraces, log.OldState); }

            // make sure the Q tables is initialized for the new state (after any opponent action)
            var nextState = logs.ElementAtOrDefault(i + 1).OldState ?? logs.ElementAt(i).NewState;
            if (!_table.ContainsKey(nextState)) { initQValue(nextState); }

            // make sure eligibility traces  is initializedfor the next state
            if (!eligibilityTraces.ContainsKey(nextState)) { initEligibilityTrace(eligibilityTraces, nextState); }

            // update Q table using the Q learning formula
            updateQTable(log, nextState, eligibilityTraces);
        }

        // update strategy
        _expStrategy.NextEpisode(summary);
    }

    private void initEligibilityTrace(QTable eligibilityTraces, IGameState state)
    {
        // initialize the eligibility trace with 0 for all possible actions
        var entry = new Dictionary<GameAction, double>();
        foreach (var action in state.GetPossibleActions()) { entry.Add(action, 0.0); }
        eligibilityTraces.Add(state, entry);
    }

    private void initQValue(IGameState state)
    {
        // initialize the state with uniform random Q value for all possible actions
        var entry = new Dictionary<GameAction, double>();
        foreach (var action in state.GetPossibleActions()) { entry.Add(action, _random.NextDouble()); }
        _table.Add(state, entry);
    }

    private void updateQTable(ActionLog log, IGameState nextState, QTable eligibilityTraces)
    {
        // (S', A') standard value for terminal state is 0
        double q_a_next_state = 0.0;

        // get A' for non terminal state S'(epsilon greedy) 
        if (nextState.GetPossibleActions().Count() != 0) {

            // make sure that the state exists in Q table
            if (!_table.ContainsKey(nextState)) { initQValue(nextState); }

            // choose an action according to the exploration strategy
            var actionNextState = _expStrategy.ChooseAction(_table[nextState]);
            q_a_next_state = _table[nextState][actionNextState];
        }

        // make sure that the state exists in eligibility traces
        if (!eligibilityTraces.ContainsKey(nextState)) { initEligibilityTrace(eligibilityTraces, nextState); }

        // Calculate delta
        double delta = log.Reward + _gamma * q_a_next_state - _table[log.OldState][log.Action];

        // Increment eligibility trace of current (state, action) tupel
        eligibilityTraces[log.OldState][log.Action] = eligibilityTraces[log.OldState][log.Action] + 1; 

        // Update all (relevant) Q table and eligibility trace values
        // Only the (state, action) tuples in eligibility traces are considered, as the update would otherwise be 0
        foreach (var entry_state in eligibilityTraces)
        {
            foreach (var entry_action in entry_state.Value)
            {
                // Q update with discounted eligibility trace
                _table[entry_state.Key][entry_action.Key] = _table[entry_state.Key][entry_action.Key] 
                    + _alpha * delta * eligibilityTraces[entry_state.Key][entry_action.Key];

                // discount eligibility trace
                eligibilityTraces[entry_state.Key][entry_action.Key] = 
                    _gamma * _lambda * eligibilityTraces[entry_state.Key][entry_action.Key];
            }
        }
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
