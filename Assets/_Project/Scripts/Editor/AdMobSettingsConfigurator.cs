using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace ReleaseTheArrow.EditorTools
{
    /// The Google Mobile Ads Unity SDK stores its Android/iOS App IDs in an internal
    /// ScriptableObject (GoogleMobileAds.Editor.GoogleMobileAdsSettings) normally edited through
    /// Assets > Google Mobile Ads > Settings. That type is internal to the SDK's own assembly,
    /// so build automation sets it via reflection here rather than forking the SDK to expose it.
    public static class AdMobSettingsConfigurator
    {
        private const string SettingsTypeName = "GoogleMobileAds.Editor.GoogleMobileAdsSettings, GoogleMobileAds.Editor";

        [MenuItem("Release The Arrow/Ads/Set AdMob Android App ID To Test ID")]
        public static void ConfigureTestAppId() => SetAndroidAppId(ReleaseTheArrow.Ads.AdMobConfig.TestAppId);

        public static void SetAndroidAppId(string appId)
        {
            var settingsType = Type.GetType(SettingsTypeName);
            if (settingsType == null)
            {
                Debug.LogError("[AdMobSettingsConfigurator] GoogleMobileAdsSettings type not found — is the Google Mobile Ads SDK imported?");
                return;
            }

            var loadInstance = settingsType.GetMethod("LoadInstance", BindingFlags.NonPublic | BindingFlags.Static);
            var instance = loadInstance.Invoke(null, null) as ScriptableObject;
            if (instance == null)
            {
                Debug.LogError("[AdMobSettingsConfigurator] Could not load/create GoogleMobileAdsSettings instance.");
                return;
            }

            var appIdProperty = settingsType.GetProperty("GoogleMobileAdsAndroidAppId", BindingFlags.Public | BindingFlags.Instance);
            appIdProperty.SetValue(instance, appId);

            EditorUtility.SetDirty(instance);
            AssetDatabase.SaveAssets();
            Debug.Log($"[AdMobSettingsConfigurator] Set AdMob Android App ID to: {appId}");
        }
    }
}
