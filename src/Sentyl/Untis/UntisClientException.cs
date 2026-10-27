namespace Sentyl.Untis;

public sealed class UntisClientException : System.Exception
{
    public int? Code { get; }

    public UntisClientException(string message) : base(message) { }
    public UntisClientException(int code, string message) : base($"{message}; (error code: {code})")
    {
        Code = code;
    }


}
