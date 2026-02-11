using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace BeatTheBank;


[ShellMap<LeaderboardPage>]
public partial class LeaderboardViewModel(
    IMediator mediator,
    ILogger<LeaderboardViewModel> logger
) : ObservableObject, IPageLifecycleAware
{
    [ObservableProperty] List<PlayerStats> players = new();
    [ObservableProperty] bool isRefreshing;


    public void OnAppearing() => _ = this.RefreshAsync();
    public void OnDisappearing() { }


    [RelayCommand]
    async Task RefreshAsync()
    {
        this.IsRefreshing = true;
        var results = await mediator.Request(new GetLeaderboardRequest(20));
        this.Players = results.Result;
        this.IsRefreshing = false;
    }
}
