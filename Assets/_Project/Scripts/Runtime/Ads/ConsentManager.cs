using System;
using System.Threading;
using UnityEngine;
#if RTA_ADMOB_SDK
using GoogleMobileAds.Ump.Api;
#endif

namespace ReleaseTheArrow.Ads
{
    /// UMP consent flow. Must run and resolve before AdManager initializes ads — ads should only
    /// ever be requested if the result says CanRequestAds() is true afterward.
    public static class ConsentManager
    {
        public static void GatherConsent(Action<bool> onComplete)
        {
#if RTA_ADMOB_SDK
            // The real UMP SDK can invoke these callbacks from a native SDK thread rather than
            // Unity's main thread (confirmed on-device: onConsentFormDismissed crashed
            // AdManager.InitializeProvider with "Internal_CreateGameObject can only be called
            // from the main thread"). Capturing the context here — while GatherConsent is still
            // being called from AdManager.Start(), on the main thread — lets every completion
            // below marshal back onto it via Post, regardless of which thread actually resolves.
            var mainThread = SynchronizationContext.Current;
            void CompleteOnMainThread(bool canRequestAds)
            {
                if (mainThread != null) mainThread.Post(_ => onComplete?.Invoke(canRequestAds), null);
                else onComplete?.Invoke(canRequestAds);
            }

            var request = new ConsentRequestParameters();
            ConsentInformation.Update(request, updateError =>
            {
                if (updateError != null)
                {
                    Debug.LogWarning($"[Consent] Update failed: {updateError.Message}");
                    CompleteOnMainThread(ConsentInformation.CanRequestAds());
                    return;
                }

                ConsentForm.LoadAndShowConsentFormIfRequired(formError =>
                {
                    if (formError != null) Debug.LogWarning($"[Consent] Form error: {formError.Message}");
                    CompleteOnMainThread(ConsentInformation.CanRequestAds());
                });
            });
#else
            onComplete?.Invoke(true);
#endif
        }
    }
}
