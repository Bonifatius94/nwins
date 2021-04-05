using System;
using System.Collections.Generic;
using nWins.Lib.Agent;

namespace nWins.Lib.Factory
{
    /// <summary>
    /// A factory for initializing exploration strategies from settings.
    /// </summary>
    public static class ExplorationStrategyFactory
    {
        #region SettingNames

        // exploration strategy type setting
        private const string PARAM_EXP_STRATEGY = "exp_strategy";
        private const string EPSILON_GREEDY_LINEAR_DECAY = "epsilon_greedy_lindecay";
        private const string GREEDY_INFERENCE = "greedy_inference";
        private const string SEMI_GREEDY_INFERENCE = "semi_greedy_inference";

        // linear decaying epsilon greedy settings
        private const string PARAM_START_EPSILON = "start_epsilon";
        private const string PARAM_MIN_EPSILON = "min_epsilon";
        private const string PARAM_DECAY_EPISODES = "decay_episodes";

        // semi greedy inference settings
        private const string PARAM_SEMI_GREEDY_EPSILON = "semi_greedy_epsilon";

        #endregion SettingNames

        /// <summary>
        /// Create an exploration strategy from the given agent parameters.
        /// </summary>
        /// <param name="parameters">The agent parameters used for exploration strategy creation.</param>
        /// <returns>a new exploration strategy instance</returns>
        public static IExplorationStrategy CreateStrategy(Dictionary<string, object> parameters)
        {
            // make sure that the exploration strategy type is specified
            if (!parameters.ContainsKey(PARAM_EXP_STRATEGY)) { throw new ArgumentException("Invalid arguments! Exploration strategy type is not specified!"); }

            switch (parameters[PARAM_EXP_STRATEGY])
            {
                case EPSILON_GREEDY_LINEAR_DECAY: return createLinDecayEpsilonGreedyStrategy(parameters);
                case GREEDY_INFERENCE: return new GreedyStrategy();
                case SEMI_GREEDY_INFERENCE: return createSemiGreedyStrategy(parameters);
                default: throw new NotSupportedException($"Unsupported exploration strategy '{ parameters[PARAM_EXP_STRATEGY] }' detected!");
            }
        }

        private static IExplorationStrategy createLinDecayEpsilonGreedyStrategy(Dictionary<string, object> parameters)
        {
            if (!parameters.ContainsKey(PARAM_MIN_EPSILON) || !parameters.ContainsKey(PARAM_START_EPSILON) || !parameters.ContainsKey(PARAM_DECAY_EPISODES)) 
            { 
                throw new ArgumentException("Missing arguments! Epsilon greedy linear decay is not specified correctly!"); 
            }
            double minEpsilon = (double)parameters[PARAM_MIN_EPSILON];
            double startEpsilon = (double)parameters[PARAM_START_EPSILON];
            int decayEpisodes = (int)(long)parameters[PARAM_DECAY_EPISODES];
            return new LinearDecayingEpsilonGreedy(minEpsilon, startEpsilon, decayEpisodes);
        }

        private static IExplorationStrategy createSemiGreedyStrategy(Dictionary<string, object> parameters)
        {
            if (!parameters.ContainsKey(PARAM_SEMI_GREEDY_EPSILON)) 
            { 
                throw new ArgumentException("Missing arguments! Semi greedy Strategy is not specified correctly!"); 
            }
            double epsilon = (double)parameters[PARAM_SEMI_GREEDY_EPSILON];
            return new SemiGreedyInferenceStrategy(epsilon);
        }
    }
}