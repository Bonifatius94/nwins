
using System;
using System.IO;
using nWins.Lib.Agent;
using nWins.Lib.Core;
using nWins.Lib.Settings;

namespace nWins.Lib.Factory
{
    /// <summary>
    /// A factory for serializing agents from settings, etc.
    /// </summary>
    public static class AgentFactory
    {
        /// <summary>
        /// Create a new trainable agent instance using the given settings.
        /// </summary>
        /// <param name="settings">The trainable agent's settings.</param>
        /// <param name="side">The side the agent is acting for.</param>
        /// <returns>a trainable agent</returns>
        public static ITrainableAgent CreateAgent(ITrainableAgentSettings settings, GameSide side)
        {
            ITrainableAgent agent;

            switch (settings.AgentType)
            {
                case AgentType.Random: agent = new RandomAgent(); break;
                case AgentType.SimpleQL: agent = new SimpleQLAgent(settings, side); break;
                case AgentType.DoubleQL: agent = new DoubleQLAgent(settings, side); break;
                case AgentType.DynaQL: agent = new DynaQLAgent(settings, side); break;
                case AgentType.SarsaLambda: agent = new SarsaLambdaAgent(settings, side); break;
                default:
                    throw new ArgumentException($"Invalid argumemnts! Unknow agent type { settings.AgentType } cannot be serialized!");
            }

            // try to load the startup model defined in settings
            if (File.Exists(settings.StartupModel)) { agent.LoadModel(settings.StartupModel); }

            return agent;
        }
    }
}