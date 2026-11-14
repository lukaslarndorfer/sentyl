using System.Net.Http.Json;
using System.Text.Json;

namespace Sentyl.Untis;

public sealed class UntisClient : ITimetableProvider
{
    private const int NotAuthenticatedCode = -8520;
    private readonly HttpClient _httpClient;
    private readonly UntisCredentials _credentials;
    private readonly string _baseUrl;
    private AuthenticateResult? _authenticationState;

    // JsonSerializerDefaults.Web for camel case
    private static readonly JsonSerializerOptions serializerOptions = new(JsonSerializerDefaults.Web);
    private static readonly EmptyParams noParams = new();

    public UntisClient(HttpClient httpClient, UntisCredentials credentials)
    {
        _httpClient = httpClient;
        _credentials = credentials;
        _baseUrl
            = $"https://{_credentials.Host}/WebUntis/jsonrpc.do?school={Uri.EscapeDataString(_credentials.School)}";
    }

    private async Task<TResult> CallAsync<TParams, TResult>(string method, TParams parameters,
                                                            string? sessionId, CancellationToken cancellationToken)
    {
        JsonRpcRequest<TParams> rpcRequest = new("1", method, parameters);

        using HttpRequestMessage request = new(HttpMethod.Post, _baseUrl);
        request.Content = JsonContent.Create(rpcRequest, options: serializerOptions);

        if (sessionId is not null)
        {
            request.Headers.Add("Cookie", $"JSESSIONID={sessionId}");
        }

        using var response = await _httpClient.SendAsync(request, cancellationToken);

        // throws on 4xx or 5xx
        response.EnsureSuccessStatusCode();

        JsonRpcResponse<TResult>? content
            = await response.Content.ReadFromJsonAsync<JsonRpcResponse<TResult>>(serializerOptions, cancellationToken)
              ?? throw new UntisClientException("Content is null");
        if (content.Error != null)
        {
            throw new UntisClientException(content.Error.Code, content.Error.Message);
        }

        return content.Result ?? throw new UntisClientException("Response contained neither result nor error");
    }

    private async Task<TResult> CallAuthenticatedAsync<TParams, TResult>(
        string method, TParams parameters, CancellationToken cancellationToken)
    {
        AuthenticateResult auth = await EnsureAuthenticatedAsync(cancellationToken);
        try
        {
            return await CallAsync<TParams, TResult>(method, parameters, auth.SessionId, cancellationToken);
        }
        catch (UntisClientException ex) when
            (ex.Code == NotAuthenticatedCode) // not authenticated; session may have expired
        {
            auth = await AuthenticateAsync(cancellationToken);

            return await CallAsync<TParams, TResult>(method, parameters, auth.SessionId, cancellationToken);
        }
    }

    public async Task<AuthenticateResult> AuthenticateAsync(CancellationToken cancellationToken)
    {
        AuthenticateParams authenticateParams = new(_credentials.User, _credentials.Password, UntisOptions.ClientName);

        AuthenticateResult result
            = await CallAsync<AuthenticateParams, AuthenticateResult>("authenticate", authenticateParams,
                                                                      sessionId: null, cancellationToken);

        _authenticationState = result;

        return result;
    }

    private async Task<AuthenticateResult> EnsureAuthenticatedAsync(CancellationToken cancellationToken) =>
        _authenticationState ?? await AuthenticateAsync(cancellationToken);

    public async Task<List<TimetableEntry>> GetTimetableAsync(DateOnly start, DateOnly end,
                                                              CancellationToken cancellationToken)
    {
        AuthenticateResult authenticated = await EnsureAuthenticatedAsync(cancellationToken);
        TimetableElement element = new(authenticated.PersonId, authenticated.PersonType);
        TimetableOptions options = new(element, start.ToUntisDate(), end.ToUntisDate());
        TimetableParams timetableParams = new(options);

        return await CallAuthenticatedAsync<TimetableParams, List<TimetableEntry>>("getTimetable", timetableParams,
         cancellationToken);
    }

    public Task<List<SchoolClass>> GetSchoolClassesAsync(CancellationToken cancellationToken) =>
        CallAuthenticatedAsync<EmptyParams, List<SchoolClass>>("getKlassen", noParams, cancellationToken);

    public Task<List<Teacher>> GetTeachersAsync(CancellationToken cancellationToken) =>
        CallAuthenticatedAsync<EmptyParams, List<Teacher>>("getTeachers", noParams, cancellationToken);

    public Task<List<Subject>> GetSubjectsAsync(CancellationToken cancellationToken) =>
        CallAuthenticatedAsync<EmptyParams, List<Subject>>("getSubjects", noParams, cancellationToken);

    public Task<List<Room>> GetRoomsAsync(CancellationToken cancellationToken) =>
        CallAuthenticatedAsync<EmptyParams, List<Room>>("getRooms", noParams, cancellationToken);
}
