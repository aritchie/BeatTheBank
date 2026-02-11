namespace BeatTheBank.Handlers;


[MediatorSingleton]
public class GetPlayerStatsHandler(
    GameDatabase database,
    ILogger<GetPlayerStatsHandler> logger
) : IRequestHandler<GetPlayerStatsRequest, PlayerStats?>
{
    public async Task<PlayerStats?> Handle(GetPlayerStatsRequest request, IMediatorContext context, CancellationToken ct)
    {
        var playerName = request.PlayerName.Trim().ToLowerInvariant();
        var games = await database.GetPlayerGamesAsync(playerName);

        if (games.Count == 0)
        {
            logger.LogDebug("No games found for player {Player}", request.PlayerName);
            return null;
        }

        return StatsCalculator.Calculate(request.PlayerName, games);
    }
}
