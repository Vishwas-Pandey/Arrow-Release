using System;
using ReleaseTheArrow.Core;
using UnityEngine;
using UnityEngine.UI;

namespace ReleaseTheArrow.UI
{
    public class MainMenuScreen : MonoBehaviour
    {
        public event Action PlayRequested;
        public event Action LevelSelectRequested;
        public event Action SettingsRequested;
        public event Action CreditsRequested;

        private Text _progressLabel;

        public static MainMenuScreen Create(Transform parent)
        {
            var rect = UIFactory.CreateFullStretchPanel(parent, "MainMenu", new Color(0, 0, 0, 0));
            var screen = rect.gameObject.AddComponent<MainMenuScreen>();
            screen.Build(rect);
            return screen;
        }

        private void Build(RectTransform rect)
        {
            var titleArea = new GameObject("TitleArea", typeof(RectTransform));
            var titleRect = (RectTransform)titleArea.transform;
            titleRect.SetParent(rect, false);
            titleRect.anchorMin = new Vector2(0.5f, 1f);
            titleRect.anchorMax = new Vector2(0.5f, 1f);
            titleRect.anchoredPosition = new Vector2(0f, -260f);
            titleRect.sizeDelta = new Vector2(900, 300);

            UIFactory.CreateText(titleRect, "RELEASE\nTHE ARROW", 88, Theme.AccentPrimary, TextAnchor.MiddleCenter, FontStyle.Bold);

            // Visible build marker — so a bug report can name the exact build it came from,
            // rather than us having to guess whether a fix actually reached the device tested.
            var versionGo = new GameObject("BuildVersion", typeof(RectTransform));
            var versionRect = (RectTransform)versionGo.transform;
            versionRect.SetParent(rect, false);
            versionRect.anchorMin = new Vector2(0.5f, 0f);
            versionRect.anchorMax = new Vector2(0.5f, 0f);
            versionRect.pivot = new Vector2(0.5f, 0f);
            versionRect.anchoredPosition = new Vector2(0f, 24f);
            versionRect.sizeDelta = new Vector2(600, 50);
            UIFactory.CreateText(versionRect, $"build {BuildInfo.Version}", 26, Theme.TextSecondary, TextAnchor.MiddleCenter);

            var progressGo = new GameObject("Progress", typeof(RectTransform));
            var progressRect = (RectTransform)progressGo.transform;
            progressRect.SetParent(rect, false);
            progressRect.anchorMin = new Vector2(0.5f, 1f);
            progressRect.anchorMax = new Vector2(0.5f, 1f);
            progressRect.anchoredPosition = new Vector2(0f, -470f);
            progressRect.sizeDelta = new Vector2(700, 80);
            _progressLabel = UIFactory.CreateText(progressRect, "LEVEL 1", 40, Theme.TextSecondary);

            var buttonColumn = new GameObject("Buttons", typeof(RectTransform));
            var columnRect = (RectTransform)buttonColumn.transform;
            columnRect.SetParent(rect, false);
            columnRect.anchorMin = new Vector2(0.5f, 0.5f);
            columnRect.anchorMax = new Vector2(0.5f, 0.5f);
            columnRect.anchoredPosition = new Vector2(0f, -60f);
            columnRect.sizeDelta = new Vector2(600, 600);
            UIFactory.AddVerticalLayout(buttonColumn, spacing: 28);

            UIFactory.CreateButton(columnRect, "PLAY", new Vector2(520, 120), Theme.AccentPrimary, Color.black, () => PlayRequested?.Invoke(), 48);
            UIFactory.CreateButton(columnRect, "LEVELS", new Vector2(520, 100), Theme.ButtonBackground, Theme.TextPrimary, () => LevelSelectRequested?.Invoke(), 40);
            UIFactory.CreateButton(columnRect, "SETTINGS", new Vector2(520, 100), Theme.ButtonBackground, Theme.TextPrimary, () => SettingsRequested?.Invoke(), 40);
            UIFactory.CreateButton(columnRect, "CREDITS", new Vector2(520, 100), Theme.ButtonBackground, Theme.TextPrimary, () => CreditsRequested?.Invoke(), 40);
        }

        public void SetProgress(int highestUnlockedLevel)
        {
            _progressLabel.text = $"LEVEL {highestUnlockedLevel}";
        }
    }
}
