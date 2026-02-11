using BeatTheBank.Models;
using BeatTheBank.Services;

namespace BeatTheBank.Tests.Services;

public class StatsCalculatorTests
{
    static GameResult MakeGame(
        PlayState status,
        int winAmount = 0,
        int potentialAmount = 0,
        int vaultsOpened = 5,
        int totalRounds = 10,
        bool isJackpot = false,
        int stopVault = 0,
        DateTime? completedAt = null
    ) => new()
    {
        PlayerName = "testplayer",
        Status = (int)status,
        WinAmount = winAmount,
        PotentialAmount = potentialAmount,
        VaultsOpened = vaultsOpened,
        TotalRounds = totalRounds,
        IsJackpot = isJackpot,
        StopVault = stopVault,
        CompletedAt = completedAt ?? DateTime.UtcNow
    };

    [Fact]
    public void EmptyGames_ReturnsDefaultStats()
    {
        var result = StatsCalculator.Calculate("Alice", []);

        result.PlayerName.ShouldBe("Alice");
        result.GamesPlayed.ShouldBe(0);
        result.TotalWon.ShouldBe(0);
        result.WinRate.ShouldBe(0);
    }

    [Fact]
    public void SingleWin_CalculatesCorrectly()
    {
        var games = new List<GameResult>
        {
            MakeGame(PlayState.WinStop, winAmount: 500, potentialAmount: 1000, vaultsOpened: 3, totalRounds: 8, stopVault: 3)
        };

        var result = StatsCalculator.Calculate("Bob", games);

        result.PlayerName.ShouldBe("Bob");
        result.GamesPlayed.ShouldBe(1);
        result.TotalWon.ShouldBe(500);
        result.PotentialWinnings.ShouldBe(1000);
        result.TimesStopped.ShouldBe(1);
        result.TimesBusted.ShouldBe(0);
        result.BestSingleGame.ShouldBe(500);
        result.WinRate.ShouldBe(100.0);
        result.MoneyLeftOnTable.ShouldBe(500);
    }

    [Fact]
    public void SingleLoss_CalculatesCorrectly()
    {
        var games = new List<GameResult>
        {
            MakeGame(PlayState.Lose, winAmount: 0, potentialAmount: 0, vaultsOpened: 10, totalRounds: 10)
        };

        var result = StatsCalculator.Calculate("Charlie", games);

        result.GamesPlayed.ShouldBe(1);
        result.TotalWon.ShouldBe(0);
        result.TimesBusted.ShouldBe(1);
        result.WinRate.ShouldBe(0.0);
        result.LongestStreak.ShouldBe(0);
    }

    [Fact]
    public void JackpotHit_OnlyCountsWhenStatusIsWin()
    {
        var games = new List<GameResult>
        {
            MakeGame(PlayState.Win, winAmount: 1000000, isJackpot: true),
            MakeGame(PlayState.Lose, isJackpot: true), // jackpot game but lost (stopped early during reveal)
            MakeGame(PlayState.WinStop, winAmount: 300, isJackpot: true), // stopped before jackpot
        };

        var result = StatsCalculator.Calculate("Lucky", games);

        result.JackpotsHit.ShouldBe(1); // Only the Win counts
    }

    [Fact]
    public void MixedGames_CalculatesAllStats()
    {
        var now = DateTime.UtcNow;
        var games = new List<GameResult>
        {
            MakeGame(PlayState.WinStop, winAmount: 200, potentialAmount: 400, vaultsOpened: 3, totalRounds: 8, completedAt: now.AddMinutes(-4)),
            MakeGame(PlayState.WinStop, winAmount: 500, potentialAmount: 800, vaultsOpened: 5, totalRounds: 10, completedAt: now.AddMinutes(-3)),
            MakeGame(PlayState.Lose, winAmount: 0, potentialAmount: 0, vaultsOpened: 6, totalRounds: 6, completedAt: now.AddMinutes(-2)),
            MakeGame(PlayState.Win, winAmount: 1000000, potentialAmount: 1000000, vaultsOpened: 10, totalRounds: 10, isJackpot: true, completedAt: now.AddMinutes(-1)),
        };

        var result = StatsCalculator.Calculate("Pro", games);

        result.GamesPlayed.ShouldBe(4);
        result.TotalWon.ShouldBe(200 + 500 + 0 + 1000000);
        result.PotentialWinnings.ShouldBe(400 + 800 + 0 + 1000000);
        result.JackpotsHit.ShouldBe(1);
        result.TimesBusted.ShouldBe(1);
        result.TimesStopped.ShouldBe(2);
        result.BestSingleGame.ShouldBe(1000000);
        result.WinRate.ShouldBe(75.0); // 3 of 4 not-lose
        result.MoneyLeftOnTable.ShouldBe((400 - 200) + (800 - 500)); // only WinStop games
    }

    [Fact]
    public void LongestStreak_CalculatedChronologically()
    {
        var now = DateTime.UtcNow;
        var games = new List<GameResult>
        {
            MakeGame(PlayState.WinStop, winAmount: 100, potentialAmount: 200, completedAt: now.AddMinutes(-6)),
            MakeGame(PlayState.WinStop, winAmount: 100, potentialAmount: 200, completedAt: now.AddMinutes(-5)),
            MakeGame(PlayState.Win, winAmount: 1000000, potentialAmount: 1000000, isJackpot: true, completedAt: now.AddMinutes(-4)),
            MakeGame(PlayState.Lose, completedAt: now.AddMinutes(-3)), // breaks streak
            MakeGame(PlayState.WinStop, winAmount: 100, potentialAmount: 200, completedAt: now.AddMinutes(-2)),
        };

        var result = StatsCalculator.Calculate("Streak", games);

        result.LongestStreak.ShouldBe(3); // first 3 games before the loss
    }

    [Fact]
    public void RiskScore_CalculatesVaultsToRoundsRatio()
    {
        var games = new List<GameResult>
        {
            MakeGame(PlayState.WinStop, winAmount: 100, potentialAmount: 200, vaultsOpened: 8, totalRounds: 10),
        };

        var result = StatsCalculator.Calculate("Risky", games);

        // RiskScore = (avgVaultsPerGame / avgTotalRounds) * 100 = (8/10)*100 = 80
        result.RiskScore.ShouldBe(80.0);
    }

    [Fact]
    public void AvgVaultsPerGame_IsRounded()
    {
        var games = new List<GameResult>
        {
            MakeGame(PlayState.WinStop, winAmount: 100, potentialAmount: 200, vaultsOpened: 3, totalRounds: 10),
            MakeGame(PlayState.WinStop, winAmount: 100, potentialAmount: 200, vaultsOpened: 4, totalRounds: 10),
        };

        var result = StatsCalculator.Calculate("Avg", games);

        result.AvgVaultsPerGame.ShouldBe(3.5);
    }

    [Fact]
    public void DisplayName_PreservedAsGiven()
    {
        var games = new List<GameResult>
        {
            MakeGame(PlayState.WinStop, winAmount: 100, potentialAmount: 200)
        };

        var result = StatsCalculator.Calculate("  Allan Ritchie  ", games);

        result.PlayerName.ShouldBe("  Allan Ritchie  "); // StatsCalculator doesn't normalize — that's the handler's job
    }
}
