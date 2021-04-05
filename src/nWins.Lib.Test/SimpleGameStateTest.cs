using System;
using System.Collections.Generic;
using System.Linq;
using nWins.Lib.Core;
using nWins.Lib.Factory;
using Xunit;
using Xunit.Abstractions;

namespace nWins.Lib.Test
{
    public class SimpleGameStateTest
    {
        // initialize logging tools
        private readonly ITestOutputHelper _logger;
        public SimpleGameStateTest(ITestOutputHelper logger) { _logger = logger; }

        [Fact]
        public void ConstructorTest()
        {
            // make sure that the constructor initializes empty fields of the expected amount
            // check if the max rows / columns are assigned correctly
            var state = GameStateFactory.CreateState(8, 9);
            Assert.True(state.Fields.Length == 72);
            Assert.True(state.MaxRows == 8 && state.MaxColumns == 9);
            Assert.True(state.Fields.All(x => x == GameSide.None));

            // make sure that the constructor also works for non-standard sized boards
            state = GameStateFactory.CreateState(51, 43);
            Assert.True(state.Fields.Length == 2193);
            Assert.True(state.MaxRows == 51 && state.MaxColumns == 43);
            Assert.True(state.Fields.All(x => x == GameSide.None));
        }

        [Fact]
        public void ApplyActionTest()
        {
            // phase 1: test if the arguments are validated correctly before executing the action
            var state = GameStateFactory.CreateState(8, 9);
            var state1 = state.ApplyAction(new GameAction(0, GameSide.SideA));

            // check if the acting side may not be GameSide.None
            try { state.ApplyAction(new GameAction(0, GameSide.None));   Assert.True(false); } catch (ArgumentException) { Assert.True(true);  }
            try { state.ApplyAction(new GameAction(0, GameSide.SideA));  Assert.True(true);  } catch (ArgumentException) { Assert.True(false); }
            try { state1.ApplyAction(new GameAction(0, GameSide.SideB)); Assert.True(true);  } catch (ArgumentException) { Assert.True(false); }

            // check if a column index overflow is detected correctly
            try { state.ApplyAction(new GameAction(8, GameSide.SideA));   Assert.True(true); } catch (IndexOutOfRangeException) { Assert.True(false);  }
            try { state.ApplyAction(new GameAction(9, GameSide.SideA));   Assert.True(false);  } catch (IndexOutOfRangeException) { Assert.True(true); }
            try { state.ApplyAction(new GameAction(100, GameSide.SideA)); Assert.True(false);  } catch (IndexOutOfRangeException) { Assert.True(true); }

            // check if the game state enforces alternating sides
            try { state.ApplyAction(new GameAction(0, GameSide.SideA));  Assert.True(true);  } catch (ArgumentException) { Assert.True(false); }
            try { state.ApplyAction(new GameAction(0, GameSide.SideB));  Assert.True(false); } catch (ArgumentException) { Assert.True(true);  }
            try { state1.ApplyAction(new GameAction(0, GameSide.SideA)); Assert.True(false); } catch (ArgumentException) { Assert.True(true);  }
            try { state1.ApplyAction(new GameAction(0, GameSide.SideB)); Assert.True(true);  } catch (ArgumentException) { Assert.True(false); }

            // check if the game state forbids putting stones into occupied columns
            var tempState = state;
            try
            {
                tempState = tempState.ApplyAction(new GameAction(0, GameSide.SideA));
                tempState = tempState.ApplyAction(new GameAction(0, GameSide.SideB));
                tempState = tempState.ApplyAction(new GameAction(0, GameSide.SideA));
                tempState = tempState.ApplyAction(new GameAction(0, GameSide.SideB));
                tempState = tempState.ApplyAction(new GameAction(0, GameSide.SideA));
                tempState = tempState.ApplyAction(new GameAction(0, GameSide.SideB));
                tempState = tempState.ApplyAction(new GameAction(0, GameSide.SideA));
                tempState = tempState.ApplyAction(new GameAction(0, GameSide.SideB));
            } 
            catch (ArgumentException) { Assert.True(false); }
            try
            {
                tempState.ApplyAction(new GameAction(0, GameSide.SideA));
                Assert.True(false);
            }
            catch (ArgumentException) { Assert.True(true); }
            try
            {
                tempState = tempState.ApplyAction(new GameAction(1, GameSide.SideA));
                tempState.ApplyAction(new GameAction(0, GameSide.SideB));
                Assert.True(false);
            }
            catch (ArgumentException) { Assert.True(true); }

            // phase 2: test if the action gets applied correctly at all possible board positions for either side

            for (int column = 0; column < state.MaxColumns; column++)
            {
                for (int row = 0; row < state.MaxRows; row++)
                {
                    // create a board where the players can put a stone at the position to be tested
                    state = prepareState(row, column);
                    state1 = state.ApplyAction(new GameAction((uint)((column + 1) % state.MaxColumns), GameSide.SideA));

                    // put a stone for both sides
                    for (int sideNum = 1; sideNum < 3; sideNum++)
                    {
                        _logger.WriteLine($"col={ column }, row={ row }, side={ (GameSide)sideNum }");

                        var side = (GameSide)sideNum;
                        var stateBefore = side == GameSide.SideA ? state : state1;
                        var stateAfter = stateBefore.ApplyAction(new GameAction((uint)column, side));
                        int changedIndex = row * stateAfter.MaxColumns + column;

                        // make sure that the field dimensions are the same
                        Assert.True(stateBefore.MaxRows == stateAfter.MaxRows && stateBefore.MaxColumns == stateAfter.MaxColumns);

                        // make sure that the affected field is changed correctly and that the remaining fields stay unchanged
                        Assert.True(Enumerable.Range(0, stateAfter.Fields.Length)
                            .All(i => i == changedIndex ? stateAfter.Fields[i] == side : stateAfter.Fields[i] == stateBefore.Fields[i]));

                        // make sure that the column sum affected is incremented and the other column sums stay unchanged
                        Assert.True(Enumerable.Range(0, stateAfter.MaxColumns)
                            .All(i => stateAfter.ColumnStoneSums[i] - stateBefore.ColumnStoneSums[i] == (i == column ? 1 : 0)));
                    }
                }
            }
        }

