using Shiny.SpeechRecognition;

namespace BeatTheBank;


public class MainViewModel : ViewModel
{
    static readonly string[] NextVaultStatements = new[]
    {
        "Alright, let's open it up",
        "Taking a chance and going for it",
        "Let's see what's in the next vault",
        "Let's do this",
        "Come on big money!"
    };

    readonly ITextToSpeech textToSpeech;
    readonly ISpeechRecognizer speechRecognizer;
    readonly SoundEffectService sounds;
    readonly Random randomizer = new();
    int rounds = 0;
    bool isJackpot = false;


    public MainViewModel(
        BaseServices services,
        ITextToSpeech textToSpeech,
        ISpeechRecognizer speechRecognizer,
        IDeviceDisplay deviceDisplay,
        SoundEffectService sounds
    ) : base(services)
    {
        this.textToSpeech = textToSpeech;
        this.speechRecognizer = speechRecognizer;
        this.sounds = sounds;
        deviceDisplay.KeepScreenOn = true;

        this.StartOver = ReactiveCommand.CreateFromTask
        (
            async () =>
            {
                this.Status = PlayState.InProgress;
                this.Vault = 0;
                this.Amount = 0;
                this.WinAmount = 0;
                this.StopVault = 0;
                this.rounds = this.randomizer.Next(4, 15);
                this.isJackpot = this.randomizer.Next(1, 40) == 39; // 1 in 40 chance

                Console.WriteLine($"Rounds: {this.rounds} - Jackpot: {this.isJackpot}");

                await this.Speak(1000, $"Good Luck {this.Name}.  Let's play!");
                await this.NextRound();
            },
            this.WhenAny(
                x => x.Name,
                x => !x.GetValue().IsEmpty()
            )
        );

        this.Continue = ReactiveCommand.CreateFromTask(
            async () => await this.NextRound(),
            this.WhenAny(
                x => x.Vault,
                x => x.Status,
                (v, st) => v.GetValue() < this.rounds && st.GetValue() == PlayState.InProgress
            )
        );

        this.Stop = ReactiveCommand.CreateFromTask
        (
            async () =>
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

                while (await this.TryNextRound())
                    await Task.Delay(500);
            },
            this.WhenAny(
                x => x.Vault,
                x => x.Status,
                (v, st) => v.GetValue() > 0 && st.GetValue() == PlayState.InProgress
            )
        );

        this.Speech = ReactiveCommand.CreateFromTask(async () =>
        {
            var result = await this.speechRecognizer.RequestAccess();
            if (result == AccessState.Available)
            {
                this.speechRecognizer
                    .ListenUntilPause()
                    .SubOnMainThread(x =>
                    {
                        Console.WriteLine("Statement: " + x);
                        var value = x.ToLower();

                        switch (value)
                        {
                            case "yes":
                            case "next":
                            case "keep going":
                            case "continue":
                            case "go":
                                if (this.Continue.CanExecute(null))
                                    this.Continue.Execute(null);
                                break;

                            case "no":
                            case "stop":
                                if (this.Stop.CanExecute(null))
                                    this.Stop.Execute(null);
                                break;

                            case "try again":
                            case "start over":
                            case "restart":
                                if (this.StartOver.CanExecute(null))
                                    this.StartOver.Execute(null);
                                break;

                            default:
                                if (value.StartsWith("my name is"))
                                {
                                    var newName = value.Replace("my name is", "").Trim();
                                    if (!newName.IsEmpty())
                                        this.Name = newName;
                                }
                                break;
                        }
                    })
                    .DisposedBy(this.DeactivateWith);
            }
        });

        this.PlaySound = ReactiveCommand.Create<string>(x =>
        {
            if (x == "lose")
                this.sounds.PlayAlarm();
            else
                this.sounds.PlayJackpot();
        });

        this.WhenAnyValue(x => x.UseSpeechRecognition)
            .Skip(1)
            .Subscribe(use =>
            {
                if (!use)
                    this.Deactivate();
                else
                    this.Speech.Execute(null);
            });
    }


    [Reactive] public bool UseSpeechRecognition { get; set; }
    [Reactive] public string Name { get; set; }
    [Reactive] public int Vault { get; private set; }
    [Reactive] public int StopVault { get; private set; }
    [Reactive] public int WinAmount { get; private set; }
    [Reactive] public int Amount { get; private set; }
    [Reactive] public PlayState Status { get; private set; }
    public ICommand StartOver { get; }
    public ICommand Continue { get; }
    public ICommand Speech { get; }
    public ICommand Stop { get; }
    public ICommand PlaySound { get; }


    async Task NextRound()
    {
        var index = this.randomizer.Next(0, NextVaultStatements.Length);
        var announce = NextVaultStatements[index];
        await this.Speak(1000, announce);

        if (await this.TryNextRound())
            await this.textToSpeech.SpeakAsync("Do you wish to continue?");
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
                this.sounds.PlayJackpot();
            }
            else
            {
                if (this.Status != PlayState.WinStop)
                {
                    this.WinAmount = 0;
                    this.Status = PlayState.Lose;
                }
                await this.Speak(500, $"Vault {this.Vault}");
                this.sounds.PlayAlarm();
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
            await this.textToSpeech.SpeakAsync(s);
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