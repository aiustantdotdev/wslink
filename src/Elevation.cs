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

    public static void WarnIfNotElevated()
    {
        if (!IsElevated)
        {
            Console.Error.WriteLine("Warning: Not running as Administrator.");
            Console.Error.WriteLine("Portproxy and firewall changes require elevation.");
            Console.Error.WriteLine("Restart as Administrator for full functionality.");
            Console.Error.WriteLine();
        }
    }
}
