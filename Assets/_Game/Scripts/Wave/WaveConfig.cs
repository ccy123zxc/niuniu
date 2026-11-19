using System;
using OldAlley.Enemy;
using UnityEngine;

namespace OldAlley.Wave
{
    [Serializable]
    public class WaveEntry
    {
        public EnemyStats stats;
        public int count;
    }

    /// <summary>单波配置。数值来自 AGENTS.md §6.1。</summary>
    [CreateAssetMenu(menuName = "旧巷守卫/WaveConfig", fileName = "Wave_New")]
    public class WaveConfig : ScriptableObject
    {
        public int index = 1;
        public string themeName;
        public float prepTime = 8f;
        public float spawnInterval = 1.2f;
        public int maxAlive = 4;
        public int clearBonus = 50;
        public WaveEntry[] entries;
    }
}