        private IGameState prepareState(int row, int col)
        {
            var state = GameStateFactory.CreateState(8, 9);

            // do nothing when row is zero
            if (row == 0) { return state; }

            // apply actions to the column by alternating sides until row-1 stones have been put there
            for (int i = 0; i < row; i++) { state = state.ApplyAction(new GameAction((uint)col, (GameSide)(i % 2 + 1))); }

            // make sure that side A is acting on the returned state
            if (row % 2 == 1) { state = state.ApplyAction(new GameAction((uint)((col + 1) % state.MaxColumns), GameSide.SideB)); }
            
            return state;
        }

        [Fact]
        public void GetPossibleActionsTest()
        {
            // for all columns: make sure that they are not on the possible actions set if they are occupied (for both sides)
            var initialState = GameStateFactory.CreateState(8, 9);
            var allColumns = Enumerable.Range(0, initialState.MaxColumns);
            var columnPermutations = SubSetsOf(allColumns);

            foreach (var perm in columnPermutations)
            {
                _logger.WriteLine($"perm: {{ { (perm.Count() > 1 ? perm.Select(x => x.ToString()).Aggregate((x, y) => x + ", " + y) : perm.FirstOrDefault()) } }}");

                // fill columns of the given set (permutation)
                var state = initialState;
                foreach (int col in perm) { state = fillColumn(state, col, initialState.MaxRows); }
                bool allOccupied = perm.SequenceEqual(allColumns);

                // make sure that the columns from the permutation do not appear on the possible actions set
                // and also make sure that all other actions do appear

                // test side A actions
                var expectedActions = allColumns.Except(perm).Select(col => new GameAction((uint)col, GameSide.SideA));
                Assert.True(state.GetPossibleActions().SequenceEqual(expectedActions));

                // test side B actions
                if (!allOccupied)
                {
                    var state1 = state.ApplyAction(new GameAction((uint)allColumns.Except(perm).First(), GameSide.SideA));
                    var expectedActions1 = allColumns.Except(perm).Select(col => new GameAction((uint)col, GameSide.SideB));
                    Assert.True(state1.GetPossibleActions().SequenceEqual(expectedActions1));
                }
            }
        }

        #region Helpers

        private IGameState fillColumn(IGameState state, int column, int rowsToFill)
        {
            for (int row = 0; row < rowsToFill; row++)
            {
                var side = (row % 2 == 0) ? GameSide.SideA : GameSide.SideB;
                state = state.ApplyAction(new GameAction((uint)column, side));
            }
            
            return state;
        }

        private IEnumerable<IEnumerable<T>> SubSetsOf<T>(IEnumerable<T> source)
        {
            // snippet source: https://stackoverflow.com/Questions/999050/how-to-get-all-subsets-of-an-array

            // recursion termination
            if (!source.Any())
                return Enumerable.Repeat(Enumerable.Empty<T>(), 1);

            // recursion call
            var element = source.Take(1);
            var haveNots = SubSetsOf(source.Skip(1));
            var haves = haveNots.Select(set => element.Concat(set));

            return haves.Concat(haveNots);
        }

