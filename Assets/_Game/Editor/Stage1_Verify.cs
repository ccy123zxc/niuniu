using System.IO;
using System.Text;
using OldAlley.UI;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;

namespace OldAlley.EditorTools
{
    /// <summary>
    /// Stage 1 自检：在编辑模式下真实构建全部面板，验证 UIFactory / 各 Panel 不抛异常，
    /// 并输出层级树。不进入 Play 模式，可在 batchmode 下运行。
    /// </summary>
    public static class Stage1_Verify
    {
        [MenuItem("旧巷守卫/工具/Stage1-自检")]
        public static void Run()
        {
            var theme = AssetDatabase.LoadAssetAtPath<UITheme>("Assets/_Game/Data/UITheme.asset");
            if (theme == null)
            {
                Debug.LogError("[Stage1Verify] 缺少 UITheme.asset，请先执行「1-构建 UI 场景」。");
                return;
            }

            UIFactory.Theme = theme;
            UIFactory.EnsureEventSystem();

            var canvas = UIFactory.CreateCanvas("VerifyCanvas");
            var holder = new GameObject("VerifyMgr");
            var mgr = holder.AddComponent<UIManager>();
            var root = (RectTransform)canvas.transform;

            var sb = new StringBuilder();
            sb.AppendLine("# Stage1 UI 运行时自检");
            sb.AppendLine();
            sb.AppendLine("在编辑模式下真实构建全部面板（不进入 Play）。");
            sb.AppendLine();

            UIPanel[] panels =
            {
                new MainMenuPanel(),
                new HowToPlayPanel(),
                new SettingsPanel(),
                new PausePanel(),
                new ResultPanel(),
                new HudPanel()
            };

            foreach (var panel in panels)
            {
                panel.Build(mgr, root);
                var nodes = Count(panel.Root);
                sb.AppendLine(string.Format("- {0}：构建成功，节点数 {1}", panel.GetType().Name, nodes));
                Debug.Log(string.Format("[Stage1Verify] {0} OK, nodes={1}", panel.GetType().Name, nodes));
            }

            var result = panels[4] as ResultPanel;
            if (result != null) result.Populate(true, 5, 1230, 312.4f);

            var hud = panels[5] as HudPanel;
            if (hud != null)
            {
                hud.SetHp(72, 100);
                hud.SetWave(3, 5);
                hud.SetScore(480);
            }

            sb.AppendLine();
            sb.AppendLine("## 数据回填");
            sb.AppendLine();
            sb.AppendLine("- ResultPanel：胜利 / 第 5 波 / 1230 分 / 312.4 秒");
            sb.AppendLine("- HudPanel：老屋 72/100 · 第 3/5 波 · 480 分");
            sb.AppendLine();
            sb.AppendLine("## 层级树（Canvas）");
            sb.AppendLine();
            sb.AppendLine("```text");
            Dump(canvas.gameObject, sb, 0);
            sb.AppendLine("```");

            var path = Path.Combine(Path.GetDirectoryName(Application.dataPath), "REPORT_Stage1_UI.txt");
            File.WriteAllText(path, sb.ToString(), new UTF8Encoding(true));

            Object.DestroyImmediate(holder);
            Object.DestroyImmediate(canvas.gameObject);

            var systems = Object.FindObjectsOfType<EventSystem>();
            foreach (var es in systems) Object.DestroyImmediate(es.gameObject);

            Debug.Log("[Stage1Verify] 全部面板构建成功 -> " + path);
        }

        private static int Count(GameObject go)
        {
            if (go == null) return 0;
            var total = 1;
            for (var i = 0; i < go.transform.childCount; i++)
                total += Count(go.transform.GetChild(i).gameObject);
            return total;
        }

        private static void Dump(GameObject go, StringBuilder sb, int depth)
        {
            sb.AppendLine(new string(' ', depth * 2) + go.name);
            for (var i = 0; i < go.transform.childCount; i++)
                Dump(go.transform.GetChild(i).gameObject, sb, depth + 1);
        }
    }
}
