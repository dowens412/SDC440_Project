using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using RSVPApp.Models;
using RSVPApp.State;

namespace RSVPApp.Services;

public class EventService
{
    private readonly HttpClient _http;
    private readonly AppState _appState;
    private readonly DatabaseService _db;
    private readonly AuthService _auth;

    public EventService(AppState appState, DatabaseService db, AuthService auth)
    {
        _http = new HttpClient();
        _appState = appState;
        _db = db;
        _auth = auth;
    }

    private async Task EnsureAuthHeaderFromSavedCredsAsync()
    {
        var creds = await _auth.GetSavedCredentialsAsync();
        if (creds == null) throw new Exception("No saved credentials. Please login again.");

        var token = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{creds.Value.Email}:{creds.Value.Password}"));
        _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", token);
    }

    public async Task<List<ApiEventSummary>> GetAllEventsAsync()
    {
        await EnsureAuthHeaderFromSavedCredsAsync();
        var url = $"{_auth.BaseUrl.TrimEnd('/')}/api/events";
        var res = await _http.GetAsync(url);

        if (!res.IsSuccessStatusCode)
            throw new Exception("Failed to load events from web service.");

        var payload = await res.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<List<ApiEventSummary>>(payload, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        }) ?? new List<ApiEventSummary>();
    }

    public async Task<List<ApiEventSummary>> GetHostingEventsAsync()
    {
        if (_appState.CurrentUser == null) return new List<ApiEventSummary>();

        var all = await GetAllEventsAsync();
        return all.Where(e => e.HostUserId == _appState.CurrentUser.Id).ToList();
    }

    public async Task<List<ApiEventSummary>> GetAttendingEventsAsync()
    {
        if (_appState.CurrentUser == null) return new List<ApiEventSummary>();

        
        var all = await GetAllEventsAsync();
        var attending = new List<ApiEventSummary>();

        foreach (var ev in all)
        {
            // local duplicate protection helps too
            if (await _db.HasRsvpedAsync(ev.Id, _appState.CurrentUser.Id))
            {
                attending.Add(ev);
                continue;
            }

            var details = await GetEventDetailsAsync(ev.Id);
            if (details.AttendeeNames.Any(n => string.Equals(n, _appState.CurrentUser.Name, StringComparison.OrdinalIgnoreCase)))
            {
                attending.Add(ev);
            }
        }

        return attending;
    }

    public async Task<ApiEventDetails> GetEventDetailsAsync(string eventId)
    {
        await EnsureAuthHeaderFromSavedCredsAsync();
        var url = $"{_auth.BaseUrl.TrimEnd('/')}/api/events/{eventId}";
        var res = await _http.GetAsync(url);

        if (!res.IsSuccessStatusCode)
            throw new Exception("Failed to load event details.");

        var payload = await res.Content.ReadAsStringAsync();
        var details = JsonSerializer.Deserialize<ApiEventDetails>(payload, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        }) ?? throw new Exception("Invalid event detail response.");

        // Merge local attendee names (offline / duplicate prevention / display)
        var localNames = await _db.GetAttendeeNamesAsync(eventId);
        if (localNames.Count > 0)
        {
            var merged = details.AttendeeNames.Concat(localNames).Distinct().OrderBy(x => x).ToList();
            details = details with { AttendeeNames = merged, CurrentAttendeeCount = Math.Max(details.CurrentAttendeeCount, merged.Count) };
        }

        return details;
    }

    public async Task<string> AddEventAsync(ApiEventCreateRequest req)
    {
        await EnsureAuthHeaderFromSavedCredsAsync();

        var url = $"{_auth.BaseUrl.TrimEnd('/')}/api/events";
        var json = JsonSerializer.Serialize(req);
        var res = await _http.PostAsync(url, new StringContent(json, Encoding.UTF8, "application/json"));

        if (!res.IsSuccessStatusCode)
        {
            var body = await res.Content.ReadAsStringAsync();
            throw new Exception($"Add event failed: {(int)res.StatusCode} {body}");
        }

        // assume API returns created event id as plain text or JSON { id: "..." }
        var payload = await res.Content.ReadAsStringAsync();
        try
        {
            using var doc = JsonDocument.Parse(payload);
            if (doc.RootElement.TryGetProperty("id", out var idProp))
                return idProp.GetString() ?? "";
        }
        catch { /* ignore */ }

        return payload.Trim('"', ' ', '\n', '\r');
    }

    public async Task RSVPAsync(ApiEventDetails ev)
    {
        if (_appState.CurrentUser == null)
            throw new Exception("You must be logged in.");

        // Rule 1: deadline
        if (DateTime.UtcNow > ev.RsvpDeadlineUtc)
            throw new Exception("RSVP deadline has passed.");

        // Rule 2: capacity
        if (ev.CurrentAttendeeCount >= ev.MaxAllowedAttendees)
            throw new Exception("This event is full.");

        // Rule 3: prevent multiple RSVP
        if (await _db.HasRsvpedAsync(ev.Id, _appState.CurrentUser.Id))
            throw new Exception("You already RSVPed for this event.");

        await EnsureAuthHeaderFromSavedCredsAsync();

        // API endpoint suggestion: POST /api/events/{id}/rsvp
        var url = $"{_auth.BaseUrl.TrimEnd('/')}/api/events/{ev.Id}/rsvp";
        var res = await _http.PostAsync(url, content: null);

        if (!res.IsSuccessStatusCode)
        {
            var body = await res.Content.ReadAsStringAsync();
            throw new Exception($"RSVP failed: {(int)res.StatusCode} {body}");
        }

        // Save local attendance to prevent duplicates + show attendee list
        await _db.AddAttendeeAsync(ev.Id, _appState.CurrentUser.Id, _appState.CurrentUser.Name);
    }
}