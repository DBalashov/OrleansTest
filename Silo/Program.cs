using Orleans.Configuration;
using Serilog;

#pragma warning disable CS4014

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
                                         c.AddMemoryGrainStorageAsDefault();
                                         c.UseLocalhostClustering();
                                         c.Configure<GrainCollectionOptions>(o =>
                                                                             {
                                                                                 o.CollectionAge     = TimeSpan.FromSeconds(15);
                                                                                 o.CollectionQuantum = TimeSpan.FromSeconds(2);
                                                                             })
                                          .Configure<ClusterOptions>(o =>
                                                                     {
                                                                         o.ClusterId = "dev";
                                                                         o.ServiceId = "O2";
                                                                     })
                                          .ConfigureLogging(l => l.AddConsole());
                                         //c.ConfigureApplicationParts(parts => parts.AddApplicationPart(typeof(UnitCalculatorGrain).Assembly).WithReferences());
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

// class SamplePlacementStrategyFixedSiloDirector : IPlacementDirector
// {
//     public Task<SiloAddress> OnAddActivation(PlacementStrategy strategy,
//                                              PlacementTarget   target,
//                                              IPlacementContext context)
//     {
//         var silos = context.GetCompatibleSilos(target).OrderBy(s => s).ToArray();
//         int silo  = GetSiloNumber(target.GrainIdentity.PrimaryKey, silos.Length);
//
//         return Task.FromResult(silos[silo]);
//     }
// }