using System.Linq;
using nWins.Lib.Agent;
using nWins.Lib.Core;
using nWins.Lib.Session;
using nWins.Lib.Settings;
using Xunit;
using Xunit.Abstractions;
using nWins.Lib.Test;
using System.IO;
using Newtonsoft.Json;

namespace nWins.Lib.Test
{
    public class GameSessionTest
    {
        // initialize logging tools
        private readonly ITestOutputHelper _logger;
        public GameSessionTest(ITestOutputHelper logger) { _logger = logger; }

        [Fact]
        public void ConstructorTest()
        {
            var agentA = new RandomAgent();
            var agentB = new RandomAgent();
        
            //Create game session with 2 random agents and standard game settings
            var session = new GameSession(agentA, agentB);

            //Check if game settings are default game settings
            Assert.True(5 == session.Settings.Columns);
            Assert.True(4 == session.Settings.Rows);
            Assert.True(4 == session.Settings.StonesToConnect);

            var settings = new GameSettings();
            
            session = new GameSession(agentA, agentB, settings);

            //Check if the game settings were applied correctly
            Assert.True(settings.Columns == session.Settings.Columns);
            Assert.True(settings.Rows == session.Settings.Rows);
            Assert.True(settings.StonesToConnect == session.Settings.StonesToConnect);

            settings = new GameSettings(){Columns = 7, Rows = 6, StonesToConnect = 3};
            
            session = new GameSession(agentA, agentB, settings);

            //Check if the game settings were applied correctly
            Assert.True(settings.Columns == session.Settings.Columns);
            Assert.True(settings.Rows == session.Settings.Rows);
            Assert.True(settings.StonesToConnect == session.Settings.StonesToConnect);

            settings = new GameSettings(){Columns = 13, Rows = 5, StonesToConnect = 3};
            
            session = new GameSession(agentA, agentB, settings);

            //Check if the game settings were applied correctly
            Assert.True(settings.Columns == session.Settings.Columns);
            Assert.True(settings.Rows == session.Settings.Rows);
            Assert.True(settings.StonesToConnect == session.Settings.StonesToConnect);
        }

        [Fact]
        public void RegisterAgentsTest()
        {
            var agentA = new RandomAgent();
            var agentB = new RandomAgent();
        
            //Create game session with 2 random agents and standard game settings
            var session = new GameSession(agentA, agentB);

            // Play one round
            session.PlayGameToEnd();
            session.ResetGame();

            // Register two new agents
            // read test settings from ql agent file
            var settings = createTrainableAgentSettings("ql_agent_settings.json");
            var agentC = new SimpleQLAgent(settings, GameSide.SideA);

            settings.AgentName = "test_agent_b";
            var agentD = new SimpleQLAgent(settings, GameSide.SideB);

            // Register both agents
            session.RegisterAgents(agentC, agentD);

            // Check if type of agents has changed
            Assert.True(session.AgentA is SimpleQLAgent);
            Assert.True(session.AgentB is SimpleQLAgent);

            // Play one round
            session.PlayGameToEnd();
        }

        [Fact]
        public void GamePlayTest()
        {
            var agentA = new RandomAgent();
            var agentB = new RandomAgent();
        
            //Create game session with 2 random agents and standard game settings
            var session = new GameSession(agentA, agentB);

            for (int i = 0; i < 10000; i++)
            {
                session.PlayGameToEnd();
                session.ResetGame();
            }

            //Create game session with 2 random agents and bigger game settings
            var settings = new GameSettings(){Columns = 13, Rows = 5, StonesToConnect = 3};  
            session = new GameSession(agentA, agentB, settings);

            for (int i = 0; i < 10000; i++)
            {
                session.PlayGameToEnd();
                session.ResetGame();
            }
        }

        private TrainableAgentSettings createTrainableAgentSettings(string jsonFile)
        {
            string json;
            string settingsFilePath = Path.Combine("test_assets", jsonFile);
            using (var reader = new StreamReader(settingsFilePath)) { json = reader.ReadToEnd();}
            var settings = JsonConvert.DeserializeObject<TrainableAgentSettings>(json);
            return settings;
        }
    }
}