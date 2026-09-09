using System;
using ReleaseTheArrow.Core;
using ReleaseTheArrow.Gameplay;
using UnityEngine;
using UnityEngine.UI;

namespace ReleaseTheArrow.UI
{
    /// Top bar during gameplay: pause button, level number, three life hearts.
    public class HudView : MonoBehaviour
    {
        public event Action PauseRequested;

        private Text _levelText;
        private Image[] _hearts;
        private GameManager _manager;

        public static HudView Create(Transform parent)
        {
            var go = new GameObject("HUD", typeof(RectTransform));
            var rect = (RectTransform)go.transform;
            rect.SetParent(parent, false);
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.sizeDelta = new Vector2(0f, 160f);

            go.AddComponent<SafeAreaFitter>();

            var hud = go.AddComponent<HudView>();
            hud.Build(rect);
            return hud;
        }

        private void Build(RectTransform rect)
        {
            var pauseButton = UIFactory.CreateIconButton(rect, IconSpriteFactory.Pause(), new Vector2(96, 96), new Vector2(40, 40), Theme.ButtonBackground, Theme.TextPrimary, () => PauseRequested?.Invoke());
            var pauseRect = (RectTransform)pauseButton.transform;
            pauseRect.anchorMin = pauseRect.anchorMax = new Vector2(0f, 0.5f);
            pauseRect.anchoredPosition = new Vector2(80f, 0f);

            var levelGo = new GameObject("LevelLabel", typeof(RectTransform));
            var levelRect = (RectTransform)levelGo.transform;
            levelRect.SetParent(rect, false);
            levelRect.anchorMin = new Vector2(0.5f, 0.5f);
            levelRect.anchorMax = new Vector2(0.5f, 0.5f);
            levelRect.sizeDelta = new Vector2(500, 100);
            _levelText = UIFactory.CreateText(levelRect, "LEVEL 1", 44, Theme.TextPrimary, TextAnchor.MiddleCenter, FontStyle.Bold);

            var heartsGo = new GameObject("Hearts", typeof(RectTransform));
            var heartsRect = (RectTransform)heartsGo.transform;
            heartsRect.SetParent(rect, false);
            heartsRect.anchorMin = heartsRect.anchorMax = new Vector2(1f, 0.5f);
            heartsRect.pivot = new Vector2(1f, 0.5f);
            heartsRect.anchoredPosition = new Vector2(-40f, 0f);
            heartsRect.sizeDelta = new Vector2(320, 100);
            UIFactory.AddHorizontalLayout(heartsGo, spacing: 12);

            _hearts = new Image[LevelSession.StartingLives];
            for (int i = 0; i < _hearts.Length; i++)
            {
                _hearts[i] = UIFactory.CreateIcon(heartsRect, IconSpriteFactory.Heart(), new Vector2(56, 56), Theme.HeartFull);
            }
        }

        public void Bind(GameManager manager)
        {
            if (_manager == manager) { Refresh(); return; } // already bound — avoid stacking duplicate subscriptions
            Unbind();
            _manager = manager;
            manager.LivesChanged += Refresh;
            Refresh();
        }

        public void Unbind()
        {
            if (_manager != null) _manager.LivesChanged -= Refresh;
        }

        private void Refresh()
        {
            if (_manager?.CurrentSession == null) return;
            _levelText.text = $"LEVEL {_manager.CurrentSession.Layout.levelId}";
            int lives = _manager.CurrentSession.Lives;
            for (int i = 0; i < _hearts.Length; i++)
            {
                _hearts[i].color = i < lives ? Theme.HeartFull : Theme.HeartEmpty;
            }
        }

        private void OnDestroy() => Unbind();
    }
}
