using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace OldAlley.EditorTools
{
    /// <summary>
    /// 自愈：修正场景里粒子速度曲线模式不一致的问题
    /// （x/y 为 Two Constants，z 为 Constant 会报
    /// "Particle Velocity curves must all be in the same mode"）。
    /// 编辑器重新编译后自动执行一次；也可手动点菜单。
    /// </summary>
    [InitializeOnLoad]
    public static class FixParticles
    {
        private const string DoneKey = "OldAlley.FixParticles.Done";

        static FixParticles()
        {
            EditorApplication.delayCall += TryFix;
        }

        [MenuItem("旧巷守卫/工具/修复粒子曲线")]
        public static void TryFix()
        {
            if (SessionState.GetBool(DoneKey, false)) return;
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;

            var systems = Object.FindObjectsOfType<ParticleSystem>();
            var fixedAny = false;

            foreach (var ps in systems)
            {
                var velocity = ps.velocityOverLifetime;
                if (!velocity.enabled) continue;

                var x = velocity.x;
                var y = velocity.y;
                var z = velocity.z;

                var alreadyOk =
                    x.mode == ParticleSystemCurveMode.TwoConstants &&
                    y.mode == ParticleSystemCurveMode.TwoConstants &&
                    z.mode == ParticleSystemCurveMode.TwoConstants;

                if (alreadyOk) continue;

                velocity.x = new ParticleSystem.MinMaxCurve(x.constantMin, x.constantMax);
                velocity.y = new ParticleSystem.MinMaxCurve(y.constantMin, y.constantMax);
                velocity.z = new ParticleSystem.MinMaxCurve(0f, 0f);

                EditorUtility.SetDirty(ps);

                var scene = ps.gameObject.scene;
                if (scene.IsValid()) EditorSceneManager.MarkSceneDirty(scene);

                fixedAny = true;
                Debug.Log("[FixParticles] 已修复 " + ps.gameObject.name + " 的速度曲线。");
            }

            if (!fixedAny) return;

            EditorSceneManager.SaveOpenScenes();
            SessionState.SetBool(DoneKey, true);
            Debug.Log("[FixParticles] 粒子曲线已统一为 Two Constants，场景已保存。");
        }
    }
}
