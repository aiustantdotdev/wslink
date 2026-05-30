using System.Text.RegularExpressions;

namespace Wslink;

internal static partial class Wsl
{
    public static List<Distro> ListRunning()
    {
        var output = Run("wsl.exe", "--list --running --quiet");
        if (output is null) return [];

        return output
            .Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(name => new Distro(name, GetIp(name)))
            .Where(d => d.Ip is not null)
            .ToList();
    }

    public static string? GetIp(string distro)
    {
        var ip = Run("wsl.exe", $"-d \"{distro}\" -- hostname -I");
        return ip?.Trim().Split(' ')[0];
    }

    internal static string? Run(string command, string args)
    {
        try
        {
            using var proc = new System.Diagnostics.Process();
            proc.StartInfo.FileName = command;
            proc.StartInfo.Arguments = args;
            proc.StartInfo.RedirectStandardOutput = true;
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

internal sealed record Distro(string Name, string? Ip);
