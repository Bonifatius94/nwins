using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using nWins.Lib.Agent;
using nWins.Lib.Core;
using nWins.Lib.Factory;
using nWins.Lib.Settings;

namespace nWins.Lib.Session;

/// <summary>
/// An interface for training session implementations.
/// </summary>
public interface ITrainingSession
{
    /// <summary>
    /// The training settings (readonly).
    /// </summary>
    ITrainingSettings Settings { get; }

    /// <summary>
    /// The agent A (readonly).
    /// </summary>
    ITrainableAgent AgentA { get; }

    /// <summary>
    /// The agent B (readonly).
    /// </summary>
    ITrainableAgent AgentB { get; }

    /// <summary>
    /// Initialize the training session with the given settings.
    /// </summary>
    /// <param name="settings">The settings to be applied.</param>
    void LoadFromSettings(ITrainingSettings settings);

    /// <summary>
    /// Run the training asynchronously, so it can be cancelled.
    /// </summary>
    /// <param name="token">The token used for graceful training shutdown.</param>
    /// <returns>an async task handle</returns>
    Task TrainAsync(CancellationToken token);
}

/// <summary>
/// Representing a common training session that can be loaded from settings 
/// and run asynchronously in the background.
/// </summary>
public class TrainingSession : ITrainingSession
{
    /// <summary>
    /// Create a training session with the given settings.
    /// </summary>
    /// <param name="settings">The settings to be applied.</param>
    public TrainingSession(ITrainingSettings settings)
    {
        envSettings = new EnvironmentSettings();
        LoadFromSettings(settings);
    }

    private readonly EnvironmentSettings envSettings;

    public ITrainingSettings Settings {get; private set;}
    public ITrainableAgent AgentA { get; private set;}
    public ITrainableAgent AgentB { get; private set;}

    private static readonly Random _random = new Random();

    public void LoadFromSettings(ITrainingSettings settings)
    {
        // apply new settings
        Settings = settings;

        // initialize agents from settings
        AgentA = AgentFactory.CreateAgent(settings.ConfigAgentA, GameSide.SideA);
        AgentB = AgentFactory.CreateAgent(settings.ConfigAgentB, GameSide.SideB);
    }

    public async Task TrainAsync(CancellationToken token)
    {
        // make sure any of the agents (or even both agents) are trainable
        //if (!Settings.ConfigAgentA.IsTrainable && !Settings.ConfigAgentB.IsTrainable) { 
        //    throw new InvalidOperationException("Cannot train when both agents are untrainable!"); }

        int episode = 0;
        bool training = true;

        // exit training loop gracefully on cancellation
        token.Register(() => {
            training = false;
            // store training results on exit
            Console.WriteLine("exiting training session, storing final models ...");
            if (Settings.ConfigAgentA.IsTrainable) { File.Move(Settings.ConfigAgentA.GetModelPath(0), Settings.ConfigAgentA.GetModelPath(episode)); }
            if (Settings.ConfigAgentB.IsTrainable) { File.Move(Settings.ConfigAgentB.GetModelPath(0), Settings.ConfigAgentB.GetModelPath(episode)); }
            Console.WriteLine("Stored models to file");
        });

        // launch training loop asynchronously
        await Task.Run(() => {

            // create a new game session
            var session = new GameSession(AgentA, AgentB, Settings.GameConfig);

            // continue until graceful shutdown
            while (training)
            {
                // store the training results for both agents (in case they are trainable)
                writeModelToTempStorage();

                Console.WriteLine("stored model to temp files");
                Console.WriteLine($"starting training episode { episode } - { episode + Settings.TrainingInterval }");

                // switch agents to training mode
                AgentA.SetMode(AgentMode.Training);
                AgentB.SetMode(AgentMode.Training);

                // run all steps of the training epoch
                for (int step = 0; step < Settings.TrainingInterval && training; step++)
                {
                    // play one game (training mode)
                    var game = session.PlayGameToEnd();
                    session.ResetGame();

                    // run garbage collector to avoid memory leaks
                    if ((episode + step) % 100000 == 0) { GC.Collect(); }

                    // update training progress (rewrite line with percentage)
                    if (step % 1000 == 0) { Console.Write($"\rtraining progress: { Math.Round((double)step / Settings.TrainingInterval * 100) } %"); }
                }

                Console.WriteLine();
                episode += Settings.TrainingInterval;

                // init wins / ties counters
                int winsSideA = 0;
                int winsSideB = 0;
                int ties = 0;

                // run inference steps and compute win rates
                for (int step = 0; step < Settings.InferenceInterval && training; step++)
                {
                    // handle assignment of greedy and semi-greedy inference strategy
                    updateInferenceMode();

                    // play one game (inference mode)
                    var game = session.PlayGameToEnd();
                    session.ResetGame();

                    // update wins / ties counter
                    if (game.Result == GameResult.WinSideA) { winsSideA++; }
                    else if (game.Result == GameResult.WinSideB) { winsSideB++; }
                    else { ties++; }

                    // update inference progress (rewrite line with percentage)
                    if (step % 100 == 0) { Console.Write($"\rinference progress: { Math.Round((double)step / Settings.InferenceInterval * 100) } %"); }
                }

                // log inference results (win rate)
                Console.WriteLine();
                Console.WriteLine($"inference results: { Settings.InferenceInterval } games played, "
                    + $"side A wins { winsSideA }, side B wins { winsSideB }, ties { ties }");

                // write win rates to CSV file
                int trainSteps = episode % Math.Max(Settings.TrainingInterval, 1) == 0 ? Settings.TrainingInterval : episode % Settings.TrainingInterval;
                logWinRates(winsSideA, winsSideB, ties, Settings.InferenceInterval, trainSteps);
            }
        });
    }

