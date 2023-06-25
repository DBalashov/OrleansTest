using GrainInterfaces;
using Microsoft.Extensions.Logging;
using Orleans.Runtime;

#pragma warning disable CS1998
#pragma warning disable CS8618

namespace Grains;

public class StateStringAccumulatorGrain : Grain<StateStringAccumulator>, IStateStringAccumulatorGrain
{
    readonly ILogger        logger;
    readonly IClusterClient client;

    readonly IPersistentState<StateStringAccumulator> state;

    public StateStringAccumulatorGrain(ILogger<StateStringAccumulatorGrain> logger, IClusterClient client, [PersistentState("state")] IPersistentState<StateStringAccumulator> stateStorage)
    {
        this.client = client;
        this.logger = logger;
        state       = stateStorage;
    }

    public override void Participate(IGrainLifecycle lifecycle)
    {
        logger.LogInformation("Initialize: {0} / {1}", RuntimeIdentity, IdentityString);
        base.Participate(lifecycle);
    }

    public override Task OnActivateAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Activate: {0} / {1}", RuntimeIdentity, IdentityString);
        return base.OnActivateAsync(cancellationToken);
    }

    public override Task OnDeactivateAsync(DeactivationReason reason, CancellationToken cancellationToken)
    {
        logger.LogInformation("Deactivate: {0} / {1}: {2}", RuntimeIdentity, IdentityString, reason.ReasonCode.ToString());
        return base.OnDeactivateAsync(reason, cancellationToken);
    }

    public async Task<string> Add(string value)
    {
        logger.LogInformation("Set: {0} / {1}: {2}", RuntimeIdentity, IdentityString, value);
        if (state.State.Acc == null!) state.State.Acc = "";
        state.State.Acc      += value + ",";
        state.State.Modified =  DateTime.UtcNow;
        await state.WriteStateAsync();
        return state.State.Acc;
    }
}

public sealed class StateStringAccumulator
{
    public string   Acc      { get; set; }
    public DateTime Modified { get; set; }
}