using System.Diagnostics;

namespace Wslink;

internal static class Netsh
{
    public static bool AddProxy(int port, string listenAddr, string connectAddr)
    {
        var result = Run("netsh",
            $"interface portproxy add v4tov4 listenport={port} " +
            $"listenaddress={listenAddr} connectport={port} " +
            $"connectaddress={connectAddr}");
        return result is not null;
    }

    public static bool DeleteProxy(int port, string listenAddr)
    {
        var result = Run("netsh",
            $"interface portproxy delete v4tov4 listenport={port} " +
            $"listenaddress={listenAddr}");
        return result is not null;
    }

    public static string? ShowAll()
    {
        return Run("netsh", "interface portproxy show all");
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
            proc.WaitForExit(5000);
            return proc.ExitCode == 0 ? proc.StandardOutput.ReadToEnd() : null;
        }
        catch
        {
            return null;
        }
    }
}
