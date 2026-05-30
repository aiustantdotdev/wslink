namespace Wslink;

internal static class Commands
{
    public static int Forward(string[] args)
    {
        if (args.Length == 0 || !int.TryParse(args[0], out var port))
        {
            Console.Error.WriteLine("Usage: wslink forward <port> [--distro <name>]");
            return 1;
        }

        var distroArg = Array.IndexOf(args, "--distro");
        var distroName = distroArg >= 0 && distroArg + 1 < args.Length
            ? args[distroArg + 1]
            : null;

        var distros = Wsl.ListRunning();

        if (distros.Count == 0)
        {
            Console.Error.WriteLine("No running WSL distros found.");
            return 1;
        }

        var distro = distroName is not null
            ? distros.Find(d =>
                d.Name.Equals(distroName, StringComparison.OrdinalIgnoreCase))
            : distros[0];

        if (distro is null)
        {
            Console.Error.WriteLine($"Distro '{distroName}' not found or not running.");
            return 1;
        }

        if (distro.Ip is null)
        {
            Console.Error.WriteLine($"Could not resolve IP for distro '{distro.Name}'.");
            return 1;
        }

        Console.WriteLine($"Distro:     {distro.Name}");
        Console.WriteLine($"IP:         {distro.Ip}");
        Console.WriteLine($"Port:       {port}");

        var n1 = Netsh.AddProxy(port, "0.0.0.0", distro.Ip);
        var n2 = Netsh.AddProxy(port, "127.0.0.1", distro.Ip);
        var fw = WslFirewall.AllowPort(port);

        if (!n1 && !n2)
        {
            Console.Error.WriteLine("Failed to set up portproxy. Try running as Administrator.");
            return 1;
        }

        Console.WriteLine($"Forward:    http://localhost:{port} → WSL ({distro.Ip}:{port})");

        if (fw)
            Console.WriteLine("Firewall:   rule added");

        Console.WriteLine();
        Console.WriteLine("Run 'wslink list' to see active rules.");
        Console.WriteLine("Run 'wslink remove <port>' to clean up.");

        return 0;
    }

    public static int Remove(string[] args)
    {
        if (args.Length == 0 || !int.TryParse(args[0], out var port))
        {
            Console.Error.WriteLine("Usage: wslink remove <port>");
            return 1;
        }

        var r1 = Netsh.DeleteProxy(port, "0.0.0.0");
        var r2 = Netsh.DeleteProxy(port, "127.0.0.1");
        var fw = WslFirewall.RemoveRule(port);

        if (!r1 && !r2)
        {
            Console.Error.WriteLine($"No proxy rule found for port {port}.");
            return 1;
        }

        Console.WriteLine($"Removed forwarding rule for port {port}.");
        if (fw)
            Console.WriteLine("Firewall rule removed.");

        return 0;
    }

    public static int List()
    {
        var output = Netsh.ShowAll();
        if (output is null || string.IsNullOrWhiteSpace(output))
        {
            Console.WriteLine("No active portproxy rules.");
            return 0;
        }

        Console.WriteLine(output);
        return 0;
    }

    public static int Service(string[] args)
    {
        if (args.Length == 0)
        {
            Console.Error.WriteLine("Usage: wslink service install|remove");
            return 1;
        }

        return args[0] switch
        {
            "install" => ServiceHelper.Install(),
            "remove" => ServiceHelper.Remove(),
            _ => ServiceHelper.Status(),
        };
    }
}
