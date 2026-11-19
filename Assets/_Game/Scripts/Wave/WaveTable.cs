using UnityEngine;

namespace OldAlley.Wave
{
    [CreateAssetMenu(menuName = "旧巷守卫/WaveTable", fileName = "WaveTable")]
    public class WaveTable : ScriptableObject
    {
        public WaveConfig[] waves;
    }
}
