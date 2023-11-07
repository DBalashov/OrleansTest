using System.Reflection;

namespace Shared;

public static class ConfigExtenders
{
    public static IConfigurationBuilder ConfigureAppSettings(this IConfigurationBuilder builder)
    {
        var LOCATION = Path.GetDirectoryName(Assembly.GetEntryAssembly()!.Location)!;
        var r = builder.SetBasePath(LOCATION)
                       .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

        var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production";
        if (!string.IsNullOrWhiteSpace(env))
            r = r.AddJsonFile($"appsettings.{env}.json", optional: true, reloadOnChange: true);

        var computerName = Environment.GetEnvironmentVariable("COMPUTERNAME");
        return r.AddJsonFile("appsettings.user.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{computerName}.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables("O2:");
    }
}