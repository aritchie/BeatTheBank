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
    CPListTemplate? template;
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

        this.template = this.BuildTemplate();
        this.interfaceController.PushTemplate(this.template, true, null);
    }

    CPListTemplate BuildTemplate()
    {
        var sections = this.BuildSections();
        var template = new CPListTemplate("Beat The Bank", sections);
        template.BackButton = new CPBarButton("Back", _ => this.onGameExit());
        return template;
    }

    CPListSection[] BuildSections()
    {
        var vm = this.viewModel!;

        // Game status section
        var nameItem = new CPListItem($"Player: {vm.Name}", null);
        var vaultText = vm.Vault == 0 ? "Vault: —" : $"Vault: {vm.Vault}";
        var vaultItem = new CPListItem(vaultText, null);
        var amountItem = new CPListItem($"Amount: ${vm.Amount:N0}", $"Winnings: ${vm.WinAmount:N0}");

        var infoItems = new ICPListTemplateItem[] { nameItem, vaultItem, amountItem };
        var sections = new List<CPListSection> { new(infoItems, "Game Status", null) };

        // Result section (only when game ended)
        if (vm.Status == PlayState.Win)
            sections.Add(new CPListSection(new ICPListTemplateItem[] { new CPListItem("🎰 JACKPOT!", $"Won ${vm.WinAmount:N0}!") }, "Result", null));
        else if (vm.Status == PlayState.Lose)
            sections.Add(new CPListSection(new ICPListTemplateItem[] { new CPListItem("🚨 BUSTED!", "Better luck next time") }, "Result", null));
        else if (vm.Status == PlayState.WinStop)
            sections.Add(new CPListSection(new ICPListTemplateItem[] { new CPListItem("💰 Stopped!", $"Won ${vm.WinAmount:N0} at Vault {vm.StopVault}") }, "Result", null));

        // Action section
        var actionItems = new List<ICPListTemplateItem>();

        if (vm.StartOverCommand.CanExecute(null))
        {
            actionItems.Add(new CPListItem("Start Game", "Begin a new round")
            {
                Handler = (item, completion) =>
                {
                    if (vm.StartOverCommand.CanExecute(null))
                        vm.StartOverCommand.Execute(null);
                    completion();
                }
            });
        }

        if (vm.ContinueCommand.CanExecute(null))
        {
            actionItems.Add(new CPListItem("Continue to Next Vault", "Open next vault")
            {
                Handler = (item, completion) =>
                {
                    if (vm.ContinueCommand.CanExecute(null))
                        vm.ContinueCommand.Execute(null);
                    completion();
                }
            });
        }

        if (vm.StopCommand.CanExecute(null))
        {
            actionItems.Add(new CPListItem("Stop at Vault", "Lock in your winnings")
            {
                Handler = (item, completion) =>
                {
                    if (vm.StopCommand.CanExecute(null))
                        vm.StopCommand.Execute(null);
                    completion();
                }
            });
        }

        if (actionItems.Count > 0)
            sections.Add(new CPListSection(actionItems.ToArray(), "Actions", null));

        return sections.ToArray();
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
            var sections = this.BuildSections();
            this.template.UpdateSections(sections);
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
        this.scope?.Dispose();
        this.scope = null;
        this.template = null;
    }
}
