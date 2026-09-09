using System;
using System.Collections;
using UnityEngine;

namespace ReleaseTheArrow.Utils
{
    /// Minimal UI tweening helper: a handful of coroutine-driven animations covering everything
    /// this game needs (tap feedback, release slide-out, blocked shake, panel fades). Each tween
    /// bails out cleanly if its target is destroyed mid-flight instead of throwing.
    public static class Tween
    {
        public static Coroutine AnchoredPosition(RectTransform target, Vector2 to, float duration, Ease ease = Ease.OutQuad, Action onComplete = null)
        {
            return TweenRunner.Instance.StartCoroutine(RunVector(
                () => target == null, () => target.anchoredPosition, v => target.anchoredPosition = v,
                to, duration, ease, onComplete));
        }

        public static Coroutine Scale(RectTransform target, Vector3 to, float duration, Ease ease = Ease.OutBack, Action onComplete = null)
        {
            return TweenRunner.Instance.StartCoroutine(RunVector(
                () => target == null, () => target.localScale, v => target.localScale = v,
                to, duration, ease, onComplete));
        }

        public static Coroutine FadeCanvasGroup(CanvasGroup target, float to, float duration, Action onComplete = null)
        {
            return TweenRunner.Instance.StartCoroutine(RunFloat(
                () => target == null, () => target.alpha, v => target.alpha = v,
                to, duration, Ease.Linear, onComplete));
        }

        /// Short punchy shake for "this move is invalid" feedback — returns to the original
        /// anchored position afterwards regardless of how the tween ends.
        public static Coroutine Shake(RectTransform target, float strength = 12f, float duration = 0.28f, Action onComplete = null)
        {
            return TweenRunner.Instance.StartCoroutine(RunShake(target, strength, duration, onComplete));
        }

        private static IEnumerator RunVector(Func<bool> isDead, Func<Vector3> get, Action<Vector3> set,
            Vector3 to, float duration, Ease ease, Action onComplete)
        {
            if (isDead()) yield break;
            Vector3 from = get();
            float t = 0f;
            while (t < duration)
            {
                if (isDead()) yield break;
                t += Time.deltaTime;
                float p = Easing.Evaluate(ease, duration <= 0f ? 1f : t / duration);
                set(Vector3.LerpUnclamped(from, to, p));
                yield return null;
            }
            if (!isDead()) set(to);
            onComplete?.Invoke();
        }

        private static IEnumerator RunFloat(Func<bool> isDead, Func<float> get, Action<float> set,
            float to, float duration, Ease ease, Action onComplete)
        {
            if (isDead()) yield break;
            float from = get();
            float t = 0f;
            while (t < duration)
            {
                if (isDead()) yield break;
                t += Time.deltaTime;
                float p = Easing.Evaluate(ease, duration <= 0f ? 1f : t / duration);
                set(Mathf.LerpUnclamped(from, to, p));
                yield return null;
            }
            if (!isDead()) set(to);
            onComplete?.Invoke();
        }

        private static IEnumerator RunShake(RectTransform target, float strength, float duration, Action onComplete)
        {
            if (target == null) yield break;
            Vector2 original = target.anchoredPosition;
            float t = 0f;
            while (t < duration)
            {
                if (target == null) yield break;
                t += Time.deltaTime;
                float damper = 1f - (t / duration);
                float offset = Mathf.Sin(t * 55f) * strength * damper;
                target.anchoredPosition = original + new Vector2(offset, 0f);
                yield return null;
            }
            if (target != null) target.anchoredPosition = original;
            onComplete?.Invoke();
        }
    }
}
