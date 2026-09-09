using System;
using System.Collections;
using UnityEngine;

namespace ReleaseTheArrow.Ads
{
    /// Used whenever the real Google Mobile Ads SDK isn't compiled in (see AdManager). Simulates
    /// realistic load/show timing so the rest of the game — continue flow, banner placement,
    /// interstitial transitions — is fully testable in-editor with zero external dependency.
    internal class StubAdProvider : MonoBehaviour, IBannerAdProvider, IInterstitialAdProvider, IRewardedAdProvider
    {
        public bool IsRewardedReady { get; private set; } = true;

        private void Awake() => Debug.Log("[Ads] Using StubAdProvider — import the Google Mobile Ads SDK and define RTA_ADMOB_SDK for real ads.");

        public void ShowBanner() => Debug.Log("[Ads] (stub) banner shown");
        public void HideBanner() => Debug.Log("[Ads] (stub) banner hidden");

        public void LoadInterstitial() => Debug.Log("[Ads] (stub) interstitial loaded");

        public void ShowInterstitial(Action onShown, Action onFailed)
        {
            Debug.Log("[Ads] (stub) interstitial shown");
            onShown?.Invoke();
        }

        public void LoadRewarded() => IsRewardedReady = true;

        public void ShowRewarded(Action onRewardEarned, Action onFailedOrSkipped)
        {
            StartCoroutine(SimulateRewarded(onRewardEarned));
        }

        private IEnumerator SimulateRewarded(Action onRewardEarned)
        {
            Debug.Log("[Ads] (stub) rewarded ad playing...");
            yield return new WaitForSeconds(0.5f);
            onRewardEarned?.Invoke();
        }
    }
}
