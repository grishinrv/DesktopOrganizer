using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DesktopOrganizer
{
    public class Engine : IDisposable
    {
        private readonly int _coolDown;
        private bool _disposed;
        private readonly string _from;
        private string _to;
        private readonly bool _logTrace;
        private DateTime _ignoredLastUpdateTime;
        private List<string> _fullNameIgnored;
        private List<string> _regExpIgnored;

        public Engine()
        {
            _coolDown = int.Parse(ConfigurationManager.AppSettings["CoolDown"]);
            _from = ConfigurationManager.AppSettings["From"];
            _to = ConfigurationManager.AppSettings["To"];
            _logTrace = bool.Parse(ConfigurationManager.AppSettings["LogTrace"]);
            Task.Run(() => DoWorkAsync());
        }

        private DateTime GetIgnoredUpdateTime()
        {
            return File.GetLastWriteTime(AppDomain.CurrentDomain.BaseDirectory + "ignore.txt");
        }

        public void Dispose()
        {
            _disposed = true;
        }

        private async Task DoWorkAsync()
        {
            while (!_disposed)
            {
                await Task.Delay(TimeSpan.FromSeconds(_coolDown));
                try
                {
                    InitIgnore();
                    MoveFiles();
                    MoveDirectories();
                }
                catch (Exception ex)
                {
                    Log(ex.Message + Environment.NewLine + ex.StackTrace);
                }
            }
        }

        private void MoveFiles()
        {
            var found = Directory.GetFiles(_from).Select(x => Path.GetFileName(x)).ToList();

            foreach (var item in found.Where(x => !_fullNameIgnored.Contains(x)
                && !_regExpIgnored.Exists(y => Regex.IsMatch(x, y))))
            {
                if (File.Exists(_to + item))
                    Log($"Файл {_to + item} уже существует, перенос невозможен", EventLogEntryType.Information);
                else if (!IsFileInUse(_from + item))
                {
                    Log($"Переносим из {_from + item} файл в {_to + item}", EventLogEntryType.Information);
                    File.Move(_from + item, _to + item);
                }
                else
                    Log($"Файл {_from + item} заблокирован другим процессом, перенос невозможен", EventLogEntryType.Information);
            }
        }

        private void MoveDirectories()
        {
            var found = Directory.GetDirectories(_from)
                .Select(x => x.Substring(x.LastIndexOf('\\') + 1, x.Length - x.LastIndexOf('\\') - 1 ))
                .ToList();

            foreach (var item in found.Where(x => !_fullNameIgnored.Contains(x)
                && !_regExpIgnored.Exists(y => Regex.IsMatch(x, y))))
            {
                if (Directory.Exists(_to + item))
                    Log($"Директория {_to + item} уже существует, перенос невозможен", EventLogEntryType.Information);
                else
                {
                    try
                    {
                        Log($"Переносим из {_from + item} директорию в {_to + item}", EventLogEntryType.Information);
                        //Now Create all of the directories
                        foreach (string dirPath in Directory.GetDirectories(_from, "*",
                            SearchOption.AllDirectories))
                            Directory.CreateDirectory(dirPath.Replace(_from, _to));

                        //Copy all the files & Replaces any files with the same name
                        foreach (string newPath in Directory.GetFiles(_from, "*.*",
                            SearchOption.AllDirectories))
                            File.Copy(newPath, newPath.Replace(_from, _to), true);
                    }
                    catch (Exception ex)
                    {
                        Log($"Не удалось перенести директорию: {ex.Message + Environment.NewLine + ex.StackTrace}");
                        continue;
                    }
                    try
                    {
                        Directory.Delete(_from + item, true);
                    }
                    catch (Exception ex)
                    {
                        Log($"Не удалось очистить директорию: {ex.Message + Environment.NewLine + ex.StackTrace}");
                    }
                }
            }
        }

        private void InitIgnore()
        {
            var lastUpdate = GetIgnoredUpdateTime();
            if (DateTime.Compare(_ignoredLastUpdateTime, lastUpdate) != 0)
            {
                GetIgnored(out _fullNameIgnored, out _regExpIgnored);
                _ignoredLastUpdateTime = lastUpdate;
            }
        }

        private void Log(string message, EventLogEntryType type = EventLogEntryType.Error)
        {
            if (type != EventLogEntryType.Error && !_logTrace)
                return;
            EventLog.WriteEntry("DesktopOrganizer", message, type);
        }

        private void GetIgnored(out List<string> fullNameIgnored, out List<string> regExpIgnored)
        {
            var ignore = File.ReadAllLines(AppDomain.CurrentDomain.BaseDirectory + "Ignore.txt");
            fullNameIgnored = ignore.Where(x => !x.StartsWith("regex_")).ToList();
            fullNameIgnored.Add("desktop.ini");
            fullNameIgnored.Add("Desktop");
            regExpIgnored = ignore.Where(x => x.StartsWith("regex_"))
                .Select(x => x.Remove(0, 6))
                .ToList();
        }

        private bool IsFileInUse(string filename)
        {
            bool locked = false;
            try
            {
                FileStream fs =
                    File.Open(filename, FileMode.OpenOrCreate,
                    FileAccess.ReadWrite, FileShare.None);
                fs.Close();
            }
            catch (IOException ex)
            {
                locked = true;
            }
            return locked;
        }
    }
}
