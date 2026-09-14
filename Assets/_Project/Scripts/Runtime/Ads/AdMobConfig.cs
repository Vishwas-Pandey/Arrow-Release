namespace ReleaseTheArrow.Ads
{
    /// Ad unit IDs. useTestAds must stay true for every development/QA build — flip it only for
    /// the real Play Store submission build, right before upload (per project checklist).
    /// Google's official test ad unit IDs below are the same for every developer and are safe to
    /// commit; the "Real*" fields are placeholders — fill them in from the AdMob dashboard once
    /// this app's ad units exist there, but never commit real IDs alongside useTestAds left on.
    public static class AdMobConfig
    {
        public const bool UseTestAds = false;

        public const string TestAppId = "ca-app-pub-3940256099942544~3347511713";
        public const string TestBannerId = "ca-app-pub-3940256099942544/6300978111";
        public const string TestInterstitialId = "ca-app-pub-3940256099942544/1033173712";
        public const string TestRewardedId = "ca-app-pub-3940256099942544/5224354917";

        public const string RealAppId = "ca-app-pub-2894715849700422~9446696824";
        public const string RealBannerId = "ca-app-pub-2894715849700422/8557121138";
        public const string RealInterstitialId = "ca-app-pub-2894715849700422/7244039462";
        public const string RealRewardedId = "ca-app-pub-2894715849700422/2139052444";

        public static string AppId => UseTestAds ? TestAppId : RealAppId;
        public static string BannerId => UseTestAds ? TestBannerId : RealBannerId;
        public static string InterstitialId => UseTestAds ? TestInterstitialId : RealInterstitialId;
        public static string RewardedId => UseTestAds ? TestRewardedId : RealRewardedId;
    }
}
