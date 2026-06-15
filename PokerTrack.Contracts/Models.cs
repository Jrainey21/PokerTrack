namespace PokerTrack.Contracts;

public class PokerSession
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;   // Entra ID object ID
    public DateTime SessionDate { get; set; }
    public string VenueName { get; set; } = string.Empty;
    public string GameType { get; set; } = "Cash";        // Cash | Tournament
    public string StakesDescription { get; set; } = string.Empty;  // e.g. "1/2 NL" or "$25 min" for BJ
    public decimal BuyInAmount { get; set; }
    public decimal CashOutAmount { get; set; }
    public int DurationMinutes { get; set; }
    public string? Notes { get; set; }

    
    public decimal Profit => CashOutAmount - BuyInAmount;
    public decimal HourlyRate => DurationMinutes > 0
        ? Profit / (DurationMinutes / 60m)
        : 0;
}

public class SessionAnalytics
{
    public string UserId { get; set; } = string.Empty;
    public int TotalSessions { get; set; }
    public decimal TotalProfit { get; set; }
    public decimal AvgProfitPerSession { get; set; }
    public decimal AvgHourlyRate { get; set; }
    public int WinningSessions { get; set; }
    public int LosingSessions { get; set; }
    public decimal WinRate { get; set; }           // divide by TotalSessions to get percentage
    public int CurrentStreak { get; set; }         // positive = wins, negative = losses
    public int LongestWinStreak { get; set; }
    public decimal BiggestWin { get; set; }
    public decimal BiggestLoss { get; set; }
    public DateTime LastUpdated { get; set; }
}
