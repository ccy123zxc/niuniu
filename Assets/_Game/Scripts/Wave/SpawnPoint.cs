using UnityEngine;

namespace OldAlley.Wave
{
    public enum SpawnKind
    {
        Left,
        Right,
        Back
    }

    /// <summary>敌人出生口。Stage 5 的 WaveManager 从这里取位置。</summary>
    public class SpawnPoint : MonoBehaviour
    {
        public SpawnKind kind = SpawnKind.Left;
    }
}
