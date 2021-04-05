using System.Collections.Generic;
using Xunit;
using nWins.Lib.Agent;
using nWins.Lib.Core;
using Newtonsoft.Json;
using nWins.Lib.Settings;
using System.IO;
using System.Linq;
using Xunit.Abstractions;

namespace nWins.Lib.Test
{
    public class GameEngineTest
    {
        // initialize logging tools
        private readonly ITestOutputHelper _logger;
        public GameEngineTest(ITestOutputHelper logger) { _logger = logger; }

        [Fact]
        public void ConstructorTest()
        {
            // read test settings from file
            string json;
            string settingsFilePath = Path.Combine("test_assets", "game_settings.json");
            using (var reader = new StreamReader(settingsFilePath)) { json = reader.ReadToEnd(); }
            var settings = JsonConvert.DeserializeObject<GameSettings>(json);

            // Create game engine
            GameEngine engine = new GameEngine(settings);

            // Check if starting state has correct columns and rows
            Assert.True(settings.Columns == engine.CurrentState.MaxColumns);
            Assert.True(settings.Rows == engine.CurrentState.MaxRows);

            // Test stones to connect is 3
            engine.ApplyAction(new GameAction(0, GameSide.SideA));
            engine.ApplyAction(new GameAction(1, GameSide.SideB));
            engine.ApplyAction(new GameAction(0, GameSide.SideA));
            engine.ApplyAction(new GameAction(1, GameSide.SideB));
            Assert.True(GameResult.Tie == engine.Result);
            engine.ApplyAction(new GameAction(0, GameSide.SideA));
            Assert.True(GameResult.WinSideA == engine.Result);

            // Create game engine with default settings
            engine = new GameEngine(null);

            // Check if starting state has correct columns(5) and rows(4)
            Assert.True(5 == engine.CurrentState.MaxColumns);
            Assert.True(4 == engine.CurrentState.MaxRows);

            // Test stones to connect is 4 (default)
            engine.ApplyAction(new GameAction(0, GameSide.SideA));
            engine.ApplyAction(new GameAction(1, GameSide.SideB));
            engine.ApplyAction(new GameAction(0, GameSide.SideA));
            engine.ApplyAction(new GameAction(1, GameSide.SideB));
            engine.ApplyAction(new GameAction(0, GameSide.SideA));
            engine.ApplyAction(new GameAction(1, GameSide.SideB));
            Assert.True(GameResult.Tie == engine.Result);
            engine.ApplyAction(new GameAction(0, GameSide.SideA));
            Assert.True(GameResult.WinSideA == engine.Result);
        }

