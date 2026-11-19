using UnityEngine;

namespace OldAlley.Core
{
    /// <summary>
    /// 挂在老屋上，记录敌人攻击的判定点（门）。
    /// Stage 4 的敌人到达这里后开始扣血。
    /// </summary>
    public class HouseMarker : MonoBehaviour
    {
        [Tooltip("敌人到达并开始捣乱的位置（门）。留空则用老屋自身位置。")]
        public Transform doorAnchor;

        public Vector2 DoorPosition
        {
            get
            {
                if (doorAnchor != null) return doorAnchor.position;
                return transform.position;
            }
        }
    }
}
