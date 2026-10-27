namespace Sentyl.Untis;

public static class UntisDateTimeExtensions
{
    public static int ToUntisDate(this DateOnly date)
        => date.Year * 10000 + date.Month * 100 + date.Day;

    public static DateOnly FromUntisDate(int value)
        => new(value / 10000, value / 100 % 100, value % 100);

    public static int ToUntisTime(this TimeOnly time)
        => time.Hour * 100 + time.Minute;

    public static TimeOnly FromUntisTime(int value)
        => new(value / 100, value % 100);
}
