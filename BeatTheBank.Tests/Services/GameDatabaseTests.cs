using BeatTheBank.Models;
using BeatTheBank.Services;

namespace BeatTheBank.Tests.Services;

public class GameDatabaseTests : IDisposable
{
    readonly string dbPath;
    readonly GameDatabase db;

    public GameDatabaseTests()
    {
        dbPath = Path.Combine(Path.GetTempPath(), $"beatthebank_test_{Guid.NewGuid():N}.db3");
        db = new GameDatabase(dbPath);
    }

    public void Dispose()
    {
        try { File.Delete(dbPath); } catch { }
    }

    [Fact]
    public async Task SaveAndRetrieve_PlayerGames()
    {
        var game = new GameResult
        {
            PlayerName = "alice",
            Status = (int)PlayState.WinStop,
            WinAmount = 500,
            PotentialAmount = 1000,
            VaultsOpened = 3,
            TotalRounds = 8,
            CompletedAt = DateTime.UtcNow
        };

        await db.SaveGameResultAsync(game);
        var games = await db.GetPlayerGamesAsync("alice");

        games.Count.ShouldBe(1);
        games[0].PlayerName.ShouldBe("alice");
        games[0].WinAmount.ShouldBe(500);
        games[0].PotentialAmount.ShouldBe(1000);
        games[0].VaultsOpened.ShouldBe(3);
    }

    [Fact]
    public async Task GetPlayerGames_FiltersToPlayer()
    {
        await db.SaveGameResultAsync(new GameResult { PlayerName = "alice", WinAmount = 100, CompletedAt = DateTime.UtcNow });
        await db.SaveGameResultAsync(new GameResult { PlayerName = "bob", WinAmount = 200, CompletedAt = DateTime.UtcNow });
        await db.SaveGameResultAsync(new GameResult { PlayerName = "alice", WinAmount = 300, CompletedAt = DateTime.UtcNow });

        var aliceGames = await db.GetPlayerGamesAsync("alice");
        var bobGames = await db.GetPlayerGamesAsync("bob");

        aliceGames.Count.ShouldBe(2);
        bobGames.Count.ShouldBe(1);
    }

    [Fact]
    public async Task GetPlayerGames_OrderedByCompletedAtDescending()
    {
        var older = DateTime.UtcNow.AddMinutes(-10);
        var newer = DateTime.UtcNow;

        await db.SaveGameResultAsync(new GameResult { PlayerName = "alice", WinAmount = 100, CompletedAt = older });
        await db.SaveGameResultAsync(new GameResult { PlayerName = "alice", WinAmount = 200, CompletedAt = newer });

        var games = await db.GetPlayerGamesAsync("alice");

        games[0].CompletedAt.ShouldBeGreaterThan(games[1].CompletedAt);
        games[0].WinAmount.ShouldBe(200);
    }

    [Fact]
    public async Task GetAllGames_ReturnsAllPlayers()
    {
        await db.SaveGameResultAsync(new GameResult { PlayerName = "alice", CompletedAt = DateTime.UtcNow });
        await db.SaveGameResultAsync(new GameResult { PlayerName = "bob", CompletedAt = DateTime.UtcNow });
        await db.SaveGameResultAsync(new GameResult { PlayerName = "charlie", CompletedAt = DateTime.UtcNow });

        var all = await db.GetAllGamesAsync();

        all.Count.ShouldBe(3);
    }

    [Fact]
    public async Task GetPlayerGames_ReturnsEmptyForUnknownPlayer()
    {
        var games = await db.GetPlayerGamesAsync("nobody");
        games.ShouldBeEmpty();
    }

    [Fact]
    public async Task SaveGameResult_AssignsAutoIncrementId()
    {
        var game1 = new GameResult { PlayerName = "alice", CompletedAt = DateTime.UtcNow };
        var game2 = new GameResult { PlayerName = "alice", CompletedAt = DateTime.UtcNow };

        await db.SaveGameResultAsync(game1);
        await db.SaveGameResultAsync(game2);

        var games = await db.GetPlayerGamesAsync("alice");
        games.Select(g => g.Id).Distinct().Count().ShouldBe(2);
    }
}
