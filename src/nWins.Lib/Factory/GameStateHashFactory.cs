using System;
using nWins.Lib.Core;

namespace nWins.Lib.Factory;

/// <summary>
/// A factory for conversions between game states and corresponding Base64 hash strings.
/// </summary>
public static class GameStateHashFactory
{
    /// <summary>
    /// Export the given game state as Base64 hash string.
    /// </summary>
    /// <param name="state">The game state to be converted to a Base64 hash.</param>
    /// <returns>a Base64 hash string representing the game state</returns>
    public static string ToBase64Hash(IGameState state)
    {
        // initialize an empty bytes array of the required size
        byte[] hashBytes = new byte[(int)Math.Ceiling((double)state.Fields.Length / 4) + 2];

        // store max rows / columns as the leading two hash bytes
        hashBytes[0] = (byte)state.MaxRows;
        hashBytes[1] = (byte)state.MaxColumns;

        // initialize offset and cache
        int offset = 2;
        byte cache = 0;

        // loop through all fields of the state
        for (int i = 0; i < (hashBytes.Length - 2) * 4; i++)
        {
            // update the cache (apply each state as 2 bits of byte)
            // fill last byte with zeros for board sizes not divisible by 4
            var field = i < state.Fields.Length ? (int)state.Fields[i] : 0;
            cache = (byte)((cache << 2) | field);

            // after every fourth field: apply the cached byte to the hash bytes array
            if (i % 4 == 3) { hashBytes[offset++] = cache; cache = 0; }
        }

        // convert the hash bytes into a Base64 string
        return Convert.ToBase64String(hashBytes);
    }

    /// <summary>
    /// Convert the given Base64 hash string to the corresponding game state.
    /// </summary>
    /// <param name="hash">The Base64 hash string to be converted.</param>
    /// <returns>a new game state instance implementing IGameState</returns>
    public static IGameState FromBase64Hash(string hash)
    {
        // get hash bytes from Base64 string
        var hashBytes = Convert.FromBase64String(hash);

        // extract max rows / columns from leading two bytes
        int maxRows = hashBytes[0];
        int maxColumns = hashBytes[1];

        // create an empty fields array of the required size
        var fields = new GameSide[maxRows * maxColumns];
        int offset = 0;

        // loop through the hash bytes and extract the fields from it
        for (int i = 2; i < hashBytes.Length; i++)
        {
            var hashByte = hashBytes[i];

            // loop through each 2-bit sequence representing a field
            for (int j = 0; j < 4; j++)
            {
                // cut the field from hash byte and apply it to the fields array
                var field = (GameSide)((hashByte >> ((3 - j) * 2)) & 0x3);
                if (offset < fields.Length) { fields[offset++] = field; }
            }
        }

        // create a new game state with the parsed fields, column stone sums and column size
        return GameStateFactory.CreateState(maxRows, maxColumns, fields);
    }
}
