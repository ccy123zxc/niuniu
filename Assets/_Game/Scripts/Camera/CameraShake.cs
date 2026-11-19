using UnityEngine;

namespace OldAlley.CameraSystem
{
    /// <summary>
    /// trauma 式相机震动：Shake() 累加 trauma，逐帧衰减，偏移上限约 0.15。
    /// 在 CameraFollow 之后执行，避免被跟随逻辑覆盖。
    /// </summary>
    [DefaultExecutionOrder(100)]
    public class CameraShake : MonoBehaviour
    {
        private float _trauma;

        public void Shake(float amount)
        {
            _trauma = Mathf.Clamp01(_trauma + amount);
        }

        private void LateUpdate()
        {
            if (_trauma <= 0f) return;

            var magnitude = _trauma * _trauma * 0.15f;
            transform.position += new Vector3(
                Random.Range(-magnitude, magnitude),
                Random.Range(-magnitude, magnitude),
                0f);

            _trauma = Mathf.Max(0f, _trauma - 1.2f * Time.deltaTime);
        }
    }
}
