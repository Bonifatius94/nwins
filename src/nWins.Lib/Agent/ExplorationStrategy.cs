
using System;
using System.Collections.Generic;
using System.Linq;
using nWins.Lib.Core;

namespace nWins.Lib.Agent
{
    /// <summary>
    /// Representing an exploration strategy interface.
    /// </summary>
    public interface IExplorationStrategy
    {
        /// <summary>
        /// Choose an action from the given action ratings according to the exploration strategy implmenetation.
        /// </summary>
        /// <param name="actionRatings">The action ratings to be evaluated.</param>
        /// <returns>a single action from the input set</returns>
        GameAction ChooseAction(IDictionary<GameAction, double> actionRatings);

        /// <summary>
        /// Prepare the strategy implementation for the next episode.
        /// </summary>
        /// <param name="summary">The episode's game summary.</param>
        void NextEpisode(IGameSummary summary);
    }

    /// <summary>
    /// A parameterizable, linearly decaying epsilon greedy exploration strategy.
    /// </summary>
    public class LinearDecayingEpsilonGreedy : IExplorationStrategy
    {
        /// <summary>
        /// Create a new epsilon greedy exploration strategy instance with the given parameters.
        /// </summary>
        /// <param name="minEpsilon">The constant epsilon used after linear decay.</param>
        /// <param name="startEpsilon">The initial epsilon value being linearly decayed to the min epsilon.</param>
        /// <param name="decayEpisodes">The episodes until fully decay.</param>
        /// <param name="episode">The initial episode (default=0, gets incremented by calling NextEpisode() function).</param>
        public LinearDecayingEpsilonGreedy(double minEpsilon, double startEpsilon = 0, int decayEpisodes = 0, int episode = 0)
        {
            // make sure the input parameters are valid
            if (minEpsilon < 0 || minEpsilon > 1) { throw new ArgumentException("Invalid arguments! Min. Epsilon needs to be within [0; 1]!"); }
            if (startEpsilon < 0 || startEpsilon > 1) { throw new ArgumentException("Invalid arguments! Start Epsilon needs to be within [0; 1]!"); }
            if (minEpsilon > startEpsilon) { throw new ArgumentException("Invalid Arguments! Min. epsilon needs to be less or equal than start epsilon!"); }
            if (decayEpisodes < 0) { throw new ArgumentException("Invalid arguments! Decay episodes needs to be greater or equal than zero!"); }
            if (episode < 0) { throw new ArgumentException("Invalid arguments! Episode needs to be greater or equal than zero!"); }

            _minEpsilon = minEpsilon;
            _startEpsilon = startEpsilon;
            _decayEpisodes = decayEpisodes;
            _episode = episode;
        }

        private double _minEpsilon;
        private double _startEpsilon;
        private int _decayEpisodes;
        private int _episode;

        private static readonly Random _random = new Random();

        public GameAction ChooseAction(IDictionary<GameAction, double> actionRatings)
        {
            // make sure the action ratings set is not empty
            if (actionRatings.Count() == 0) { throw new ArgumentException("Invalid arguments! Action ratings set must not be empty!"); }

            // exploit: perform the greedy action
            bool isGreedyAction = _random.NextDouble() > getEpsilon();
            if (isGreedyAction) { return actionRatings.OrderByDescending(x => x.Value).First().Key; }

            // explore: perform a random possible action
            var actions = actionRatings.Keys;
            return actions.ElementAt(_random.Next() % actions.Count());
        }

        public void NextEpisode(IGameSummary summary) => _episode++;

        private double getEpsilon()
        {
            // linear decaying epsilon
            double dynFactor = (_episode < _decayEpisodes) ? (1 - _episode / _decayEpisodes) : 0;
            return dynFactor * (_startEpsilon - _minEpsilon) + _minEpsilon;
        }
    }

    /// <summary>
    /// A greedy-only acting exploration strategy always selecting the highest rated action (i.e. for inference use cases).
    /// </summary>
    public class GreedyStrategy : IExplorationStrategy
    {
        public GameAction ChooseAction(IDictionary<GameAction, double> actionRatings)
        {
            // make sure the action ratings set is not empty
            if (actionRatings.Count() == 0) { throw new ArgumentException("Invalid arguments! Action ratings set must not be empty!"); }

            // always choose the action with the best rating (100% greedy)
            return actionRatings.OrderByDescending(x => x.Value).First().Key;
        }

        public void NextEpisode(IGameSummary summary)
        {
            // nothing to do here ...
        }
    }

    /// <summary>
    /// A epsilon greedy acting exploration strategy that is selecting the highest rated action or the second highest rated in epsilon case.
    /// </summary>
    public class SemiGreedyInferenceStrategy : IExplorationStrategy
    {
        public SemiGreedyInferenceStrategy(double epsilon)
        {
            // make sure the input parameter is valid
            if (epsilon < 0 || epsilon > 1) { throw new ArgumentException("Invalid arguments! Min. Epsilon needs to be within [0; 1]!"); }

            _epsilon = epsilon;
        }

        private double _epsilon;
        private static readonly Random _random = new Random();

        public GameAction ChooseAction(IDictionary<GameAction, double> actionRatings)
        {
            // make sure the action ratings set is not empty
            if (actionRatings.Count() == 0) { throw new ArgumentException("Invalid arguments! Action ratings set must not be empty!"); }

            // make sure the action ratings has more than one element
            if (actionRatings.Count() == 1) { return actionRatings.OrderByDescending(x => x.Value).First().Key;}
            // exploit: perform the greedy action
            bool isGreedyAction = _random.NextDouble() > _epsilon;
            if(isGreedyAction) {return actionRatings.OrderByDescending(x => x.Value).First().Key;}

            // explore: perform the second best action
            return actionRatings.OrderByDescending(x => x.Value).ElementAt(1).Key;
        }

        public void NextEpisode(IGameSummary summary)
        {
            // nothing to do here ...
        }
    }
}