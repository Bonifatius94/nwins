using System;
using System.Collections.Generic;

namespace nWins.Lib.Core
{
    /// <summary>
    /// An interface representing a n-wins game state with all attributes and functions required.
    /// </summary>
    public interface IGameState : IEquatable<IGameState>, IEquatable<string>
    {
        /// <summary>
        /// The board's fields at the current game state.
        /// </summary>
        GameSide[] Fields { get; }

        /// <summary>
        /// The amount of stones per column.
        /// </summary>
        int[] ColumnStoneSums { get; }

        /// <summary>
        /// The board's column size.
        /// </summary>
        int MaxColumns { get; }

        /// <summary>
        /// The board's row size.
        /// </summary>
        int MaxRows { get; }

        /// <summary>
        /// Retrieve an immutable instance of the new game state after applying the given action.
        /// </summary>
        /// <param name="action">The action to be applied.</param>
        /// <returns>an immutable instance of the new game state</returns>
        IGameState ApplyAction(GameAction action);

        /// <summary>
        /// Retrieve a set of possible actions for the acting side given the current game state.
        /// </summary>
        /// <returns>a set of possible actions</returns>
        IEnumerable<GameAction> GetPossibleActions();

        /// <summary>
        /// Determine whether the win condition is met (-> current state is a terminal state).
        /// </summary>
        /// <param name="n">The connections to be achieved to win.</param>
        /// <returns>boolean whether the win condition is met</returns>
        bool IsConnectN(int n);
    }
}