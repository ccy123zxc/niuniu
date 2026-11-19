using System.Collections;
using OldAlley.Audio;
using OldAlley.Core;
using UnityEngine;

namespace OldAlley.Enemy
{
    /// <summary>训练假人（3D）：999 血，只用于验证命中。</summary>
    public class Dummy : MonoBehaviour
    {
        public int hp = 999;
        public Renderer[] renderers;

        private Material[] _materials;
        private Color[] _baseColors;
        private Vector3 _home;
        private Coroutine _flash;

        private void Awake()
        {
            _home = transform.position;

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

        public void TakeHit(int damage, Vector3 direction)
        {
            hp -= damage;

            AudioManager.Play(SfxId.Hit);
            HitStop.Play(0.03f);
            StarBurst.Spawn(transform.position + Vector3.up * 0.8f, direction);

            if (_flash != null) StopCoroutine(_flash);
            _flash = StartCoroutine(Flash());
        }

        private IEnumerator Flash()
        {
            for (var i = 0; i < _materials.Length; i++)
                if (_materials[i] != null) _materials[i].color = Color.white;

            yield return new WaitForSeconds(0.06f);

            for (var i = 0; i < _materials.Length; i++)
                if (_materials[i] != null) _materials[i].color = _baseColors[i];
        }
    }
}
