namespace nWins.Lib.Core;

/// <summary>
/// Representing an action log entry with all relevant information of applying the action at a given state.
/// </summary>
public struct ActionLog
{
    /// <summary>
    /// Create a new action log instance with the given parameters.
    /// </summary>
    /// <param name="oldState">The old state before executing the action.</param>
    /// <param name="newState">The new state after executing the action.</param>
    /// <param name="action">The action that was executed.</param>
    /// <param name="reward">The reward of the action.</param>
    /// <param name="isTerminal">Indicates if the action lead into a terminal state.</param>
    public ActionLog(IGameState oldState, IGameState newState, GameAction action, double reward, bool isTerminal)
    {
        OldState = oldState;
        NewState = newState;
        Action = action;
        Reward = reward;
        IsTerminal = isTerminal;
    }

    /// <summary>
    /// The state before applying the action.
    /// </summary>
    public IGameState OldState;

    /// <summary>
    /// The state after applying the action.
    /// </summary>
    public IGameState NewState;

    /// <summary>
    /// The action that was applied.
    /// </summary>
    public GameAction Action;

    /// <summary>
    /// The action's reward.
    /// </summary>
    public double Reward;

    /// <summary>
    /// Indicates whether the action lead into a terminal state.
    /// </summary>
    public bool IsTerminal;

    #region Override

    public override string ToString()
        => $"action: { Action }, old state: { OldState }, new state: { NewState }, "
            + $"reward: { Reward }, is terminal: { IsTerminal }";

    #endregion Override
}
