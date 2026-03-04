using Shiny.SqliteDocumentDb;

namespace BeatTheBank.Services;


[Singleton]
public class GameDatabase(IDocumentStore store)
{
    public async Task SaveGameResultAsync(GameResult result)
        => await store.Set(result);

    public async Task<List<GameResult>> GetPlayerGamesAsync(string playerName)
    {
        var results = await store
            .Query<GameResult>()
            .Where(g => g.PlayerName == playerName)
            .OrderByDescending(g => g.CompletedAt)
            .ToList();
        return results.ToList();
    }

    public async Task<List<GameResult>> GetAllGamesAsync()
    {
        var results = await store
            .Query<GameResult>()
            .OrderByDescending(g => g.CompletedAt)
            .ToList();
        return results.ToList();
    }
}
