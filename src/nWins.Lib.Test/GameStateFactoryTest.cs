using Xunit.Abstractions;
using Xunit;
using nWins.Lib.Factory;
using nWins.Lib.Core;
using System.Linq;
using System;


namespace nWins.Lib.Test {
    
    public class GameStateFactoryTest
    {
        //initialize logging tools
        private readonly ITestOutputHelper _logger;
        public GameStateFactoryTest(ITestOutputHelper logger) { _logger = logger;}

        [Fact]
        public void CreateStateTest()
        {
            //Create standard starting state rows = 4 and columns = 5
            IGameState state = GameStateFactory.CreateState(4, 5);

            //Check if state is type BitwiseGameState
            //Check if rows and columns are assigned correctly
            Assert.True(state is BitwiseGameState);     
            Assert.True(state.MaxRows == 4);
            Assert.True(state.MaxColumns == 5);
            Assert.True(state.Fields.Length == 4 * 5);
            Assert.True(state.Fields.All(x => x == GameSide.None));
            Assert.True(state.ColumnStoneSums.All(x => x == 0));
            
            //Create edge case state rows = 8 and columns = 8
            state = GameStateFactory.CreateState(8, 8);

            //Check if state is type BitwiseGameState
            //Check if rows and columns are assigned correctly
            Assert.True(state is BitwiseGameState);
            Assert.True(state.MaxRows == 8);
            Assert.True(state.MaxColumns == 8);
            Assert.True(state.Fields.Length == 8 * 8);
            Assert.True(state.Fields.All(x => x == GameSide.None));
            Assert.True(state.ColumnStoneSums.All(x => x == 0));

            //Create edge case state rows = 13 and columns = 5
            state = GameStateFactory.CreateState(13, 5);

            //Check if state is type SimpleGameState
            //Check if rows and columns are assigned correctly
            Assert.True(state is SimpleGameState);
            Assert.True(state.MaxRows == 13);
            Assert.True(state.MaxColumns == 5);
            Assert.True(state.Fields.Length == 13* 5);
            Assert.True(state.Fields.All(x => x == GameSide.None));
            Assert.True(state.ColumnStoneSums.All(x => x == 0));

            //Check if it is forbidden for GameStateFactory to create empty states
            try {GameStateFactory.CreateState(4, 0); Assert.True(false);} catch(ArgumentException) {Assert.True(true);} 
            try {GameStateFactory.CreateState(0, 4); Assert.True(false);} catch(ArgumentException) {Assert.True(true);} 
            try {GameStateFactory.CreateState(0, 0); Assert.True(false);} catch(ArgumentException) {Assert.True(true);} 

            //Check if it is forbidden for GameStateFactory to create states with negativ rows or column
            try {GameStateFactory.CreateState(4, -1); Assert.True(false);} catch(ArgumentException) {Assert.True(true);} 
            try {GameStateFactory.CreateState(-1, 4); Assert.True(false);} catch(ArgumentException) {Assert.True(true);} 
            try {GameStateFactory.CreateState(-1, -1); Assert.True(false);} catch(ArgumentException) {Assert.True(true);} 
        }

