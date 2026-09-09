// Compiled only once the Google Mobile Ads Unity SDK has been imported AND the
// "RTA_ADMOB_SDK" scripting define symbol has been added in Player Settings.
// Until then, AdManager transparently falls back to StubAdProvider so the game
// always builds and plays correctly with zero ad dependency (Section 36).
#if RTA_ADMOB_SDK
using System;
using GoogleMobileAds.Api;
using UnityEngine;

namespace ReleaseTheArrow.Ads
{
    internal class GoogleMobileAdsProvider : MonoBehaviour, IBannerAdProvider, IInterstitialAdProvider, IRewardedAdProvider
    {
        private BannerView _bannerView;
        private InterstitialAd _interstitialAd;
        private RewardedAd _rewardedAd;

        public bool IsRewardedReady => _rewardedAd != null && _rewardedAd.CanShowAd();

        private void Awake()
        {
            MobileAds.Initialize(_ =>
            {
                LoadInterstitial();
                LoadRewarded();
            });
        }

        public void ShowBanner()
        {
            if (_bannerView == null)
            {
                _bannerView = new BannerView(AdMobConfig.BannerId, AdSize.Banner, AdPosition.Bottom);
            }
            _bannerView.LoadAd(new AdRequest());
            _bannerView.Show();
        }

        public void HideBanner() => _bannerView?.Hide();

        public void LoadInterstitial()
        {
            InterstitialAd.Load(AdMobConfig.InterstitialId, new AdRequest(), (ad, error) =>
            {
                if (error != null || ad == null)
                {
                    Debug.LogWarning($"[Ads] Interstitial failed to load: {error?.GetMessage()}");
                    return;
                }
                _interstitialAd = ad;
            });
        }

        public void ShowInterstitial(Action onShown, Action onFailed)
        {
            if (_interstitialAd == null || !_interstitialAd.CanShowAd())
            {
                onFailed?.Invoke();
                LoadInterstitial();
                return;
            }

            _interstitialAd.OnAdFullScreenContentClosed += () => LoadInterstitial();
            _interstitialAd.Show();
            onShown?.Invoke();
        }

        public void LoadRewarded()
        {
            RewardedAd.Load(AdMobConfig.RewardedId, new AdRequest(), (ad, error) =>
            {
                if (error != null || ad == null)
                {
                    Debug.LogWarning($"[Ads] Rewarded ad failed to load: {error?.GetMessage()}");
                    return;
                }
                _rewardedAd = ad;
            });
        }

        public void ShowRewarded(Action onRewardEarned, Action onFailedOrSkipped)
        {
            if (_rewardedAd == null || !_rewardedAd.CanShowAd())
            {
                onFailedOrSkipped?.Invoke();
                LoadRewarded();
                return;
            }

            bool rewardGranted = false;
            _rewardedAd.OnAdFullScreenContentClosed += () =>
            {
                LoadRewarded();
                if (!rewardGranted) onFailedOrSkipped?.Invoke();
            };

            _rewardedAd.Show(_ =>
            {
                rewardGranted = true;
                onRewardEarned?.Invoke();
            });
        }
    }
}
#endif
