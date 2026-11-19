using UnityEngine;
using UnityEngine.EventSystems;

namespace OldAlley.UI
{
    /// <summary>按钮 hover 1.05 / 按下 0.95。</summary>
    public class ButtonScaler : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler,
        IPointerDownHandler, IPointerUpHandler
    {
        public float hoverScale = 1.05f;
        public float pressScale = 0.95f;

        private RectTransform _rect;
        private Vector3 _base = Vector3.one;

        private void Awake()
        {
            _rect = (RectTransform)transform;
            _base = _rect.localScale;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            _rect.localScale = _base * hoverScale;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            _rect.localScale = _base;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            _rect.localScale = _base * pressScale;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            _rect.localScale = _base * hoverScale;
        }
    }
}
