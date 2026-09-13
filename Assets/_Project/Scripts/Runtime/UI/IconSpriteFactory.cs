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
        public static Sprite Star() => GetOrBuild("star", IsInsideStar);
        public static Sprite Gear() => GetOrBuild("gear", IsInsideGear);
        public static Sprite BarChart() => GetOrBuild("barchart", IsInsideBarChart);

        private static Sprite _roundedRect;

        /// A 9-sliced rounded-rectangle/pill background, used for buttons that sit over
        /// photographic art where the flat rectangular Theme.ButtonBackground look would clash.
        /// The border keeps corners round at any final button size (Image.type must be set to
        /// Sliced by the caller — Sprite.Create alone doesn't do that).
        public static Sprite RoundedRect()
        {
            if (_roundedRect != null) return _roundedRect;

            const int size = 64;
            const float radius = 22f;
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp
            };
            const float half = size / 2f;
            var pixels = new Color32[size * size];
            for (int py = 0; py < size; py++)
            {
                for (int px = 0; px < size; px++)
                {
                    float x = px + 0.5f, y = py + 0.5f;
                    float qx = Mathf.Max(Mathf.Abs(x - half) - (half - radius), 0f);
                    float qy = Mathf.Max(Mathf.Abs(y - half) - (half - radius), 0f);
                    float signedDist = Mathf.Sqrt(qx * qx + qy * qy) - radius;
                    float alpha = Mathf.Clamp01(0.5f - signedDist);
                    pixels[py * size + px] = new Color(1f, 1f, 1f, alpha);
                }
            }
            texture.SetPixels32(pixels);
            texture.Apply();

            int border = (int)radius;
            _roundedRect = Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size,
                0, SpriteMeshType.FullRect, new Vector4(border, border, border, border));
            return _roundedRect;
        }

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

        private static readonly Vector2[] StarPoints = BuildStarPoints(5, 0.46f, 0.18f);

        private static Vector2[] BuildStarPoints(int points, float outerRadius, float innerRadius)
        {
            var verts = new Vector2[points * 2];
            for (int i = 0; i < points * 2; i++)
            {
                float r = (i % 2 == 0) ? outerRadius : innerRadius;
                float angle = Mathf.PI / 2f + i * Mathf.PI / points;
                verts[i] = new Vector2(Mathf.Cos(angle) * r, Mathf.Sin(angle) * r);
            }
            return verts;
        }

        private static bool IsInsideStar(float x, float y) => PointInPolygon(x, y, StarPoints);

        private static bool IsInsideGear(float x, float y)
        {
            float dist = Mathf.Sqrt(x * x + y * y);
            if (dist <= 0.14f) return false; // center hole
            if (dist <= 0.24f) return true; // hub ring
            if (dist > 0.40f) return false;
            // 8 teeth as angular spokes around the outer ring.
            float angle = Mathf.Atan2(y, x);
            float slice = (angle + Mathf.PI * 2f) % (Mathf.PI / 4f);
            return slice < (Mathf.PI / 4f) * 0.55f;
        }

        private static bool IsInsideBarChart(float x, float y)
        {
            if (x < -0.38f || x > 0.38f || y < -0.34f || y > 0.34f) return false;
            // Three ascending bars, baseline at the bottom of the icon.
            if (x >= -0.38f && x <= -0.16f) return y <= -0.34f + 0.42f;
            if (x >= -0.09f && x <= 0.13f) return y <= -0.34f + 0.62f;
            if (x >= 0.20f && x <= 0.38f) return y <= -0.34f + 0.86f;
            return false;
        }

        private static bool PointInPolygon(float px, float py, Vector2[] poly)
        {
            bool inside = false;
            for (int i = 0, j = poly.Length - 1; i < poly.Length; j = i++)
            {
                float xi = poly[i].x, yi = poly[i].y;
                float xj = poly[j].x, yj = poly[j].y;
                bool intersect = (yi > py) != (yj > py) &&
                    px < (xj - xi) * (py - yi) / (yj - yi) + xi;
                if (intersect) inside = !inside;
            }
            return inside;
        }

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
