using UnityEngine;

namespace OldAlley.Core
{
    /// <summary>
    /// 全局基础数值。所有魔法数字的唯一来源（AGENTS.md §6）。
    /// </summary>
    [CreateAssetMenu(menuName = "旧巷守卫/GameConfig", fileName = "GameConfig")]
    public class GameConfig : ScriptableObject
    {
        [Header("老屋")]
        public int houseMaxHp = 100;

        [Header("玩家")]
        public float playerSpeed = 4.8f;
        public float playerAcceleration = 60f;
        public float playerDeceleration = 80f;

        [Header("弹弓")]
        public float fireInterval = 0.32f;
        public int bulletDamage = 12;
        public float bulletSpeed = 14f;
        public float bulletLife = 1.0f;
        public float bulletSpread = 1.5f;
        public float muzzleOffset = 0.35f;
        public int projectilePoolSize = 64;

        [Header("道具")]
        public int pickupMaxOnField = 2;
    }
}
