using SQLite;

namespace BeatTheBank.Models;


public class GameVault
{
    [PrimaryKey]
    public Guid Id { get; set; }
    public Guid GameId { get; set; }
    public int DollarAmount { get; set; }
    public int Vault { get; set; }
}