using System.Collections.Generic;
using ReleaseTheArrow.Audio;
using ReleaseTheArrow.Core;
using ReleaseTheArrow.UI;
using ReleaseTheArrow.Utils;
using UnityEngine;

namespace ReleaseTheArrow.Gameplay
{
    /// Renders the active LevelSession's board: spawns/positions pooled ArrowViews, turns taps
    /// into GameManager.TapArrow calls, and plays the resulting release/blocked feedback. Cell
    /// size auto-fits the viewport with a floor on touch-target size — boards that would need
    /// smaller cells than that floor scroll instead of shrinking hitboxes (Section 22/23).
    public class BoardController : MonoBehaviour
    {
        private const float MinCellSize = 90f;
        private const float MaxCellSize = 180f;

        private RectTransform viewport;
        private RectTransform content;

        private ObjectPool<ArrowView> _arrowPool;
        private readonly Dictionary<int, ArrowView> _activeViews = new Dictionary<int, ArrowView>();
        private readonly List<DotGraphic> _cellBackgrounds = new List<DotGraphic>();
        private ObjectPool<DotGraphic> _cellPool;

        private float _cellSize;
        private int _width;
        private int _height;

        /// Everything here is built procedurally rather than wired up in a hand-authored scene,
        /// so the viewport/content rects are supplied by the code that constructed this object.
        public void Initialize(RectTransform viewportRect, RectTransform contentRect)
        {
            viewport = viewportRect;
            content = contentRect;
            _arrowPool = new ObjectPool<ArrowView>(() => ArrowView.CreateInstance(content), prewarm: 32);
            _cellPool = new ObjectPool<DotGraphic>(CreateCellBackground, prewarm: 32);
        }

        public void LoadSession(LevelSession session)
        {
            ClearBoard();

            _width = session.Layout.width;
            _height = session.Layout.height;
            _cellSize = ComputeCellSize(_width, _height);

            content.sizeDelta = new Vector2(_width * _cellSize, _height * _cellSize);

            SpawnGridBackground();

            foreach (var arrow in session.Board.ActiveArrows)
            {
                SpawnArrow(arrow);
            }
        }

        private float ComputeCellSize(int width, int height)
        {
            float sizeByWidth = viewport.rect.width / width;
            float sizeByHeight = viewport.rect.height / height;
            float size = Mathf.Min(sizeByWidth, sizeByHeight);
            return Mathf.Clamp(size, MinCellSize, MaxCellSize);
        }

        /// One small dot per grid cell rather than a solid colored tile — arrows render directly
        /// on top with no box behind them, and a dot is what's left once its arrow is released.
        private void SpawnGridBackground()
        {
            for (int c = 0; c < _width; c++)
            {
                for (int r = 0; r < _height; r++)
                {
                    var cell = _cellPool.Get();
                    var rect = (RectTransform)cell.transform;
                    rect.anchorMin = rect.anchorMax = Vector2.zero;
                    rect.pivot = new Vector2(0.5f, 0.5f);
                    rect.sizeDelta = new Vector2(_cellSize * 0.16f, _cellSize * 0.16f);
                    rect.anchoredPosition = new Vector2(c * _cellSize + _cellSize * 0.5f, r * _cellSize + _cellSize * 0.5f);
                    _cellBackgrounds.Add(cell);
                }
            }
        }

        private DotGraphic CreateCellBackground()
        {
            var go = new GameObject("Cell", typeof(RectTransform), typeof(CanvasRenderer), typeof(DotGraphic));
            go.transform.SetParent(content, false);
            var dot = go.GetComponent<DotGraphic>();
            dot.color = Theme.GridDot;
            dot.raycastTarget = false;
            return dot;
        }

        private void SpawnArrow(Core.ArrowSpec spec)
        {
            var view = _arrowPool.Get();
            view.Bind(spec, _cellSize, OnArrowTapped);
            _activeViews[spec.id] = view;
        }

        private void OnArrowTapped(int arrowId)
        {
            var manager = GameManager.Instance;
            if (manager == null || manager.CurrentSession == null) return;
            if (!_activeViews.TryGetValue(arrowId, out var view)) return;

            manager.CurrentSession.Board.TryGetArrow(arrowId, out var spec);
            var result = manager.TapArrow(arrowId);

            switch (result)
            {
                case TapResult.Released:
                    AudioManager.Instance?.Play(Sfx.ArrowRelease);
                    Vector2 exit = ComputeExitPosition(spec);
                    view.PlayReleaseAndDeactivate(exit, () =>
                    {
                        _activeViews.Remove(arrowId);
                        _arrowPool.Release(view);
                    });
                    break;

                case TapResult.Blocked:
                    AudioManager.Instance?.Play(Sfx.ArrowBlocked);
                    AudioManager.Instance?.Play(Sfx.LifeLost);
                    HapticManager.Notify();
                    view.PlayBlockedShake();
                    break;
            }
        }

        private Vector2 ComputeExitPosition(Core.ArrowSpec spec)
        {
            float x = spec.col * _cellSize + _cellSize * 0.5f;
            float y = spec.row * _cellSize + _cellSize * 0.5f;

            switch (spec.direction)
            {
                case ArrowDirection.Up: return new Vector2(x, _height * _cellSize + _cellSize);
                case ArrowDirection.Down: return new Vector2(x, -_cellSize);
                case ArrowDirection.Left: return new Vector2(-_cellSize, y);
                default: return new Vector2(_width * _cellSize + _cellSize, y); // Right
            }
        }

        private void ClearBoard()
        {
            foreach (var view in _activeViews.Values)
            {
                // A view mid release/blocked animation must not let that stale tween's
                // completion callback fire against whatever this pooled instance becomes next.
                view.Invalidate();
                _arrowPool.Release(view);
            }
            _activeViews.Clear();

            foreach (var cell in _cellBackgrounds) _cellPool.Release(cell);
            _cellBackgrounds.Clear();
        }
    }
}
