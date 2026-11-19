using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.IO;

/// <summary>一键生成《旧巷守卫》第一关 3D 老巷场景（内置管线 / Standard 材质）</summary>
public class Alley3DBuilder
{
    const string TEX = "Assets/Textures/Alley/";
    const string SCENE = "Assets/Scenes/Alley3D.unity";
    const float ALLEY = 3.6f;

    static Material Mat(string key, float tile, Color? tint = null, bool emissive = false)
    {
        var shader = Shader.Find("Standard");
        var m = new Material(shader);
        m.name = key;

        string dp = TEX + key + "_Diffuse.jpg";
        var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(dp);
        if (tex != null)
        {
            m.mainTexture = tex;
            m.mainTextureScale = new Vector2(tile, tile);
        }
        if (tint.HasValue) m.color = tint.Value;

        string np = TEX + key + "_nor_gl.jpg";
        var nrm = AssetDatabase.LoadAssetAtPath<Texture2D>(np);
        if (nrm != null)
        {
            m.SetTexture("_BumpMap", nrm);
            m.EnableKeyword("_NORMALMAP");
        }
        m.SetFloat("_Glossiness", 0.18f);
        m.SetFloat("_Metallic", 0f);
        if (emissive)
        {
            m.EnableKeyword("_EMISSION");
            m.SetColor("_EmissionColor", new Color(1f, 0.18f, 0.06f) * 1.6f);
        }
        AssetDatabase.CreateAsset(m, "Assets/Materials/Alley_" + key + (emissive ? "_E" : "") + ".mat");
        return m;
    }

