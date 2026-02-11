using BeatTheBank.Contracts;
using BeatTheBank.Services;
using Microsoft.Extensions.Logging;
using Shiny;
using Shiny.Mediator;

namespace BeatTheBank.Tests.ViewModels;

public class GameViewModelTests
{
    readonly ILogger<GameViewModel> logger;
    readonly INavigator navigator;
    readonly ISpeechService speech;
    readonly IDeviceDisplay deviceDisplay;
    readonly SoundEffectService sounds;
    readonly IMediator mediator;
    readonly GameViewModel vm;

    public GameViewModelTests()
    {
        logger = Substitute.For<ILogger<GameViewModel>>();
        navigator = Substitute.For<INavigator>();
        speech = Substitute.For<ISpeechService>();
        deviceDisplay = Substitute.For<IDeviceDisplay>();
        sounds = Substitute.For<SoundEffectService>();
        mediator = Substitute.For<IMediator>();

        vm = new GameViewModel(logger, navigator, speech, deviceDisplay, sounds, mediator);
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
    public void OnDisappearing_StopsListening()
    {
        vm.OnDisappearing();
        speech.Received(1).StopListening();
    }

    [Fact]
    public void PlaySoundCommand_PlaysAlarmForLose()
    {
        vm.PlaySoundCommand.Execute("lose");
        sounds.Received(1).PlayAlarm();
    }

    [Fact]
    public void PlaySoundCommand_PlaysJackpotForWin()
    {
        vm.PlaySoundCommand.Execute("win");
        sounds.Received(1).PlayJackpot();
    }
}
