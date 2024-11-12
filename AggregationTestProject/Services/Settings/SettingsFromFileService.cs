using AggregationTestProject.Utilities.Interfaces;
using log4net;
using Newtonsoft.Json;
using System.IO;

namespace AggregationTestProject.Services.Settings
{
    public class SettingsFromFileService<T> : ISettingsService<T>
        where T : class
    {
        private string _configPath;

        private ILog _log;

        public T GetSettings()
        {
            try
            {
                if (!File.Exists(_configPath))
                {
                    throw new FileNotFoundException("Config file not found");
                }

                var file = File.ReadAllText(_configPath);

                if (file is null || file.Length == 0)
                {
                    throw new ArgumentNullException("Config file is empty");
                }

                T settings = JsonConvert.DeserializeObject<T>(file)!;

                return settings;
            }
            catch (FileNotFoundException ex)
            {
                _log.Fatal("Settings service: can't find config file", ex);
                throw;
            }
            catch (ArgumentNullException ex)
            {
                _log.Fatal("Settings service: config file was null", ex);
                throw;
            }
            catch (Exception ex)
            {
                _log.Debug("Settings service: unknown error", ex);
                throw;
            }
        }

        public void SetSettings(T settings)
        {
            try
            {
                var settingsFile = JsonConvert.SerializeObject(settings);

                File.WriteAllText(_configPath, settingsFile);
            }
            catch (Exception ex)
            {
                _log.Debug("Settings service: unknown error");
                throw;
            }
        }

        public SettingsFromFileService(ILog log)
        {
            _configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "appsettings.json");

            _log = log;
        }
    }
}
