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
        public string[] ApplicationNames { get; set; } = null!;

        static Config()
        {
            Console.WriteLine("Fetching config from " + Constants.Backend);
            string configJsonStr = Http.Client.GetStringRetry("https://raw.githubusercontent.com/" + Constants.Backend + "/main/Config.json").Result;
            Default = JsonSerializer.Deserialize<Config>(configJsonStr)!;
        }
    }
}
