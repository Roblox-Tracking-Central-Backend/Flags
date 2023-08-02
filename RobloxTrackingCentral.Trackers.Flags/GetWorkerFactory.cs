using LibGit2Sharp;
using RobloxTrackingCentral.Trackers.Flags.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace RobloxTrackingCentral.Trackers.Flags
{
    internal class GetWorkerFactory
    {
        private const string BaseUrl = "roblox.com";

        private Queue<string> _ApplicationNames;

        public Dictionary<string, Dictionary<string, string>> Applications { get; }

        public GetWorkerFactory(Queue<string> applicationNames)
        {
            _ApplicationNames = applicationNames;

            Applications = new();
        }

        private async Task<Dictionary<string, string>> GetClientSettings(string application)
        {
            string url = "https://clientsettingscdn." + BaseUrl + "/v2/settings/application/" + application;

            string settingsStr = await Http.Client.GetStringRetry(url);

            var settings = JsonSerializer.Deserialize<ApplicationSettings>(settingsStr)!;

            return settings.Settings;
        }

        public async Task Create()
        {
            while (_ApplicationNames.TryDequeue(out string? application))
            {
                if (application == null)
                    continue;

                Console.WriteLine($"[{application}] Fetching flags");

                var flags = await GetClientSettings(application);

                Applications.Add(application, flags);
            }
        }
    }
}
