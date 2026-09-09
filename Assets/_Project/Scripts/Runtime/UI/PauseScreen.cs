using System;
using UnityEngine;

namespace ReleaseTheArrow.UI
{
    public class PauseScreen : MonoBehaviour
    {
        public event Action ResumeRequested;
        public event Action RestartRequested;
        public event Action SettingsRequested;
        public event Action MainMenuRequested;

        public static PauseScreen Create(Transform parent)
        {
            var backdrop = UIFactory.CreateFullStretchPanel(parent, "PauseBackdrop", new Color(0f, 0f, 0f, 0.6f));
            var screen = backdrop.gameObject.AddComponent<PauseScreen>();
            screen.Build(backdrop);
            return screen;
        }

        private void Build(RectTransform backdrop)
        {
            var panel = UIFactory.CreatePanel(backdrop, "Panel", new Vector2(680, 760), Theme.PanelBackground);
            panel.anchorMin = panel.anchorMax = new Vector2(0.5f, 0.5f);
            var columnGo = panel.gameObject;
            UIFactory.AddVerticalLayout(columnGo, spacing: 26, padding: new RectOffset(40, 40, 60, 60));

            AddTitle(panel, "PAUSED");
            AddButton(panel, "RESUME", Theme.AccentPrimary, Color.black, () => ResumeRequested?.Invoke());
            AddButton(panel, "RESTART", Theme.ButtonBackground, Theme.TextPrimary, () => RestartRequested?.Invoke());
            AddButton(panel, "SETTINGS", Theme.ButtonBackground, Theme.TextPrimary, () => SettingsRequested?.Invoke());
            AddButton(panel, "MAIN MENU", Theme.ButtonBackground, Theme.TextPrimary, () => MainMenuRequested?.Invoke());
        }

        private static void AddButton(Transform parent, string label, Color bg, Color fg, Action onClick)
        {
            UIFactory.CreateButton(parent, label, new Vector2(560, 110), bg, fg, onClick, 40);
        }

        internal static void AddTitle(Transform parent, string label, int fontSize = 56)
        {
            var go = new GameObject("Title", typeof(RectTransform));
            var rect = (RectTransform)go.transform;
            rect.SetParent(parent, false);
            rect.sizeDelta = new Vector2(560, 110);
            UIFactory.CreateText(rect, label, fontSize, Theme.TextPrimary, TextAnchor.MiddleCenter, FontStyle.Bold);
        }
    }
}
