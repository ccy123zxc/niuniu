using UnityEngine;

namespace OldAlley.Core
{
    /// <summary>
    /// 2.5D 深度排序：越靠下（y 越小）越靠前，sortingOrder 越大。
    /// 挂在角色身上，让它在场景里走位时自然穿插到道具前后。
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public class YSorter : MonoBehaviour
    {
        [Tooltip("排序精度：order = -y × scale")]
        public float scale = 100f;

        private SpriteRenderer _renderer;

        private void Awake()
        {
            _renderer = GetComponent<SpriteRenderer>();
        }

        private void LateUpdate()
        {
            _renderer.sortingOrder = -Mathf.RoundToInt(transform.position.y * scale);
        }
    }
}
