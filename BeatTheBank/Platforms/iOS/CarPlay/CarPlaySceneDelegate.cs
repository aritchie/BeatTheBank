using CarPlay;
using Foundation;
using UIKit;

namespace BeatTheBank;

[Register("CarPlaySceneDelegate")]
public class CarPlaySceneDelegate : CPTemplateApplicationSceneDelegate
{
    CPInterfaceController? interfaceController;
    CarPlayLeaderboardManager? leaderboardManager;
    CarPlayGameManager? gameManager;

    public override void DidConnect(CPTemplateApplicationScene templateApplicationScene, CPInterfaceController @interfaceController)
    {
        this.interfaceController = interfaceController;
        this.leaderboardManager = new CarPlayLeaderboardManager(interfaceController, this.StartGame);
        this.leaderboardManager.Show();
    }

    public override void DidDisconnect(CPTemplateApplicationScene templateApplicationScene, CPInterfaceController @interfaceController)
    {
        this.gameManager?.Cleanup();
        this.gameManager = null;
        this.leaderboardManager?.Cleanup();
        this.leaderboardManager = null;
        this.interfaceController = null;
    }

    void StartGame(string playerName)
    {
        if (this.interfaceController == null)
            return;

        this.gameManager?.Cleanup();
        this.gameManager = new CarPlayGameManager(this.interfaceController, this.OnGameExit);
        this.gameManager.StartGame(playerName);
    }

    void OnGameExit()
    {
        this.gameManager?.Cleanup();
        this.gameManager = null;
        this.interfaceController?.PopTemplate(true, null);
        this.leaderboardManager?.Show();
    }
}
