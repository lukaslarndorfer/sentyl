namespace Sentyl.Untis;

public sealed record Room(int Id,
    string Name,
    string LongName // can be equal to Name, but can also be a more descriptive name
    );
