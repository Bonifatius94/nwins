
using System;
using System.Collections.Generic;
using nWins.Lib.Agent;
using nWins.Lib.Core;
using nWins.Lib.Settings;

namespace nWins.Lib.Session
{
    /// <summary>
    /// An interface representing a game session for AI vs. AI gameplay / training.
    /// </summary>
    public interface IGameSession
    {
        /// <summary>
        /// The game settings (readonly).
        /// </summary>
        IGameSettings Settings { get; }

        /// <summary>
        /// The game instance (readonly).
        /// </summary>
        IGameEngine Game { get; }

        /// <summary>
        /// The agent A (readonly).
        /// </summary>
        IAgent AgentA { get; }

        /// <summary>
        /// The agent B (readonly).
        /// </summary>
        IAgent AgentB { get; }

        /// <summary>
        /// Register the two agents playing against each other.
        /// </summary>
        /// <param name="agent1">The first agent to be registered.</param>
        /// <param name="agent2">The second agent to be registered.</param>
        void RegisterAgents(IAgent agent1, IAgent agent2);

        /// <summary>
        /// Make the agents play one game against each other and retrieve the game's outcome.
        /// </summary>
        /// <returns>the game's outcome as summary</returns>
        IGameSummary PlayGameToEnd();

        /// <summary>
        /// Reset the existing game instance and prepare the next game.
        /// </summary>
        void ResetGame();
    }

    /// <summary>
    /// Representing a training game session optimized for synchronous, sequential background gameplay.
    /// </summary>
    public class GameSession : IGameSession
    {
        /// <summary>
        /// Create a new game session with the given agents and game settings.
        /// </summary>
        /// <param name="agentA">The agent drawing first.</param>
        /// <param name="agentB">The agent drawing second.</param>
        /// <param name="settings">The game settings to be applied.(Default: 5 x 4 Board, 4 Connections)</param>
        public GameSession(IAgent agentA, IAgent agentB, IGameSettings settings = null)
        {
            Settings = settings ?? new GameSettings();
            RegisterAgents(agentA, agentB);
            ResetGame();
        }

        public IAgent AgentA { get; private set;}
        public IAgent AgentB { get; private set;}

        public IGameSettings Settings { get; private set;}
        public IGameEngine Game { get; private set; }

        public void RegisterAgents(IAgent agentA, IAgent agentB)
        {
            AgentA = agentA;
            AgentB = agentB;
        }

        public IGameSummary PlayGameToEnd()
        {
            int i = 0;
            ActionLog log;
            bool isActionIntoTie = false;

            do
            {
                // select player to act alternatingly
                var drawingPlayer = i++ % 2 == 0 ? AgentA : AgentB;

                // retrieve the next action from the agent and apply it
                var action = drawingPlayer.ChooseAction(Game.CurrentState, Game.LastAction);
                log = Game.ApplyAction(action);

                // handle edge case where the second last state of a tie game is also marked as terminal
                isActionIntoTie = i == Settings.Rows * Settings.Columns - 1 && !log.NewState.IsConnectN(Settings.StonesToConnect);
            }
            // exit as soon as a terminal state is reached
            while (!log.IsTerminal || isActionIntoTie);

            // notify both agents about the game's outcome
            var result = Game.Result;
            var winner = Game.LastAction.Value.ActingSide;
            AgentA.OnGameOver(Game, winner == GameSide.SideA ? null : Game.LastAction);
            AgentB.OnGameOver(Game, winner == GameSide.SideB ? null : Game.LastAction);

            return Game;
        }

        public void ResetGame()
        {
            // create a new n-wins game instance
            Game = new GameEngine(Settings);
        }
    }
}