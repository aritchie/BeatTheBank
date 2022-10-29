namespace BeatTheBank;


public partial class MainPage : ContentPage
{
	public MainPage(MainViewModel viewModel)
	{
		this.InitializeComponent();
		this.BindingContext = viewModel;
	}
}


