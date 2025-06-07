using SQLite;

namespace BeatTheBank.Models;


public class Game
{
    [PrimaryKey]
    public Guid Id { get; set; }
    public Guid PlayerId { get; set; }
    public int Vaults { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}