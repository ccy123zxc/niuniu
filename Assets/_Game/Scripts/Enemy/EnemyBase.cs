using System.Collections;
using OldAlley.Audio;
using OldAlley.CameraSystem;
using OldAlley.Core;
using OldAlley.Player;
using OldAlley.Wave;
using UnityEngine;

namespace OldAlley.Enemy
{
    public enum EnemyState
    {
        Idle,
        Move,
        Stagger,
        Attack,
        Surrender
    }

    /// <summary>
    /// 3D 野物敌人：在 XZ 平面直线冲向老屋，带分离力与障碍绕行；
    /// 被打会顿一下，到达后捣乱扣血，血量归零则冒星星缩小消失。
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class EnemyBase : MonoBehaviour
    {
        public EnemyStats stats;
        public Transform model;
        public Transform[] modelVariants;
        public HouseController house;
        public EnemyPool pool;

        private static readonly Collider[] Neighbors = new Collider[24];

        private Rigidbody _rb;
        private CapsuleCollider _collider;
        private EnemyState _state = EnemyState.Idle;
        private float _stateTimer;
        private int _hp;
        private int _enemyMask;
        private int _wallMask;
        private float _pushCooldown;
        private CameraShake _shake;
        private WallBrick _attackBrick;

        public EnemyState State { get { return _state; } }
        public int Hp { get { return _hp; } }

        private void Awake()
        {
            _rb = GetComponent<Rigidbody>();
            _collider = GetComponent<CapsuleCollider>();
            _rb.isKinematic = true;
            _rb.useGravity = false;
            _rb.constraints = RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePositionY;

            _enemyMask = LayerMask.GetMask("Enemy");
            _wallMask = LayerMask.GetMask("Wall");
            _shake = Camera.main != null ? Camera.main.GetComponent<CameraShake>() : null;
        }

        public void Spawn(EnemyStats enemyStats, Vector3 position)
        {
            stats = enemyStats;
            _hp = stats.hp;

            transform.position = position;
            transform.localScale = Vector3.one * stats.scale;

            gameObject.SetActive(true);

            if (modelVariants != null)
            {
                for (var i = 0; i < modelVariants.Length; i++)
                    if (modelVariants[i] != null)
                        modelVariants[i].gameObject.SetActive(i == stats.visualIndex);
            }

            if (_collider != null)
            {
                _collider.radius = stats.colliderRadius;
                _collider.height = stats.colliderRadius * 2.4f;
                _collider.center = Vector3.zero;
            }

            SetState(EnemyState.Move);
            SnapFaceHouse();
        }

        public void TakeDamage(int damage, Vector3 direction)
        {
            if (_state == EnemyState.Surrender) return;

            _hp -= damage;

            if (_hp <= 0)
            {
                HitStop.Play(0.06f);
                if (_shake != null) _shake.Shake(0.25f);
                AudioManager.Play(SfxId.Ko);
                StarBurst.SpawnWhite(transform.position + Vector3.up * 0.8f, direction);
                Surrender(true);
                return;
            }

            HitStop.Play(0.03f);
            AudioManager.Play(SfxId.Hit);

            SetState(EnemyState.Stagger);
            _stateTimer = 0.18f;
            StartCoroutine(Flash());
        }

        private void SetState(EnemyState next)
        {
            _state = next;
            if (next == EnemyState.Move) _stateTimer = 0f;
        }

        private void FixedUpdate()
        {
            switch (_state)
            {
                case EnemyState.Move:
                    Move();
                    break;
                case EnemyState.Stagger:
                    _stateTimer -= Time.fixedDeltaTime;
                    if (_stateTimer <= 0f) SetState(EnemyState.Move);
                    break;
                case EnemyState.Attack:
                    FaceHouse();
                    _stateTimer -= Time.fixedDeltaTime;
                    if (_stateTimer <= 0f) AttackHouse();
                    break;
            }
        }

        private void Move()
        {
            // 优先找最近的一块好砖；墙全倒了才去打老屋本体
            var wall = WallController.Instance;
            var brick = FindBrick(6f);

            Vector3 target;
            if (brick != null)
            {
                // WHY: 固定站在墙的正面（-Z 一侧）攻击。用「相对自己的方向」会把门洞里的怪算到墙内侧，永远打不到。
                target = new Vector3(brick.transform.position.x, 0.5f,
                    brick.transform.position.z - 0.85f);
            }
            else
            {
                target = house != null ? house.DoorPosition : transform.position;
            }

            var toTarget = target - _rb.position;
            toTarget.y = 0f;

            if (toTarget.magnitude < 0.55f)
            {
                _attackBrick = brick;
                SetState(EnemyState.Attack);
                _stateTimer = 0.5f;
                return;
            }

            var dir = toTarget.normalized;
            FaceDirection(dir);

            var move = (dir + Separation() * 0.6f).normalized;

            RaycastHit hit;
            if (Physics.SphereCast(_rb.position + Vector3.up * 0.5f, 0.3f, move, out hit, 0.55f, _wallMask))
            {
                var tangent = Vector3.Cross(Vector3.up, hit.normal);
                if (Vector3.Dot(tangent, dir) < 0f) tangent = -tangent;
                move = tangent.normalized;
            }

            var next = _rb.position + move * (stats.speed * Time.fixedDeltaTime);
            next.y = 0.5f;

            // 硬限制：不许穿到正门墙的后面，否则玩家永远打不到它
            if (wall != null)
            {
                var limitZ = wall.facadeCenter.z - 0.55f;
                var halfWidth = wall.facadeWidth * 0.5f + 1.2f;
                if (next.z > limitZ && Mathf.Abs(next.x - wall.facadeCenter.x) < halfWidth)
                    next.z = limitZ;
            }

            _rb.MovePosition(next);
        }

        /// <summary>体素野物的头统一朝局部 +X，转向时补 -90°，让头对着前进方向/老屋。</summary>
        private void FaceDirection(Vector3 direction)
        {
            if (model == null) return;

            direction.y = 0f;
            if (direction.sqrMagnitude < 0.0001f) return;

            var target = Quaternion.LookRotation(direction, Vector3.up) * Quaternion.Euler(0f, -90f, 0f);
            model.rotation = Quaternion.Slerp(model.rotation, target, 10f * Time.fixedDeltaTime);
        }

        private void FaceHouse()
        {
            if (house == null) return;
            FaceDirection(house.DoorPosition - transform.position);
        }

        /// <summary>出生瞬间直接朝向老屋，避免先歪一下。</summary>
        private void SnapFaceHouse()
        {
            if (model == null || house == null) return;

            var direction = house.DoorPosition - transform.position;
            direction.y = 0f;
            if (direction.sqrMagnitude < 0.0001f) return;

            model.rotation = Quaternion.LookRotation(direction, Vector3.up) * Quaternion.Euler(0f, -90f, 0f);
        }

        private Vector3 Separation()
        {
            var sum = Vector3.zero;
            var count = Physics.OverlapSphereNonAlloc(_rb.position, 1.1f, Neighbors, _enemyMask);

            for (var i = 0; i < count; i++)
            {
                var other = Neighbors[i];
                if (other == null || other.attachedRigidbody == _rb) continue;

                var away = _rb.position - other.transform.position;
                away.y = 0f;
                var dist = away.magnitude;
                if (dist < 0.001f) continue;

                sum += away / dist * ((1.1f - dist) / 1.1f);
            }

            return sum;
        }

        private void AttackHouse()
        {
            var brick = _attackBrick != null && _attackBrick.gameObject.activeSelf ? _attackBrick : FindBrick(3f);
            _attackBrick = null;

            // 墙还在就先拆砖，拆完才伤害老屋
            if (brick != null)
            {
                brick.TakeDamage(Mathf.Max(2, stats.damageToHouse / 2), transform.position);
                SetState(EnemyState.Move);
                return;
            }

            if (house != null) house.TakeDamage(stats.damageToHouse);
            Surrender(false);
        }

        private WallBrick FindBrick(float maxDistance)
        {
            var wall = WallController.Instance;
            return wall != null ? wall.FindNearest(_rb.position, maxDistance) : null;
        }

        private void Surrender(bool awardScore)
        {
            if (_state == EnemyState.Surrender) return;

            if (awardScore && WaveManager.Instance != null)
                WaveManager.Instance.AddKillScore(stats.score);

            SetState(EnemyState.Surrender);
            StarBurst.Spawn(transform.position + Vector3.up * 0.8f, Vector3.up);
            StartCoroutine(ShrinkAndRecycle());
        }

        private IEnumerator Flash()
        {
            if (model == null) yield break;

            var scale = model.localScale;
            model.localScale = scale * 1.08f;
            yield return new WaitForSeconds(0.08f);
            model.localScale = scale;
        }

        private IEnumerator ShrinkAndRecycle()
        {
            var start = transform.localScale;
            var t = 0f;

            while (t < 0.4f)
            {
                t += Time.deltaTime;
                transform.localScale = Vector3.Lerp(start, Vector3.zero, t / 0.4f);
                yield return null;
            }

            Recycle();
        }

        public void Recycle()
        {
            StopAllCoroutines();
            transform.localScale = Vector3.one * (stats != null ? stats.scale : 1f);
            gameObject.SetActive(false);
        }

        private void Update()
        {
            if (_pushCooldown > 0f) _pushCooldown -= Time.deltaTime;
        }

        private void OnCollisionStay(Collision collision)
        {
            if (collision.gameObject.layer != LayerMask.NameToLayer("Player")) return;
            if (_pushCooldown > 0f) return;

            _pushCooldown = 0.4f;

            var controller = collision.gameObject.GetComponent<PlayerController>();
            if (controller == null) return;

            var dir = collision.transform.position - transform.position;
            dir.y = 0f;
            if (dir.sqrMagnitude < 0.0001f) return;

            controller.Slow(0.4f, 0.6f);
            controller.Push(dir.normalized, 0.3f);
        }
    }
}
