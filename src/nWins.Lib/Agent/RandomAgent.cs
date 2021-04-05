using System;
using System.Linq;
using nWins.Lib.Core;
using nWins.Lib.Settings;

namespace nWins.Lib.Agent
{
    /// <summary>
    /// An agent playing randomly (used for training sessions).
    /// </summary>
    public class RandomAgent : ITrainableAgent
    {
        private static readonly Random random = new Random();

        public void LoadFromSettings(ITrainableAgentSettings settings)
        {
            // nothing to do here ...
        }

        public void SetMode(AgentMode mode)
        {
            // nothing to do here ...
        }

        public GameAction ChooseAction(IGameState state, GameAction? oppAction)
        {
            var actions = state.GetPossibleActions();
            return actions.ElementAt(random.Next(actions.Count()));
        }

        public void OnGameOver(IGameSummary summary, GameAction? oppAction = null)
        {
            // nothing to do here ...
        }

        public void LoadModel(string filePath)
        {
            // nothing to do here ...
        }

        public void StoreModel(string filePath)
        {
            // nothing to do here ...
        }
    }
}