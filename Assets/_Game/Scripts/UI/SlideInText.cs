using UnityEngine;
using UnityEngine.UI;

namespace OldAlley.UI
{
    /// <summary>提示文字从上方滑入（0.25s），用于波次提示。</summary>
    public class SlideInText : MonoBehaviour
    {
        private RectTransform _rect;
        private Text _text;
        private Vector2 _base;
        private float _elapsed;
        private bool _animating;

        private void Awake()
        {
            _rect = (RectTransform)transform;
            _text = GetComponent<Text>();
            _base = _rect.anchoredPosition;
        }

        public void Show(string value)
        {
            if (_text == null) return;
            if (_text.text == value && !_animating) return;

            _text.text = value;

            _rect.anchoredPosition = _base + Vector2.up * 36f;
            var color = _text.color;
            color.a = 0f;
            _text.color = color;

            _elapsed = 0f;
            _animating = true;
        }

        private void Update()
        {
            if (!_animating || _text == null) return;

            _elapsed += Time.unscaledDeltaTime;
            var t = Mathf.Clamp01(_elapsed / 0.25f);

            _rect.anchoredPosition = Vector2.Lerp(_base + Vector2.up * 36f, _base, t);

            var color = _text.color;
            color.a = t;
            _text.color = color;

            if (t >= 1f) _animating = false;
        }
    }
}
