using System;
using System.IO;
using System.Runtime.Loader;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using nWins.Lib.Agent;
using nWins.Lib.Core;
using nWins.Lib.Session;
using nWins.Lib.Settings;

namespace nWins.Training
{
    public class Program
    {
        public static void Main(string[] args)
        {
            // parse the training settings
            var settings = parseSettings(args);

            // create a training session with the loaded settings
            var session = new TrainingSession(settings);

            Console.WriteLine("Starting training");

            // initialize task cancellation synchronization
            var mutex = new Mutex(false);
            var cancellationSource = new CancellationTokenSource();

            // create a cancellable training task
            var trainingTask = Task.Run(() => {
                mutex.WaitOne();
                session.TrainAsync(cancellationSource.Token).Wait();
                mutex.ReleaseMutex();
            });

            // create cancellation handler
            Action<string> onCancel = (string source) => {

                Console.WriteLine($"Requested shutdown from { source }. Trying to exit gracefully ...");
                cancellationSource.Cancel();
                mutex.WaitOne();
            };

            // attach the cancellation handler to various exit events that may occur
            AppDomain.CurrentDomain.ProcessExit += (object sender, EventArgs e) => onCancel("ProcessExit");
            Console.CancelKeyPress += (object sender, ConsoleCancelEventArgs args) => onCancel("Ctrl+C");
            // AssemblyLoadContext.Default.Unloading += (AssemblyLoadContext obj) => onCancel("Unloading");

            trainingTask.Wait();
            Console.WriteLine("Exited training gracefully!");
        }

        private static ITrainingSettings parseSettings(string[] args)
        {
            if (args.Length != 2) { throw new ArgumentException("Invalid arguments! Exactly 2 arguments expected!"); }
            if (!args[0].Equals("--settings")) { throw new ArgumentException("Invalid arguments! Settings parameter is not specified!"); }

            // retrieve the settings file either as absolute path or as path relating to $SETTINGS_ROOT env variable
            string settingsFilePath = Path.IsPathFullyQualified(args[1]) ? args[1]
                : Path.Combine(EnvironmentSettings.Instance.SettingsRootDir, args[1]);

            // make sure that the settings file exists
            if (!File.Exists(settingsFilePath)) { throw new ArgumentException($"Invalid arguments! The settings file '{ settingsFilePath }' does not exist!"); }

            // parse the training settings from JSON file
            string json;
            using (var reader = new StreamReader(settingsFilePath)) { json = reader.ReadToEnd(); }
            var settings = JsonConvert.DeserializeObject<TrainingSettings>(json);

            return settings;
        }
    }
}
