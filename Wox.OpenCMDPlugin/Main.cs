using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;
using System.Web;


namespace Wox.Plugin.OpenCMD
{
    public class Main : IPlugin
    {
        private const string ExplorerProcessName = "explorer";
        private List<SystemWindow> _openingWindows = new List<SystemWindow>();

        static Main()
        {
            // use to auto load Interop.SHDocVw.dll from resources
            // only copy to plugin folder can not load correctly
            AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(CurrentDomain_AssemblyResolve);
        }

        public List<Result> Query(Query query)
        {
            var list = new List<Result>();

            if (TryGetLastUsedExplorerWindowHandle(out var windowHandle))
            {
                foreach (var window in GetWindowsWithPaths())
                {
                    // immediately open the windows command
                    if (windowHandle == (IntPtr)window.HWND)
                    {
                        var path = GetAndVerifyPathFromWindow(window);
                        if (path == null)
                            break;

                        StartShell(path);
                        return list;
                    }
                }
            }

            // list all windows explorer paths
            foreach (var window in GetWindowsWithPaths())
            {
                var path = GetAndVerifyPathFromWindow(window);
                if (path == null)
                    continue;

                list.Add(new Result
                {
                    IcoPath = "Images\\app.png",
                    Title = path,
                    SubTitle = "Open cmd in this path",
                    Action = (c) =>
                    {
                        StartShell(path);
                        return true;
                    }
                });
            }

            return list;
        }

        public IEnumerable<SHDocVw.InternetExplorer> GetWindowsWithPaths()
        {
            var windows = new SHDocVw.ShellWindows();
            foreach (SHDocVw.InternetExplorer shellWindow in windows)
            {
                if (IsWindowsExplorerWindowWithPath(shellWindow))
                {
                    yield return shellWindow;
                }
            }
        }

        public string GetAndVerifyPathFromWindow(SHDocVw.InternetExplorer window)
        {
            var path = window.LocationURL.Replace("file:///", "");
            path = HttpUtility.UrlDecode(path);
            if (!Directory.Exists(path))
                return null;

            return path;
        }

        public bool IsWindowsExplorerWindowWithPath(SHDocVw.InternetExplorer window)
        {
            var filename = Path.GetFileNameWithoutExtension(window.FullName).ToLower();
            if (filename.ToLowerInvariant() == ExplorerProcessName)
            {
                if (!window.LocationURL.ToLower().Contains("file:"))
                    return false;

                return true;
            }

            return false;
        }

        public void Init(PluginInitContext context)
        {
        }

        private static void StartShell(string path)
        {
            Process.Start(new ProcessStartInfo()
            {
                FileName = "cmd",
                WorkingDirectory = path
            });
        }

        private bool TryGetLastUsedExplorerWindowHandle(out IntPtr windowHandle)
        {
            _openingWindows = new List<SystemWindow>();
            WinApi.EnumWindowsProc callback = EnumWindows;
            WinApi.EnumWindows(callback, 0);
            if (_openingWindows.Count > 0 && _openingWindows[0].Process.ProcessName == ExplorerProcessName)
            {
                windowHandle = _openingWindows[0].HWnd;
                return true;
            }

            windowHandle = default;
            return false;
        }

        private bool EnumWindows(IntPtr hWnd, int lParam)
        {
            if (!WinApi.IsWindowVisible(hWnd))
                return true;

            var title = new StringBuilder(256);
            WinApi.GetWindowText(hWnd, title, 256);

            if (string.IsNullOrEmpty(title.ToString()))
            {
                return true;
            }

            if (title.Length != 0 || (title.Length == 0 & hWnd != WinApi.statusbar))
            {
                var window = new SystemWindow(hWnd);
                if (window.IsAltTabWindow() && !window.IsTopmostWindow())
                {
                    _openingWindows.Add(window);
                }
            }

            return true;
        }

        public static System.Reflection.Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            string dllName = args.Name.Contains(',') ? args.Name.Substring(0, args.Name.IndexOf(',')) : args.Name.Replace(".dll", "");

            dllName = dllName.Replace(".", "_");

            if (dllName.EndsWith("_resources")) return null;

            System.Resources.ResourceManager rm = new System.Resources.ResourceManager(typeof(Main).Namespace + ".Properties.Resources", System.Reflection.Assembly.GetExecutingAssembly());

            byte[] bytes = (byte[])rm.GetObject(dllName);

            return System.Reflection.Assembly.Load(bytes);
        }


    }
}
