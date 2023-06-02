using GrainInterfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

const string ClusterId = "dev";
const string ServiceId = "O2";

try
{
    var host   = await buildHost();
    var client = host.Services.GetRequiredService<IClusterClient>();
    while (true)
    {
        var i1 = int.Parse(Console.ReadLine());
        var x  = await client.GetGrain<IUnitCalculator>(1).Calculate(i1);
        Console.WriteLine(x);
    }

    await host.StopAsync();
    Console.WriteLine("Exited");
    Console.ReadLine();
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
                                      c.UseLocalhostClustering(serviceId: ServiceId, clusterId: ClusterId)
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