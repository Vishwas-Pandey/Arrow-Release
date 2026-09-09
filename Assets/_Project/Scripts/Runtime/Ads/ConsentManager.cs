using System;
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
            var request = new ConsentRequestParameters();
            ConsentInformation.Update(request, updateError =>
            {
                if (updateError != null)
                {
                    Debug.LogWarning($"[Consent] Update failed: {updateError.Message}");
                    onComplete?.Invoke(ConsentInformation.CanRequestAds());
                    return;
                }

                ConsentForm.LoadAndShowConsentFormIfRequired(formError =>
                {
                    if (formError != null) Debug.LogWarning($"[Consent] Form error: {formError.Message}");
                    onComplete?.Invoke(ConsentInformation.CanRequestAds());
                });
            });
#else
            onComplete?.Invoke(true);
#endif
        }
    }
}
