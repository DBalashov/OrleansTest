using GrainInterfaces;
using Microsoft.Extensions.Logging;

#pragma warning disable CS1998
#pragma warning disable CS8618

namespace Grains;

public class StateStringAccumulatorGrain : BaseStateGrain<StateStringAccumulator>, IStateStringAccumulatorGrain
{
    public StateStringAccumulatorGrain(ILogger<StateStringAccumulatorGrain> logger, IClusterClient client) : base(logger, client)
    {
    }

    public async Task<string> Add(string value)
    {
        Logger.LogInformation("Set: {0} / {1}: {2}", RuntimeIdentity, IdentityString, value);
        
        if (State.Acc == null!) State.Acc = "";
        State.Acc      += value + ",";
        State.Modified =  DateTime.UtcNow;

        await WriteStateAsync();
        return State.Acc;
    }
}

public sealed class StateStringAccumulator
{
    public string   Acc      { get; set; }
    public DateTime Modified { get; set; }
}