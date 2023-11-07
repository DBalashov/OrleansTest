using System.Net;
using Orleans.Configuration;
using Orleans.Providers.MongoDB.Configuration;
using Orleans.Providers.MongoDB.StorageProviders.Serializers;
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
                                         c.Services.AddSingleton<IGrainStateSerializer, BsonGrainStateSerializer>();
                                         c.Configure<ClusterOptions>(o =>
                                                                     {
                                                                         o.ClusterId = OrleansConfig.ClusterId;
                                                                         o.ServiceId = OrleansConfig.ServiceId;
                                                                     });
                                         c.ConfigureEndpoints(IPAddress.Loopback,
                                                              siloPort: 11111    + portIncrement,
                                                              gatewayPort: 30000 + portIncrement);

                                         c.Configure<GrainCollectionOptions>(o =>
                                                                             {
                                                                                 o.CollectionAge     = TimeSpan.FromSeconds(15);
                                                                                 o.CollectionQuantum = TimeSpan.FromSeconds(2);
                                                                             })
                                          .UseMongoDBClient(OrleansConfig.MongoUrl)
                                          .UseMongoDBClustering(o =>
                                                                {
                                                                    o.DatabaseName = OrleansConfig.DatabaseName;
                                                                    o.Strategy     = MongoDBMembershipStrategy.SingleDocument;
                                                                })
                                          .UseMongoDBReminders(o => o.DatabaseName             = OrleansConfig.DatabaseName)
                                          .AddMongoDBGrainStorageAsDefault(o => o.DatabaseName = OrleansConfig.DatabaseName);

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

class IncomingGrainFilter : IIncomingGrainCallFilter
{
    public async Task Invoke(IIncomingGrainCallContext ctx)
    {
        await ctx.Invoke();
    }
}