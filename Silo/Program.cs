using System.Net;
using Orleans.Configuration;
using Orleans.Runtime;
using Serilog;
using Shared;

#pragma warning disable CS4014

var portIncrement = int.TryParse(Environment.GetCommandLineArgs().Skip(1).FirstOrDefault(), out var port) ? port : 0;

try
{
    using var host = Host.CreateDefaultBuilder(args)
                         .ConfigureHostConfiguration(r => r.ConfigureAppSettings())
                         .UseSerilog((context, services, config) => config.ReadFrom.Configuration(context.Configuration)
                                                                          .ReadFrom.Services(services)
                                                                          .Enrich.FromLogContext())
                         .UseOrleans(c =>
                                     {
                                         c.Configure<ReminderOptions>(o => o.MinimumReminderPeriod = TimeSpan.FromSeconds(3))
                                          .Configure<ClusterOptions>(o =>
                                                                     {
                                                                         o.ClusterId = OrleansConfig.ClusterId;
                                                                         o.ServiceId = OrleansConfig.ServiceId;
                                                                     })
                                          .ConfigureEndpoints(IPAddress.Loopback,
                                                              siloPort: 11111    + portIncrement,
                                                              gatewayPort: 30000 + portIncrement);
                                         
                                         c.Configure<GrainCollectionOptions>(o =>
                                                                             {
                                                                                 o.CollectionAge     = TimeSpan.FromSeconds(15);
                                                                                 o.CollectionQuantum = TimeSpan.FromSeconds(2);
                                                                             });
                                         c.UseLocalhostClustering();
                                         // c.UseRedisClustering(o =>
                                         //                      {
                                         //                          o.Database         = 0;
                                         //                          o.ConnectionString = OrleansConfig.RedisUrl;
                                         //                      })
                                         //  .UseRedisReminderService(o =>
                                         //                           {
                                         //                               o.DatabaseNumber   = 0;
                                         //                               o.ConnectionString = OrleansConfig.RedisUrl;
                                         //                           })
                                         //  .AddRedisGrainStorageAsDefault(o =>
                                         //                                 {
                                         //                                     o.DatabaseNumber   = 0;
                                         //                                     o.ConnectionString = OrleansConfig.RedisUrl;
                                         //                                 });
                                         
                                         c.AddIncomingGrainCallFilter<IncomingGrainFilter>();
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

class IncomingGrainFilter(ILogger<IncomingGrainFilter> logger) : IIncomingGrainCallFilter
{
    public async Task Invoke(IIncomingGrainCallContext ctx)
    {
        var s1 = RequestContext.Get("aaa");
        // logger.LogInformation($"Grain {ctx.Grain.GetType().Name} invoked method {ctx.InterfaceMethod.Name} with arguments {ctx.SourceId}");
        await ctx.Invoke();
    }
}