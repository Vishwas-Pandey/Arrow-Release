using System;
using System.Collections.Generic;
using ReleaseTheArrow.Generation;
using UnityEngine;
using UnityEngine.UI;

namespace ReleaseTheArrow.UI
{
    /// Paginated level grid for all 1500 levels (50 per page) rather than 1500 buttons at once.
    /// Progression here is strictly sequential, so only three states exist per level: completed
    /// (< highest unlocked), current (== highest unlocked), or locked (> highest unlocked).
    public class LevelSelectScreen : MonoBehaviour
    {
        private const int PageSize = 50;
        private const int Columns = 5;

        public event Action BackRequested;
        public event Action<int> LevelChosen;

        private RectTransform _gridContainer;
        private Text _pageLabel;
        private Button _prevButton, _nextButton;
        private int _currentPage;
        private int _highestUnlocked = 1;
        private readonly List<GameObject> _spawned = new List<GameObject>();

        public static LevelSelectScreen Create(Transform parent)
        {
            var rect = UIFactory.CreateFullStretchPanel(parent, "LevelSelect", Theme.BackgroundBottom);
            var screen = rect.gameObject.AddComponent<LevelSelectScreen>();
            screen.Build(rect);
            return screen;
        }

        private void Build(RectTransform rect)
        {
            rect.gameObject.AddComponent<SafeAreaFitter>();

            var header = new GameObject("Header", typeof(RectTransform));
            var headerRect = (RectTransform)header.transform;
            headerRect.SetParent(rect, false);
            headerRect.anchorMin = new Vector2(0f, 1f);
            headerRect.anchorMax = new Vector2(1f, 1f);
            headerRect.pivot = new Vector2(0.5f, 1f);
            headerRect.sizeDelta = new Vector2(0f, 180f);
            headerRect.anchoredPosition = new Vector2(0f, -20f);

            var backButton = UIFactory.CreateButton(headerRect, "BACK", new Vector2(180, 100), Theme.ButtonBackground, Theme.TextPrimary, () => BackRequested?.Invoke(), 32);
            var backRect = (RectTransform)backButton.transform;
            backRect.anchorMin = backRect.anchorMax = new Vector2(0f, 0.5f);
            backRect.anchoredPosition = new Vector2(120f, 0f);

            var titleGo = new GameObject("Title", typeof(RectTransform));
            var titleRect = (RectTransform)titleGo.transform;
            titleRect.SetParent(headerRect, false);
            titleRect.anchorMin = titleRect.anchorMax = new Vector2(0.5f, 0.5f);
            titleRect.sizeDelta = new Vector2(600, 100);
            UIFactory.CreateText(titleRect, "SELECT LEVEL", 46, Theme.TextPrimary, TextAnchor.MiddleCenter, FontStyle.Bold);

            var gridGo = new GameObject("Grid", typeof(RectTransform), typeof(GridLayoutGroup));
            _gridContainer = (RectTransform)gridGo.transform;
            _gridContainer.SetParent(rect, false);
            _gridContainer.anchorMin = new Vector2(0.5f, 0.5f);
            _gridContainer.anchorMax = new Vector2(0.5f, 0.5f);
            _gridContainer.anchoredPosition = new Vector2(0f, -40f);
            _gridContainer.sizeDelta = new Vector2(980, 1400);

            var grid = gridGo.GetComponent<GridLayoutGroup>();
            grid.cellSize = new Vector2(168, 168);
            grid.spacing = new Vector2(20, 20);
            grid.childAlignment = TextAnchor.UpperCenter;
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = Columns;

            var footer = new GameObject("Footer", typeof(RectTransform));
            var footerRect = (RectTransform)footer.transform;
            footerRect.SetParent(rect, false);
            footerRect.anchorMin = new Vector2(0.5f, 0f);
            footerRect.anchorMax = new Vector2(0.5f, 0f);
            footerRect.pivot = new Vector2(0.5f, 0f);
            footerRect.anchoredPosition = new Vector2(0f, 40f);
            footerRect.sizeDelta = new Vector2(900, 140);
            UIFactory.AddHorizontalLayout(footer, spacing: 30);

            _prevButton = UIFactory.CreateButton(footerRect, "< PREV", new Vector2(260, 100), Theme.ButtonBackground, Theme.TextPrimary, () => ChangePage(-1), 32);

            var pageLabelGo = new GameObject("PageLabel", typeof(RectTransform));
            var pageLabelRect = (RectTransform)pageLabelGo.transform;
            pageLabelRect.SetParent(footerRect, false);
            pageLabelRect.sizeDelta = new Vector2(320, 100);
            _pageLabel = UIFactory.CreateText(pageLabelRect, "1-50", 34, Theme.TextSecondary);

            _nextButton = UIFactory.CreateButton(footerRect, "NEXT >", new Vector2(260, 100), Theme.ButtonBackground, Theme.TextPrimary, () => ChangePage(1), 32);
        }

        public void Open(int highestUnlockedLevel)
        {
            _highestUnlocked = highestUnlockedLevel;
            _currentPage = (Mathf.Clamp(highestUnlockedLevel, 1, DifficultyCurve.MaxLevel) - 1) / PageSize;
            RenderPage();
        }

        private void ChangePage(int delta)
        {
            int maxPage = (DifficultyCurve.MaxLevel - 1) / PageSize;
            _currentPage = Mathf.Clamp(_currentPage + delta, 0, maxPage);
            RenderPage();
        }

        private void RenderPage()
        {
            foreach (var go in _spawned) Destroy(go);
            _spawned.Clear();

            int start = _currentPage * PageSize + 1;
            int end = Mathf.Min(start + PageSize - 1, DifficultyCurve.MaxLevel);
            _pageLabel.text = $"{start}-{end}";

            _prevButton.interactable = _currentPage > 0;
            _nextButton.interactable = end < DifficultyCurve.MaxLevel;

            for (int level = start; level <= end; level++)
            {
                _spawned.Add(BuildLevelButton(level));
            }
        }

        private GameObject BuildLevelButton(int level)
        {
            bool completed = level < _highestUnlocked;
            bool current = level == _highestUnlocked;
            bool locked = level > _highestUnlocked;

            Color bg = locked ? Theme.LockedLevel : (current ? Theme.CurrentLevel : Theme.CompletedLevel);
            Color fg = locked ? Theme.TextSecondary : Color.black;

            Button button;
            if (locked)
            {
                button = UIFactory.CreateIconButton(_gridContainer, IconSpriteFactory.Lock(), new Vector2(150, 150), new Vector2(56, 56), bg, fg, null);
                button.interactable = false;
            }
            else
            {
                button = UIFactory.CreateButton(_gridContainer, level.ToString(), new Vector2(150, 150), bg, fg, () => LevelChosen?.Invoke(level), 40);
                if (completed)
                {
                    var badge = UIFactory.CreateIcon(button.transform, IconSpriteFactory.Check(), new Vector2(36, 36), new Color(0f, 0f, 0f, 0.55f));
                    var badgeRect = (RectTransform)badge.transform;
                    badgeRect.anchorMin = badgeRect.anchorMax = new Vector2(1f, 1f);
                    badgeRect.anchoredPosition = new Vector2(-24f, -24f);
                }
            }
            return button.gameObject;
        }
    }
}
