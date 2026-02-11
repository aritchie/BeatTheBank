using System.ComponentModel;
using System.Globalization;
using CommunityToolkit.Maui.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace BeatTheBank;


[ShellMap<MainPage>(registerRoute: false)]
public partial class MainViewModel(
    ILogger<MainViewModel> logger,
    INavigator navigator,
    ITextToSpeech textToSpeech,
    ISpeechToText speechRecognizer,
    IDeviceDisplay deviceDisplay,
    SoundEffectService sounds,
    IMediator mediator
) : ObservableObject, IPageLifecycleAware
{
    static readonly string[] NextVaultStatements = new[]
    {
        "Alright, let's open it up",
        "Taking a chance and going for it",
        "Let's see what's in the next vault",
        "Let's do this",
        "Come on big money!"
    };

    readonly Random randomizer = new();
    [ObservableProperty] int rounds = 0;
    [ObservableProperty] bool isJackpot = false;


    void NotifyExecuteChanged()
    {
        this.StartOverCommand.NotifyCanExecuteChanged();
        this.ContinueCommand.NotifyCanExecuteChanged();
        this.StopCommand.NotifyCanExecuteChanged();
    }

    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);
        this.NotifyExecuteChanged();
    }

    public void OnAppearing()
    {
        deviceDisplay.KeepScreenOn = true;
        speechRecognizer.RecognitionResultCompleted += this.SpeechRecognizerOnRecognitionResultCompleted;
        this.NotifyExecuteChanged();
    }

    public void OnDisappearing()
    {
        speechRecognizer.RecognitionResultCompleted -= this.SpeechRecognizerOnRecognitionResultCompleted;
    }
    
    [ObservableProperty] string speechText = "Start Speech Recognizer";
    [ObservableProperty] string name;
    [ObservableProperty] int vault;
    [ObservableProperty] int stopVault;
    [ObservableProperty] int winAmount;
    [ObservableProperty] int amount;
    [ObservableProperty] PlayState status;


    [RelayCommand(CanExecute = nameof(CanStartOver))]
    async Task StartOver()
    {
        this.Status = PlayState.InProgress;
        this.Vault = 0;
        this.Amount = 0;
        this.WinAmount = 0;
        this.StopVault = 0;
        this.Rounds = this.randomizer.Next(4, 15);
        this.IsJackpot = this.randomizer.Next(1, 40) == 39; // 1 in 40 chance
        
        logger.LogDebug($"Rounds: {this.Rounds} - Jackpot: {this.IsJackpot}");
        
        await this.Speak(1000, $"Good Luck {this.Name}.  Let's play!");
        await this.NextRound();
    }
    bool CanStartOver() => !String.IsNullOrWhiteSpace(this.Name);

    [RelayCommand(CanExecute = nameof(CanContinue))]
    Task Continue() => this.NextRound();
    bool CanContinue() => this.Vault < this.Rounds && this.Status == PlayState.InProgress;

    const string ENABLE = "Enable Speech Recognizer";
    const string DISABLE = "Disable Speech Recognizer";
    
    [RelayCommand]
    async Task Speech()
    {
        try
        {
            if (speechRecognizer.CurrentState == SpeechToTextState.Listening)
            {
                await speechRecognizer.StopListenAsync();
                this.SpeechText = ENABLE;
                return;
            }

            var granted = await speechRecognizer.RequestPermissions();
            if (!granted)
            {
                await navigator.Alert("Speech", "Permission denied");
                return;
            }

            await speechRecognizer.StartListenAsync(new SpeechToTextOptions
            {
                Culture = new CultureInfo("en-US"),
                ShouldReportPartialResults = false
            });
            this.SpeechText = DISABLE;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "There is an issue with speech recognition");
            await navigator.Alert("Error", "Something is wrong with speech recognition");
        }
    }
    

    void SpeechRecognizerOnRecognitionResultCompleted(object? sender, SpeechToTextRecognitionResultCompletedEventArgs e)
    {
        logger.LogInformation("Incoming Speech Result");
        if (!e.RecognitionResult.IsSuccessful)
            return;

        var txt = e.RecognitionResult.Text?.ToLower() ?? String.Empty;
        logger.LogInformation("Speech Result: {txt}", txt);
        
        switch (txt)
        {
            case "yes":
            case "next":
            case "keep going":
            case "continue":
            case "go":
                if (this.ContinueCommand.CanExecute(null))
                    this.ContinueCommand.Execute(null);
                break;
            
            case "no":
            case "stop":
                if (this.StopCommand.CanExecute(null))
                    this.StopCommand.Execute(null);
                break;
            
            case "try again":
            case "start over":
            case "restart":
                if (this.StartOverCommand.CanExecute(null))
                    this.StartOverCommand.Execute(null);
                break;
            
            default:
                if (txt.StartsWith("my name is"))
                {
                    var newName = txt.Replace("my name is", "").Trim();
                    if (!String.IsNullOrWhiteSpace(newName))
                        this.Name = newName;
                }
                break;
        }
    }
    

    [RelayCommand(CanExecute = nameof(CanStop))]
    async Task Stop()
    {
        this.Status = PlayState.WinStop;
        this.WinAmount = this.Amount;
        this.StopVault = this.Vault;

        await this.Speak(
            500,
            $"Good Job {this.Name}",
            $"You won {this.Amount} dollars",
            "Let's see what you could have won"
        );

        // TODO: this keeps going which screws up starting a new game
        while (await this.TryNextRound())
            await Task.Delay(500);

        // Save after reveal so PotentialAmount (this.Amount) reflects full game
        await this.SaveGameResult();
    }
    bool CanStop() => this.Vault > 0 && this.Status == PlayState.InProgress;

    
    [RelayCommand]
    void PlaySound(string sound)
    {
        if (sound == "lose")
            sounds.PlayAlarm();
        else
            sounds.PlayJackpot();
    }

    
    async Task NextRound()
    {
        var index = this.randomizer.Next(0, NextVaultStatements.Length);
        var announce = NextVaultStatements[index];
        await this.Speak(1000, announce);

        if (await this.TryNextRound())
            await textToSpeech.SpeakAsync("Do you wish to continue?");
    }


    async Task<bool> TryNextRound()
    {
        var next = false;
        this.Vault++;

        if (this.Vault == this.Rounds)
        {
            if (this.IsJackpot)
            {
                if (this.Status != PlayState.WinStop)
                {
                    this.Status = PlayState.Win;
                    this.WinAmount = 1000000;
                    await this.SaveGameResult();
                }
                sounds.PlayJackpot();
            }
            else
            {
                if (this.Status != PlayState.WinStop)
                {
                    this.WinAmount = 0;
                    this.Status = PlayState.Lose;
                    await this.SaveGameResult();
                }
                await this.Speak(500, $"Vault {this.Vault}");
                sounds.PlayAlarm();
            }
        }
        else
        {
            if (this.Status != PlayState.WinStop)
                this.Status = PlayState.InProgress;

            this.Amount += this.GetNextAmount();
            await this.Speak(
                500,
                $"Vault {this.Vault}",
                $"{this.Amount} dollars"
            );
            next = true;
        }
        return next;
    }


    List<int> amounts;
    int GetNextAmount()
    {
        if (this.amounts == null)
        {
            this.amounts = new();
            this.amounts.AddRange(Enumerable.Repeat(50, 10));
            this.amounts.AddRange(Enumerable.Repeat(100, 25));
            this.amounts.AddRange(Enumerable.Repeat(200, 25));
            this.amounts.AddRange(Enumerable.Repeat(250, 25));
            this.amounts.AddRange(Enumerable.Repeat(300, 25));
            this.amounts.AddRange(Enumerable.Repeat(500, 25));
            this.amounts.AddRange(Enumerable.Repeat(1000, 25));
        }
        var index = new Random().Next(0, this.amounts.Count);
        var amount = this.amounts[index];
        return amount;
    }


    Task SaveGameResult() => mediator.Send(new SaveGameResultCommand(
        this.Name,
        this.Status,
        this.WinAmount,
        this.Amount,
        this.Vault,
        this.Rounds,
        this.StopVault,
        this.IsJackpot
    ));


    async Task Speak(int pauseBetween, params string[] sentences)
    {
        foreach (var s in sentences)
        {
            await textToSpeech.SpeakAsync(s);
            await Task.Delay(pauseBetween);
        }
    }
}


public enum PlayState
{
    InProgress,
    Win,
    WinStop,
    Lose
}