using System;
using ReleaseTheArrow.Ads;
using ReleaseTheArrow.Audio;
using UnityEngine;
using UnityEngine.UI;

namespace ReleaseTheArrow.UI
{
    /// Covers both the progressive-continue Game Over state and the final "no more continues"
    /// state. The watch-ad button is always an explicit, opt-in action — never auto-triggered.
    public class GameOverScreen : MonoBehaviour
    {
        public event Action RestartRequested;
        public event Action ContinueGranted;

        private Text _title;
        private Text _watchAdLabel;
        private Text _statusText;
        private Button _watchAdButton;
        private GameObject _watchAdRoot;
        private int _adsRequired;

        public static GameOverScreen Create(Transform parent)
        {
            var backdrop = UIFactory.CreateFullStretchPanel(parent, "GameOverBackdrop", new Color(0f, 0f, 0f, 0.75f));
            var screen = backdrop.gameObject.AddComponent<GameOverScreen>();
            screen.Build(backdrop);
            return screen;
        }

        private void Build(RectTransform backdrop)
        {
            var panel = UIFactory.CreatePanel(backdrop, "Panel", new Vector2(700, 780), Theme.PanelBackground);
            panel.anchorMin = panel.anchorMax = new Vector2(0.5f, 0.5f);
            UIFactory.AddVerticalLayout(panel.gameObject, spacing: 24, padding: new RectOffset(40, 40, 60, 60));

            var titleGo = new GameObject("Title", typeof(RectTransform));
            var titleRect = (RectTransform)titleGo.transform;
            titleRect.SetParent(panel, false);
            titleRect.sizeDelta = new Vector2(600, 110);
            _title = UIFactory.CreateText(titleRect, "GAME OVER", 56, Theme.Danger, TextAnchor.MiddleCenter, FontStyle.Bold);

            _watchAdRoot = new GameObject("WatchAdGroup", typeof(RectTransform));
            var watchAdRect = (RectTransform)_watchAdRoot.transform;
            watchAdRect.SetParent(panel, false);
            watchAdRect.sizeDelta = new Vector2(620, 220);
            UIFactory.AddVerticalLayout(_watchAdRoot, spacing: 14);

            _watchAdButton = UIFactory.CreateButton(watchAdRect, "WATCH AD\n+1 LIFE", new Vector2(560, 130), Theme.AccentSecondary, Color.black, OnWatchAdClicked, 34);
            _watchAdLabel = _watchAdButton.GetComponentInChildren<Text>();

            var statusGo = new GameObject("Status", typeof(RectTransform));
            var statusRect = (RectTransform)statusGo.transform;
            statusRect.SetParent(watchAdRect, false);
            statusRect.sizeDelta = new Vector2(560, 60);
            _statusText = UIFactory.CreateText(statusRect, "", 28, Theme.TextSecondary);

            UIFactory.CreateButton(panel, "RESTART LEVEL", new Vector2(560, 110), Theme.ButtonBackground, Theme.TextPrimary, () => RestartRequested?.Invoke(), 38);
        }

        public void Configure(int adsRequired, bool isFinal)
        {
            _adsRequired = adsRequired;
            _title.text = isFinal ? "LEVEL FAILED" : "GAME OVER";
            _watchAdRoot.SetActive(!isFinal);
            _statusText.text = "";
            SetWatchAdInteractable(true);

            if (!isFinal)
            {
                _watchAdLabel.text = adsRequired <= 1 ? "WATCH AD\n+1 LIFE" : $"WATCH {adsRequired} ADS\n+1 LIFE";
            }
        }

        private void OnWatchAdClicked()
        {
            SetWatchAdInteractable(false);
            _statusText.text = "Loading ad...";

            if (AdManager.Instance == null)
            {
                _statusText.text = "Ads unavailable right now — try again, or restart the level.";
                SetWatchAdInteractable(true);
                return;
            }

            AdManager.Instance.ShowRewardedSequence(_adsRequired,
                onAllCompleted: () =>
                {
                    AudioManager.Instance?.Play(Sfx.RewardedContinue);
                    ContinueGranted?.Invoke();
                },
                onFailedOrCancelled: () =>
                {
                    _statusText.text = "Ad unavailable — try again, or restart the level.";
                    SetWatchAdInteractable(true);
                });
        }

        private void SetWatchAdInteractable(bool interactable) => _watchAdButton.interactable = interactable;
    }
}
