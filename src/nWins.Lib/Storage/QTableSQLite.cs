using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Data.Sqlite;
using nWins.Lib.Core;
using nWins.Lib.Factory;
using nWins.Lib.Settings;

namespace nWins.Lib.Storage
{
    /// <summary>
    /// A helper class implementing the connection between SQLite and bestActions for each state
    /// </summary>
    public class QTableSQLite
    {
        public QTableSQLite()
        {
            var _dbFilePath = Path.Combine(Environment.CurrentDirectory, "model", "NwinsQtable.db");
            _connection = new SqliteConnection($"Data Source={_dbFilePath}");
        }

        private SqliteConnection _connection;

        /// <summary>
        /// Retrieve the best action for the given state. If the state is not in the cache, -1 is returned.
        /// </summary>
        /// <param name="agentType">The type of the agent</param>
        /// <param name="state">The current state</param>
        /// <returns>The best action (if there is one in the cache else -1)</returns>
        public int GetBestColumn(AgentType agentType, IGameState state)
        {
            if (_connection.State != System.Data.ConnectionState.Open) { _connection.Open(); }

            var bestAction = -1;
            var command = _connection.CreateCommand();
            command.CommandText = $"SELECT Column "
                + $"FROM NwinsQtable_{(int)agentType} "
                + $"WHERE HashBefore = '{GameStateHashFactory.ToBase64Hash(state)}' "
                + $"ORDER BY QValue DESC "
                + $"LIMIT 1";
            
            using (var reader = command.ExecuteReader())
            {
                reader.Read();
                bestAction = reader.GetInt32(0);
            }
            return bestAction;
        }
    }
}