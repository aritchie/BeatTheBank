namespace BeatTheBank.Services;

public class GameContext
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
        // this.Rounds = this.randomizer.Next(2, 15);
        //this.IsJackpot = this.randomizer.Next(1, 40) == 39; // 1 in 40 chance

        // TODO: generate entire game
    }

    public void Stop()
    {
        
    }
    
    public bool NextRound()
    {
        return true;
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