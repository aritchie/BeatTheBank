using CarPlay;
using Foundation;
using Microsoft.Extensions.DependencyInjection;

namespace BeatTheBank;

public class CarPlayLeaderboardManager
{
    readonly CPInterfaceController interfaceController;
    readonly Action<string> onStartGame;
    IServiceScope? scope;

    public CarPlayLeaderboardManager(CPInterfaceController interfaceController, Action<string> onStartGame)
    {
        this.interfaceController = interfaceController;
        this.onStartGame = onStartGame;
    }

    public void Show()
    {
        var template = this.BuildTemplate([]);
        this.interfaceController.SetRootTemplate(template, false, null);
        _ = this.LoadPlayers();
    }

    CPListTemplate BuildTemplate(CPListSection[] playerSections)
    {
        var newGameItem = new CPListItem("New Game", "Play as CarPlay")
        {
            Handler = (item, completion) =>
            {
                this.onStartGame("CarPlay");
                completion();
            }
        };

        var allSections = new[] { new CPListSection([newGameItem as ICPListTemplateItem], "Actions", null) }
            .Concat(playerSections)
            .ToArray();

        return new CPListTemplate("Beat The Bank", allSections);
    }

    async Task LoadPlayers()
    {
        try
        {
            var services = IPlatformApplication.Current!.Services;
            this.scope = services.CreateScope();
            var vm = this.scope.ServiceProvider.GetRequiredService<LeaderboardViewModel>();
            await vm.RefreshCommand.ExecuteAsync(null);
            var players = vm.Players;

            this.scope.Dispose();
            this.scope = null;

            if (players == null || players.Count == 0)
                return;

            var items = players.Select(p =>
            {
                var item = new CPListItem(
                    p.PlayerName,
                    $"Won: ${p.TotalWon:N0} | Games: {p.GamesPlayed}"
                )
                {
                    Handler = (listItem, completion) =>
                    {
                        this.onStartGame(p.PlayerName);
                        completion();
                    }
                };
                return (ICPListTemplateItem)item;
            }).ToArray();

            var playerSection = new CPListSection(items, "Top Players", null);
            var template = this.BuildTemplate([playerSection]);
            this.interfaceController.SetRootTemplate(template, true, null);
        }
        catch (Exception ex)
        {
            var logger = IPlatformApplication.Current!.Services.GetRequiredService<ILogger<CarPlayLeaderboardManager>>();
            logger.LogError(ex, "Failed to load CarPlay leaderboard");
            this.scope?.Dispose();
            this.scope = null;
        }
    }

    public void Cleanup()
    {
        this.scope?.Dispose();
        this.scope = null;
    }
}
