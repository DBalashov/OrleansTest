using System.Net;
using Orleans.Configuration;
using Serilog;

#pragma warning disable CS4014

const string ConnectionString    = @"Data Source=localhost,1433; Initial Catalog=Orleans;Integrated Security=True; Pooling=False;Max Pool Size=200;";
const string ConnectionInvariant = "System.Data.SqlClient";
const string ClusterId           = "dev";
const string ServiceId           = "O2";

var arg = Environment.GetCommandLineArgs().Skip(1).FirstOrDefault();

try
{
    const string LOG_TEMPLATE = "{Timestamp:HH:mm:ss.fff} [{Level:u3}] {Message:l}{NewLine}{Exception}";

    using var host = Host.CreateDefaultBuilder(args)
                         .UseSerilog((context, services, configuration) =>
                                     {
                                         var path    = Path.GetDirectoryName(typeof(Program).Assembly.Location);
                                         var logFile = Path.Combine(path!, "Logs", "fmo.log");
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
                                         c.UseAdoNetClustering(o =>
                                                               {
                                                                   o.Invariant        = ConnectionInvariant;
                                                                   o.ConnectionString = ConnectionString;
                                                               })
                                          .UseAdoNetReminderService(o =>
                                                                    {
                                                                        o.Invariant        = ConnectionInvariant;
                                                                        o.ConnectionString = ConnectionString;
                                                                    })
                                          .AddAdoNetGrainStorageAsDefault(o =>
                                                                          {
                                                                              o.Invariant        = ConnectionInvariant;
                                                                              o.ConnectionString = ConnectionString;
                                                                          });

                                         c.ConfigureEndpoints(IPAddress.Parse(arg ?? "192.168.100.4"), siloPort: 11111, gatewayPort: 30000);

                                         c.Configure<GrainCollectionOptions>(o =>
                                                                             {
                                                                                 o.CollectionAge     = TimeSpan.FromSeconds(15);
                                                                                 o.CollectionQuantum = TimeSpan.FromSeconds(2);
                                                                             })
                                          .Configure<ClusterOptions>(o =>
                                                                     {
                                                                         o.ClusterId = ClusterId;
                                                                         o.ServiceId = ServiceId;
                                                                     })
                                          .ConfigureLogging(l => l.AddConsole());
                                     })
                         .Build();
    host.StartAsync();

    Console.WriteLine("\n\n Press Enter to terminate...\n\n");
    Console.ReadLine();

    await host.StopAsync();
}
catch (Exception ex)
{
    Console.WriteLine(ex);
}