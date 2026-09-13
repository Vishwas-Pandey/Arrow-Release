using System;
using System.Collections.Generic;

namespace ReleaseTheArrow.Save
{
    [Serializable]
    public class InProgressLevelState
    {
        public int levelId;
        public List<int> removedArrowIds = new List<int>();
        public int lives;
        public int continuesUsed;

        public InProgressLevelState Clone()
        {
            return new InProgressLevelState
            {
                levelId = levelId,
                removedArrowIds = new List<int>(removedArrowIds),
                lives = lives,
                continuesUsed = continuesUsed
            };
        }
    }

    [Serializable]
    public class SaveData
    {
        public const int CurrentVersion = 1;

        public int version = CurrentVersion;
        public int highestUnlockedLevel = 1;

        public bool musicOn = true;
        public bool soundOn = true;
        public bool vibrationOn = true;

        /// Null when there is no in-progress attempt (e.g. player just finished a level, or hasn't started one).
        /// JsonUtility can't serialize null references cleanly, so presence is tracked via hasInProgress.
        public bool hasInProgress;
        public InProgressLevelState inProgress = new InProgressLevelState();

        /// Best-ever star rating (1-3) per level, indexed by levelId - 1. Shorter than
        /// highestUnlockedLevel whenever a level hasn't been completed yet — GetStars/SetStars
        /// below treat any out-of-range index as "no stars yet" rather than growing this eagerly.
        public List<int> levelStars = new List<int>();

        public int GetStars(int levelId)
        {
            int index = levelId - 1;
            return index >= 0 && index < levelStars.Count ? levelStars[index] : 0;
        }

        /// Records `stars` for `levelId` only if it beats whatever's already stored.
        public void SetStarsIfBetter(int levelId, int stars)
        {
            int index = levelId - 1;
            if (index < 0) return;
            while (levelStars.Count <= index) levelStars.Add(0);
            if (stars > levelStars[index]) levelStars[index] = stars;
        }

        public SaveData Clone()
        {
            return new SaveData
            {
                version = version,
                highestUnlockedLevel = highestUnlockedLevel,
                musicOn = musicOn,
                soundOn = soundOn,
                vibrationOn = vibrationOn,
                hasInProgress = hasInProgress,
                inProgress = inProgress.Clone(),
                levelStars = new List<int>(levelStars)
            };
        }
    }
}
