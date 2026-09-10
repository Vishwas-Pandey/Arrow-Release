using System;
using ReleaseTheArrow.Core;
using ReleaseTheArrow.UI;
using ReleaseTheArrow.Utils;
using UnityEngine;
using UnityEngine.EventSystems;

namespace ReleaseTheArrow.Gameplay
{
    /// One on-screen arrow. Pooled and rebound between levels rather than instantiated/destroyed
    /// per level (Section 38/39 performance requirement — late levels can have 200 of these).
    public class ArrowView : MonoBehaviour, IPointerDownHandler, IPointerClickHandler
    {
        public int ArrowId { get; private set; }

        private RectTransform _rect;
        private ArrowGraphic _graphic;
        private Action<int> _onTapped;
        private bool _interactable;

        // Bumped by Bind() and Invalidate(). Any in-flight tween captures this value and checks
        // it before acting in its completion callback — since this instance is pooled and
        // rebound rather than destroyed, a tween started under a previous binding must never be
        // allowed to mutate or complete against whatever this view has since been rebound to.
        private int _generation;

        public static ArrowView CreateInstance(Transform parent)
        {
            var go = new GameObject("Arrow", typeof(RectTransform), typeof(CanvasRenderer), typeof(ArrowGraphic));
            go.transform.SetParent(parent, false);
            var view = go.AddComponent<ArrowView>();
            view._rect = go.GetComponent<RectTransform>();
            view._graphic = go.GetComponent<ArrowGraphic>();
            view._graphic.raycastTarget = true;
            return view;
        }

        public void Bind(ArrowSpec spec, float cellSize, Action<int> onTapped)
        {
            _generation++;
            ArrowId = spec.id;
            _onTapped = onTapped;

            // Fitted to its own cell rather than overflowing into the next one. Overflow made
            // sense back when boards had empty gaps between arrows (nothing to collide with),
            // but every board is now 100% full — an overflowing arrow overlaps its neighbor's
            // arrow directly, and two overlapping same-direction arrows visually fuse into what
            // reads as one long, ambiguous shape (this is what was actually behind reports of
            // arrows "pointing the wrong way": the direction logic was correct, but adjacent
            // cells' glyphs were visually indistinguishable from each other).
            _rect.anchorMin = _rect.anchorMax = new Vector2(0f, 0f);
            _rect.pivot = new Vector2(0.5f, 0.5f);
            _rect.sizeDelta = new Vector2(cellSize * 0.80f, cellSize * 0.94f);
            _rect.anchoredPosition = new Vector2(
                spec.col * cellSize + cellSize * 0.5f,
                spec.row * cellSize + cellSize * 0.5f);
            _rect.localEulerAngles = new Vector3(0f, 0f, RotationFor(spec.direction));
            _rect.localScale = Vector3.one;

            _graphic.color = Theme.ArrowNormal;
            _interactable = true;
            gameObject.SetActive(true);
        }

        public void Deactivate() => gameObject.SetActive(false);

        /// Invalidates any tween currently in flight against this view without changing its
        /// binding. Call this whenever a view is force-released (e.g. clearing the board for a
        /// restart) so a stale release/blocked animation from the previous level can never fire
        /// its completion callback against whatever this pooled instance is reused for next.
        public void Invalidate() => _generation++;

        // These values assume the base shape (0-degree rotation) visually points Up, rotated by
        // Unity's standard counter-clockwise convention for a UI RectTransform's Z rotation.
        // That assumption is true for ArrowGraphic (its mesh is built with the tip at +Y, no
        // flip) but was FALSE for the raster sprite this used to render with (ArrowSpriteFactory
        // had a vertical-flip bug in its coverage sampling, so its "Up" texture actually rendered
        // pointing down) — the four constants below were reverse-engineered on-device to
        // compensate for that flip. When ArrowGraphic replaced the raster sprite, this method
        // should have been reverted to the values below (the flip it was compensating for no
        // longer exists), but wasn't — leaving every direction rendering rotated 180 degrees
        // from correct while the release/blocking logic (which uses spec.direction directly,
        // never this rotation) stayed right. That mismatch is exactly "arrow flies opposite the
        // way it's pointing." Re-verify with the same on-device tap sweep after touching this.
        private static float RotationFor(ArrowDirection direction) => direction switch
        {
            ArrowDirection.Up => 0f,
            ArrowDirection.Right => -90f,
            ArrowDirection.Down => 180f,
            ArrowDirection.Left => 90f,
            _ => 0f
        };

        public void OnPointerDown(PointerEventData eventData)
        {
            if (!_interactable) return;
            Tween.Scale(_rect, Vector3.one * 0.86f, 0.05f, Ease.OutQuad);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (!_interactable) return;
            _onTapped?.Invoke(ArrowId);
        }

        public void PlayReleaseAndDeactivate(Vector2 exitAnchoredPosition, Action onComplete)
        {
            _interactable = false;
            int gen = _generation;
            _graphic.color = Theme.ArrowReleasedGlow;
            Tween.Scale(_rect, Vector3.one * 1.12f, 0.06f, Ease.OutQuad);
            Tween.AnchoredPosition(_rect, exitAnchoredPosition, 0.26f, Ease.InQuad, () =>
            {
                if (gen != _generation) return; // recycled for a different arrow mid-animation
                gameObject.SetActive(false);
                onComplete?.Invoke();
            });
        }

        public void PlayBlockedShake()
        {
            int gen = _generation;
            Tween.Scale(_rect, Vector3.one, 0.08f, Ease.OutQuad);
            _graphic.color = Theme.ArrowBlockedFlash;
            Tween.Shake(_rect, 10f, 0.26f, () =>
            {
                if (gen != _generation) return;
                if (_graphic != null) _graphic.color = Theme.ArrowNormal;
            });
        }
    }
}
