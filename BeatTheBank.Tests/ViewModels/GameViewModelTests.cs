using BeatTheBank.Services;
using Microsoft.Extensions.Logging;
using Shiny;
using Shiny.Audio;
using Shiny.Speech;

namespace BeatTheBank.Tests.ViewModels;

public class GameViewModelTests
{
    readonly ILogger<GameViewModel> logger;
    readonly INavigator navigator;
    readonly IDialogs dialogs;
    readonly ISpeechToTextService stt;
    readonly ITextToSpeechService tts;
    readonly IDeviceDisplay deviceDisplay;
    readonly SoundEffectService sounds;
    readonly IMediator mediator;
    readonly GameViewModel vm;

    public GameViewModelTests()
    {
        logger = Substitute.For<ILogger<GameViewModel>>();
        navigator = Substitute.For<INavigator>();
        dialogs = Substitute.For<IDialogs>();
        stt = Substitute.For<ISpeechToTextService>();
        tts = Substitute.For<ITextToSpeechService>();
        deviceDisplay = Substitute.For<IDeviceDisplay>();
        sounds = Substitute.For<SoundEffectService>(
            Substitute.For<ILogger<SoundEffectService>>(),
            Substitute.For<IAudioPlayer>(),
            Substitute.For<IAudioPlayer>()
        );
        mediator = Substitute.For<IMediator>();

        vm = new GameViewModel(logger, navigator, dialogs, stt, tts, deviceDisplay, sounds, mediator);
    }

    [Fact]
    public void InitialState_AllDefaults()
    {
        vm.Vault.ShouldBe(0);
        vm.Amount.ShouldBe(0);
        vm.WinAmount.ShouldBe(0);
        vm.StopVault.ShouldBe(0);
        vm.Status.ShouldBe(PlayState.InProgress);
        vm.Rounds.ShouldBe(0);
    }

    [Fact]
    public void StartOverCommand_CannotExecute_WhenNameIsEmpty()
    {
        vm.Name = "";
        vm.StartOverCommand.CanExecute(null).ShouldBeFalse();
    }

    [Fact]
    public void StartOverCommand_CanExecute_WhenNameIsSet()
    {
        vm.Name = "Alice";
        vm.StartOverCommand.CanExecute(null).ShouldBeTrue();
    }

    [Fact]
    public void ContinueCommand_CannotExecute_WhenNotInProgress()
    {
        vm.ContinueCommand.CanExecute(null).ShouldBeFalse();
    }

    [Fact]
    public void StopCommand_CannotExecute_WhenVaultIsZero()
    {
        vm.StopCommand.CanExecute(null).ShouldBeFalse();
    }

    [Fact]
    public void OnAppearing_SetsKeepScreenOn()
    {
        vm.OnAppearing();
        deviceDisplay.KeepScreenOn.ShouldBeTrue();
    }

    [Fact]
    public void OnDisappearing_DoesNotThrow()
    {
        vm.OnDisappearing();
    }

    [Fact]
    public void CancelGameCommand_CannotExecute_WhenVaultIsZero()
    {
        vm.CancelGameCommand.CanExecute(null).ShouldBeFalse();
    }

    [Fact]
    public void CancelGameCommand_CannotExecute_WhenNotInProgress()
    {
        vm.CancelGameCommand.CanExecute(null).ShouldBeFalse();
    }

    [Fact]
    public async Task CancelGameCommand_NavigatesBack_WhenConfirmed()
    {
        dialogs.Confirm(Arg.Any<string?>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>())
            .Returns(true);

        // Simulate an in-progress game state
        vm.Name = "Alice";
        await vm.StartOverCommand.ExecuteAsync(null);

        await vm.CancelGameCommand.ExecuteAsync(null);
        await navigator.Received(1).GoBack();
    }

    [Fact]
    public async Task CancelGameCommand_DoesNotNavigateBack_WhenNotConfirmed()
    {
        dialogs.Confirm(Arg.Any<string?>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>())
            .Returns(false);

        vm.Name = "Alice";
        await vm.StartOverCommand.ExecuteAsync(null);

        await vm.CancelGameCommand.ExecuteAsync(null);
        await navigator.DidNotReceive().GoBack();
    }
}