        [Fact]
        public void PropertiesTest()
        {
            // Create game engine with default settings
            GameEngine engine = new GameEngine(null);

            // Check if all properties are correct initialized
            Assert.True(engine.ActingSide == GameSide.SideA);
            Assert.True(!engine.ActionHistory.Any());
            Assert.True(!engine.ActionsSideA.Any());
            Assert.True(!engine.ActionsSideB.Any());
            Assert.True(!engine.AllActions.Any());
            Assert.True(null == engine.LastAction);
            Assert.True(Enumerable.SequenceEqual(engine.CurrentState.GetPossibleActions(), engine.PossibleActions));
            Assert.True(GameResult.Tie == engine.Result);

            // Initialize expected logs
            Stack<ActionLog> expectedActionHistory = new Stack<ActionLog>();
            List<ActionLog> expectedActionSideA = new List<ActionLog>();
            List<ActionLog> expectedActionSideB = new List<ActionLog>();
            List<ActionLog> expectedAllActions = new List<ActionLog>();

            //Play some nWins rounds til A wins, and write logs to own properties
            GameAction action = new GameAction(0, GameSide.SideA);
            applyActionToEngineAndProperties(action, engine, expectedActionHistory, expectedActionSideA, expectedActionSideB, expectedAllActions );
            action = new GameAction(1, GameSide.SideB);
            applyActionToEngineAndProperties(action, engine, expectedActionHistory, expectedActionSideA, expectedActionSideB, expectedAllActions );
            action = new GameAction(2, GameSide.SideA);
            applyActionToEngineAndProperties(action, engine, expectedActionHistory, expectedActionSideA, expectedActionSideB, expectedAllActions );
            action = new GameAction(3, GameSide.SideB);
            applyActionToEngineAndProperties(action, engine, expectedActionHistory, expectedActionSideA, expectedActionSideB, expectedAllActions );
            action = new GameAction(0, GameSide.SideA);
            applyActionToEngineAndProperties(action, engine, expectedActionHistory, expectedActionSideA, expectedActionSideB, expectedAllActions );
            action = new GameAction(3, GameSide.SideB);
            applyActionToEngineAndProperties(action, engine, expectedActionHistory, expectedActionSideA, expectedActionSideB, expectedAllActions );
            action = new GameAction(0, GameSide.SideA);
            applyActionToEngineAndProperties(action, engine, expectedActionHistory, expectedActionSideA, expectedActionSideB, expectedAllActions );
            action = new GameAction(4, GameSide.SideB);
            applyActionToEngineAndProperties(action, engine, expectedActionHistory, expectedActionSideA, expectedActionSideB, expectedAllActions );
            action = new GameAction(0, GameSide.SideA);
            applyActionToEngineAndProperties(action, engine, expectedActionHistory, expectedActionSideA, expectedActionSideB, expectedAllActions );

            // Check if all properties are correct after some game rounds
            Assert.True(engine.ActingSide == GameSide.SideB);
            Assert.True(Enumerable.SequenceEqual(expectedActionHistory ,engine.ActionHistory));
            Assert.True(Enumerable.SequenceEqual(expectedActionSideA ,engine.ActionsSideA));
            Assert.True(Enumerable.SequenceEqual(expectedActionSideB ,engine.ActionsSideB));
            Assert.True(Enumerable.SequenceEqual(expectedAllActions ,engine.AllActions));
            Assert.Equal(action, engine.LastAction);
            Assert.True(Enumerable.SequenceEqual(engine.CurrentState.GetPossibleActions(), engine.PossibleActions));
            Assert.True(GameResult.WinSideA == engine.Result);


            // Create game engine with default settings
            engine = new GameEngine(null);

            // Reset expected logs
            expectedActionHistory = new Stack<ActionLog>();
            expectedActionSideA = new List<ActionLog>();
            expectedActionSideB = new List<ActionLog>();
            expectedAllActions = new List<ActionLog>();

            //Play some nWins rounds til B wins, and write logs to own properties
            action = new GameAction(0, GameSide.SideA);
            applyActionToEngineAndProperties(action, engine, expectedActionHistory, expectedActionSideA, expectedActionSideB, expectedAllActions );
            action = new GameAction(1, GameSide.SideB);
            applyActionToEngineAndProperties(action, engine, expectedActionHistory, expectedActionSideA, expectedActionSideB, expectedAllActions );
            action = new GameAction(2, GameSide.SideA);
            applyActionToEngineAndProperties(action, engine, expectedActionHistory, expectedActionSideA, expectedActionSideB, expectedAllActions );
            action = new GameAction(1, GameSide.SideB);
            applyActionToEngineAndProperties(action, engine, expectedActionHistory, expectedActionSideA, expectedActionSideB, expectedAllActions );
            action = new GameAction(3, GameSide.SideA);
            applyActionToEngineAndProperties(action, engine, expectedActionHistory, expectedActionSideA, expectedActionSideB, expectedAllActions );
            action = new GameAction(1, GameSide.SideB);
            applyActionToEngineAndProperties(action, engine, expectedActionHistory, expectedActionSideA, expectedActionSideB, expectedAllActions );
            action = new GameAction(0, GameSide.SideA);
            applyActionToEngineAndProperties(action, engine, expectedActionHistory, expectedActionSideA, expectedActionSideB, expectedAllActions );
            action = new GameAction(1, GameSide.SideB);
            applyActionToEngineAndProperties(action, engine, expectedActionHistory, expectedActionSideA, expectedActionSideB, expectedAllActions );

            // Check if all properties are correct after some game rounds
            Assert.True(engine.ActingSide == GameSide.SideA);
            Assert.True(Enumerable.SequenceEqual(expectedActionHistory ,engine.ActionHistory));
            Assert.True(Enumerable.SequenceEqual(expectedActionSideA ,engine.ActionsSideA));
            Assert.True(Enumerable.SequenceEqual(expectedActionSideB ,engine.ActionsSideB));
            Assert.True(Enumerable.SequenceEqual(expectedAllActions ,engine.AllActions));
            Assert.Equal(action, engine.LastAction);
            Assert.True(Enumerable.SequenceEqual(engine.CurrentState.GetPossibleActions(), engine.PossibleActions));
            Assert.True(GameResult.WinSideB == engine.Result);


            // Create game engine with default settings
            engine = new GameEngine(null);

            // Reset expected logs
            expectedActionHistory = new Stack<ActionLog>();
            expectedActionSideA = new List<ActionLog>();
            expectedActionSideB = new List<ActionLog>();
            expectedAllActions = new List<ActionLog>();

            //Play some nWins rounds til tie, and write logs to own properties
            action = new GameAction(0, GameSide.SideA);
            applyActionToEngineAndProperties(action, engine, expectedActionHistory, expectedActionSideA, expectedActionSideB, expectedAllActions );
            action = new GameAction(1, GameSide.SideB);
            applyActionToEngineAndProperties(action, engine, expectedActionHistory, expectedActionSideA, expectedActionSideB, expectedAllActions );
            action = new GameAction(2, GameSide.SideA);
            applyActionToEngineAndProperties(action, engine, expectedActionHistory, expectedActionSideA, expectedActionSideB, expectedAllActions );
            action = new GameAction(3, GameSide.SideB);
            applyActionToEngineAndProperties(action, engine, expectedActionHistory, expectedActionSideA, expectedActionSideB, expectedAllActions );
            action = new GameAction(4, GameSide.SideA);
            applyActionToEngineAndProperties(action, engine, expectedActionHistory, expectedActionSideA, expectedActionSideB, expectedAllActions );
            action = new GameAction(4, GameSide.SideB);
            applyActionToEngineAndProperties(action, engine, expectedActionHistory, expectedActionSideA, expectedActionSideB, expectedAllActions );
            action = new GameAction(3, GameSide.SideA);
            applyActionToEngineAndProperties(action, engine, expectedActionHistory, expectedActionSideA, expectedActionSideB, expectedAllActions );
            action = new GameAction(2, GameSide.SideB);
            applyActionToEngineAndProperties(action, engine, expectedActionHistory, expectedActionSideA, expectedActionSideB, expectedAllActions );
            action = new GameAction(1, GameSide.SideA);
            applyActionToEngineAndProperties(action, engine, expectedActionHistory, expectedActionSideA, expectedActionSideB, expectedAllActions );
            action = new GameAction(0, GameSide.SideB);
            applyActionToEngineAndProperties(action, engine, expectedActionHistory, expectedActionSideA, expectedActionSideB, expectedAllActions );
            action = new GameAction(0, GameSide.SideA);
            applyActionToEngineAndProperties(action, engine, expectedActionHistory, expectedActionSideA, expectedActionSideB, expectedAllActions );
            action = new GameAction(1, GameSide.SideB);
            applyActionToEngineAndProperties(action, engine, expectedActionHistory, expectedActionSideA, expectedActionSideB, expectedAllActions );
            action = new GameAction(2, GameSide.SideA);
            applyActionToEngineAndProperties(action, engine, expectedActionHistory, expectedActionSideA, expectedActionSideB, expectedAllActions );
            action = new GameAction(3, GameSide.SideB);
            applyActionToEngineAndProperties(action, engine, expectedActionHistory, expectedActionSideA, expectedActionSideB, expectedAllActions );
            action = new GameAction(4, GameSide.SideA);
            applyActionToEngineAndProperties(action, engine, expectedActionHistory, expectedActionSideA, expectedActionSideB, expectedAllActions );
            action = new GameAction(3, GameSide.SideB);
            applyActionToEngineAndProperties(action, engine, expectedActionHistory, expectedActionSideA, expectedActionSideB, expectedAllActions );
            action = new GameAction(4, GameSide.SideA);
            applyActionToEngineAndProperties(action, engine, expectedActionHistory, expectedActionSideA, expectedActionSideB, expectedAllActions );
            action = new GameAction(2, GameSide.SideB);
            applyActionToEngineAndProperties(action, engine, expectedActionHistory, expectedActionSideA, expectedActionSideB, expectedAllActions );
            action = new GameAction(0, GameSide.SideA);
            applyActionToEngineAndProperties(action, engine, expectedActionHistory, expectedActionSideA, expectedActionSideB, expectedAllActions );
            action = new GameAction(1, GameSide.SideB);
            applyActionToEngineAndProperties(action, engine, expectedActionHistory, expectedActionSideA, expectedActionSideB, expectedAllActions );         

            // Check if all properties are correct after some game rounds
            Assert.True(engine.ActingSide == GameSide.SideA);
            Assert.True(Enumerable.SequenceEqual(expectedActionHistory ,engine.ActionHistory));
            Assert.True(Enumerable.SequenceEqual(expectedActionSideA ,engine.ActionsSideA));
            Assert.True(Enumerable.SequenceEqual(expectedActionSideB ,engine.ActionsSideB));
            Assert.True(Enumerable.SequenceEqual(expectedAllActions ,engine.AllActions));
            Assert.Equal(action, engine.LastAction);
            Assert.True(Enumerable.SequenceEqual(engine.CurrentState.GetPossibleActions(), engine.PossibleActions));
            Assert.True(GameResult.Tie == engine.Result);


            // Tests for bigger games > 64 fields
            var settings = new GameSettings(){Columns = 5, Rows = 13, StonesToConnect = 4};

            engine = new GameEngine(settings); 

            // Reset expected logs
            expectedActionHistory = new Stack<ActionLog>();
            expectedActionSideA = new List<ActionLog>();
            expectedActionSideB = new List<ActionLog>();
            expectedAllActions = new List<ActionLog>();

            //Play some nWins rounds til A wins, and write logs to own properties
            action = new GameAction(0, GameSide.SideA);
            applyActionToEngineAndProperties(action, engine, expectedActionHistory, expectedActionSideA, expectedActionSideB, expectedAllActions );
            action = new GameAction(1, GameSide.SideB);
            applyActionToEngineAndProperties(action, engine, expectedActionHistory, expectedActionSideA, expectedActionSideB, expectedAllActions );
            action = new GameAction(2, GameSide.SideA);
            applyActionToEngineAndProperties(action, engine, expectedActionHistory, expectedActionSideA, expectedActionSideB, expectedAllActions );
            action = new GameAction(3, GameSide.SideB);
            applyActionToEngineAndProperties(action, engine, expectedActionHistory, expectedActionSideA, expectedActionSideB, expectedAllActions );
            action = new GameAction(0, GameSide.SideA);
            applyActionToEngineAndProperties(action, engine, expectedActionHistory, expectedActionSideA, expectedActionSideB, expectedAllActions );
            action = new GameAction(3, GameSide.SideB);
            applyActionToEngineAndProperties(action, engine, expectedActionHistory, expectedActionSideA, expectedActionSideB, expectedAllActions );
            action = new GameAction(0, GameSide.SideA);
            applyActionToEngineAndProperties(action, engine, expectedActionHistory, expectedActionSideA, expectedActionSideB, expectedAllActions );
            action = new GameAction(4, GameSide.SideB);
            applyActionToEngineAndProperties(action, engine, expectedActionHistory, expectedActionSideA, expectedActionSideB, expectedAllActions );
            action = new GameAction(0, GameSide.SideA);
            applyActionToEngineAndProperties(action, engine, expectedActionHistory, expectedActionSideA, expectedActionSideB, expectedAllActions );

            // Check if all properties are correct after some game rounds
            Assert.True(engine.ActingSide == GameSide.SideB);
            Assert.True(Enumerable.SequenceEqual(expectedActionHistory ,engine.ActionHistory));
            Assert.True(Enumerable.SequenceEqual(expectedActionSideA ,engine.ActionsSideA));
            Assert.True(Enumerable.SequenceEqual(expectedActionSideB ,engine.ActionsSideB));
            Assert.True(Enumerable.SequenceEqual(expectedAllActions ,engine.AllActions));
            Assert.Equal(action, engine.LastAction);
            Assert.True(Enumerable.SequenceEqual(engine.CurrentState.GetPossibleActions(), engine.PossibleActions));
            Assert.True(GameResult.WinSideA == engine.Result);

            // Reset game
            engine = new GameEngine(settings); 

            // Reset expected logs
            expectedActionHistory = new Stack<ActionLog>();
            expectedActionSideA = new List<ActionLog>();
            expectedActionSideB = new List<ActionLog>();
            expectedAllActions = new List<ActionLog>();

            //Play some nWins rounds til B wins, and write logs to own properties
            action = new GameAction(0, GameSide.SideA);
            applyActionToEngineAndProperties(action, engine, expectedActionHistory, expectedActionSideA, expectedActionSideB, expectedAllActions );
            action = new GameAction(1, GameSide.SideB);
            applyActionToEngineAndProperties(action, engine, expectedActionHistory, expectedActionSideA, expectedActionSideB, expectedAllActions );
            action = new GameAction(2, GameSide.SideA);
            applyActionToEngineAndProperties(action, engine, expectedActionHistory, expectedActionSideA, expectedActionSideB, expectedAllActions );
            action = new GameAction(1, GameSide.SideB);
            applyActionToEngineAndProperties(action, engine, expectedActionHistory, expectedActionSideA, expectedActionSideB, expectedAllActions );
            action = new GameAction(3, GameSide.SideA);
            applyActionToEngineAndProperties(action, engine, expectedActionHistory, expectedActionSideA, expectedActionSideB, expectedAllActions );
            action = new GameAction(1, GameSide.SideB);
            applyActionToEngineAndProperties(action, engine, expectedActionHistory, expectedActionSideA, expectedActionSideB, expectedAllActions );
            action = new GameAction(0, GameSide.SideA);
            applyActionToEngineAndProperties(action, engine, expectedActionHistory, expectedActionSideA, expectedActionSideB, expectedAllActions );
            action = new GameAction(1, GameSide.SideB);
            applyActionToEngineAndProperties(action, engine, expectedActionHistory, expectedActionSideA, expectedActionSideB, expectedAllActions );

            // Check if all properties are correct after some game rounds
            Assert.True(engine.ActingSide == GameSide.SideA);
            Assert.True(Enumerable.SequenceEqual(expectedActionHistory ,engine.ActionHistory));
            Assert.True(Enumerable.SequenceEqual(expectedActionSideA ,engine.ActionsSideA));
            Assert.True(Enumerable.SequenceEqual(expectedActionSideB ,engine.ActionsSideB));
            Assert.True(Enumerable.SequenceEqual(expectedAllActions ,engine.AllActions));
            Assert.Equal(action, engine.LastAction);
            Assert.True(Enumerable.SequenceEqual(engine.CurrentState.GetPossibleActions(), engine.PossibleActions));
            Assert.True(GameResult.WinSideB == engine.Result);
        }

