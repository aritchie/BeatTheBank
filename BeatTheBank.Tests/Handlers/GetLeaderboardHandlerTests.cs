using BeatTheBank.Handlers;
using BeatTheBank.Models;
using BeatTheBank.Services;
using BeatTheBank.Contracts;
using Microsoft.Extensions.Logging;
using Shiny.Mediator;

namespace BeatTheBank.Tests.Handlers;

public class GetLeaderboardHandlerTests
{
    readonly string dbPath;
    readonly GameDatabase database;
    readonly GetLeaderboardHandler handler;

    public GetLeaderboardHandlerTests()
    {
        dbPath = Path.Combine(Path.GetTempPath(), $"beatthebank_test_{Guid.NewGuid():N}.db3");
        database = new GameDatabase(dbPath);
        var logger = Substitute.For<ILogger<GetLeaderboardHandler>>();
        handler = new GetLeaderboardHandler(database, logger);
    }

    [Fact]
    public async Task Handle_ReturnsEmptyList_WhenNoGames()
    {
        var result = await handler.Handle(
            new GetLeaderboardRequest(10),
            Substitute.For<IMediatorContext>(),
            CancellationToken.None
        );

        result.ShouldBeEmpty();
    }

    [Fact]
    public async Task Handle_GroupsByPlayer()
    {
        await database.SaveGameResultAsync(new GameResult { PlayerName = "alice", Status = (int)PlayState.WinStop, WinAmount = 100, PotentialAmount = 200, VaultsOpened = 3, TotalRounds = 8, CompletedAt = DateTime.UtcNow });
        await database.SaveGameResultAsync(new GameResult { PlayerName = "alice", Status = (int)PlayState.WinStop, WinAmount = 200, PotentialAmount = 400, VaultsOpened = 4, TotalRounds = 8, CompletedAt = DateTime.UtcNow });
        await database.SaveGameResultAsync(new GameResult { PlayerName = "bob", Status = (int)PlayState.WinStop, WinAmount = 500, PotentialAmount = 800, VaultsOpened = 5, TotalRounds = 10, CompletedAt = DateTime.UtcNow });

        var result = await handler.Handle(
            new GetLeaderboardRequest(10),
            Substitute.For<IMediatorContext>(),
            CancellationToken.None
        );

        result.Count.ShouldBe(2); // alice and bob
    }

    [Fact]
    public async Task Handle_OrdersByTotalWonDescending()
    {
        await database.SaveGameResultAsync(new GameResult { PlayerName = "loser", Status = (int)PlayState.WinStop, WinAmount = 50, PotentialAmount = 100, VaultsOpened = 2, TotalRounds = 10, CompletedAt = DateTime.UtcNow });
        await database.SaveGameResultAsync(new GameResult { PlayerName = "winner", Status = (int)PlayState.WinStop, WinAmount = 999, PotentialAmount = 1500, VaultsOpened = 8, TotalRounds = 10, CompletedAt = DateTime.UtcNow });
        await database.SaveGameResultAsync(new GameResult { PlayerName = "middle", Status = (int)PlayState.WinStop, WinAmount = 300, PotentialAmount = 600, VaultsOpened = 4, TotalRounds = 10, CompletedAt = DateTime.UtcNow });

        var result = await handler.Handle(
            new GetLeaderboardRequest(10),
            Substitute.For<IMediatorContext>(),
            CancellationToken.None
        );

        result[0].PlayerName.ShouldBe("winner");
        result[1].PlayerName.ShouldBe("middle");
        result[2].PlayerName.ShouldBe("loser");
    }

    [Fact]
    public async Task Handle_RespectsTopN()
    {
        for (int i = 0; i < 5; i++)
        {
            await database.SaveGameResultAsync(new GameResult
            {
                PlayerName = $"player{i}",
                Status = (int)PlayState.WinStop,
                WinAmount = (i + 1) * 100,
                PotentialAmount = (i + 1) * 200,
                VaultsOpened = 3,
                TotalRounds = 10,
                CompletedAt = DateTime.UtcNow
            });
        }

        var result = await handler.Handle(
            new GetLeaderboardRequest(3),
            Substitute.For<IMediatorContext>(),
            CancellationToken.None
        );

        result.Count.ShouldBe(3);
        result[0].TotalWon.ShouldBe(500); // player4 (highest)
    }

    [Fact]
    public async Task Handle_TiebreaksByWinRateThenGamesPlayed()
    {
        // Two players with same total won
        await database.SaveGameResultAsync(new GameResult { PlayerName = "efficient", Status = (int)PlayState.WinStop, WinAmount = 500, PotentialAmount = 800, VaultsOpened = 5, TotalRounds = 10, CompletedAt = DateTime.UtcNow });

        await database.SaveGameResultAsync(new GameResult { PlayerName = "grinder", Status = (int)PlayState.WinStop, WinAmount = 250, PotentialAmount = 400, VaultsOpened = 4, TotalRounds = 8, CompletedAt = DateTime.UtcNow });
        await database.SaveGameResultAsync(new GameResult { PlayerName = "grinder", Status = (int)PlayState.WinStop, WinAmount = 250, PotentialAmount = 400, VaultsOpened = 4, TotalRounds = 8, CompletedAt = DateTime.UtcNow });

        var result = await handler.Handle(
            new GetLeaderboardRequest(10),
            Substitute.For<IMediatorContext>(),
            CancellationToken.None
        );

        // Both have $500 total, both 100% win rate, efficient has fewer games (1 vs 2) so sorts first
        result[0].PlayerName.ShouldBe("efficient");
        result[1].PlayerName.ShouldBe("grinder");
    }
}
