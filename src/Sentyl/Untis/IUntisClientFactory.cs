namespace Sentyl.Untis;

public interface IUntisClientFactory
{
    public UntisClient Create(UntisCredentials credentials);
}
