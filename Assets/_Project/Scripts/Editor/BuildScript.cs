using System;
using UnityEditor;
using UnityEngine;

namespace ReleaseTheArrow.EditorTools
{
    /// Release build entry point. Per the project's own security rules: the keystore and its
    /// passwords are never hardcoded or committed — they're read from environment variables at
    /// build time only, and only "RTA_KEYSTORE_PATH" ever names an on-disk file (which itself
    /// must live outside the repo, per .gitignore).
    ///
    /// Usage (CI or local):
    ///   RTA_KEYSTORE_PATH=/secure/path/upload.keystore \
    ///   RTA_KEYSTORE_PASSWORD=*** RTA_KEY_ALIAS_NAME=upload RTA_KEY_ALIAS_PASSWORD=*** \
    ///   Unity -batchmode -nographics -quit -projectPath . \
    ///     -executeMethod ReleaseTheArrow.EditorTools.BuildScript.BuildAndroidRelease
    ///
    /// Without those environment variables set, this still produces a valid, buildable .aab —
    /// just unsigned with a release keystore (fine for local sanity builds, never for Play
    /// Console upload).
    public static class BuildScript
    {
        private const string OutputPath = "Builds/Android/ReleaseTheArrow.aab";

        [MenuItem("Release The Arrow/Build Android Release (.aab)")]
        public static void BuildAndroidRelease()
        {
            EditorUserBuildSettings.buildAppBundle = true;
            EditorUserBuildSettings.development = false;

            ApplyKeystoreFromEnvironment();

            var scenes = new[] { "Assets/_Project/Scenes/Main.unity" };
            var options = new BuildPlayerOptions
            {
                scenes = scenes,
                locationPathName = OutputPath,
                target = BuildTarget.Android,
                options = BuildOptions.None
            };

            var report = BuildPipeline.BuildPlayer(options);
            var summary = report.summary;

            if (summary.result == UnityEditor.Build.Reporting.BuildResult.Succeeded)
            {
                Debug.Log($"[BuildScript] Build succeeded: {OutputPath} ({summary.totalSize / 1024 / 1024} MB, versionCode {PlayerSettings.Android.bundleVersionCode}).");
            }
            else
            {
                Debug.LogError($"[BuildScript] Build FAILED: {summary.result}. See errors above.");
            }
        }

        private static void ApplyKeystoreFromEnvironment()
        {
            string keystorePath = Environment.GetEnvironmentVariable("RTA_KEYSTORE_PATH");
            string keystorePassword = Environment.GetEnvironmentVariable("RTA_KEYSTORE_PASSWORD");
            string keyAliasName = Environment.GetEnvironmentVariable("RTA_KEY_ALIAS_NAME");
            string keyAliasPassword = Environment.GetEnvironmentVariable("RTA_KEY_ALIAS_PASSWORD");

            bool haveAll = !string.IsNullOrEmpty(keystorePath) && !string.IsNullOrEmpty(keystorePassword)
                && !string.IsNullOrEmpty(keyAliasName) && !string.IsNullOrEmpty(keyAliasPassword);

            if (!haveAll)
            {
                Debug.LogWarning("[BuildScript] RTA_KEYSTORE_* environment variables not fully set — " +
                                  "building without a release signing key. Do not upload this build to Play Console.");
                PlayerSettings.Android.useCustomKeystore = false;
                return;
            }

            PlayerSettings.Android.useCustomKeystore = true;
            PlayerSettings.Android.keystoreName = keystorePath;
            PlayerSettings.Android.keystorePass = keystorePassword;
            PlayerSettings.Android.keyaliasName = keyAliasName;
            PlayerSettings.Android.keyaliasPass = keyAliasPassword;
        }
    }
}
