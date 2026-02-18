using CarPlay;
using Foundation;

namespace BeatTheBank;

public class CarPlayLeaderboardManager
{
    readonly CPInterfaceController interfaceController;
    readonly Action<string?> onStartGame;

    public CarPlayLeaderboardManager(CPInterfaceController interfaceController, Action<string?> onStartGame)
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

    CPListTemplate BuildTemplate(CPListSection[] sections)
    {
        var newGameItem = new CPListItem("New Game", "Say your name to start")
        {
            Handler = (item, completion) =>
            {
                this.onStartGame(null);
                completion();
            }
        };

        var allSections = new[] { new CPListSection([newGameItem as ICPListTemplateItem], "Actions", null) }
            .Concat(sections)
            .ToArray();

        return new CPListTemplate("Beat The Bank", allSections);
    }

    async Task LoadPlayers()
    {
        try
        {
            var mediator = IPlatformApplication.Current!.Services.GetRequiredService<IMediator>();
            var result = await mediator.Request(new GetLeaderboardRequest(10));
            var players = result.Result;

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
        }
    }
}
