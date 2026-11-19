using UnityEngine;

namespace OldAlley.Pickup
{
    public enum PickupType
    {
        Bubblegum,
        TinFrog,
        Marble,
        Latiao
    }

    /// <summary>童年道具配置。数值来自 AGENTS.md §6.3。</summary>
    [CreateAssetMenu(menuName = "旧巷守卫/PickupConfig", fileName = "Pickup_New")]
    public class PickupConfig : ScriptableObject
    {
        public PickupType type;
        public string displayName;
        public Sprite icon;
        public Color color = Color.white;

        [Tooltip("泡泡糖/铁皮青蛙 = 加成比例；弹珠 = 穿透数；辣条 = 回血")]
        public float valueA;

        [Tooltip("弹珠 = 弹速加成比例")]
        public float valueB;

        [Tooltip("持续秒数；辣条为 0")]
        public float duration;

        public float pickupRadius = 0.6f;
        public float weight = 1f;
    }
}
