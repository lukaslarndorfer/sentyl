namespace Sentyl.Untis.Client;

public interface IUntisCredentialsValidator
{
    public Task<UntisCredentialsCheck> CheckAsync(UntisCredentials credentials, CancellationToken cancellationToken);
}
