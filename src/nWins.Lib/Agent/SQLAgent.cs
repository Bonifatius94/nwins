using System.Linq;
using nWins.Lib.Core;
using nWins.Lib.Settings;
using nWins.Lib.Storage;

namespace nWins.Lib.Agent;

/// <summary>
/// An implementation of the IAgent interface
/// </summary>
public class SQLAgent : IAgent
{
    /// <summary>
    /// Create a new sql agent of the given type.
    /// </summary>
    /// <param name="type">The actual type of the agent</param>
    public SQLAgent(AgentType type)
    {
        _type = type;
    }

    private AgentType _type;

    #region IAgent

    public GameAction ChooseAction(IGameState state, GameAction? oppAction)
    {
        var side = oppAction?.ActingSide.Opponent() ?? GameSide.SideA;
        // choose an action by looking into sqLite database, else the first possible action
        QTableSQLite sQLite = new QTableSQLite();
        int column = sQLite.GetBestColumn(_type, state);
        GameAction bestAction = column > -1 ? new GameAction((uint)column, side) : state.GetPossibleActions().First();

        return bestAction;
    }

    public void OnGameOver(IGameSummary summary, GameAction? oppAction = null)
    {
        // nothing to do here ...
    }

    #endregion IAgent
}
