using GrainInterfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Orleans.Configuration;

const string ConnectionString    = @"Data Source=localhost,1433; Initial Catalog=Orleans;Integrated Security=True; Pooling=False;Max Pool Size=200;";
const string ConnectionInvariant = "System.Data.SqlClient";
const string ClusterId           = "dev";
const string ServiceId           = "O2";

try
{
    var host   = await buildHost();
    var client = host.Services.GetRequiredService<IClusterClient>();
    while (true)
    {
        var s1 = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(s1)) break;

        var value = int.Parse(s1);
        var grain = client.GetGrain<IStateGrain>(1);
        Console.WriteLine("Current state: {0}", await grain.Get());
        await grain.Set(value * 2);
        Console.WriteLine("New     state: {0}", await grain.Get());
        
        // var friend   = client.GetGrain<IUnitCalculator>(value);
        // var response = await friend.Calculate(value);
        //Console.WriteLine("Response: {0}", response);
    }

    await host.StopAsync();
    Console.WriteLine("Exited");
}
catch (Exception e)
{
    Console.WriteLine($"\n\nException while trying to run client: {e.Message}");
}

static async Task<IHost> buildHost()
{
    var client = new HostBuilder()
                .ConfigureLogging(logging => logging.AddConsole())
                .UseOrleansClient(c =>
                                  {
                                      c.Configure<ClusterOptions>(o =>
                                                                  {
                                                                      o.ClusterId = ClusterId;
                                                                      o.ServiceId = ServiceId;
                                                                  })
                                       .UseAdoNetClustering(o =>
                                                            {
                                                                o.Invariant        = ConnectionInvariant;
                                                                o.ConnectionString = ConnectionString;
                                                            })
                                       .UseConnectionRetryFilter(async (ex, cancellationToken) =>
                                                                 {
                                                                     Console.WriteLine("Can't connect, retry in 300 ms: {0}", ex.Message);
                                                                     await Task.Delay(300);
                                                                     return true;
                                                                 });
                                  })
                .Build();

    await client.StartAsync();
    Console.WriteLine("Client successfully connected to silo host");

    return client;
}