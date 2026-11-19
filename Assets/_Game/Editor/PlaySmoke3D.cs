using OldAlley.DebugTools;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace OldAlley.EditorTools
{
    /// <summary>临时自检入口：打开 Game3D 并进入 Play。</summary>
    public static class PlaySmoke3D
    {
        [MenuItem("旧巷守卫/工具/3D-运行自检")]
        public static void Run()
        {
            EditorSceneManager.OpenScene("Assets/_Game/Scenes/Game3D.unity", OpenSceneMode.Single);

            var go = new GameObject("SmokeRunner");
            go.AddComponent<SmokeRunner3D>();

            EditorApplication.isPlaying = true;
            Debug.Log("[SMOKE] 已进入 Play");
        }
    }
}
