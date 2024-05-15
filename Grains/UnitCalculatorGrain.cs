using GrainInterfaces;
using Microsoft.Extensions.Logging;
using Orleans.Runtime;

#pragma warning disable CS1998

namespace Grains;

public class UnitCalculatorGrain(ILogger<UnitCalculatorGrain> logger,
                                 IClusterClient               client,
                                 IMembershipTable             membershipTable) : Grain, IUnitCalculator, IRemindable
{
    public async Task<double> Calculate(int value)
    {
        logger.LogInformation("Calculate: [{0} - {1} - {2}], Value={3}", RuntimeIdentity, IdentityString, this.GetPrimaryKeyString(), value);
        if (value == 1)
        {
            // RegisterTimer(async a => { logger.LogInformation("Calculate: [{0} - {1} - {2}], Grain timer", RuntimeIdentity, IdentityString, this.GetPrimaryKeyString()); },
            //               null!, TimeSpan.Zero, TimeSpan.FromSeconds(3));
            await this.RegisterOrUpdateReminder("rem-" + this.GetPrimaryKeyString(), TimeSpan.Zero, TimeSpan.FromSeconds(60));
        }
        
        return value * 2;
    }
    
    public async IAsyncEnumerable<string> Powers(int value)
    {
        while (value < 256)
        {
            value *= 2;
            await Task.Delay(100);
            yield return value.ToString();
        }
    }
    
    public async Task<double> Multi(Parms p) => p.X * p.Y;
    
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
    
    public async Task ReceiveReminder(string reminderName, TickStatus status)
    {
        var rem = await this.GetReminder("rem-" + this.GetPrimaryKeyString());
        if (rem != null)
        {
            await this.UnregisterReminder(rem);
            logger.LogInformation("Reminder unregistered: [{0} - {1} - {2}], Grain timer", RuntimeIdentity, IdentityString, this.GetPrimaryKeyString());
        }
    }
}