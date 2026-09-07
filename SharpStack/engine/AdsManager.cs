using System;
#if GOOGLE_ADS
using Android.Content;
using Android.Views;
using Android.Widget;
using Android.Gms.Ads;
#endif

namespace SharpStack.engine
{
    /// <summary>
    /// Lightweight Ads manager. When compiled with GOOGLE_ADS this contains the Android AdMob logic.
    /// Otherwise it's a no-op implementation so the class can be reused across projects without changes.
    /// </summary>
    internal class AdsManager
    {
#if GOOGLE_ADS
        private readonly Context _context;
        private readonly RelativeLayout _layout;
        private AdView _adBanner;
        private bool _initialized;

        // Listener adapter for MobileAds initialization callback
        private class InitCompleteListener : Java.Lang.Object, Android.Gms.Ads.Initialization.IOnInitializationCompleteListener
        {
            public void OnInitializationComplete(Android.Gms.Ads.Initialization.IInitializationStatus status)
            {
                // no-op
            }
        }

        public AdsManager(Context context, RelativeLayout layout)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _layout = layout ?? throw new ArgumentNullException(nameof(layout));
        }

        /// <summary>
        /// Initialize and show a banner ad. Pass personalized=false to request non-personalized ads.
        /// </summary>
        public void Initialize(bool personalized = true)
        {
            if (_initialized) return;

            try
            {
                MobileAds.Initialize(_context, new InitCompleteListener());

                var ai = _context.PackageManager.GetApplicationInfo(_context.PackageName, Android.Content.PM.PackageInfoFlags.MetaData);
                var bannerId = ai.MetaData?.GetString("ADMOB_BANNER_ID");
                if (string.IsNullOrEmpty(bannerId))
                {
                    // no banner id provided; nothing to do
                    return;
                }

                _adBanner = new AdView(_context) { AdUnitId = bannerId, AdSize = AdSize.FullBanner };

                var adParams = new RelativeLayout.LayoutParams(
                    RelativeLayout.LayoutParams.WrapContent,
                    RelativeLayout.LayoutParams.WrapContent);

                adParams.AddRule(LayoutRules.AlignParentBottom);

                _layout.AddView(_adBanner, adParams);

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
                        // ignore if adapter class not found
                    }
                }

                var request = builder.Build();
                _adBanner.LoadAd(request);
                _initialized = true;
            }
            catch (Exception ex)
            {
                try { Android.Util.Log.Warn("AdsManager", "Failed to initialize ads: " + ex); } catch { }
            }
        }
#else
        // No-op implementation when ads are not enabled
        public AdsManager(object context, object layout) { }
        public void Initialize(bool personalized = true) { }
#endif
    }
}
