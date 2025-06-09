using SQLite;

namespace BeatTheBank.Models;


public class Game
{
    [PrimaryKey]
    public Guid Id { get; set; }
    public Guid PlayerId { get; set; }
    
    // jackpot if vaults == stopvault and amount == 1000000
    public int Vaults { get; set; }
    
    // lost amounts sum amount where same game and vault > stopvault
    public int StopVault { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}