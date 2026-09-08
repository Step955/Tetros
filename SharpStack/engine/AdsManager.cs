using System;
#if GOOGLE_ADS
using Android.Content;
using Android.App;
using Android.Views;
using Android.Widget;
using Google.Android.Gms.Ads;
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
        private ConsentManager _consentManager;

        // Listener adapter for MobileAds initialization callback
        private class InitCompleteListener : Java.Lang.Object, Google.Android.Gms.Ads.Initialization.IOnInitializationCompleteListener
        {
            public void OnInitializationComplete(Google.Android.Gms.Ads.Initialization.IInitializationStatus status)
            {
                // no-op
            }
        }

        public AdsManager(Context context, RelativeLayout layout)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _layout = layout ?? throw new ArgumentNullException(nameof(layout));
            _consentManager = new ConsentManager();
        }

        /// <summary>
        /// Request ad consent using the official Google UMP flow, then initialize ads based on consent.
        /// </summary>
        public void RequestConsentAndInitialize()
        {
            // Use official Google UMP consent flow.
            var activity = _context as Activity;
            if (activity == null)
            {
                Android.Util.Log.Warn("AdsManager", "Activity is null, skipping ads initialization");
                Initialize(false);
                return;
            }

            try
            {
                Android.Util.Log.Info("AdsManager", "Starting consent and ads initialization...");
                // Start the UMP consent flow. When completed, initialize ads based on consent status.
                _ = _consentManager.InitializeConsentAndAdsAsync(activity).ContinueWith(t =>
                {
                    try
                    {
                        // After consent flow completes, decide whether to initialize personalized ads
                        bool canRequest = _consentManager.CanRequestAds;
                        Android.Util.Log.Info("AdsManager", $"Consent flow complete. CanRequestAds: {canRequest}");
                        // CRITICAL: Must run on main thread for UI operations
                        activity.RunOnUiThread(() => Initialize(canRequest));
                    }
                    catch (Exception ex)
                    {
                        try { Android.Util.Log.Warn("AdsManager", "Consent continuation failed: " + ex); } catch { }
                        activity.RunOnUiThread(() => Initialize(false));
                    }
                });
            }
            catch (Exception ex)
            {
                try { Android.Util.Log.Warn("AdsManager", "Consent flow failed: " + ex); } catch { }
                // fallback: initialize non-personalized
                Initialize(false);
            }
        }

        /// <summary>
        /// Show the privacy options form to allow user to change their consent choice.
        /// This is required by Google Play policy.
        /// </summary>
        public void ShowPrivacyOptions()
        {
            var activity = _context as Activity;
            if (activity != null && _consentManager != null)
            {
                _consentManager.ShowPrivacyOptionsForm(activity);
            }
        }

        /// <summary>
        /// Whether privacy options are required to be shown to the user.
        /// </summary>
        public bool PrivacyOptionsRequired => _consentManager?.PrivacyOptionsRequired ?? false;

        /// <summary>
        /// Initialize and show a banner ad. Pass personalized=false to request non-personalized ads.
        /// </summary>
        public void Initialize(bool personalized = true)
        {
            if (_initialized) 
            {
                Android.Util.Log.Info("AdsManager", "Ads already initialized, skipping");
                return;
            }

            try
            {
                Android.Util.Log.Info("AdsManager", "Initializing MobileAds...");
                MobileAds.Initialize(_context, new InitCompleteListener());
                Android.Util.Log.Info("AdsManager", "MobileAds initialized");

                var ai = _context.PackageManager.GetApplicationInfo(_context.PackageName, Android.Content.PM.PackageInfoFlags.MetaData);
                var bannerId = ai.MetaData?.GetString("ADMOB_BANNER_ID");
                Android.Util.Log.Info("AdsManager", $"Banner ID from manifest: {bannerId}");

                if (string.IsNullOrEmpty(bannerId))
                {
                    // no banner id provided; nothing to do
                    Android.Util.Log.Warn("AdsManager", "No banner ID provided in manifest");
                    return;
                }

                Android.Util.Log.Info("AdsManager", "Creating AdView...");
                _adBanner = new AdView(_context) { AdUnitId = bannerId, AdSize = AdSize.FullBanner };

                var adParams = new RelativeLayout.LayoutParams(
                    RelativeLayout.LayoutParams.WrapContent,
                    RelativeLayout.LayoutParams.WrapContent);

                adParams.AddRule(LayoutRules.AlignParentBottom);

                _layout.AddView(_adBanner, adParams);
                Android.Util.Log.Info("AdsManager", "AdView added to layout");

                var builder = new AdRequest.Builder();
                if (!personalized)
                {
                    Android.Util.Log.Info("AdsManager", "Loading non-personalized ads");
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
                else
                {
                    Android.Util.Log.Info("AdsManager", "Loading personalized ads");
                }

                var request = builder.Build();
                Android.Util.Log.Info("AdsManager", "Calling LoadAd...");
                _adBanner.LoadAd(request);
                _initialized = true;
                Android.Util.Log.Info("AdsManager", "Ads loaded successfully");
            }
            catch (Exception ex)
            {
                try { Android.Util.Log.Warn("AdsManager", "Failed to initialize ads: " + ex); } catch { }
            }
        }
#else
        // No-op implementation when ads are not enabled
        public AdsManager(object context, object layout) { }
        public void RequestConsentAndInitialize() { }
        public void Initialize(bool personalized = true) { }
        public void ShowPrivacyOptions() { }
        public bool PrivacyOptionsRequired => false;
#endif
    }
}
