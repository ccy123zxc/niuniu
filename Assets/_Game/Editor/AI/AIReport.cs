using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace OldAlley.EditorTools
{
    /// <summary>
    /// 扫描 Assets/_Game 并生成阶段报告，作为每阶段的交付证据。
    /// </summary>
    public static class AIReport
    {
        private const string Root = "Assets/_Game";

        private static readonly string[] KnownMenus =
        {
            "旧巷守卫/0-工程初始化",
            "旧巷守卫/工具/生成阶段报告"
        };

        [MenuItem("旧巷守卫/工具/生成阶段报告")]
        public static void GenerateStage0Report()
        {
            var projectRoot = Path.GetDirectoryName(Application.dataPath);
            if (string.IsNullOrEmpty(projectRoot))
            {
                Debug.LogError("[AIReport] 无法定位项目根目录。");
                return;
            }

            var files = Directory.Exists(Root)
                ? Directory.GetFiles(Root, "*", SearchOption.AllDirectories)
                    .Where(f => !f.EndsWith(".meta", StringComparison.OrdinalIgnoreCase))
                    .ToArray()
                : Array.Empty<string>();

            var scenes = Directory.Exists(Root + "/Scenes")
                ? Directory.GetFiles(Root + "/Scenes", "*.unity", SearchOption.TopDirectoryOnly)
                : Array.Empty<string>();

            var buildScenes = EditorBuildSettings.scenes
                .Select((s, i) => $"{i}. {(s.enabled ? "[x]" : "[ ]")} {s.path}")
                .ToArray();

            var sb = new StringBuilder();
            sb.AppendLine("# REPORT_Stage0 · 工程骨架");
            sb.AppendLine();
            sb.AppendLine($"> 生成时间：{DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            sb.AppendLine($"> Unity：{Application.unityVersion}");
            sb.AppendLine();
            sb.AppendLine("## 资产统计");
            sb.AppendLine();
            sb.AppendLine($"- Assets/_Game 文件数：**{files.Length}**");
            sb.AppendLine($"- 场景数：**{scenes.Length}**");
            sb.AppendLine();
            sb.AppendLine("## 场景清单");
            sb.AppendLine();
            foreach (var s in scenes)
                sb.AppendLine($"- `{s.Replace('\\', '/')}`");
            sb.AppendLine();
            sb.AppendLine("## Build Settings");
            sb.AppendLine();
            foreach (var s in buildScenes)
                sb.AppendLine($"- {s}");
            sb.AppendLine();
            sb.AppendLine("## 菜单清单");
            sb.AppendLine();
            foreach (var m in KnownMenus)
                sb.AppendLine($"- `{m}`");
            sb.AppendLine();
            sb.AppendLine("## 编译");
            sb.AppendLine();
            sb.AppendLine("- 结果：见 `Logs/stage0.log` 中 `error CS` / `warning CS` 统计（由批处理编译写入）。");
            sb.AppendLine();
            sb.AppendLine("## 目录树");
            sb.AppendLine();
            sb.AppendLine("```text");
            foreach (var dir in Directory.GetDirectories(Root, "*", SearchOption.AllDirectories)
                         .OrderBy(d => d, StringComparer.Ordinal))
            {
                var rel = dir.Replace('\\', '/');
                var depth = rel.Split('/').Length - 1;
                sb.AppendLine(new string(' ', depth * 2) + Path.GetFileName(rel) + "/");
            }
            sb.AppendLine("```");

            var outPath = Path.Combine(projectRoot, "REPORT_Stage0.md");
            File.WriteAllText(outPath, sb.ToString(), new UTF8Encoding(true));
            AssetDatabase.Refresh();

            Debug.Log("[AIReport] 已生成 " + outPath + $"（文件 {files.Length} / 场景 {scenes.Length}）");
        }
    }
}
