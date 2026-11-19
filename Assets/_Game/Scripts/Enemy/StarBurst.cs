using UnityEngine;

namespace OldAlley.Enemy
{
    /// <summary>命中星星与击杀白色像素爆点（不喷血）。</summary>
    public static class StarBurst
    {
        private static Sprite _star;
        private static Sprite _pixel;

        public static void Spawn(Vector3 position, Vector2 direction)
        {
            if (_star == null) _star = MakeStar();

            for (var i = 0; i < 3; i++)
            {
                var go = new GameObject("HitStar", typeof(SpriteRenderer), typeof(StarKick));
                go.transform.position = position;

                var sr = go.GetComponent<SpriteRenderer>();
                sr.sprite = _star;
                sr.sortingLayerName = "FX";
                sr.sortingOrder = 900;

                go.GetComponent<StarKick>().Init(direction, i);
            }
        }

        /// <summary>击杀时 6 颗白色像素爆点。</summary>
        public static void SpawnWhite(Vector3 position, Vector2 direction)
        {
            if (_pixel == null) _pixel = MakePixel();

            for (var i = 0; i < 6; i++)
            {
                var go = new GameObject("HitPixel", typeof(SpriteRenderer), typeof(StarKick));
                go.transform.position = position;

                var sr = go.GetComponent<SpriteRenderer>();
                sr.sprite = _pixel;
                sr.sortingLayerName = "FX";
                sr.sortingOrder = 910;

                go.GetComponent<StarKick>().Init(direction, i, Color.white, 2.4f);
            }
        }

        private static Sprite MakeStar()
        {
            var tex = new Texture2D(7, 7, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Point;

            for (var y = 0; y < 7; y++)
            for (var x = 0; x < 7; x++)
            {
                var dx = Mathf.Abs(x - 3);
                var dy = Mathf.Abs(y - 3);
                var on = dx + dy <= 2 || dx == 0 || dy == 0;
                tex.SetPixel(x, y, on ? Color.white : new Color(0f, 0f, 0f, 0f));
            }

            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, 7, 7), new Vector2(0.5f, 0.5f), 16f, 0, SpriteMeshType.FullRect);
        }

        private static Sprite MakePixel()
        {
            var tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Point;

            for (var y = 0; y < 2; y++)
            for (var x = 0; x < 2; x++)
                tex.SetPixel(x, y, Color.white);

            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, 2, 2), new Vector2(0.5f, 0.5f), 16f, 0, SpriteMeshType.FullRect);
        }
    }

    public class StarKick : MonoBehaviour
    {
        private const float Life = 0.35f;

        private SpriteRenderer _sr;
        private Vector2 _velocity;
        private float _time;

        public void Init(Vector2 direction, int index)
        {
            Init(direction, index, new Color(1f, 0.86f, 0.42f, 1f), 1.6f);
        }

        public void Init(Vector2 direction, int index, Color color, float speed)
        {
            _sr = GetComponent<SpriteRenderer>();

            var angle = (index - 2) * 30f;
            var baseVelocity = direction * speed + Vector2.up * 1.3f;
            _velocity = (Vector2)(Quaternion.Euler(0f, 0f, angle) * baseVelocity);

            if (_sr != null) _sr.color = color;
        }

        private void Update()
        {
            _time += Time.deltaTime;

            transform.position += (Vector3)(_velocity * Time.deltaTime);
            _velocity += Vector2.down * (6f * Time.deltaTime);

            if (_sr != null)
            {
                var color = _sr.color;
                color.a = Mathf.Clamp01(1f - _time / Life);
                _sr.color = color;
            }

            if (_time >= Life) Destroy(gameObject);
        }
    }
}
