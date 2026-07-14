using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Content;
using Android.Views;
using Android.Widget;
#if GOOGLE_ADS
using Android.Gms.Ads;
#endif
using Microsoft.Xna.Framework;

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
#if GOOGLE_ADS
        AdView AdBanner;

        // Listener adapter for MobileAds initialization callback
        private class InitCompleteListener : Java.Lang.Object, Android.Gms.Ads.Initialization.IOnInitializationCompleteListener
        {
            public void OnInitializationComplete(Android.Gms.Ads.Initialization.IInitializationStatus status)
            {
                // no-op
            }
        }
#endif

#if GOOGLE_ADS
        private void InitializeAds(bool personalized = true)
        {
            try
            {
                MobileAds.Initialize(this, new InitCompleteListener());

                // Create banner (TEST AD unit)
                var ai = Application.Context.PackageManager.GetApplicationInfo(Application.Context.PackageName, Android.Content.PM.PackageInfoFlags.MetaData);
                var bannerId = ai.MetaData?.GetString("ADMOB_BANNER_ID");
                if (string.IsNullOrEmpty(bannerId))
                {
                    // no banner id provided; skip creating banner
                    return;
                }

                AdBanner = new AdView(this) { AdUnitId = bannerId, AdSize = AdSize.FullBanner };

                var adParams = new RelativeLayout.LayoutParams(
                    RelativeLayout.LayoutParams.WrapContent,
                    RelativeLayout.LayoutParams.WrapContent);

                adParams.AddRule(LayoutRules.AlignParentBottom);

                _layout.AddView(AdBanner, adParams);

                var builder = new AdRequest.Builder();
                if (!personalized)
                {
                    var extras = new Android.OS.Bundle();
                    extras.PutString("npa", "1");
                    try
                    {
                        var admobAdapterClass = Java.Lang.Class.ForName("com.google.ads.mediation.admob.AdMobAdapter");
                        builder.AddNetworkExtrasBundle(admobAdapterClass, extras);
                    }
                    catch
                    {
                        // fallback: ignore if adapter class not found
                    }
                }

                var request = builder.Build();
                AdBanner.LoadAd(request);
            }
            catch
            {
                // ignore
            }
        }
#else
        private void InitializeAds() { }
#endif

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

            // Cookie / consent for ads (GDPR-friendly simple flow)
            var prefs = GetSharedPreferences("tetros_prefs", FileCreationMode.Private);
            var consent = prefs.GetString("cookie_consent", null);
            if (consent == null)
            {
                // Show blocking consent dialog with options
                new AlertDialog.Builder(this)
                    .SetTitle("Cookies & Ads")
                    .SetMessage("This app may show ads. Choose personalized ads, non-personalized ads, or decline ads.")
                    .SetCancelable(false)
                    .SetPositiveButton("Personalized", (sender, args) =>
                    {
                        prefs.Edit().PutString("cookie_consent", "personalized").Commit();
                        InitializeAds(true);
                    })
                    .SetNeutralButton("Non-personalized", (sender, args) =>
                    {
                        prefs.Edit().PutString("cookie_consent", "nonpersonalized").Commit();
                        InitializeAds(false);
                    })
                    .SetNegativeButton("Decline", (sender, args) =>
                    {
                        prefs.Edit().PutString("cookie_consent", "declined").Commit();
                    })
                    .Show();
            }
            else if (consent == "personalized")
            {
                InitializeAds(true);
            }
            else if (consent == "nonpersonalized")
            {
                InitializeAds(false);
            }

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
