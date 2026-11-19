using System.Collections.Generic;
using System.IO;
using OldAlley.Core;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace OldAlley.EditorTools
{
    /// <summary>
    /// Stage 0：把空项目变成可编译、可重复初始化的工程骨架。
    /// 菜单可反复执行，结果一致（幂等）。
    /// </summary>
    public static class Stage0_ProjectSetup
    {
        private const string Root = "Assets/_Game";
        private const string SceneDir = Root + "/Scenes";
        private const string DataDir = Root + "/Data";
        private const string BootPath = SceneDir + "/Boot.unity";
        private const string MenuPath = SceneDir + "/MainMenu.unity";
        private const string GamePath = SceneDir + "/Game.unity";
        private const string ConfigPath = DataDir + "/GameConfig.asset";

        private static readonly string[] Folders =
        {
            Root + "/Art/Generated",
            Root + "/Art/Fonts",
            Root + "/Art/UI",
            Root + "/Audio/BGM",
            Root + "/Audio/SFX",
            Root + "/Data",
            Root + "/Prefabs/Player",
            Root + "/Prefabs/Enemy",
            Root + "/Prefabs/Projectile",
            Root + "/Prefabs/Pickup",
            Root + "/Prefabs/FX",
            Root + "/Prefabs/UI",
            Root + "/Scenes",
            Root + "/Scripts/Core",
            Root + "/Scripts/Player",
            Root + "/Scripts/Enemy",
            Root + "/Scripts/Wave",
            Root + "/Scripts/Pickup",
            Root + "/Scripts/UI",
            Root + "/Scripts/Camera",
            Root + "/Scripts/Audio",
            Root + "/Editor",
            Root + "/Editor/AI"
        };

        [MenuItem("旧巷守卫/0-工程初始化")]
        public static void Run()
        {
            Debug.Log("[Stage0] === 开始工程初始化 ===");

            CreateFolders();
            ApplyPlayerSettings();
            ApplyInputHandling();
            CreateScenes();
            ApplyBuildSettings();
            CreateGameConfig();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("[Stage0] === 工程初始化完成 ===");
        }

        // ---------- 1. 目录树 ----------

        private static void CreateFolders()
        {
            foreach (var folder in Folders)
                EnsureFolder(folder);

            AssetDatabase.Refresh();
            Debug.Log($"[Stage0] 目录检查完成，共 {Folders.Length} 个目录。");
        }

        private static void EnsureFolder(string assetPath)
        {
            if (AssetDatabase.IsValidFolder(assetPath)) return;

            var parts = assetPath.Split('/');
            var current = parts[0];
            for (var i = 1; i < parts.Length; i++)
            {
                var next = current + "/" + parts[i];
                if (!AssetDatabase.IsValidFolder(next))
                    AssetDatabase.CreateFolder(current, parts[i]);
                current = next;
            }

            // 空目录在版本管理里会消失，用 .gitkeep 占位。
            var gitkeep = Path.Combine(assetPath, ".gitkeep");
            if (!File.Exists(gitkeep))
                File.WriteAllText(gitkeep, string.Empty);
        }

        // ---------- 2. PlayerSettings ----------

        private static void ApplyPlayerSettings()
        {
            PlayerSettings.productName = "旧巷守卫";
            PlayerSettings.companyName = "OldAlley";
            PlayerSettings.defaultScreenWidth = 960;
            PlayerSettings.defaultScreenHeight = 540;
            PlayerSettings.fullScreenMode = FullScreenMode.Windowed;
            PlayerSettings.resizableWindow = true;

            Debug.Log("[Stage0] PlayerSettings: 旧巷守卫 / OldAlley / 960x540 / Windowed");
        }

        // ---------- 3. 输入处理 = Both ----------

        private static void ApplyInputHandling()
        {
            const string settingsPath = "ProjectSettings/ProjectSettings.asset";
            const string backupPath = settingsPath + ".stage0.bak";

            if (!File.Exists(settingsPath))
            {
                Debug.LogError("[Stage0] 找不到 ProjectSettings.asset，跳过输入设置。");
                return;
            }

            if (!File.Exists(backupPath))
            {
                File.Copy(settingsPath, backupPath, true);
                Debug.Log("[Stage0] 已备份 ProjectSettings.asset -> " + backupPath);
            }

            var assets = AssetDatabase.LoadAllAssetsAtPath(settingsPath);
            if (assets == null || assets.Length == 0)
            {
                Debug.LogError("[Stage0] 无法加载 ProjectSettings.asset。");
                return;
            }

            var so = new SerializedObject(assets[0]);
            var prop = so.FindProperty("activeInputHandler");
            if (prop == null)
            {
                Debug.LogWarning("[Stage0] 未找到 activeInputHandler 字段，请手动设为 Both。");
                return;
            }

            var before = prop.intValue;
            if (before != 2) // 0=Old, 1=New, 2=Both
            {
                prop.intValue = 2;
                so.ApplyModifiedProperties();
                AssetDatabase.SaveAssets();
                Debug.Log($"[Stage0] activeInputHandler: {before} -> {prop.intValue}（需要重启 Unity 生效）");
            }
            else
            {
                Debug.Log("[Stage0] activeInputHandler 已是 Both(2)，无需修改。");
            }
        }

        // ---------- 4. 三个空场景 ----------

        private static void CreateScenes()
        {
            EnsureScene(BootPath);
            EnsureScene(MenuPath);
            EnsureScene(GamePath);
        }

        private static void EnsureScene(string path)
        {
            if (File.Exists(path))
            {
                Debug.Log("[Stage0] 场景已存在，跳过: " + path);
                return;
            }

            // WHY: 批处理启动时是一个"未命名空场景"，此时 Additive 新建会抛
            // InvalidOperationException。直接把未命名场景另存为第一个目标场景即可。
            var active = SceneManager.GetActiveScene();
            if (string.IsNullOrEmpty(active.path))
            {
                EditorSceneManager.SaveScene(active, path, false);
                Debug.Log("[Stage0] 新建场景（另存未命名场景）: " + path);
                return;
            }

            // 其余场景用 Additive 新建，避免顶掉当前打开的场景。
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
            EditorSceneManager.SaveScene(scene, path, false);
            EditorSceneManager.CloseScene(scene, true);

            Debug.Log("[Stage0] 新建场景: " + path);
        }

        // ---------- 5. Build Settings ----------

        private static void ApplyBuildSettings()
        {
            var wanted = new[]
            {
                new EditorBuildSettingsScene(BootPath, true),
                new EditorBuildSettingsScene(MenuPath, true),
                new EditorBuildSettingsScene(GamePath, true)
            };

            var current = EditorBuildSettings.scenes;
            var same = current != null && current.Length == wanted.Length;
            if (same)
            {
                for (var i = 0; i < wanted.Length; i++)
                {
                    if (current[i].path != wanted[i].path || current[i].enabled != wanted[i].enabled)
                    {
                        same = false;
                        break;
                    }
                }
            }

            if (same)
            {
                Debug.Log("[Stage0] Build Settings 已是 Boot/MainMenu/Game，无需修改。");
                return;
            }

            EditorBuildSettings.scenes = wanted;
            Debug.Log("[Stage0] Build Settings 已写入 3 个场景（Boot = index 0）。");
        }

        // ---------- 6. GameConfig 资产 ----------

        private static void CreateGameConfig()
        {
            var config = AssetDatabase.LoadAssetAtPath<GameConfig>(ConfigPath);
            if (config != null)
            {
                Debug.Log("[Stage0] GameConfig.asset 已存在，跳过。");
                return;
            }

            config = ScriptableObject.CreateInstance<GameConfig>();
            AssetDatabase.CreateAsset(config, ConfigPath);
            Debug.Log("[Stage0] 新建 GameConfig.asset（houseMaxHp=100 / speed=4.8 / fire=0.32 / dmg=12 / bullet=14 / pickupMax=2）");
        }
    }
}
