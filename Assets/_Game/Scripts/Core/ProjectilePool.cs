using System.Collections.Generic;
using OldAlley.Player;
using UnityEngine;

namespace OldAlley.Core
{
    /// <summary>石子对象池。启动时预生成固定数量，射击时只做启停，不 Instantiate。</summary>
    [DisallowMultipleComponent]
    public class ProjectilePool : MonoBehaviour
    {
        public Projectile template;
        public int size = 64;

        private readonly List<Projectile> _items = new List<Projectile>();
        private int _cursor;

        public int Total { get { return _items.Count; } }

        public int FreeCount
        {
            get
            {
                var count = 0;
                for (var i = 0; i < _items.Count; i++)
                    if (!_items[i].gameObject.activeSelf) count++;
                return count;
            }
        }

        private void Awake()
        {
            Build();
        }

        public void Build()
        {
            if (_items.Count > 0 || template == null) return;

            for (var i = 0; i < size; i++)
            {
                var projectile = Instantiate(template, transform);
                projectile.gameObject.SetActive(false);
                _items.Add(projectile);
            }

            template.gameObject.SetActive(false);
        }

        public Projectile Get()
        {
            if (_items.Count == 0) Build();
            if (_items.Count == 0) return null;

            for (var i = 0; i < _items.Count; i++)
            {
                var index = (_cursor + i) % _items.Count;
                if (!_items[index].gameObject.activeSelf)
                {
                    _cursor = (index + 1) % _items.Count;
                    return _items[index];
                }
            }

            // 全部在用：复用最老的一发，避免无限生成。
            var oldest = _items[_cursor];
            _cursor = (_cursor + 1) % _items.Count;
            return oldest;
        }
    }
}
