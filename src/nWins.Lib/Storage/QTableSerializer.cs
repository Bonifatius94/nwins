using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using nWins.Lib.Core;
using nWins.Lib.Factory;

namespace nWins.Lib.Storage
{
    /// <summary>
    /// Representing a Q table data structure for efficient key lookup as hash map.
    /// Q values for a specific (state, action) tuple can be looked up in O(1).
    /// </summary>
    public class QTable : Dictionary<IGameState, IDictionary<GameAction, double>> { }

    /// <summary>
    /// A helper class implementing the serialization between Q tables and a CSV file format.
    /// <para>
    /// Each CSV line consists of 4 attributes:
    /// <list type="bullet">
    /// <item>the state as Base64 hash string</item>
    /// <item>the acting side as enum int</item>
    /// <item>the column index of the action as int</item>
    /// <item>the Q value as double</item>
    /// </list>
    /// </para>
    /// </summary>
    public static class QTableCSVSerializer
    {
        /// <summary>
        /// Deserialize a Q table from the given file path.
        /// </summary>
        /// <param name="filePath">The path to the file to be parsed.</param>
        /// <returns>a new Q table instance</returns>
        public static QTable Deserialize(string filePath)
        {
            // create empty Q table
            var table = new QTable();

            // create textual file reader
            using (var reader = new StreamReader(filePath))
            {
                string line;

                // loop through every line
                while ((line = reader.ReadLine()) != null)
                {
                    // get comma separated line parts as string
                    var parts = line.Split(new char[] { ',' }, 5, StringSplitOptions.RemoveEmptyEntries);

                    // parse raw line attributes
                    string stateHash = parts[0];
                    var actingSide = (GameSide)int.Parse(parts[2]);
                    uint columnIndex = (uint)int.Parse(parts[3]);
                    double qValue = double.Parse(parts[4]);

                    // create high-level data types from attributes
                    var state = GameStateHashFactory.FromBase64Hash(stateHash);
                    var action = new GameAction(columnIndex, actingSide);

                    // apply data to table entry (create state keys if they don't exist)
                    if (table.ContainsKey(state))
                    {
                        // append an (action, Q value) tuple to an existing table entry
                        table[state].Add(action, qValue);
                    }
                    else
                    {
                        // create new table entry with first (action, Q value) tuple
                        var entry = new Dictionary<GameAction, double>() { { action, qValue } };
                        table.Add(state, entry);
                    }
                }
            }

            return table;
        }

        /// <summary>
        /// Write the given Q table to the specified file path.
        /// </summary>
        /// <param name="table">The Q table to be written.</param>
        /// <param name="filePath">The output file path to be written to.</param>
        public static void Serialize(QTable table, string filePath)
        {
            // make sure that the directory exists
            string dir = Path.GetDirectoryName(filePath);
            if (!Directory.Exists(dir)) { Directory.CreateDirectory(dir); }

            // create textual file writer (this overwrites an existing file with the same name)
            using (var writer = new StreamWriter(filePath))
            {
                // loop through all states
                foreach (var state in table.Keys)
                {
                    // determine the Base64 hash of the state
                    string stateHash = GameStateHashFactory.ToBase64Hash(state);

                    // loop through the (action, Q value) tuples of the state
                    foreach (var action in table[state].Keys)
                    {
                        // write each (state, action, Q value) tuple as CSV line
                        double qValue = table[state][action];
                        string newStateHash = GameStateHashFactory.ToBase64Hash(state.ApplyAction(action));
                        string line = $"{ stateHash },{ newStateHash },{ (int)action.ActingSide },{ action.ColumnIndex },{ qValue }";
                        writer.WriteLine(line);
                    }
                }
            }
        }
    }
}