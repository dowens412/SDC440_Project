using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Swagger in Development
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// In-memory stores (Week 4 demo)
var users = new ConcurrentDictionary<string, ApiUser>(StringComparer.OrdinalIgnoreCase);
var eventsStore = new ConcurrentDictionary<string, ApiEvent>(StringComparer.OrdinalIgnoreCase);

SeedDemoData();

// ------------------------
// Endpoints
// ------------------------

// Register (no auth)
app.MapPost("/api/users/register", ([FromBody] ApiUserRegisterRequest req) =>
{
    if (string.IsNullOrWhiteSpace(req.Name) ||
        string.IsNullOrWhiteSpace(req.Email) ||
        string.IsNullOrWhiteSpace(req.Password) ||
        string.IsNullOrWhiteSpace(req.MobilePhone))
        return Results.BadRequest("All fields are required.");

    if (users.ContainsKey(req.Email))
        return Results.BadRequest("Email already registered.");

    var user = new ApiUser
    {
        Id = NewId(),
        Name = req.Name.Trim(),
        Email = req.Email.Trim(),
        MobilePhone = req.MobilePhone.Trim(),
        PasswordHash = HashPassword(req.Password)
    };

    users[user.Email] = user;

    return Results.Ok(new ApiUserDto(user.Id, user.Name, user.Email, user.MobilePhone));
});

// Login check (Basic Auth) - return current user info
app.MapGet("/api/auth/me", (HttpRequest req) =>
{
    if (!TryGetBasicUser(req, out var user, out var error))
        return Results.Unauthorized();

    return Results.Ok(new ApiUserDto(user!.Id, user.Name, user.Email, user.MobilePhone));
});

// Get events (filter: All / Attending / Hosting)
app.MapGet("/api/events", (HttpRequest req, string filter = "All") =>
{
    if (!TryGetBasicUser(req, out var user, out var error))
        return Results.Unauthorized();

    var list = eventsStore.Values.ToList();

    filter = (filter ?? "All").Trim();

    if (filter.Equals("Hosting", StringComparison.OrdinalIgnoreCase))
    {
        list = list.Where(e => e.HostUserId == user!.Id).ToList();
    }
    else if (filter.Equals("Attending", StringComparison.OrdinalIgnoreCase))
    {
        list = list.Where(e => e.AttendeeUserIds.Contains(user!.Id)).ToList();
    }

    var summaries = list
        .OrderBy(e => e.EventDateTimeUtc)
        .Select(e => new ApiEventSummary(
            e.Id,
            e.HostName,
            e.EventName,
            e.EventAddress,
            e.EventDateTimeUtc,
            e.RsvpDeadlineUtc,
            e.MaxAllowedAttendees,
            e.AttendeeUserIds.Count
        ))
        .ToList();

    return Results.Ok(summaries);
});

// Add event (must be logged in)
app.MapPost("/api/events", (HttpRequest req, [FromBody] ApiEventCreateRequest body) =>
{
    if (!TryGetBasicUser(req, out var user, out var error))
        return Results.Unauthorized();

    if (string.IsNullOrWhiteSpace(body.HostName) ||
        string.IsNullOrWhiteSpace(body.EventName) ||
        string.IsNullOrWhiteSpace(body.EventAddress) ||
        body.MaxAllowedAttendees <= 0)
        return Results.BadRequest("Invalid event data.");

    var id = NewId();

    var ev = new ApiEvent
    {
        Id = id,
        HostUserId = user!.Id,
        HostName = body.HostName.Trim(),
        EventName = body.EventName.Trim(),
        EventAddress = body.EventAddress.Trim(),
        MaxAllowedAttendees = body.MaxAllowedAttendees,
        EventDateTimeUtc = body.EventDateTimeUtc,
        RsvpDeadlineUtc = body.RsvpDeadlineUtc
    };

    eventsStore[id] = ev;

    return Results.Ok(new { id });
});

// Event details (must be logged in)
app.MapGet("/api/events/{id}", (HttpRequest req, string id) =>
{
    if (!TryGetBasicUser(req, out var user, out var error))
        return Results.Unauthorized();

    if (!eventsStore.TryGetValue(id, out var ev))
        return Results.NotFound("Event not found.");

    // attendee names
    var attendeeNames = ev.AttendeeUserIds
        .Select(uid => users.Values.FirstOrDefault(u => u.Id == uid)?.Name)
        .Where(n => !string.IsNullOrWhiteSpace(n))
        .ToList()!;

    var details = new ApiEventDetails(
        ev.Id,
        ev.HostName,
        ev.EventName,
        ev.EventAddress,
        ev.MaxAllowedAttendees,
        ev.AttendeeUserIds.Count,
        ev.EventDateTimeUtc,
        ev.RsvpDeadlineUtc,
        attendeeNames
    );

    return Results.Ok(details);
});

