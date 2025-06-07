using SQLite;

namespace BeatTheBank.Models;

public class Player
{
    [PrimaryKey]
    public Guid Id { get; set; }
    public string Name { get; set; }
}