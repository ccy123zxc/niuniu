using UnityEngine;

namespace OldAlley.Player
{
    /// <summary>
    /// 程序化走路：两条腿前后摆、身体上下颠，只在真正移动时播放。
    /// 不依赖动画资源，符合「脚本生成资源」的宪法条款。
    /// </summary>
    public class PlayerWalkAnimator : MonoBehaviour
    {
        public Transform leftLeg;
        public Transform rightLeg;
        public Transform body;

        [Header("步态")]
        public float strideAngle = 28f;
        public float bobHeight = 0.05f;
        public float cadence = 9f;

        private Rigidbody _rb;
        private Vector3 _bodyBase;
        private float _phase;
        private float _weight;

        private void Awake()
        {
            _rb = GetComponentInParent<Rigidbody>();
            if (body == null) body = transform;
            _bodyBase = body.localPosition;
        }

        private void Update()
        {
            var speed = 0f;
            if (_rb != null)
                speed = new Vector3(_rb.velocity.x, 0f, _rb.velocity.z).magnitude;

            var moving = speed > 0.2f;
            _weight = Mathf.MoveTowards(_weight, moving ? 1f : 0f, Time.deltaTime * 6f);

            _phase += Time.deltaTime * (moving ? cadence + speed * 0.6f : 4f);

            var angle = Mathf.Sin(_phase) * strideAngle * _weight;

            if (leftLeg != null) leftLeg.localRotation = Quaternion.Euler(angle, 0f, 0f);
            if (rightLeg != null) rightLeg.localRotation = Quaternion.Euler(-angle, 0f, 0f);

            var bob = Mathf.Abs(Mathf.Sin(_phase)) * bobHeight * _weight;
            if (body != null) body.localPosition = _bodyBase + new Vector3(0f, bob, 0f);
        }
    }
}
