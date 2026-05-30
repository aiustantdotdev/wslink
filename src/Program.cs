namespace Wslink;

public sealed class Program
{
    public static int Main(string[] args)
    {
        if (args.Length == 0)
        {
            PrintUsage();
            return 0;
        }

        if (args[0] is "forward" or "remove")
            Elevation.WarnIfNotElevated();
        if (args[0] == "service" && args.Length > 1 && args[1] != "status")
            Elevation.WarnIfNotElevated();

        return args[0] switch
        {
            "forward" => Commands.Forward(args[1..]),
            "remove" => Commands.Remove(args[1..]),
            "list" => Commands.List(),
            "service" => Commands.Service(args[1..]),
            _ => PrintUsage(1),
        };
    }

    private static int PrintUsage(int exitCode = 0)
    {
        Console.WriteLine("""
            wslink — Windows ↔ WSL port bridge

            Usage:
              wslink forward <port> [--distro <name>]    Forward WSL port to Windows
              wslink remove <port>                        Remove forwarding rule
              wslink list                                 List active rules
              wslink service install                      Install as background service
              wslink service remove                       Remove background service

            Examples:
              wslink forward 4444
              wslink forward 4444 --distro Ubuntu
              wslink remove 4444
            """);
        return exitCode;
    }
}
