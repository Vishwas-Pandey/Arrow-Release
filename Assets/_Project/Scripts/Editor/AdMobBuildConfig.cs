using UnityEditor;
using UnityEditor.Build;

namespace ReleaseTheArrow.EditorTools
{
    /// Flips the RTA_ADMOB_SDK scripting define that gates GoogleMobileAdsProvider/ConsentManager
    /// (see AdManager) between the real Google Mobile Ads SDK and the ad-free StubAdProvider.
    /// Kept as an explicit, separately-run step rather than something every build always does —
    /// enabling ads is a deliberate release decision, not an incidental side effect of building.
    public static class AdMobBuildConfig
    {
        private const string Define = "RTA_ADMOB_SDK";

        [MenuItem("Release The Arrow/Ads/Enable Real AdMob SDK (set define symbol)")]
        public static void EnableAdMobDefine() => SetDefine(true);

        [MenuItem("Release The Arrow/Ads/Disable Real AdMob SDK (back to stub provider)")]
        public static void DisableAdMobDefine() => SetDefine(false);

        public static void SetDefine(bool enabled)
        {
            var target = NamedBuildTarget.Android;
            string existing = PlayerSettings.GetScriptingDefineSymbols(target);
            var defines = new System.Collections.Generic.List<string>(
                existing.Split(new[] { ';' }, System.StringSplitOptions.RemoveEmptyEntries));
            bool has = defines.Contains(Define);

            if (enabled && !has)
            {
                defines.Add(Define);
                PlayerSettings.SetScriptingDefineSymbols(target, string.Join(";", defines));
                UnityEngine.Debug.Log($"[AdMobBuildConfig] Added {Define} scripting define for Android.");
            }
            else if (!enabled && has)
            {
                defines.RemoveAll(d => d == Define);
                PlayerSettings.SetScriptingDefineSymbols(target, string.Join(";", defines));
                UnityEngine.Debug.Log($"[AdMobBuildConfig] Removed {Define} scripting define for Android.");
            }
            else
            {
                UnityEngine.Debug.Log($"[AdMobBuildConfig] {Define} already {(enabled ? "enabled" : "disabled")} — no change.");
            }
        }
    }
}
