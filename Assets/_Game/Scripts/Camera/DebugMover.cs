using UnityEngine;

namespace OldAlley.CameraSystem
{
    /// <summary>
    /// Stage 2 临时替身：用方向键移动玩家占位物，方便验证相机跟随与边界。
    /// Stage 3 接入 PlayerController 后删除。
    /// </summary>
    public class DebugMover : MonoBehaviour
    {
        public float speed = 6f;
        public Vector2 min = new Vector2(-58f, -2.8f);
        public Vector2 max = new Vector2(58f, -0.5f);

        private void Update()
        {
            var input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
            if (input.sqrMagnitude > 0.01f) input.Normalize();

            var p = transform.position + (Vector3)(input * (speed * Time.deltaTime));
            p.x = Mathf.Clamp(p.x, min.x, max.x);
            p.y = Mathf.Clamp(p.y, min.y, max.y);
            transform.position = p;
        }
    }
}
