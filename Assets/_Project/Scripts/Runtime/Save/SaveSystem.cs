using System;
using System.IO;
using UnityEngine;

namespace ReleaseTheArrow.Save
{
    /// Local progression save. Uses JsonUtility (never BinaryFormatter — obsolete, real security
    /// issues). Writes to a temp file and atomically replaces the real save file so a crash or
    /// kill mid-write can never leave a corrupt/half-written save on disk.
    public static class SaveSystem
    {
        private static readonly string SavePath = Path.Combine(Application.persistentDataPath, "save.json");
        private static readonly string TempPath = Path.Combine(Application.persistentDataPath, "save.json.tmp");

        private static SaveData _cache;

        public static SaveData Load()
        {
            if (_cache != null) return _cache;

            try
            {
                if (File.Exists(SavePath))
                {
                    string json = File.ReadAllText(SavePath);
                    var data = JsonUtility.FromJson<SaveData>(json);
                    if (data != null)
                    {
                        if (data.version != SaveData.CurrentVersion)
                        {
                            // Puzzle generation/format may have changed since this was written —
                            // keep level progress, but don't risk replaying an in-progress attempt
                            // against logic that could now produce a different layout for it.
                            data.hasInProgress = false;
                            data.inProgress = new InProgressLevelState();
                            data.version = SaveData.CurrentVersion;
                        }
                        _cache = data;
                        return _cache;
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[SaveSystem] Save file unreadable, starting fresh. {e.Message}");
            }

            _cache = new SaveData();
            return _cache;
        }

        public static void Save(SaveData data)
        {
            _cache = data;
            try
            {
                string json = JsonUtility.ToJson(data, prettyPrint: false);
                File.WriteAllText(TempPath, json);

                if (!File.Exists(SavePath))
                {
                    File.Move(TempPath, SavePath);
                    return;
                }

                try
                {
                    // File.Replace performs a single atomic rename at the OS level — there is no
                    // window where neither file exists, unlike delete-then-move.
                    File.Replace(TempPath, SavePath, null);
                }
                catch (Exception)
                {
                    // Rare fallback for platforms/cases where Replace isn't available. Not
                    // atomic, but only reached if the atomic path itself failed.
                    File.Delete(SavePath);
                    File.Move(TempPath, SavePath);
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveSystem] Failed to persist save data: {e.Message}");
            }
        }

        public static void ClearInProgress(SaveData data)
        {
            data.hasInProgress = false;
            data.inProgress = new InProgressLevelState();
            Save(data);
        }

        /// Test/editor hook — production code should never need to bypass the cache.
        internal static void ResetCacheForTests() => _cache = null;
    }
}
