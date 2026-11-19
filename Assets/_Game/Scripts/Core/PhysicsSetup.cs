using UnityEngine;

namespace OldAlley.Core
{
    /// <summary>
    /// 2D 碰撞矩阵：只允许 Player/Enemy/Projectile 撞 Wall，其余自定义层之间互相忽略。
    /// WHY: 运行时统一下发，避免改 ProjectSettings 的二进制矩阵出错；结果与 Inspector 里配的一致。
    /// </summary>
    public static class PhysicsSetup
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Apply()
        {
            var player = LayerMask.NameToLayer("Player");
            var enemy = LayerMask.NameToLayer("Enemy");
            var projectile = LayerMask.NameToLayer("Projectile");
            var wall = LayerMask.NameToLayer("Wall");
            var pickup = LayerMask.NameToLayer("Pickup");

            if (player < 0 || enemy < 0 || projectile < 0 || wall < 0 || pickup < 0)
            {
                Debug.LogWarning("[PhysicsSetup] 自定义 Layer 未配全，跳过碰撞矩阵设置。");
                return;
            }

            // 先关掉所有自定义层两两组合（含自身）。
            var custom = new[] { player, enemy, projectile, wall, pickup };
            for (var i = 0; i < custom.Length; i++)
            for (var j = i; j < custom.Length; j++)
                Physics2D.IgnoreLayerCollision(custom[i], custom[j], true);

            // 只开这几组：角色/子弹撞墙，子弹触发命中敌人。
            Physics2D.IgnoreLayerCollision(player, wall, false);
            Physics2D.IgnoreLayerCollision(enemy, wall, false);
            Physics2D.IgnoreLayerCollision(projectile, wall, false);
            Physics2D.IgnoreLayerCollision(projectile, enemy, false);
            Physics2D.IgnoreLayerCollision(player, enemy, false);   // 敌人可以推挤玩家
            Physics2D.IgnoreLayerCollision(player, pickup, false);  // 玩家自动拾取道具
        }
    }
}
