using openbrow;
using System.Diagnostics;
using System.Runtime.InteropServices;

string url = string.Empty;
string profile = string.Empty;

foreach (var arg in args)
{
    if (arg.StartsWith("-url="))
    {
        url = arg.Substring(5);
    }
    else if (arg.StartsWith("-profile="))
    {
        profile = arg.Substring(9).Trim('"');
    }
}

if (string.IsNullOrEmpty(url) && string.IsNullOrEmpty(profile))
{
    Console.WriteLine("Invalid arguments. Please provide either -url and/or -profile arguments.");
    return;
}

if (string.IsNullOrEmpty(profile))
{
    profile = "Default";
}

if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
{
    var isChromeRunning = Utilities.IsChromeRunningWithProfile(profile, out Process? process);

    if (isChromeRunning && process != null)
    {
        // bring browser to front
        var task1 = Task.Run(() => Utilities.BringToFront(process));

        // focus tab
        var task2 = Task.Run(async () => await ChromeUtilities.FocusOrCreateTab(url));

        Task.WhenAll(task1, task2).Wait();
    }
    else
    {
        Utilities.StartChrome(url, profile);
    }
}