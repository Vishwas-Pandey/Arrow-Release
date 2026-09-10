using System;
using UnityEngine;

namespace ReleaseTheArrow.UI
{
    public class CreditsScreen : MonoBehaviour
    {
        public event Action BackRequested;

        public static CreditsScreen Create(Transform parent)
        {
            var backdrop = UIFactory.CreateFullStretchPanel(parent, "CreditsBackdrop", new Color(0f, 0f, 0f, 0.6f));
            var screen = backdrop.gameObject.AddComponent<CreditsScreen>();
            screen.Build(backdrop);
            return screen;
        }

        private void Build(RectTransform backdrop)
        {
            var panel = UIFactory.CreatePanel(backdrop, "Panel", new Vector2(700, 700), Theme.PanelBackground);
            panel.anchorMin = panel.anchorMax = new Vector2(0.5f, 0.5f);
            UIFactory.AddVerticalLayout(panel.gameObject, spacing: 26, padding: new RectOffset(40, 40, 60, 60));

            PauseScreen.AddTitle(panel, "CREDITS");

            var bodyGo = new GameObject("Body", typeof(RectTransform));
            var bodyRect = (RectTransform)bodyGo.transform;
            bodyRect.SetParent(panel, false);
            bodyRect.sizeDelta = new Vector2(600, 320);
            UIFactory.SetPreferredSize(bodyGo, bodyRect.sizeDelta);
            UIFactory.CreateText(bodyRect, "RELEASE THE ARROW\n\nDeveloped and published by\nVirevia\n\nThank you for playing!",
                34, Theme.TextSecondary, TextAnchor.MiddleCenter);

            UIFactory.CreateButton(panel, "BACK", new Vector2(560, 100), Theme.ButtonBackground, Theme.TextPrimary, () => BackRequested?.Invoke(), 38);
        }
    }
}
