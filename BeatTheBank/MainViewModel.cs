using System.ComponentModel;
using System.Reactive;
using CommunityToolkit.Maui.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace BeatTheBank;


[ShellMap<MainPage>(registerRoute: false)]
public partial class MainViewModel(
    ILogger<MainViewModel> logger,
    ITextToSpeech textToSpeech,
    ISpeechToText speechRecognizer,
    IDeviceDisplay deviceDisplay,
    SoundEffectService sounds
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
    int rounds = 0;
    bool isJackpot = false;


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
        this.NotifyExecuteChanged();
        // this.WhenAnyValue(x => x.UseSpeechRecognition)
        //     .Skip(1)
        //     .Subscribe(use =>
        //     {
        //         if (!use)
        //             this.Deactivate();
        //         else
        //             this.Speech.Execute(null);
        //     });
    }

    public void OnDisappearing() {}
    
    [ObservableProperty] bool useSpeechRecognition;
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
        this.rounds = this.randomizer.Next(4, 15);
        this.isJackpot = this.randomizer.Next(1, 40) == 39; // 1 in 40 chance
        
        logger.LogDebug($"Rounds: {this.rounds} - Jackpot: {this.isJackpot}");
        
        await this.Speak(1000, $"Good Luck {this.Name}.  Let's play!");
        await this.NextRound();
    }
    bool CanStartOver() => !String.IsNullOrWhiteSpace(this.Name);

    [RelayCommand(CanExecute = nameof(CanContinue))]
    Task Continue() => this.NextRound();
    bool CanContinue() => this.Vault < this.rounds && this.Status == PlayState.InProgress;

    [RelayCommand]
    async Task Speech()
    {
        var granted = await speechRecognizer.RequestPermissions();
        
        
    //     speechToText.RecognitionResultUpdated += OnRecognitionTextUpdated;
    //     speechToText.RecognitionResultCompleted += OnRecognitionTextCompleted;
    //     await speechToText.StartListenAsync(CultureInfo.CurrentCulture, CancellationToken.None);
    // }
    //
    // async Task StopListening(CancellationToken cancellationToken)
    // {
    //     await speechToText.StopListenAsync(CancellationToken.None);
    //     speechToText.RecognitionResultUpdated -= OnRecognitionTextUpdated;
    //     speechToText.RecognitionResultCompleted -= OnRecognitionTextCompleted;
        // var recognitionResult = await speechToText.ListenAsync(
        //     CultureInfo.GetCultureInfo(Language),
        //     new Progress<string>(partialText =>
        //     {
        //         RecognitionText += partialText + " ";
        //     }), cancellationToken);
        //
        // if (recognitionResult.IsSuccessful)
        // {
        //     RecognitionText = recognitionResult.Text;
        // }
        // else
        // {
        //     await Toast.Make(recognitionResult.Exception?.Message ?? "Unable to recognize speech").Show(CancellationToken.None);
        // }
        // var result = await this.speechRecognizer.RequestAccess();
        // if (result == AccessState.Available)
        // {
        //     this.speechRecognizer
        //         .ListenUntilPause()
        //         .SubOnMainThread(x =>
        //         {
        //             Console.WriteLine("Statement: " + x);
        //             var value = x.ToLower();
        //
        //             switch (value)
        //             {
        //                 case "yes":
        //                 case "next":
        //                 case "keep going":
        //                 case "continue":
        //                 case "go":
        //                     if (this.Continue.CanExecute(null))
        //                         this.Continue.Execute(null);
        //                     break;
        //
        //                 case "no":
        //                 case "stop":
        //                     if (this.Stop.CanExecute(null))
        //                         this.Stop.Execute(null);
        //                     break;
        //
        //                 case "try again":
        //                 case "start over":
        //                 case "restart":
        //                     if (this.StartOver.CanExecute(null))
        //                         this.StartOver.Execute(null);
        //                     break;
        //
        //                 default:
        //                     if (value.StartsWith("my name is"))
        //                     {
        //                         var newName = value.Replace("my name is", "").Trim();
        //                         if (!newName.IsEmpty())
        //                             this.Name = newName;
        //                     }
        //                     break;
        //             }
        //         })
        //         .DisposedBy(this.DeactivateWith);
        // }
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
        
        while (await this.TryNextRound() && this.Status == PlayState.WinStop)
            await Task.Delay(500);
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

        if (this.Vault == this.rounds)
        {
            if (this.isJackpot)
            {
                if (this.Status != PlayState.WinStop)
                {
                    this.Status = PlayState.Win;
                    this.WinAmount = 1000000;
                }
                sounds.PlayJackpot();
            }
            else
            {
                if (this.Status != PlayState.WinStop)
                {
                    this.WinAmount = 0;
                    this.Status = PlayState.Lose;
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