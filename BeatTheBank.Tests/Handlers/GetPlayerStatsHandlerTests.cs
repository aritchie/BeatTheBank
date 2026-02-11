using BeatTheBank.Handlers;
using BeatTheBank.Models;
using BeatTheBank.Services;
using BeatTheBank.Contracts;
using Microsoft.Extensions.Logging;
using Shiny.Mediator;

namespace BeatTheBank.Tests.Handlers;

public class GetPlayerStatsHandlerTests
{
    readonly string dbPath;
    readonly GameDatabase database;
    readonly GetPlayerStatsHandler handler;

    public GetPlayerStatsHandlerTests()
    {
        dbPath = Path.Combine(Path.GetTempPath(), $"beatthebank_test_{Guid.NewGuid():N}.db3");
        database = new GameDatabase(dbPath);
        var logger = Substitute.For<ILogger<GetPlayerStatsHandler>>();
        handler = new GetPlayerStatsHandler(database, logger);
    }

    [Fact]
    public async Task Handle_ReturnsNull_WhenNoGamesExist()
    {
        var request = new GetPlayerStatsRequest("Unknown");

        var result = await handler.Handle(request, Substitute.For<IMediatorContext>(), CancellationToken.None);

        result.ShouldBeNull();
    }

    [Fact]
    public async Task Handle_ReturnsStats_WhenGamesExist()
    {
        await database.SaveGameResultAsync(new GameResult
        {
            PlayerName = "alice",
            Status = (int)PlayState.WinStop,
            WinAmount = 500,
            PotentialAmount = 1000,
            VaultsOpened = 3,
            TotalRounds = 8,
            CompletedAt = DateTime.UtcNow
        });

        var result = await handler.Handle(
            new GetPlayerStatsRequest("Alice"),
            Substitute.For<IMediatorContext>(),
            CancellationToken.None
        );

        result.ShouldNotBeNull();
        result!.PlayerName.ShouldBe("Alice"); // preserves display casing
        result.GamesPlayed.ShouldBe(1);
        result.TotalWon.ShouldBe(500);
    }

    [Fact]
    public async Task Handle_NormalizesPlayerName_CaseInsensitive()
    {
        await database.SaveGameResultAsync(new GameResult
        {
            PlayerName = "alice",
            Status = (int)PlayState.WinStop,
            WinAmount = 300,
            PotentialAmount = 600,
            VaultsOpened = 4,
            TotalRounds = 10,
            CompletedAt = DateTime.UtcNow
        });

        // Query with different casing
        var result = await handler.Handle(
            new GetPlayerStatsRequest("  ALICE  "),
            Substitute.For<IMediatorContext>(),
            CancellationToken.None
        );

        result.ShouldNotBeNull();
        result!.GamesPlayed.ShouldBe(1);
    }

    [Fact]
    public async Task Handle_AggregatesMultipleGames()
    {
        await database.SaveGameResultAsync(new GameResult
        {
            PlayerName = "alice", Status = (int)PlayState.WinStop,
            WinAmount = 200, PotentialAmount = 400, VaultsOpened = 3, TotalRounds = 8, CompletedAt = DateTime.UtcNow
        });
        await database.SaveGameResultAsync(new GameResult
        {
            PlayerName = "alice", Status = (int)PlayState.WinStop,
            WinAmount = 300, PotentialAmount = 600, VaultsOpened = 5, TotalRounds = 10, CompletedAt = DateTime.UtcNow
        });

        var result = await handler.Handle(
            new GetPlayerStatsRequest("Alice"),
            Substitute.For<IMediatorContext>(),
            CancellationToken.None
        );

        result.ShouldNotBeNull();
        result!.GamesPlayed.ShouldBe(2);
        result.TotalWon.ShouldBe(500);
        result.PotentialWinnings.ShouldBe(1000);
    }
}
