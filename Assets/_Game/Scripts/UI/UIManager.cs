using System.Collections;
using System.Collections.Generic;
using OldAlley.Core;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace OldAlley.UI
{
    /// <summary>
    /// 界面总管：按当前场景构建对应面板，提供面板栈与流程入口。
    /// 场景里只需要一个挂着本组件的空物体，其余 UI 全部由 UIFactory 在运行时生成。
    /// </summary>
    [DisallowMultipleComponent]
    public class UIManager : MonoBehaviour
    {
        public static UIManager Instance { get; private set; }

        public UITheme theme;

        public MainMenuPanel MainMenu { get; private set; }
        public HowToPlayPanel HowTo { get; private set; }
        public SettingsPanel Settings { get; private set; }
        public PausePanel Pause { get; private set; }
        public ResultPanel Result { get; private set; }
        public HudPanel Hud { get; private set; }

        private Canvas _canvas;
        private string _sceneName;
        private bool _paused;
        private Image _vignette;
        private readonly List<UIPanel> _stack = new List<UIPanel>();

        public bool IsPaused { get { return _paused; } }
        public bool IsResultOpen { get { return Result != null && Result.IsOpen; } }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;

            UIFactory.Theme = theme;
            GameSettings.Apply();

            UIFactory.EnsureEventSystem();
            _canvas = UIFactory.CreateCanvas("UI_Canvas");

            _sceneName = SceneManager.GetActiveScene().name;
            BuildForScene();
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        private void BuildForScene()
        {
            var root = (RectTransform)_canvas.transform;

            if (_sceneName == GameFlow.SceneGame)
            {
                Hud = new HudPanel();
                Hud.Build(this, root);

                Pause = new PausePanel();
                Pause.Build(this, root);
                Pause.Hide();

                Result = new ResultPanel();
                Result.Build(this, root);
                Result.Hide();

                Settings = new SettingsPanel();
                Settings.Build(this, root);
                Settings.Hide();
            }
            else
            {
                MainMenu = new MainMenuPanel();
                MainMenu.Build(this, root);

                HowTo = new HowToPlayPanel();
                HowTo.Build(this, root);
                HowTo.Hide();

                Settings = new SettingsPanel();
                Settings.Build(this, root);
                Settings.Hide();
            }

            var vignette = UIFactory.MakeImage(root, "DamageVignette", new Color(0.8f, 0.1f, 0.1f, 0f), false);
            UIFactory.Stretch((RectTransform)vignette.transform);
            _vignette = vignette;
        }

        private void Update()
        {
            if (!Input.GetKeyDown(KeyCode.Escape)) return;

            if (_sceneName == GameFlow.SceneGame)
            {
                if (Result != null && Result.IsOpen) return;
                if (Settings != null && Settings.IsOpen)
                {
                    Settings.Hide();
                    return;
                }

                TogglePause();
            }
            else
            {
                if (Settings != null && Settings.IsOpen)
                {
                    Settings.Hide();
                    return;
                }

                if (HowTo != null && HowTo.IsOpen) HowTo.Hide();
            }
        }

        // ---------- 面板栈 ----------

        public void Push(UIPanel panel)
        {
            if (panel == null) return;
            panel.Show();
            if (!_stack.Contains(panel)) _stack.Add(panel);
        }

        public void Pop(UIPanel panel)
        {
            if (panel == null) return;
            panel.Hide();
            _stack.Remove(panel);
        }

        public void HideAll()
        {
            for (var i = 0; i < _stack.Count; i++) _stack[i].Hide();
            _stack.Clear();
        }

        // ---------- 流程入口 ----------

        public void StartGame()
        {
            if (GameFlow.Instance != null) GameFlow.Instance.StartGame();
        }

        public void ShowHowTo()
        {
            if (HowTo != null) HowTo.Show();
        }

        public void ShowSettings()
        {
            if (Settings != null) Settings.Show();
        }

        public void BackFromSub()
        {
            if (Settings != null) Settings.Hide();
            if (HowTo != null) HowTo.Hide();
        }

        public void QuitGame()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        public void TogglePause()
        {
            if (_sceneName != GameFlow.SceneGame) return;

            if (_paused) Resume();
            else
            {
                _paused = true;
                Time.timeScale = 0f;
                if (Pause != null) Pause.Show();
            }
        }

        public void Resume()
        {
            _paused = false;
            Time.timeScale = 1f;
            if (Pause != null) Pause.Hide();
        }

        public void RestartGame()
        {
            Time.timeScale = 1f;
            _paused = false;
            SceneManager.LoadScene(GameFlow.SceneGame);
        }

        public void ReturnToMenu()
        {
            Time.timeScale = 1f;
            _paused = false;
            if (GameFlow.Instance != null) GameFlow.Instance.ReturnToMainMenu();
            else SceneManager.LoadScene(GameFlow.SceneMainMenu);
        }

        /// <summary>在角色头顶飘一行字（世界坐标 → 屏幕坐标）。</summary>
        public void ShowFloatText(string text, Vector3 worldPos, Color color)
        {
            if (_canvas == null || Camera.main == null) return;

            var go = new GameObject("FloatText", typeof(RectTransform), typeof(Text));
            go.transform.SetParent(_canvas.transform, false);

            var rt = (RectTransform)go.transform;
            var screen = Camera.main.WorldToScreenPoint(worldPos);

            Vector2 local;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                (RectTransform)_canvas.transform, screen, null, out local);

            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = local;

            var label = go.GetComponent<Text>();
            label.font = UIFactory.GetFont();
            label.text = text;
            label.fontSize = 28;
            label.color = color;
            label.alignment = TextAnchor.MiddleCenter;
            label.raycastTarget = false;

            StartCoroutine(FloatRoutine(rt, label));
        }

        private IEnumerator FloatRoutine(RectTransform rt, Text label)
        {
            var elapsed = 0f;
            var start = rt.anchoredPosition;

            while (elapsed < 0.8f)
            {
                elapsed += Time.unscaledDeltaTime;
                rt.anchoredPosition = start + Vector2.up * (elapsed * 60f);

                var color = label.color;
                color.a = 1f - elapsed / 0.8f;
                label.color = color;

                yield return null;
            }

            Destroy(label.gameObject);
        }

        public void ShowResult(bool win)
        {
            ShowResult(win, 1, 0, 0f);
        }

        public void ShowResult(bool win, int wave, int score, float time)
        {
            Time.timeScale = 0f;
            if (Result == null) return;

            Result.Populate(win, wave, score, time);
            Result.Show();

            if (Result.Group != null)
            {
                Result.Group.alpha = 0f;
                StartCoroutine(FadeIn(Result.Group, 0.3f));
            }

            // 结算后 0.5s 内禁用按钮，防误触连点（用真实时间，因为 timeScale=0）。
            if (Result.AgainButton != null) Result.AgainButton.interactable = false;
            if (Result.MenuButton != null) Result.MenuButton.interactable = false;
            StartCoroutine(UnlockResultButtons());
        }

        private IEnumerator UnlockResultButtons()
        {
            yield return new WaitForSecondsRealtime(0.5f);

            if (Result == null) yield break;
            if (Result.AgainButton != null) Result.AgainButton.interactable = true;
            if (Result.MenuButton != null) Result.MenuButton.interactable = true;
        }

        private IEnumerator FadeIn(CanvasGroup group, float duration)
        {
            var elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                group.alpha = Mathf.Clamp01(elapsed / duration);
                yield return null;
            }

            group.alpha = 1f;
        }

        /// <summary>老屋受伤：红色边缘闪烁 + HUD 血条抖动。</summary>
        public void FlashDamage()
        {
            if (_vignette != null) StartCoroutine(VignetteRoutine());
            if (Hud != null && Hud.HpRoot != null) StartCoroutine(ShakeHudRoutine());
        }

        private IEnumerator VignetteRoutine()
        {
            const float duration = 0.15f;
            var elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                var color = _vignette.color;
                color.a = 0.2f * (1f - elapsed / duration);
                _vignette.color = color;
                yield return null;
            }

            var end = _vignette.color;
            end.a = 0f;
            _vignette.color = end;
        }

        private IEnumerator ShakeHudRoutine()
        {
            var rect = Hud.HpRoot;
            var basePosition = rect.anchoredPosition;
            const float duration = 0.15f;
            var elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                rect.anchoredPosition = basePosition + new Vector2(Random.Range(-6f, 6f), Random.Range(-4f, 4f));
                yield return null;
            }

            rect.anchoredPosition = basePosition;
        }
    }
}
