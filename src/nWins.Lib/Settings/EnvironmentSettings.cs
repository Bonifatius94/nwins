
using System;

namespace nWins.Lib.Settings
{
    /// <summary>
    /// Representing a lookup for specific environment settings (access via singleton pattern).
    /// </summary>
    public class EnvironmentSettings
    {
        /// <summary>
        /// The models root directory storing the agent's training results.
        /// </summary>
        public string ModelRootDir => Environment.ExpandEnvironmentVariables("%MODELS_ROOT%");

        /// <summary>
        /// The settings root directory containing training configurations, etc.
        /// </summary>
        public string SettingsRootDir => Environment.ExpandEnvironmentVariables("%SETTINGS_ROOT%");

        /// <summary>
        /// The logs root directory containing training logs and other textual files.
        /// </summary>
        public string LogsRootDir => Environment.ExpandEnvironmentVariables("%LOGS_ROOT%");

        /// <summary>
        /// The source code root directory where the application's code base is located.
        /// </summary>
        public string SourceCodeRootDir => Environment.ExpandEnvironmentVariables("%SRC_ROOT%");
    }
}