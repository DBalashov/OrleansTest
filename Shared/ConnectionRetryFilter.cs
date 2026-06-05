namespace Shared;

public class ConnectionRetryFilter(ILogger<ConnectionRetryFilter> logger) : IClientConnectionRetryFilter
{
    public async Task<bool> ShouldRetryConnectionAttempt(Exception exception, CancellationToken cancellationToken)
    {
        logger.LogError("Can't connect, retry in 300 ms: {0}", exception.Message);
        await Task.Delay(300, cancellationToken);
        return !cancellationToken.IsCancellationRequested;
    }
}