    private void writeModelToTempStorage()
    {
        // store training results for agent A
        if (Settings.ConfigAgentA.IsTrainable)
        {
            string tempModelPath = Path.Combine(Path.GetDirectoryName(Settings.ConfigAgentA.GetModelPath(0)), "temp_a.csv");
            AgentA.StoreModel(tempModelPath);

            #if (NET5_0 || NETCOREAPP3_1 || NETCOREAPP3_0)
            File.Move(tempModelPath, Settings.ConfigAgentA.GetModelPath(0), true);
            // use fallback operation for unsupported .NET frameworks
            #else
            File.Delete(Settings.ConfigAgentA.GetModelPath(0));   
            File.Move(tempModelPath, Settings.ConfigAgentA.GetModelPath(0));  
            #endif
        }

        // store training results for agent B
        if (Settings.ConfigAgentB.IsTrainable)
        {
            string tempModelPath = Path.Combine(Path.GetDirectoryName(Settings.ConfigAgentB.GetModelPath(0)), "temp_b.csv");
            AgentB.StoreModel(tempModelPath);

            #if (NET5_0 || NETCOREAPP3_1 || NETCOREAPP3_0)
            File.Move(tempModelPath, Settings.ConfigAgentB.GetModelPath(0), true);
            // use fallback operation for unsupported .NET frameworks
            #else
            File.Delete(Settings.ConfigAgentB.GetModelPath(0));   
            File.Move(tempModelPath, Settings.ConfigAgentB.GetModelPath(0));  
            #endif
        }
    }

    private void updateInferenceMode()
    {
        // if only one agent is trainable: make the trainable agent greedy and the non-trainable agent semi-greedy
        // if both agents are trainable: assign the modes uniform-randomly, so the results are not affected by static semi-greedy assignments

        // case agent A trainable, agent B not trainable -> agent A 100% greedy, agent B semi-greedy
        if (Settings.ConfigAgentA.IsTrainable && !Settings.ConfigAgentB.IsTrainable)
        {
            AgentA.SetMode(AgentMode.Inference);
            AgentB.SetMode(AgentMode.SemiGreedy);
        }
        // case agent A trainable, agent B not trainable -> agent A 100% greedy, agent B semi-greedy
        else if (!Settings.ConfigAgentA.IsTrainable && Settings.ConfigAgentB.IsTrainable) {
            AgentA.SetMode(AgentMode.SemiGreedy);
            AgentB.SetMode(AgentMode.Inference);
        }
        // case both agents trainable -> assign 100% greedy and semi-greedy mode uniform-randomly
        else {
            bool isAgentAGreedy = (_random.Next() % 2 == 0);
            AgentA.SetMode(isAgentAGreedy ? AgentMode.Inference : AgentMode.SemiGreedy);
            AgentB.SetMode(isAgentAGreedy ? AgentMode.SemiGreedy : AgentMode.Inference);
        }
    }

    private void logWinRates(int winsSideA, int winsSideB, int ties, int logInterval, int trainInterval)
    {
        string agentsDirectoryName = Settings.ConfigAgentA.AgentName + "_vs_" + Settings.ConfigAgentB.AgentName;
        string agentsDirectoryPath = Path.Combine(envSettings.LogsRootDir, agentsDirectoryName);

        if (!Directory.Exists(agentsDirectoryPath)) {
            Directory.CreateDirectory(agentsDirectoryPath);
        }

        string logfilePath = Path.Combine(agentsDirectoryPath, "win_rates.csv");

        using (var writer = new StreamWriter(logfilePath, true))
        {
            double winRateA = (double)winsSideA / logInterval;
            double winRateB = (double)winsSideB / logInterval;
            double tieRate = (double)ties / logInterval;
            writer.WriteLine($"{ trainInterval },{ winRateA },{ winRateB },{ tieRate }");
        }
    }
}
