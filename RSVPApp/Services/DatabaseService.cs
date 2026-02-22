using SQLite;
using RSVPApp.Models;

namespace RSVPApp.Services;

public class DatabaseService
{
    private SQLiteAsyncConnection? _db;

    public async Task InitAsync()
    {
        if (_db != null) return;

        var dbPath = Path.Combine(FileSystem.AppDataDirectory, "rsvpapp.db3");
        _db = new SQLiteAsyncConnection(dbPath);

        // These tables already exist in your project (per screenshot).
        // If you renamed them, update the CreateTable calls.
        await _db.CreateTableAsync<DbUser>();
        await _db.CreateTableAsync<DbEvent>();

        // NEW
        await _db.CreateTableAsync<DbEventAttendee>();
    }

    private async Task<SQLiteAsyncConnection> GetDbAsync()
    {
        await InitAsync();
        return _db!;
    }

    // Attendees (local)
    public async Task<bool> HasRsvpedAsync(string eventId, string userId)
    {
        var db = await GetDbAsync();
        var existing = await db.Table<DbEventAttendee>()
            .Where(x => x.EventId == eventId && x.UserId == userId)
            .FirstOrDefaultAsync();

        return existing != null;
    }

    public async Task AddAttendeeAsync(string eventId, string userId, string userName)
    {
        var db = await GetDbAsync();

        // prevent duplicates locally
        if (await HasRsvpedAsync(eventId, userId)) return;

        await db.InsertAsync(new DbEventAttendee
        {
            EventId = eventId,
            UserId = userId,
            UserName = userName
        });
    }

    public async Task<List<string>> GetAttendeeNamesAsync(string eventId)
    {
        var db = await GetDbAsync();
        var rows = await db.Table<DbEventAttendee>()
            .Where(x => x.EventId == eventId)
            .ToListAsync();

        return rows.Select(r => r.UserName).Distinct().OrderBy(x => x).ToList();
    }

    // Local event cache helpers (optional but useful)
    public async Task UpsertEventsAsync(IEnumerable<DbEvent> events)
    {
        var db = await GetDbAsync();
        foreach (var ev in events)
        {
            // If DbEvent has an Id string, you can upsert by Id.
            // If your DbEvent uses int PK, keep it simple and just InsertAll after clearing.
            await db.InsertOrReplaceAsync(ev);
        }
    }

    public async Task<List<DbEvent>> GetAllLocalEventsAsync()
    {
        var db = await GetDbAsync();
        return await db.Table<DbEvent>().ToListAsync();
    }
}