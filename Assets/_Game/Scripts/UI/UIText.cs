namespace OldAlley.UI
{
    /// <summary>
    /// 所有界面文案集中在这里，方便以后改词。
    /// </summary>
    public static class UIText
    {
        public const string Title = "旧巷守卫";
        public const string Subtitle = "90 年代 · 童年巷战";
        public const string Version = "v0.1";

        public const string BtnStart = "开始游戏";
        public const string BtnHowTo = "玩法说明";
        public const string BtnSettings = "设置";
        public const string BtnQuit = "退出";
        public const string BtnBack = "返回";
        public const string BtnResume = "继续";
        public const string BtnRestart = "重新开始";
        public const string BtnMainMenu = "回主菜单";
        public const string BtnAgain = "再来一局";
        public const string BtnPause = "II";
        public const string BtnDebugResult = "调试:结算";

        public const string HowToTitle = "玩法说明";
        public const string HowToBody =
            "WASD / 方向键：移动\n" +
            "左键 / J：发射弹弓（按住持续转向）\n" +
            "右键：点地移动；点在老屋正门墙上则补砖\n" +
            "\n" +
            "野兽会一块块拆掉正门的砖墙。砖被打落会掉在地上，\n" +
            "走过去自动拾取，最多带 5 块，补砖时不能射击。\n" +
            "每打退一波后约有 10 秒休整期，只有这时才能补墙。\n" +
            "\n" +
            "拾取童年小物件可获得临时加成。\n" +
            "守住老屋，撑过 5 波！";

        public const string SettingsTitle = "设置";
        public const string VolumeMaster = "主音量";
        public const string VolumeSfx = "音效音量";
        public const string Fullscreen = "全屏";
        public const string AimAssist = "瞄准辅助";

        public const string PauseTitle = "暂停";
        public const string ResultWin = "老屋守住了！";
        public const string ResultLose = "老屋被拆了…";
        public const string BuffArea = "BUFF";

        public const string HudWaveFormat = "第 {0} / {1} 波";
        public const string HudHouseFormat = "老屋 {0}/{1}";
        public const string HudBrickFormat = "砖 {0}/{1} · 本轮可补 {2}";
        public const string HudScoreFormat = "0000";
        public const string HudHintStart = "WASD 移动 · 左键射击 · 右键点地/补墙";
    }
}
