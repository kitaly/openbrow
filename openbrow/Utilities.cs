using System.Diagnostics;
using System.Management;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace openbrow
{
    internal class Utilities
    {
        [SupportedOSPlatform("windows")]
        internal static bool IsChromeRunningWithProfile(string profileName, out Process? process)
        {
            // Get all running processes named "chrome"
            Process[] chromeProcesses = [];
            List<(int ProcessId, string CommandLine)> processesCommandLine = [];

            var task1 = Task.Run(() => chromeProcesses = Process.GetProcessesByName("chrome"));
            var task2 = Task.Run(() => processesCommandLine = GetProcessesWithCommandLines());

            Task.WaitAll(task1, task2);

            bool found = false;
            Process? foundProcess = null;

            var chromeProcess = processesCommandLine
                .Where(p => p.CommandLine.Contains($"--profile-directory=\"{profileName}\""))
                .Select(p => chromeProcesses.SingleOrDefault(cp => cp.Id == p.ProcessId))
                .FirstOrDefault();

            if (chromeProcess != null)
            {
                foundProcess = chromeProcess;
                found = true;
            }

            process = foundProcess;

            return found;
        }

        internal static void BringToFront(Process process)
        {
            // Get the first Chrome process (you may want to target a specific process)
            IntPtr chromeHandle = process.MainWindowHandle;

            if (chromeHandle != IntPtr.Zero)
            {
                // Restore the window if it's minimized
                ShowWindow(chromeHandle, SW_SHOW);

                // Bring the window to the foreground
                SetForegroundWindow(chromeHandle);
            }
            else
            {
                Console.WriteLine("Could not get the window handle.");
            }
        }

        internal static void StartChrome(string url, string profile)
        {
            // Specify the executable file and the arguments
            string executablePath = @"C:\Program Files\Google\Chrome\Application\chrome.exe"; // Replace with your executable path
            string args = string.Empty;

            if (string.IsNullOrEmpty(url))
            {
                args = $"--profile-directory=\"{profile}\"";
            }
            else
            {
                if (Uri.TryCreate(url, UriKind.Absolute, out Uri? uri))
                {
                    args = $"--profile-directory=\"{profile}\" --remote-debugging-port=9222 --remote-allow-origins=* {uri.AbsoluteUri}"; // Replace with your command-line arguments
                }
                else
                {
                    Console.WriteLine("The url parameter is not a valid url");
                    return;
                }
            }

            // Create a new process start info
            ProcessStartInfo startInfo = new()
            {
                FileName = executablePath,
                Arguments = args
            };

            try
            {
                // Start the process
                Process.Start(startInfo);
            }
            catch (Exception ex)
            {
                // Handle any errors that occur during the process start
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }

        [SupportedOSPlatform("windows")]
        private static List<(int ProcessId, string CommandLine)> GetProcessesWithCommandLines()
        {
            // List to store the process ID and command line information
            List<(int, string)> processes = new List<(int, string)>();

            try
            {
                // Create a ManagementObjectSearcher to query the Win32_Process class
                using (ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT ProcessId, CommandLine FROM Win32_Process"))
                {
                    // Execute the query
                    ManagementObjectCollection results = searcher.Get();

                    // Iterate through each ManagementObject in the collection
                    foreach (ManagementObject obj in results)
                    {
                        try
                        {
                            // Get the process ID
                            int processId = Convert.ToInt32(obj["ProcessId"]);

                            // Get the command line (can be null for system processes)
                            string commandLine = obj["CommandLine"]?.ToString() ?? string.Empty;

                            // Add the process ID and command line to the list
                            processes.Add((processId, commandLine));
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error processing object: {ex.Message}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error querying processes: {ex.Message}");
            }

            return processes;
        }

        // Import necessary user32.dll functions
        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        private const int SW_RESTORE = 9; // Restore the window if minimized

        private const int SW_SHOW = 5; // Shows the window in its current state

        private const int SW_MAXIMIZE = 3;  // Maximize the window
    }
}
