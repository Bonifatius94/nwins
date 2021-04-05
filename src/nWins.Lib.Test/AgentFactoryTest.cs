
using System;
using System.IO;
using Newtonsoft.Json;
using nWins.Lib.Factory;
using nWins.Lib.Agent;
using nWins.Lib.Core;
using nWins.Lib.Settings;
using Xunit;
using Xunit.Abstractions;

namespace nWins.Lib.Test
{
    public class AgentFactoryTest
    {
        //initialize logging tools
        private readonly ITestOutputHelper _logger;
        public AgentFactoryTest(ITestOutputHelper logger) { _logger = logger;}

        [Fact]
        public void CreateAgentTest()
        {
            //read test settings from random agent file
            var settings = createTrainableAgentSettings("random_agent_settings.json");
            
            //create random agent with agent settings
            IAgent agent = AgentFactory.CreateAgent(settings, GameSide.SideA);

            //make sure that the current agent type is random
            Assert.True(agent is RandomAgent);


            //read test settings from ql agent file
            settings = createTrainableAgentSettings("ql_agent_settings.json");
            
            //create ql agent with agent settings
            agent = AgentFactory.CreateAgent(settings, GameSide.SideA);

            //make sure that the current agent type is ql 
            Assert.True(agent is SimpleQLAgent);

            //check if ql agent has correct GameSide
            IGameState startingState = GameStateFactory.CreateState(4, 5);
            GameAction action_ql = agent.ChooseAction(startingState, null);
            Assert.True(action_ql.ActingSide == GameSide.SideA);


            //read test settings from dql agent file
            settings = createTrainableAgentSettings("dql_agent_settings.json");
            
            //create dql agent with agent settings
            agent = AgentFactory.CreateAgent(settings, GameSide.SideA);

            //make sure that the current agent type is dql 
            Assert.True(agent is DoubleQLAgent);

            //check if dql_agent has correct GameSide
            startingState = GameStateFactory.CreateState(4, 5);
            GameAction action_dql = agent.ChooseAction(startingState, null);
            Assert.True(action_dql.ActingSide == GameSide.SideA);


            //read test settings from sarsalamba agent file
            settings = createTrainableAgentSettings("sl_agent_settings.json");
            
            //create sl agent with agent settings
            agent = AgentFactory.CreateAgent(settings, GameSide.SideA);

            //make sure that the current agent type is sl 
            Assert.True(agent is SarsaLambdaAgent);

            //check if sl_agent has correct GameSide
            startingState = GameStateFactory.CreateState(4, 5);
            GameAction action_sl = agent.ChooseAction(startingState, null);
            Assert.True(action_sl.ActingSide == GameSide.SideA);
            //make sure that the current agent type is doubleql 
            Assert.True(agent is SarsaLambdaAgent);

            //check if dql agent has correct GameSide
            startingState = GameStateFactory.CreateState(4, 5);
            action_ql = agent.ChooseAction(startingState, null);
            Assert.True(action_ql.ActingSide == GameSide.SideA);


            //read test settings from dynaql agent file
            settings = createTrainableAgentSettings("dynaql_agent_settings.json");
            
            //create dynaql agent with agent settings
            agent = AgentFactory.CreateAgent(settings, GameSide.SideA);

            //make sure that the current agent type is dynaql 
            Assert.True(agent is DynaQLAgent);

            //check if dynaql agent has correct GameSide
            startingState = GameStateFactory.CreateState(4, 5);
            action_ql = agent.ChooseAction(startingState, null);
            Assert.True(action_ql.ActingSide == GameSide.SideA);

            //create agent with unknown agent settings
            //check if agent creation failed
            try{
                settings = createTrainableAgentSettings("unknown_agent_settings.json");
                AgentFactory.CreateAgent(settings, GameSide.SideA);
                Assert.True(false);
            } catch(Exception) {
                Assert.True(true);
            }

            // create QL agent in inference mode
            settings = createTrainableAgentSettings("ql_agent_settings_inference.json");
            agent = AgentFactory.CreateAgent(settings, GameSide.SideA);
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