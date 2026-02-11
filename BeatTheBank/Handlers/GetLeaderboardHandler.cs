namespace BeatTheBank.Handlers;


[MediatorSingleton]
public class GetLeaderboardHandler(
    GameDatabase database,
    ILogger<GetLeaderboardHandler> logger
) : IRequestHandler<GetLeaderboardRequest, List<PlayerStats>>
{
    public async Task<List<PlayerStats>> Handle(GetLeaderboardRequest request, IMediatorContext context, CancellationToken ct)
    {
        var allGames = await database.GetAllGamesAsync();
        logger.LogDebug("Loaded {Count} total games for leaderboard", allGames.Count);

        if (allGames.Count == 0)
            return new List<PlayerStats>();

        var playerGroups = allGames.GroupBy(g => g.PlayerName);

        var allStats = playerGroups
            .Select(group => StatsCalculator.Calculate(group.Key, group.ToList()))
            .ToList();

        return allStats
            .OrderByDescending(s => s.TotalWon)
            .ThenByDescending(s => s.WinRate)
            .ThenBy(s => s.GamesPlayed)
            .Take(request.TopN)
            .ToList();
    }
}
