using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Management;
using System.Runtime.InteropServices;
using System.Runtime.Remoting.Messaging;

namespace Wox.Plugin.ProcessKiller
{
    public class Main : IPlugin
    {
        private readonly HashSet<string> _systemProcessList = new HashSet<string>(){
            "conhost",
            "svchost",
            "idle",
            "system",
            "rundll32",
            "csrss",
            "lsass",
            "lsm",
            "smss",
            "wininit",
            "winlogon",
            "services",
            "spoolsv",
            "explorer"};

        public List<Result> Query(Query query)
        {
            var termToSearch = query.Terms.Length == 0
                ? null
                : query.FirstSearch.ToLower();
            var processlist = GetProcesslist(termToSearch);

            return 
                !processlist.Any() 
                    ? null 
                    : CreateResultsFromProcesses(processlist, termToSearch);
        }

        private List<Result> CreateResultsFromProcesses(List<Process> processlist, string termToSearch)
        {
            var results = new List<Result>();

            foreach (var proc in processlist)
            {
                var p = proc;
                var path = GetPath(p);
                results.Add(new Result()
                {
                    IcoPath = path,
                    Title = p.ProcessName + " - " + p.Id,
                    SubTitle = path,
                    Action = (c) =>
                    {
                        KillProcess(p);
                        return true;
                    }
                });
            }

            if (processlist.Count > 1 && !string.IsNullOrEmpty(termToSearch))
            {
                results.Insert(0, new Result()
                {
                    IcoPath = "Images\\app.png",
                    Title = "kill all \"" + termToSearch + "\" process",
                    SubTitle = "",
                    Action = (c) =>
                    {
                        foreach (var p in processlist)
                        {
                            KillProcess(p);
                        }

                        return true;
                    }
                });
            }

            return results;

            void KillProcess(Process p)
            {
                if (!p.HasExited)
                {
                    p.Kill();
                }
            }
        }

        private List<Process> GetProcesslist(string termToSearch)
        {
            var processlist = new List<Process>();
            var processes = Process.GetProcesses();
            if (string.IsNullOrWhiteSpace(termToSearch))
            {
                // show all process
                foreach (var p in processes)
                {
                    if (FilterSystemProcesses(p)) continue;

                    processlist.Add(p);
                }
            }
            else
            {
                foreach (var p in processes)
                {
                    if (FilterSystemProcesses(p)) continue;

                    if ((p.ProcessName + p.Id).ToLower().Contains(termToSearch))
                    {
                        processlist.Add(p);
                    }
                }
            }

            return processlist;

            bool FilterSystemProcesses(Process p)
            {
                var name = p.ProcessName.ToLower();
                if (_systemProcessList.Contains(name))
                    return true;
                return false;
            }
        }

        private string GetPath(Process p)
        {
            try
            {
                var path = GetProcessFilename(p);
                return path.ToLower();
            }
            catch
            {
                return "";
            }
        }

        public void Init(PluginInitContext context)
        {
        }

        [Flags]
        private enum ProcessAccessFlags : uint
        {
            QueryLimitedInformation = 0x00001000
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool QueryFullProcessImageName(
            [In] IntPtr hProcess,
            [In] int dwFlags,
            [Out] StringBuilder lpExeName,
            ref int lpdwSize);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr OpenProcess(
            ProcessAccessFlags processAccess,
            bool bInheritHandle,
            int processId);

        private string GetProcessFilename(Process p)
        {
            int capacity = 2000;
            StringBuilder builder = new StringBuilder(capacity);
            IntPtr ptr = OpenProcess(ProcessAccessFlags.QueryLimitedInformation, false, p.Id);
            if (!QueryFullProcessImageName(ptr, 0, builder, ref capacity))
            {
                return String.Empty;
            }

            return builder.ToString();
        }
    }
}
