using OldAlley.Audio;
using OldAlley.Player;
using UnityEngine;

namespace OldAlley.Pickup
{
    /// <summary>3D 拾取物：悬浮方块 + 触发器，靠近自动拾取，15 秒后消失。</summary>
    public class PickupItem : MonoBehaviour
    {
        public PickupConfig config;
        public Transform body;
        public Renderer bodyRenderer;
        public float lifetime = 15f;

        private float _age;
        private bool _consumed;
        private Material _material;
        private Vector3 _bodyBase;
        private bool _hasBody;

        private void Awake()
        {
            if (body != null)
            {
                _bodyBase = body.localPosition;
                _hasBody = true;
            }

            if (bodyRenderer != null) _material = bodyRenderer.material;
        }

        public void Configure(PickupConfig cfg)
        {
            config = cfg;
            _age = 0f;
            _consumed = false;

            if (_material == null && bodyRenderer != null) _material = bodyRenderer.material;
            if (_material != null) _material.color = cfg.color;
            if (bodyRenderer != null) bodyRenderer.enabled = true;

            gameObject.SetActive(true);
        }

        private void Update()
        {
            if (_consumed) return;

            _age += Time.deltaTime;

            if (_hasBody && body != null)
            {
                var y = _bodyBase.y + Mathf.Sin(_age * (2f * Mathf.PI / 1.5f)) * 0.12f;
                body.localPosition = new Vector3(_bodyBase.x, y, _bodyBase.z);
            }

            if (_age >= lifetime)
            {
                Destroy(gameObject);
                return;
            }

            if (_age >= lifetime - 3f && bodyRenderer != null)
                bodyRenderer.enabled = Mathf.PingPong(_age * 6f, 1f) > 0.5f;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (_consumed) return;
            if (other.GetComponent<PlayerController>() == null) return;

            _consumed = true;

            if (BuffSystem.Instance != null)
                BuffSystem.Instance.Apply(config, transform.position + Vector3.up * 1f);

            AudioManager.Play(SfxId.Pickup);
            Destroy(gameObject);
        }
    }
}
