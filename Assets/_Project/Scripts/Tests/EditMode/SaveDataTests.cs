using NUnit.Framework;
using ReleaseTheArrow.Save;

namespace ReleaseTheArrow.Tests
{
    public class SaveDataTests
    {
        [Test]
        public void GetStars_DefaultsToZero_ForAnUnplayedLevel()
        {
            var data = new SaveData();
            Assert.AreEqual(0, data.GetStars(1));
            Assert.AreEqual(0, data.GetStars(500));
        }

        [Test]
        public void SetStarsIfBetter_StoresAndRetrievesExactly()
        {
            var data = new SaveData();
            data.SetStarsIfBetter(7, 2);
            Assert.AreEqual(2, data.GetStars(7));
            Assert.AreEqual(0, data.GetStars(6), "Unrelated levels must be unaffected.");
        }

        [Test]
        public void SetStarsIfBetter_NeverDowngradesAnExistingBestScore()
        {
            var data = new SaveData();
            data.SetStarsIfBetter(3, 3);
            data.SetStarsIfBetter(3, 1);
            Assert.AreEqual(3, data.GetStars(3), "A worse replay must never overwrite a better recorded score.");
        }

        [Test]
        public void SetStarsIfBetter_UpgradesWhenBetterScoreArrives()
        {
            var data = new SaveData();
            data.SetStarsIfBetter(3, 1);
            data.SetStarsIfBetter(3, 3);
            Assert.AreEqual(3, data.GetStars(3));
        }

        [Test]
        public void Clone_CopiesStarsIndependently()
        {
            var data = new SaveData();
            data.SetStarsIfBetter(10, 2);
            var clone = data.Clone();
            clone.SetStarsIfBetter(10, 3);

            Assert.AreEqual(2, data.GetStars(10), "Mutating the clone must not affect the original.");
            Assert.AreEqual(3, clone.GetStars(10));
        }
    }
}
