namespace BeatTheBank.Models;


public record PlayerStats
{
    public string PlayerName { get; init; } = String.Empty;
    public int GamesPlayed { get; init; }
    public int TotalWon { get; init; }
    public int PotentialWinnings { get; init; }
    public int JackpotsHit { get; init; }
    public int TimesBusted { get; init; }
    public int TimesStopped { get; init; }
    public int BestSingleGame { get; init; }
    public double AvgVaultsPerGame { get; init; }
    public double WinRate { get; init; }
    public int LongestStreak { get; init; }
    public int MoneyLeftOnTable { get; init; }
    public double RiskScore { get; init; }
}
