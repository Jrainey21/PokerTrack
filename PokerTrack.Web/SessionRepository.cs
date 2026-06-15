using Dapper;
using System.Data;
using PokerTrack.Contracts;

namespace PokerTrack.Web;
//All database access for poker sessions.
//Dapper used for full SQL control
public class SessionRepository(IDbConnection db)
{
    public async Task<IEnumerable<PokerSession>> GetByUserAsync(string userId) =>
        await db.QueryAsync<PokerSession>(
            "SELECT * FROM Sessions WHERE UserId = @UserId ORDER BY SessionDate DESC",
            new { UserId = userId });

    public async Task<PokerSession?> GetByIdAsync(int id, string userId) =>
        await db.QuerySingleOrDefaultAsync<PokerSession>(
            "SELECT * FROM Sessions WHERE Id = @Id AND UserId = @UserId",
            new { Id = id, UserId = userId });
    //LogSession
    public async Task<int> CreateAsync(PokerSession session)
    {
        const string sql = """
            INSERT INTO Sessions (UserId, SessionDate, VenueName, GameType, StakesDescription, BuyInAmount, CashOutAmount, DurationMinutes, Notes)
            OUTPUT INSERTED.Id
            VALUES (@UserId, @SessionDate, @VenueName, @GameType, @StakesDescription, @BuyInAmount, @CashOutAmount, @DurationMinutes, @Notes)
            """;
        return await db.ExecuteScalarAsync<int>(sql, session);
    }
    //EditSession
    public async Task UpdateAsync(PokerSession session)
    {
        const string sql = """
            UPDATE Sessions
            SET SessionDate       = @SessionDate,
                VenueName         = @VenueName,
                GameType          = @GameType,
                StakesDescription = @StakesDescription,
                BuyInAmount       = @BuyInAmount,
                CashOutAmount     = @CashOutAmount,
                DurationMinutes   = @DurationMinutes,
                Notes             = @Notes
            WHERE Id = @Id AND UserId = @UserId
            """;
        await db.ExecuteAsync(sql, session);
    }
    // UserId in WHERE clause prevents a user from deleting another user's session
    public async Task DeleteAsync(int id, string userId) =>
        await db.ExecuteAsync(
            "DELETE FROM Sessions WHERE Id = @Id AND UserId = @UserId",
            new { Id = id, UserId = userId });

    // Analytics are computed by the Worker Service after each CDC event —
    // this table is read-only.
    public async Task<SessionAnalytics?> GetAnalyticsAsync(string userId) =>
        await db.QuerySingleOrDefaultAsync<SessionAnalytics>(
            "SELECT * FROM Analytics WHERE UserId = @UserId",
            new { UserId = userId });
}
