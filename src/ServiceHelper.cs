using System.Diagnostics;

namespace Wslink;

internal static class ServiceHelper
{
    private static readonly string TaskName = "wslink-bridge";

    public static int Install()
    {
        var exePath = Environment.ProcessPath;
        if (exePath is null)
        {
            Console.Error.WriteLine("Could not determine executable path.");
            return 1;
        }

        var result = Run("schtasks",
            $"/create /tn \"{TaskName}\" /tr \"\\\"{exePath}\\\" forward 4444\" " +
            $"/sc onstart /ru SYSTEM /f");

        if (result is null)
        {
            Console.Error.WriteLine("Failed to create scheduled task. Try running as Administrator.");
            return 1;
        }

        Console.WriteLine("Scheduled task 'wslink-bridge' created.");
        Console.WriteLine("It will run on system startup, forwarding port 4444.");
        return 0;
    }

    public static int Remove()
    {
        var result = Run("schtasks", $"/delete /tn \"{TaskName}\" /f");
        if (result is null)
        {
            Console.Error.WriteLine("Failed to remove scheduled task.");
            return 1;
        }

        Console.WriteLine("Scheduled task removed.");
        return 0;
    }

    public static int Status()
    {
        var result = Run("schtasks", $"/query /tn \"{TaskName}\" /fo LIST");
        if (result is null)
        {
            Console.WriteLine("Scheduled task 'wslink-bridge' is not installed.");
            return 0;
        }

        Console.WriteLine("Scheduled task 'wslink-bridge':");
        Console.WriteLine(result);
        return 0;
    }

    private static string? Run(string command, string args)
    {
        try
        {
            using var proc = new Process();
            proc.StartInfo.FileName = command;
            proc.StartInfo.Arguments = args;
            proc.StartInfo.RedirectStandardOutput = true;
            proc.StartInfo.RedirectStandardError = true;
            proc.StartInfo.CreateNoWindow = true;
            proc.StartInfo.UseShellExecute = false;
            proc.Start();
            proc.WaitForExit(10000);
            return proc.ExitCode == 0 ? proc.StandardOutput.ReadToEnd() : null;
        }
        catch
        {
            return null;
        }
    }
}
