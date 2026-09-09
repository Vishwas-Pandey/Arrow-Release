using System;
using System.Collections;
using ReleaseTheArrow.Utils;
using UnityEngine;

namespace ReleaseTheArrow.Ads
{
    /// Single entry point the rest of the game talks to for ads. Gates initialization behind
    /// UMP consent, frequency-caps interstitials, and chains however many rewarded ads a
    /// continue currently requires — never granting the reward until every one of them has
    /// actually completed. If ads are unavailable for any reason, every method here still
    /// resolves (never hangs) so gameplay is never blocked (Section 36).
    public class AdManager : MonoBehaviour
    {
        public static AdManager Instance { get; private set; }

        private const float MinInterstitialIntervalSeconds = 30f;
        private const int SkipInterstitialUpToLevel = 2;

        private IBannerAdProvider _banner;
        private IInterstitialAdProvider _interstitial;
        private IRewardedAdProvider _rewarded;
        private bool _adsAllowed;
        private float _lastInterstitialTime = -9999f;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            ConsentManager.GatherConsent(canRequestAds =>
            {
                _adsAllowed = canRequestAds;
                if (!_adsAllowed)
                {
                    Debug.Log("[Ads] Consent does not permit ad requests — ads disabled for this session.");
                    return;
                }
                InitializeProvider();
            });
        }

        private void InitializeProvider()
        {
            var providerGo = new GameObject("~AdProvider");
            providerGo.transform.SetParent(transform);

#if RTA_ADMOB_SDK
            var provider = providerGo.AddComponent<GoogleMobileAdsProvider>();
#else
            var provider = providerGo.AddComponent<StubAdProvider>();
#endif
            _banner = provider;
            _interstitial = provider;
            _rewarded = provider;
        }

        public void ShowBanner()
        {
            if (_adsAllowed) _banner?.ShowBanner();
        }

        public void HideBanner() => _banner?.HideBanner();

        /// Always resolves via onDone, whether or not an interstitial actually showed.
        public void MaybeShowLevelCompleteInterstitial(int justCompletedLevelId, Action onDone)
        {
            bool eligible = _adsAllowed && _interstitial != null
                && justCompletedLevelId > SkipInterstitialUpToLevel
                && Time.unscaledTime - _lastInterstitialTime >= MinInterstitialIntervalSeconds;

            if (!eligible)
            {
                onDone?.Invoke();
                return;
            }

            _interstitial.ShowInterstitial(
                onShown: () => { _lastInterstitialTime = Time.unscaledTime; onDone?.Invoke(); },
                onFailed: () => onDone?.Invoke());
        }

        /// Plays `adsRequired` rewarded ads back to back. onAllCompleted only fires if every one
        /// of them finished successfully; any failure or early skip calls onFailedOrCancelled and
        /// grants nothing, so the player never loses a continue attempt to a bad ad load.
        public void ShowRewardedSequence(int adsRequired, Action onAllCompleted, Action onFailedOrCancelled)
        {
            if (!_adsAllowed || _rewarded == null)
            {
                onFailedOrCancelled?.Invoke();
                return;
            }
            StartCoroutine(RunRewardedSequence(adsRequired, onAllCompleted, onFailedOrCancelled));
        }

        private IEnumerator RunRewardedSequence(int remaining, Action onAllCompleted, Action onFailedOrCancelled)
        {
            if (remaining <= 0)
            {
                onAllCompleted?.Invoke();
                yield break;
            }

            bool finished = false;
            bool success = false;
            _rewarded.ShowRewarded(
                onRewardEarned: () => { success = true; finished = true; },
                onFailedOrSkipped: () => { success = false; finished = true; });

            yield return new WaitUntil(() => finished);

            if (!success)
            {
                AnalyticsManager.AdEvent("rewarded", "failed_or_skipped");
                onFailedOrCancelled?.Invoke();
                yield break;
            }

            AnalyticsManager.AdEvent("rewarded", "completed");
            yield return RunRewardedSequence(remaining - 1, onAllCompleted, onFailedOrCancelled);
        }
    }
}
