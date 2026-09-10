using UnityEngine;

namespace ReleaseTheArrow.UI
{
    /// The game's original visual identity: a dark, modern "neon on navy" palette, distinct from
    /// the light candy-color boards common in this genre's reference games.
    public static class Theme
    {
        public static readonly Color BackgroundTop = new Color32(0x1B, 0x1F, 0x3B, 0xFF);
        public static readonly Color BackgroundBottom = new Color32(0x0B, 0x0D, 0x1C, 0xFF);

        public static readonly Color CellEmpty = new Color32(0x24, 0x29, 0x48, 0x66);
        public static readonly Color CellEmptyBorder = new Color32(0x35, 0x3B, 0x63, 0xFF);
        public static readonly Color GridDot = new Color32(0x40, 0x47, 0x74, 0xFF);

        public static readonly Color ArrowNormal = new Color32(0x53, 0xD3, 0xE0, 0xFF);      // teal/cyan
        public static readonly Color ArrowNormalShadow = new Color32(0x2B, 0x8E, 0x9E, 0xFF);
        public static readonly Color ArrowPressed = new Color32(0xFF, 0xC8, 0x5C, 0xFF);     // warm amber
        public static readonly Color ArrowBlockedFlash = new Color32(0xFF, 0x5C, 0x6E, 0xFF); // coral red
        public static readonly Color ArrowReleasedGlow = new Color32(0x74, 0xF2, 0xB0, 0xFF); // mint

        public static readonly Color AccentPrimary = new Color32(0xFF, 0xC8, 0x5C, 0xFF);
        public static readonly Color AccentSecondary = new Color32(0x53, 0xD3, 0xE0, 0xFF);
        public static readonly Color Danger = new Color32(0xFF, 0x5C, 0x6E, 0xFF);
        public static readonly Color Success = new Color32(0x74, 0xF2, 0xB0, 0xFF);

        public static readonly Color PanelBackground = new Color32(0x1E, 0x22, 0x3F, 0xF2);
        public static readonly Color ButtonBackground = new Color32(0x2C, 0x32, 0x57, 0xFF);

        public static readonly Color TextPrimary = new Color32(0xF4, 0xF6, 0xFF, 0xFF);
        public static readonly Color TextSecondary = new Color32(0xA6, 0xAC, 0xD4, 0xFF);

        public static readonly Color HeartFull = new Color32(0xFF, 0x5C, 0x8A, 0xFF);
        public static readonly Color HeartEmpty = new Color32(0x3A, 0x3F, 0x5C, 0xFF);

        public static readonly Color LockedLevel = new Color32(0x2A, 0x2F, 0x50, 0xFF);
        public static readonly Color CompletedLevel = new Color32(0x74, 0xF2, 0xB0, 0xFF);
        public static readonly Color CurrentLevel = new Color32(0xFF, 0xC8, 0x5C, 0xFF);
    }
}