        private void applyActionToEngineAndProperties(GameAction action, GameEngine engine, Stack<ActionLog> expectedActionHistory, List<ActionLog> expectedActionSideA , List<ActionLog> expectedActionSideB, List<ActionLog> expectedAllActions) {
            ActionLog log = engine.ApplyAction(action);
            expectedActionHistory.Push(log);
            if (action.ActingSide == GameSide.SideA) {
                expectedActionSideA.Add(log);
            } else {
                expectedActionSideB.Add(log);
            }
            expectedAllActions.Add(log);
        }

        [Fact]
        public void ApplyActionTest()
        {
            // Logs were tested in properties tests
            // Tests here: granted rewards for wins on both sides and ties
            // Create game engine with default settings
            GameEngine engine = new GameEngine(null);

            //Play some nWins rounds til A wins
            engine.ApplyAction(new GameAction(0, GameSide.SideA));
            engine.ApplyAction(new GameAction(1, GameSide.SideB));
            engine.ApplyAction(new GameAction(2, GameSide.SideA));
            engine.ApplyAction(new GameAction(3, GameSide.SideB));
            engine.ApplyAction(new GameAction(0, GameSide.SideA));
            engine.ApplyAction(new GameAction(3, GameSide.SideB));
            engine.ApplyAction(new GameAction(0, GameSide.SideA));
            engine.ApplyAction(new GameAction(4, GameSide.SideB));
            engine.ApplyAction(new GameAction(0, GameSide.SideA));

            // Test if last state has winning reward and all others state have no reward
            Assert.True(1 == engine.AllActions.Last().Reward);
            for (int i = 0; i < engine.AllActions.Count() - 1; i++)
            {
                Assert.True(0 == engine.AllActions.ElementAt(i).Reward);
            }

            // Create new game engine with default settings
            engine = new GameEngine(null);

            //Play some nWins rounds til B wins
            engine.ApplyAction(new GameAction(0, GameSide.SideA));
            engine.ApplyAction(new GameAction(1, GameSide.SideB));
            engine.ApplyAction(new GameAction(2, GameSide.SideA));
            engine.ApplyAction(new GameAction(1, GameSide.SideB));
            engine.ApplyAction(new GameAction(3, GameSide.SideA));
            engine.ApplyAction(new GameAction(1, GameSide.SideB));
            engine.ApplyAction(new GameAction(0, GameSide.SideA));
            engine.ApplyAction(new GameAction(1, GameSide.SideB));

            // Test if last state has winning reward and all others state have no reward
            Assert.True(1 == engine.AllActions.Last().Reward);
            for (int i = 0; i < engine.AllActions.Count() - 1; i++)
            {
                Assert.True(0 == engine.AllActions.ElementAt(i).Reward);
            }

            // Create new game engine with default settings
            engine = new GameEngine(null);

            //Play some nWins rounds til tie
            engine.ApplyAction(new GameAction(0, GameSide.SideA));
            engine.ApplyAction(new GameAction(1, GameSide.SideB));
            engine.ApplyAction(new GameAction(2, GameSide.SideA));
            engine.ApplyAction(new GameAction(3, GameSide.SideB));
            engine.ApplyAction(new GameAction(4, GameSide.SideA));
            engine.ApplyAction(new GameAction(4, GameSide.SideB));
            engine.ApplyAction(new GameAction(3, GameSide.SideA));
            engine.ApplyAction(new GameAction(2, GameSide.SideB));
            engine.ApplyAction(new GameAction(1, GameSide.SideA));
            engine.ApplyAction(new GameAction(0, GameSide.SideB));
            engine.ApplyAction(new GameAction(0, GameSide.SideA));
            engine.ApplyAction(new GameAction(1, GameSide.SideB));
            engine.ApplyAction(new GameAction(2, GameSide.SideA));
            engine.ApplyAction(new GameAction(3, GameSide.SideB));
            engine.ApplyAction(new GameAction(4, GameSide.SideA));
            engine.ApplyAction(new GameAction(3, GameSide.SideB));
            engine.ApplyAction(new GameAction(4, GameSide.SideA));
            engine.ApplyAction(new GameAction(2, GameSide.SideB));
            engine.ApplyAction(new GameAction(0, GameSide.SideA));
            engine.ApplyAction(new GameAction(1, GameSide.SideB));

            // Test if the last two states have tie reward and all others state have no reward
            Assert.True(0.5 == engine.AllActions.Last().Reward);
            Assert.True(0.5 == engine.AllActions.ElementAt(engine.AllActions.Count() - 2).Reward);
            for (int i = 0; i < engine.AllActions.Count() - 2; i++)
            {
                Assert.True(0 == engine.AllActions.ElementAt(i).Reward);
            }


            // Tests for bigger games > 64 fields
            var settings = new GameSettings(){Columns = 5, Rows = 13, StonesToConnect = 4};

            engine = new GameEngine(settings); 

             //Play some nWins rounds til A wins
            engine.ApplyAction(new GameAction(0, GameSide.SideA));
            engine.ApplyAction(new GameAction(1, GameSide.SideB));
            engine.ApplyAction(new GameAction(2, GameSide.SideA));
            engine.ApplyAction(new GameAction(3, GameSide.SideB));
            engine.ApplyAction(new GameAction(0, GameSide.SideA));
            engine.ApplyAction(new GameAction(3, GameSide.SideB));
            engine.ApplyAction(new GameAction(0, GameSide.SideA));
            engine.ApplyAction(new GameAction(4, GameSide.SideB));
            engine.ApplyAction(new GameAction(0, GameSide.SideA));

            // Test if last state has winning reward and all others state have no reward
            Assert.True(1 == engine.AllActions.Last().Reward);
            for (int i = 0; i < engine.AllActions.Count() - 1; i++)
            {
                Assert.True(0 == engine.AllActions.ElementAt(i).Reward);
            }

            // Create new game engine with default settings
            engine = new GameEngine(settings);

            //Play some nWins rounds til B wins
            engine.ApplyAction(new GameAction(0, GameSide.SideA));
            engine.ApplyAction(new GameAction(1, GameSide.SideB));
            engine.ApplyAction(new GameAction(2, GameSide.SideA));
            engine.ApplyAction(new GameAction(1, GameSide.SideB));
            engine.ApplyAction(new GameAction(3, GameSide.SideA));
            engine.ApplyAction(new GameAction(1, GameSide.SideB));
            engine.ApplyAction(new GameAction(0, GameSide.SideA));
            engine.ApplyAction(new GameAction(1, GameSide.SideB));

            // Test if last state has winning reward and all others state have no reward
            Assert.True(1 == engine.AllActions.Last().Reward);
            for (int i = 0; i < engine.AllActions.Count() - 1; i++)
            {
                Assert.True(0 == engine.AllActions.ElementAt(i).Reward);
            }
        }
    }
}
