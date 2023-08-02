using LibGit2Sharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace RobloxTrackingCentral.Trackers.Flags
{
    internal class FlagWorker
    {
        private Repository _Repository;
        private Dictionary<string, Dictionary<string, string>> _Applications;

        private Dictionary<string, Dictionary<string, string>> _FilteredApplications;

        public List<string> Changes { get; }

        public FlagWorker(Repository repository, Dictionary<string, Dictionary<string, string>> applications)
        {
            _Repository = repository;
            _Applications = applications;

            _FilteredApplications = new();

            Changes = new List<string>();
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

        private void CheckForChanges()
        {
            foreach (var application in _FilteredApplications)
            {
                string applicationName = application.Key;
                var flags = application.Value;

                string newContents = JsonSerializer.Serialize(flags, new JsonSerializerOptions { WriteIndented = true });

                if (!AnyChanges(applicationName, newContents))
                    return;

                Console.WriteLine($"[{applicationName}] Changes found");

                CommitChanges(applicationName, newContents);
            }
        }

        private void CheckShareSameFlipTime()
        {
            string? flipTime = null;

            foreach (var application in _Applications)
            {
                var flags = application.Value;

                if (!flags.ContainsKey("FStringFlagRepoGitHashFastString"))
                    continue;

                if (flipTime == null)
                    flipTime = flags["FStringFlagRepoGitHashFastString"];

                if (flipTime != flags["FStringFlagRepoGitHashFastString"])
                    throw new Exception("Flip times do not match up, got flags as they were changing!"); // TODO: make it redo instead of dying
            }
        }

        private Dictionary<string, string> FindCommon(string[] applicationNames)
        {
            IEnumerable<KeyValuePair<string, string>>? common = null;

            foreach (string applicationName in applicationNames)
            {
                var flags = _Applications[applicationName];

                if (common == null)
                    common = flags;
                else
                    common = common.Intersect(flags);
            }

            var commonDict = common!.ToDictionary(p => p.Key, p => p.Value);
            return commonDict;
        }

        private void FindCommons()
        {
            _FilteredApplications["CommonClient"] = FindCommon(Config.Default.ClientApplications);
            _FilteredApplications["CommonBootstrapper"] = FindCommon(Config.Default.BootstrapperApplications);
        }

        private void FilterCommon(string commonKey, string[] applicationNames)
        {
            var common = _FilteredApplications[commonKey];

            foreach (string applicationName in applicationNames)
            {
                var flags = _Applications[applicationName];

                var unique = flags.Except(common);

                var uniqueDict = unique.ToDictionary(p => p.Key, p => p.Value);

                _FilteredApplications[applicationName] = uniqueDict;
            }
        }

        private void FilterCommons()
        {
            FilterCommon("CommonClient", Config.Default.ClientApplications);
            FilterCommon("CommonBootstrapper", Config.Default.BootstrapperApplications);
        }

        private void AddOtherApplications()
        {
            foreach (string applicationName in Config.Default.OtherApplications)
            {
                var flags = _Applications[applicationName];

                _FilteredApplications[applicationName] = flags;
            }
        }

        // TODO: we should do this when adding
        private void FilterBadFlags()
        {
            foreach (var flags in _FilteredApplications.Values)
            {
                flags.Remove("FStringFlagRepoGitHashFastString");
                flags.Remove("DFStringFlagRepoGitHashDynamicString");
                flags.Remove("FStringFlipTimeStampFastString");
                flags.Remove("DFStringFlipTimeStampDynamicString");
            }
        }

        public void Start()
        {
            CheckShareSameFlipTime();

            FindCommons();

            FilterCommons();

            AddOtherApplications();

            FilterBadFlags();

            CheckForChanges();
        }
    }
}
