using System.ComponentModel;
using CarPlay;
using CoreLocation;
using Foundation;
using MapKit;

namespace BeatTheBank;

public class CarPlayGameManager
{
    readonly CPInterfaceController interfaceController;
    GameViewModel? viewModel;
    CPPointOfInterestTemplate? template;
    bool isCleanedUp;

    public CarPlayGameManager(CPInterfaceController interfaceController)
    {
        this.interfaceController = interfaceController;
    }

    public void StartGame(string? playerName)
    {
        var services = IPlatformApplication.Current!.Services;
        this.viewModel = services.GetRequiredService<GameViewModel>();

        if (!string.IsNullOrWhiteSpace(playerName))
            this.viewModel.Name = playerName;

        this.viewModel.PropertyChanged += this.OnViewModelPropertyChanged;

        this.template = this.BuildTemplate();
        this.interfaceController.PushTemplate(this.template, true, null);

        if (!string.IsNullOrWhiteSpace(playerName))
            _ = this.StartNewGame();
        else
            _ = this.PromptForName();
    }

    async Task PromptForName()
    {
        var speech = IPlatformApplication.Current!.Services.GetRequiredService<ISpeechService>();
        await speech.Speak("Welcome to Beat the Bank. Say your name to start. For example, say, my name is John.");
        await this.EnableSpeech();
    }

    async Task StartNewGame()
    {
        await this.EnableSpeech();

        if (this.viewModel!.StartOverCommand.CanExecute(null))
            await this.viewModel.StartOverCommand.ExecuteAsync(null);
    }

    async Task EnableSpeech()
    {
        if (this.viewModel == null)
            return;

        if (!this.viewModel.SpeechCommand.IsRunning)
            await this.viewModel.SpeechCommand.ExecuteAsync(null);
    }

    CPPointOfInterestTemplate BuildTemplate()
    {
        var poi = this.CreateCurrentPoi();
        var template = new CPPointOfInterestTemplate("Beat The Bank", [poi], nint.Zero);
        return template;
    }

    CPPointOfInterest CreateCurrentPoi()
    {
        var title = this.GetTitle();
        var subtitle = this.GetSubtitle();

        var location = new MKMapItem(new MKPlacemark(new CLLocationCoordinate2D(0, 0)));
        var poi = new CPPointOfInterest(location, title, subtitle, null, null, null, null, null);
        return poi;
    }

    string GetTitle()
    {
        if (this.viewModel == null)
            return "Beat The Bank";

        if (string.IsNullOrWhiteSpace(this.viewModel.Name))
            return "Say: My name is...";

        return this.viewModel.Status switch
        {
            PlayState.Win => $"JACKPOT! ${this.viewModel.WinAmount:N0}!",
            PlayState.Lose => "ALARM! You lost!",
            PlayState.WinStop => $"Stopped! Won ${this.viewModel.WinAmount:N0}",
            _ => this.viewModel.Vault == 0
                ? $"Ready, {this.viewModel.Name}!"
                : $"Vault {this.viewModel.Vault}: ${this.viewModel.Amount:N0}"
        };
    }

    string GetSubtitle()
    {
        if (this.viewModel == null)
            return "Voice controlled";

        return this.viewModel.Status switch
        {
            PlayState.Win => "Say 'start over' to play again",
            PlayState.Lose => "Say 'start over' to play again",
            PlayState.WinStop => "Say 'start over' to play again",
            _ => this.viewModel.Vault == 0
                ? "Say 'start over' to begin"
                : "Say 'continue' or 'stop'"
        };
    }

    void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (this.isCleanedUp || this.template == null)
            return;

        if (e.PropertyName is nameof(GameViewModel.Vault)
            or nameof(GameViewModel.Amount)
            or nameof(GameViewModel.Status)
            or nameof(GameViewModel.WinAmount)
            or nameof(GameViewModel.Name))
        {
            this.UpdateDisplay();
        }
    }

    void UpdateDisplay()
    {
        if (this.template == null)
            return;

        MainThread.BeginInvokeOnMainThread(() =>
        {
            var poi = this.CreateCurrentPoi();
            this.template.SetPointsOfInterest([poi], nint.Zero);
        });
    }

    public void Cleanup()
    {
        this.isCleanedUp = true;
        if (this.viewModel != null)
        {
            this.viewModel.PropertyChanged -= this.OnViewModelPropertyChanged;
            this.viewModel.OnDisappearing();
            this.viewModel = null;
        }
        this.template = null;
    }
}
