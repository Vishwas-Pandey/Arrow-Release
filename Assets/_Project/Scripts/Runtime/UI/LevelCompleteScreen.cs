using System;
using UnityEngine;
using UnityEngine.UI;

namespace ReleaseTheArrow.UI
{
    public class LevelCompleteScreen : MonoBehaviour
    {
        public event Action NextLevelRequested;

        private RectTransform _starRow;
        private readonly System.Collections.Generic.List<Image> _starImages = new System.Collections.Generic.List<Image>();

        public static LevelCompleteScreen Create(Transform parent)
        {
            var backdrop = UIFactory.CreateFullStretchPanel(parent, "LevelCompleteBackdrop", new Color(0f, 0f, 0f, 0.7f));
            var screen = backdrop.gameObject.AddComponent<LevelCompleteScreen>();
            screen.Build(backdrop);
            return screen;
        }

        /// Fills in (or empties) the three star icons for how clean this clear was.
        public void Configure(int starsEarned)
        {
            for (int i = 0; i < _starImages.Count; i++)
            {
                _starImages[i].color = i < starsEarned ? Theme.StarFull : Theme.StarEmpty;
            }
        }

        private void Build(RectTransform backdrop)
        {
            var panel = UIFactory.CreatePanel(backdrop, "Panel", new Vector2(680, 620), Theme.PanelBackground);
            panel.anchorMin = panel.anchorMax = new Vector2(0.5f, 0.5f);
            UIFactory.AddVerticalLayout(panel.gameObject, spacing: 28, padding: new RectOffset(40, 60, 70, 70));

            PauseScreen.AddTitle(panel, "LEVEL COMPLETE!", 60);

            var starRowGo = new GameObject("StarRow", typeof(RectTransform));
            _starRow = (RectTransform)starRowGo.transform;
            _starRow.SetParent(panel, false);
            _starRow.sizeDelta = new Vector2(560, 100);
            UIFactory.SetPreferredSize(starRowGo, _starRow.sizeDelta);
            UIFactory.AddHorizontalLayout(starRowGo, spacing: 20);
            for (int i = 0; i < 3; i++)
            {
                var star = UIFactory.CreateIcon(_starRow, IconSpriteFactory.Star(), new Vector2(80, 80), Theme.StarEmpty);
                _starImages.Add(star);
            }

            var subGo = new GameObject("Subtitle", typeof(RectTransform));
            var subRect = (RectTransform)subGo.transform;
            subRect.SetParent(panel, false);
            subRect.sizeDelta = new Vector2(560, 90);
            UIFactory.SetPreferredSize(subGo, subRect.sizeDelta);
            UIFactory.CreateText(subRect, "Great job!", 40, Theme.Success);

            UIFactory.CreateButton(panel, "NEXT LEVEL", new Vector2(560, 120), Theme.AccentPrimary, Color.black, () => NextLevelRequested?.Invoke(), 44);
        }
    }
}
