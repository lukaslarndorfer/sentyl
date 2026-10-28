namespace Sentyl.Untis;

public sealed class MasterDataProvider(MasterDataLoader masterDataLoader)
{
    private MasterData? _masterData;

    public async Task<MasterData> GetAsync(CancellationToken cancellationToken)
    {
        _masterData ??= await masterDataLoader.LoadAsync(cancellationToken);
        return _masterData;
    }
}
