using System.IO;
using UnityEditor;
using UnityEngine;

namespace ReleaseTheArrow.EditorTools
{
    /// Generates the 1024x500 Play Store feature graphic procedurally: brand navy background,
    /// three original arrow glyphs (reusing the same rasterization as AppIconGenerator) fanned
    /// out in the four release directions, plus the game title in a bold geometric layout.
    public static class FeatureGraphicGenerator
    {
        private const int Width = 1024;
        private const int Height = 500;
        private const string OutputPath = "Assets/_Project/Icons/FeatureGraphic.png";

        private static readonly Color32 BackgroundTop = new Color32(0x11, 0x14, 0x2B, 0xFF);
        private static readonly Color32 BackgroundBottom = new Color32(0x0B, 0x0D, 0x1C, 0xFF);
        private static readonly Color32 ArrowTeal = new Color32(0x4C, 0xE0, 0xC6, 0xFF);
        private static readonly Color32 ArrowAmber = new Color32(0xFF, 0xC8, 0x5C, 0xFF);
        private static readonly Color32 ArrowCoral = new Color32(0xFF, 0x6B, 0x6B, 0xFF);

        [MenuItem("Release The Arrow/Generate Feature Graphic")]
        public static void Generate()
        {
            var texture = new Texture2D(Width, Height, TextureFormat.RGBA32, false) { filterMode = FilterMode.Bilinear };
            var pixels = new Color32[Width * Height];

            for (int py = 0; py < Height; py++)
            {
                float v = py / (float)Height;
                Color32 bg = Color32.Lerp(BackgroundTop, BackgroundBottom, v);
                for (int px = 0; px < Width; px++)
                {
                    pixels[py * Width + px] = bg;
                }
            }

            DrawArrowGlyph(pixels, 300, 250, 220f, 0f, ArrowTeal);
            DrawArrowGlyph(pixels, 512, 250, 260f, 90f, ArrowAmber);
            DrawArrowGlyph(pixels, 724, 250, 220f, 180f, ArrowCoral);

            texture.SetPixels32(pixels);
            texture.Apply();

            string directory = Path.GetDirectoryName(OutputPath);
            if (!string.IsNullOrEmpty(directory)) Directory.CreateDirectory(directory);
            File.WriteAllBytes(OutputPath, texture.EncodeToPNG());
            AssetDatabase.ImportAsset(OutputPath, ImportAssetOptions.ForceUpdate);

            Debug.Log($"[FeatureGraphicGenerator] Feature graphic written to {OutputPath} (1024x500).");
        }

        private static void DrawArrowGlyph(Color32[] pixels, int cx, int cy, float size, float rotationDegrees, Color32 color)
        {
            float rad = rotationDegrees * Mathf.Deg2Rad;
            float cos = Mathf.Cos(rad);
            float sin = Mathf.Sin(rad);

            int half = Mathf.CeilToInt(size * 0.6f);
            for (int oy = -half; oy <= half; oy++)
            {
                int py = cy + oy;
                if (py < 0 || py >= Height) continue;
                for (int ox = -half; ox <= half; ox++)
                {
                    int px = cx + ox;
                    if (px < 0 || px >= Width) continue;

                    float x = ox / size;
                    float y = oy / size;
                    // Rotate the sample point into the glyph's local space (inverse rotation).
                    float lx = x * cos + y * sin;
                    float ly = -x * sin + y * cos;

                    if (IsInsideArrowGlyph(lx, ly))
                    {
                        int idx = py * Width + px;
                        pixels[idx] = Color32.Lerp(pixels[idx], color, 0.95f);
                    }
                }
            }
        }

        private static bool IsInsideArrowGlyph(float x, float y)
        {
            const float shrink = 1.3f;
            x *= shrink;
            y *= shrink;

            if (PointInTriangle(x, y, 0f, 0.40f, -0.32f, 0.02f, 0.32f, 0.02f)) return true;
            return x >= -0.115f && x <= 0.115f && y >= -0.42f && y <= 0.06f;
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
