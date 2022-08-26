using System;

namespace nWins.Lib.Core;

/// <summary>
/// Representing the state of a single field of a n-wins board.
/// </summary>
public enum GameSide
{
    /// <summary>
    /// Representing none of the two sides.
    /// </summary>
    None = 0,

    /// <summary>
    /// Representing the side of player A.
    /// </summary>
    SideA = 1,

    /// <summary>
    /// Representing the side of player B.
    /// </summary>
    SideB = 2,
}

public static class FieldStateEx
{
    /// <summary>
    /// Determine the opponent of the given side.
    /// </summary>
    /// <param name="side">The side to be evaluated.</param>
    /// <returns>the opponent of the given side</returns>
    public static GameSide Opponent(this GameSide side)
    {
        if (side == GameSide.None) { throw new ArgumentException("Invalid field state! State must not be unoccupied!"); }
        return side == GameSide.SideA ? GameSide.SideB : GameSide.SideA;
    }
}
