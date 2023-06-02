using System.Net;
using Grains;
using Orleans.Configuration;
using Serilog;

#pragma warning disable CS4014

const string ClusterId = "dev";
const string ServiceId = "O2";

var arg = Environment.GetCommandLineArgs().Skip(1).FirstOrDefault();

try
{
    const string LOG_TEMPLATE = "{Timestamp:HH:mm:ss.fff}\t{Level:u3}\t{Message:l}{NewLine}{Exception}"; // {SourceContext}\t

    using var host = Host.CreateDefaultBuilder(args)
                         .UseSerilog((context, services, configuration) =>
                                     {
                                         var path    = Path.GetDirectoryName(typeof(Program).Assembly.Location);
                                         var logFile = Path.Combine(path!, "Logs", ".log");
                                         configuration.ReadFrom.Configuration(context.Configuration)
                                                      .ReadFrom.Services(services)
                                                      .Enrich.FromLogContext()
                                                      .WriteTo.Console(outputTemplate: LOG_TEMPLATE)
                                                      .WriteTo.File(logFile,
                                                                    rollingInterval: RollingInterval.Day,
                                                                    fileSizeLimitBytes: 100 * 1024 * 1024,
                                                                    retainedFileCountLimit: 7,
                                                                    rollOnFileSizeLimit: false,
                                                                    shared: true,
                                                                    outputTemplate: LOG_TEMPLATE,
                                                                    flushToDiskInterval: TimeSpan.FromSeconds(3));
                                     })
                         .UseOrleans(c =>
                                     {
                                         c.UseLocalhostClustering(serviceId: ServiceId, clusterId: ClusterId)
                                          .Configure<GrainCollectionOptions>(o =>
                                                                             {
                                                                                 o.CollectionAge     = TimeSpan.FromSeconds(15);
                                                                                 o.CollectionQuantum = TimeSpan.FromSeconds(2);
                                                                             })
                                          .UseInMemoryReminderService()
                                          .ConfigureLogging(l => l.AddSerilog());
                                     })
                         .Build();
    host.StartAsync();
    var l = host.Services.GetRequiredService<ILogger<Program>>();
    l.LogInformation("\n\n Press Enter to terminate...\n\n");
    Console.ReadLine();

    await host.StopAsync();
}
catch (Exception ex)
{
    Console.WriteLine(ex);
}