        #endregion Helpers

        [Fact]
        public void IsConnectNTest()
        {
            // test vertical win condition (side A)
            var state = GameStateFactory.CreateState(8, 9);
            state = state.ApplyAction(new GameAction(0, GameSide.SideA));
            state = state.ApplyAction(new GameAction(1, GameSide.SideB));
            state = state.ApplyAction(new GameAction(0, GameSide.SideA));
            state = state.ApplyAction(new GameAction(1, GameSide.SideB));
            state = state.ApplyAction(new GameAction(0, GameSide.SideA));
            state = state.ApplyAction(new GameAction(1, GameSide.SideB));
            Assert.True(!state.IsConnectN(4)); // expect false
            state = state.ApplyAction(new GameAction(0, GameSide.SideA));
            Assert.True(state.IsConnectN(4)); // expect true

            // test horizontal win condition (side A)
            state = GameStateFactory.CreateState(8, 9);
            state = state.ApplyAction(new GameAction(0, GameSide.SideA));
            state = state.ApplyAction(new GameAction(4, GameSide.SideB));
            state = state.ApplyAction(new GameAction(1, GameSide.SideA));
            state = state.ApplyAction(new GameAction(4, GameSide.SideB));
            state = state.ApplyAction(new GameAction(2, GameSide.SideA));
            state = state.ApplyAction(new GameAction(4, GameSide.SideB));
            Assert.True(!state.IsConnectN(4)); // expect false
            state = state.ApplyAction(new GameAction(3, GameSide.SideA));
            Assert.True(state.IsConnectN(4)); // expect true

            // test diagonal win condition (side A)
            state = GameStateFactory.CreateState(8, 9);
            state = state.ApplyAction(new GameAction(0, GameSide.SideA));
            state = state.ApplyAction(new GameAction(1, GameSide.SideB));
            state = state.ApplyAction(new GameAction(1, GameSide.SideA));
            state = state.ApplyAction(new GameAction(2, GameSide.SideB));
            state = state.ApplyAction(new GameAction(2, GameSide.SideA));
            state = state.ApplyAction(new GameAction(3, GameSide.SideB));
            state = state.ApplyAction(new GameAction(2, GameSide.SideA));
            state = state.ApplyAction(new GameAction(3, GameSide.SideB));
            state = state.ApplyAction(new GameAction(3, GameSide.SideA));
            state = state.ApplyAction(new GameAction(5, GameSide.SideB));
            Assert.True(!state.IsConnectN(4)); // expect false
            state = state.ApplyAction(new GameAction(3, GameSide.SideA));
            Assert.True(state.IsConnectN(4)); // expect true

            // test diagonal win condition (side A, reverse)
            state = GameStateFactory.CreateState(8, 9);
            state = state.ApplyAction(new GameAction(6, GameSide.SideA));
            state = state.ApplyAction(new GameAction(5, GameSide.SideB));
            state = state.ApplyAction(new GameAction(5, GameSide.SideA));
            state = state.ApplyAction(new GameAction(4, GameSide.SideB));
            state = state.ApplyAction(new GameAction(4, GameSide.SideA));
            state = state.ApplyAction(new GameAction(3, GameSide.SideB));
            state = state.ApplyAction(new GameAction(4, GameSide.SideA));
            state = state.ApplyAction(new GameAction(3, GameSide.SideB));
            state = state.ApplyAction(new GameAction(3, GameSide.SideA));
            state = state.ApplyAction(new GameAction(1, GameSide.SideB));
            Assert.True(!state.IsConnectN(4)); // expect false
            state = state.ApplyAction(new GameAction(3, GameSide.SideA));
            Assert.True(state.IsConnectN(4)); // expect true

            // test vertical win condition (side B)
            state = GameStateFactory.CreateState(8, 9);
            state = state.ApplyAction(new GameAction(5, GameSide.SideA));
            state = state.ApplyAction(new GameAction(0, GameSide.SideB));
            state = state.ApplyAction(new GameAction(1, GameSide.SideA));
            state = state.ApplyAction(new GameAction(0, GameSide.SideB));
            state = state.ApplyAction(new GameAction(1, GameSide.SideA));
            state = state.ApplyAction(new GameAction(0, GameSide.SideB));
            state = state.ApplyAction(new GameAction(1, GameSide.SideA));
            Assert.True(!state.IsConnectN(4)); // expect false
            state = state.ApplyAction(new GameAction(0, GameSide.SideB));
            Assert.True(state.IsConnectN(4)); // expect true

            // test horizontal win condition (side B)
            state = GameStateFactory.CreateState(8, 9);
            state = state.ApplyAction(new GameAction(5, GameSide.SideA));
            state = state.ApplyAction(new GameAction(0, GameSide.SideB));
            state = state.ApplyAction(new GameAction(4, GameSide.SideA));
            state = state.ApplyAction(new GameAction(1, GameSide.SideB));
            state = state.ApplyAction(new GameAction(4, GameSide.SideA));
            state = state.ApplyAction(new GameAction(2, GameSide.SideB));
            state = state.ApplyAction(new GameAction(4, GameSide.SideA));
            Assert.True(!state.IsConnectN(4)); // expect false
            state = state.ApplyAction(new GameAction(3, GameSide.SideB));
            Assert.True(state.IsConnectN(4)); // expect true

            // test diagonal win condition (side B)
            state = GameStateFactory.CreateState(8, 9);
            state = state.ApplyAction(new GameAction(5, GameSide.SideA));
            state = state.ApplyAction(new GameAction(0, GameSide.SideB));
            state = state.ApplyAction(new GameAction(1, GameSide.SideA));
            state = state.ApplyAction(new GameAction(1, GameSide.SideB));
            state = state.ApplyAction(new GameAction(2, GameSide.SideA));
            state = state.ApplyAction(new GameAction(2, GameSide.SideB));
            state = state.ApplyAction(new GameAction(3, GameSide.SideA));
            state = state.ApplyAction(new GameAction(2, GameSide.SideB));
            state = state.ApplyAction(new GameAction(3, GameSide.SideA));
            state = state.ApplyAction(new GameAction(3, GameSide.SideB));
            state = state.ApplyAction(new GameAction(5, GameSide.SideA));
            Assert.True(!state.IsConnectN(4)); // expect false
            state = state.ApplyAction(new GameAction(3, GameSide.SideB));
            Assert.True(state.IsConnectN(4)); // expect true

            // test diagonal win condition (side B, reverse)
            state = GameStateFactory.CreateState(8, 9);
            state = state.ApplyAction(new GameAction(1, GameSide.SideA));
            state = state.ApplyAction(new GameAction(6, GameSide.SideB));
            state = state.ApplyAction(new GameAction(5, GameSide.SideA));
            state = state.ApplyAction(new GameAction(5, GameSide.SideB));
            state = state.ApplyAction(new GameAction(4, GameSide.SideA));
            state = state.ApplyAction(new GameAction(4, GameSide.SideB));
            state = state.ApplyAction(new GameAction(3, GameSide.SideA));
            state = state.ApplyAction(new GameAction(4, GameSide.SideB));
            state = state.ApplyAction(new GameAction(3, GameSide.SideA));
            state = state.ApplyAction(new GameAction(3, GameSide.SideB));
            state = state.ApplyAction(new GameAction(1, GameSide.SideA));
            Assert.True(!state.IsConnectN(4)); // expect false
            state = state.ApplyAction(new GameAction(3, GameSide.SideB));
            Assert.True(state.IsConnectN(4)); // expect true
        }

