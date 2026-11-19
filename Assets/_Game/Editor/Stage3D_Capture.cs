using System.IO;
using OldAlley.Enemy;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace OldAlley.EditorTools
{
    /// <summary>3D 场景截图：打开 Game3D，临时摆四只野物，渲染 PNG 供验收。</summary>
    public static class Stage3D_Capture
    {
        [MenuItem("旧巷守卫/工具/3D-截图")]
        public static void Run()
        {
            EditorSceneManager.OpenScene("Assets/_Game/Scenes/Game3D.unity", OpenSceneMode.Single);

            var cam = Camera.main;
            if (cam == null)
            {
                Debug.LogError("[Capture3D] 场景里没有 Main Camera");
                return;
            }

            var spawned = new[]
            {
                Preview("Enemy_Kid", new Vector3(-4f, 0.5f, -2f)),
                Preview("Enemy_Speeder", new Vector3(4f, 0.5f, -4f)),
                Preview("Enemy_Fatty", new Vector3(-7f, 0.5f, -7f)),
                Preview("Enemy_King", new Vector3(7f, 0.5f, -8f))
            };

            Capture(cam, "stage3d_voxel.png");
            CapturePlayer("player3d.png");

            foreach (var go in spawned)
                if (go != null) Object.DestroyImmediate(go);

            Debug.Log("[Capture3D] 完成");
        }

        private static GameObject Preview(string assetName, Vector3 position)
        {
            GameObject template = null;
            foreach (var found in Resources.FindObjectsOfTypeAll<EnemyBase>())
            {
                if (found == null || found.name != "EnemyTemplate") continue;
                template = found.gameObject;
                break;
            }

            if (template == null) return null;

            var stats = AssetDatabase.LoadAssetAtPath<EnemyStats>("Assets/_Game/Data/" + assetName + ".asset");
            if (stats == null) return null;

            var clone = Object.Instantiate(template);
            clone.name = "Preview_" + assetName;

            var enemy = clone.GetComponent<EnemyBase>();
            clone.SetActive(true);
            enemy.Spawn(stats, position);
            return clone;
        }

        private static void CapturePlayer(string fileName)
        {
            var player = GameObject.Find("Player");
            if (player == null) return;

            var go = new GameObject("PreviewCamera", typeof(Camera));
            go.transform.position = player.transform.position + new Vector3(4.2f, 4.6f, 4.2f);
            go.transform.LookAt(player.transform.position + Vector3.up * 0.55f);

            var cam = go.GetComponent<Camera>();
            cam.orthographic = true;
            cam.orthographicSize = 2.1f;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.9f, 0.87f, 0.78f);

            Capture(cam, fileName);
            Object.DestroyImmediate(go);
        }

        private static void Capture(Camera cam, string fileName)
        {
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

            var path = Path.Combine(Path.GetDirectoryName(Application.dataPath), fileName);
            File.WriteAllBytes(path, tex.EncodeToPNG());

            Object.DestroyImmediate(tex);
            rt.Release();
            Object.DestroyImmediate(rt);

            Debug.Log("[Capture3D] 已输出 " + path);
        }
    }
}
