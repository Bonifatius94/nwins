using nWins.Lib.Factory;
using nWins.Lib.Agent;
using System;
using System.Collections.Generic;
using Xunit;
using Xunit.Abstractions;

namespace nWins.Lib.Test
{
    public class ExplorationStrategyFactoryTest
    {
        //initialize logging tools
        private readonly ITestOutputHelper _logger;
        public ExplorationStrategyFactoryTest(ITestOutputHelper logger) { _logger = logger;}

        [Fact]
        public void CreateStrategyTest()
        {
            Dictionary<string, object> testParams = new Dictionary<string, object>();

            //Add correct keys and available values to create epsilon greedy linear decay
            testParams.Add("exp_strategy", "epsilon_greedy_lindecay");
            testParams.Add("start_epsilon", 0.1);
            testParams.Add("min_epsilon", 0.01);
            testParams.Add("decay_episodes", (long) 100);

            //Create Strategy with correct Dictionary 
            IExplorationStrategy strategy = ExplorationStrategyFactory.CreateStrategy(testParams);
            
            //Check if strategy is a LinearDecayingEpsionGreedy strategy
            Assert.True(strategy is LinearDecayingEpsilonGreedy);

            //Reset Dictionary
            testParams = new Dictionary<string, object>();

            //Add incorrect strategy key to dictionary
            testParams.Add("strategy", "epsilon_greedy_lindecay");

            //Check if false argument is recognized
            try{ ExplorationStrategyFactory.CreateStrategy(testParams); Assert.True(false);} catch(ArgumentException) {Assert.True(true);}

            //Reset Dictionary
            testParams = new Dictionary<string, object>();

            //Add correct keys and available values to create epsilon greedy linear decay
            testParams.Add("exp_strategy", "semi_greedy_inference");
            testParams.Add("semi_greedy_epsilon", 0.1);

            //Create Strategy with correct Dictionary 
            strategy = ExplorationStrategyFactory.CreateStrategy(testParams);
            
            //Check if strategy is a LinearDecayingEpsionGreedy strategy
            Assert.True(strategy is SemiGreedyInferenceStrategy);


            //Reset Dictionary
            testParams = new Dictionary<string, object>();

            //Add unknown exp_strategy value to dictionary
            testParams.Add("exp_strategy", "epsilon_greedy_expdecay");

            //Check if unknown exp_strategy is recognized
            try{ ExplorationStrategyFactory.CreateStrategy(testParams); Assert.True(false);} catch(NotSupportedException) {Assert.True(true);}
        }

        [Fact]
        public void createLinDecayEpsilonGreedyStrategyTest()
        {
            Dictionary<string, object> testParams = new Dictionary<string, object>();

            //Add correct keys and available values to create epsilon greedy linear decay
            testParams.Add("exp_strategy", "epsilon_greedy_lindecay");
            testParams.Add("start_epsilon", 0.1);
            testParams.Add("min_epsilon", 0.01);
            testParams.Add("decay_episodes", (long) 100);

            //Create epsilon greedy linear decay with dictionary 
            IExplorationStrategy strategy = ExplorationStrategyFactory.CreateStrategy(testParams);
            
            //Check if strategy is a LinearDecayingEpsionGreedy strategy
            Assert.True(strategy is LinearDecayingEpsilonGreedy);

            //Reset Dictionary
            testParams = new Dictionary<string, object>();

            //Add keys and available values to create epsilon greedy linear decay
            //but decay_episodes is missing
            testParams.Add("exp_strategy", "epsilon_greedy_lindecay");
            testParams.Add("start_epsilon", 0.1);
            testParams.Add("min_epsilon", 0.01);

            //Check if missing argument is recognized
            try{ ExplorationStrategyFactory.CreateStrategy(testParams); Assert.True(false);} catch(ArgumentException) {Assert.True(true);}

            //Reset Dictionary
            testParams = new Dictionary<string, object>();

            //Add keys and available values to create epsilon greedy linear decay
            //but min_epsilon is missing
            testParams.Add("exp_strategy", "epsilon_greedy_lindecay");
            testParams.Add("start_epsilon", 0.1);
            testParams.Add("decay_episodes", (long) 100);

            //Check if missing argument is recognized
            try{ ExplorationStrategyFactory.CreateStrategy(testParams); Assert.True(false);} catch(ArgumentException) {Assert.True(true);}

            //Reset Dictionary
            testParams = new Dictionary<string, object>();

            //Add keys and available values to create epsilon greedy linear decay
            //but start_epsilon is missing
            testParams.Add("exp_strategy", "epsilon_greedy_lindecay");
            testParams.Add("min_epsilon", 0.01);
            testParams.Add("decay_episodes", (long) 100);

            //Check if missing argument is recognized
            try{ ExplorationStrategyFactory.CreateStrategy(testParams); Assert.True(false);} catch(ArgumentException) {Assert.True(true);}
        }
    }
}