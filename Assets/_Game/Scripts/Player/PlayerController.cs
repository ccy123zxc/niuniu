using System.Collections;
using OldAlley.Core;
using OldAlley.FX;
using OldAlley.Pickup;
using OldAlley.UI;
using OldAlley.Wave;
using UnityEngine;

namespace OldAlley.Player
{
    /// <summary>3D 版玩家：XZ 平面移动、鼠标地面瞄准、镜头方向朝鼠标。</summary>
    [RequireComponent(typeof(Rigidbody))]
    public class PlayerController : MonoBehaviour
    {
        public GameConfig config;
        public Transform model;
        public Slingshot slingshot;

        [Header("右键点地移动")]
        public bool clickToMove = true;

        [Header("补墙")]
        public int maxBricks = 5;
        public float repairSeconds = 0.4f;

        private static readonly Collider[] AimBuffer = new Collider[8];

        private IInputSource _input;
        private Rigidbody _rb;
        private Vector3 _move;
        private Vector3 _aim = Vector3.forward;
        private bool _showDebug;
        private float _slowUntil;
        private float _slowMultiplier = 1f;
        private int _enemyMask;
        private int _blockMask;
        private float _dustTimer;

        private bool _hasMoveTarget;
        private Vector3 _moveTarget;
        private float _bestDistance;
        private float _stuckTimer;
        private GameObject _moveMarker;

        private int _bricks;
        private WallController _wall;
        private int _repairBudget;
        private bool _wasRepairWindow;

        public int Bricks { get { return _bricks; } }
        public bool IsRepairing { get; private set; }

        /// <summary>自检/回放用的输入替身；正常游戏为 null。</summary>
        public IInputSource InputOverride { get; set; }

        private void Awake()
        {
            _rb = GetComponent<Rigidbody>();
            _rb.useGravity = false;
            _rb.constraints = RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePositionY;
            _rb.interpolation = RigidbodyInterpolation.Interpolate;
            _rb.collisionDetectionMode = CollisionDetectionMode.Continuous;

            _input = new LegacyInputSource(Camera.main);
            _enemyMask = LayerMask.GetMask("Enemy");
            _blockMask = LayerMask.GetMask("Wall", "Default");
        }

        private void Update()
        {
            var input = InputOverride != null ? InputOverride : _input;
            if (input == null) return;

            _move = input.Move;

            // 补墙期间站定，不能射击
            if (IsRepairing)
            {
                _move = Vector3.zero;
                if (slingshot != null) slingshot.Aim(_aim);
                return;
            }

            RefreshRepairBudget();
            HandleClickMove(input);

            var wantsFire = input.FireHeld || input.FirePressed;

            // 平时不跟着鼠标乱转；按下左键瞬间对准，按住期间持续跟随鼠标微调角度，松开后保持最后朝向。
            if (!input.UseFacingAim && wantsFire)
            {
                var aim = input.AimWorld(_rb.position);
                if (GameSettings.AimAssist) aim = ApplyAimAssist(aim);

                if (aim.sqrMagnitude > 0.0001f)
                {
                    _aim = aim;
                    if (model != null)
                    {
                        var target = Quaternion.LookRotation(_aim, Vector3.up);
                        model.rotation = input.FirePressed
                            ? target
                            : Quaternion.Slerp(model.rotation, target, 24f * Time.deltaTime);
                    }
                }
            }

            if (slingshot != null)
            {
                slingshot.Aim(_aim);
                if (wantsFire && slingshot.TryFire(_rb.position, _aim)) Recoil(-_aim);
            }

            if (_move.sqrMagnitude > 0.01f)
            {
                _dustTimer -= Time.deltaTime;
                if (_dustTimer <= 0f)
                {
                    _dustTimer = 0.25f;
                    Dust.Spawn(_rb.position + Vector3.down * 0.4f);
                }
            }

            if (Input.GetKeyDown(KeyCode.F1)) _showDebug = !_showDebug;
            if (_showDebug) Debug.DrawLine(_rb.position, _rb.position + _aim * 8f, Color.yellow);
        }

        // ---------- 补墙 ----------

        private void Start()
        {
            UpdateBrickHud();
        }

        public bool AddBrick(int amount)
        {
            if (_bricks >= maxBricks) return false;

            _bricks = Mathf.Min(maxBricks, _bricks + amount);
            UpdateBrickHud();

            if (UIManager.Instance != null)
                UIManager.Instance.ShowFloatText("+砖", _rb.position + Vector3.up * 1.6f, new Color(1f, 0.92f, 0.6f));

            return true;
        }

        private void UpdateBrickHud()
        {
            if (UIManager.Instance != null && UIManager.Instance.Hud != null)
                UIManager.Instance.Hud.SetBricks(_bricks, maxBricks, _repairBudget);
        }

