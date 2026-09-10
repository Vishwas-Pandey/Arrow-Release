using UnityEngine;
using UnityEngine.UI;

namespace ReleaseTheArrow.UI
{
    /// The board's arrow glyph, drawn as an actual mesh (triangle head + rectangular shaft)
    /// rather than a rasterized texture. A mesh scales with the RectTransform and stays
    /// perfectly crisp at any zoom level, which a fixed-resolution sprite cannot — needed once
    /// the board supports pinch-zoom. Same proportions as the original ArrowSpriteFactory glyph
    /// (which still generates the raster app icon/feature graphic, where a mesh isn't relevant).
    public class ArrowGraphic : MaskableGraphic
    {
        private const float HeadTipY = 0.46f;
        private const float HeadBaseY = 0.10f;
        private const float HeadHalfWidth = 0.19f;
        private const float ShaftHalfWidth = 0.065f;
        private const float ShaftTop = HeadBaseY + 0.02f;
        private const float ShaftBottom = -0.46f;

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
            var rect = rectTransform.rect;
            float w = rect.width, h = rect.height;
            Color32 c = color;

            Vector2 P(float nx, float ny) => new Vector2(nx * w, ny * h);

            AddTriangle(vh, P(0f, HeadTipY), P(-HeadHalfWidth, HeadBaseY), P(HeadHalfWidth, HeadBaseY), c);
            AddQuad(vh,
                P(-ShaftHalfWidth, ShaftTop), P(ShaftHalfWidth, ShaftTop),
                P(ShaftHalfWidth, ShaftBottom), P(-ShaftHalfWidth, ShaftBottom), c);
        }

        private static void AddTriangle(VertexHelper vh, Vector2 a, Vector2 b, Vector2 c, Color32 color)
        {
            int start = vh.currentVertCount;
            vh.AddVert(a, color, Vector2.zero);
            vh.AddVert(b, color, Vector2.zero);
            vh.AddVert(c, color, Vector2.zero);
            vh.AddTriangle(start, start + 1, start + 2);
        }

        private static void AddQuad(VertexHelper vh, Vector2 a, Vector2 b, Vector2 c, Vector2 d, Color32 color)
        {
            int start = vh.currentVertCount;
            vh.AddVert(a, color, Vector2.zero);
            vh.AddVert(b, color, Vector2.zero);
            vh.AddVert(c, color, Vector2.zero);
            vh.AddVert(d, color, Vector2.zero);
            vh.AddTriangle(start, start + 1, start + 2);
            vh.AddTriangle(start, start + 2, start + 3);
        }
    }

    /// The board's resting grid marker, drawn as an actual circular mesh for the same
    /// stay-crisp-at-any-zoom reason as ArrowGraphic.
    public class DotGraphic : MaskableGraphic
    {
        private const int Segments = 20;
        private const float Radius = 0.34f;

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
            var rect = rectTransform.rect;
            float w = rect.width, h = rect.height;
            Color32 c = color;

            int center = 0;
            vh.AddVert(Vector2.zero, c, Vector2.zero);
            for (int i = 0; i <= Segments; i++)
            {
                float angle = i / (float)Segments * Mathf.PI * 2f;
                var p = new Vector2(Mathf.Cos(angle) * Radius * w, Mathf.Sin(angle) * Radius * h);
                vh.AddVert(p, c, Vector2.zero);
                if (i > 0) vh.AddTriangle(center, center + i, center + i + 1);
            }
        }
    }
}
