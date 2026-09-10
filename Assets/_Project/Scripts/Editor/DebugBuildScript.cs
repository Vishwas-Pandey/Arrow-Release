using UnityEditor;
using UnityEngine;

namespace ReleaseTheArrow.EditorTools
{
    /// Fast local debug APK for installing on an emulator/device to eyeball the game.
    /// Not for Play Console — unsigned with the auto debug key, development build.
    public static class DebugBuildScript
    {
        private const string OutputPath = "Builds/Android/ReleaseTheArrowDebug.apk";

        [MenuItem("Release The Arrow/Build Android Debug APK")]
        public static void BuildAndroidDebugApk()
        {
            EditorUserBuildSettings.buildAppBundle = false;
            EditorUserBuildSettings.development = true;
            PlayerSettings.Android.useCustomKeystore = false;

            var options = new BuildPlayerOptions
            {
                scenes = new[] { "Assets/_Project/Scenes/Main.unity" },
                locationPathName = OutputPath,
                target = BuildTarget.Android,
                options = BuildOptions.Development
            };

            var report = BuildPipeline.BuildPlayer(options);
            var summary = report.summary;

            if (summary.result == UnityEditor.Build.Reporting.BuildResult.Succeeded)
            {
                Debug.Log($"[DebugBuildScript] Debug APK build succeeded: {OutputPath} ({summary.totalSize / 1024 / 1024} MB).");
            }
            else
            {
                Debug.LogError($"[DebugBuildScript] Debug APK build FAILED: {summary.result}. See errors above.");
            }
        }
    }
}
