using System;
using ReleaseTheArrow.Audio;
using ReleaseTheArrow.Utils;
using UnityEngine;
using UnityEngine.UI;

namespace ReleaseTheArrow.UI
{
    public class SettingsScreen : MonoBehaviour
    {
        public event Action BackRequested;
        public event Action<bool, bool, bool> SettingsChanged; // music, sound, vibration

        private bool _music = true, _sound = true, _vibration = true;
        private Text _musicLabel, _soundLabel, _vibrationLabel;

        public static SettingsScreen Create(Transform parent)
        {
            var backdrop = UIFactory.CreateFullStretchPanel(parent, "SettingsBackdrop", new Color(0f, 0f, 0f, 0.6f));
            var screen = backdrop.gameObject.AddComponent<SettingsScreen>();
            screen.Build(backdrop);
            return screen;
        }

        private void Build(RectTransform backdrop)
        {
            var panel = UIFactory.CreatePanel(backdrop, "Panel", new Vector2(680, 820), Theme.PanelBackground);
            panel.anchorMin = panel.anchorMax = new Vector2(0.5f, 0.5f);
            UIFactory.AddVerticalLayout(panel.gameObject, spacing: 24, padding: new RectOffset(40, 40, 60, 60));

            PauseScreen.AddTitle(panel, "SETTINGS");

            _musicLabel = AddToggleRow(panel, "MUSIC", () => { _music = !_music; Apply(); });
            _soundLabel = AddToggleRow(panel, "SOUND", () => { _sound = !_sound; Apply(); });
            _vibrationLabel = AddToggleRow(panel, "VIBRATION", () => { _vibration = !_vibration; Apply(); });

            UIFactory.CreateButton(panel, "BACK", new Vector2(560, 100), Theme.ButtonBackground, Theme.TextPrimary, () => BackRequested?.Invoke(), 38);
        }

        private Text AddToggleRow(Transform parent, string label, Action onToggle)
        {
            var button = UIFactory.CreateButton(parent, $"{label}: ON", new Vector2(560, 110), Theme.ButtonBackground, Theme.TextPrimary, onToggle, 36);
            return button.GetComponentInChildren<Text>();
        }

        public void SetInitialValues(bool music, bool sound, bool vibration)
        {
            _music = music; _sound = sound; _vibration = vibration;
            RefreshLabels();
        }

        private void Apply()
        {
            RefreshLabels();
            AudioManager.Instance?.ApplySettings(_sound, _music);
            HapticManager.VibrationOn = _vibration;
            SettingsChanged?.Invoke(_music, _sound, _vibration);
        }

        private void RefreshLabels()
        {
            _musicLabel.text = $"MUSIC: {(_music ? "ON" : "OFF")}";
            _soundLabel.text = $"SOUND: {(_sound ? "ON" : "OFF")}";
            _vibrationLabel.text = $"VIBRATION: {(_vibration ? "ON" : "OFF")}";
        }
    }
}
