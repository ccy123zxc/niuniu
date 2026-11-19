using System.Collections;
using OldAlley.Audio;
using OldAlley.Core;
using OldAlley.Pickup;
using UnityEngine;

namespace OldAlley.Player
{
    public class Slingshot : MonoBehaviour
    {
        public GameConfig config;
        public ProjectilePool pool;
        public Transform muzzle;

        private Vector3 _aim = Vector3.forward;
        private float _nextFireTime;

        public ProjectilePool Pool { get { return pool; } }

        public void Aim(Vector3 direction)
        {
            if (direction.sqrMagnitude > 0.0001f) _aim = direction.normalized;
        }

        public bool TryFire(Vector3 origin, Vector3 direction)
        {
            if (config == null || pool == null) return false;
            if (Time.time < _nextFireTime) return false;

            var buffs = BuffSystem.Instance;
            var interval = config.fireInterval * (buffs != null ? buffs.FireIntervalMultiplier : 1f);
            _nextFireTime = Time.time + interval;

            var spread = Random.Range(-config.bulletSpread, config.bulletSpread);
            var shotDir = Quaternion.Euler(0f, spread, 0f) * direction;
            shotDir.y = 0f;
            shotDir.Normalize();

            var spawn = origin + shotDir * config.muzzleOffset;
            spawn.y = 0.65f;

            var speed = config.bulletSpeed * (buffs != null ? buffs.BulletSpeedMultiplier : 1f);
            var pierce = buffs != null ? buffs.Pierce : 0;

            var projectile = pool.Get();
            if (projectile != null)
                projectile.Launch(spawn, shotDir, speed, config.bulletLife, config.bulletDamage, pierce);

            AudioManager.Play(SfxId.Shoot);
            if (muzzle != null) StartCoroutine(Stretch());
            return true;
        }

        private IEnumerator Stretch()
        {
            var baseScale = muzzle.localScale;
            muzzle.localScale = new Vector3(baseScale.x * 1.12f, baseScale.y * 0.88f, baseScale.z);
            yield return null;
            muzzle.localScale = baseScale;
        }
    }
}
