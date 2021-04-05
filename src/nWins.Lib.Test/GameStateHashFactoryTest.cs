using nWins.Lib.Factory;
using nWins.Lib.Core;
using System;
using System.Linq;
using Xunit;
using Xunit.Abstractions;

namespace nWins.Lib.Test
{
    public class GameStateHashFactoryTest
    {
        // initialize logging tools
        private readonly ITestOutputHelper _logger;
        public GameStateHashFactoryTest(ITestOutputHelper logger) { _logger = logger; }

        [Fact]
        public void ToBase64HashTest()
        {
            //Create standard starting state
            IGameState state = GameStateFactory.CreateState(4, 5);

            //Hash state to Base64 string
            string hash = GameStateHashFactory.ToBase64Hash(state);

            //Get hash bytes from Base64 string
            var hashBytes = Convert.FromBase64String(hash);

            //Check if hashBytes are representing the starting state
            //2 for rows and columns and 2 bits for each field
            Assert.True(2 + (2 * 4 * 5) / 8 == hashBytes.Length);
            //Check if two first bytes are rows and columns
            Assert.True(4 == hashBytes[0]);
            Assert.True(5 == hashBytes[1]);
            //Check if all other bytes are empty
            for (int i = 2; i < hashBytes.Length; i++)
            {
                Assert.True(0 == hashBytes[i]);
            }


            //Create starting state for a bigger field
            state = GameStateFactory.CreateState(13, 5);

            //Hash state to Base64 string
            hash = GameStateHashFactory.ToBase64Hash(state);

            //Get hash bytes from Base64 string
            hashBytes = Convert.FromBase64String(hash);  

            //Check if hashBytes are representing the starting state
            //2 for rows and columns and 2 bits for each field
            //(2 * 13 * 5) / 8 = 16.25 -> 17
            Assert.True(2 + 17 == hashBytes.Length);
            //Check if two first bytes are rows and columns
            Assert.True(13 == hashBytes[0]);
            Assert.True(5 == hashBytes[1]);
            //Check if all other bytes are empty
            for (int i = 2; i < hashBytes.Length; i++)
            {
                Assert.True(0 == hashBytes[i]);
            }
        }

        [Fact]
        public void FromBase64HashTest()
        {          
            //Create standard starting state from GameStateFactory
            IGameState expectedState = GameStateFactory.CreateState(4, 5);

            //Create hash code for standard starting state
            String hash = GameStateHashFactory.ToBase64Hash(expectedState);

            //Create state from hash code
            IGameState state = GameStateHashFactory.FromBase64Hash(hash);

            //Check if state is equal to a starting state from GameStateFactory    
            Assert.True(expectedState.MaxRows == state.MaxRows);
            Assert.True(expectedState.MaxColumns == state.MaxColumns);
            Assert.True(expectedState.Fields.Length == state.Fields.Length);
            Assert.True(Enumerable.SequenceEqual(expectedState.Fields, state.Fields));
            Assert.True(Enumerable.SequenceEqual(expectedState.ColumnStoneSums, state.ColumnStoneSums));


            //Create bigger starting state from GameStateFactory
            expectedState = GameStateFactory.CreateState(13, 5);

            //Create hash code for a bigger starting state
            hash = GameStateHashFactory.ToBase64Hash(expectedState);

            //Create state from hash code
            state = GameStateHashFactory.FromBase64Hash(hash);

            //Check if state is equal to a bigger starting state from GameStateFactory    
            Assert.True(expectedState.MaxRows == state.MaxRows);
            Assert.True(expectedState.MaxColumns == state.MaxColumns);
            Assert.True(expectedState.Fields.Length == state.Fields.Length);
            Assert.True(Enumerable.SequenceEqual(expectedState.Fields, state.Fields));
            Assert.True(Enumerable.SequenceEqual(expectedState.ColumnStoneSums, state.ColumnStoneSums));
        

            //Create a complex state
            expectedState = GameStateFactory.CreateState(4, 5);

            //Play some nWins rounds
            expectedState = expectedState.ApplyAction(new GameAction(0, GameSide.SideA));
            expectedState = expectedState.ApplyAction(new GameAction(1, GameSide.SideB));
            expectedState = expectedState.ApplyAction(new GameAction(0, GameSide.SideA));
            expectedState = expectedState.ApplyAction(new GameAction(1, GameSide.SideB));
            expectedState = expectedState.ApplyAction(new GameAction(2, GameSide.SideA));
            expectedState = expectedState.ApplyAction(new GameAction(3, GameSide.SideB));
            expectedState = expectedState.ApplyAction(new GameAction(0, GameSide.SideA));

            //Create hash string for a complex state
            hash = GameStateHashFactory.ToBase64Hash(expectedState);

            //Create state from hash code
            state = GameStateHashFactory.FromBase64Hash(hash);

            //Check if state is equal to the complex state  
            Assert.True(expectedState.MaxRows == state.MaxRows);
            Assert.True(expectedState.MaxColumns == state.MaxColumns);
            Assert.True(expectedState.Fields.Length == state.Fields.Length);
            Assert.True(Enumerable.SequenceEqual(expectedState.Fields, state.Fields));
            Assert.True(Enumerable.SequenceEqual(expectedState.ColumnStoneSums, state.ColumnStoneSums));
        }
    }
}