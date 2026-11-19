using UnityEngine;

namespace OldAlley.Enemy
{
    /// <summary>Stage 4 临时刷怪器：开局放 10 个敌人验证寻路，F3 再刷一波，F1 打印状态。</summary>
    public class DebugEnemySpawner : MonoBehaviour
    {
        public EnemyPool pool;
        public EnemyStats[] types;
        public bool spawnOnStart = true;
        public int count = 10;

        private void Start()
        {
            if (spawnOnStart) SpawnWave();
        }

        public void SpawnWave()
        {
            if (pool == null || types == null || types.Length == 0) return;

            for (var i = 0; i < count; i++)
            {
                var type = types[i % types.Length];
                var lane = i % 3;
                var x = lane == 0 ? -22f - i : lane == 1 ? 22f + i : Random.Range(-12f, 12f);
                var y = Random.Range(-2f, -0.8f);
                pool.Spawn(type, new Vector2(x, y));
            }

            Debug.Log("[Stage4] 已生成 " + count + " 个测试敌人");
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.F3)) SpawnWave();
            if (Input.GetKeyDown(KeyCode.F1)) LogStates();
        }

        private void LogStates()
        {
            if (pool == null) return;

            var items = pool.Items;
            for (var i = 0; i < items.Count; i++)
            {
                var enemy = items[i];
                if (enemy != null && enemy.gameObject.activeSelf)
                    Debug.Log("[Enemy] " + enemy.name + " " + enemy.State + " HP " + enemy.Hp);
            }
        }
    }
}
