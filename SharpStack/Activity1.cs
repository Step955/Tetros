using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Content;
using Android.Views;
using Android.Widget;
#if GOOGLE_ADS
using Google.Android.Gms.Ads;
#endif
using Microsoft.Xna.Framework;
using SharpStack.engine;

namespace SharpStack
{
    [Activity(
        Label = "SharpStack",
        MainLauncher = true,
        AlwaysRetainTaskState = true,
        LaunchMode = LaunchMode.SingleInstance,
        ScreenOrientation = ScreenOrientation.Portrait,
        ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.Keyboard | ConfigChanges.KeyboardHidden | ConfigChanges.ScreenSize
    )]
    public class Activity1 : AndroidGameActivity
    {
        private Game1 _game;
        private View _view;
        private RelativeLayout _layout;
        private AdsManager _adsManager;

        // Ads are managed by AdsManager (defined in SharpStack.engine). AdsManager is a thin wrapper
        // that contains platform-specific code under the GOOGLE_ADS symbol and is a no-op otherwise.

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            // Log AdMob APP ID from manifest for diagnostics
            try
            {
                var ai = PackageManager.GetApplicationInfo(PackageName, Android.Content.PM.PackageInfoFlags.MetaData);
                var appId = ai.MetaData != null ? ai.MetaData.GetString("com.google.android.gms.ads.APPLICATION_ID") : null;
                Android.Util.Log.Info("SharpStack", $"AdMob APP ID from manifest: {appId}");
            }
            catch (System.Exception ex)
            {
                Android.Util.Log.Warn("SharpStack", "Failed to read AdMob APP ID: " + ex);
            }

            if (Build.VERSION.SdkInt >= BuildVersionCodes.P)
            {
                Window.Attributes.LayoutInDisplayCutoutMode = LayoutInDisplayCutoutMode.ShortEdges;
            }

            if (Build.VERSION.SdkInt < BuildVersionCodes.R)
            {
#pragma warning disable CS0618
                Window.SetFlags(WindowManagerFlags.Fullscreen, WindowManagerFlags.Fullscreen);
#pragma warning restore CS0618
            }

            // MonoGame setup
            _game = new Game1();
            _view = _game.Services.GetService(typeof(View)) as View;

            // Create layout and add game view
            _layout = new RelativeLayout(this);
            if (_view != null)
            {
                _layout.AddView(_view);
            }

            // initialize AdsManager (no-op if GOOGLE_ADS not defined)
            _adsManager = new AdsManager(this, _layout);

            // Request consent and initialize ads (UMP with fallback is handled by AdsManager)
            _adsManager.RequestConsentAndInitialize();

            SetContentView(_layout);
            _game.Run();
        }

        public override void OnWindowFocusChanged(bool hasFocus)
        {
            base.OnWindowFocusChanged(hasFocus);
            if (!hasFocus) return;

            if (Build.VERSION.SdkInt >= BuildVersionCodes.R)
            {
                Window.SetDecorFitsSystemWindows(false);
                Window.InsetsController?.Hide(Android.Views.WindowInsets.Type.SystemBars());
            }
            else
            {
#pragma warning disable CS0618
                Window.DecorView.SystemUiVisibility = (StatusBarVisibility)(
                    SystemUiFlags.LayoutStable |
                    SystemUiFlags.LayoutHideNavigation |
                    SystemUiFlags.LayoutFullscreen |
                    SystemUiFlags.HideNavigation |
                    SystemUiFlags.Fullscreen |
                    SystemUiFlags.ImmersiveSticky);
#pragma warning restore CS0618
            }
        }
    }
}
