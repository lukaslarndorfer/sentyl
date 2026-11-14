using System.Net;

using Polly.Timeout;

namespace Sentyl.Untis.Client;

public sealed class UntisCredentialsValidator(IUntisClientFactory factory, ILogger<UntisCredentialsValidator> logger) : IUntisCredentialsValidator
{
    private const int BadCredentialsCode = -8504;
    private const int InvalidSchoolCode = -8500;

    public async Task<UntisCredentialsCheck> CheckAsync(UntisCredentials credentials,
                                                        CancellationToken cancellationToken)
    {
        var client = factory.Create(credentials);
        try
        {
            await client.AuthenticateAsync(cancellationToken);

            logger.LogDebug("Credentials are valid");
            return UntisCredentialsCheck.Valid;
        }
        catch (UntisClientException ex) when (ex.Code == BadCredentialsCode)
        {
            return UntisCredentialsCheck.InvalidCredentials;
        }
        catch (UntisClientException ex) when (ex.Code == InvalidSchoolCode)
        {
            return UntisCredentialsCheck.SchoolNotFound;
        }
        catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            return UntisCredentialsCheck.SchoolNotFound;
        }
        // everything else that is not the caller cancelling
        catch (Exception ex) when (ex is UntisClientException or HttpRequestException or TimeoutRejectedException
                                   && !cancellationToken.IsCancellationRequested)
        {
            logger.LogWarning(ex, "Could not reach Untis for {Host} - {School}", credentials.Host, credentials.School);
            return UntisCredentialsCheck.Unreachable;
        }
    }
}
