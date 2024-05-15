using GrainInterfaces;
using Microsoft.Extensions.Logging;
using Orleans.Runtime;
using Orleans.Streams;

namespace Grains;

public class StreamingGrain : Grain, IStreamingGrain
{
    readonly ILogger        logger;
    IAsyncStream<DateTime>? stream;
    IDisposable             timer;
    int                     count;

    public StreamingGrain(ILogger<StreamingGrain> logger)
    {
        this.logger = logger;
    }

    public async Task<string> Execute(int value)
    {
        logger.LogInformation("Execute: [{0} - {1} - {2}], Value={3}", RuntimeIdentity, IdentityString, this.GetPrimaryKeyString(), value);

        var streamProvider = this.GetStreamProvider("provider1");
        var key            = Guid.NewGuid().ToString();
        var streamId       = StreamId.Create("RANDOMDATA", key);
        stream = streamProvider.GetStream<DateTime>(streamId);

        count = value;
        timer = RegisterTimer(onTimer, value, TimeSpan.Zero, TimeSpan.FromSeconds(1));

        return key;
    }

    async Task onTimer(object o)
    {
        if (stream == null) return;
        logger.LogInformation("Execute.Item: [{0} - {1} - {2}], Grain timer", RuntimeIdentity, IdentityString, this.GetPrimaryKeyString());
        await stream.OnNextAsync(DateTime.UtcNow);
        if (count-- <= 0)
        {
            await stream.OnCompletedAsync();
            stream = null;
            timer.Dispose();
        }
    }
}