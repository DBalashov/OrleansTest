using GrainInterfaces;
using Microsoft.Extensions.Logging;
using Orleans.Runtime;

#pragma warning disable CS1998
#pragma warning disable CS8618

namespace Grains;

public class StateGrain : Orleans.Grain<StateGrainExample>, IStateGrain
{
    readonly ILogger        logger;
    readonly IClusterClient client;

    readonly IPersistentState<StateGrainExample> state;

    public StateGrain(ILogger<StateGrain> logger, IClusterClient client, [PersistentState("state")] IPersistentState<StateGrainExample> stateStorage)
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

    public async Task<double> Get()
    {
        return state.State.Value;
    }

    public async Task Set(double value)
    {
        state.State.Value    = value;
        state.State.Modified = DateTime.UtcNow;
        await state.WriteStateAsync();
    }
}

public sealed class StateGrainExample
{
    public double   Value    { get; set; }
    public DateTime Modified { get; set; }
}