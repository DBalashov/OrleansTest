using GrainInterfaces;
using Microsoft.Extensions.Logging;
using Orleans.Runtime;
using Orleans.Streams;

namespace Grains;

public class StreamingGrain(ILogger<StreamingGrain> logger) : Grain, IStreamingGrain
{
    readonly ILogger        logger = logger;
    IAsyncStream<DateTime>? stream;
    IDisposable?            timer;
    int                     count;

    public async Task<string> Execute(int value)
    {
        logger.LogInformation("Execute: [{0} - {1} - {2}], Value={3}", RuntimeIdentity, IdentityString, this.GetPrimaryKeyString(), value);

        var streamProvider = this.GetStreamProvider("provider1");
        var key            = Guid.NewGuid().ToString();
        var streamId       = StreamId.Create("RANDOMDATA", key);
        stream = streamProvider.GetStream<DateTime>(streamId);

        count = value;
        timer = this.RegisterGrainTimer(onTimer, value, new GrainTimerCreationOptions(TimeSpan.Zero, TimeSpan.FromSeconds(1)));

        return key;
    }

    async Task onTimer(int state)
    {
        if (stream == null) return;
        logger.LogInformation("Execute.Item: [{0} - {1} - {2}], Grain timer", RuntimeIdentity, IdentityString, this.GetPrimaryKeyString());
        await stream.OnNextAsync(DateTime.UtcNow);
        if (count-- <= 0)
        {
            await stream.OnCompletedAsync();
            stream = null;
            timer?.Dispose();
        }
    }
}