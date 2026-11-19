using UnityEngine;

namespace OldAlley.Player
{
    /// <summary>输入抽象（3D：XZ 平面移动，鼠标射线打到地面）。</summary>
    public interface IInputSource
    {
        Vector3 Move { get; }
        Vector3 AimWorld(Vector3 origin);
        bool FireHeld { get; }
        bool FirePressed { get; }
        bool UseFacingAim { get; }
    }

    public class LegacyInputSource : IInputSource
    {
        private readonly Camera _camera;
        private Plane _ground = new Plane(Vector3.up, Vector3.zero);

        public LegacyInputSource(Camera camera)
        {
            _camera = camera;
        }

        public Vector3 Move
        {
            get
            {
                var v = new Vector3(Input.GetAxisRaw("Horizontal"), 0f, Input.GetAxisRaw("Vertical"));
                return v.sqrMagnitude > 1f ? v.normalized : v;
            }
        }

        public Vector3 AimWorld(Vector3 origin)
        {
            if (_camera == null) return Vector3.forward;

            _ground = new Plane(Vector3.up, new Vector3(0f, origin.y, 0f));
            var ray = _camera.ScreenPointToRay(Input.mousePosition);

            float distance;
            if (!_ground.Raycast(ray, out distance)) return Vector3.forward;

            var point = ray.GetPoint(distance);
            var direction = point - origin;
            direction.y = 0f;

            return direction.sqrMagnitude > 0.0001f ? direction.normalized : Vector3.forward;
        }

        public bool FireHeld { get { return Input.GetMouseButton(0); } }
        public bool FirePressed { get { return Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.J); } }
        public bool UseFacingAim { get { return Input.GetKey(KeyCode.J); } }
    }
}
