using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Wslink;

internal static class Elevation
{
    [DllImport("advapi32.dll", SetLastError = true)]
    private static extern bool OpenProcessToken(
        nint ProcessHandle, uint DesiredAccess, out nint TokenHandle);

    [DllImport("advapi32.dll", SetLastError = true)]
    private static extern bool GetTokenInformation(
        nint TokenHandle, uint TokenInformationClass,
        nint TokenInformation, uint TokenInformationLength,
        out uint ReturnLength);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool CloseHandle(nint hObject);

    private const uint TokenQuery = 0x0008;
    private const uint TokenElevationType = 18;

    public static bool IsElevated
    {
        get
        {
            try
            {
                var currentProc = -1;
                if (!OpenProcessToken(currentProc, TokenQuery, out var token))
                    return false;

                try
                {
                    var elevationType = 0u;
                    var size = (int)sizeof(uint);
                    var buf = Marshal.AllocHGlobal(size);
                    try
                    {
                        if (!GetTokenInformation(token, TokenElevationType,
                            buf, (uint)size, out _))
                            return false;

                        elevationType = (uint)Marshal.ReadInt32(buf);
                    }
                    finally
                    {
                        Marshal.FreeHGlobal(buf);
                    }

                    return elevationType == 2; // TokenElevationTypeFull
                }
                finally
                {
                    CloseHandle(token);
                }
            }
            catch
            {
                return false;
            }
        }
    }

    public static int RelaunchAsAdmin(string[] args)
    {
        var exePath = Environment.ProcessPath;
        if (exePath is null)
        {
            Console.Error.WriteLine("Cannot determine executable path.");
            return 1;
        }

        var psi = new ProcessStartInfo(exePath, string.Join(" ", args.Select(a => a.Contains(' ') ? $"\"{a}\"" : a)))
        {
            Verb = "runas",
            UseShellExecute = true,
        };

        try
        {
            using var proc = Process.Start(psi);
            proc?.WaitForExit();
            return proc?.ExitCode ?? 0;
        }
        catch
        {
            Console.Error.WriteLine("Elevation cancelled or failed.");
            Console.Error.WriteLine("Run the command from an Administrator terminal instead.");
            return 1;
        }
    }
}
