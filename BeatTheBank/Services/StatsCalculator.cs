namespace BeatTheBank.Services;


public static class StatsCalculator
{
    public static PlayerStats Calculate(string displayName, List<GameResult> games)
    {
        if (games.Count == 0)
            return new PlayerStats { PlayerName = displayName };

        var gamesPlayed = games.Count;
        var totalWon = games.Sum(g => g.WinAmount);
        var potentialWinnings = games.Sum(g => g.PotentialAmount);
        var jackpotsHit = games.Count(g => g.IsJackpot && g.Status == (int)PlayState.Win);
        var timesBusted = games.Count(g => g.Status == (int)PlayState.Lose);
        var timesStopped = games.Count(g => g.Status == (int)PlayState.WinStop);
        var bestSingleGame = games.Max(g => g.WinAmount);
        var avgVaultsPerGame = games.Average(g => (double)g.VaultsOpened);
        var winRate = (games.Count(g => g.Status != (int)PlayState.Lose) / (double)gamesPlayed) * 100;
        var longestStreak = CalculateLongestStreak(games);
        var moneyLeftOnTable = games
            .Where(g => g.Status == (int)PlayState.WinStop)
            .Sum(g => g.PotentialAmount - g.WinAmount);
        var avgTotalRounds = games.Average(g => (double)g.TotalRounds);
        var riskScore = avgTotalRounds > 0
            ? (avgVaultsPerGame / avgTotalRounds) * 100
            : 0;

        return new PlayerStats
        {
            PlayerName = displayName,
            GamesPlayed = gamesPlayed,
            TotalWon = totalWon,
            PotentialWinnings = potentialWinnings,
            JackpotsHit = jackpotsHit,
            TimesBusted = timesBusted,
            TimesStopped = timesStopped,
            BestSingleGame = bestSingleGame,
            AvgVaultsPerGame = Math.Round(avgVaultsPerGame, 1),
            WinRate = Math.Round(winRate, 1),
            LongestStreak = longestStreak,
            MoneyLeftOnTable = moneyLeftOnTable,
            RiskScore = Math.Round(riskScore, 1)
        };
    }


    static int CalculateLongestStreak(List<GameResult> games)
    {
        var maxStreak = 0;
        var currentStreak = 0;

        foreach (var game in games.OrderBy(g => g.CompletedAt))
        {
            if (game.Status == (int)PlayState.Lose)
            {
                currentStreak = 0;
            }
            else
            {
                currentStreak++;
                maxStreak = Math.Max(maxStreak, currentStreak);
            }
        }
        return maxStreak;
    }
}
