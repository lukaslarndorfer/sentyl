using System.Text.Json.Serialization;

namespace Sentyl.Untis;

// params can vary per request, hence the generic TParams 
public sealed record JsonRpcRequest<TParams>(
    string Id,
    string Method,
    TParams Params,
    [property: JsonPropertyName("jsonrpc")] string JsonRpcVersion = "2.0");

public sealed record JsonRpcError(
    int Code,
    string Message
);


// the result type differs per request, hence TResult is used 
public sealed record JsonRpcResponse<TResult>(
    string Id,
    TResult? Result,
    JsonRpcError? Error,
    [property: JsonPropertyName("jsonrpc")] string JsonRpcVersion = "2.0"
);

public sealed record EmptyParams();
