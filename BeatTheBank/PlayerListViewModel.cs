namespace BeatTheBank;


[ShellMap<PlayerListPage>(registerRoute: false)]
public partial class PlayerListViewModel(INavigator navigator) : ObservableObject, IPageLifecycleAware
{
    public void OnAppearing()
    {
    }

    
    public void OnDisappearing()
    {
    }
}