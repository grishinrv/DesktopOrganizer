using System.Security.AccessControl;
using System.Security.Principal;
using System.Text.Json;
using System.Runtime.InteropServices;

namespace Shared
{
    public sealed class ProjectsService
    {
        private const string NAMED_MUTEX_PROJECT_FILES = "Global\\BackupManagerProjectFilesAccessMutex";
        private const string PROJECT_FILES_FILE_NAME = "projectFiles.json";
        bool _isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
        private readonly MutexSecurity? securitySettings;

        public ProjectsService()
        {
            if (_isWindows)
            {
#pragma warning disable CA1416 // Validate platform compatibility
                var allowEveryoneRule =
                    new MutexAccessRule(
                        new SecurityIdentifier(WellKnownSidType.WorldSid, null),
                        MutexRights.FullControl,
                        AccessControlType.Allow);
                securitySettings = new MutexSecurity();
                securitySettings.AddAccessRule(allowEveryoneRule);
#pragma warning restore CA1416 // Validate platform compatibility
            }
        }

        private Mutex CreateMutex()
        {
            if (_isWindows)
            {
#pragma warning disable CA1416 // Validate platform compatibility
                return MutexAcl.Create(false, NAMED_MUTEX_PROJECT_FILES, out bool createdNew, securitySettings!);
#pragma warning restore CA1416 // Validate platform compatibility
            }
            else
            {
                return new Mutex(false, NAMED_MUTEX_PROJECT_FILES);
            }
        }

        public async Task<ProjectFiles> Read()
        {
            using (var mutex = CreateMutex())
            {
                while (!mutex.WaitOne(TimeSpan.FromMilliseconds(300), false))
                {
                    Console.WriteLine("Another instance is running.");
                }

                try
                {
                    var jsonString = await File.ReadAllTextAsync(PROJECT_FILES_FILE_NAME);

                    var  result = JsonSerializer.Deserialize<ProjectFiles>(jsonString);
                    return result!;
                }
                finally
                {
                    mutex.ReleaseMutex();
                }
            }
        }

        public async Task Save(ProjectFiles model)
        {
            using (var mutex = CreateMutex())
            {
                while (!mutex.WaitOne(TimeSpan.FromMilliseconds(300), false))
                {
                    Console.WriteLine("Another instance is running.");
                }

                try
                {
                    var jsonString = JsonSerializer.Serialize<ProjectFiles>(model);
                    await File.WriteAllTextAsync(PROJECT_FILES_FILE_NAME, jsonString);
                }
                finally
                {
                    mutex.ReleaseMutex();
                }
            }
        }

    }
}
