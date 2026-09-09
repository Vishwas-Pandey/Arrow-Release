using System;
using System.Collections.Generic;
using UnityEngine;

namespace ReleaseTheArrow.Utils
{
    [Serializable]
    public class AnalyticsEvent
    {
        public string name;
        public string paramsJson;
        public long unixTimeSeconds;
    }

    /// Local event queue for gameplay telemetry. No backend is wired up yet — events are kept
    /// in memory (and logged in the editor/dev builds) so the call sites already exist
    /// throughout gameplay code; pointing this at a real analytics backend later is pure
    /// plumbing, not a gameplay-code change.
    public static class AnalyticsManager
    {
        private static readonly List<AnalyticsEvent> Queue = new List<AnalyticsEvent>();
        public static IReadOnlyList<AnalyticsEvent> QueuedEvents => Queue;

        public static void Log(string eventName, Dictionary<string, string> parameters = null)
        {
            string json = parameters != null ? MiniJson(parameters) : "{}";
            var evt = new AnalyticsEvent
            {
                name = eventName,
                paramsJson = json,
                unixTimeSeconds = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
            };
            Queue.Add(evt);
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.Log($"[Analytics] {eventName} {json}");
#endif
        }

        public static void GameStarted(int levelId) => Log("game_started", new Dictionary<string, string> { { "level", levelId.ToString() } });
        public static void GameOver(int levelId, int continuesUsed) => Log("game_over", new Dictionary<string, string> { { "level", levelId.ToString() }, { "continues_used", continuesUsed.ToString() } });
        public static void LevelCompleted(int levelId) => Log("level_completed", new Dictionary<string, string> { { "level", levelId.ToString() } });
        public static void Retry(int levelId) => Log("retry", new Dictionary<string, string> { { "level", levelId.ToString() } });
        public static void AdEvent(string adType, string outcome) => Log("ad_event", new Dictionary<string, string> { { "type", adType }, { "outcome", outcome } });

        private static string MiniJson(Dictionary<string, string> dict)
        {
            var sb = new System.Text.StringBuilder("{");
            bool first = true;
            foreach (var kvp in dict)
            {
                if (!first) sb.Append(',');
                first = false;
                sb.Append('"').Append(kvp.Key).Append("\":\"").Append(kvp.Value).Append('"');
            }
            sb.Append('}');
            return sb.ToString();
        }
    }
}
