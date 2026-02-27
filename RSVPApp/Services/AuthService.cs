using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using RSVPApp.Models;
using RSVPApp.State;

namespace RSVPApp.Services;

public class AuthService
{
    private readonly HttpClient _http;
    private readonly AppState _appState;
    private readonly ICredentialStore _creds;

    public string BaseUrl { get; set; } = "http://192.168.1.58:5049";

    public AuthService(AppState appState, ICredentialStore creds)
    {
        _http = new HttpClient();
        _appState = appState;
        _creds = creds;
    }

    public async Task<ApiUserResponse> RegisterAsync(ApiUserRegisterRequest req)
    {
        var url = $"{BaseUrl.TrimEnd('/')}/api/users/register";
        var json = JsonSerializer.Serialize(req);
        var res = await _http.PostAsync(url, new StringContent(json, Encoding.UTF8, "application/json"));

        if (!res.IsSuccessStatusCode)
        {
            var body = await res.Content.ReadAsStringAsync();
            throw new Exception($"Register failed: {(int)res.StatusCode} {res.ReasonPhrase} {body}");
        }

        var payload = await res.Content.ReadAsStringAsync();
        var user = JsonSerializer.Deserialize<ApiUserResponse>(payload, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        }) ?? throw new Exception("Register failed: invalid response.");

        return user;
    }

    public async Task<ApiUserResponse> LoginAsync(string email, string password)
    {
        // Basic Auth header
        var token = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{email}:{password}"));
        _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", token);

        var url = $"{BaseUrl.TrimEnd('/')}/api/auth/me";
        var res = await _http.GetAsync(url);

        if (!res.IsSuccessStatusCode)
            throw new Exception("Login failed: invalid email/password (or the web service is not running).");

        var payload = await res.Content.ReadAsStringAsync();
        var user = JsonSerializer.Deserialize<ApiUserResponse>(payload, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        }) ?? throw new Exception("Login failed: invalid response.");

        // Save “session”
        _appState.SetUser(user);

        await _creds.SaveAsync(email, password);

        return user;
    }

    public Task<(string Email, string Password)?> GetSavedCredentialsAsync()
        => _creds.GetAsync();

    public Task ClearSavedCredentialsAsync()
        => _creds.ClearAsync();
}