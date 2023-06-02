using GrainInterfaces;
using Microsoft.Extensions.Logging;
using Orleans.Placement;

#pragma warning disable CS1998

namespace Grains;

[RandomPlacement, Immutable]
public class UnitCalculatorGrain : Grain, IUnitCalculator
{
    readonly ILogger          logger;
    readonly IClusterClient   client;
    readonly IMembershipTable membershipTable;

    public UnitCalculatorGrain(ILogger<UnitCalculatorGrain> logger, IClusterClient client, IMembershipTable membershipTable)
    {
        this.client          = client;
        this.logger          = logger;
        this.membershipTable = membershipTable;
    }

    public async Task<double> Calculate(int value)
    {
        logger.LogInformation("Calculate: [{0} - {1} - {2}], Value={3}", RuntimeIdentity, IdentityString, this.GetPrimaryKeyString(), value);
        if (value == 1)
        {
            RegisterTimer(async a => { logger.LogInformation("Calculate: [{0} - {1} - {2}], Grain timer", RuntimeIdentity, IdentityString, this.GetPrimaryKeyString()); },
                          null, TimeSpan.Zero, TimeSpan.FromSeconds(3));
        }

        return value * 2;
    }

    public async Task<double> Multi(Parms p)
    {
        return p.X * p.Y;
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
}