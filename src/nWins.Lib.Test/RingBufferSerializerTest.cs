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
    public class RingBufferSerializerTest
    {
        // initialize logging tools
        private readonly ITestOutputHelper _logger;
        public RingBufferSerializerTest(ITestOutputHelper logger) { _logger = logger; }

        [Fact]
        public void SerializationTest()
        {
            // create a new RingBuffer using some gameplay action
            int ringBufferSize = 2000;
            var experienceMemory = new RingBuffer<ActionLog>(ringBufferSize);
            var session = new GameSession(new RandomAgent(), new RandomAgent(), new GameSettings());

            for (int i = 0; i < 10000; i++)
            {
                // play one game until the end
                session.ResetGame();
                var game = session.PlayGameToEnd();

                // write game actions to the Q table (use raw rewards instead of real Q values)
                foreach (var log in game.AllActions)
                {
                    experienceMemory.AddItem(log);
                }
            }

            // now, test the serializer logic: write the RingBuffer to file
            string filePath = "temp_ring_buffer/buffer.csv";
            RingBufferSerializer.Serialize(filePath, experienceMemory);

            // make sure that the output directory was created
            Assert.True(Directory.Exists("temp_ring_buffer"));

            // then, test if the RingBuffer can be parsed properly from file
            var parsedExperienceMemory = RingBufferSerializer.Deserialize(filePath, ringBufferSize);

            // make sure that the original and the serialized RingBuffer have same size
            Assert.True(experienceMemory.Count == parsedExperienceMemory.Count);

            // make sure that the original and the serialized RingBuffer are content-equal
            foreach (ActionLog log in experienceMemory)
            {
                // make sure that the parsed RingBuffer contains the log of the experienceMemory
                Assert.True(parsedExperienceMemory.Contains(log));
            }

            // finally, clean up the temporary RingBuffer directory
            Directory.Delete("temp_ring_buffer", true);
        }
    }
}