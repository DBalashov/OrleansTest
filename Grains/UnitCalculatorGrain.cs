using GrainInterfaces;
using Microsoft.Extensions.Logging;
using Orleans;
using Orleans.Placement;
using Orleans.Runtime;
#pragma warning disable CS1998

namespace Grains;

[RandomPlacement]
public class UnitCalculatorGrain : Orleans.Grain, IUnitCalculator
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
        logger.LogInformation("Calculate: [{0}/{1}], Value={2}", RuntimeIdentity, IdentityString, value);

        // var rows = await membershipTable.ReadAll();
        // foreach (var row in rows.Members)
        //     logger.LogInformation("{0}: {1} - {2} - {3} - {4} - {5}", row.Item2, row.Item1.Status, row.Item1.HostName, row.Item1.RoleName, row.Item1.SiloName, row.Item1.StartTime);

        // timer = RegisterTimer(async o =>
        //                       {
        //                           if (Counter++ == 10) timer.Dispose();
        //                           logger.LogInformation("Calculate[P/{3}]: [{0}] [{1}], Value={2}", RuntimeIdentity, IdentityString, value, Counter);
        //                       }, 1, TimeSpan.Zero, TimeSpan.FromSeconds(1));

        return value * 2;
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