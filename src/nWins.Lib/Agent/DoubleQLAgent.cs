
using System;
using System.Collections.Generic;
using System.Linq;
using nWins.Lib.Core;
using nWins.Lib.Factory;
using nWins.Lib.Settings;
using nWins.Lib.Storage;

namespace nWins.Lib.Agent
{
    /// <summary>
    /// A trainable agent using the double Q learning algorithm for training.
    /// </summary>
    public class DoubleQLAgent : ITrainableAgent
    {
        /// <summary>
        /// Creat a double Q-learning agent from settings.
        /// </summary>
        /// <param name="settings">The settings used for agent creation.</param>
        /// <param name="side">The side the agent is acting for.</param>
        public DoubleQLAgent(ITrainableAgentSettings settings, GameSide side)
        {
            _side = side;
            LoadFromSettings(settings);
        }

        private const string PARAM_ALPHA = "alpha";
        private const string PARAM_GAMMA = "gamma";
        private const string PARAM_COMBINE_QTABLES = "combine_qtables";
        private const string COMBINE_TYPE_SUM = "sum";

        private const string COMBINE_TYPE_AVG = "avg";

        private static readonly Random _random = new Random();

        private GameSide _side;
        private ITrainableAgentSettings _settings;
        private AgentMode _mode = AgentMode.Training;

        private string _combineType;

        private double _alpha;
        private double _gamma;
        private IExplorationStrategy _expStrategy;
        private IExplorationStrategy _infStrategy;
        private IExplorationStrategy _semiGreedyStrategy;

        private QTable _tableOne = new QTable();

        private QTable _tableTwo = new QTable();

        public void LoadFromSettings(ITrainableAgentSettings settings)
        {
            _settings = settings;
            _mode = settings.IsTrainable ? AgentMode.Training : AgentMode.Inference;
            _infStrategy = new GreedyStrategy();
            _semiGreedyStrategy = new SemiGreedyInferenceStrategy(settings.epsilon);
            _combineType = COMBINE_TYPE_SUM;

            if (settings.IsTrainable)
            {
                // make sure that alpha, gamma and combine qtables type are specified
                if (!settings.Params.ContainsKey(PARAM_ALPHA)) { throw new ArgumentException("Invalid arguments! Missing alpha on trainable agent settings!"); }
                if (!settings.Params.ContainsKey(PARAM_GAMMA)) { throw new ArgumentException("Invalid arguments! Missing gamma on trainable agent settings!"); }
                if (!settings.Params.ContainsKey(PARAM_COMBINE_QTABLES)) { throw new ArgumentException("Invalid arguments! Missing combine qtables type on trainable agent settings!"); }

                // make sure combine qtables type is available
                if (!settings.Params[PARAM_COMBINE_QTABLES].Equals(COMBINE_TYPE_SUM) && !settings.Params[PARAM_COMBINE_QTABLES].Equals(COMBINE_TYPE_AVG)) { throw new ArgumentException("Invalid arguments! Combine qtables type is not available!"); }

                // initialize learning hyper-params
                _alpha = (double)settings.Params[PARAM_ALPHA];
                _gamma = (double)settings.Params[PARAM_GAMMA];
                _expStrategy = ExplorationStrategyFactory.CreateStrategy(settings.Params);

                // initialize combine type
                _combineType = (String) settings.Params[PARAM_COMBINE_QTABLES];
            }

            Console.WriteLine($"agent settings: side={ _side }, alpha={ _alpha }, gamma={ _gamma }");
        }

        public void SetMode(AgentMode mode) => _mode = mode;

        #region IAgent

        public GameAction ChooseAction(IGameState state, GameAction? oppAction)
        {
            // make sure that the state exists in both Q tables
            if (!_tableOne.ContainsKey(state)) { initQValue(state, _tableOne); }
            if (!_tableTwo.ContainsKey(state)) { initQValue(state, _tableTwo); }

            // Create one ActionDictionary for both qtables in explicit state
            Dictionary<GameAction, double> _combinedActions = combineActionsInState(state);

             // choose an action using the exploration strategy
            var strategy = _expStrategy;
            switch(_mode) 
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
            return strategy.ChooseAction(_combinedActions);
        }

        private Dictionary<GameAction, double> combineActionsInState(IGameState state)
        {
             if(_combineType.Equals(COMBINE_TYPE_SUM)) {
                // Sum both ActionDictionarys for explicit state
                return _tableOne[state].Concat(_tableTwo[state]).GroupBy(x => x.Key).ToDictionary(x => x.Key, x => x.Sum(y => y.Value));
            } else
            {
                // CombinedActions is average of both ActionDictionarys for explicit state
                return _tableOne[state].Concat(_tableTwo[state]).GroupBy(x => x.Key).ToDictionary(x => x.Key, x => x.Average(y => y.Value));
            }   
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
                if (!_tableOne.ContainsKey(log.OldState)) { initQValue(log.OldState, _tableOne); }
                if (!_tableTwo.ContainsKey(log.OldState)) { initQValue(log.OldState, _tableTwo); }

                // make sure the Q tables are initialized for the new state (after any opponent action)
                var nextState = logs.ElementAtOrDefault(i + 1).OldState ?? logs.ElementAt(i).NewState;
                if (!_tableOne.ContainsKey(nextState)) { initQValue(nextState, _tableOne); }
                if (!_tableTwo.ContainsKey(nextState)) { initQValue(nextState, _tableTwo); }

                // update Q table using the Q learning formula
                updateQTable(log, nextState);
            }

            // update strategy
            _expStrategy.NextEpisode(summary);
        }

        private void initQValue(IGameState state, QTable _table)
        {
            // initialize the state with uniform random Q value for all possible actions
            var entry = new Dictionary<GameAction, double>();
            foreach (var action in state.GetPossibleActions()) { entry.Add(action, _random.NextDouble()); }
            _table.Add(state, entry);
        }

        private void updateQTable(ActionLog log, IGameState nextState)
        {
            // decide random which qtable should be updated
            QTable _q1Table;
            QTable _q2Table;

            if(_random.NextDouble() < 0.5) {
                _q1Table = _tableOne;
                _q2Table = _tableTwo;
            } else
            {
                _q1Table = _tableTwo;
                _q2Table = _tableOne;
            }

            // apply the Q formula to the old state
            double Q_sa = _q1Table[log.OldState][log.Action];
            double Q_maxNextState = log.IsTerminal ? 0 : _q2Table[nextState][_q1Table[nextState].OrderBy(action => action.Value).Last().Key];
            double newQValue = Q_sa + _alpha * (log.Reward + _gamma * Q_maxNextState - Q_sa);
            _q1Table[log.OldState][log.Action] = newQValue;
        }

        #endregion IAgent

        #region ISerializableAgent

        public void LoadModel(string filePath)
        {
            _tableOne = QTableCSVSerializer.Deserialize(filePath);
            _tableTwo = QTableCSVSerializer.Deserialize(filePath);
        }

        public void StoreModel(string filePath)
        {
            QTable _table = new QTable();
            
            // Combine both qtables to store the model
            foreach(KeyValuePair<IGameState, IDictionary<GameAction, double>> entry in _tableOne)
            {
                _table.Add(entry.Key, combineActionsInState(entry.Key));
            }

            QTableCSVSerializer.Serialize(_table, filePath);        
        }

        #endregion ISerializableAgent
    }
}