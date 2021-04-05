
using System;
using System.Linq;
using nWins.Lib.Core;

namespace nWins.Lib.Factory
{
    /// <summary>
    /// A factory for initializing game states with the given features.
    /// </summary>
    public static class GameStateFactory
    {
        /// <summary>
        /// Create an empty game board of the given size.
        /// If rows * columns is less than 64, a very efficient implementation is used instead of the standard one.
        /// </summary>
        /// <param name="rows">The amount of rows to be created.</param>
        /// <param name="columns">The amount of columns to be created.</param>
        /// <returns>an empty game board of the given size as IGameState</returns>
        public static IGameState CreateState(int rows, int columns)
        {
            // use optimized implementation for small board sizes fitting into 64-bit integers
            return (rows * columns <= 64) ? (IGameState)new BitwiseGameState(rows, columns) : new SimpleGameState(rows, columns);
        }

        /// <summary>
        /// Create an game board of the given size and field features.
        /// If rows * columns is less than 64, a very efficient implementation is used instead of the standard one.
        /// </summary>
        /// <param name="rows">The amount of rows to be created.</param>
        /// <param name="columns">The amount of columns to be created.</param>
        /// <param name="fields">The field features to be applied to the game state.</param>
        /// <returns>an empty game board of the given size as IGameState</returns>
        public static IGameState CreateState(int rows, int columns, GameSide[] fields)
        {
            // check if rows and columns are greather than 0 and rows * colums equal the fields size
            if (rows <= 0 || columns <= 0 || rows * columns != fields.Length) {
                throw new ArgumentException("Rows and columns have to be greather than 0 and fields size should be equal to rows * columns!"); 
            }

            // calculate the column stone sums using the parsed fields array
            var rowIndices = Enumerable.Range(0, rows);
            var columnStoneSums = Enumerable.Range(0, columns).Select(col => 
                rowIndices.Select(row => fields[row * columns + col] != GameSide.None ? 1 : 0).Sum()
            ).ToArray();

            // use optimized implementation for small board sizes fitting into 64-bit integers
            var simpleState = new SimpleGameState(fields, columnStoneSums, columns);
            return (rows * columns <= 64) ? (IGameState)new BitwiseGameState(simpleState) : simpleState;
        }
    }
}