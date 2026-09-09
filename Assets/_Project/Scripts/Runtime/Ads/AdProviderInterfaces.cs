using System;

namespace ReleaseTheArrow.Ads
{
    public interface IBannerAdProvider
    {
        void ShowBanner();
        void HideBanner();
    }

    public interface IInterstitialAdProvider
    {
        void LoadInterstitial();
        /// onShown is called immediately if a valid ad displays; onFailed if none was ready.
        /// Never blocks gameplay — always resolves synchronously to one of the two callbacks.
        void ShowInterstitial(Action onShown, Action onFailed);
    }

    public interface IRewardedAdProvider
    {
        void LoadRewarded();
        /// onRewardEarned fires only if the ad played to completion. onFailedOrSkipped covers
        /// both "no ad available" and "player closed it early" — the caller must not treat a
        /// skipped ad as if the reward was granted, and must not treat a load failure as
        /// consuming the player's continue attempt.
        void ShowRewarded(Action onRewardEarned, Action onFailedOrSkipped);
        bool IsRewardedReady { get; }
    }
}
