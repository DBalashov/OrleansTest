using Microsoft.Extensions.Logging;

namespace Grains;

public abstract class BaseStateGrain<STATE>(ILogger logger, IClusterClient client) : Grain<STATE>
{
    protected readonly ILogger        Logger = logger;
    protected readonly IClusterClient Client = client;

    public override Task OnActivateAsync(CancellationToken cancellationToken)
    {
        Logger.LogInformation("Activate: {0} / {1}", RuntimeIdentity, IdentityString);
        return base.OnActivateAsync(cancellationToken);
    }

    public override Task OnDeactivateAsync(DeactivationReason reason, CancellationToken cancellationToken)
    {
        Logger.LogInformation("Deactivate: {0} / {1}: {2}", RuntimeIdentity, IdentityString, reason.ReasonCode.ToString());
        return base.OnDeactivateAsync(reason, cancellationToken);
    }
}