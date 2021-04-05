using nWins.Lib.Core;
using nWins.Lib.Settings;

namespace nWins.Lib.Agent
{
    /// <summary>
    /// An interface representing a fully capable game agent.
    /// </summary>
    public interface IAgent
    {
        /// <summary>
        /// Choose the next action depending on the opponent's last action and the current game state.
        /// </summary>
        /// <param name="state">The new game state after applying the opponent's action.</param>
        /// <param name="oppAction">The opponent's action.</param>
        /// <returns>the next action to be executed</returns>
        GameAction ChooseAction(IGameState state, GameAction? oppAction);

        /// <summary>
        /// Evaluate the outcome of the game that just ended.
        /// </summary>
        /// <param name="summary">The game summary to be evaluated.</param>
        /// <param name="oppAction">The opponent's last action (if the opponent ended the game, default: null).</param>
        void OnGameOver(IGameSummary summary, GameAction? oppAction = null);
    }

    /// <summary>
    /// An interface extension for agents to be serialized from/to data files.
    /// </summary>
    public interface ISesrializableAgent
    {
        /// <summary>
        /// Load a (trained) model from the given data file.
        /// </summary>
        /// <param name="filePath">The path to the data file to be loaded.</param>
        void LoadModel(string filePath);

        /// <summary>
        /// Store the current agent's model to the given output file.
        /// </summary>
        /// <param name="filePath">The path to the data file to store the model to.</param>
        void StoreModel(string filePath);
    }

    /// <summary>
    /// An enumeration representing trainable agent modes (mainly concerning the agent's exploration).
    /// </summary>
    public enum AgentMode
    {
        /// <summary>
        /// Representing training mode including some exploration according to the agent's settings.
        /// </summary>
        Training,

        /// <summary>
        /// Representing inference mode for 100% greedy exploitation.
        /// </summary>
        Inference,

        /// <summary>
        /// Representing semi-greedy inference mode for epsilon greedy exploitation.
        /// </summary>
        SemiGreedy
    }

    /// <summary>
    /// An interface summarizing all agent interfaces to be implemented for a trainable agent.
    /// </summary>
    public interface ITrainableAgent : IAgent, ISesrializableAgent
    {
        /// <summary>
        /// Initialize the trainable agent from settings.
        /// </summary>
        /// <param name="settings">The settings used for initialization.</param>
        void LoadFromSettings(ITrainableAgentSettings settings);

        /// <summary>
        /// Set the agent's mode defining its behavior.
        /// </summary>
        /// <param name="mode">The mode to be used.</param>
        void SetMode(AgentMode mode);
    }
}
