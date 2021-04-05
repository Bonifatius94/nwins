
using Newtonsoft.Json;
using nWins.Lib.Core;

namespace nWins.Lib.Settings
{
    /// <summary>
    /// An interface providing settings for human vs. AI session settings.
    /// </summary>
    public interface IGameSessionSettings
    {
        /// <summary>
        /// The game settings used (rows, columns, win condition).
        /// </summary>
        IGameSettings GameConfig { get; set; }

        /// <summary>
        /// The trained AI agent to be played against.
        /// </summary>
        AgentType AIOpponent { get; set; }

        /// <summary>
        /// The side preferred by the human player.
        /// </summary>
        GameSide PreferredSide { get; set; }
    }

    /// <summary>
    /// A JSON-serializable implementation of the IGameSessionSettings interface.
    /// </summary>
    [JsonObject]
    public class GameSessionSettings : IGameSessionSettings
    {
        [JsonRequired]
        [JsonProperty("game_settings")]
        public IGameSettings GameConfig { get; set; }

        [JsonRequired]
        [JsonProperty("opp_type")]
        public AgentType AIOpponent { get; set; }

        [JsonProperty("pref_side")]
        public GameSide PreferredSide { get; set; } = GameSide.None;
    }
}