using System.Collections;
using OldAlley.UI;
using UnityEngine;

namespace OldAlley.Core
{
    /// <summary>
    /// 顿帧：把 timeScale 短暂降到 0.05，用 unscaled 时间恢复。
    /// 暂停/结算期间不触发，避免把 timeScale=0 的界面「顶开」。
    /// </summary>
    public class HitStop : MonoBehaviour
    {
        public static HitStop Instance { get; private set; }

        private Coroutine _routine;

        private void Awake()
        {
            Instance = this;
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        public static void Play(float duration)
        {
            if (Instance == null) return;
            if (UIManager.Instance != null &&
                (UIManager.Instance.IsPaused || UIManager.Instance.IsResultOpen)) return;
            if (Time.timeScale <= 0f) return;

            if (Instance._routine != null) Instance.StopCoroutine(Instance._routine);
            Instance._routine = Instance.StartCoroutine(Instance.Run(duration));
        }

        private IEnumerator Run(float duration)
        {
            Time.timeScale = 0.05f;
            yield return new WaitForSecondsRealtime(duration);

            if (Time.timeScale > 0f) Time.timeScale = 1f;
            _routine = null;
        }
    }
}
