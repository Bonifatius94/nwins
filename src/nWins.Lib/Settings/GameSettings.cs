using Newtonsoft.Json;

namespace nWins.Lib.Settings;

/// <summary>
/// An interface representing essential n-wins game settings (amount of rows / columns, stones to connect).
/// </summary>
public interface IGameSettings
{
    /// <summary>
    /// The amount of columns on the n-wins game board.
    /// </summary>
    int Columns { get; set; }

    /// <summary>
    /// The amount of rows on the n-wins game board.
    /// </summary>
    int Rows { get; set; }

    /// <summary>
    /// The amount of stones to connect in order to win the game.
    /// </summary>
    int StonesToConnect { get; set; }
}

/// <summary>
/// A JSON-serializable implementation of the IGameSettings interface.
/// </summary>
[JsonObject]
public class GameSettings : IGameSettings
{
    [JsonProperty("columns")]
    public int Columns { get; set; } = 5;

    [JsonProperty("rows")]
    public int Rows { get; set; } = 4;

    [JsonProperty("win_conn")]
    public int StonesToConnect { get; set; } = 4;
}
