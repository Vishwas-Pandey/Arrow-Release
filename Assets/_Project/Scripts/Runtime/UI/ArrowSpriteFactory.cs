using UnityEngine;

namespace ReleaseTheArrow.UI
{
    /// Builds the game's single original arrow glyph at runtime (flat, geometric, supersampled
    /// for clean anti-aliased edges) — pointing canonically Up. Every on-screen arrow reuses this
    /// one sprite and just rotates its RectTransform for the other three directions and tints its
    /// Image color per state (normal/selected/blocked/released), so the whole game needs exactly
    /// one generated texture instead of four.
    public static class ArrowSpriteFactory
    {
        private static Sprite _cached;
        private const int Size = 128;
        private const int Supersample = 4;

        public static Sprite GetArrowSprite()
        {
            if (_cached != null) return _cached;

            var texture = new Texture2D(Size, Size, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp
            };

            var pixels = new Color32[Size * Size];
            for (int py = 0; py < Size; py++)
            {
                for (int px = 0; px < Size; px++)
                {
                    float coverage = SampleCoverage(px, py);
                    pixels[py * Size + px] = new Color(1f, 1f, 1f, coverage);
                }
            }

            texture.SetPixels32(pixels);
            texture.Apply();

            _cached = Sprite.Create(texture, new Rect(0, 0, Size, Size), new Vector2(0.5f, 0.5f), Size);
            return _cached;
        }

        private static float SampleCoverage(int px, int py)
        {
            int hits = 0;
            for (int sy = 0; sy < Supersample; sy++)
            {
                for (int sx = 0; sx < Supersample; sx++)
                {
                    float u = (px + (sx + 0.5f) / Supersample) / Size;
                    float v = (py + (sy + 0.5f) / Supersample) / Size;
                    // Normalize to [-0.5, 0.5], v flipped so +y is up (canonical "Up" arrow).
                    float x = u - 0.5f;
                    float y = (1f - v) - 0.5f;
                    if (IsInsideArrow(x, y)) hits++;
                }
            }
            return hits / (float)(Supersample * Supersample);
        }

        private static bool IsInsideArrow(float x, float y)
        {
            // Triangular head.
            if (PointInTriangle(x, y, 0f, 0.40f, -0.32f, 0.02f, 0.32f, 0.02f)) return true;
            // Rectangular shaft, slightly rounded at the bottom corners.
            if (x >= -0.115f && x <= 0.115f && y >= -0.40f && y <= 0.06f)
            {
                float cornerRadius = 0.05f;
                bool nearBottom = y < -0.40f + cornerRadius;
                if (!nearBottom) return true;

                float cx = Mathf.Sign(x) * (0.115f - cornerRadius);
                if (Mathf.Abs(x) < 0.115f - cornerRadius) return true;
                float dx = x - cx;
                float dy = y - (-0.40f + cornerRadius);
                return dx * dx + dy * dy <= cornerRadius * cornerRadius;
            }
            return false;
        }

        private static bool PointInTriangle(float px, float py, float ax, float ay, float bx, float by, float cx, float cy)
        {
            float d1 = Sign(px, py, ax, ay, bx, by);
            float d2 = Sign(px, py, bx, by, cx, cy);
            float d3 = Sign(px, py, cx, cy, ax, ay);

            bool hasNeg = (d1 < 0) || (d2 < 0) || (d3 < 0);
            bool hasPos = (d1 > 0) || (d2 > 0) || (d3 > 0);
            return !(hasNeg && hasPos);
        }

        private static float Sign(float px, float py, float ax, float ay, float bx, float by)
            => (px - bx) * (ay - by) - (ax - bx) * (py - by);
    }
}
