using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace OldAlley.Core
{
    public enum GameState
    {
        Boot,
        MainMenu,
        Playing,
        Paused,
        Result
    }

    /// <summary>
    /// 跨场景的游戏流程状态机（单例，DontDestroyOnLoad）。
    /// 只负责"我在哪个阶段"以及阶段之间的场景切换，不含玩法逻辑。
    /// </summary>
    [DisallowMultipleComponent]
    public class GameFlow : MonoBehaviour
    {
        public const string SceneBoot = "Boot";
        public const string SceneMainMenu = "MainMenu";
        public const string SceneGame = "Game3D";

        public static GameFlow Instance { get; private set; }

        public GameState State { get; private set; } = GameState.Boot;

        /// <summary>状态变化回调。订阅方必须在 OnDisable 退订。</summary>
        public event Action<GameState> OnStateChanged;

        // WHY: 场景里不需要手动摆 GameFlow —— 进入 Play 时自动建一个，保证无论从哪个场景启动都能工作。
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void AutoBootstrap()
        {
            EnsureInstance();
        }

        public static GameFlow EnsureInstance()
        {
            if (Instance != null) return Instance;
            var go = new GameObject("[GameFlow]");
            Instance = go.AddComponent<GameFlow>();
            DontDestroyOnLoad(go);
            return Instance;
        }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            // Stage 0：Boot 场景的唯一职责就是把流程推进到主菜单。
            if (State == GameState.Boot && SceneManager.GetActiveScene().name == SceneBoot)
                ChangeState(GameState.MainMenu);
        }

        /// <summary>切换流程状态；切到 MainMenu / Playing 时顺带加载对应场景。</summary>
        public void ChangeState(GameState next)
        {
            if (State == next) return;

            State = next;
            OnStateChanged?.Invoke(next);

            if (next == GameState.MainMenu)
                LoadIfNotActive(SceneMainMenu);
        }

        public void StartGame()
        {
            ChangeState(GameState.Playing);
            LoadIfNotActive(SceneGame);
        }

        public void ReturnToMainMenu()
        {
            Time.timeScale = 1f;
            ChangeState(GameState.MainMenu);
            LoadIfNotActive(SceneMainMenu);
        }

        private static void LoadIfNotActive(string sceneName)
        {
            if (!Application.CanStreamedLevelBeLoaded(sceneName))
            {
                Debug.LogWarning("[GameFlow] 场景未加入 Build Settings: " + sceneName);
                return;
            }

            if (SceneManager.GetActiveScene().name != sceneName)
                SceneManager.LoadScene(sceneName);
        }
    }
}
