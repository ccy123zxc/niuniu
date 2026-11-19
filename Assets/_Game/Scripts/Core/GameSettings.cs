using UnityEngine;

namespace OldAlley.Core
{
    /// <summary>
    /// 运行期设置（Stage 9 再接 PlayerPrefs 持久化）。
    /// </summary>
    public static class GameSettings
    {
        public static float MasterVolume = 1f;
        public static float SfxVolume = 1f;
        public static bool Fullscreen = true;
        public static bool AimAssist = true;

        public static void Apply()
        {
            AudioListener.volume = Mathf.Clamp01(MasterVolume);
            Screen.fullScreen = Fullscreen;
        }
    }
}
