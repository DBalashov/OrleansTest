using GrainInterfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Orleans.Configuration;
using Orleans.Runtime;
using Serilog;
using Shared;

var host = buildHost();
await host.StartAsync();
Console.WriteLine("Client successfully connected to silo host");

while (true)
    try
    {
        var client = host.Services.GetRequiredService<IClusterClient>();

        // var g   = client.GetGrain<IStreamingGrain>(1);
        // var streamKey = await g.Execute(5);
        // var stream = client
        //             .GetStreamProvider("provider1")
        //             .GetStream<DateTime>(StreamId.Create("RANDOMDATA", streamKey));
        // await stream.SubscribeAsync(OnNextAsync);

        Console.WriteLine("Enter string");
        for (var i = 1; i <= 1; i++)
        {
            var k = i;
            new Thread(async () =>
                       {
                           while (true)
                           {
                               //var i1 = int.Parse(Console.ReadLine());
                               //var x  = await client.GetGrain<IUnitCalculator>(1).Calculate(i1);
                               var g = client.GetGrain<IUnitCalculator>(1);
                               RequestContext.Set("aaa", "T" + k);

                               await foreach (var s in client.GetGrain<IUnitCalculator>(1).Powers(k))
                               {
                                   Console.WriteLine(s);
                               }

                               Thread.Sleep(5000);
                               // var s = Console.ReadLine();
                               // if (string.IsNullOrEmpty(s)) break;
                               //
                               // var x = await client.GetGrain<IStateStringAccumulatorGrain>(1).Add(s);
                               // Console.WriteLine(x);
                           }
                       }).Start();
        }

        Console.ReadLine();

        await host.StopAsync();
        Console.WriteLine("Exited");
        Console.ReadLine();
        break;
    }
    catch (Exception e)
    {
        Console.WriteLine($"\n\nException while trying to run client: {e.Message}");
    }

// static async Task OnNextAsync(DateTime item, StreamSequenceToken token)
// {
//     Console.WriteLine("OnNextAsync: item: {0}, token = {1}", item, token);
// }

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

                                      c.UseLocalhostClustering();
                                      // c.UseRedisClustering(o =>
                                      //                      {
                                      //                          o.Database         = 0;
                                      //                          o.ConnectionString = OrleansConfig.RedisUrl;
                                      //                      });

                                      // c.UseMongoDBClient(OrleansConfig.MongoUrl)
                                      //  .UseMongoDBClustering(o =>
                                      //                        {
                                      //                            o.DatabaseName = OrleansConfig.DatabaseName;
                                      //                            o.Strategy     = MongoDBMembershipStrategy.SingleDocument;
                                      //                        });
                                  })
                .Build();

    return client;
}