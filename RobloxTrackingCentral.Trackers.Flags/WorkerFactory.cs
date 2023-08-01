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
    internal class WorkerFactory
    {
        private const string BaseUrl = "roblox.com";

        private Repository _Repository;
        private Queue<string> _ApplicationNames;

        public List<string> Changes { get; }

        public WorkerFactory(Repository repository, Queue<string> applicationNames)
        {
            _Repository = repository;
            _ApplicationNames = applicationNames;

            Changes = new List<string>();
        }

        private async Task<Dictionary<string, string>> GetClientSettings(string application)
        {
            string url = "https://clientsettingscdn." + BaseUrl + "/v2/settings/application/" + application;

            string settingsStr = await Http.Client.GetStringRetry(url);

            var settings = JsonSerializer.Deserialize<ApplicationSettings>(settingsStr)!;

            return settings.Settings;
        }

        private void FilterFlags(Dictionary<string, string> flags)
        {
            flags.Remove("FStringFlagRepoGitHashFastString");
            flags.Remove("DFStringFlagRepoGitHashDynamicString");
            flags.Remove("FStringFlipTimeStampFastString");
            flags.Remove("DFStringFlipTimeStampDynamicString");
        }

        private string ConstructPath(string application, bool git)
        {
            if (!git)
                return Path.Combine(application + ".json");
            else
                return Path.Combine(Constants.ClonePath, application + ".json");
        }

        private bool AnyChanges(string file, string newContents)
        {
            string clonePath = ConstructPath(file, true);
            if (!File.Exists(clonePath))
                return true;

            string contents = File.ReadAllText(clonePath);

            return contents != newContents;
        }

        private void CommitChanges(string application, string newContents)
        {
            Changes.Add(application);

            string clonePath = ConstructPath(application, true);
            File.WriteAllText(clonePath, newContents);

            string gitPath = ConstructPath(application, false);
            _Repository.Index.Add(gitPath);
        }

        private void CheckForChanges(string application, Dictionary<string, string> flags)
        {
            string newContents = JsonSerializer.Serialize(flags, new JsonSerializerOptions { WriteIndented = true });

            if (!AnyChanges(application, newContents))
                return;

            Console.WriteLine($"[{application}] Changes found");

            CommitChanges(application, newContents);
        }

        public async Task Create()
        {
            while (_ApplicationNames.TryDequeue(out string? application))
            {
                if (application == null)
                    continue;

                Console.WriteLine($"[{application}] Fetching flags");

                var flags = await GetClientSettings(application);

                FilterFlags(flags);

                CheckForChanges(application, flags);
            }
        }
    }
}
