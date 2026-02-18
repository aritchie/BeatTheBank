using Foundation;
using UIKit;

namespace BeatTheBank;

[Register("AppDelegate")]
public class AppDelegate : MauiUIApplicationDelegate
{
	protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();

#if CARPLAY
	[Export("application:configurationForConnectingSceneSession:options:")]
	public override UISceneConfiguration GetConfiguration(UIApplication application, UISceneSession connectingSceneSession, UISceneConnectionOptions options)
	{
		if (connectingSceneSession.Role.GetConstant() == UIWindowSceneSessionRole.CarTemplateApplication.GetConstant())
		{
			var config = new UISceneConfiguration("CarPlay", connectingSceneSession.Role);
			config.DelegateType = typeof(CarPlaySceneDelegate);
			return config;
		}

		return new UISceneConfiguration("Default", connectingSceneSession.Role);
	}
#endif
}

