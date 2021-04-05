using System;
using Xunit;
using nWins.Lib.Agent;
using nWins.Lib.Core;
using nWins.Lib.Factory;
using Xunit.Abstractions;

namespace nWins.Lib.Test
{
    public class AgentsTest
    {
        // initialize logging tools
        private readonly ITestOutputHelper _logger;
        public AgentsTest(ITestOutputHelper logger) { _logger = logger; }

        [Fact]
        public void RandomAgentTest()
        {
            // initialize the game board and a random agent
            var state = GameStateFactory.CreateState(6, 7);
            var agent = new RandomAgent();
            testAgent(agent);
        }

        private void testAgent(IAgent agent)
        {
            // initialize a blank game board and make the agent choose an action
            var state = GameStateFactory.CreateState(6, 7);
            var action = agent.ChooseAction(state, null);

            // check if the chosen action can be applied to the blank game board
            try { var newState = state.ApplyAction(action); }
            catch (Exception) { Assert.True(false); }

            // add some more stones to entirely fill up one column
            // -> check if the agent ignores the occupied column
            for (uint column = 0; column < state.MaxColumns; column++)
            {
                state = GameStateFactory.CreateState(6, 7);
                state = state.ApplyAction(new GameAction(column, GameSide.SideA));
                state = state.ApplyAction(new GameAction(column, GameSide.SideB));
                state = state.ApplyAction(new GameAction(column, GameSide.SideA));
                state = state.ApplyAction(new GameAction(column, GameSide.SideB));
                state = state.ApplyAction(new GameAction(column, GameSide.SideA));

                var lastAction = new GameAction(column, GameSide.SideB);
                state = state.ApplyAction(lastAction);

                action = agent.ChooseAction(state, lastAction);
                _logger.WriteLine($"chosen action: { action.ToString() }");

                Assert.True(action.ColumnIndex != column);
            }
        }
    }
}
