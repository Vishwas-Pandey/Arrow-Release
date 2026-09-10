using System.IO;
using UnityEditor;
using UnityEngine;

namespace ReleaseTheArrow.EditorTools
{
    /// Generates the game's app icon procedurally (same original arrow glyph as in-game, on the
    /// brand background) and assigns it as the legacy Android application icon — closing the
    /// "empty icon slot silently ships with Unity's default icon" gap. Adaptive icon
    /// foreground/background layers are a nice-to-have beyond this; this covers the icon Play
    /// Console and every launcher actually requires.
    public static class AppIconGenerator
    {
        private const int Size = 1024;
        private const int Supersample = 2;
        private const string OutputPath = "Assets/_Project/Icons/AppIcon.png";

        private static readonly Color32 Background = new Color32(0x0B, 0x0D, 0x1C, 0xFF); // Theme.BackgroundBottom
        private static readonly Color32 ArrowColor = new Color32(0xFF, 0xC8, 0x5C, 0xFF);  // Theme.AccentPrimary

        [MenuItem("Release The Arrow/Generate + Assign App Icon")]
        public static void GenerateAndAssign()
        {
            var texture = BuildIconTexture();

            string directory = Path.GetDirectoryName(OutputPath);
            if (!string.IsNullOrEmpty(directory)) Directory.CreateDirectory(directory);
            File.WriteAllBytes(OutputPath, texture.EncodeToPNG());
            AssetDatabase.ImportAsset(OutputPath, ImportAssetOptions.ForceUpdate);

            if (AssetImporter.GetAtPath(OutputPath) is TextureImporter importer)
            {
                importer.textureType = TextureImporterType.Default;
                importer.mipmapEnabled = false;
                importer.maxTextureSize = 1024;
                importer.SaveAndReimport();
            }

            var iconAsset = AssetDatabase.LoadAssetAtPath<Texture2D>(OutputPath);
            var icons = new[] { iconAsset };
            PlayerSettings.SetIconsForTargetGroup(BuildTargetGroup.Android, icons);
            PlayerSettings.SetIconsForTargetGroup(BuildTargetGroup.Unknown, icons);

            Debug.Log($"[AppIconGenerator] Icon written to {OutputPath} and assigned as the Android app icon.");
        }

        private static Texture2D BuildIconTexture()
        {
            var texture = new Texture2D(Size, Size, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Bilinear
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
                            if (IsInsideArrowGlyph(x, y)) hits++;
                        }
                    }
                    float coverage = hits / (float)(Supersample * Supersample);
                    pixels[py * Size + px] = Color32.Lerp(Background, ArrowColor, coverage);
                }
            }

            texture.SetPixels32(pixels);
            texture.Apply();
            return texture;
        }

        private static bool IsInsideArrowGlyph(float x, float y)
        {
            // Shrink the glyph to ~70% of the canvas (adaptive-icon-safe margin) by scaling the
            // sample point up before testing against the same proportions as the in-game arrow.
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
