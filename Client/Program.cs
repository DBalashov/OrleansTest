using GrainInterfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

try
{
    var host = await BuildHost();
    var client = host.Services.GetRequiredService<IClusterClient>();
    var friend   = client.GetGrain<IUnitCalculator>(0);
    var response = await friend.Calculate(100);
    Console.WriteLine($"\n\n{response}\n\n");
    Console.ReadLine();
    await host.StopAsync();
}
catch (Exception e)
{
    Console.WriteLine($"\nException while trying to run client: {e.Message}");
}

static async Task<IHost> BuildHost()
{
    var client = new HostBuilder()
              .ConfigureLogging(logging => logging.AddConsole())
              .UseOrleansClient((context, client) =>
                                {
                                    client.UseLocalhostClustering(serviceId: "O2", clusterId: "dev")
                                          .UseConnectionRetryFilter(async (ex, cancellationToken) =>
                                                                    {
                                                                        await Task.Delay(300);
                                                                        return true;
                                                                    });
                                })
              .Build();
    
    await client.StartAsync();
    Console.WriteLine("Client successfully connected to silo host");

    return client;
}