    static GameObject Box(string name, Vector3 pos, Vector3 size, Material mat, Vector3 euler = default(Vector3))
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = name;
        go.transform.position = pos;
        go.transform.localScale = size;
        go.transform.eulerAngles = euler;
        go.GetComponent<Renderer>().sharedMaterial = mat;
        return go;
    }

    [MenuItem("旧巷守卫/3D/生成老巷场景")]
    public static void Build()
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        Directory.CreateDirectory("Assets/Materials");
        var wall = Mat("wall", 1.0f);
        var wall2 = Mat("wall", 1.0f, new Color(1.02f, 0.98f, 0.92f));
        var brick = Mat("brick", 1.0f);
        var roof = Mat("roof", 1.0f, new Color(0.55f, 0.57f, 0.6f));
        var floor = Mat("floor", 2.0f);
        var wood = Mat("wood", 1.0f, new Color(0.75f, 0.55f, 0.42f));
        var darkwood = Mat("wood", 1.0f, new Color(0.4f, 0.28f, 0.22f));
        var lantern = Mat("wood", 1.0f, new Color(0.8f, 0.1f, 0.06f), true);

        // 地面
        var g = GameObject.CreatePrimitive(PrimitiveType.Plane);
        g.name = "Ground";
        g.transform.localScale = new Vector3(0.72f, 1f, 6.0f);
        g.transform.position = new Vector3(0, 0, 15);
        g.GetComponent<Renderer>().sharedMaterial = floor;

        // 两侧房屋
        float[] hs = { 3.6f, 4.5f, 3.4f, 4.9f, 3.6f, 4.2f };
        float z = -9.5f;
        for (int i = 0; i < hs.Length; i++)
        {
            float len = 8.2f;
            float h = hs[i];
            for (int s = -1; s <= 1; s += 2)
            {
                float x = s * (ALLEY + 3.4f);
                Box("Wall", new Vector3(x, h * 0.5f, z + len * 0.5f), new Vector3(6.8f, h, len),
                    (i % 2 == 0) ? wall : wall2);
                Box("BrickBase", new Vector3(s * (ALLEY + 0.01f), 0.45f, z + len * 0.5f),
                    new Vector3(0.12f, 0.9f, len), brick);

                // 门
                Box("Door", new Vector3(s * (ALLEY - 0.03f), 1.2f, z + len * 0.55f),
                    new Vector3(0.1f, 2.3f, 1.5f), wood);
                Box("DoorFrame", new Vector3(s * (ALLEY - 0.06f), 2.45f, z + len * 0.55f),
                    new Vector3(0.12f, 0.25f, 1.8f), darkwood);

                // 窗
                for (int k = 0; k < 2; k++)
                {
                    Box("Window", new Vector3(s * (ALLEY - 0.03f), 1.9f, z + 1.2f + k * 5.2f),
                        new Vector3(0.1f, 1.0f, 1.1f), darkwood);
                }

                // 屋面
                float ang = 26f * s;
                Box("Roof", new Vector3(s * (ALLEY + 1.9f), h + 0.95f, z + len * 0.5f),
                    new Vector3(4.6f, 0.22f, len + 1.0f), roof, new Vector3(0, 0, ang));
                Box("Eave", new Vector3(s * (ALLEY - 0.35f), h - 0.1f, z + len * 0.5f),
                    new Vector3(0.7f, 0.18f, len + 1.0f), darkwood);

                // 灯笼
                if (i % 2 == 0)
                {
                    var lan = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    lan.name = "Lantern";
                    lan.transform.position = new Vector3(s * (ALLEY - 0.45f), h - 0.6f, z + len * 0.5f);
                    lan.transform.localScale = new Vector3(0.34f, 0.4f, 0.34f);
                    lan.GetComponent<Renderer>().sharedMaterial = lantern;
                    var lt = new GameObject("LanternLight").AddComponent<Light>();
                    lt.type = LightType.Point;
                    lt.range = 6f;
                    lt.intensity = 1.2f;
                    lt.color = new Color(1f, 0.45f, 0.2f);
                    lt.transform.position = lan.transform.position;
                }
            }
            z += len + 0.6f;
        }

        // 巷尾牌坊
        for (int s = -1; s <= 1; s += 2)
        {
            for (int k = -1; k <= 1; k += 2)
            {
                Box("PaifangCol", new Vector3(k * 3.1f, 2.4f, 38f), new Vector3(0.35f, 4.8f, 0.35f), brick);
            }
        }
        Box("PaifangBeam", new Vector3(0, 4.7f, 38f), new Vector3(7.4f, 0.4f, 0.5f), brick);
        Box("PaifangRoof", new Vector3(0, 5.15f, 38f), new Vector3(8.2f, 0.28f, 1.4f), roof,
            new Vector3(12f, 0, 0));

        // 石狮
        for (int s = -1; s <= 1; s += 2)
        {
            Box("LionBase", new Vector3(s * 3.0f, 0.4f, 4.4f), new Vector3(0.8f, 0.8f, 0.9f), brick);
            var lion = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            lion.name = "Lion";
            lion.transform.position = new Vector3(s * 3.0f, 1.15f, 4.4f);
            lion.transform.localScale = new Vector3(0.75f, 0.85f, 0.8f);
            lion.GetComponent<Renderer>().sharedMaterial = brick;
        }

        // 水缸
        for (int i = 0; i < 4; i++)
        {
            var jar = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            jar.name = "Jar";
            float x = (i % 2 == 0) ? 2.9f : -2.9f;
            jar.transform.position = new Vector3(x, 0.45f, 3f + i * 7f);
            jar.transform.localScale = new Vector3(0.6f, 0.8f, 0.6f);
            jar.GetComponent<Renderer>().sharedMaterial = darkwood;
        }

        // 长凳 / 竹椅
        for (int i = 0; i < 3; i++)
        {
            Box("Bench", new Vector3((i % 2 == 0) ? -2.7f : 2.7f, 0.45f, 7f + i * 9f),
                new Vector3(1.0f, 0.12f, 0.45f), wood);
        }

        // 树（墙后）
        for (int i = 0; i < 3; i++)
        {
            float x = (i % 2 == 0) ? 6.5f : -6.5f;
            float zz = 10f + i * 11f;
            var trunk = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            trunk.name = "Trunk";
            trunk.transform.position = new Vector3(x, 3.0f, zz);
            trunk.transform.localScale = new Vector3(0.35f, 3.0f, 0.35f);
            trunk.GetComponent<Renderer>().sharedMaterial = darkwood;
            var crown = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            crown.name = "Crown";
            crown.transform.position = new Vector3(x, 6.4f, zz);
            crown.transform.localScale = new Vector3(3.4f, 2.4f, 3.2f);
            var leaf = new Material(Shader.Find("Standard"));
            leaf.color = new Color(0.16f, 0.26f, 0.12f);
            crown.GetComponent<Renderer>().sharedMaterial = leaf;
        }

        // 灯光
        var sunGo = new GameObject("Sun");
        var sun = sunGo.AddComponent<Light>();
        sun.type = LightType.Directional;
        sun.intensity = 1.15f;
        sun.color = new Color(1f, 0.93f, 0.84f);
        sun.shadows = LightShadows.Soft;
        sunGo.transform.eulerAngles = new Vector3(42f, -35f, 0);

        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
        RenderSettings.ambientLight = new Color(0.45f, 0.5f, 0.58f);
        RenderSettings.skybox = AssetDatabase.GetBuiltinExtraResource<Material>("Default-Skybox.mat");

        // 相机
        var camGo = new GameObject("Main Camera");
        var cam = camGo.AddComponent<Camera>();
        camGo.tag = "MainCamera";
        cam.fieldOfView = 42f;
        cam.nearClipPlane = 0.1f;
        cam.farClipPlane = 200f;
        camGo.transform.position = new Vector3(0, 3.3f, -11f);
        camGo.transform.LookAt(new Vector3(0, 3.4f, 16f));

        EditorSceneManager.SaveScene(scene, SCENE);
        AssetDatabase.SaveAssets();
        Debug.Log("[Alley3D] scene saved: " + SCENE);

        // 截图
        const int W = 2560, H = 1440;
        var rt = new RenderTexture(W, H, 24);
        cam.targetTexture = rt;
        cam.Render();
        RenderTexture.active = rt;
        var tex = new Texture2D(W, H, TextureFormat.RGB24, false);
        tex.ReadPixels(new Rect(0, 0, W, H), 0, 0);
        tex.Apply();
        File.WriteAllBytes(Path.GetFullPath("Assets/Scenes/alley3d_preview.png"), tex.EncodeToPNG());
        RenderTexture.active = null;
        cam.targetTexture = null;
        Debug.Log("[Alley3D] screenshot saved");
    }

    public static void BuildBatch()
    {
        Build();
        EditorApplication.Exit(0);
    }
}
