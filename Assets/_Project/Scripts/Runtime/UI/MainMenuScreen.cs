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
            BuildBackground(rect);

            // Top-left/top-right icon buttons, no visible plate — floating directly on the
            // artwork's sky, matching the reference mockup rather than the flat button style
            // used everywhere else in the app.
            var settingsButton = UIFactory.CreateIconButton(rect, IconSpriteFactory.Gear(),
                new Vector2(110, 110), new Vector2(56, 56), new Color(0f, 0f, 0f, 0f), Theme.MenuIconGlyph,
                () => SettingsRequested?.Invoke());
            var settingsRect = (RectTransform)settingsButton.transform;
            settingsRect.anchorMin = settingsRect.anchorMax = new Vector2(0f, 1f);
            settingsRect.anchoredPosition = new Vector2(85f, -85f);

            var levelsIconButton = UIFactory.CreateIconButton(rect, IconSpriteFactory.BarChart(),
                new Vector2(110, 110), new Vector2(56, 56), new Color(0f, 0f, 0f, 0f), Theme.MenuIconGlyph,
                () => LevelSelectRequested?.Invoke());
            var levelsIconRect = (RectTransform)levelsIconButton.transform;
            levelsIconRect.anchorMin = levelsIconRect.anchorMax = new Vector2(1f, 1f);
            levelsIconRect.anchoredPosition = new Vector2(-85f, -85f);

            // The artwork already carries the "RELEASE THE ARROW" title and subtitle, so progress
            // gets its own small opaque pill (rather than bare text) — that way it stays legible
            // and never turns into overlapping text-on-text no matter what's behind it.
            // Anchored as a FRACTION of screen height (not a fixed pixel offset from the top) so
            // this lines up with the background art on every device aspect ratio — a fixed
            // offset assumes a specific canvas height, which CanvasScaler's MatchWidthOrHeight
            // blend does not actually guarantee (this cost real alignment bugs during testing).
            var progressGo = new GameObject("Progress", typeof(RectTransform), typeof(Image));
            var progressRect = (RectTransform)progressGo.transform;
            progressRect.SetParent(rect, false);
            progressRect.anchorMin = progressRect.anchorMax = new Vector2(0.5f, 0.70f);
            progressRect.anchoredPosition = Vector2.zero;
            progressRect.sizeDelta = new Vector2(300, 66);
            var progressImage = progressGo.GetComponent<Image>();
            progressImage.sprite = IconSpriteFactory.RoundedRect();
            progressImage.type = Image.Type.Sliced;
            progressImage.color = Theme.MenuPillSecondary;
            _progressLabel = UIFactory.CreateText(progressRect, "LEVEL 1", 32, Theme.MenuCreamText);

            // The artwork already paints its own "PLAY" pill — rather than drawing a second one
            // on top (which looked like a duplicate button), this is an invisible hit-zone sized
            // and positioned to sit exactly over that painted button.
            var playButton = UIFactory.CreateButton(rect, string.Empty, new Vector2(560, 150),
                new Color(0f, 0f, 0f, 0f), new Color(0f, 0f, 0f, 0f), () => PlayRequested?.Invoke(), 48);
            var playRect = (RectTransform)playButton.transform;
            playRect.anchorMin = playRect.anchorMax = new Vector2(0.5f, 0.346f);
            playRect.anchoredPosition = Vector2.zero;

            // Credits doesn't need prime real estate — tucked in the bottom-right corner, clear
            // of the artwork's own Leaderboard/Achievements/How To Play row along the bottom edge.
            var creditsButton = UIFactory.CreateButton(rect, "CREDITS", new Vector2(220, 60),
                Theme.MenuPillSecondary, Theme.MenuCreamText, () => CreditsRequested?.Invoke(), 26);
            var creditsRect = (RectTransform)creditsButton.transform;
            creditsRect.anchorMin = creditsRect.anchorMax = new Vector2(1f, 0f);
            creditsRect.pivot = new Vector2(1f, 0f);
            creditsRect.anchoredPosition = new Vector2(-30f, 150f);
            StyleAsPill(creditsButton);

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
            UIFactory.CreateText(versionRect, $"build {BuildInfo.Version}", 24, new Color(1f, 1f, 1f, 0.55f), TextAnchor.MiddleCenter);
        }

        private static void BuildBackground(RectTransform parent)
        {
            var texture = Resources.Load<Texture2D>("MainMenuBackground");
            if (texture == null) return; // falls back to the plain background panel behind this

            var go = new GameObject("BackgroundArt", typeof(RectTransform), typeof(Image));
            var bgRect = (RectTransform)go.transform;
            bgRect.SetParent(parent, false);
            bgRect.SetAsFirstSibling();
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.offsetMin = Vector2.zero;
            bgRect.offsetMax = Vector2.zero;

            var image = go.GetComponent<Image>();
            image.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
            image.type = Image.Type.Simple;
            image.preserveAspect = false; // the art's own aspect ratio already matches the canvas closely
        }

        private static void StyleAsPill(Button button)
        {
            var image = button.GetComponent<Image>();
            image.sprite = IconSpriteFactory.RoundedRect();
            image.type = Image.Type.Sliced;
        }

        public void SetProgress(int highestUnlockedLevel)
        {
            _progressLabel.text = $"LEVEL {highestUnlockedLevel}";
        }
    }
}
