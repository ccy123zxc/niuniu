using System.Collections.Generic;
using OldAlley.Core;
using UnityEngine;

namespace OldAlley.Enemy
{
    /// <summary>敌人对象池：预生成 32 个，按需复用，回收时重置状态。</summary>
    public class EnemyPool : MonoBehaviour
    {
        public EnemyBase template;
        public HouseController house;
        public int size = 32;

        private readonly List<EnemyBase> _items = new List<EnemyBase>();

        public IReadOnlyList<EnemyBase> Items { get { return _items; } }

        private void Awake()
        {
            Build();
        }

        public void Build()
        {
            if (_items.Count > 0 || template == null) return;

            for (var i = 0; i < size; i++)
            {
                var enemy = Instantiate(template, transform);
                enemy.house = house;
                enemy.pool = this;
                enemy.gameObject.SetActive(false);
                _items.Add(enemy);
            }

            template.gameObject.SetActive(false);
        }

        public EnemyBase Spawn(EnemyStats stats, Vector3 position)
        {
            if (_items.Count == 0) Build();

            for (var i = 0; i < _items.Count; i++)
            {
                if (!_items[i].gameObject.activeSelf)
                {
                    _items[i].Spawn(stats, position);
                    return _items[i];
                }
            }

            return null;
        }
    }
}
