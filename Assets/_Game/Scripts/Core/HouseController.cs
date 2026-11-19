using System.Collections;
using OldAlley.Audio;
using OldAlley.CameraSystem;
using OldAlley.UI;
using UnityEngine;

namespace OldAlley.Core
{
    /// <summary>3D 老屋血量与受伤反馈。</summary>
    public class HouseController : MonoBehaviour
    {
        public int maxHp = 100;
        public int hp = 100;
        public Transform doorAnchor;
        public CameraShake shake;
        public Renderer[] renderers;

        public event System.Action<int, int> OnHpChanged;
        public event System.Action OnDestroyed;

        private Material[] _materials;
        private Color[] _baseColors;
        private Coroutine _flash;
        private bool _destroyed;

        public Vector3 DoorPosition
        {
            get { return doorAnchor != null ? doorAnchor.position : transform.position; }
        }

        private void Awake()
        {
            if (maxHp <= 0) maxHp = 100;
            hp = maxHp;

            if (renderers == null || renderers.Length == 0)
                renderers = GetComponentsInChildren<Renderer>();

            _materials = new Material[renderers.Length];
            _baseColors = new Color[renderers.Length];

            for (var i = 0; i < renderers.Length; i++)
            {
                _materials[i] = renderers[i].material;
                _baseColors[i] = _materials[i].color;
            }
        }

        public void TakeDamage(int amount)
        {
            ApplyDamage(amount, true);
        }

        /// <summary>掉砖导致的小额失血：只更新血量与 HUD，不触发大反馈。</summary>
        public void TakeDamageQuiet(int amount)
        {
            ApplyDamage(amount, false);
        }

        private void ApplyDamage(int amount, bool feedback)
        {
            hp -= amount;

            if (feedback)
            {
                if (shake != null) shake.Shake(0.5f);
                AudioManager.Play(SfxId.HouseHurt);
                if (UIManager.Instance != null) UIManager.Instance.FlashDamage();

                if (_flash != null) StopCoroutine(_flash);
                _flash = StartCoroutine(Flash());
            }

            if (OnHpChanged != null) OnHpChanged(hp, maxHp);

            if (hp <= 0 && !_destroyed)
            {
                _destroyed = true;
                Debug.Log("[House] 老屋被拆了");
                if (OnDestroyed != null) OnDestroyed();
            }
        }

        public void Heal(int amount)
        {
            if (amount <= 0) return;

            hp = Mathf.Min(maxHp, hp + amount);
            if (OnHpChanged != null) OnHpChanged(hp, maxHp);
        }

        private IEnumerator Flash()
        {
            for (var i = 0; i < _materials.Length; i++)
                if (_materials[i] != null) _materials[i].color = new Color(1f, 0.35f, 0.3f, 1f);

            yield return new WaitForSeconds(0.12f);

            for (var i = 0; i < _materials.Length; i++)
                if (_materials[i] != null) _materials[i].color = _baseColors[i];
        }
    }
}
