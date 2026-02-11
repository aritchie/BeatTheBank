namespace BeatTheBank.Contracts;


public record SaveGameResultCommand(
    string PlayerName,
    PlayState Status,
    int WinAmount,
    int PotentialAmount,
    int VaultsOpened,
    int TotalRounds,
    int StopVault,
    bool IsJackpot
) : ICommand;
