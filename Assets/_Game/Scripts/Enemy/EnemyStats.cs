using UnityEngine;

namespace OldAlley.Enemy
{
    /// <summary>敌人数值与外观。数值严格来自 AGENTS.md §6.2。</summary>
    [CreateAssetMenu(menuName = "旧巷守卫/EnemyStats", fileName = "Enemy_New")]
    public class EnemyStats : ScriptableObject
    {
        public string displayName;

        [Header("数值")]
        public int hp = 30;
        public float speed = 1.6f;
        public int damageToHouse = 8;
        public float scale = 1f;
        public int score = 10;
        public float weight = 1f;

        [Header("外观")]
        public Color tint = Color.white;
        public Sprite sprite;

        [Header("3D 体素")]
        [Tooltip("0=野鸡 1=野狗 2=水牛 3=野猪王")]
        public int visualIndex;
        public float colliderRadius = 0.35f;
    }
}
