using BeatTheBank.Handlers;
using BeatTheBank.Models;
using BeatTheBank.Services;
using BeatTheBank.Contracts;
using Microsoft.Extensions.Logging;
using Shiny.Mediator;

namespace BeatTheBank.Tests.Handlers;

public class SaveGameResultHandlerTests
{
    readonly string dbPath;
    readonly GameDatabase database;
    readonly SaveGameResultHandler handler;

    public SaveGameResultHandlerTests()
    {
        dbPath = Path.Combine(Path.GetTempPath(), $"beatthebank_test_{Guid.NewGuid():N}.db3");
        database = new GameDatabase(dbPath);
        var logger = Substitute.For<ILogger<SaveGameResultHandler>>();
        handler = new SaveGameResultHandler(database, logger);
    }

    [Fact]
    public async Task Handle_SavesGameToDatabase()
    {
        var command = new SaveGameResultCommand(
            PlayerName: "Alice",
            Status: PlayState.WinStop,
            WinAmount: 500,
            PotentialAmount: 1000,
            VaultsOpened: 3,
            TotalRounds: 8,
            StopVault: 3,
            IsJackpot: false
        );

        await handler.Handle(command, Substitute.For<IMediatorContext>(), CancellationToken.None);

        var games = await database.GetPlayerGamesAsync("alice");
        games.Count.ShouldBe(1);
        games[0].WinAmount.ShouldBe(500);
        games[0].PotentialAmount.ShouldBe(1000);
        games[0].VaultsOpened.ShouldBe(3);
        games[0].TotalRounds.ShouldBe(8);
        games[0].StopVault.ShouldBe(3);
        games[0].Status.ShouldBe((int)PlayState.WinStop);
    }

    [Fact]
    public async Task Handle_NormalizesPlayerNameToLowercase()
    {
        var command = new SaveGameResultCommand(
            PlayerName: "  ALICE  ",
            Status: PlayState.Win,
            WinAmount: 1000000,
            PotentialAmount: 1000000,
            VaultsOpened: 10,
            TotalRounds: 10,
            StopVault: 0,
            IsJackpot: true
        );

        await handler.Handle(command, Substitute.For<IMediatorContext>(), CancellationToken.None);

        var games = await database.GetPlayerGamesAsync("alice");
        games.Count.ShouldBe(1);
        games[0].PlayerName.ShouldBe("alice");
    }

    [Fact]
    public async Task Handle_SetsCompletedAtToUtcNow()
    {
        var before = DateTime.UtcNow;

        var command = new SaveGameResultCommand("test", PlayState.Lose, 0, 0, 5, 5, 0, false);
        await handler.Handle(command, Substitute.For<IMediatorContext>(), CancellationToken.None);

        var after = DateTime.UtcNow;
        var games = await database.GetPlayerGamesAsync("test");
        games[0].CompletedAt.ShouldBeInRange(before, after);
    }

    [Fact]
    public async Task Handle_PersistsIsJackpotFlag()
    {
        var command = new SaveGameResultCommand("test", PlayState.Win, 1000000, 1000000, 10, 10, 0, true);
        await handler.Handle(command, Substitute.For<IMediatorContext>(), CancellationToken.None);

        var games = await database.GetPlayerGamesAsync("test");
        games[0].IsJackpot.ShouldBeTrue();
    }
}
