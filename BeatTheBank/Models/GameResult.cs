using SQLite;

namespace BeatTheBank.Models;


[Table("GameResults")]
public class GameResult
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    [Indexed]
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
