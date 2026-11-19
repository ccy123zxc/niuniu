using System.Collections.Generic;
using OldAlley.Enemy;
using UnityEngine;

namespace OldAlley.Player
{
    /// <summary>3D 石子：触发器命中，穿透计数，超时回收。</summary>
    public class Projectile : MonoBehaviour
    {
        private Rigidbody _rb;
        private SphereCollider _collider;
        private int _damage;
        private float _dieAt;
        private bool _active;
        private int _hitsLeft;
        private readonly List<Collider> _hits = new List<Collider>();

        private void Awake()
        {
            _rb = GetComponent<Rigidbody>();
            _collider = GetComponent<SphereCollider>();
            if (_collider != null) _collider.isTrigger = true;
        }

        public void Launch(Vector3 position, Vector3 direction, float speed, float life, int damage, int pierce = 0)
        {
            if (_rb == null) _rb = GetComponent<Rigidbody>();

            _damage = damage;
            _dieAt = Time.time + life;
            _hitsLeft = 1 + pierce;
            _hits.Clear();

            transform.position = position;
            if (direction.sqrMagnitude > 0.0001f) transform.forward = direction;

            gameObject.SetActive(true);
            _active = true;
            _rb.velocity = direction * speed;
        }

        private void Update()
        {
            if (_active && Time.time >= _dieAt) Recycle();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!_active) return;
            if (other.GetComponent<Projectile>() != null) return;

            // 拾取物是触发器，不应吃掉飞行中的石子
            if (other.gameObject.layer == LayerMask.NameToLayer("Pickup")) return;

            if (_hits.Contains(other)) return;

            var dummy = other.GetComponent<Dummy>();
            var enemy = other.GetComponent<EnemyBase>();

            if (dummy == null && enemy == null)
            {
                Recycle();
                return;
            }

            _hits.Add(other);

            if (dummy != null) dummy.TakeHit(_damage, _rb.velocity.normalized);
            if (enemy != null) enemy.TakeDamage(_damage, _rb.velocity.normalized);

            _hitsLeft--;
            if (_hitsLeft <= 0) Recycle();
        }

        public void Recycle()
        {
            _active = false;
            if (_rb != null) _rb.velocity = Vector3.zero;
            gameObject.SetActive(false);
        }
    }
}
