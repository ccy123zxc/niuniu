using UnityEngine;

namespace OldAlley.Core
{
    /// <summary>3D 碰撞矩阵：子弹不撞子弹/玩家/道具，敌人之间不物理碰撞（分离力负责）。</summary>
    public static class Physics3DSetup
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Apply()
        {
            var player = LayerMask.NameToLayer("Player");
            var enemy = LayerMask.NameToLayer("Enemy");
            var projectile = LayerMask.NameToLayer("Projectile");
            var pickup = LayerMask.NameToLayer("Pickup");

            if (player < 0 || enemy < 0 || projectile < 0 || pickup < 0) return;

            Physics.IgnoreLayerCollision(projectile, projectile, true);
            Physics.IgnoreLayerCollision(projectile, player, true);
            Physics.IgnoreLayerCollision(projectile, pickup, true);
            Physics.IgnoreLayerCollision(enemy, enemy, true);
            Physics.IgnoreLayerCollision(enemy, pickup, true);
        }
    }
}