        [Fact]
        public void Base64HashTest()
        {
            // convert a game state to the corresponding Base64 hash
            var state = GameStateFactory.CreateState(8, 9);
            string hash = GameStateHashFactory.ToBase64Hash(state);
            _logger.WriteLine($"original state:\n{ state }");
            _logger.WriteLine($"Base64 hash: { hash }");
            Assert.Equal("CAkAAAAAAAAAAAAAAAAAAAAAAAA=", hash);

            // convert the Base64 hash back to the original game state
            var newState = GameStateHashFactory.FromBase64Hash(hash);
            _logger.WriteLine($"parsed state:\n{ newState }");
            Assert.Equal(state, newState);

            var random = new Random();

            // play 1000 random games and convert each game state
            for (int i = 0; i < 1000; i++)
            {
                // reset the game state to empty board
                state = GameStateFactory.CreateState(8, 9);

                // play until the game is over
                while (state.IsConnectN(4) || state.ColumnStoneSums.Sum() >= state.MaxRows * state.MaxColumns)
                {
                    // choose a random action from all possible actions
                    var actions = state.GetPossibleActions();
                    var selectedAction = actions.ElementAt(random.Next(actions.Count()));

                    // perform the chosen action
                    state = state.ApplyAction(selectedAction);

                    // test Base64 hash serialization for the resulting state
                    hash = GameStateHashFactory.ToBase64Hash(state);
                    newState = GameStateHashFactory.FromBase64Hash(hash);
                    Assert.Equal(state, newState);
                }
            }
        }
    }
}
