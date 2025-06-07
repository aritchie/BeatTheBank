using BeatTheBank.Models;

namespace BeatTheBank;


[ShellMap<PlayerEditPage>]
public partial class PlayerEditViewModel(INavigator navigator) : ObservableObject
{
    public Player? Player { get; set; }
    public bool CanDelete => this.Player?.Id != Guid.Empty;
    
    [ObservableProperty] string title;
    [ObservableProperty] string name;

    [RelayCommand]
    async Task Save()
    {
        
        await navigator.GoBack();
    }


    [RelayCommand]
    async Task Delete()
    {
        var confirm = await navigator.Confirm("Confirm", "Are you sure you want to delete this player?");
        if (confirm)
        {
            // TODO: delete
            await navigator.GoBack();    
        }
    }
}