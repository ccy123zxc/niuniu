using OldAlley.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

namespace OldAlley.EditorTools
{
    /// <summary>
    /// Stage 1：为主菜单与 Game 场景放好相机、EventSystem、UIManager。
    /// 菜单可重复执行，不会重复添加组件。
    /// </summary>
    public static class Stage1_UIBuilder
    {
        private const string ThemePath = "Assets/_Game/Data/UITheme.asset";
        private const string MainMenuScene = "Assets/_Game/Scenes/MainMenu.unity";
        private const string GameScene = "Assets/_Game/Scenes/Game.unity";

        [MenuItem("旧巷守卫/1-构建 UI 场景")]
        public static void Run()
        {
            var theme = EnsureTheme();
            var temp = PrepareUnsavedScene();

            try
            {
                BuildScene(MainMenuScene, theme);
                BuildScene(GameScene, theme);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                Debug.Log("[Stage1] UI 场景构建完成");
            }
            finally
            {
                if (!string.IsNullOrEmpty(temp)) AssetDatabase.DeleteAsset(temp);
            }
        }

        private static UITheme EnsureTheme()
        {
            var theme = AssetDatabase.LoadAssetAtPath<UITheme>(ThemePath);
            if (theme == null)
            {
                theme = ScriptableObject.CreateInstance<UITheme>();
                AssetDatabase.CreateAsset(theme, ThemePath);
                Debug.Log("[Stage1] 新建 UITheme.asset");
                return theme;
            }

            // WHY: 旧版本用 ColorUtility 做字段初始化，会让资产变成全透明，这里自愈。
            if (theme.paper.a <= 0.01f)
            {
                theme.ResetToDefaults();
                EditorUtility.SetDirty(theme);
                Debug.LogWarning("[Stage1] UITheme 配色为透明，已重置为默认 8 色。");
            }

            return theme;
        }

        /// <summary>批处理启动时可能是一个未命名场景，先另存到临时资产，避免 OpenScene 报错。</summary>
        private static string PrepareUnsavedScene()
        {
            var active = SceneManager.GetActiveScene();
            if (!string.IsNullOrEmpty(active.path)) return null;

            const string tempPath = "Assets/_Game/Scenes/_Stage1Temp.unity";
            EditorSceneManager.SaveScene(active, tempPath, false);
            Debug.Log("[Stage1] 未命名场景暂存为 " + tempPath);
            return tempPath;
        }

        private static void BuildScene(string path, UITheme theme)
        {
            var scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);

            EnsureCamera(theme);
            EnsureEventSystem();
            EnsureUIManager(theme);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log("[Stage1] 场景已更新: " + path);
        }

        private static void EnsureCamera(UITheme theme)
        {
            var cam = Camera.main;
            if (cam == null)
            {
                var go = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener));
                go.tag = "MainCamera";
                cam = go.GetComponent<Camera>();
                cam.transform.position = new Vector3(0f, 0f, -10f);
            }

            cam.orthographic = true;
            cam.orthographicSize = 5.625f;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = theme.skyGray;
            cam.nearClipPlane = 0.1f;
            cam.farClipPlane = 100f;
        }

        private static void EnsureEventSystem()
        {
            if (Object.FindObjectOfType<EventSystem>() != null) return;
            new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
        }

        private static void EnsureUIManager(UITheme theme)
        {
            var mgr = Object.FindObjectOfType<UIManager>();
            if (mgr == null)
            {
                var go = new GameObject("[UI]");
                mgr = go.AddComponent<UIManager>();
            }

            mgr.theme = theme;
            EditorUtility.SetDirty(mgr);
        }
    }
}
