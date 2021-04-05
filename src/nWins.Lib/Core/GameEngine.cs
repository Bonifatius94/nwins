using System.Collections.Generic;
using System.Linq;
using nWins.Lib.Factory;
using nWins.Lib.Settings;

namespace nWins.Lib.Core
{
    /// <summary>
    /// A combined interface to facilitate gameplay as well as game evaluation during post-processing.
    /// </summary>
    public interface IGameEngine : IGameCallback, IGameSummary {}

    /// <summary>
    /// An interface implementing basic n-wins attributes and functions used for gameplay.
    /// </summary>
    public interface IGameCallback
    {
        /// <summary>
        /// The game's draw history.
        /// </summary>
        Stack<ActionLog> ActionHistory { get; }

        /// <summary>
        /// The game's last action.
        /// </summary>
        GameAction? LastAction { get; }

        /// <summary>
        /// The game's current state.
        /// </summary>
        IGameState CurrentState { get; }

        /// <summary>
        /// The side to be acting next.
        /// </summary>
        GameSide ActingSide { get; }

        /// <summary>
        /// All possible actions for the current game state.
        /// </summary>
        /// <value></value>
        IEnumerable<GameAction> PossibleActions { get; }

        /// <summary>
        /// Apply an action to the game engine and retrieve the draw's outcome as 
        /// </summary>
        /// <param name="action">The action to be added.</param>
        /// <returns>an action log with all relevant action meta-information</returns>
        ActionLog ApplyAction(GameAction action);
    }

    /// <summary>
    /// An interface facilitates the evaluation of games during post-processing. 
    /// It intentionally hides gameplay functionality because the game is already over.
    /// </summary>
    public interface IGameSummary
    {
        /// <summary>
        /// Get all action logs of both players (alternatingly) as list.
        /// </summary>
        IEnumerable<ActionLog> AllActions { get; }

        /// <summary>
        /// Get all action logs concerning actions of side A as list.
        /// </summary>
        IEnumerable<ActionLog> ActionsSideA { get; }

        /// <summary>
        /// Get all action logs concerning actions of side B as list.
        /// </summary>
        IEnumerable<ActionLog> ActionsSideB { get; }

        /// <summary>
        /// In case the game is in a terminal state, retrieve the game's outcome.
        /// </summary>
        GameResult Result { get; }
    }

    /// <summary>
    /// A full-featured implementation of the IGameCallback interface that can be initialized with generic game settings.
    /// This class represents a single n-wins game including the action history, outcome, granted rewards, etc.
    /// </summary>
    public class GameEngine : IGameEngine
    {
        /// <summary>
        /// Create a new game engine instance with the given parameters (defaulting to standard parameters if not specified).
        /// </summary>
        /// <param name="settings">The parameters of the new game engine (defaulting to standard parameters if not specified).</param>
        public GameEngine(IGameSettings settings = null)
        {
            _settings = settings ?? new GameSettings();
        }

        private IGameSettings _settings;

        #region IGameSummary

        public GameResult Result => CurrentState.IsConnectN(_settings.StonesToConnect) 
            ? (LastAction.Value.ActingSide == GameSide.SideA ? GameResult.WinSideA : GameResult.WinSideB) : GameResult.Tie;

        public IEnumerable<ActionLog> AllActions => ActionHistory.Reverse().ToList();

        public IEnumerable<ActionLog> ActionsSideA => ActionHistory.Reverse().Where(x => x.Action.ActingSide == GameSide.SideA).ToList();

        public IEnumerable<ActionLog> ActionsSideB => ActionHistory.Reverse().Where(x => x.Action.ActingSide == GameSide.SideB).ToList();

        #endregion IGameSummary

        #region IGameCallback

        public Stack<ActionLog> ActionHistory { get; } = new Stack<ActionLog>();

        public GameAction? LastAction => ActionHistory.Count > 0 ? (GameAction?)ActionHistory.Peek().Action : null;

        public IGameState CurrentState => ActionHistory.Count > 0 ? ActionHistory.Peek().NewState : GameStateFactory.CreateState(_settings.Rows, _settings.Columns);

        public GameSide ActingSide => ActionHistory.Count > 0 ? LastAction.Value.ActingSide.Opponent() : GameSide.SideA;

        public IEnumerable<GameAction> PossibleActions => CurrentState.GetPossibleActions();

        public ActionLog ApplyAction(GameAction action)
        {
            // apply the action and determine the new state
            var oldState = CurrentState;
            var newState = oldState.ApplyAction(action);

            // check if the action is resulting into a win or tie
            bool isWin = newState.IsConnectN(_settings.StonesToConnect);
            bool isTie = ActionHistory.Count == _settings.Rows * _settings.Columns - 1 && !isWin;

            // investigate tie case on second last action (required to reward both players for a tie)
            if (ActionHistory.Count == _settings.Rows * _settings.Columns - 2)
            {
                // check if the opponent can win with his very last action (if not it's a tie)
                var finalAction = newState.GetPossibleActions().First();
                var finalState = newState.ApplyAction(finalAction);
                isTie = !finalState.IsConnectN(_settings.StonesToConnect);
            }

            // determine the reward (win=1, tie=0.5, loss=0.0)
            // in case of a tie, both players' last actions get the 0.5 reward
            double reward = isTie ? 0.5 : (isWin ? 1 : 0);
            bool isTerminal = isWin || isTie;

            // log the action's outcome to the history
            var log = new ActionLog(oldState, newState, action, reward, isTerminal);
            ActionHistory.Push(log);
            return log;
        }

        #endregion IGameCallback
    }
}