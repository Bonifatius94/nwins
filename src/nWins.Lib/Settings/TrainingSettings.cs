using Newtonsoft.Json;

namespace nWins.Lib.Settings;

/// <summary>
/// An interface representing essential training session configuration settings (game settings, agents to be trained, amount of training steps, ...)
/// </summary>
public interface ITrainingSettings
{
    /// <summary>
    /// The game settings to be used during training.
    /// </summary>
    IGameSettings GameConfig { get; set; }

    /// <summary>
    /// The trainable agent settings used to train side A's agent.
    /// </summary>
    ITrainableAgentSettings ConfigAgentA { get; set; }

    /// <summary>
    /// The trainable agent settings used to train side B's agent.
    /// </summary>
    ITrainableAgentSettings ConfigAgentB { get; set; }

    /// <summary>
    /// The amount of games played during one training phase.
    /// </summary>
    int TrainingInterval { get; set; }

    /// <summary>
    /// The amount of games played during one inference phase.
    /// </summary>
    int InferenceInterval { get; set; }
}

/// <summary>
/// A JSON-serializable implementation of the ITrainingSettings interface.
/// </summary>
[JsonObject]
public class TrainingSettings : ITrainingSettings
{
    [JsonRequired]
    [JsonProperty("game_settings")]
    [JsonConverter(typeof(JsonGenericConverter<GameSettings>))]
    public IGameSettings GameConfig { get; set; }

    [JsonRequired]
    [JsonProperty("agent_a")]
    [JsonConverter(typeof(JsonGenericConverter<TrainableAgentSettings>))]
    public ITrainableAgentSettings ConfigAgentA { get; set; }

    [JsonRequired]
    [JsonProperty("agent_b")]
    [JsonConverter(typeof(JsonGenericConverter<TrainableAgentSettings>))]
    public ITrainableAgentSettings ConfigAgentB { get; set; }

    [JsonProperty("training_interval")]
    public int TrainingInterval { get; set; } = 1000000;

    [JsonProperty("inference_interval")]
    public int InferenceInterval { get; set; } = 10000;
}
