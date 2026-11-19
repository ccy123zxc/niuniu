using System.Collections.Generic;
using OldAlley.Core;
using OldAlley.UI;
using UnityEngine;

namespace OldAlley.Pickup
{
    /// <summary>
    /// BUFF 管理：同类刷新时长不叠加，不同类共存，最多 3 个（第 4 个顶掉最早的）。
    /// 所有加成都走「基础值 × 倍率」，绝不修改基础数值。
    /// </summary>
    public class BuffSystem : MonoBehaviour
    {
        public static BuffSystem Instance { get; private set; }

        public HouseController house;
        public int maxBuffs = 3;

        public class BuffInstance
        {
            public PickupConfig config;
            public float remaining;
            public float total;
        }

        private readonly List<BuffInstance> _active = new List<BuffInstance>();

        public IReadOnlyList<BuffInstance> Active { get { return _active; } }

        private void Awake()
        {
            Instance = this;
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        public void Apply(PickupConfig config, Vector3 worldPos)
        {
            if (config == null) return;

            if (config.type == PickupType.Latiao)
            {
                if (house != null) house.Heal(Mathf.RoundToInt(config.valueA));
                ShowText("老屋 +" + Mathf.RoundToInt(config.valueA), worldPos);
                return;
            }

            var existing = _active.Find(b => b.config.type == config.type);
            if (existing != null)
            {
                existing.remaining = config.duration;
                existing.total = config.duration;
            }
            else
            {
                if (_active.Count >= maxBuffs) _active.RemoveAt(0);
                _active.Add(new BuffInstance
                {
                    config = config,
                    remaining = config.duration,
                    total = config.duration
                });
            }

            ShowText(config.displayName, worldPos);
        }

        private void ShowText(string text, Vector3 worldPos)
        {
            if (UIManager.Instance != null)
                UIManager.Instance.ShowFloatText(text, worldPos, new Color(1f, 0.95f, 0.75f));
        }

        private void Update()
        {
            for (var i = _active.Count - 1; i >= 0; i--)
            {
                _active[i].remaining -= Time.deltaTime;
                if (_active[i].remaining <= 0f) _active.RemoveAt(i);
            }

            if (UIManager.Instance != null && UIManager.Instance.Hud != null)
                UIManager.Instance.Hud.SetBuffs(_active);
        }

        public float MoveMultiplier
        {
            get
            {
                var m = 1f;
                for (var i = 0; i < _active.Count; i++)
                    if (_active[i].config.type == PickupType.TinFrog)
                        m *= 1f + _active[i].config.valueA;
                return m;
            }
        }

        public float FireIntervalMultiplier
        {
            get
            {
                var m = 1f;
                for (var i = 0; i < _active.Count; i++)
                    if (_active[i].config.type == PickupType.Bubblegum)
                        m *= 1f / (1f + _active[i].config.valueA);
                return m;
            }
        }

        public float BulletSpeedMultiplier
        {
            get
            {
                var m = 1f;
                for (var i = 0; i < _active.Count; i++)
                    if (_active[i].config.type == PickupType.Marble)
                        m *= 1f + _active[i].config.valueB;
                return m;
            }
        }

        public int Pierce
        {
            get
            {
                var pierce = 0;
                for (var i = 0; i < _active.Count; i++)
                    if (_active[i].config.type == PickupType.Marble)
                        pierce = Mathf.Max(pierce, Mathf.RoundToInt(_active[i].config.valueA));
                return pierce;
            }
        }
    }
}
