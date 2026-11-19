using UnityEngine;

namespace OldAlley.CameraSystem
{
    /// <summary>3D 等距相机：固定偏移跟随，Clamp 在地图范围内。</summary>
    [DefaultExecutionOrder(-50)]
    public class CameraFollow : MonoBehaviour
    {
        public Transform target;
        public Vector3 offset = new Vector3(12f, 14f, -12f);

        [Header("地图边界")]
        public float minX = -16f;
        public float maxX = 16f;
        public float minZ = -8f;
        public float maxZ = 8f;

        public float smooth = 8f;

        private void LateUpdate()
        {
            if (target == null) return;

            var center = new Vector3(
                Mathf.Clamp(target.position.x, minX, maxX),
                target.position.y,
                Mathf.Clamp(target.position.z, minZ, maxZ));

            var desired = center + offset;
            var t = 1f - Mathf.Exp(-smooth * Time.deltaTime);
            transform.position = Vector3.Lerp(transform.position, desired, t);
            transform.rotation = Quaternion.LookRotation((center + Vector3.up * 0.8f) - transform.position, Vector3.up);
        }
    }
}
