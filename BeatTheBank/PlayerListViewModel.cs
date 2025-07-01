using BeatTheBank.Models;
using BeatTheBank.Services;

namespace BeatTheBank;


[ShellMap<PlayerListPage>(registerRoute: false)]
public partial class PlayerListViewModel(
    INavigator navigator,
    AppSqliteConnection data
) : ObservableObject, IPageLifecycleAware
{
    [RelayCommand] Task NavToNewPlayer() => navigator.NavigateTo<PlayerEditViewModel>();

    
    [RelayCommand]
    async Task NavToPlayer(PlayerStats player)
    {
        var e = await data.GetAsync<Player>(player.PlayerId);
        await navigator.NavigateTo<PlayerEditViewModel>(x => x.Player = e);
    }


    [RelayCommand]
    async Task NavToNewGame(PlayerStats player)
    {
        var e = await data.GetAsync<Player>(player.PlayerId);
        await navigator.NavigateTo<GameViewModel>(x => x.Player = e);
    }

    [ObservableProperty] List<PlayerStats> players;

    public async void OnAppearing()
    {
    // // jackpot if vaults == stopvault and amount == 1000000
    // public int Vaults { get; set; }
    //
    // // lost amounts sum amount where same game and vault > stopvault
    // public int StopVault { get; set; }
    
    // jackpot if vaults == stopvault and amount == 1000000
    // lost amounts sum amount where same game and vault > stopvault
        this.Players = await data.QueryAsync<PlayerStats>(
            """
            SELECT
                PlayerId = p.Id,
                PlayerName = p.Name,
                GameCount = (SELECT COUNT(*) FROM Game WHERE PlayerId = p.Id),
                WinAmount = (
                    SELECT
                        SUM(DollarAmount)
                    FROM
                        GameVault gv1
                        INNER JOIN Game g1 ON gv1.Id = g1.Id AND gv1.Vault = g1.StopVault
                    WHERE
                        gv1.PlayerId = p.Id
                )
            FROM 
                Player p
            ORDER BY
                p.Name
            """);
    }

    
    public void OnDisappearing()
    {
    }
}

public class PlayerStats
{
    public Guid PlayerId { get; set; }
    public string PlayerName { get; set; }
    public int GameCount { get; set; }
    public int WinAmount { get; set; }
}