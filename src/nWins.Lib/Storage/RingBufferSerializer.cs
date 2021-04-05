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
    /// A helper class implementing the serialization between RingBuffer and a CSV file format.
    /// <para>
    /// Each CSV line consists of 5 attributes:
    /// <list type="bullet">
    /// <item>the old state as Base64 hash string</item>
    /// <item>the old state as Base64 hash string</item>
    /// <item>the column index and acting side of the action as int</item>
    /// <item>the reward value as double</item>
    /// <item>the isTerminal value as boolean</item>
    /// </list>
    /// </para>
    /// </summary>
    public static class RingBufferSerializer
    {
        /// <summary>
        /// Deserialize a RingBuffer from the given file path.
        /// </summary>
        /// <param name="filePath">The path to the file to be parsed.</param>
        /// <returns>a new RingBuffer instance</returns>
        public static RingBuffer<ActionLog> Deserialize(string filePath, int dynaQMemorySize)
        {
            // create empty RingBuffer
            var experienceMemory = new RingBuffer<ActionLog>(dynaQMemorySize);

            // create textual file reader
            using (var reader = new StreamReader(filePath))
            {
                string line;

                // loop through every line
                while ((line = reader.ReadLine()) != null)
                {
                    // get comma separated line parts as string
                    var parts = line.Split(new char[] {','}, 6, StringSplitOptions.RemoveEmptyEntries);

                    // parse raw line attributes
                    string oldStateHash = parts[0];
                    string newStateHash = parts[1];
                    var actingSide = (GameSide)int.Parse(parts[2]);
                    uint columnIndex = (uint)int.Parse(parts[3]);
                    double reward = double.Parse(parts[4]);
                    bool isTerminal = bool.Parse(parts[5]);

                    // create high-level data types from attributes
                    var oldState = GameStateHashFactory.FromBase64Hash(oldStateHash);
                    var newState = GameStateHashFactory.FromBase64Hash(newStateHash);
                    var action = new GameAction(columnIndex, actingSide);

                    // create new ring buffer entry with (oldState, newState, action, reward, isTerminal) tuple
                    var entry = new ActionLog(oldState, newState, action, reward, isTerminal);
                    experienceMemory.AddItem(entry);
                }
            }

            return experienceMemory;
        }

        /// <summary>
        /// Write the given RingBuffer to the specified file path.
        /// </summary>
        /// <param name="table">The RingBuffer to be written.</param>
        /// <param name="filePath">The output file path to be written to.</param>
        public static void Serialize(string filePath, RingBuffer<ActionLog> experienceMemory)
        {
            // make sure that the directory exists
            string dir = Path.GetDirectoryName(filePath);
            if (!Directory.Exists(dir)) { Directory.CreateDirectory(dir); }

            // create textual file writer (this overwrites an existing file with the same name)
            using (var writer = new StreamWriter(filePath))
            {
                // loop through all logs
                foreach (ActionLog log in experienceMemory)
                {
                    // determine the Base64 hash of the old state
                    string oldStateHash = GameStateHashFactory.ToBase64Hash(log.OldState);

                     // determine the Base64 hash of the new state
                    string newStateHash = GameStateHashFactory.ToBase64Hash(log.NewState);

                    // write each (oldState, newState, action, reward, isTerminal) tuple as CSV line
                    string line = $"{ oldStateHash },{ newStateHash },{ (int)log.Action.ActingSide },{ log.Action.ColumnIndex },{ log.Reward },{ log.IsTerminal }";
                    writer.WriteLine(line);
                }
            }
        }
    }
}