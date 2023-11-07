using GrainInterfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Orleans.Configuration;
using Orleans.Providers.MongoDB.Configuration;
using Serilog;
using Shared;


var host = buildHost();
await host.StartAsync();
Console.WriteLine("Client successfully connected to silo host");

while (true)
    try
    {
        var client = host.Services.GetRequiredService<IClusterClient>();
        Console.WriteLine("Enter string");
        while (true)
        {
            // var i1 = int.Parse(Console.ReadLine());
            // var x  = await client.GetGrain<IUnitCalculator>(1).Calculate(i1);

            var s = Console.ReadLine();
            if (string.IsNullOrEmpty(s)) break;

            var x = await client.GetGrain<IStateStringAccumulatorGrain>(1).Add(s);
            Console.WriteLine(x);
        }

        await host.StopAsync();
        Console.WriteLine("Exited");
        Console.ReadLine();
        break;
    }
    catch (Exception e)
    {
        Console.WriteLine($"\n\nException while trying to run client: {e.Message}");
    }

static IHost buildHost()
{
    var client = new HostBuilder()
                .ConfigureHostConfiguration(r => r.ConfigureAppSettings())
                .UseSerilog((context, services, config) => config.ReadFrom.Configuration(context.Configuration)
                                                                 .ReadFrom.Services(services)
                                                                 .Enrich.FromLogContext())
                .UseOrleansClient(c =>
                                  {
                                      c.Configure<ClusterOptions>(o =>
                                                                  {
                                                                      o.ClusterId = OrleansConfig.ClusterId;
                                                                      o.ServiceId = OrleansConfig.ServiceId;
                                                                  });
                                      c.ConfigureServices(s => s.AddSingleton<IClientConnectionRetryFilter, ConnectionRetryFilter>());
                                      c.UseMongoDBClient(OrleansConfig.MongoUrl)
                                       .UseMongoDBClustering(o =>
                                                             {
                                                                 o.DatabaseName = OrleansConfig.DatabaseName;
                                                                 o.Strategy     = MongoDBMembershipStrategy.SingleDocument;
                                                             });
                                  })
                .Build();

    return client;
}