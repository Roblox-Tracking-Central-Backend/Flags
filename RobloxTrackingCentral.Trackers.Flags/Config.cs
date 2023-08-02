using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace RobloxTrackingCentral.Trackers.Flags
{
    internal class Config
    {
        public static Config Default { get; }

        public int Workers { get; set; } = 5;
        public string[] ClientApplications { get; set; } = null!;
        public string[] BootstrapperApplications { get; set; } = null!;
        public string[] OtherApplications { get; set; } = null!;

        public string[] AllApplications { get { return ClientApplications.Concat(BootstrapperApplications).Concat(OtherApplications).ToArray(); } }

        static Config()
        {
            string configJsonStr;

            if (!File.Exists("Config.json"))
            {
                Console.WriteLine("Fetching config from " + Constants.Backend);
                configJsonStr = Http.Client.GetStringRetry("https://raw.githubusercontent.com/" + Constants.Backend + "/main/Config.json").Result;
            }
            else
            {
                Console.WriteLine("Fetching config locally");
                configJsonStr = File.ReadAllText("Config.json");
            }

            Default = JsonSerializer.Deserialize<Config>(configJsonStr)!;
        }
    }
}
