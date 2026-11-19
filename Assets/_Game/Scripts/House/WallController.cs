using System.Collections.Generic;
using UnityEngine;

namespace OldAlley.Core
{
    /// <summary>
    /// 正门砖墙管理员：登记所有砖块，提供「最近的砖」「最近的缺口」查询，
    /// 砖被打掉时生成可拾取的砖块。
    /// </summary>
    public class WallController : MonoBehaviour
    {
        public static WallController Instance { get; private set; }

        public GameObject brickPickupTemplate;
        public Transform dropRoot;

        [Header("补墙判定（纯数学，不放碰撞体，避免挡子弹）")]
        public Vector3 facadeCenter;
        public float facadeWidth = 10f;
        public float facadeHeight = 3.2f;

        public int aliveCount;
        public int totalCount;

        [Header("掉砖扣血")]
        public int hpLossPerBrick = 1;

        private readonly List<WallBrick> _bricks = new List<WallBrick>();
        private HouseController _house;

        public IReadOnlyList<WallBrick> Bricks { get { return _bricks; } }

        private void Awake()
        {
            Instance = this;

            _bricks.Clear();
            _bricks.AddRange(GetComponentsInChildren<WallBrick>(true));

            var alive = 0;
            foreach (var brick in _bricks)
            {
                brick.wall = this;
                if (brick.gameObject.activeSelf) alive++;
            }

            aliveCount = alive;
            totalCount = _bricks.Count;

            _house = GetComponentInParent<HouseController>();
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        /// <summary>离指定位置最近的、还完好的砖。</summary>
        public WallBrick FindNearest(Vector3 position, float maxDistance)
        {
            WallBrick best = null;
            var bestSqr = maxDistance * maxDistance;

            for (var i = 0; i < _bricks.Count; i++)
            {
                var brick = _bricks[i];
                if (!brick.gameObject.activeSelf) continue;

                var sqr = (brick.transform.position - position).sqrMagnitude;
                if (sqr >= bestSqr) continue;

                bestSqr = sqr;
                best = brick;
            }

            return best;
        }

        /// <summary>射线是否落在正门墙的立面上（含缺口）。命中则给出立面交点。</summary>
        public bool TryGetFacadePoint(Ray ray, float maxDistance, out Vector3 point)
        {
            point = Vector3.zero;

            var plane = new Plane(-transform.forward, facadeCenter);
            float distance;
            if (!plane.Raycast(ray, out distance)) return false;
            if (distance < 0f || distance > maxDistance) return false;

            point = ray.GetPoint(distance);
            var local = transform.InverseTransformPoint(point);
            local -= transform.InverseTransformPoint(facadeCenter);

            if (Mathf.Abs(local.x) > facadeWidth * 0.5f) return false;
            if (local.y < -facadeHeight * 0.5f || local.y > facadeHeight * 0.5f) return false;

            return true;
        }

        /// <summary>离指定位置最近的空缺口（被打掉的砖）。</summary>
        public WallBrick FindNearestGap(Vector3 position, float maxDistance)
        {
            WallBrick best = null;
            var bestSqr = maxDistance * maxDistance;

            for (var i = 0; i < _bricks.Count; i++)
            {
                var brick = _bricks[i];
                if (brick.gameObject.activeSelf) continue;

                var sqr = (brick.transform.position - position).sqrMagnitude;
                if (sqr >= bestSqr) continue;

                bestSqr = sqr;
                best = brick;
            }

            return best;
        }

        public void BreakBrick(WallBrick brick)
        {
            if (brick == null || !brick.gameObject.activeSelf) return;

            var dropPosition = brick.transform.position;
            brick.gameObject.SetActive(false);
            aliveCount = Mathf.Max(0, aliveCount - 1);

            // 掉砖伴随掉血（老屋受损）
            if (_house != null && hpLossPerBrick > 0)
                _house.TakeDamageQuiet(hpLossPerBrick);

            DropPickup(dropPosition);
        }

        public bool PlaceBrick(WallBrick brick)
        {
            if (brick == null || brick.gameObject.activeSelf) return false;

            brick.ResetBrick();
            aliveCount = Mathf.Min(totalCount, aliveCount + 1);
            return true;
        }

        private void DropPickup(Vector3 position)
        {
            if (brickPickupTemplate == null) return;

            var parent = dropRoot != null ? dropRoot : transform;
            var pickup = Instantiate(brickPickupTemplate, parent);
            pickup.transform.position = new Vector3(position.x, 0.42f, position.z + 0.35f);
            pickup.SetActive(true);
        }
    }
}
