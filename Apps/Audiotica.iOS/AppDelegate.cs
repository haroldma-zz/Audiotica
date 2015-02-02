using Foundation;

using UIKit;

namespace Audiotica.iOS
{
    // The UIApplicationDelegate for the application. This class is responsible for launching the 
    // User Interface of the application, as well as listening (and optionally responding) to 
    // application events from iOS.
    [Register("AppDelegate")]
    public class AppDelegate : UIApplicationDelegate
    {
        public static AppDelegate Current
        {
            get
            {
                return UIApplication.SharedApplication.Delegate as AppDelegate;
            }
        }

        // class-level declarations
        public override UIWindow Window { get; set; }

        public Locator Locator { get; set; }

        // This method should be used to release shared resources and it should store the application state.
        // If your application supports background exection this method is called instead of WillTerminate
        // when the user quits.
        public override void DidEnterBackground(UIApplication application)
        {
        }

        // This method is invoked when the application has loaded and is ready to run. In this 
        // method you should instantiate the window, load the UI into it and then make the window
        // visible.
        // You have 17 seconds to return from this method, or iOS will terminate your application.
        public override bool FinishedLaunching(UIApplication application, NSDictionary launchOptions)
        {
            this.Locator = new Locator();
            return true;
        }

        public override async void OnActivated(UIApplication application)
        {
            await this.Locator.SqlService.InitializeAsync();
            await this.Locator.BgSqlService.InitializeAsync();
            await this.Locator.CollectionService.LoadLibraryAsync();
        }

        // This method is invoked when the application is about to move from active to inactive state.
        // OpenGL applications should use this method to pause.
        public override void OnResignActivation(UIApplication application)
        {
        }

        /// <summary>
        /// Called prior to the application returning from a backgrounded state.
        /// </summary>
        /// <param name="application">
        /// Reference to the UIApplication that invoked this delegate method.
        /// </param>
        /// <remarks>
        /// Immediately after this call, the application will call <see cref="M:MonoTouchUIKit.UIApplicationDelegate.OnActivated"/>.
        /// </remarks>
        public override void WillEnterForeground(UIApplication application)
        {
        }

        /// <summary>
        /// Called if the application is being terminated due to memory constraints or directly by the user.
        /// </summary>
        /// <param name="application">
        /// Reference to the UIApplication that invoked this delegate method.
        /// </param>
        /// <altmember cref="M:UIKit.UIApplicationDelegate.OnResignActivation"/>
        /// <altmember cref="M:UIKit.UIApplicationDelegate.WillEnterBackground"/>
        /// <remarks>
        /// <para>
        /// iOS applications are expected to be long-lived, with many transitions between activated and non-activated states (see <see cref="M:UIKit.UIApplicationDelegate.OnActivated"/>, <see cref="M:UIKit.UIApplicationDelegate.OnResignActivation"/>) and are typically only terminated by user command or, rarely, due to memory exhaustion (see <see cref="M:UIKit.UIApplicationDelegate.ReceiveMemoryWarning"/>).
        /// </para>
        /// <para>
        /// <img href="UIApplicationDelegate.Lifecycle.png"/>
        /// </para>
        /// </remarks>
        public override void WillTerminate(UIApplication application)
        {
        }
    }
}