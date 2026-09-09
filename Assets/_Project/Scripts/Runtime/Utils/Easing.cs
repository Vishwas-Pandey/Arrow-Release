using UnityEngine;

namespace ReleaseTheArrow.Utils
{
    public enum Ease { Linear, OutQuad, InQuad, OutBack, OutElastic, OutBounce }

    public static class Easing
    {
        public static float Evaluate(Ease ease, float t)
        {
            t = Mathf.Clamp01(t);
            switch (ease)
            {
                case Ease.Linear: return t;
                case Ease.InQuad: return t * t;
                case Ease.OutQuad: return 1f - (1f - t) * (1f - t);
                case Ease.OutBack:
                {
                    const float c1 = 1.70158f;
                    const float c3 = c1 + 1f;
                    float x = t - 1f;
                    return 1f + c3 * x * x * x + c1 * x * x;
                }
                case Ease.OutElastic:
                {
                    const float c4 = (2f * Mathf.PI) / 3f;
                    if (t <= 0f) return 0f;
                    if (t >= 1f) return 1f;
                    return Mathf.Pow(2f, -10f * t) * Mathf.Sin((t * 10f - 0.75f) * c4) + 1f;
                }
                case Ease.OutBounce:
                {
                    const float n1 = 7.5625f;
                    const float d1 = 2.75f;
                    if (t < 1f / d1) return n1 * t * t;
                    if (t < 2f / d1) { t -= 1.5f / d1; return n1 * t * t + 0.75f; }
                    if (t < 2.5f / d1) { t -= 2.25f / d1; return n1 * t * t + 0.9375f; }
                    t -= 2.625f / d1;
                    return n1 * t * t + 0.984375f;
                }
                default: return t;
            }
        }
    }
}
