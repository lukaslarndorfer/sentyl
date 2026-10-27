using System.Net.Http.Json;
using System.Text.Json;

using Microsoft.Extensions.Options;

namespace Sentyl.Untis;

public sealed class UntisClient : ITimetableProvider
{
    private const int NotAuthenticatedCode = -8520;
    private readonly HttpClient _httpClient;
    private readonly UntisOptions _options;
    private readonly string _baseUrl;
    private AuthenticateResult? _authenticationState;

    // JsonSerializerDefaults.Web for camel case
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);
    private static readonly EmptyParams NoParams = new();

    public UntisClient(HttpClient httpClient, IOptions<UntisOptions> options)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _baseUrl = $"https://{_options.Host}/WebUntis/jsonrpc.do?school={Uri.EscapeDataString(_options.School)}";
    }

    private async Task<TResult> CallAsync<TParams, TResult>(string method, TParams parameters, CancellationToken cancellationToken)
    {
        JsonRpcRequest<TParams> request = new("1", method, parameters);

        using HttpResponseMessage response =
        await _httpClient.PostAsJsonAsync(_baseUrl, request, SerializerOptions, cancellationToken);

        // throws on 4xx or 5xx
        response.EnsureSuccessStatusCode();

        JsonRpcResponse<TResult>? content = await response.Content.ReadFromJsonAsync<JsonRpcResponse<TResult>>(SerializerOptions, cancellationToken)
        ?? throw new UntisClientException("Content is null");
        if (content.Error != null)
        {
            throw new UntisClientException(content.Error.Code, content.Error.Message);
        }

        return content.Result ?? throw new UntisClientException("Response contained neither result nor error");
    }

    private async Task<TResult> CallAuthenticatedAsync<TParams, TResult>(string method, TParams parameters, CancellationToken cancellationToken)
    {
        await EnsureAuthenticatedAsync(cancellationToken);
        try
        {
            return await CallAsync<TParams, TResult>(method, parameters, cancellationToken);
        }
        catch (UntisClientException ex) when (ex.Code == NotAuthenticatedCode) // not authenticated; session may have expired
        {
            await AuthenticateAsync(cancellationToken);
            return await CallAsync<TParams, TResult>(method, parameters, cancellationToken);
        }
    }

    public async Task<AuthenticateResult> AuthenticateAsync(CancellationToken cancellationToken)
    {
        AuthenticateParams authenticateParams = new(_options.User, _options.Password, UntisOptions.ClientName);

        AuthenticateResult result = await CallAsync<AuthenticateParams, AuthenticateResult>("authenticate", authenticateParams, cancellationToken);
        _authenticationState = result;
        return result;
    }

    private async Task<AuthenticateResult> EnsureAuthenticatedAsync(CancellationToken cancellationToken)
         => _authenticationState ?? await AuthenticateAsync(cancellationToken);

    public async Task<List<TimetableEntry>> GetTimetableAsync(DateOnly start, DateOnly end, CancellationToken cancellationToken)
    {
        AuthenticateResult authenticated = await EnsureAuthenticatedAsync(cancellationToken);
        TimetableElement element = new(authenticated.PersonId, authenticated.PersonType);
        TimetableOptions options = new(element, start.ToUntisDate(), end.ToUntisDate());
        TimetableParams timetableParams = new(options);

        return await CallAuthenticatedAsync<TimetableParams, List<TimetableEntry>>("getTimetable", timetableParams, cancellationToken);
    }

    public Task<List<SchoolClass>> GetSchoolClassesAsync(CancellationToken cancellationToken)
        => CallAuthenticatedAsync<EmptyParams, List<SchoolClass>>("getKlassen", NoParams, cancellationToken);

    public Task<List<Teacher>> GetTeachersAsync(CancellationToken cancellationToken)
        => CallAuthenticatedAsync<EmptyParams, List<Teacher>>("getTeachers", NoParams, cancellationToken);

    public Task<List<Subject>> GetSubjectsAsync(CancellationToken cancellationToken)
        => CallAuthenticatedAsync<EmptyParams, List<Subject>>("getSubjects", NoParams, cancellationToken);

    public Task<List<Room>> GetRoomsAsync(CancellationToken cancellationToken)
        => CallAuthenticatedAsync<EmptyParams, List<Room>>("getRooms", NoParams, cancellationToken);
}
