using SQLite;
using RSVPApp.Models;

namespace RSVPApp.Services;

public class DatabaseService
{
    private SQLiteAsyncConnection? _db;

    private async Task<SQLiteAsyncConnection> GetDbAsync()
    {
        if (_db != null) return _db;

        var dbPath = Path.Combine(FileSystem.AppDataDirectory, "rsvpapp.db3");
        _db = new SQLiteAsyncConnection(dbPath);

        await _db.CreateTableAsync<DbUser>();
        await _db.CreateTableAsync<DbEvent>();

        return _db;
    }

    // ---------- Users ----------
    public async Task<int> CreateUserAsync(DbUser user)
    {
        var db = await GetDbAsync();
        return await db.InsertAsync(user);
    }

    public async Task<DbUser?> GetUserByEmailAsync(string email)
    {
        var db = await GetDbAsync();
        var normalized = (email ?? "").Trim().ToLowerInvariant();

        return await db.Table<DbUser>()
            .Where(u => u.Email.ToLower() == normalized)
            .FirstOrDefaultAsync();
    }

    // ---------- Events ----------
    public async Task<List<DbEvent>> GetAllEventsAsync()
    {
        var db = await GetDbAsync();
        return await db.Table<DbEvent>()
            .OrderBy(e => e.EventDateTimeIso)
            .ToListAsync();
    }

    public async Task<int> CreateEventAsync(DbEvent ev)
    {
        var db = await GetDbAsync();
        return await db.InsertAsync(ev);
    }

    // ---------- Seed data (for Week 3 demo) ----------
    public async Task SeedIfEmptyAsync()
    {
        var db = await GetDbAsync();

        var userCount = await db.Table<DbUser>().CountAsync();
        if (userCount == 0)
        {
            // Seed the Week 2 hard-coded user so login still works
            await db.InsertAsync(new DbUser
            {
                FirstName = "Test",
                LastName = "User",
                Email = "test@example.com",
                Password = "Password123"
            });
        }

        var eventCount = await db.Table<DbEvent>().CountAsync();
        if (eventCount == 0)
        {
            var host = await GetUserByEmailAsync("test@example.com");
            var hostId = host?.UserId ?? 1;

            await db.InsertAsync(new DbEvent
            {
                Title = "Garage Gym Meetup",
                Description = "Lift + hangout.",
                Location = "123 Main St, Richmond, VA",
                EventDateTimeIso = DateTime.Now.AddDays(7).Date.AddHours(18).ToString("o"),
                HostUserId = hostId
            });

            await db.InsertAsync(new DbEvent
            {
                Title = "Nutrition Q&A",
                Description = "Bring your questions.",
                Location = "45 Fitness Ave, Charlotte, NC",
                EventDateTimeIso = DateTime.Now.AddDays(3).Date.AddHours(19).ToString("o"),
                HostUserId = hostId
            });

            await db.InsertAsync(new DbEvent
            {
                Title = "Posing Practice",
                Description = "Mandatory poses + feedback.",
                Location = "9 Stage Rd, Columbia, SC",
                EventDateTimeIso = DateTime.Now.AddDays(10).Date.AddHours(16).ToString("o"),
                HostUserId = hostId
            });
        }
    }
}
