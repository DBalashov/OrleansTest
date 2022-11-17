using GrainInterfaces;
using Microsoft.Extensions.Logging;
using Orleans;

namespace Grains;

public class UnitCalculatorGrain : Orleans.Grain, IUnitCalculator
{
    readonly ILogger        logger;
    readonly IClusterClient client;

    public UnitCalculatorGrain(ILogger<UnitCalculatorGrain> logger, IClusterClient client)
    {
        this.client = client;
        this.logger = logger;
    }

    public async Task<double> Calculate(int value)
    {
        logger.LogInformation("Calculate: [{0}] [{1}], Value={2}", RuntimeIdentity, IdentityString, value);
        // timer = RegisterTimer(async o =>
        //                       {
        //                           if (Counter++ == 10) timer.Dispose();
        //                           logger.LogInformation("Calculate[P/{3}]: [{0}] [{1}], Value={2}", RuntimeIdentity, IdentityString, value, Counter);
        //                       }, 1, TimeSpan.Zero, TimeSpan.FromSeconds(1));

        return value * 2;
    }

    public override Task OnActivateAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Activate: {0}", IdentityString);
        return base.OnActivateAsync(cancellationToken);
    }

    public override Task OnDeactivateAsync(DeactivationReason reason, CancellationToken cancellationToken)
    {
        logger.LogInformation("Deactivate: {0} - {1}", IdentityString, reason.ReasonCode.ToString());
        return base.OnDeactivateAsync(reason, cancellationToken);
    }
}