// RSVP (must be logged in)
app.MapPost("/api/events/{id}/rsvp", (HttpRequest req, string id) =>
{
    if (!TryGetBasicUser(req, out var user, out var error))
        return Results.Unauthorized();

    if (!eventsStore.TryGetValue(id, out var ev))
        return Results.NotFound("Event not found.");

    // Deadline check
    if (DateTime.UtcNow > ev.RsvpDeadlineUtc)
        return Results.BadRequest("RSVP deadline has passed.");

    // Prevent duplicates
    if (ev.AttendeeUserIds.Contains(user!.Id))
        return Results.BadRequest("You already RSVPed for this event.");

    // Capacity check
    if (ev.AttendeeUserIds.Count >= ev.MaxAllowedAttendees)
        return Results.BadRequest("Event is full.");

    ev.AttendeeUserIds.Add(user.Id);

    return Results.Ok("RSVP confirmed.");
});

app.Run();


// ------------------------
// Helper functions
// ------------------------

bool TryGetBasicUser(HttpRequest req, out ApiUser? user, out string? error)
{
    user = null;
    error = null;

    if (!req.Headers.TryGetValue("Authorization", out var authHeader))
        return false;

    var header = authHeader.ToString();
    if (!header.StartsWith("Basic ", StringComparison.OrdinalIgnoreCase))
        return false;

    var encoded = header["Basic ".Length..].Trim();
    string decoded;

    try
    {
        decoded = Encoding.UTF8.GetString(Convert.FromBase64String(encoded));
    }
    catch
    {
        return false;
    }

    var parts = decoded.Split(':', 2);
    if (parts.Length != 2) return false;

    var email = parts[0];
    var pass = parts[1];

    if (!users.TryGetValue(email, out var found))
        return false;

    if (!SlowEquals(found.PasswordHash, HashPassword(pass)))
        return false;

    user = found;
    return true;
}

string NewId() => Guid.NewGuid().ToString("N");

string HashPassword(string password)
{
    // simple hash for class project (not production)
    using var sha = SHA256.Create();
    var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(password ?? ""));
    return Convert.ToHexString(bytes);
}

bool SlowEquals(string a, string b)
{
    // constant-time-ish compare
    if (a.Length != b.Length) return false;
    var diff = 0;
    for (int i = 0; i < a.Length; i++)
        diff |= a[i] ^ b[i];
    return diff == 0;
}

void SeedDemoData()
{
    // Seed one user
    var demo = new ApiUser
    {
        Id = NewId(),
        Name = "Demo User",
        Email = "demo@example.com",
        MobilePhone = "5551112222",
        PasswordHash = HashPassword("password")
    };
    users[demo.Email] = demo;

    // Seed one event
    var ev = new ApiEvent
    {
        Id = NewId(),
        HostUserId = demo.Id,
        HostName = "Demo User",
        EventName = "Welcome Event",
        EventAddress = "123 Main St",
        MaxAllowedAttendees = 10,
        EventDateTimeUtc = DateTime.UtcNow.AddDays(7),
        RsvpDeadlineUtc = DateTime.UtcNow.AddDays(5)
    };
    eventsStore[ev.Id] = ev;
}


// ------------------------
// Models (must be AFTER app.Run in Minimal API)
// ------------------------

record ApiUserRegisterRequest(string Name, string Email, string Password, string MobilePhone);
record ApiUserDto(string Id, string Name, string Email, string MobilePhone);

record ApiEventCreateRequest(
    string HostUserId,
    string HostName,
    string EventName,
    string EventAddress,
    int MaxAllowedAttendees,
    DateTime EventDateTimeUtc,
    DateTime RsvpDeadlineUtc
);

record ApiEventSummary(
    string Id,
    string HostName,
    string EventName,
    string EventAddress,
    DateTime EventDateTimeUtc,
    DateTime RsvpDeadlineUtc,
    int MaxAllowedAttendees,
    int CurrentAttendeeCount
);

record ApiEventDetails(
    string Id,
    string HostName,
    string EventName,
    string EventAddress,
    int MaxAllowedAttendees,
    int CurrentAttendeeCount,
    DateTime EventDateTimeUtc,
    DateTime RsvpDeadlineUtc,
    List<string> AttendeeNames
);

class ApiUser
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public string Email { get; set; } = "";
    public string MobilePhone { get; set; } = "";
    public string PasswordHash { get; set; } = "";
}

class ApiEvent
{
    public string Id { get; set; } = "";
    public string HostUserId { get; set; } = "";
    public string HostName { get; set; } = "";
    public string EventName { get; set; } = "";
    public string EventAddress { get; set; } = "";
    public int MaxAllowedAttendees { get; set; }
    public DateTime EventDateTimeUtc { get; set; }
    public DateTime RsvpDeadlineUtc { get; set; }

    public HashSet<string> AttendeeUserIds { get; } = new();
}