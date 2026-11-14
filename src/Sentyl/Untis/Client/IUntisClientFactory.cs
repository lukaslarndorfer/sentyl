namespace Sentyl.Untis.Client;

public interface IUntisClientFactory
{
    public UntisClient Create(UntisCredentials credentials);
}
