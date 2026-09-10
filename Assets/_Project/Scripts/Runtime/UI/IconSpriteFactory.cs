using UnityEngine;

namespace ReleaseTheArrow.UI
{
    /// A handful of small original icon glyphs (heart, lock, checkmark, pause), built the same
    /// supersampled-rasterization way as the arrow sprite. Keeps the whole game's iconography
    /// consistent and dependency-free.
    public static class IconSpriteFactory
    {
        private const int Size = 96;
        private const int Supersample = 4;

        private static readonly System.Collections.Generic.Dictionary<string, Sprite> Cache = new System.Collections.Generic.Dictionary<string, Sprite>();

        public static Sprite Heart() => GetOrBuild("heart", IsInsideHeart);
        public static Sprite Lock() => GetOrBuild("lock", IsInsideLock);
        public static Sprite Check() => GetOrBuild("check", IsInsideCheck);
        public static Sprite Pause() => GetOrBuild("pause", IsInsidePause);
        public static Sprite Dot() => GetOrBuild("dot", IsInsideDot);

        private static Sprite GetOrBuild(string key, System.Func<float, float, bool> shape)
        {
            if (Cache.TryGetValue(key, out var sprite) && sprite != null) return sprite;

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
                    int hits = 0;
                    for (int sy = 0; sy < Supersample; sy++)
                    {
                        for (int sx = 0; sx < Supersample; sx++)
                        {
                            float u = (px + (sx + 0.5f) / Supersample) / Size;
                            float v = (py + (sy + 0.5f) / Supersample) / Size;
                            float x = u - 0.5f;
                            float y = (1f - v) - 0.5f;
                            if (shape(x, y)) hits++;
                        }
                    }
                    pixels[py * Size + px] = new Color(1f, 1f, 1f, hits / (float)(Supersample * Supersample));
                }
            }

            texture.SetPixels32(pixels);
            texture.Apply();
            sprite = Sprite.Create(texture, new Rect(0, 0, Size, Size), new Vector2(0.5f, 0.5f), Size);
            Cache[key] = sprite;
            return sprite;
        }

        private static bool IsInsideHeart(float x, float y)
        {
            if (InCircle(x, y, -0.20f, 0.15f, 0.28f)) return true;
            if (InCircle(x, y, 0.20f, 0.15f, 0.28f)) return true;
            return PointInTriangle(x, y, 0f, -0.46f, -0.44f, 0.14f, 0.44f, 0.14f);
        }

        private static bool IsInsideLock(float x, float y)
        {
            // Body.
            if (x >= -0.30f && x <= 0.30f && y >= -0.38f && y <= 0.02f) return true;
            // Shackle (ring arch).
            float dist = Mathf.Sqrt(x * x + (y - 0.05f) * (y - 0.05f));
            return dist <= 0.24f && dist >= 0.12f && y >= 0.03f;
        }

        private static bool IsInsideCheck(float x, float y)
        {
            if (DistToSegment(x, y, -0.32f, -0.02f, -0.06f, -0.28f) <= 0.075f) return true;
            return DistToSegment(x, y, -0.06f, -0.28f, 0.34f, 0.32f) <= 0.075f;
        }

        private static bool IsInsidePause(float x, float y)
        {
            if (x >= -0.28f && x <= -0.08f && y >= -0.32f && y <= 0.32f) return true;
            return x >= 0.08f && x <= 0.28f && y >= -0.32f && y <= 0.32f;
        }

        /// The board's resting grid marker — every cell shows one, arrows sit on top of it, and
        /// it's what's left once an arrow is released, per the "matrix of dots" board look.
        private static bool IsInsideDot(float x, float y) => InCircle(x, y, 0f, 0f, 0.34f);

        private static bool InCircle(float x, float y, float cx, float cy, float r) => (x - cx) * (x - cx) + (y - cy) * (y - cy) <= r * r;

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

        private static float DistToSegment(float px, float py, float ax, float ay, float bx, float by)
        {
            float abx = bx - ax, aby = by - ay;
            float apx = px - ax, apy = py - ay;
            float lenSq = abx * abx + aby * aby;
            float t = lenSq > 0f ? Mathf.Clamp01((apx * abx + apy * aby) / lenSq) : 0f;
            float cx = ax + abx * t, cy = ay + aby * t;
            float dx = px - cx, dy = py - cy;
            return Mathf.Sqrt(dx * dx + dy * dy);
        }
    }
}
