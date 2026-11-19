using System.Collections;
using System.Collections.Generic;
using OldAlley.Audio;
using OldAlley.Core;
using OldAlley.Enemy;
using OldAlley.UI;
using UnityEngine;

namespace OldAlley.Wave
{
    /// <summary>
    /// 五波闭环：Prepare（倒计时）→ Spawning（按间隔/同屏上限）→ WaitingClear → 下一波。
    /// 第 5 波清空 → 胜利；老屋血量归零 → 失败。
    /// </summary>
    public class WaveManager : MonoBehaviour
    {
        public static WaveManager Instance { get; private set; }

        public WaveTable table;
        public EnemyPool pool;
        public HouseController house;
        public Transform[] spawnPoints;
        public bool autoStart = true;

        public int Score { get; private set; }
        public int CurrentWave { get; private set; }
        public bool IsRunning { get; private set; }

        /// <summary>只有两波之间（准备/休整）才能补墙；开打后一律不行。</summary>
        public bool CanRepair
        {
            get { return _phase == Phase.Prepare || _phase == Phase.Idle; }
        }

        private enum Phase
        {
            Idle,
            Prepare,
            Spawning,
            WaitingClear,
            Victory,
            Defeat
        }

        private Phase _phase = Phase.Idle;
        private float _phaseTimer;
        private float _spawnTimer;
        private float _startTime;
        private bool _awaitingJump;
        private Coroutine _toast;
        private readonly Queue<EnemyStats> _pending = new Queue<EnemyStats>();
        private readonly List<Transform> _validSpawns = new List<Transform>();

        private void Awake()
        {
            Instance = this;

            _validSpawns.Clear();
            if (spawnPoints != null)
            {
                foreach (var point in spawnPoints)
                    if (point != null) _validSpawns.Add(point);
            }

            if (house != null)
            {
                house.OnHpChanged += OnHouseHpChanged;
                house.OnDestroyed += Defeat;
            }

            if (pool != null) pool.Build();
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;

            if (house != null)
            {
                house.OnHpChanged -= OnHouseHpChanged;
                house.OnDestroyed -= Defeat;
            }
        }

        private void Start()
        {
            if (autoStart) Begin();
        }

        public void Begin()
        {
            if (IsRunning) return;

            IsRunning = true;
            _startTime = Time.time;
            Score = 0;
            BeginWave(0);
        }

        private void BeginWave(int index, float prepOverride = -1f)
        {
            if (table == null || table.waves == null || index >= table.waves.Length)
            {
                Victory();
                return;
            }

            CurrentWave = index;
            var wave = table.waves[index];

            _pending.Clear();
            if (wave.entries != null)
            {
                foreach (var entry in wave.entries)
                    for (var i = 0; i < entry.count; i++)
                        _pending.Enqueue(entry.stats);
            }

            _phase = Phase.Prepare;
            _phaseTimer = prepOverride >= 0f ? prepOverride : wave.prepTime;
            _spawnTimer = 0f;

            UpdateHud();
            AudioManager.Play(SfxId.Wave);
            ShowToast("第 " + (index + 1) + " 波 · " + wave.themeName, 2.5f);
        }

        private void Update()
        {
            HandleDebugKeys();

            if (!IsRunning) return;
            if (table == null || table.waves == null || CurrentWave >= table.waves.Length) return;

            var wave = table.waves[CurrentWave];

            switch (_phase)
            {
                case Phase.Prepare:
                    _phaseTimer -= Time.deltaTime;
                    SetHint("第 " + (CurrentWave + 1) + " 波 " +
                            Mathf.CeilToInt(Mathf.Max(0f, _phaseTimer)) + " 秒后来袭");
                    if (_phaseTimer <= 0f)
                    {
                        _phase = Phase.Spawning;
                        _spawnTimer = 0f;
                    }
                    break;

                case Phase.Spawning:
                    _spawnTimer -= Time.deltaTime;
                    if (_spawnTimer <= 0f && _pending.Count > 0 && AliveCount() < wave.maxAlive)
                    {
                        SpawnOne();
                        _spawnTimer = wave.spawnInterval;
                    }
                    if (_pending.Count == 0) _phase = Phase.WaitingClear;
                    break;

                case Phase.WaitingClear:
                    if (AliveCount() == 0) ClearWave(wave);
                    break;
            }
        }

