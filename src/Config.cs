namespace SHRestAPI
{

    using System;
    using System.Collections.Generic;
    using System.IO;
    using Newtonsoft.Json;

    [JsonObject(MemberSerialization.OptIn)]
    class Config
    {
        public static Config Instance { get; set; }

        [JsonProperty("enabled")]
        public bool Enabled { get; set; }

        [JsonProperty("port")]
        public int Port { get; set; }

        public Config()
        {
            this.Enabled = true;
            this.Port = 8081;
        }

        public void Validate()
        {
            if (this.Port <= 0)
            {
                throw new Exception("Invalid Port.");
            }
        }

        public static void LoadConfig()
        {
            var configPath = SHRestServer.ConfigPath;
            Logging.LogTrace("Loading config at: " + configPath);

            string configText;
            try
            {
                configText = File.ReadAllText(configPath);
            }
            catch (FileNotFoundException)
            {
                Logging.LogInfo("No config file present.");
                Instance = new Config();
                return;
            }

            try
            {
                Config.Instance = JsonConvert.DeserializeObject<Config>(configText);
                Logging.LogTrace("Config loaded successfully.");
            }
            catch (Exception e)
            {
                Logging.LogError(
                    new Dictionary<string, string>() {
                        {"ConfigPath", configPath}
                    },
                    "Failed to load config file: " + e.Message
                );
                Instance = new Config()
                {
                    Enabled = false
                };
                return;
            }

            try
            {
                Instance.Validate();
            }
            catch (Exception e)
            {
                Logging.LogError(
                new Dictionary<string, string>() {
                    {"ConfigPath", configPath}
                },
                "Invalid configuration: " + e.Message
                );
                Instance.Enabled = false;
            }
        }
    }
}