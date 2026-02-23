using System.ComponentModel;
using CarPlay;
using Foundation;
using Microsoft.Extensions.DependencyInjection;

namespace BeatTheBank;

public class CarPlayGameManager
{
    readonly CPInterfaceController interfaceController;
    readonly Action onGameExit;
    IServiceScope? scope;
    GameViewModel? viewModel;
    CPInformationTemplate? template;
    bool isCleanedUp;

    public CarPlayGameManager(CPInterfaceController interfaceController, Action onGameExit)
    {
        this.interfaceController = interfaceController;
        this.onGameExit = onGameExit;
    }

    public void StartGame(string playerName)
    {
        var services = IPlatformApplication.Current!.Services;
        this.scope = services.CreateScope();
        this.viewModel = this.scope.ServiceProvider.GetRequiredService<GameViewModel>();
        this.viewModel.Name = playerName;
        this.viewModel.PropertyChanged += this.OnViewModelPropertyChanged;
        this.viewModel.StartOverCommand.CanExecuteChanged += this.OnCommandCanExecuteChanged;
        this.viewModel.ContinueCommand.CanExecuteChanged += this.OnCommandCanExecuteChanged;
        this.viewModel.StopCommand.CanExecuteChanged += this.OnCommandCanExecuteChanged;

        this.template = this.BuildTemplate();
        this.interfaceController.PushTemplate(this.template, true, null);
    }

    CPInformationTemplate BuildTemplate()
    {
        var template = new CPInformationTemplate(
            "Beat The Bank",
            CPInformationTemplateLayout.TwoColumn,
            this.BuildInfoItems(),
            this.BuildActions()
        );
        template.BackButton = new CPBarButton("Back", _ => this.onGameExit());
        template.TrailingNavigationBarButtons = this.BuildNavBarButtons();
        return template;
    }

    CPInformationItem[] BuildInfoItems()
    {
        var vm = this.viewModel!;
        var items = new List<CPInformationItem>
        {
            new(vm.Name ?? "—", "Player"),
            new(vm.Vault == 0 ? "—" : vm.Vault.ToString(), "Vault"),
            new($"${vm.Amount:N0}", "Amount"),
            new($"${vm.WinAmount:N0}", "Winnings")
        };

        if (vm.Status == PlayState.Win)
            items.Add(new("🎰 JACKPOT!", $"Won ${vm.WinAmount:N0}!"));
        else if (vm.Status == PlayState.Lose)
            items.Add(new("🚨 BUSTED!", "Better luck next time"));
        else if (vm.Status == PlayState.WinStop)
            items.Add(new("💰 Stopped!", $"Won ${vm.WinAmount:N0} at Vault {vm.StopVault}"));

        return items.ToArray();
    }

    CPBarButton[] BuildNavBarButtons()
    {
        var vm = this.viewModel;
        if (vm == null || !vm.StartOverCommand.CanExecute(null))
            return [];

        return
        [
            new CPBarButton("Restart", _ =>
            {
                if (!vm.StartOverCommand.CanExecute(null))
                    return;

                if (vm.Vault > 0)
                {
                    var alert = new CPActionSheetTemplate(
                        "Start Over",
                        "Are you sure you want to start a new game?",
                        [
                            new CPAlertAction("Yes", CPAlertActionStyle.Default, _ =>
                            {
                                this.interfaceController.DismissTemplate(true, null);
                                vm.Vault = 0;
                                vm.StartOverCommand.Execute(null);
                            }),
                            new CPAlertAction("Cancel", CPAlertActionStyle.Cancel, _ =>
                            {
                                this.interfaceController.DismissTemplate(true, null);
                            })
                        ]
                    );
                    this.interfaceController.PresentTemplate(alert, true, null);
                }
                else
                {
                    vm.StartOverCommand.Execute(null);
                }
            })
        ];
    }

    CPTextButton[] BuildActions()
    {
        var vm = this.viewModel!;
        var actions = new List<CPTextButton>();

        if (vm.StartOverCommand.CanExecute(null) && vm.Vault == 0)
        {
            actions.Add(new CPTextButton("Start Game", CPTextButtonStyle.Confirm, _ =>
            {
                if (vm.StartOverCommand.CanExecute(null))
                    vm.StartOverCommand.Execute(null);
            }));
        }

        if (vm.ContinueCommand.CanExecute(null))
        {
            actions.Add(new CPTextButton("Continue", CPTextButtonStyle.Confirm, _ =>
            {
                if (vm.ContinueCommand.CanExecute(null))
                    vm.ContinueCommand.Execute(null);
            }));
        }

        if (vm.StopCommand.CanExecute(null))
        {
            actions.Add(new CPTextButton("Stop", CPTextButtonStyle.Cancel, _ =>
            {
                if (vm.StopCommand.CanExecute(null))
                    vm.StopCommand.Execute(null);
            }));
        }

        return actions.ToArray();
    }

    void OnCommandCanExecuteChanged(object? sender, EventArgs e)
    {
        if (this.isCleanedUp || this.template == null)
            return;

        this.UpdateDisplay();
    }

    void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (this.isCleanedUp || this.template == null)
            return;

        if (e.PropertyName is nameof(GameViewModel.Vault)
            or nameof(GameViewModel.Amount)
            or nameof(GameViewModel.Status)
            or nameof(GameViewModel.WinAmount)
            or nameof(GameViewModel.StopVault))
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
            if (this.isCleanedUp || this.template == null || this.viewModel == null)
                return;

            this.template.Items = this.BuildInfoItems();
            this.template.Actions = this.BuildActions();
            this.template.TrailingNavigationBarButtons = this.BuildNavBarButtons();
        });
    }

    public void Cleanup()
    {
        this.isCleanedUp = true;
        if (this.viewModel != null)
        {
            this.viewModel.PropertyChanged -= this.OnViewModelPropertyChanged;
            this.viewModel.StartOverCommand.CanExecuteChanged -= this.OnCommandCanExecuteChanged;
            this.viewModel.ContinueCommand.CanExecuteChanged -= this.OnCommandCanExecuteChanged;
            this.viewModel.StopCommand.CanExecuteChanged -= this.OnCommandCanExecuteChanged;
            this.viewModel.OnDisappearing();
            this.viewModel = null;
        }
        this.scope?.Dispose();
        this.scope = null;
        this.template = null;
    }
}
