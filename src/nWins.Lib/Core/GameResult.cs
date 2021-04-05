using System;

namespace nWins.Lib.Core
{
    /// <summary>
    /// Representing the outcome of a n-wins game.
    /// </summary>
    public enum GameResult
    {
        /// <summary>
        /// Representing a game won by player A.
        /// </summary>
        WinSideA = 0,

        /// <summary>
        /// Representing a tied game.
        /// </summary>
        Tie = 1,

        /// <summary>
        /// Representing a game won by player B.
        /// </summary>
        WinSideB = 2,
    }

    /// <summary>
    /// An extension class facilitating the work with game results.
    /// </summary>
    public static class GameResultEx
    {
        /// <summary>
        /// Retrieve a printable text representing the given game result.
        /// </summary>
        /// <param name="result">The game result to convert into printable text.</param>
        /// <returns>a printable text as string</returns>
        public static string AsText(this GameResult result)
        {
            return result == GameResult.Tie ? "tie" : $"player { (result == GameResult.WinSideA ? "A" : "B") } wins";
        }
    }
}