        /// <summary>每次进入休整/准备期，重置本轮补砖额度：最多补损失砖数的 1/3。</summary>
        private void RefreshRepairBudget()
        {
            var wave = WaveManager.Instance;
            var inWindow = wave == null || wave.CanRepair;

            if (inWindow && !_wasRepairWindow)
            {
                if (_wall == null) _wall = FindObjectOfType<WallController>();

                var missing = _wall != null ? _wall.totalCount - _wall.aliveCount : 0;
                _repairBudget = Mathf.CeilToInt(missing / 3f);
                UpdateBrickHud();
            }

            _wasRepairWindow = inWindow;
        }

        /// <summary>右键点墙：判定射线是否落在正门墙上，能补就补。返回 true 表示这次右键已被补墙吃掉。</summary>
        private bool TryRepairAt(Vector3 screenPosition)
        {
            var camera = Camera.main;
            if (camera == null) return false;

            if (_wall == null) _wall = FindObjectOfType<WallController>();
            if (_wall == null) return false;

            var ray = camera.ScreenPointToRay(screenPosition);

            // 纯数学判定：射线是否落在正门立面上（不放触发器碰撞体，避免子弹出膛即被回收）
            Vector3 point;
            if (!_wall.TryGetFacadePoint(ray, 150f, out point)) return false;

            // 只有两波之间才能补墙
            var wave = WaveManager.Instance;
            if (wave != null && !wave.CanRepair)
            {
                if (UIManager.Instance != null)
                    UIManager.Instance.ShowFloatText("打退这一波才能补墙", _rb.position + Vector3.up * 1.9f,
                        new Color(1f, 0.72f, 0.5f));
                return true;
            }

            var gap = _wall.FindNearestGap(point, 2.6f);
            if (gap == null) return false;
            if (Vector3.Distance(_rb.position, gap.transform.position) > 5f) return false;

            if (_repairBudget <= 0)
            {
                if (UIManager.Instance != null)
                    UIManager.Instance.ShowFloatText("这轮最多补损失的 1/3", _rb.position + Vector3.up * 1.9f,
                        new Color(1f, 0.72f, 0.5f));
                return true;
            }

            if (_bricks <= 0)
            {
                if (UIManager.Instance != null)
                    UIManager.Instance.ShowFloatText("没有砖了", gap.transform.position + Vector3.up, new Color(1f, 0.78f, 0.5f));
                return true;
            }

            StartCoroutine(RepairRoutine(_wall, gap));
            return true;
        }

        private IEnumerator RepairRoutine(WallController wall, WallBrick brick)
        {
            IsRepairing = true;
            _hasMoveTarget = false;
            HideMarker();

            if (model != null && brick != null)
            {
                var direction = brick.transform.position - _rb.position;
                direction.y = 0f;
                if (direction.sqrMagnitude > 0.0001f)
                    model.rotation = Quaternion.LookRotation(direction.normalized, Vector3.up);

                model.localScale = new Vector3(1.06f, 0.9f, 1.06f);
            }

            yield return new WaitForSeconds(repairSeconds);

            IsRepairing = false;
            if (model != null) model.localScale = Vector3.one;

            if (brick == null || wall == null) yield break;
            if (!wall.PlaceBrick(brick)) yield break;

            _bricks--;
            _repairBudget = Mathf.Max(0, _repairBudget - 1);
            UpdateBrickHud();

            if (UIManager.Instance != null)
                UIManager.Instance.ShowFloatText("补好了", brick.transform.position + Vector3.up, new Color(0.7f, 1f, 0.7f));
        }

        // ---------- 右键点地移动 ----------

        private void HandleClickMove(IInputSource input)
        {
            if (!clickToMove)
            {
                HideMarker();
                return;
            }

            if (Input.GetMouseButtonDown(1))
            {
                // 右键优先判断是不是点在墙上：是则补砖，否则点地移动
                if (TryRepairAt(Input.mousePosition)) return;

                if (TryGetGroundPoint(Input.mousePosition, out var point))
                {
                    _moveTarget = point;
                    _hasMoveTarget = true;
                    _bestDistance = Vector3.Distance(point, _rb.position);
                    _stuckTimer = 0f;
                    ShowMarker(point);
                }
            }

            // 键盘一按就交还控制权
            if (input != null && input.Move.sqrMagnitude > 0.01f)
            {
                _hasMoveTarget = false;
                HideMarker();
                return;
            }

            if (!_hasMoveTarget)
            {
                HideMarker();
                return;
            }

            var to = _moveTarget - _rb.position;
            to.y = 0f;
            var distance = to.magnitude;

            if (distance < 0.15f)
            {
                _hasMoveTarget = false;
                HideMarker();
                return;
            }

            // 超过 1.2 秒没有更接近目标，判定被挡住，放弃这次点地
            if (distance < _bestDistance - 0.05f)
            {
                _bestDistance = distance;
                _stuckTimer = 0f;
            }
            else
            {
                _stuckTimer += Time.deltaTime;
            }

            if (_stuckTimer > 1.2f)
            {
                _hasMoveTarget = false;
                HideMarker();
                return;
            }

            _move = AvoidWalls(to / distance);

            if (_moveMarker != null && _moveMarker.activeSelf)
            {
                var pulse = 1f + Mathf.Sin(Time.time * 8f) * 0.08f;
                _moveMarker.transform.localScale = new Vector3(0.55f * pulse, 0.02f, 0.55f * pulse);
            }
        }

