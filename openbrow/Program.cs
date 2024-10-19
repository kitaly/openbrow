using openbrow;
using System.Diagnostics;

Stopwatch stopwatch = new Stopwatch();

stopwatch.Start();

if (args.Length != 1)
{
    Console.WriteLine("The website url parameter is mandatory");

    return;
}

if (Uri.TryCreate(args[0], UriKind.Absolute, out Uri? uri))
{
    var isChromeRunning = Utilities.IsChromeRunningWithProfile("Default", out Process? process);

    Console.WriteLine($"Is Chrome Running executed in: {stopwatch.Elapsed.TotalMilliseconds} ms");

    if (isChromeRunning && process != null)
    {
        // bring browser to front
        var task1 = Task.Run(() => Utilities.BringToFront(process));

        // focus tab
        var task2 = Task.Run(async () => await ChromeUtilities.FocusOrCreateTab(uri));

        Task.WhenAll(task1, task2).Wait();

        Console.WriteLine($"Focus tab executed in: {stopwatch.Elapsed.TotalMilliseconds} ms");
    }
    else
    {
        Utilities.StartChrome(uri);
    }
}
else
{
    Console.WriteLine("The website url parameter is not a valid url");
}

stopwatch.Stop();

Console.WriteLine($"Program executed in: {stopwatch.Elapsed.TotalMilliseconds} ms");