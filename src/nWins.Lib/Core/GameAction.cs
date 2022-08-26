using System;

namespace nWins.Lib.Core;

/// <summary>
/// Representing a n-wins game action.
/// </summary>
public readonly struct GameAction : IEquatable<GameAction>
{
    /// <summary>
    /// Create a new game action instance with the given parameters.
    /// </summary>
    /// <param name="columnIndex">The column to insert the stone into.</param>
    /// <param name="actingSide">The side executing the action.</param>
    public GameAction(uint columnIndex, GameSide actingSide)
    {
        ColumnIndex = columnIndex;
        ActingSide = actingSide;
    }

    /// <summary>
    /// The index of the column where to put the player's stone in.
    /// </summary>
    public readonly uint ColumnIndex;

    /// <summary>
    /// The side indicating which player put the stone.
    /// </summary>
    public readonly GameSide ActingSide;

    #region Helpers

    public bool Equals(GameAction other)
    {
        // check of equality of the two numeric attributes (slower fallback for failing hash comparison)
        return this.ColumnIndex == other.ColumnIndex && this.ActingSide == other.ActingSide;
    }

    public override int GetHashCode()
    {
        // define a 'unique hash' to compare actions by this hash
        // this only fails if there's an overflow error
        // to cause such an error, the col index needs to be greater than ~ 2^31 / 3
        return (int)(ColumnIndex * 3 + (uint)ActingSide);
    }

    public override string ToString()
    {
        return $"{ ActingSide } put a stone at { ColumnIndex }";
    }

    #endregion Helpers
}
