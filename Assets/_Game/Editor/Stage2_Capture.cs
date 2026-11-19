using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace OldAlley.EditorTools
{
    /// <summary>Stage 2 截图工具：把 Game 场景的相机渲染到 PNG，便于无图形界面时验收。</summary>
    public static class Stage2_Capture
    {
        [MenuItem("旧巷守卫/工具/Stage2-截图")]
        public static void Run()
        {
            EditorSceneManager.OpenScene("Assets/_Game/Scenes/Game.unity", OpenSceneMode.Single);

            var cam = Camera.main;
            if (cam == null)
            {
                Debug.LogError("[Stage2Capture] 场景里没有 Main Camera");
                return;
            }

            const int width = 1280;
            const int height = 720;

            var rt = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32);
            var previous = cam.targetTexture;

            cam.targetTexture = rt;
            cam.Render();

            RenderTexture.active = rt;
            var tex = new Texture2D(width, height, TextureFormat.RGB24, false);
            tex.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            tex.Apply();
            RenderTexture.active = null;

            cam.targetTexture = previous;

            var path = Path.Combine(Path.GetDirectoryName(Application.dataPath), "stage2_alley.png");
            File.WriteAllBytes(path, tex.EncodeToPNG());

            Object.DestroyImmediate(tex);
            rt.Release();
            Object.DestroyImmediate(rt);

            Debug.Log("[Stage2Capture] 已输出 " + path);
        }
    }
}
