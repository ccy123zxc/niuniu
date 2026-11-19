using UnityEngine;

namespace OldAlley.UI
{
    /// <summary>
    /// UI 调色板与字号档。唯一来源，禁止在面板里写死颜色/字号。
    /// WHY: 颜色用 Color32 字面量而不是 ColorUtility——后者禁止在 ScriptableObject
    /// 字段初始化器里调用，否则整个资产会变成全透明。
    /// </summary>
    [CreateAssetMenu(menuName = "旧巷守卫/UITheme", fileName = "UITheme")]
    public class UITheme : ScriptableObject
    {
        [Header("调色板")]
        public Color paper = new Color32(242, 232, 207, 255);        // #F2E8CF 宣纸底
        public Color wood = new Color32(140, 74, 50, 255);           // #8C4A32 木牌
        public Color outline = new Color32(58, 36, 24, 255);         // #3A2418 深棕描边
        public Color text = new Color32(46, 33, 24, 255);            // #2E2118 正文
        public Color accentRed = new Color32(201, 79, 61, 255);      // #C94F3D 强调红
        public Color successGreen = new Color32(110, 139, 74, 255);  // #6E8B4A 成功绿
        public Color skyGray = new Color32(143, 163, 173, 255);      // #8FA3AD 天空灰蓝
        public Color shadow = new Color(0f, 0f, 0f, 0.2f);

        [Header("字号（1920×1080 基准）")]
        public int fontTitle = 72;
        public int fontHeading = 48;
        public int fontBody = 32;
        public int fontSmall = 24;

        [Header("面板")]
        public int panelBorder = 2;   // 像素描边
        public int panelPadding = 16; // 内边距

        /// <summary>把配色恢复为默认 8 色（用于修复历史损坏的资产）。</summary>
        public void ResetToDefaults()
        {
            paper = new Color32(242, 232, 207, 255);
            wood = new Color32(140, 74, 50, 255);
            outline = new Color32(58, 36, 24, 255);
            text = new Color32(46, 33, 24, 255);
            accentRed = new Color32(201, 79, 61, 255);
            successGreen = new Color32(110, 139, 74, 255);
            skyGray = new Color32(143, 163, 173, 255);
            shadow = new Color(0f, 0f, 0f, 0.2f);
        }

        public static Color Hex(string hex)
        {
            Color c;
            if (ColorUtility.TryParseHtmlString("#" + hex, out c)) return c;
            return Color.magenta;
        }
    }
}
