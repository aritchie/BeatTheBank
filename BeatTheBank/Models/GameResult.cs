namespace BeatTheBank.Models;


public class GameResult
{
    public int Id { get; set; }

    public string PlayerName { get; set; } = String.Empty;

    public DateTime CompletedAt { get; set; }

    public int Status { get; set; }

    public int WinAmount { get; set; }

    public int PotentialAmount { get; set; }

    public int VaultsOpened { get; set; }

    public int TotalRounds { get; set; }

    public int StopVault { get; set; }

    public bool IsJackpot { get; set; }
}
