using System.IO;
using Newtonsoft.Json;
using nWins.Lib.Settings;
using Xunit;
using Xunit.Abstractions;
using nWins.Lib.Session;
using nWins.Lib.Agent;

namespace nWins.Lib.Test
{
    public class TrainingSessionTest
    {
        // initialize logging tools
        private readonly ITestOutputHelper _logger;
        public TrainingSessionTest(ITestOutputHelper logger) { _logger = logger; }

        [Fact]
        public void ConstructorTest()
        {
            // Tests also LoadFromSettings
            // Create test settings from json
            string json;
            string settingsFilePath = Path.Combine("test_assets", "training_settings.json");
            using (var reader = new StreamReader(settingsFilePath)) { json = reader.ReadToEnd();}
            var settings = JsonConvert.DeserializeObject<TrainingSettings>(json);

            var session = new TrainingSession(settings);

            // Check if settings were set correctly
            Assert.True("ql_a".Equals(session.Settings.ConfigAgentA.AgentName));
            Assert.True(AgentType.SimpleQL == session.Settings.ConfigAgentA.AgentType);
            Assert.True(session.Settings.ConfigAgentA.IsTrainable);

            Assert.True("rand_b".Equals(session.Settings.ConfigAgentB.AgentName));
            Assert.True(AgentType.Random == session.Settings.ConfigAgentB.AgentType);
            Assert.False(session.Settings.ConfigAgentB.IsTrainable);

            Assert.True(5 == session.Settings.GameConfig.Columns);
            Assert.True(4 == session.Settings.GameConfig.Rows);
            Assert.True(4 == session.Settings.GameConfig.StonesToConnect);

            // Check if correct agents types were created
            Assert.True(session.AgentA is SimpleQLAgent);
            Assert.True(session.AgentB is RandomAgent);
        }
    }
}