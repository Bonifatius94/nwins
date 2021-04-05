using System;
using System.Collections.Generic;
using nWins.Lib.Agent;
using nWins.Lib.Core;
using nWins.Lib.Factory;
using nWins.Lib.Settings;

namespace nWins.Lib.Session
{
    /// <summary>
    /// An implementation of a game session designed for human vs. AI gameplay.
    /// </summary>
    public class OnlineGameSession
    {
        /// <summary>
        /// Create a new game session with the given settings.
        /// </summary>
        /// <param name="settings">The settings used to initialize the game session with.</param>
        public OnlineGameSession(IGameSessionSettings settings)
        {
            // load from settings
            _settings = settings;
            LoadFromSettings(settings);
        }

        private IGameSessionSettings _settings;

        /// <summary>
        /// The human player's game side.
        /// </summary>
        public GameSide PlayerSide { get; set; }

        /// <summary>
        /// The game callback representing the current game state.
        /// </summary>
        public IGameEngine Game { get; set; }

        /// <summary>
        /// The AI agent competing with the human player.
        /// </summary>
        public IAgent AIAgent { get; set; }

        /// <summary>
        /// A boolean flag indicating whether the current game state is terminal (-> game over).
        /// </summary>
        public bool IsGameOver => Game.CurrentState.IsConnectN(_settings.GameConfig.StonesToConnect) 
            || _settings.GameConfig.Rows * _settings.GameConfig.Columns == Game.ActionHistory.Count;

        #region Cache

        private static readonly Random _random = new Random();

        #endregion Cache

        public void LoadFromSettings(IGameSessionSettings settings)
        {
            // create a new game in starting formation
            Game = new GameEngine(settings.GameConfig);

            // determine the player's side
            PlayerSide = settings.PreferredSide == GameSide.None 
                ? (GameSide)(_random.Next(2) + 1) : settings.PreferredSide;

            // load the opponent agent
            AIAgent = loadAgent(settings.AIOpponent);

            // apply the first AI draw right away as the AI player starts
            if (PlayerSide == GameSide.SideB) { applyAIOpponentAction(); }
        }

        private IAgent loadAgent(AgentType type)
            => (type == AgentType.Random) ?  (IAgent)new RandomAgent() : new SQLAgent(type);

        public ActionLog ApplyPlayerAction(GameAction action)
        {
            // apply the player's action
            var playerLog = Game.ApplyAction(action);

            // exit if the game is over after the player's action (either win or tie)
            if (IsGameOver) { return playerLog; }

            // make an action chosen by the AI
            var oppLog = applyAIOpponentAction();

            // update the IsTerminal attribute
            playerLog.IsTerminal = playerLog.IsTerminal 
                || Game.CurrentState.IsConnectN(_settings.GameConfig.StonesToConnect);

            return playerLog;
        }

        private ActionLog applyAIOpponentAction()
        {
            // apply the AI opponent's action
            var oppAction = AIAgent.ChooseAction(Game.CurrentState, Game.LastAction);
            var log = Game.ApplyAction(oppAction);
            return log;
        }
    }
}