        [Fact]
        public void CreateStateWithGameSideFieldTest()
        {
            //Create standard starting state rows = 4 and columns = 5
            GameSide[] fields = new GameSide[4 * 5];
            Array.Fill<GameSide>(fields, GameSide.None);
            IGameState state = GameStateFactory.CreateState(4, 5, fields);

            //Check if state is type BitwiseGameState
            //Check if rows and columns are assigned correctly
            Assert.True(state is BitwiseGameState);     
            Assert.True(state.MaxRows == 4);
            Assert.True(state.MaxColumns == 5);
            Assert.True(state.Fields.Length == 4 * 5);
            Assert.True(state.Fields.All(x => x == GameSide.None));
            Assert.True(state.ColumnStoneSums.All(x => x == 0));

            //Create edge case state rows = 8 and columns = 8
            fields = new GameSide[8 * 8];
            Array.Fill<GameSide>(fields, GameSide.None);
            state = GameStateFactory.CreateState(8, 8, fields);

            //Check if state is type BitwiseGameState
            //Check if rows and columns are assigned correctly
            Assert.True(state is BitwiseGameState);
            Assert.True(state.MaxRows == 8);
            Assert.True(state.MaxColumns == 8);
            Assert.True(state.Fields.Length == 8 * 8);
            Assert.True(state.Fields.All(x => x == GameSide.None));
            Assert.True(state.ColumnStoneSums.All(x => x == 0));

            //Create edge case state rows = 13 and columns = 5
            fields = new GameSide[13 * 5];
            Array.Fill<GameSide>(fields, GameSide.None);
            state = GameStateFactory.CreateState(13, 5, fields);
            
            //Check if state is type SimpleGameState
            //Check if rows and columns are assigned correctly
            Assert.True(state is SimpleGameState);
            Assert.True(state.MaxRows == 13);
            Assert.True(state.MaxColumns == 5);
            Assert.True(state.Fields.Length == 13 * 5);
            Assert.True(state.Fields.All(x => x == GameSide.None));
            Assert.True(state.ColumnStoneSums.All(x => x == 0));

            // Create empty GameSide field
            fields = new GameSide[0];
            Array.Fill<GameSide>(fields, GameSide.None);
            //Check if it is forbidden for GameStateFactory to create empty states
            try {GameStateFactory.CreateState(4, 0, fields); Assert.True(false);} catch(ArgumentException) {Assert.True(true);} 
            try {GameStateFactory.CreateState(0, 4, fields); Assert.True(false);} catch(ArgumentException) {Assert.True(true);} 
            try {GameStateFactory.CreateState(0, 0, fields); Assert.True(false);} catch(ArgumentException) {Assert.True(true);} 
            try {GameStateFactory.CreateState(4, 4, fields); Assert.True(false);} catch(ArgumentException) {Assert.True(true);} 

            //Create GameSide field
            fields = new GameSide[20];
            Array.Fill<GameSide>(fields, GameSide.None);
            //Check if it is forbidden for GameStateFactory to create states with field size is unequal to rows * columns
            try {GameStateFactory.CreateState(4, 4, fields); Assert.True(false);} catch(ArgumentException) {Assert.True(true);}
            
            
            //Create standard starting state
            IGameState expectedState = GameStateFactory.CreateState(4, 5);

            //Play some nWins rounds
            expectedState = expectedState.ApplyAction(new GameAction(0, GameSide.SideA));
            expectedState = expectedState.ApplyAction(new GameAction(1, GameSide.SideB));
            expectedState = expectedState.ApplyAction(new GameAction(0, GameSide.SideA));
            expectedState = expectedState.ApplyAction(new GameAction(1, GameSide.SideB));
            expectedState = expectedState.ApplyAction(new GameAction(2, GameSide.SideA));
            expectedState = expectedState.ApplyAction(new GameAction(3, GameSide.SideB));
            expectedState = expectedState.ApplyAction(new GameAction(0, GameSide.SideA));

            //Create new state with field from expectedState
            fields = new GameSide[4 * 5];
            Array.Copy(expectedState.Fields, fields, 4 * 5);
            state = GameStateFactory.CreateState(4, 5, fields);

            //Check if expectedState and state are equal     
            Assert.True(expectedState.MaxRows == state.MaxRows);
            Assert.True(expectedState.MaxColumns == state.MaxColumns);
            Assert.True(expectedState.Fields.Length == state.Fields.Length);
            Assert.True(Enumerable.SequenceEqual(expectedState.Fields, state.Fields));
            Assert.True(Enumerable.SequenceEqual(expectedState.ColumnStoneSums, state.ColumnStoneSums));

            //Check if SideA cannnot apply an action, but SideB can do it
            try {state.ApplyAction(new GameAction(2, GameSide.SideA)); Assert.True(false);} catch(ArgumentException){Assert.True(true);}
            try {state.ApplyAction(new GameAction(2, GameSide.SideB)); Assert.True(true);} catch(ArgumentException){Assert.True(false);}
            }
    }
}