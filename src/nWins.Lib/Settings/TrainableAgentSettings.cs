using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

namespace nWins.Lib.Settings
{
    /// <summary>
    /// An enum representing different types of n-wins agents.
    /// </summary>
    public enum AgentType
    {
        /// <summary>
        /// Representing a randomly acting agent.
        /// </summary>
        Random = 0,

        /// <summary>
        /// Representing an agent being trained with the naive Q learning algorithm.
        /// </summary>
        SimpleQL = 1,

        /// <summary>
        /// Representing an agent being trained with the double Q learning algorithm.
        /// </summary>
        DoubleQL = 2,

        /// <summary>
        /// Representing an agent being trained with the dynamic Q learning algorithm.
        /// </summary>
        DynaQL = 3,

        /// <summary>
        /// Representing an agent being trained with the SARSA-Lambda algorithm making use of eligibility traces.
        /// </summary>
        SarsaLambda = 4
    }

    /// <summary>
    /// An interface representing essential settings of a trainable agent (algorithm, trainability, model-to-load, training hyperparams).
    /// </summary>
    public interface ITrainableAgentSettings
    {
        /// <summary>
        /// The agent's name used for model identification.
        /// </summary>
        string AgentName { get; set; }

        /// <summary>
        /// The agent's type specifying the related learning algorithm.
        /// </summary>
        AgentType AgentType { get; set; }

        /// <summary>
        /// Indicates whether the agent may modify the loaded model during training.
        /// </summary>
        bool IsTrainable { get; set; }

        /// <summary>
        /// An explicit (trained) model file to be loaded 
        /// (if not specified: load model with highest training episode in file name).
        /// </summary>
        string ExplicitModelToLoad { get; set; }

        /// <summary>
        /// The model file inside the models root directory with the highest episode in file name.
        /// </summary>
        string LatestModelPath { get; }

        /// <summary>
        /// The concrete model to be loaded on startup (default behavior: uses ExplicitModelToLoad and defaults to LatestModelPath).
        /// </summary>
        string StartupModel { get; }

        /// <summary>
        /// The hyperparameters dictionary used for agent-specific settings.
        /// </summary>
        Dictionary<string, object> Params { get; set; }

        /// <summary>
        /// The epsilon value for the semi-greedy inference mode.
        /// </summary>
        double epsilon { get; set; }

        /// <summary>
        /// Get the model file path for a model tagged by the given episode.
        /// The path includes the model directories. The generated file name should look like 
        /// {agent name}_{episode}{ file ext }
        /// </summary>
        /// <param name="episode">The episode to tag the model with.</param>
        /// <param name="fileExt">The model's file extension to be appended.</param>
        /// <returns>a model file path as string</returns>
        string GetModelPath(int episode, string fileExt = ".csv");
    }

    /// <summary>
    /// A JSON-serializable implementation of the ITrainableAgentSettings interface.
    /// </summary>
    [JsonObject]
    public class TrainableAgentSettings : ITrainableAgentSettings
    {
        [JsonRequired]
        [JsonProperty("name")]
        public string AgentName { get; set; }

        [JsonRequired]
        [JsonProperty("type")]
        public AgentType AgentType { get; set; }

        [JsonRequired]
        [JsonProperty("trainable")]
        public bool IsTrainable { get; set; }

        // default behavior if not specified: load latest model
        [JsonProperty("load_explicit_model")]
        public string ExplicitModelToLoad { get; set; } = null;

        [JsonProperty("params")]
        public Dictionary<string, object> Params { get; set; } = new Dictionary<string, object>();

        [JsonProperty("semi_greedy_epsilon")]
        public double epsilon { get; set; }

        [JsonIgnore]
        public string LatestModelPath => getLatestModelFilePath();

        [JsonIgnore]
        public string StartupModel => ExplicitModelToLoad ?? LatestModelPath;

        private string getLatestModelFilePath()
        {
            string modelDir = Path.Combine(EnvironmentSettings.Instance.ModelRootDir, AgentName);
            if (!Directory.Exists(modelDir)) { return null; }

            var modelFiles = Directory.GetFiles(modelDir, $"*{ AgentName }*");

            int temp;
            var filesWithIds = modelFiles.Select(x => Path.GetFileName(x))
                .Select(x => new Tuple<string, int>(x, Math.Abs(int.TryParse(x, out temp) ? temp : temp)));

            string latestModelFile = filesWithIds.OrderByDescending(x => x.Item2).Select(x => x.Item1).FirstOrDefault();
            return Path.Combine(modelDir, latestModelFile);
        }

        public string GetModelPath(int episode, string fileExt = ".csv")
        {
            return Path.Combine(EnvironmentSettings.Instance.ModelRootDir, AgentName, $"{ AgentName }_{ episode }{ fileExt }");
        }
    }
}