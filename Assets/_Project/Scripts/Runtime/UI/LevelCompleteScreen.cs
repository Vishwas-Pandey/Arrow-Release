using System;
using UnityEngine;

namespace ReleaseTheArrow.UI
{
    public class LevelCompleteScreen : MonoBehaviour
    {
        public event Action NextLevelRequested;

        public static LevelCompleteScreen Create(Transform parent)
        {
            var backdrop = UIFactory.CreateFullStretchPanel(parent, "LevelCompleteBackdrop", new Color(0f, 0f, 0f, 0.7f));
            var screen = backdrop.gameObject.AddComponent<LevelCompleteScreen>();
            screen.Build(backdrop);
            return screen;
        }

        private void Build(RectTransform backdrop)
        {
            var panel = UIFactory.CreatePanel(backdrop, "Panel", new Vector2(680, 620), Theme.PanelBackground);
            panel.anchorMin = panel.anchorMax = new Vector2(0.5f, 0.5f);
            UIFactory.AddVerticalLayout(panel.gameObject, spacing: 28, padding: new RectOffset(40, 60, 70, 70));

            PauseScreen.AddTitle(panel, "LEVEL COMPLETE!", 60);

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
