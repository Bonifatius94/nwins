using System.Collections.Generic;
using System.IO;
using System.Linq;
using nWins.Lib.Agent;
using nWins.Lib.Core;
using nWins.Lib.Session;
using nWins.Lib.Settings;
using nWins.Lib.Storage;
using Xunit;
using Xunit.Abstractions;

namespace nWins.Lib.Test
{
    public class ModelSerializerTest
    {
        // initialize logging tools
        private readonly ITestOutputHelper _logger;
        public ModelSerializerTest(ITestOutputHelper logger) { _logger = logger; }

        [Fact]
        public void SerializationTest()
        {
            // create a new Q table using some gameplay results (the values are raw rewards instead of real Q values)
            var table = new QTable();
            var session = new GameSession(new RandomAgent(), new RandomAgent(), new GameSettings());

            for (int i = 0; i < 10000; i++)
            {
                // play one game until the end
                session.ResetGame();
                var game = session.PlayGameToEnd();

                // write game actions to the Q table (use raw rewards instead of real Q values)
                foreach (var log in game.AllActions)
                {
                    if (table.ContainsKey(log.OldState))
                    {
                        if (!table[log.OldState].ContainsKey(log.Action)) { table[log.OldState].Add(log.Action, log.Reward); }
                    }
                    else { table.Add(log.OldState, new Dictionary<GameAction, double>() { { log.Action, log.Reward } }); }
                }
            }

            // now, test the serializer logic: write the Q table to file
            string filePath = "temp_qtable/qtable.csv";
            QTableCSVSerializer.Serialize(table, filePath);

            // make sure that the output directory was created
            Assert.True(Directory.Exists("temp_qtable"));

            // then, test if the Q table can be parsed properly from file
            var parsedTable = QTableCSVSerializer.Deserialize(filePath);

            // make sure that the original and the serialized Q table are content-equal
            foreach (var state in table.Keys)
            {
                // make sure that the parsed table contains the state key
                Assert.True(parsedTable.ContainsKey(state));

                // compare the (action, Q value) tuples of the state
                foreach (var action in table[state].Keys)
                {
                    // make sure that the parsed table contains the (state, action) tuple
                    Assert.True(parsedTable[state].ContainsKey(action));

                    // make sure that the Q values are (almost) equal
                    double qExpected = table[state][action];
                    double qParsed = parsedTable[state][action];
                    Assert.Equal(qExpected, qParsed, 4);
                }
            }

            // finally, clean up the temporary Q table directory
            Directory.Delete("temp_qtable", true);
        }
    }
}