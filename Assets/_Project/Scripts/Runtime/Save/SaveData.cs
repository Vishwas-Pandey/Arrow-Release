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
                inProgress = inProgress.Clone()
            };
        }
    }
}
