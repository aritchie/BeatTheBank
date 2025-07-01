using BeatTheBank.Models;

namespace BeatTheBank.Services;

public class GameContext(
    ILogger<GameContext> logger,
    AppSqliteConnection data
)
{
    readonly Random randomizer = new();
    readonly List<int> vaultAmounts = new();
    public IReadOnlyList<int> VaultAmounts { get; }
    
    public int CurrentVault { get; private set; }
    public int CurrentAmount { get; private set; }

    public void Start(Guid PlayerId)
    {
        this.CurrentVault = 1;
        this.CurrentAmount = 0;
        var vaults = this.randomizer.Next(2, 15);
        var jackpot = this.randomizer.Next(1, 40) == 39; // 1 in 40 chance

        for (var i = 0; i < vaults; i++)
        {
            var amount = this.randomizer.Next(50, 1000);
            if (i == vaults - 1)
            {
                amount = jackpot 
                    ? Constants.JackpotAmount
                    : Constants.LoseAmount;
            }
            this.vaultAmounts.Add(amount);
        }
    }

    public void Stop()
    {
        var game = new Game
        {
            Id = Guid.NewGuid(),
            PlayerId = Guid.NewGuid(), // TODO: set player id
            StopVault = this.CurrentVault,
            CreatedAt = DateTimeOffset.UtcNow
        };

        for (var i = 0; i < this.CurrentVault; i++)
        {
            var vault = new GameVault
            {
                Vault = i + 1,
                DollarAmount = this.vaultAmounts[i],
                GameId = game.Id
            };
        }
        // TODO: save game and vaults to database
    }
    
    public int NextRound()
    {
        // TODO: amount or loss
        this.CurrentVault++;
        if (this.CurrentVault > this.vaultAmounts.Count - 1)
        {
            // no more rounds
            return Constants.LoseAmount;
        }

        this.CurrentAmount = this.VaultAmounts[this.CurrentVault];
    }
}

// int GetNextAmount()
// {
//     if (this.amounts == null)
//     {
//         this.amounts = new();
//         this.amounts.AddRange(Enumerable.Repeat(50, 10));
//         this.amounts.AddRange(Enumerable.Repeat(100, 25));
//         this.amounts.AddRange(Enumerable.Repeat(200, 25));
//         this.amounts.AddRange(Enumerable.Repeat(250, 25));
//         this.amounts.AddRange(Enumerable.Repeat(300, 25));
//         this.amounts.AddRange(Enumerable.Repeat(500, 25));
//         this.amounts.AddRange(Enumerable.Repeat(1000, 25));
//     }
//     var index = new Random().Next(0, this.amounts.Count);
//     var amount = this.amounts[index];
//     return amount;
// }