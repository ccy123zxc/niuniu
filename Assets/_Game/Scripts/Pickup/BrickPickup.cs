using OldAlley.Player;
using UnityEngine;

namespace OldAlley.Pickup
{
    /// <summary>被野兽打落的墙砖：悬浮旋转；背包没满时靠近会被吸过来并拾取。</summary>
    public class BrickPickup : MonoBehaviour
    {
        public float spinSpeed = 110f;
        public float magnetRadius = 2.2f;
        public float magnetSpeed = 10f;
        public float pickupRadius = 0.7f;

        private Transform _player;
        private PlayerController _controller;

        private void Start()
        {
            var player = GameObject.Find("Player");
            if (player == null) return;

            _player = player.transform;
            _controller = player.GetComponent<PlayerController>();
        }

        private void Update()
        {
            transform.Rotate(Vector3.up, spinSpeed * Time.deltaTime, Space.World);

            if (_player == null || _controller == null) return;

            var target = new Vector3(_player.position.x, transform.position.y, _player.position.z);
            var distance = Vector3.Distance(transform.position, target);
            var hasRoom = _controller.Bricks < _controller.maxBricks;

            // 背包满了就不再吸附，掉在原地等玩家腾出位置
            if (hasRoom && distance <= magnetRadius)
                transform.position = Vector3.MoveTowards(transform.position, target, magnetSpeed * Time.deltaTime);

            if (distance <= pickupRadius && _controller.AddBrick(1))
                Destroy(gameObject);
        }
    }
}