        private bool TryGetGroundPoint(Vector3 screenPosition, out Vector3 point)
        {
            point = Vector3.zero;

            var camera = Camera.main;
            if (camera == null) return false;

            var ray = camera.ScreenPointToRay(screenPosition);
            var plane = new Plane(Vector3.up, new Vector3(0f, _rb.position.y, 0f));

            float distance;
            if (!plane.Raycast(ray, out distance)) return false;

            var hit = ray.GetPoint(distance);
            point = new Vector3(Mathf.Clamp(hit.x, -30f, 30f), 0.5f, Mathf.Clamp(hit.z, -20f, 20f));
            return true;
        }

        private Vector3 AvoidWalls(Vector3 direction)
        {
            RaycastHit hit;
            if (!Physics.SphereCast(_rb.position + Vector3.up * 0.55f, 0.38f, direction, out hit, 0.75f,
                    _blockMask, QueryTriggerInteraction.Ignore))
                return direction;

            var tangent = Vector3.Cross(Vector3.up, hit.normal);
            if (Vector3.Dot(tangent, direction) < 0f) tangent = -tangent;

            return (tangent.normalized * 0.9f + direction * 0.1f).normalized;
        }

        private void ShowMarker(Vector3 position)
        {
            if (_moveMarker == null)
            {
                _moveMarker = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                _moveMarker.name = "MoveMarker";

                var markerCollider = _moveMarker.GetComponent<Collider>();
                if (markerCollider != null) Destroy(markerCollider);

                var material = new Material(Shader.Find("Standard"));
                material.color = new Color(1f, 0.86f, 0.35f);
                material.EnableKeyword("_EMISSION");
                material.SetColor("_EmissionColor", new Color(1f, 0.72f, 0.2f) * 1.1f);

                var markerRenderer = _moveMarker.GetComponent<Renderer>();
                markerRenderer.material = material;
                markerRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            }

            _moveMarker.SetActive(true);
            _moveMarker.transform.position = new Vector3(position.x, 0.1f, position.z);
            _moveMarker.transform.localScale = new Vector3(0.55f, 0.02f, 0.55f);
        }

        private void HideMarker()
        {
            if (_moveMarker != null) _moveMarker.SetActive(false);
        }

        private void FixedUpdate()
        {
            if (config == null) return;

            var speedScale = Time.time < _slowUntil ? _slowMultiplier : 1f;
            if (BuffSystem.Instance != null) speedScale *= BuffSystem.Instance.MoveMultiplier;

            var target = _move * (config.playerSpeed * speedScale);
            var rate = _move.sqrMagnitude > 0.01f ? config.playerAcceleration : config.playerDeceleration;

            var velocity = _rb.velocity;
            velocity.y = 0f;
            velocity = Vector3.MoveTowards(velocity, target, rate * Time.fixedDeltaTime);
            _rb.velocity = velocity;
        }

        private void Recoil(Vector3 direction)
        {
            _rb.velocity += direction * 0.05f;
        }

        public void Slow(float duration, float multiplier)
        {
            _slowUntil = Mathf.Max(_slowUntil, Time.time + duration);
            _slowMultiplier = multiplier;
        }

        public void Push(Vector3 direction, float force)
        {
            _rb.velocity += direction * force;
        }

        private Vector3 ApplyAimAssist(Vector3 aim)
        {
            var count = Physics.OverlapSphereNonAlloc(_rb.position, 12f, AimBuffer, _enemyMask);
            if (count <= 0) return aim;

            var bestAngle = 3f;
            var best = aim;
            var found = false;

            for (var i = 0; i < count; i++)
            {
                var collider = AimBuffer[i];
                if (collider == null) continue;

                var direction = collider.transform.position - _rb.position;
                direction.y = 0f;
                if (direction.sqrMagnitude < 0.0001f) continue;

                direction.Normalize();
                var angle = Vector3.Angle(aim, direction);
                if (angle >= bestAngle) continue;

                bestAngle = angle;
                best = direction;
                found = true;
            }

            return found ? Vector3.Lerp(aim, best, 0.3f).normalized : aim;
        }
    }
}
