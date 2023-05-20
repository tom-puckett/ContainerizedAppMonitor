using System;
using System.Timers;

namespace ContainerizedAppMonitor
{
    public class MountedShares : IHostedService
    {
        static int counter = 0;
        static string? MAP_DATA_MOUNT_PATH = Environment.GetEnvironmentVariable("MAP_DATA_MOUNT_PATH");
        static string? MAP_DATA_MOUNTS_JSON = Environment.GetEnvironmentVariable("MAP_DATA_MOUNTS_JSON");
        static List<string> sharePaths = 
            !string.IsNullOrWhiteSpace(MAP_DATA_MOUNTS_JSON)
            ? MAP_DATA_MOUNTS_JSON.Split(';').ToList()
            : MAP_DATA_MOUNT_PATH is not null
              ? new() { MAP_DATA_MOUNT_PATH }
              : new();

        public Task StartAsync(CancellationToken token)
        {
            System.Threading.Timer timer = new System.Threading.Timer(ProcessAllMountedShares, null, TimeSpan.Zero, TimeSpan.FromSeconds(10));

            return Task.CompletedTask;
        }

        private void ProcessAllMountedShares(object? state)
        {
            string message = string.Empty;
            Console.WriteLine($"**************** Processing shares, counter is {counter++}");

            foreach (string sharePath in sharePaths)
            {
                DirectoryInfo? dirInfo = default;
                try
                {
                    dirInfo = Directory.CreateDirectory(sharePath);
                }
                catch (DirectoryNotFoundException)
                {
                    Console.WriteLine($"Share root directory <{sharePath}> not found when trying to examine contents");
                    continue;
                }

                MountedShares worker = new MountedShares();

                var dirContents = worker.GetDirContents(dirInfo.FullName);
                dirContents.ForEach(e => message += $"{e}{Environment.NewLine}");

                Console.WriteLine($"Share <{sharePath}> found with the following contents{message}{Environment.NewLine}");
            }
        }

        public Task StopAsync(CancellationToken token) => Task.CompletedTask;

        public List<string> GetDirContents(string contentsOf, bool recursive = true)
        {
            List<string> returnVal = new() { contentsOf + Path.DirectorySeparatorChar };

            var files = Directory.GetFiles(contentsOf).ToList();
            files.ForEach(f => returnVal.Add(f));

            var subdirectories = Directory.GetDirectories(contentsOf).ToList();
            subdirectories.ForEach(d =>
            {
                if (recursive)
                {
                    returnVal.AddRange(GetDirContents(d));
                }
            }
            );

            return returnVal;
        }

    }
}
