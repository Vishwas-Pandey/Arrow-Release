using ReleaseTheArrow.Core;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace ReleaseTheArrow.EditorTools
{
    /// One-time (and re-runnable) scene setup: the entire game is one scene containing a single
    /// GameFlowController, which builds the rest of the UI/gameplay hierarchy procedurally at
    /// runtime. Invoked via -executeMethod from the command line so the resulting scene file is
    /// genuine Unity-serialized YAML rather than hand-authored.
    public static class SceneBootstrapper
    {
        private const string ScenePath = "Assets/_Project/Scenes/Main.unity";

        [MenuItem("Release The Arrow/Rebuild Main Scene")]
        public static void CreateMainScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var root = new GameObject("GameFlowController");
            root.AddComponent<GameFlowController>();

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, ScenePath);

            var scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
            EditorBuildSettings.scenes = scenes;

            Debug.Log($"[SceneBootstrapper] Main scene written to {ScenePath} and registered in Build Settings.");
        }
    }
}
