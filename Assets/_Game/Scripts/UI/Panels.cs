using System.Collections.Generic;
using OldAlley.Core;
using OldAlley.Pickup;
using UnityEngine;
using UnityEngine.UI;

namespace OldAlley.UI
{
    /// <summary>面板基类：持有根节点，Show/Hide 只切激活状态。</summary>
    public abstract class UIPanel
    {
        public GameObject Root { get; protected set; }
        public bool IsOpen { get { return Root != null && Root.activeSelf; } }

        public abstract void Build(UIManager mgr, RectTransform parent);

        public virtual void Show() { if (Root != null) Root.SetActive(true); }
        public virtual void Hide() { if (Root != null) Root.SetActive(false); }

        protected RectTransform CreateRoot(RectTransform parent, string name, bool dimBackground)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);

            var rt = (RectTransform)go.transform;
            UIFactory.Stretch(rt);
            Root = go;

            if (dimBackground)
            {
                var dim = UIFactory.MakeImage(rt, "Dim", new Color(0f, 0f, 0f, 0.45f), true);
                UIFactory.Stretch((RectTransform)dim.transform);
            }

            return rt;
        }

        protected static Text Heading(Transform parent, string content)
        {
            var t = UIFactory.Label(parent, content, UIFactory.T.fontHeading, UIFactory.T.text, TextAnchor.MiddleCenter);
            UIFactory.Place((RectTransform)t.transform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                new Vector2(0f, -40f), new Vector2(800f, 70f));
            return t;
        }

        protected static void Center(RectTransform rt, Vector2 size)
        {
            UIFactory.Place(rt, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, size);
        }
    }

    // ---------------- 主菜单 ----------------

    public class MainMenuPanel : UIPanel
    {
        public override void Build(UIManager mgr, RectTransform parent)
        {
            var root = CreateRoot(parent, "MainMenu", false);

            var bg = UIFactory.MakeImage(root, "BG", UIFactory.T.skyGray, true);
            UIFactory.Stretch((RectTransform)bg.transform);

            var title = UIFactory.Label(root, UIText.Title, UIFactory.T.fontTitle, UIFactory.T.text);
            UIFactory.Place((RectTransform)title.transform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                new Vector2(0f, -120f), new Vector2(1200f, 130f));

            var sub = UIFactory.Label(root, UIText.Subtitle, UIFactory.T.fontBody, UIFactory.T.text);
            UIFactory.Place((RectTransform)sub.transform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                new Vector2(0f, -245f), new Vector2(1200f, 60f));

            var stack = UIFactory.VStack(root, "Buttons", 18f, 0);
            UIFactory.Place(stack, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0f, -40f), new Vector2(360f, 440f));

            UIFactory.MakeButton(stack, UIText.BtnStart, new Vector2(360f, 80f), mgr.StartGame);
            UIFactory.MakeButton(stack, UIText.BtnHowTo, new Vector2(360f, 80f), mgr.ShowHowTo);
            UIFactory.MakeButton(stack, UIText.BtnSettings, new Vector2(360f, 80f), mgr.ShowSettings);
            UIFactory.MakeButton(stack, UIText.BtnQuit, new Vector2(360f, 80f), mgr.QuitGame);

            var ver = UIFactory.Label(root, UIText.Version, UIFactory.T.fontSmall, UIFactory.T.text, TextAnchor.LowerRight);
            UIFactory.Place((RectTransform)ver.transform, new Vector2(1f, 0f), new Vector2(1f, 0f),
                new Vector2(-30f, 24f), new Vector2(200f, 40f));
        }
    }

    // ---------------- 玩法说明 ----------------

    public class HowToPlayPanel : UIPanel
    {
        public override void Build(UIManager mgr, RectTransform parent)
        {
            var root = CreateRoot(parent, "HowToPlay", true);

            var card = UIFactory.Panel(root, "Card", new Vector2(1080f, 760f));
            Center(card, new Vector2(1080f, 760f));

            Heading(card, UIText.HowToTitle);

            var body = UIFactory.Label(card, UIText.HowToBody, UIFactory.T.fontBody, UIFactory.T.text, TextAnchor.UpperLeft);
            UIFactory.Place((RectTransform)body.transform, new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(70f, -140f), new Vector2(940f, 430f));

            var back = UIFactory.MakeButton(card, UIText.BtnBack, new Vector2(240f, 72f), mgr.BackFromSub);
            UIFactory.Place((RectTransform)back.transform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
                new Vector2(0f, 50f), new Vector2(240f, 72f));
        }
    }

    // ---------------- 设置 ----------------

    public class SettingsPanel : UIPanel
    {
        public override void Build(UIManager mgr, RectTransform parent)
        {
            var root = CreateRoot(parent, "Settings", true);

            var card = UIFactory.Panel(root, "Card", new Vector2(900f, 780f));
            Center(card, new Vector2(900f, 780f));

            Heading(card, UIText.SettingsTitle);

            var l1 = UIFactory.Label(card, UIText.VolumeMaster, UIFactory.T.fontBody, UIFactory.T.text, TextAnchor.MiddleLeft);
            UIFactory.Place((RectTransform)l1.transform, new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(70f, -170f), new Vector2(260f, 60f));

            var s1 = UIFactory.MakeSlider(card, new Vector2(440f, 60f), 0f, 100f, GameSettings.MasterVolume * 100f, v =>
            {
                GameSettings.MasterVolume = v / 100f;
                GameSettings.Apply();
            });
            UIFactory.Place((RectTransform)s1.transform, new Vector2(1f, 1f), new Vector2(1f, 1f),
                new Vector2(-70f, -170f), new Vector2(440f, 60f));

            var l2 = UIFactory.Label(card, UIText.VolumeSfx, UIFactory.T.fontBody, UIFactory.T.text, TextAnchor.MiddleLeft);
            UIFactory.Place((RectTransform)l2.transform, new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(70f, -280f), new Vector2(260f, 60f));

            var s2 = UIFactory.MakeSlider(card, new Vector2(440f, 60f), 0f, 100f, GameSettings.SfxVolume * 100f, v =>
            {
                GameSettings.SfxVolume = v / 100f;
            });
            UIFactory.Place((RectTransform)s2.transform, new Vector2(1f, 1f), new Vector2(1f, 1f),
                new Vector2(-70f, -280f), new Vector2(440f, 60f));

            var tg = UIFactory.MakeToggle(card, UIText.Fullscreen, GameSettings.Fullscreen, v =>
            {
                GameSettings.Fullscreen = v;
                GameSettings.Apply();
            });
            UIFactory.Place((RectTransform)tg.transform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                new Vector2(0f, -400f), new Vector2(400f, 56f));

            var aimToggle = UIFactory.MakeToggle(card, UIText.AimAssist, GameSettings.AimAssist,
                v => GameSettings.AimAssist = v);
            UIFactory.Place((RectTransform)aimToggle.transform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                new Vector2(0f, -470f), new Vector2(400f, 56f));

            var back = UIFactory.MakeButton(card, UIText.BtnBack, new Vector2(240f, 72f), mgr.BackFromSub);
            UIFactory.Place((RectTransform)back.transform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
                new Vector2(0f, 50f), new Vector2(240f, 72f));
        }
    }

    // ---------------- 暂停 ----------------

    public class PausePanel : UIPanel
    {
        public override void Build(UIManager mgr, RectTransform parent)
        {
            var root = CreateRoot(parent, "Pause", true);

            var card = UIFactory.Panel(root, "Card", new Vector2(680f, 660f));
            Center(card, new Vector2(680f, 660f));

            Heading(card, UIText.PauseTitle);

            var stack = UIFactory.VStack(card, "Buttons", 14f, 0);
            UIFactory.Place(stack, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0f, -30f), new Vector2(360f, 380f));

            UIFactory.MakeButton(stack, UIText.BtnResume, new Vector2(360f, 76f), mgr.Resume);
            UIFactory.MakeButton(stack, UIText.BtnRestart, new Vector2(360f, 76f), mgr.RestartGame);
            UIFactory.MakeButton(stack, UIText.BtnSettings, new Vector2(360f, 76f), mgr.ShowSettings);
            UIFactory.MakeButton(stack, UIText.BtnMainMenu, new Vector2(360f, 76f), mgr.ReturnToMenu);
        }
    }

    // ---------------- 结算 ----------------

    public class ResultPanel : UIPanel
    {
        public Button AgainButton { get; private set; }
        public Button MenuButton { get; private set; }
        public CanvasGroup Group { get; private set; }

        private Text _title;
        private Text _data;

        public override void Build(UIManager mgr, RectTransform parent)
        {
            var root = CreateRoot(parent, "Result", true);
            Group = Root.AddComponent<CanvasGroup>();

            var card = UIFactory.Panel(root, "Card", new Vector2(900f, 700f));
            Center(card, new Vector2(900f, 700f));

            _title = UIFactory.Label(card, UIText.ResultWin, UIFactory.T.fontHeading, UIFactory.T.successGreen);
            UIFactory.Place((RectTransform)_title.transform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                new Vector2(0f, -60f), new Vector2(760f, 70f));

            _data = UIFactory.Label(card, string.Empty, UIFactory.T.fontBody, UIFactory.T.text);
            UIFactory.Place((RectTransform)_data.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0f, 30f), new Vector2(700f, 220f));

            AgainButton = UIFactory.MakeButton(card, UIText.BtnAgain, new Vector2(240f, 72f), mgr.RestartGame);
            UIFactory.Place((RectTransform)AgainButton.transform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
                new Vector2(-130f, 60f), new Vector2(240f, 72f));

            MenuButton = UIFactory.MakeButton(card, UIText.BtnMainMenu, new Vector2(240f, 72f), mgr.ReturnToMenu);
            UIFactory.Place((RectTransform)MenuButton.transform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
                new Vector2(130f, 60f), new Vector2(240f, 72f));
        }

        public void Populate(bool win, int wave, int score, float time)
        {
            if (_title != null)
            {
                _title.text = win ? UIText.ResultWin : UIText.ResultLose;
                _title.color = win ? UIFactory.T.successGreen : UIFactory.T.accentRed;
            }

            if (_data != null)
                _data.text = string.Format("坚持波次：第 {0} 波\n得分：{1}\n用时：{2:0.0} 秒", wave, score, time);
        }
    }

    // ---------------- 战斗 HUD ----------------

    public class HudPanel : UIPanel
    {
        private class BuffSlot
        {
            public GameObject root;
            public Image icon;
            public RectTransform fill;
        }

        private readonly List<BuffSlot> _buffSlots = new List<BuffSlot>();

        public RectTransform HpRoot { get; private set; }

        private Text _hpText;
        private Text _waveText;
        private Text _scoreText;
        private Text _brickText;
        private Text _hintText;
        private SlideInText _hintSlide;
        private UIProgressBar _hpBar;

        public override void Build(UIManager mgr, RectTransform parent)
        {
            var root = CreateRoot(parent, "HUD", false);

            var hp = UIFactory.Panel(root, "HpPanel", new Vector2(340f, 100f));
            UIFactory.Place(hp, new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(24f, -24f), new Vector2(340f, 100f));
            HpRoot = hp;

            _hpText = UIFactory.Label(hp, string.Format(UIText.HudHouseFormat, 100, 100), 26,
                UIFactory.T.text, TextAnchor.MiddleCenter);
            UIFactory.Place((RectTransform)_hpText.transform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                new Vector2(0f, -10f), new Vector2(310f, 32f));

            _hpBar = UIFactory.MakeBar(hp, "HpBar", new Vector2(310f, 26f), UIFactory.T.accentRed);
            UIFactory.Place(_hpBar.Root, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
                new Vector2(0f, 12f), new Vector2(310f, 26f));
            _hpBar.Set(1f);

            var brickPanel = UIFactory.Panel(root, "BrickPanel", new Vector2(330f, 46f));
            UIFactory.Place(brickPanel, new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(24f, -134f), new Vector2(330f, 46f));

            _brickText = UIFactory.Label(brickPanel, string.Format(UIText.HudBrickFormat, 0, 5, 0), 22,
                UIFactory.T.text, TextAnchor.MiddleCenter);
            UIFactory.Place((RectTransform)_brickText.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                Vector2.zero, new Vector2(310f, 34f));

            _waveText = Shadowed(UIFactory.Label(root, string.Format(UIText.HudWaveFormat, 1, 5),
                UIFactory.T.fontHeading, UIFactory.T.paper));
            UIFactory.Place((RectTransform)_waveText.transform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                new Vector2(0f, -30f), new Vector2(600f, 60f));

            _scoreText = Shadowed(UIFactory.Label(root, "0000", UIFactory.T.fontBody, UIFactory.T.paper, TextAnchor.MiddleRight));
            UIFactory.Place((RectTransform)_scoreText.transform, new Vector2(1f, 1f), new Vector2(1f, 1f),
                new Vector2(-24f, -30f), new Vector2(260f, 50f));

            var pauseBtn = UIFactory.MakeButton(root, UIText.BtnPause, new Vector2(72f, 72f), mgr.TogglePause, 24);
            UIFactory.Place((RectTransform)pauseBtn.transform, new Vector2(1f, 1f), new Vector2(1f, 1f),
                new Vector2(-24f, -92f), new Vector2(72f, 72f));

            _hintText = Shadowed(UIFactory.Label(root, UIText.HudHintStart, UIFactory.T.fontSmall, UIFactory.T.paper));
            UIFactory.Place((RectTransform)_hintText.transform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
                new Vector2(0f, 90f), new Vector2(1000f, 44f));
            _hintSlide = _hintText.gameObject.AddComponent<SlideInText>();

            var buff = UIFactory.Panel(root, "BuffArea", new Vector2(360f, 80f));
            UIFactory.Place(buff, new Vector2(0f, 0f), new Vector2(0f, 0f),
                new Vector2(24f, 24f), new Vector2(360f, 80f));

            for (var i = 0; i < 3; i++)
            {
                var slotGo = new GameObject("BuffSlot" + i, typeof(RectTransform));
                slotGo.transform.SetParent(buff, false);

                var slotRt = (RectTransform)slotGo.transform;
                UIFactory.Place(slotRt, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f),
                    new Vector2(12f + i * 112f, 0f), new Vector2(96f, 64f));

                var icon = UIFactory.MakeImage(slotRt, "Icon", Color.white, false);
                UIFactory.Place((RectTransform)icon.transform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                    new Vector2(0f, -2f), new Vector2(44f, 44f));

                var barBg = UIFactory.MakeImage(slotRt, "BarBg", UIFactory.T.outline, false);
                UIFactory.Place((RectTransform)barBg.transform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
                    new Vector2(0f, 4f), new Vector2(80f, 8f));

                var fill = UIFactory.MakeImage(barBg.transform, "Fill", UIFactory.T.successGreen, false);
                var fillRt = (RectTransform)fill.transform;
                fillRt.anchorMin = Vector2.zero;
                fillRt.anchorMax = Vector2.one;
                fillRt.offsetMin = new Vector2(1f, 1f);
                fillRt.offsetMax = new Vector2(-1f, -1f);

                slotGo.SetActive(false);
                _buffSlots.Add(new BuffSlot { root = slotGo, icon = icon, fill = fillRt });
            }

            var debug = UIFactory.MakeButton(root, UIText.BtnDebugResult, new Vector2(200f, 64f),
                () => mgr.ShowResult(false), 24);
            UIFactory.Place((RectTransform)debug.transform, new Vector2(1f, 0f), new Vector2(1f, 0f),
                new Vector2(-24f, 24f), new Vector2(200f, 64f));
        }

        public void SetHp(int current, int max)
        {
            if (_hpText != null) _hpText.text = string.Format(UIText.HudHouseFormat, current, max);
            if (_hpBar != null) _hpBar.Set(max <= 0 ? 0f : current / (float)max);
        }

        public void SetBricks(int current, int max, int repairBudget)
        {
            if (_brickText != null)
                _brickText.text = string.Format(UIText.HudBrickFormat, current, max, repairBudget);
        }

        public void SetWave(int current, int total)
        {
            if (_waveText != null) _waveText.text = string.Format(UIText.HudWaveFormat, current, total);
        }

        public void SetScore(int score)
        {
            if (_scoreText != null) _scoreText.text = score.ToString(UIText.HudScoreFormat);
        }

        public void SetHint(string hint)
        {
            if (_hintSlide != null) _hintSlide.Show(hint);
            else if (_hintText != null) _hintText.text = hint;
        }

        /// <summary>刷新左下角 BUFF 图标与倒计时条。</summary>
        public void SetBuffs(IReadOnlyList<BuffSystem.BuffInstance> buffs)
        {
            for (var i = 0; i < _buffSlots.Count; i++)
            {
                var slot = _buffSlots[i];

                if (buffs == null || i >= buffs.Count)
                {
                    if (slot.root.activeSelf) slot.root.SetActive(false);
                    continue;
                }

                if (!slot.root.activeSelf) slot.root.SetActive(true);

                var buff = buffs[i];
                if (slot.icon.sprite != buff.config.icon) slot.icon.sprite = buff.config.icon;

                var t = buff.total > 0f ? Mathf.Clamp01(buff.remaining / buff.total) : 0f;
                slot.fill.anchorMax = new Vector2(t, 1f);
            }
        }

        private static Text Shadowed(Text text)
        {
            var shadow = text.gameObject.AddComponent<Shadow>();
            shadow.effectColor = new Color(0f, 0f, 0f, 0.6f);
            shadow.effectDistance = new Vector2(2f, -2f);
            return text;
        }
    }
}
