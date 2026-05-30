using System.Diagnostics;

namespace Wslink;

internal static class WslFirewall
{
    public static bool AllowPort(int port)
    {
        var result = Run("netsh",
            $"advfirewall firewall add rule " +
            $"name=\"wslink-port-{port}\" " +
            $"dir=in action=allow protocol=TCP localport={port}");
        return result is not null;
    }

    public static bool RemoveRule(int port)
    {
        var result = Run("netsh",
            $"advfirewall firewall delete rule " +
            $"name=\"wslink-port-{port}\"");
        return result is not null;
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
