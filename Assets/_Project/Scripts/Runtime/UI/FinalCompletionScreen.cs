using System;
using UnityEngine;

namespace ReleaseTheArrow.UI
{
    public class FinalCompletionScreen : MonoBehaviour
    {
        public event Action PlayAgainRequested;
        public event Action MainMenuRequested;

        public static FinalCompletionScreen Create(Transform parent)
        {
            var backdrop = UIFactory.CreateFullStretchPanel(parent, "FinalCompletionBackdrop", new Color(0f, 0f, 0f, 0.8f));
            var screen = backdrop.gameObject.AddComponent<FinalCompletionScreen>();
            screen.Build(backdrop);
            return screen;
        }

        private void Build(RectTransform backdrop)
        {
            var panel = UIFactory.CreatePanel(backdrop, "Panel", new Vector2(760, 780), Theme.PanelBackground);
            panel.anchorMin = panel.anchorMax = new Vector2(0.5f, 0.5f);
            UIFactory.AddVerticalLayout(panel.gameObject, spacing: 28, padding: new RectOffset(40, 40, 70, 70));

            PauseScreen.AddTitle(panel, "CONGRATULATIONS!", 58);

            var subGo = new GameObject("Subtitle", typeof(RectTransform));
            var subRect = (RectTransform)subGo.transform;
            subRect.SetParent(panel, false);
            subRect.sizeDelta = new Vector2(660, 120);
            UIFactory.SetPreferredSize(subGo, subRect.sizeDelta);
            UIFactory.CreateText(subRect, "YOU COMPLETED\nALL 1500 LEVELS!", 40, Theme.AccentPrimary, TextAnchor.MiddleCenter, FontStyle.Bold);

            UIFactory.CreateButton(panel, "PLAY AGAIN", new Vector2(600, 110), Theme.AccentPrimary, Color.black, () => PlayAgainRequested?.Invoke(), 40);
            UIFactory.CreateButton(panel, "MAIN MENU", new Vector2(600, 110), Theme.ButtonBackground, Theme.TextPrimary, () => MainMenuRequested?.Invoke(), 40);
        }
    }
}
