﻿using LibGit2Sharp;
using System.Reflection;
using System.Text.Json;

namespace RobloxTrackingCentral.Trackers.Flags
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Roblox Tracking Central");
            Console.WriteLine("Flags Tracker");
            Console.WriteLine($"Version {Assembly.GetExecutingAssembly().GetName().Version}");

            string? personalToken = Environment.GetEnvironmentVariable("RTC_FLAGS_TOKEN");
            string? authUsername = Environment.GetEnvironmentVariable("RTC_FLAGS_USER");

            if (string.IsNullOrEmpty(personalToken))
            {
                Console.WriteLine("Environment variable RTC_FLAGS_TOKEN are missing or empty");
                return;
            }

            if (string.IsNullOrEmpty(authUsername))
            {
                Console.WriteLine("Environment variable RTC_FLAGS_USER are missing or empty");
                return;
            }

            // TODO: sync w/ latest instead of deleting
            if (Directory.Exists(Constants.ClonePath))
            {
                Console.WriteLine("Deleting current clone directory");
                DirectoryHelper.ForceDelete(Constants.ClonePath);
            }

            Console.WriteLine("Cloning " + Constants.Tracker);
            string gitPath = Repository.Clone("https://github.com/" + Constants.Tracker + ".git", Constants.ClonePath);
            Repository repository = new Repository(gitPath);

            Console.WriteLine("Fetching application names from " + Constants.Backend);

            Queue<string> applicationQueue = new Queue<string>();
            foreach (string application in Config.Default.AllApplications)
                applicationQueue.Enqueue(application);

            Console.WriteLine($"Got {applicationQueue.Count} applications");
            Console.WriteLine($"Using {Config.Default.Workers} workers");

            Console.WriteLine("Starting" + nameof(GetWorkerFactory));
            GetWorkerFactory factory = new GetWorkerFactory(applicationQueue);

            List<Task> workers = new List<Task>();

            for (int i = 1; i <= Config.Default.Workers; i++)
                workers.Add(factory.Create());

            Task.WaitAll(workers.ToArray());
            Console.WriteLine("Workers have finished");

            Console.WriteLine("Starting " + nameof(FlagWorker));
            FlagWorker flagWorker = new FlagWorker(repository, factory.Applications);
            flagWorker.Start();

            List<string> changes = flagWorker.Changes;
            changes.Sort();

            Console.WriteLine("Flag worker has finished");

            string changesJoined = string.Join(", ", changes);

#if RELEASE
            Console.WriteLine("Committing changes");

            try
            {
                var time = DateTimeOffset.Now;
                var signature = new Signature("Roblox Tracking Central", "rtc@rtc.local", time);
                var commit = repository.Commit($"{time.ToString("dd/MM/yyyy HH:mm:ss")} [{changesJoined}]", signature, signature);
                Console.WriteLine("Committing!");

                var remote = repository.Network.Remotes["origin"];
                var options = new PushOptions
                {
                    CredentialsProvider = (_url, _user, _cred) => new UsernamePasswordCredentials { Username = authUsername, Password = personalToken }
                };
                var pushRefSpec = "refs/heads/main";
                repository.Network.Push(remote, pushRefSpec, options);
            }
            catch (EmptyCommitException) // any better way?
            {
                Console.WriteLine("No changes found");
            }
#else
            Console.WriteLine("Committing is disabled for debug builds");
            Console.WriteLine("Changes:");
            Console.WriteLine(changesJoined);
#endif
        }
    }
}