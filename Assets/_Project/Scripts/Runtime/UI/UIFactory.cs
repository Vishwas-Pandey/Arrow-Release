using System;
using ReleaseTheArrow.Audio;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ReleaseTheArrow.UI
{
    /// Tiny procedural-UI helper. The whole game's UI is built in code rather than hand-authored
    /// scenes/prefabs (this also makes 1500 level-select entries and the varying board sizes
    /// trivial to lay out). Every button is wired exactly once, here, in code — never also in the
    /// inspector — which structurally rules out the double-listener class of bug.
    public static class UIFactory
    {
        private static Font _font;
        public static Font DefaultFont => _font != null ? _font : (_font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"));

        public static Canvas CreateRootCanvas(string name)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = go.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            var scaler = go.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;

            if (UnityEngine.Object.FindFirstObjectByType<EventSystem>() == null)
            {
                new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
            }
            return canvas;
        }

        public static RectTransform CreateFullStretchPanel(Transform parent, string name, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            var rect = (RectTransform)go.transform;
            rect.SetParent(parent, false);
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            go.GetComponent<Image>().color = color;
            return rect;
        }

        public static RectTransform CreatePanel(Transform parent, string name, Vector2 size, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            var rect = (RectTransform)go.transform;
            rect.SetParent(parent, false);
            rect.sizeDelta = size;
            go.GetComponent<Image>().color = color;
            return rect;
        }

        /// LayoutGroups size children via ILayoutElement, not RectTransform.sizeDelta — anything
        /// placed inside one needs this, or it collapses to zero size and overlaps its siblings.
        public static LayoutElement SetPreferredSize(GameObject go, Vector2 size)
        {
            var layoutElement = go.GetComponent<LayoutElement>() ?? go.AddComponent<LayoutElement>();
            layoutElement.preferredWidth = size.x;
            layoutElement.preferredHeight = size.y;
            return layoutElement;
        }

        public static Text CreateText(Transform parent, string content, int fontSize, Color color,
            TextAnchor alignment = TextAnchor.MiddleCenter, FontStyle style = FontStyle.Normal)
        {
            var go = new GameObject("Text", typeof(RectTransform), typeof(Text));
            var rect = (RectTransform)go.transform;
            rect.SetParent(parent, false);
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            var text = go.GetComponent<Text>();
            text.font = DefaultFont;
            text.text = content;
            text.fontSize = fontSize;
            text.color = color;
            text.alignment = alignment;
            text.fontStyle = style;
            text.horizontalOverflow = HorizontalWrapMode.Overflow;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            return text;
        }

        public static Button CreateButton(Transform parent, string label, Vector2 size, Color bgColor, Color textColor, Action onClick, int fontSize = 42)
        {
            var go = new GameObject($"Button_{label}", typeof(RectTransform), typeof(Image), typeof(Button));
            var rect = (RectTransform)go.transform;
            rect.SetParent(parent, false);
            rect.sizeDelta = size;
            SetPreferredSize(go, size);

            var image = go.GetComponent<Image>();
            image.color = bgColor;

            var button = go.GetComponent<Button>();
            var colors = button.colors;
            colors.pressedColor = Color.Lerp(bgColor, Color.black, 0.25f);
            colors.highlightedColor = Color.Lerp(bgColor, Color.white, 0.1f);
            button.colors = colors;

            CreateText(rect, label, fontSize, textColor, TextAnchor.MiddleCenter, FontStyle.Bold);
            button.onClick.AddListener(() =>
            {
                AudioManager.Instance?.Play(Sfx.ButtonTap);
                onClick?.Invoke();
            });
            return button;
        }

        public static Button CreateIconButton(Transform parent, Sprite icon, Vector2 buttonSize, Vector2 iconSize, Color bgColor, Color iconColor, Action onClick)
        {
            var go = new GameObject("IconButton", typeof(RectTransform), typeof(Image), typeof(Button));
            var rect = (RectTransform)go.transform;
            rect.SetParent(parent, false);
            rect.sizeDelta = buttonSize;
            SetPreferredSize(go, buttonSize);

            var image = go.GetComponent<Image>();
            image.color = bgColor;

            var button = go.GetComponent<Button>();
            var colors = button.colors;
            colors.pressedColor = Color.Lerp(bgColor, Color.black, 0.25f);
            colors.highlightedColor = Color.Lerp(bgColor, Color.white, 0.1f);
            button.colors = colors;

            CreateIcon(rect, icon, iconSize, iconColor);
            button.onClick.AddListener(() =>
            {
                AudioManager.Instance?.Play(Sfx.ButtonTap);
                onClick?.Invoke();
            });
            return button;
        }

        public static Image CreateIcon(Transform parent, Sprite sprite, Vector2 size, Color color)
        {
            var go = new GameObject("Icon", typeof(RectTransform), typeof(Image));
            var rect = (RectTransform)go.transform;
            rect.SetParent(parent, false);
            rect.sizeDelta = size;
            SetPreferredSize(go, size);
            var image = go.GetComponent<Image>();
            image.sprite = sprite;
            image.color = color;
            return image;
        }

        public static VerticalLayoutGroup AddVerticalLayout(GameObject go, int spacing = 20, RectOffset padding = null, TextAnchor childAlignment = TextAnchor.UpperCenter)
        {
            var layout = go.AddComponent<VerticalLayoutGroup>();
            layout.spacing = spacing;
            layout.padding = padding ?? new RectOffset(0, 0, 0, 0);
            layout.childAlignment = childAlignment;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;
            return layout;
        }

        public static HorizontalLayoutGroup AddHorizontalLayout(GameObject go, int spacing = 20, RectOffset padding = null, TextAnchor childAlignment = TextAnchor.MiddleCenter)
        {
            var layout = go.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = spacing;
            layout.padding = padding ?? new RectOffset(0, 0, 0, 0);
            layout.childAlignment = childAlignment;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;
            return layout;
        }
    }
}
