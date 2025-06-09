using System.ComponentModel;
using BeatTheBank.Models;
using BeatTheBank.Services;

namespace BeatTheBank;


[ShellMap<PlayerEditPage>]
public partial class PlayerEditViewModel(
    INavigator navigator,
    AppSqliteConnection data
) : ObservableObject
{
    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(this.Player) && this.Player != null)
        {
            this.Name = this.Player.Name;
            this.Title = "Edit Player";
        }
        base.OnPropertyChanged(e);
    }

    [ObservableProperty] Player? player;
    public bool CanDelete => this.Player?.Id != Guid.Empty;
    
    [ObservableProperty] string title = "New Player";
    [ObservableProperty] string name;

    [RelayCommand]
    async Task Save()
    {
        var e = this.Player ?? new();
        e.Name = this.Name;
        await data.InsertOrReplaceAsync(e);
        await navigator.GoBack();
    }


    [RelayCommand]
    async Task Delete()
    {
        var confirm = await navigator.Confirm("Confirm", "Are you sure you want to delete this player?");
        if (confirm)
        {
            await data.DeleteAsync(this.Player!);
            await navigator.GoBack();    
        }
    }
}