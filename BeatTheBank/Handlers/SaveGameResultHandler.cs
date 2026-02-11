namespace BeatTheBank.Handlers;


[MediatorSingleton]
public class SaveGameResultHandler(
    GameDatabase database,
    ILogger<SaveGameResultHandler> logger
) : ICommandHandler<SaveGameResultCommand>
{
    public async Task Handle(SaveGameResultCommand command, IMediatorContext context, CancellationToken ct)
    {
        var result = new GameResult
        {
            PlayerName = command.PlayerName.Trim().ToLowerInvariant(),
            CompletedAt = DateTime.UtcNow,
            Status = (int)command.Status,
            WinAmount = command.WinAmount,
            PotentialAmount = command.PotentialAmount,
            VaultsOpened = command.VaultsOpened,
            TotalRounds = command.TotalRounds,
            StopVault = command.StopVault,
            IsJackpot = command.IsJackpot
        };

        await database.SaveGameResultAsync(result);
        logger.LogInformation(
            "Saved game for {Player}: {Status}, Won ${WinAmount}",
            command.PlayerName,
            command.Status,
            command.WinAmount
        );
    }
}
