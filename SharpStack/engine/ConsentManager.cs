using System;
using System.Threading.Tasks;

#if GOOGLE_ADS
using Android.App;
using Android.Content;
using Google.Android.Gms.Ads;
using Xamarin.Google.UserMesssagingPlatform;

namespace SharpStack.engine
{
    /// <summary>
    /// ConsentManager implements Google User Messaging Platform (UMP) flow.
    /// This class manages GDPR consent via the official UMP SDK.
    /// Does not store Activity references permanently — takes Activity as parameters in methods.
    /// </summary>
    internal class ConsentManager
    {
        private IConsentInformation _consentInformation;

        public bool CanRequestAds => _consentInformation?.CanRequestAds() ?? true;

        public bool PrivacyOptionsRequired
        {
            get
            {
                try
                {
                    // PrivacyOptionsRequirementStatus is: 0 = REQUIRED, 1 = NOT_REQUIRED
                    var status = _consentInformation?.PrivacyOptionsRequirementStatus;
                    // True if status is 0 (REQUIRED)
                    return status?.Ordinal() == 0;
                }
                catch
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// Runs the full UMP flow and completes when consent is resolved.
        /// After this task completes, check CanRequestAds to decide whether to load ads.
        /// </summary>
        public Task InitializeConsentAndAdsAsync(Activity activity)
        {
            if (activity == null) throw new ArgumentNullException(nameof(activity));
            var tcs = new TaskCompletionSource<bool>();

            try
            {
                // Get the singleton ConsentInformation instance
                _consentInformation = UserMessagingPlatform.GetConsentInformation(activity);
                Android.Util.Log.Info("ConsentManager", "Got ConsentInformation singleton");

                // Do not check ConsentStatus here: RequestConsentInfoUpdate must be called
                // to refresh the stored consent state. The update listener will determine
                // whether the consent form needs to be shown or the consent has already
                // been obtained and persisted on the device.
                Android.Util.Log.Info("ConsentManager", "Requesting consent info update to determine stored consent status");

                // Build consent request parameters
                var paramBuilder = new ConsentRequestParameters.Builder();

                // Debug: You can uncomment SetDebugGeography to force consent form display for testing
                // WARNING: This FORCES the form to show every app launch and prevents consent from being saved!
                // Only use this temporarily for testing the consent form itself.
                // Comment this out to test the normal flow where consent is saved and form only shows once.
                //
                // try
                // {
                //     var debugBuilder = new ConsentDebugSettings.Builder(activity);
                //     debugBuilder.SetDebugGeography(1); // 1 = EEA
                //     // Uncomment and add your test device hash ID from first run logcat:
                //     // debugBuilder.AddTestDeviceHashedId("XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX");
                //     var debugSettings = debugBuilder.Build();
                //     paramBuilder.SetConsentDebugSettings(debugSettings);
                // }
                // catch (System.Exception ex)
                // {
                //     Android.Util.Log.Warn("ConsentManager", "Failed to set debug consent settings: " + ex);
                // }

                var consentParams = paramBuilder.Build();
                Android.Util.Log.Info("ConsentManager", "Built ConsentRequestParameters");

                // Request consent info update with listeners
                _consentInformation.RequestConsentInfoUpdate(
                    activity,
                    consentParams,
                    new OnConsentInfoUpdateSuccessListener(activity, _consentInformation, tcs),
                    new OnConsentInfoUpdateFailureListener(tcs));
                Android.Util.Log.Info("ConsentManager", "Called RequestConsentInfoUpdate");
            }
            catch (System.Exception ex)
            {
                Android.Util.Log.Warn("ConsentManager", "Failed to initialize consent flow: " + ex);
                tcs.TrySetResult(true);
            }

            return tcs.Task;
        }

        /// <summary>
        /// Shows the privacy options form (allows user to change consent after initial choice).
        /// This is required by Google Play policy.
        /// </summary>
        public void ShowPrivacyOptionsForm(Activity activity)
        {
            if (activity == null) return;

            try
            {
                UserMessagingPlatform.ShowPrivacyOptionsForm(activity, new OnPrivacyOptionsFormDismissedListener());
            }
            catch (System.Exception ex)
            {
                Android.Util.Log.Warn("ConsentManager", "Failed to show privacy options form: " + ex);
            }
        }

        // --- Listener implementations ---

        /// <summary>
        /// Listener for successful consent info update. If form is available, load and show it.
        /// </summary>
        private class OnConsentInfoUpdateSuccessListener : Java.Lang.Object, IConsentInformationOnConsentInfoUpdateSuccessListener
        {
            private readonly Activity _activity;
            private readonly IConsentInformation _consentInfo;
            private readonly TaskCompletionSource<bool> _tcs;

            public OnConsentInfoUpdateSuccessListener(Activity activity, IConsentInformation consentInfo, TaskCompletionSource<bool> tcs)
            {
                _activity = activity;
                _consentInfo = consentInfo;
                _tcs = tcs;
            }

            public void OnConsentInfoUpdateSuccess()
            {
                try
                {
                    var status = _consentInfo.ConsentStatus;
                    Android.Util.Log.Info("ConsentManager", $"OnConsentInfoUpdateSuccess called. ConsentStatus: {status}, IsConsentFormAvailable: {_consentInfo.IsConsentFormAvailable}");

                    // If consent was already obtained and persisted (OBTAINED == 3), skip showing the form.
                    if (status == 3)
                    {
                        Android.Util.Log.Info("ConsentManager", "Consent already obtained (OBTAINED status), skipping consent form");
                        _tcs.TrySetResult(true);
                        return;
                    }

                    if (_consentInfo.IsConsentFormAvailable)
                    {
                        Android.Util.Log.Info("ConsentManager", "Loading consent form...");
                        // Load the consent form
                        UserMessagingPlatform.LoadConsentForm(
                            _activity,
                            new OnConsentFormLoadSuccessListener(_activity, _tcs),
                            new OnConsentFormLoadFailureListener(_tcs));
                    }
                    else
                    {
                        Android.Util.Log.Info("ConsentManager", "No consent form available, completing consent flow");
                        // No form needed; complete the flow
                        _tcs.TrySetResult(true);
                    }
                }
                catch (System.Exception ex)
                {
                    Android.Util.Log.Warn("ConsentManager", "Error in OnConsentInfoUpdateSuccess: " + ex);
                    _tcs.TrySetResult(true);
                }
            }
        }

        /// <summary>
        /// Listener for failed consent info update. Complete the flow anyway (non-personalized ads).
        /// </summary>
        private class OnConsentInfoUpdateFailureListener : Java.Lang.Object, IConsentInformationOnConsentInfoUpdateFailureListener
        {
            private readonly TaskCompletionSource<bool> _tcs;

            public OnConsentInfoUpdateFailureListener(TaskCompletionSource<bool> tcs)
            {
                _tcs = tcs;
            }

            public void OnConsentInfoUpdateFailure(FormError error)
            {
                if (error != null)
                {
                    Android.Util.Log.Warn("ConsentManager", $"Consent info update failed: {error}");
                }
                _tcs.TrySetResult(true);
            }
        }

        /// <summary>
        /// Listener for successful consent form load. Show the form.
        /// </summary>
        private class OnConsentFormLoadSuccessListener : Java.Lang.Object, UserMessagingPlatform.IOnConsentFormLoadSuccessListener
        {
            private readonly Activity _activity;
            private readonly TaskCompletionSource<bool> _tcs;

            public OnConsentFormLoadSuccessListener(Activity activity, TaskCompletionSource<bool> tcs)
            {
                _activity = activity;
                _tcs = tcs;
            }

            public void OnConsentFormLoadSuccess(IConsentForm consentForm)
            {
                try
                {
                    Android.Util.Log.Info("ConsentManager", "Consent form loaded successfully, showing form...");
                    consentForm.Show(_activity, new OnConsentFormDismissedListener(_tcs));
                }
                catch (System.Exception ex)
                {
                    Android.Util.Log.Warn("ConsentManager", "Error showing consent form: " + ex);
                    _tcs.TrySetResult(true);
                }
            }
        }

        /// <summary>
        /// Listener for failed consent form load. Complete the flow anyway.
        /// </summary>
        private class OnConsentFormLoadFailureListener : Java.Lang.Object, UserMessagingPlatform.IOnConsentFormLoadFailureListener
        {
            private readonly TaskCompletionSource<bool> _tcs;

            public OnConsentFormLoadFailureListener(TaskCompletionSource<bool> tcs)
            {
                _tcs = tcs;
            }

            public void OnConsentFormLoadFailure(FormError error)
            {
                if (error != null)
                {
                    Android.Util.Log.Warn("ConsentManager", $"Consent form load failed: {error}");
                }
                _tcs.TrySetResult(true);
            }
        }

        /// <summary>
        /// Listener for when consent form is dismissed (after user made choice).
        /// </summary>
        private class OnConsentFormDismissedListener : Java.Lang.Object, IConsentFormOnConsentFormDismissedListener
        {
            private readonly TaskCompletionSource<bool> _tcs;

            public OnConsentFormDismissedListener(TaskCompletionSource<bool> tcs)
            {
                _tcs = tcs;
            }

            public void OnConsentFormDismissed(FormError error)
            {
                if (error != null)
                {
                    Android.Util.Log.Warn("ConsentManager", $"Consent form error: {error}");
                }
                Android.Util.Log.Info("ConsentManager", "Consent form dismissed, consent flow complete");
                _tcs.TrySetResult(true);
            }
        }

        /// <summary>
        /// Listener for when privacy options form is dismissed.
        /// </summary>
        private class OnPrivacyOptionsFormDismissedListener : Java.Lang.Object, IConsentFormOnConsentFormDismissedListener
        {
            public void OnConsentFormDismissed(FormError error)
            {
                if (error != null)
                {
                    Android.Util.Log.Warn("ConsentManager", $"Privacy options form error: {error}");
                }
            }
        }
    }
}

#else

// No-op implementation when GOOGLE_ADS is not defined
namespace SharpStack.engine
{
    internal class ConsentManager
    {
        public bool CanRequestAds => true;
        public bool PrivacyOptionsRequired => false;
        public System.Threading.Tasks.Task InitializeConsentAndAdsAsync(object activity) => System.Threading.Tasks.Task.CompletedTask;
        public void ShowPrivacyOptionsForm(object activity) { }
    }
}

#endif
