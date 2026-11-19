using System.Collections;
using UnityEngine;

namespace OldAlley.Core
{
    /// <summary>正门墙上的一块砖：独立血量，被打掉后会留出缺口，可被玩家补回去。</summary>
    public class WallBrick : MonoBehaviour
    {
        public WallController wall;
        public int maxHp = 24;
        public int column;
        public int row;

        private int _hp;
        private Vector3 _baseScale = Vector3.one;
        private Coroutine _punch;

        public bool IsAlive { get { return gameObject.activeSelf; } }
        public int Hp { get { return _hp; } }

        private void Awake()
        {
            _baseScale = transform.localScale;
            _hp = maxHp;
        }

        /// <summary>补回去时重置血量与形变。</summary>
        public void ResetBrick()
        {
            _hp = maxHp;
            transform.localScale = _baseScale;
            gameObject.SetActive(true);

            if (_punch != null) StopCoroutine(_punch);
            _punch = null;
        }

        public void TakeDamage(int damage, Vector3 from)
        {
            if (!gameObject.activeSelf) return;

            _hp -= damage;

            if (_hp <= 0)
            {
                if (wall != null) wall.BreakBrick(this);
                return;
            }

            if (!gameObject.activeInHierarchy) return;

            if (_punch != null) StopCoroutine(_punch);
            _punch = StartCoroutine(Punch());
        }

        private IEnumerator Punch()
        {
            transform.localScale = _baseScale * 0.88f;
            yield return new WaitForSeconds(0.06f);
            transform.localScale = _baseScale;
            _punch = null;
        }
    }
}
