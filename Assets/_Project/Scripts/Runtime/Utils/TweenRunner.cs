using UnityEngine;

namespace ReleaseTheArrow.Utils
{
    /// Hidden singleton that hosts tween coroutines so callers never need to worry about their
    /// own MonoBehaviour being disabled mid-tween. Auto-created on first use.
    internal class TweenRunner : MonoBehaviour
    {
        private static TweenRunner _instance;

        public static TweenRunner Instance
        {
            get
            {
                if (_instance != null) return _instance;
                var go = new GameObject("~TweenRunner");
                Object.DontDestroyOnLoad(go);
                _instance = go.AddComponent<TweenRunner>();
                return _instance;
            }
        }
    }
}