        private void SpawnOne()
        {
            if (_validSpawns.Count == 0 || pool == null) return;

            var stats = _pending.Dequeue();
            var point = _validSpawns[Random.Range(0, _validSpawns.Count)];
            var position = point.position + new Vector3(Random.Range(-0.6f, 0.6f), 0f, Random.Range(-0.6f, 0.6f));
            position.y = 0.5f;
            pool.Spawn(stats, position);
        }

        private int AliveCount()
        {
            if (pool == null) return 0;

            var items = pool.Items;
            var count = 0;
            for (var i = 0; i < items.Count; i++)
                if (items[i] != null && items[i].gameObject.activeSelf) count++;

            return count;
        }

        private void ClearWave(WaveConfig wave)
        {
            Score += wave.clearBonus;
            UpdateHud();

            // 先停在 Idle，让「撑过去了」的提示能看清，再进下一波。
            _phase = Phase.Idle;
            ShowToast("第 " + (CurrentWave + 1) + " 波 撑过去了！", 2f);

            var next = CurrentWave + 1;
            if (table.waves.Length <= next) Victory();
            else StartCoroutine(RestBeforeWave(next, 10f));
        }

        /// <summary>波间休整：约 10 秒，不刷怪，允许补墙。</summary>
        private IEnumerator RestBeforeWave(int index, float seconds)
        {
            var remaining = seconds;

            while (remaining > 0f && IsRunning)
            {
                SetHint("休整 · 第 " + (index + 1) + " 波 " +
                        Mathf.CeilToInt(remaining) + " 秒后来袭 · 可右键补墙");
                remaining -= Time.deltaTime;
                yield return null;
            }

            if (IsRunning) BeginWave(index, 0f);
        }

        public void AddKillScore(int points)
        {
            Score += points;
            UpdateHud();
        }

        private void OnHouseHpChanged(int hp, int max)
        {
            UpdateHud();
        }

        private void UpdateHud()
        {
            if (UIManager.Instance == null || UIManager.Instance.Hud == null) return;

            var hud = UIManager.Instance.Hud;
            hud.SetHp(house != null ? house.hp : 0, house != null ? house.maxHp : 100);
            hud.SetWave(CurrentWave + 1, table != null && table.waves != null ? table.waves.Length : 5);
            hud.SetScore(Score);
        }

        private void SetHint(string text)
        {
            if (UIManager.Instance != null && UIManager.Instance.Hud != null)
                UIManager.Instance.Hud.SetHint(text);
        }

        private void ShowToast(string text, float seconds)
        {
            SetHint(text);

            if (_toast != null) StopCoroutine(_toast);
            _toast = StartCoroutine(ClearHint(seconds));
        }

        private IEnumerator ClearHint(float seconds)
        {
            yield return new WaitForSeconds(seconds);
            SetHint("");
        }

        private void Victory()
        {
            if (_phase == Phase.Victory) return;

            _phase = Phase.Victory;
            IsRunning = false;

            if (house != null) Score += Mathf.Max(0, house.hp) * 5;
            UpdateHud();
            ShowResult(true);
        }

        private void Defeat()
        {
            if (_phase == Phase.Defeat || _phase == Phase.Victory) return;

            _phase = Phase.Defeat;
            IsRunning = false;
            ShowResult(false);
        }

        private void ShowResult(bool win)
        {
            if (UIManager.Instance == null) return;

            var total = table != null && table.waves != null ? table.waves.Length : 5;
            var wave = Mathf.Min(CurrentWave + 1, total);
            UIManager.Instance.ShowResult(win, wave, Score, Time.time - _startTime);
        }

        // ---------- 调试 ----------

        private void HandleDebugKeys()
        {
            if (Input.GetKeyDown(KeyCode.F4))
            {
                if (table != null && table.waves != null) CurrentWave = table.waves.Length - 1;
                Victory();
                return;
            }

            if (Input.GetKeyDown(KeyCode.F3))
            {
                _awaitingJump = true;
                return;
            }

            if (!_awaitingJump) return;

            for (var i = 1; i <= 5; i++)
            {
                if (!Input.GetKeyDown((KeyCode)((int)KeyCode.Alpha0 + i))) continue;

                _awaitingJump = false;
                if (table == null || table.waves == null || i > table.waves.Length) return;

                _phase = Phase.Idle;
                ClearActiveEnemies();
                BeginWave(i - 1);
                return;
            }
        }

        private void ClearActiveEnemies()
        {
            if (pool == null) return;

            var items = pool.Items;
            for (var i = 0; i < items.Count; i++)
                if (items[i] != null && items[i].gameObject.activeSelf) items[i].Recycle();
        }
    }
}
