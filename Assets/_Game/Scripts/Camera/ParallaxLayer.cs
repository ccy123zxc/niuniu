using UnityEngine;

namespace OldAlley.CameraSystem
{
    /// <summary>
    /// 2.5D 视差层。
    /// depth = 1：世界锁定（和地面同一平面，近景角色/道具）。
    /// depth &lt; 1：越远越跟随相机（远山 0.05、中景 0.5）。
    /// depth &gt; 1：前景层，反向移动，滚动速度比世界更快（底部草丛、屋檐）。
    /// </summary>
    [DefaultExecutionOrder(50)]
    public class ParallaxLayer : MonoBehaviour
    {
        public float depth = 1f;

        private Vector3 _start;
        private float _startCameraX;

        private void Start()
        {
            _start = transform.position;
            _startCameraX = Camera.main != null ? Camera.main.transform.position.x : 0f;
        }

        private void LateUpdate()
        {
            var cam = Camera.main;
            if (cam == null) return;

            var move = (cam.transform.position.x - _startCameraX) * (1f - depth);
            transform.position = new Vector3(_start.x + move, _start.y, _start.z);
        }
    }
}
