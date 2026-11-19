using System;
using System.Collections.Generic;
using OldAlley.Audio;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace OldAlley.UI
{
    /// <summary>
    /// 运行时 UI 构建工具。所有界面必须走这里，禁止在场景里手摆 Canvas 子物体。
    /// </summary>
    public static class UIFactory
    {
        public static UITheme Theme;

        private static Font _font;
        private static Sprite _white;
        private static readonly Dictionary<string, Sprite> SpriteCache = new Dictionary<string, Sprite>();

        public static UITheme T
        {
            get
            {
                if (Theme == null) Theme = ScriptableObject.CreateInstance<UITheme>();
                return Theme;
            }
        }

        // ---------- 基础资源 ----------

        public static Font GetFont()
        {
            if (_font != null) return _font;

            _font = Font.CreateDynamicFontFromOSFont(
                new[] { "Microsoft YaHei", "SimHei", "PingFang SC", "Arial" }, 32);

            if (_font == null)
                _font = Resources.GetBuiltinResource<Font>("Arial.ttf");

            Debug.LogWarning("<color=yellow>[UIFactory] 未找到项目字体，已使用系统字体，WebGL 下会失效。</color>");
            return _font;
        }

        public static Sprite WhiteSprite()
        {
            if (_white != null) return _white;

            var tex = new Texture2D(4, 4, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Point;
            tex.wrapMode = TextureWrapMode.Clamp;

            var px = new Color32[16];
            for (var i = 0; i < px.Length; i++) px[i] = new Color32(255, 255, 255, 255);
            tex.SetPixels32(px);
            tex.Apply();

            _white = Sprite.Create(tex, new Rect(0, 0, 4, 4), new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect);
            return _white;
        }

        /// <summary>生成 8×8、四角 borderPx 像素描边的九宫格精灵。</summary>
        public static Sprite BoxSprite(Color fill, Color border, int borderPx = 2, int size = 8)
        {
            var key = ColorUtility.ToHtmlStringRGBA(fill) + "|" + ColorUtility.ToHtmlStringRGBA(border) + "|" + borderPx;
            Sprite cached;
            if (SpriteCache.TryGetValue(key, out cached)) return cached;

            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Point;
            tex.wrapMode = TextureWrapMode.Clamp;

            for (var y = 0; y < size; y++)
            for (var x = 0; x < size; x++)
            {
                var isBorder = x < borderPx || y < borderPx || x >= size - borderPx || y >= size - borderPx;
                tex.SetPixel(x, y, isBorder ? border : fill);
            }

            tex.Apply();
            cached = Sprite.Create(
                tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect,
                new Vector4(borderPx, borderPx, borderPx, borderPx));
            SpriteCache[key] = cached;
            return cached;
        }

        // ---------- 布局 ----------

        public static void Stretch(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }

        public static void Place(RectTransform rt, Vector2 anchor, Vector2 pivot, Vector2 pos, Vector2 size)
        {
            rt.anchorMin = anchor;
            rt.anchorMax = anchor;
            rt.pivot = pivot;
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;
        }

        public static RectTransform VStack(Transform parent, string name, float spacing, int padding)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);

            var rt = (RectTransform)go.transform;
            var layout = go.AddComponent<VerticalLayoutGroup>();
            layout.spacing = spacing;
            layout.padding = new RectOffset(padding, padding, padding, padding);
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;

            return rt;
        }

        // ---------- 元件 ----------

        public static Canvas CreateCanvas(string name)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));

            var canvas = go.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;

            var scaler = go.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;

            return canvas;
        }

        public static void EnsureEventSystem()
        {
            if (UnityEngine.Object.FindObjectOfType<EventSystem>() != null) return;
            new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
        }

        public static Image MakeImage(Transform parent, string name, Color color, bool raycast = false)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);

            var img = go.GetComponent<Image>();
            img.sprite = WhiteSprite();
            img.color = color;
            img.raycastTarget = raycast;
            return img;
        }

        public static RectTransform Panel(Transform parent, string name, Vector2 size)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);

            var img = go.GetComponent<Image>();
            img.sprite = BoxSprite(T.paper, T.outline, T.panelBorder);
            img.type = Image.Type.Sliced;
            img.raycastTarget = true;

            var rt = (RectTransform)go.transform;
            rt.sizeDelta = size;
            return rt;
        }

        public static Text Label(Transform parent, string content, int fontSize, Color color,
            TextAnchor anchor = TextAnchor.MiddleCenter)
        {
            var go = new GameObject("Txt", typeof(RectTransform), typeof(Text));
            go.transform.SetParent(parent, false);

            var t = go.GetComponent<Text>();
            t.font = GetFont();
            t.text = content;
            t.fontSize = fontSize;
            t.color = color;
            t.alignment = anchor;
            t.raycastTarget = false;
            t.horizontalOverflow = HorizontalWrapMode.Wrap;
            t.verticalOverflow = VerticalWrapMode.Overflow;
            t.supportRichText = true;
            return t;
        }

        public static Button MakeButton(Transform parent, string label, Vector2 size, Action onClick, int fontSize = -1)
        {
            var go = new GameObject("Btn_" + label, typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);

            var img = go.GetComponent<Image>();
            img.sprite = BoxSprite(T.wood, T.outline, T.panelBorder);
            img.type = Image.Type.Sliced;
            img.color = Color.white;
            img.raycastTarget = true;

            var rt = (RectTransform)go.transform;
            rt.sizeDelta = size;

            var le = go.AddComponent<LayoutElement>();
            le.preferredHeight = size.y;
            le.minHeight = size.y;
            le.preferredWidth = size.x;

            var btn = go.GetComponent<Button>();
            var colors = btn.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(1.12f, 1.12f, 1.12f, 1f);
            colors.pressedColor = new Color(0.78f, 0.78f, 0.78f, 1f);
            colors.selectedColor = Color.white;
            colors.disabledColor = new Color(0.5f, 0.5f, 0.5f, 0.6f);
            colors.fadeDuration = 0.06f;
            btn.colors = colors;

            go.AddComponent<ButtonScaler>();

            btn.onClick.AddListener(() =>
            {
                AudioManager.Play(SfxId.UiClick);
                if (onClick != null) onClick();
            });

            var text = Label(go.transform, label, fontSize > 0 ? fontSize : T.fontBody, T.paper, TextAnchor.MiddleCenter);
            Stretch((RectTransform)text.transform);

            return btn;
        }

        public static Slider MakeSlider(Transform parent, Vector2 size, float min, float max, float value,
            Action<float> onChange)
        {
            var go = new GameObject("Slider", typeof(RectTransform));
            go.transform.SetParent(parent, false);

            var rt = (RectTransform)go.transform;
            rt.sizeDelta = size;

            var le = go.AddComponent<LayoutElement>();
            le.preferredHeight = size.y;

            var bg = MakeImage(go.transform, "Background", T.outline, true);
            var bgRt = (RectTransform)bg.transform;
            bgRt.anchorMin = new Vector2(0f, 0.5f);
            bgRt.anchorMax = new Vector2(1f, 0.5f);
            bgRt.sizeDelta = new Vector2(0f, 14f);
            bgRt.anchoredPosition = Vector2.zero;

            var fillArea = new GameObject("Fill Area", typeof(RectTransform));
            fillArea.transform.SetParent(go.transform, false);
            var faRt = (RectTransform)fillArea.transform;
            faRt.anchorMin = new Vector2(0f, 0.5f);
            faRt.anchorMax = new Vector2(1f, 0.5f);
            faRt.sizeDelta = new Vector2(-20f, 14f);
            faRt.anchoredPosition = Vector2.zero;

            var fill = MakeImage(fillArea.transform, "Fill", T.accentRed, false);
            var fillRt = (RectTransform)fill.transform;
            fillRt.anchorMin = Vector2.zero;
            fillRt.anchorMax = Vector2.one;
            fillRt.sizeDelta = Vector2.zero;
            fillRt.anchoredPosition = Vector2.zero;

            var handleArea = new GameObject("Handle Slide Area", typeof(RectTransform));
            handleArea.transform.SetParent(go.transform, false);
            var haRt = (RectTransform)handleArea.transform;
            haRt.anchorMin = Vector2.zero;
            haRt.anchorMax = Vector2.one;
            haRt.sizeDelta = new Vector2(-20f, 0f);
            haRt.anchoredPosition = Vector2.zero;

            var handle = MakeImage(handleArea.transform, "Handle", T.paper, true);
            var hRt = (RectTransform)handle.transform;
            hRt.sizeDelta = new Vector2(22f, 34f);

            var slider = go.AddComponent<Slider>();
            slider.fillRect = fillRt;
            slider.handleRect = hRt;
            slider.targetGraphic = handle;
            slider.direction = Slider.Direction.LeftToRight;
            slider.minValue = min;
            slider.maxValue = max;
            slider.value = value;
            slider.onValueChanged.AddListener(v => { if (onChange != null) onChange(v); });

            return slider;
        }

        public static Toggle MakeToggle(Transform parent, string label, bool value, Action<bool> onChange)
        {
            var go = new GameObject("Tgl_" + label, typeof(RectTransform));
            go.transform.SetParent(parent, false);

            var rt = (RectTransform)go.transform;
            rt.sizeDelta = new Vector2(400f, 56f);

            var le = go.AddComponent<LayoutElement>();
            le.preferredHeight = 56f;

            var bg = MakeImage(go.transform, "BG", T.outline, true);
            var bgRt = (RectTransform)bg.transform;
            Place(bgRt, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(0f, 0f), new Vector2(32f, 32f));

            var check = MakeImage(bg.transform, "Check", T.successGreen, false);
            var checkRt = (RectTransform)check.transform;
            checkRt.anchorMin = Vector2.zero;
            checkRt.anchorMax = Vector2.one;
            checkRt.offsetMin = new Vector2(6f, 6f);
            checkRt.offsetMax = new Vector2(-6f, -6f);

            var text = Label(go.transform, label, T.fontBody, T.text, TextAnchor.MiddleLeft);
            Place((RectTransform)text.transform, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f),
                new Vector2(48f, 0f), new Vector2(340f, 56f));

            var toggle = go.AddComponent<Toggle>();
            toggle.targetGraphic = bg;
            toggle.graphic = check;
            toggle.isOn = value;
            toggle.onValueChanged.AddListener(v => { if (onChange != null) onChange(v); });

            return toggle;
        }

        public static UIProgressBar MakeBar(Transform parent, string name, Vector2 size, Color fillColor)
        {
            var bg = MakeImage(parent, name, T.outline, false);
            var bgRt = (RectTransform)bg.transform;
            bgRt.sizeDelta = size;

            var fill = MakeImage(bg.transform, "Fill", fillColor, false);
            var fillRt = (RectTransform)fill.transform;
            fillRt.anchorMin = Vector2.zero;
            fillRt.anchorMax = Vector2.one;
            fillRt.offsetMin = new Vector2(2f, 2f);
            fillRt.offsetMax = new Vector2(-2f, -2f);

            return new UIProgressBar(bgRt, fillRt);
        }
    }

    /// <summary>条状进度控件（血条 / 倒计时等）。</summary>
    public class UIProgressBar
    {
        private readonly RectTransform _root;
        private readonly RectTransform _fill;

        public RectTransform Root { get { return _root; } }

        public UIProgressBar(RectTransform root, RectTransform fill)
        {
            _root = root;
            _fill = fill;
        }

        public void Set(float t)
        {
            var clamped = Mathf.Clamp01(t);
            _fill.anchorMax = new Vector2(clamped, 1f);
        }
    }
}
