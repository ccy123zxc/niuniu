using UnityEngine;

namespace OldAlley.FX
{
    /// <summary>脚步灰尘：小灰点，0.35s 淡出。</summary>
    public static class Dust
    {
        private static Sprite _sprite;

        public static void Spawn(Vector3 position)
        {
            if (_sprite == null) _sprite = Make();

            var go = new GameObject("Dust", typeof(SpriteRenderer), typeof(DustFade));
            go.transform.position = position;

            var sr = go.GetComponent<SpriteRenderer>();
            sr.sprite = _sprite;
            sr.color = new Color(0.75f, 0.72f, 0.65f, 0.5f);
            sr.sortingLayerName = "FX";
            sr.sortingOrder = 350;
        }

        private static Sprite Make()
        {
            var tex = new Texture2D(3, 3, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Point;

            for (var y = 0; y < 3; y++)
            for (var x = 0; x < 3; x++)
                tex.SetPixel(x, y, (x + y) % 2 == 0
                    ? new Color(1f, 1f, 1f, 1f)
                    : new Color(1f, 1f, 1f, 0.45f));

            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, 3, 3), new Vector2(0.5f, 0.5f), 16f, 0, SpriteMeshType.FullRect);
        }
    }

    public class DustFade : MonoBehaviour
    {
        private SpriteRenderer _renderer;
        private float _time;

        private void Awake()
        {
            _renderer = GetComponent<SpriteRenderer>();
        }

        private void Update()
        {
            _time += Time.deltaTime;
            transform.position += Vector3.up * (0.4f * Time.deltaTime);

            if (_renderer != null)
            {
                var color = _renderer.color;
                color.a = Mathf.Lerp(0.5f, 0f, _time / 0.35f);
                _renderer.color = color;
            }

            if (_time >= 0.35f) Destroy(gameObject);
        }
    }
}
