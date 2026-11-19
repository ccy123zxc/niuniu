using System.Collections.Generic;
using System.IO;
using OldAlley.Audio;
using OldAlley.CameraSystem;
using OldAlley.Core;
using OldAlley.Enemy;
using OldAlley.Pickup;
using OldAlley.Player;
using OldAlley.UI;
using OldAlley.Wave;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

namespace OldAlley.EditorTools
{
    /// <summary>
    /// 3D 体素场景构建器：等距相机 + 方块村庄 + 体素角色。
    /// 菜单「旧巷守卫/3D-构建体素场景」。
    /// </summary>
    public static class Stage3D_SceneBuilder
    {
        private const string ScenePath = "Assets/_Game/Scenes/Game3D.unity";
        private const string MatDir = "Assets/_Game/Materials3D";
        private const string Data = "Assets/_Game/Data/";

        private static readonly Dictionary<string, Material> Mats = new Dictionary<string, Material>();

        [MenuItem("旧巷守卫/3D-构建体素场景")]
        public static void Run()
        {
            EnsureFolder("Assets/_Game/Materials3D");
            LoadMaterials();

            var temp = PrepareUnsavedScene();
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            try
            {
                Build();
                EditorSceneManager.SaveScene(scene, ScenePath);
                AddToBuildSettings();
                AssetDatabase.SaveAssets();
                Debug.Log("[3D] 体素场景构建完成: " + ScenePath);
            }
            finally
            {
                if (!string.IsNullOrEmpty(temp)) AssetDatabase.DeleteAsset(temp);
            }
        }

        // ---------------- 材质 ----------------

        private static void LoadMaterials()
        {
            Mats.Clear();
            Mats["stoneMid"] = Mat("StoneMid", new Color(0.56f, 0.54f, 0.5f));
            Mats["stoneLight"] = Mat("StoneLight", new Color(0.76f, 0.74f, 0.69f));
            Mats["stoneDark"] = Mat("StoneDark", new Color(0.34f, 0.33f, 0.31f));
            Mats["wood"] = Mat("Wood", new Color(0.46f, 0.3f, 0.2f));
            Mats["woodDark"] = Mat("WoodDark", new Color(0.3f, 0.2f, 0.14f));
            Mats["plaster"] = Mat("Plaster", new Color(0.88f, 0.84f, 0.74f));
            Mats["roof"] = Mat("Roof", new Color(0.46f, 0.26f, 0.21f));
            Mats["grass"] = Mat("Grass", new Color(0.42f, 0.56f, 0.33f));
            Mats["dirt"] = Mat("Dirt", new Color(0.63f, 0.54f, 0.41f));
            Mats["red"] = Mat("Red", new Color(0.7f, 0.2f, 0.18f));
            Mats["gold"] = Mat("Gold", new Color(0.85f, 0.65f, 0.25f));
            Mats["pink"] = Mat("Pink", new Color(0.9f, 0.55f, 0.62f));
            Mats["green"] = Mat("Green", new Color(0.3f, 0.5f, 0.28f));
            Mats["greenDark"] = Mat("GreenDark", new Color(0.2f, 0.35f, 0.2f));
            Mats["water"] = Mat("Water", new Color(0.25f, 0.45f, 0.55f));
            Mats["skin"] = Mat("Skin", new Color(0.95f, 0.8f, 0.66f));
            Mats["shirt"] = Mat("Shirt", new Color(0.25f, 0.4f, 0.62f));
            Mats["hair"] = Mat("Hair", new Color(0.12f, 0.1f, 0.1f));
            Mats["chicken"] = Mat("Chicken", new Color(0.7f, 0.46f, 0.28f));
            Mats["chickenGreen"] = Mat("ChickenGreen", new Color(0.22f, 0.43f, 0.38f));
            Mats["dog"] = Mat("Dog", new Color(0.6f, 0.52f, 0.44f));
            Mats["buffalo"] = Mat("Buffalo", new Color(0.36f, 0.35f, 0.34f));
            Mats["boar"] = Mat("Boar", new Color(0.44f, 0.31f, 0.23f));
            Mats["ivory"] = Mat("Ivory", new Color(0.93f, 0.9f, 0.84f));
            Mats["glow"] = Glow("Glow", new Color(1f, 0.72f, 0.35f));
        }

        private static Material Mat(string name, Color color)
        {
            var path = MatDir + "/M_" + name + ".mat";
            var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (mat == null)
            {
                mat = new Material(Shader.Find("Standard"));
                AssetDatabase.CreateAsset(mat, path);
            }

            mat.color = color;
            mat.SetFloat("_Glossiness", 0.12f);
            mat.SetFloat("_Metallic", 0f);
            EditorUtility.SetDirty(mat);
            return mat;
        }

        private static Material Glow(string name, Color color)
        {
            var mat = Mat(name, color);
            mat.EnableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", color * 1.6f);
            EditorUtility.SetDirty(mat);
            return mat;
        }

        private static Material M(string key)
        {
            Material mat;
            return Mats.TryGetValue(key, out mat) ? mat : Mats["stoneMid"];
        }

        // ---------------- 总装 ----------------

        private static void Build()
        {
            BuildLighting();

            var world = new GameObject("[World3D]");
            BuildGround(world.transform);
            BuildVillage(world.transform);
            BuildPlayerHouse(world.transform);
            BuildVegetation(world.transform);
            BuildProps(world.transform);

            var player = BuildPlayer(world.transform);
            BuildCamera(player);

            var house = Object.FindObjectOfType<HouseController>();
            if (house != null) house.shake = Object.FindObjectOfType<CameraShake>();
            var pool = BuildEnemyPool(world.transform, house);
            BuildPickups(world.transform, house, player);
            BuildWaves(world.transform, pool, house);

            BuildUI();
            BuildServices();
        }

        private static void BuildLighting()
        {
            var lightGo = new GameObject("Directional Light", typeof(Light));
            var light = lightGo.GetComponent<Light>();
            light.type = LightType.Directional;
            light.color = new Color(1f, 0.94f, 0.84f);
            light.intensity = 1.15f;
            light.shadows = LightShadows.Soft;
            lightGo.transform.rotation = Quaternion.Euler(52f, -32f, 0f);

            RenderSettings.ambientMode = AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.56f, 0.52f, 0.47f);
        }

        private static void BuildCamera(GameObject player)
        {
            var camGo = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener));
            camGo.tag = "MainCamera";

            var cam = camGo.GetComponent<Camera>();
            cam.orthographic = true;
            cam.orthographicSize = 11f;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.9f, 0.87f, 0.78f);
            cam.nearClipPlane = 0.1f;
            cam.farClipPlane = 200f;

            camGo.transform.position = new Vector3(14f, 16f, -14f);
            camGo.transform.LookAt(new Vector3(0f, 1f, 1f));

            var follow = camGo.AddComponent<CameraFollow>();
            follow.target = player.transform;
            follow.offset = new Vector3(14f, 16f, -14f);
            follow.minX = -20f;
            follow.maxX = 20f;
            follow.minZ = -12f;
            follow.maxZ = 12f;
            follow.smooth = 8f;

            camGo.AddComponent<CameraShake>();
        }

        // ---------------- 地面与建筑 ----------------

        private static void BuildGround(Transform parent)
        {
            Cube(parent, "Ground", new Vector3(0f, -0.25f, 0f), new Vector3(64f, 0.5f, 44f), M("grass"), true);
            Cube(parent, "Plaza", new Vector3(0f, 0.02f, 1f), new Vector3(26f, 0.05f, 17f), M("stoneLight"));

            // 五条土路：从各个路口通向中心广场
            Cube(parent, "PathN", new Vector3(0f, 0.03f, -11f), new Vector3(2.8f, 0.06f, 8f), M("dirt"));
            Cube(parent, "PathNW", new Vector3(-15.5f, 0.03f, -6f), new Vector3(2.6f, 0.06f, 12f), M("dirt")).transform.rotation = Quaternion.Euler(0f, -35f, 0f);
            Cube(parent, "PathNE", new Vector3(15.5f, 0.03f, -6f), new Vector3(2.6f, 0.06f, 12f), M("dirt")).transform.rotation = Quaternion.Euler(0f, 35f, 0f);
            Cube(parent, "PathW", new Vector3(-17f, 0.03f, 6f), new Vector3(2.6f, 0.06f, 10f), M("dirt")).transform.rotation = Quaternion.Euler(0f, 58f, 0f);
            Cube(parent, "PathE", new Vector3(17f, 0.03f, 6f), new Vector3(2.6f, 0.06f, 10f), M("dirt")).transform.rotation = Quaternion.Euler(0f, -58f, 0f);

            Cube(parent, "WallN", new Vector3(0f, 0.6f, 22f), new Vector3(64f, 1.2f, 0.5f), M("stoneDark"), true).layer = Layer("Wall");
            Cube(parent, "WallS", new Vector3(0f, 0.6f, -22f), new Vector3(64f, 1.2f, 0.5f), M("stoneDark"), true).layer = Layer("Wall");
            Cube(parent, "WallE", new Vector3(32f, 0.6f, 0f), new Vector3(0.5f, 1.2f, 44f), M("stoneDark"), true).layer = Layer("Wall");
            Cube(parent, "WallW", new Vector3(-32f, 0.6f, 0f), new Vector3(0.5f, 1.2f, 44f), M("stoneDark"), true).layer = Layer("Wall");
        }

        private static void BuildVillage(Transform parent)
        {
            BuildHouse(parent, new Vector3(-22f, 0f, 10f), 9f, 3.2f, 7f, false);
            BuildHouse(parent, new Vector3(22f, 0f, 10f), 9f, 3.2f, 7f, false);
            BuildHouse(parent, new Vector3(-24f, 0f, -5f), 8f, 3f, 6f, false);
            BuildHouse(parent, new Vector3(24f, 0f, -5f), 8f, 3f, 6f, false);
            BuildHouse(parent, new Vector3(-13f, 0f, 17f), 7f, 3f, 6f, false);
            BuildHouse(parent, new Vector3(13f, 0f, 17f), 7f, 3f, 6f, false);
        }

        private static void BuildPlayerHouse(Transform parent)
        {
            var house = BuildHouse(parent, new Vector3(0f, 0f, 10f), 10f, 3.2f, 7f, true, true);
            var door = house.transform.Find("HouseDoor");
            var controller = house.GetComponent<HouseController>();
            controller.doorAnchor = door;
            controller.renderers = house.GetComponentsInChildren<Renderer>();
        }

        private static GameObject BuildHouse(Transform parent, Vector3 center, float w, float h, float d,
            bool withController, bool voxelFront = false)
        {
            var house = new GameObject(withController ? "House" : "VillageHouse");
            house.transform.SetParent(parent, false);
            house.transform.position = center;

            if (voxelFront)
            {
                // 正面让位给砖墙，主体往后退 0.3
                Cube(house.transform, "Wall", new Vector3(0f, h * 0.5f, 0.3f), new Vector3(w, h, d - 0.6f), M("plaster"), true);
            }
            else
            {
                Cube(house.transform, "Wall", new Vector3(0f, h * 0.5f, 0f), new Vector3(w, h, d), M("plaster"), true);
            }
            Cube(house.transform, "Roof1", new Vector3(0f, h + 0.25f, 0f), new Vector3(w + 1.6f, 0.5f, d + 1.6f), M("roof"));
            Cube(house.transform, "Roof2", new Vector3(0f, h + 0.72f, 0f), new Vector3(w + 0.5f, 0.5f, d + 0.5f), M("roof"));
            Cube(house.transform, "Roof3", new Vector3(0f, h + 1.18f, 0f), new Vector3(w - 0.9f, 0.5f, d - 0.9f), M("roof"));
            Cube(house.transform, "Ridge", new Vector3(0f, h + 1.5f, 0f), new Vector3(w - 2.4f, 0.35f, 0.5f), M("woodDark"));

            if (voxelFront)
            {
                Cube(house.transform, "Door", new Vector3(0f, 1.1f, -d * 0.5f + 0.16f), new Vector3(1.8f, 2.2f, 0.18f), M("wood"));
                BuildVoxelFront(house.transform, w, h, d);
            }
            else
            {
                Cube(house.transform, "Door", new Vector3(0f, 1.1f, -d * 0.5f - 0.06f), new Vector3(1.7f, 2.2f, 0.16f), M("wood"));
                Cube(house.transform, "WinL", new Vector3(-w * 0.3f, 1.7f, -d * 0.5f - 0.05f), new Vector3(1.1f, 1.1f, 0.12f), M("water"));
                Cube(house.transform, "WinR", new Vector3(w * 0.3f, 1.7f, -d * 0.5f - 0.05f), new Vector3(1.1f, 1.1f, 0.12f), M("water"));
            }
            Cube(house.transform, "BannerL", new Vector3(-w * 0.5f - 0.1f, 1.8f, -d * 0.5f - 0.05f), new Vector3(0.12f, 2.2f, 0.7f), M("red"));
            Cube(house.transform, "BannerR", new Vector3(w * 0.5f + 0.1f, 1.8f, -d * 0.5f - 0.05f), new Vector3(0.12f, 2.2f, 0.7f), M("red"));

            var doorAnchor = new GameObject("HouseDoor");
            doorAnchor.transform.SetParent(house.transform, false);
            doorAnchor.transform.localPosition = new Vector3(0f, 0.5f, -d * 0.5f - 0.9f);

            if (withController)
            {
                var controller = house.AddComponent<HouseController>();
                controller.maxHp = 100;
            }

            return house;
        }

        /// <summary>玩家老屋的正门砖墙：10×6 格砖，中间下方留门洞；带补墙判定区与掉落砖模板。</summary>
        private static void BuildVoxelFront(Transform house, float w, float h, float d)
        {
            var front = new GameObject("FrontWall");
            front.transform.SetParent(house, false);

            var wall = front.AddComponent<WallController>();

            // 只用数学平面判定补墙，绝不放碰撞体/触发器：否则子弹会在墙前被吃掉
            wall.facadeCenter = front.transform.TransformPoint(new Vector3(0f, h * 0.5f, -d * 0.5f + 0.16f));
            wall.facadeWidth = w;
            wall.facadeHeight = h;

            wall.dropRoot = front.transform;
            wall.brickPickupTemplate = BuildBrickPickupTemplate(front.transform);

            const int cols = 10;
            const int rows = 6;
            var brickW = w / cols;
            var brickH = h / rows;
            var z = -d * 0.5f + 0.16f;

            for (var row = 0; row < rows; row++)
            {
                for (var col = 0; col < cols; col++)
                {
                    var x = -w * 0.5f + brickW * (col + 0.5f);
                    var y = brickH * (row + 0.5f);

                    // 门洞：中间两列、下四排留空
                    if (Mathf.Abs(x) < 0.9f && y < 2.3f) continue;

                    var brickGo = Cube(front.transform, "Brick_" + col + "_" + row,
                        new Vector3(x, y, z),
                        new Vector3(brickW * 0.97f, brickH * 0.94f, 0.32f),
                        M("plaster"), true);

                    var brick = brickGo.AddComponent<WallBrick>();
                    brick.maxHp = 24;
                    brick.column = col;
                    brick.row = row;
                }
            }
        }

        private static GameObject BuildBrickPickupTemplate(Transform parent)
        {
            var root = new GameObject("BrickPickupTemplate", typeof(BrickPickup), typeof(SphereCollider));
            root.transform.SetParent(parent, false);
            root.transform.localPosition = new Vector3(0f, 0.5f, -16f);
            root.SetActive(false);

            var trigger = root.GetComponent<SphereCollider>();
            trigger.isTrigger = true;
            trigger.radius = 0.9f;

            var body = Cube(root.transform, "Body", Vector3.zero,
                new Vector3(0.42f, 0.3f, 0.34f), M("plaster"));
            body.transform.localEulerAngles = new Vector3(0f, 0f, 10f);

            return root;
        }

        // ---------------- 植被与道具 ----------------

        private static void BuildVegetation(Transform parent)
        {
            BuildTree(parent, new Vector3(-16f, 0f, 1f), M("pink"));
            BuildTree(parent, new Vector3(17f, 0f, 4f), M("green"));
            BuildTree(parent, new Vector3(-20f, 0f, -13f), M("green"));
            BuildTree(parent, new Vector3(21f, 0f, -14f), M("pink"));
        }

        private static void BuildTree(Transform parent, Vector3 position, Material leaf)
        {
            var tree = new GameObject("Tree");
            tree.transform.SetParent(parent, false);
            tree.transform.position = position;

            Cube(tree.transform, "Trunk", new Vector3(0f, 1.9f, 0f), new Vector3(0.8f, 3.8f, 0.8f), M("wood"));
            Cube(tree.transform, "BranchL", new Vector3(-1.0f, 3.6f, 0.1f), new Vector3(2.2f, 0.4f, 0.4f), M("wood"));
            Cube(tree.transform, "BranchR", new Vector3(1.0f, 3.6f, -0.1f), new Vector3(2.2f, 0.4f, 0.4f), M("wood"));

            Cube(tree.transform, "Canopy1", new Vector3(0f, 4.9f, 0f), new Vector3(5.4f, 1.3f, 4.8f), leaf);
            Cube(tree.transform, "Canopy2", new Vector3(-1.7f, 4.3f, 0.7f), new Vector3(2.8f, 1.0f, 2.6f), leaf);
            Cube(tree.transform, "Canopy3", new Vector3(1.8f, 4.4f, -0.5f), new Vector3(2.6f, 1.0f, 2.4f), leaf);
            Cube(tree.transform, "Canopy4", new Vector3(0f, 5.6f, 0f), new Vector3(3.0f, 0.9f, 2.8f), leaf);
        }

        private static void BuildProps(Transform parent)
        {
            // 水池
            Cube(parent, "Pond", new Vector3(17f, 0.06f, -11f), new Vector3(9f, 0.12f, 6f), M("water"));
            Cube(parent, "PondRimN", new Vector3(17f, 0.15f, -7.7f), new Vector3(9.4f, 0.3f, 0.4f), M("stoneDark"));
            Cube(parent, "PondRimS", new Vector3(17f, 0.15f, -14.3f), new Vector3(9.4f, 0.3f, 0.4f), M("stoneDark"));
            Cube(parent, "PondRimE", new Vector3(21.6f, 0.15f, -11f), new Vector3(0.4f, 0.3f, 6.4f), M("stoneDark"));
            Cube(parent, "PondRimW", new Vector3(12.4f, 0.15f, -11f), new Vector3(0.4f, 0.3f, 6.4f), M("stoneDark"));

            // 灯笼
            BuildLantern(parent, new Vector3(-6f, 0f, 4f));
            BuildLantern(parent, new Vector3(6f, 0f, 4f));
            BuildLantern(parent, new Vector3(-12f, 0f, -2f));
            BuildLantern(parent, new Vector3(12f, 0f, -1f));
            BuildLantern(parent, new Vector3(0f, 0f, -13f));
            BuildLantern(parent, new Vector3(-20f, 0f, 4f));
            BuildLantern(parent, new Vector3(20f, 0f, 4f));

            // 寨门
            var gate = new GameObject("Gate");
            gate.transform.SetParent(parent, false);
            gate.transform.position = new Vector3(0f, 0f, -17f);
            Cube(gate.transform, "PostL", new Vector3(-2.4f, 2f, 0f), new Vector3(0.5f, 4f, 0.5f), M("woodDark"), true).layer = Layer("Wall");
            Cube(gate.transform, "PostR", new Vector3(2.4f, 2f, 0f), new Vector3(0.5f, 4f, 0.5f), M("woodDark"), true).layer = Layer("Wall");
            Cube(gate.transform, "Lintel", new Vector3(0f, 3.9f, 0f), new Vector3(5.6f, 0.5f, 0.6f), M("roof"));
            Cube(gate.transform, "Banner", new Vector3(0f, 2.9f, 0f), new Vector3(1.6f, 1.4f, 0.15f), M("red"));

            // 障碍物（可阻挡）
            BuildCrate(parent, new Vector3(-8f, 0.6f, -2f), 1.3f);
            BuildCrate(parent, new Vector3(8f, 0.6f, -4f), 1.2f);
            BuildCrate(parent, new Vector3(-3f, 0.6f, -8f), 1.4f);
            BuildCrate(parent, new Vector3(11f, 0.6f, 2f), 1.1f);
            BuildCrate(parent, new Vector3(-14f, 0.6f, -4f), 1.2f);
            BuildBarrel(parent, new Vector3(4.5f, 0.5f, -3f));
            BuildBarrel(parent, new Vector3(-12f, 0.5f, 6f));
            BuildBarrel(parent, new Vector3(14f, 0.5f, 6f));
        }

        private static void BuildLantern(Transform parent, Vector3 position)
        {
            var go = new GameObject("Lantern");
            go.transform.SetParent(parent, false);
            go.transform.position = position;

            Cube(go.transform, "Post", new Vector3(0f, 1.2f, 0f), new Vector3(0.2f, 2.4f, 0.2f), M("woodDark"), true).layer = Layer("Wall");
            Cube(go.transform, "Head", new Vector3(0f, 2.65f, 0f), new Vector3(0.6f, 0.6f, 0.6f), M("glow"));
        }

        private static void BuildCrate(Transform parent, Vector3 position, float size)
        {
            var go = Cube(parent, "Crate", position, new Vector3(size, size, size), M("wood"), true);
            go.layer = Layer("Wall");
        }

        private static void BuildBarrel(Transform parent, Vector3 position)
        {
            var go = Cube(parent, "Barrel", position, new Vector3(0.7f, 1.0f, 0.7f), M("woodDark"), true);
            go.layer = Layer("Wall");
        }

        // ---------------- 玩家 ----------------

        private static GameObject BuildPlayer(Transform parent)
        {
            var go = new GameObject("Player", typeof(Rigidbody), typeof(CapsuleCollider),
                typeof(PlayerController), typeof(Slingshot));
            go.transform.SetParent(parent, false);
            go.transform.position = new Vector3(0f, 0.5f, 0f);
            go.layer = Layer("Player");

            var rb = go.GetComponent<Rigidbody>();
            rb.useGravity = false;
            rb.isKinematic = false;
            rb.constraints = RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePositionY;
            rb.interpolation = RigidbodyInterpolation.Interpolate;
            rb.collisionDetectionMode = CollisionDetectionMode.Continuous;

            var capsule = go.GetComponent<CapsuleCollider>();
            // 胶囊底部必须离地：脚底在 y=0，胶囊底 y=0.05，避免卡进地面（Y 轴锁定后无法自动脱离）
            capsule.center = new Vector3(0f, 0.4f, 0f);
            capsule.height = 1.7f;
            capsule.radius = 0.35f;

            var model = BuildPlayerModel(go.transform);

            var controller = go.GetComponent<PlayerController>();
            controller.config = AssetDatabase.LoadAssetAtPath<GameConfig>(Data + "GameConfig.asset");
            controller.model = model;
            controller.slingshot = go.GetComponent<Slingshot>();

            var slingshot = go.GetComponent<Slingshot>();
            slingshot.config = controller.config;
            slingshot.muzzle = model;
            slingshot.pool = BuildProjectilePool(parent);

            return go;
        }

        private static Transform BuildPlayerModel(Transform parent)
        {
            var model = new GameObject("Model");
            model.transform.SetParent(parent, false);
            var t = model.transform;

            // ---- 腿（带髋部枢轴，走路时前后摆） ----
            var legL = new GameObject("LegPivotL").transform;
            legL.SetParent(t, false);
            legL.localPosition = new Vector3(-0.18f, 0.06f, 0f);
            BuildLeg(legL);

            var legR = new GameObject("LegPivotR").transform;
            legR.SetParent(t, false);
            legR.localPosition = new Vector3(0.18f, 0.06f, 0f);
            BuildLeg(legR);

            // ---- 工装短裤 ----
            Cube(t, "ShortsL", new Vector3(-0.18f, -0.08f, 0f), new Vector3(0.3f, 0.34f, 0.34f), M("wood"));
            Cube(t, "ShortsR", new Vector3(0.18f, -0.08f, 0f), new Vector3(0.3f, 0.34f, 0.34f), M("wood"));
            Cube(t, "PocketL", new Vector3(-0.34f, -0.1f, 0.02f), new Vector3(0.06f, 0.16f, 0.2f), M("woodDark"));
            Cube(t, "PocketR", new Vector3(0.34f, -0.1f, 0.02f), new Vector3(0.06f, 0.16f, 0.2f), M("woodDark"));

            // ---- 腰带 ----
            Cube(t, "Belt", new Vector3(0f, 0.09f, 0f), new Vector3(0.66f, 0.1f, 0.38f), M("woodDark"));
            Cube(t, "Buckle", new Vector3(0f, 0.09f, 0.2f), new Vector3(0.12f, 0.09f, 0.04f), M("gold"));

            // ---- 白石子弹袋 ----
            Cube(t, "Pouch", new Vector3(-0.38f, 0.02f, 0.06f), new Vector3(0.18f, 0.24f, 0.16f), M("dog"));
            Cube(t, "PouchStrap", new Vector3(-0.3f, 0.14f, 0.02f), new Vector3(0.24f, 0.05f, 0.06f), M("woodDark"));
            Cube(t, "Stone1", new Vector3(-0.4f, 0.16f, 0.08f), new Vector3(0.07f, 0.07f, 0.07f), M("stoneMid"));
            Cube(t, "Stone2", new Vector3(-0.33f, 0.17f, 0.05f), new Vector3(0.07f, 0.07f, 0.07f), M("stoneMid"));
            Cube(t, "Stone3", new Vector3(-0.38f, 0.19f, -0.02f), new Vector3(0.06f, 0.06f, 0.06f), M("stoneMid"));

            // ---- 白背心 + 牛仔外套 ----
            Cube(t, "Tank", new Vector3(0f, 0.38f, 0f), new Vector3(0.46f, 0.46f, 0.34f), M("ivory"));
            Cube(t, "JacketL", new Vector3(-0.31f, 0.38f, 0f), new Vector3(0.2f, 0.52f, 0.4f), M("shirt"));
            Cube(t, "JacketR", new Vector3(0.31f, 0.38f, 0f), new Vector3(0.2f, 0.52f, 0.4f), M("shirt"));
            Cube(t, "JacketBack", new Vector3(0f, 0.38f, -0.19f), new Vector3(0.66f, 0.52f, 0.06f), M("shirt"));
            Cube(t, "CollarL", new Vector3(-0.17f, 0.63f, 0.05f), new Vector3(0.2f, 0.1f, 0.34f), M("shirt"));
            Cube(t, "CollarR", new Vector3(0.17f, 0.63f, 0.05f), new Vector3(0.2f, 0.1f, 0.34f), M("shirt"));
            Cube(t, "ShoulderL", new Vector3(-0.4f, 0.56f, 0f), new Vector3(0.16f, 0.16f, 0.3f), M("shirt"));
            Cube(t, "ShoulderR", new Vector3(0.4f, 0.56f, 0f), new Vector3(0.16f, 0.16f, 0.3f), M("shirt"));

            // ---- 手臂：左手持弓前伸，右臂拉弓 ----
            Cube(t, "ArmL", new Vector3(-0.42f, 0.34f, 0.1f), new Vector3(0.16f, 0.16f, 0.44f), M("skin"));
            Cube(t, "SleeveL", new Vector3(-0.42f, 0.36f, -0.04f), new Vector3(0.18f, 0.18f, 0.18f), M("shirt"));
            Cube(t, "HandL", new Vector3(-0.42f, 0.34f, 0.36f), new Vector3(0.16f, 0.16f, 0.16f), M("skin"));

            Cube(t, "ArmR", new Vector3(0.4f, 0.42f, 0.12f), new Vector3(0.16f, 0.16f, 0.34f), M("skin"));
            Cube(t, "SleeveR", new Vector3(0.4f, 0.48f, 0.06f), new Vector3(0.18f, 0.18f, 0.18f), M("ivory"));
            Cube(t, "Bandage1", new Vector3(0.4f, 0.42f, 0.24f), new Vector3(0.18f, 0.18f, 0.08f), M("ivory"));
            Cube(t, "Bandage2", new Vector3(0.4f, 0.42f, 0.32f), new Vector3(0.17f, 0.17f, 0.06f), M("ivory"));
            Cube(t, "HandR", new Vector3(0.33f, 0.46f, 0.32f), new Vector3(0.14f, 0.14f, 0.14f), M("skin"));

            // ---- 弹弓（木叉 + 皮筋） ----
            Cube(t, "SlingGrip", new Vector3(-0.42f, 0.42f, 0.44f), new Vector3(0.08f, 0.26f, 0.08f), M("wood"));
            CubeRot(t, "SlingProngL", new Vector3(-0.5f, 0.62f, 0.44f), new Vector3(0.07f, 0.24f, 0.07f), new Vector3(0f, 0f, 22f), M("wood"));
            CubeRot(t, "SlingProngR", new Vector3(-0.34f, 0.62f, 0.44f), new Vector3(0.07f, 0.24f, 0.07f), new Vector3(0f, 0f, -22f), M("wood"));
            CubeRot(t, "Band1", new Vector3(-0.34f, 0.53f, 0.34f), new Vector3(0.025f, 0.025f, 0.28f), new Vector3(0f, 20f, 0f), M("woodDark"));
            CubeRot(t, "Band2", new Vector3(-0.3f, 0.5f, 0.33f), new Vector3(0.025f, 0.025f, 0.28f), new Vector3(0f, 12f, 0f), M("woodDark"));

            // ---- 头 ----
            Cube(t, "Head", new Vector3(0f, 0.9f, 0f), new Vector3(0.56f, 0.54f, 0.5f), M("skin"));
            Cube(t, "EarL", new Vector3(-0.31f, 0.9f, 0f), new Vector3(0.08f, 0.14f, 0.1f), M("skin"));
            Cube(t, "EarR", new Vector3(0.31f, 0.9f, 0f), new Vector3(0.08f, 0.14f, 0.1f), M("skin"));

            // ---- 头发（乱翘） ----
            Cube(t, "HairBack", new Vector3(0f, 1.05f, -0.08f), new Vector3(0.64f, 0.24f, 0.52f), M("hair"));
            CubeRot(t, "HairSpike1", new Vector3(0.12f, 1.26f, 0.02f), new Vector3(0.13f, 0.3f, 0.13f), new Vector3(8f, 0f, -28f), M("hair"));
            CubeRot(t, "HairSpike2", new Vector3(-0.14f, 1.28f, -0.04f), new Vector3(0.12f, 0.32f, 0.12f), new Vector3(-6f, 0f, 24f), M("hair"));
            CubeRot(t, "HairSpike3", new Vector3(0.26f, 1.18f, -0.16f), new Vector3(0.12f, 0.26f, 0.12f), new Vector3(32f, 0f, -42f), M("hair"));
            CubeRot(t, "HairSpike4", new Vector3(-0.27f, 1.16f, 0.12f), new Vector3(0.12f, 0.26f, 0.12f), new Vector3(-24f, 0f, 36f), M("hair"));
            CubeRot(t, "HairSpike5", new Vector3(0.02f, 1.3f, -0.2f), new Vector3(0.12f, 0.28f, 0.12f), new Vector3(26f, 0f, 0f), M("hair"));
            Cube(t, "SideburnL", new Vector3(-0.29f, 0.98f, 0.14f), new Vector3(0.05f, 0.22f, 0.18f), M("hair"));
            Cube(t, "SideburnR", new Vector3(0.29f, 0.98f, 0.14f), new Vector3(0.05f, 0.22f, 0.18f), M("hair"));

            // ---- 额头护目镜 ----
            Cube(t, "GoggleStrap", new Vector3(0f, 1.13f, 0.01f), new Vector3(0.62f, 0.09f, 0.56f), M("woodDark"));
            Cube(t, "GoggleRimL", new Vector3(-0.13f, 1.13f, 0.27f), new Vector3(0.2f, 0.16f, 0.06f), M("ivory"));
            Cube(t, "GoggleRimR", new Vector3(0.13f, 1.13f, 0.27f), new Vector3(0.2f, 0.16f, 0.06f), M("ivory"));
            Cube(t, "GoggleLensL", new Vector3(-0.13f, 1.13f, 0.3f), new Vector3(0.14f, 0.12f, 0.03f), M("water"));
            Cube(t, "GoggleLensR", new Vector3(0.13f, 1.13f, 0.3f), new Vector3(0.14f, 0.12f, 0.03f), M("water"));

            // ---- 五官 ----
            Cube(t, "EyeL", new Vector3(-0.13f, 0.92f, 0.255f), new Vector3(0.09f, 0.09f, 0.03f), M("hair"));
            Cube(t, "EyeR", new Vector3(0.13f, 0.92f, 0.255f), new Vector3(0.09f, 0.09f, 0.03f), M("hair"));
            CubeRot(t, "BrowL", new Vector3(-0.13f, 0.99f, 0.26f), new Vector3(0.13f, 0.035f, 0.02f), new Vector3(0f, 0f, 12f), M("hair"));
            CubeRot(t, "BrowR", new Vector3(0.13f, 0.99f, 0.26f), new Vector3(0.13f, 0.035f, 0.02f), new Vector3(0f, 0f, -12f), M("hair"));
            Cube(t, "Mouth", new Vector3(0.02f, 0.79f, 0.255f), new Vector3(0.13f, 0.03f, 0.02f), M("woodDark"));
            Cube(t, "Smudge", new Vector3(0.2f, 0.82f, 0.255f), new Vector3(0.08f, 0.06f, 0.02f), M("stoneMid"));

            var walk = model.AddComponent<PlayerWalkAnimator>();
            walk.leftLeg = legL;
            walk.rightLeg = legR;
            walk.body = t;

            return model.transform;
        }

        private static void BuildLeg(Transform pivot)
        {
            Cube(pivot, "Leg", new Vector3(0f, -0.3f, 0f), new Vector3(0.17f, 0.36f, 0.17f), M("skin"));
            Cube(pivot, "Boot", new Vector3(0f, -0.42f, 0.02f), new Vector3(0.24f, 0.2f, 0.34f), M("hair"));
            Cube(pivot, "Lace", new Vector3(0f, -0.39f, 0.17f), new Vector3(0.16f, 0.04f, 0.05f), M("ivory"));
            Cube(pivot, "Sole", new Vector3(0f, -0.53f, 0.02f), new Vector3(0.28f, 0.06f, 0.4f), M("ivory"));
        }

        private static ProjectilePool BuildProjectilePool(Transform parent)
        {
            var poolGo = new GameObject("ProjectilePool", typeof(ProjectilePool));
            poolGo.transform.SetParent(parent, false);
            var pool = poolGo.GetComponent<ProjectilePool>();

            var template = new GameObject("ProjectileTemplate",
                typeof(Rigidbody), typeof(SphereCollider), typeof(Projectile));
            template.transform.SetParent(poolGo.transform, false);
            template.layer = Layer("Projectile");
            template.SetActive(false);

            Cube(template.transform, "Body", Vector3.zero, Vector3.one * 0.18f, M("stoneDark"));

            var rb = template.GetComponent<Rigidbody>();
            rb.useGravity = false;
            rb.collisionDetectionMode = CollisionDetectionMode.Continuous;

            var collider = template.GetComponent<SphereCollider>();
            collider.radius = 0.12f;
            collider.isTrigger = true;

            pool.template = template.GetComponent<Projectile>();
            pool.size = 64;
            return pool;
        }

        // ---------------- 敌人 ----------------

        private static EnemyPool BuildEnemyPool(Transform parent, HouseController house)
        {
            EnsureStats();

            var poolGo = new GameObject("EnemyPool", typeof(EnemyPool));
            poolGo.transform.SetParent(parent, false);
            var pool = poolGo.GetComponent<EnemyPool>();

            var template = new GameObject("EnemyTemplate",
                typeof(Rigidbody), typeof(CapsuleCollider), typeof(EnemyBase));
            template.transform.SetParent(poolGo.transform, false);
            template.transform.position = new Vector3(0f, 0.5f, -30f);
            template.layer = Layer("Enemy");

            var rb = template.GetComponent<Rigidbody>();
            rb.isKinematic = true;
            rb.useGravity = false;
            rb.constraints = RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePositionY;

            var collider = template.GetComponent<CapsuleCollider>();
            collider.center = Vector3.zero;
            collider.height = 0.9f;
            collider.radius = 0.35f;

            var modelRoot = new GameObject("Model");
            modelRoot.transform.SetParent(template.transform, false);

            var variants = new List<Transform>
            {
                BuildChicken(modelRoot.transform),
                BuildDog(modelRoot.transform),
                BuildBuffalo(modelRoot.transform),
                BuildBoar(modelRoot.transform)
            };

            var enemy = template.GetComponent<EnemyBase>();
            enemy.model = modelRoot.transform;
            enemy.modelVariants = variants.ToArray();
            enemy.house = house;
            enemy.pool = pool;

            template.SetActive(false);

            pool.template = enemy;
            pool.house = house;
            pool.size = 32;

            return pool;
        }

        private static EnemyStats[] EnsureStats()
        {
            var kid = EnsureOne("Enemy_Kid", "野鸡", 30, 1.6f, 8, 10, 1.0f, 0, 0.45f);
            var speeder = EnsureOne("Enemy_Speeder", "野狗", 18, 2.6f, 5, 15, 1.2f, 1, 0.55f);
            var fatty = EnsureOne("Enemy_Fatty", "水牛", 90, 1.0f, 15, 25, 0.8f, 2, 0.75f);
            var king = EnsureOne("Enemy_King", "野猪王", 320, 0.9f, 30, 100, 0.3f, 3, 0.85f);
            return new[] { kid, speeder, fatty, king };
        }

        private static EnemyStats EnsureOne(string assetName, string displayName, int hp, float speed,
            int damage, int score, float weight, int visualIndex, float radius)
        {
            var path = Data + assetName + ".asset";
            var stats = AssetDatabase.LoadAssetAtPath<EnemyStats>(path);
            if (stats == null)
            {
                stats = ScriptableObject.CreateInstance<EnemyStats>();
                AssetDatabase.CreateAsset(stats, path);
            }

            stats.displayName = displayName;
            stats.hp = hp;
            stats.speed = speed;
            stats.damageToHouse = damage;
            stats.scale = 1f;
            stats.score = score;
            stats.weight = weight;
            stats.tint = Color.white;
            stats.visualIndex = visualIndex;
            stats.colliderRadius = radius;

            EditorUtility.SetDirty(stats);
            return stats;
        }

        private static Transform BuildChicken(Transform parent)
        {
            var root = new GameObject("Chicken").transform;
            root.SetParent(parent, false);
            Cube(root, "Body", new Vector3(0f, 0f, 0f), new Vector3(0.9f, 0.5f, 0.6f), M("chicken"));
            Cube(root, "Head", new Vector3(0.42f, 0.3f, 0f), new Vector3(0.34f, 0.34f, 0.32f), M("chickenGreen"));
            Cube(root, "Comb", new Vector3(0.42f, 0.53f, 0f), new Vector3(0.16f, 0.16f, 0.12f), M("red"));
            Cube(root, "Beak", new Vector3(0.64f, 0.28f, 0f), new Vector3(0.2f, 0.1f, 0.1f), M("gold"));
            Cube(root, "Tail", new Vector3(-0.55f, 0.15f, 0f), new Vector3(0.45f, 0.3f, 0.22f), M("chickenGreen"));
            Cube(root, "LegL", new Vector3(-0.18f, -0.32f, 0.12f), new Vector3(0.08f, 0.3f, 0.08f), M("gold"));
            Cube(root, "LegR", new Vector3(0.18f, -0.32f, -0.12f), new Vector3(0.08f, 0.3f, 0.08f), M("gold"));
            return root;
        }

        private static Transform BuildDog(Transform parent)
        {
            var root = new GameObject("Dog").transform;
            root.SetParent(parent, false);
            Cube(root, "Body", Vector3.zero, new Vector3(1.15f, 0.5f, 0.5f), M("dog"));
            Cube(root, "Head", new Vector3(0.62f, 0.28f, 0f), new Vector3(0.42f, 0.42f, 0.38f), M("dog"));
            Cube(root, "Snout", new Vector3(0.88f, 0.2f, 0f), new Vector3(0.3f, 0.22f, 0.22f), M("dog"));
            Cube(root, "EarL", new Vector3(0.6f, 0.52f, 0.14f), new Vector3(0.12f, 0.2f, 0.12f), M("woodDark"));
            Cube(root, "EarR", new Vector3(0.6f, 0.52f, -0.14f), new Vector3(0.12f, 0.2f, 0.12f), M("woodDark"));
            Cube(root, "Tail", new Vector3(-0.66f, 0.22f, 0f), new Vector3(0.38f, 0.12f, 0.12f), M("dog"));
            BuildLegs(root, 0.4f, 0.24f, 0.12f, M("dog"));
            return root;
        }

        private static Transform BuildBuffalo(Transform parent)
        {
            var root = new GameObject("Buffalo").transform;
            root.SetParent(parent, false);
            Cube(root, "Body", Vector3.zero, new Vector3(1.55f, 0.75f, 0.75f), M("buffalo"));
            Cube(root, "Head", new Vector3(0.85f, 0.22f, 0f), new Vector3(0.62f, 0.62f, 0.58f), M("buffalo"));
            Cube(root, "HornL", new Vector3(0.95f, 0.58f, 0.28f), new Vector3(0.4f, 0.12f, 0.12f), M("ivory"));
            Cube(root, "HornR", new Vector3(0.95f, 0.58f, -0.28f), new Vector3(0.4f, 0.12f, 0.12f), M("ivory"));
            Cube(root, "Snout", new Vector3(1.16f, 0.1f, 0f), new Vector3(0.24f, 0.28f, 0.3f), M("stoneDark"));
            BuildLegs(root, 0.55f, 0.3f, 0.18f, M("buffalo"));
            return root;
        }

        private static Transform BuildBoar(Transform parent)
        {
            var root = new GameObject("Boar").transform;
            root.SetParent(parent, false);
            Cube(root, "Body", Vector3.zero, new Vector3(1.65f, 0.85f, 0.85f), M("boar"));
            Cube(root, "Head", new Vector3(0.92f, 0.1f, 0f), new Vector3(0.7f, 0.7f, 0.62f), M("boar"));
            Cube(root, "Snout", new Vector3(1.3f, 0f, 0f), new Vector3(0.28f, 0.3f, 0.3f), M("woodDark"));
            Cube(root, "TuskL", new Vector3(1.1f, -0.15f, 0.22f), new Vector3(0.1f, 0.26f, 0.08f), M("ivory"));
            Cube(root, "TuskR", new Vector3(1.1f, -0.15f, -0.22f), new Vector3(0.1f, 0.26f, 0.08f), M("ivory"));
            for (var i = 0; i < 4; i++)
                Cube(root, "Spike" + i, new Vector3(-0.5f + i * 0.35f, 0.52f, 0f), new Vector3(0.18f, 0.3f, 0.18f), M("woodDark"));
            Cube(root, "Tail", new Vector3(-0.9f, 0.2f, 0f), new Vector3(0.18f, 0.18f, 0.18f), M("woodDark"));
            BuildLegs(root, 0.6f, 0.32f, 0.2f, M("woodDark"));
            return root;
        }

        private static void BuildLegs(Transform root, float x, float z, float thickness, Material mat)
        {
            Cube(root, "LegFL", new Vector3(x, -0.42f, z), new Vector3(thickness, 0.38f, thickness), mat);
            Cube(root, "LegFR", new Vector3(x, -0.42f, -z), new Vector3(thickness, 0.38f, thickness), mat);
            Cube(root, "LegBL", new Vector3(-x, -0.42f, z), new Vector3(thickness, 0.38f, thickness), mat);
            Cube(root, "LegBR", new Vector3(-x, -0.42f, -z), new Vector3(thickness, 0.38f, thickness), mat);
        }

        // ---------------- 道具与波次 ----------------

        private static void BuildPickups(Transform parent, HouseController house, GameObject player)
        {
            var old = GameObject.Find("[Pickups]");
            if (old != null) Object.DestroyImmediate(old);

            var root = new GameObject("[Pickups]");
            root.transform.SetParent(parent, false);

            var buffGo = new GameObject("BuffSystem", typeof(BuffSystem));
            buffGo.transform.SetParent(root.transform, false);
            var buff = buffGo.GetComponent<BuffSystem>();
            buff.house = house;
            buff.maxBuffs = 3;

            var spawnerGo = new GameObject("PickupSpawner", typeof(PickupSpawner));
            spawnerGo.transform.SetParent(root.transform, false);
            var spawner = spawnerGo.GetComponent<PickupSpawner>();

            var template = new GameObject("PickupTemplate", typeof(PickupItem), typeof(SphereCollider));
            template.transform.SetParent(spawnerGo.transform, false);
            template.transform.position = new Vector3(0f, 0.5f, 30f);
            template.layer = Layer("Pickup");
            template.SetActive(false);

            var trigger = template.GetComponent<SphereCollider>();
            trigger.isTrigger = true;
            trigger.radius = 0.6f;

            var body = Cube(template.transform, "Body", new Vector3(0f, 0.35f, 0f), Vector3.one * 0.45f, M("stoneLight"));
            var item = template.GetComponent<PickupItem>();
            item.body = body.transform;
            item.bodyRenderer = body.GetComponent<Renderer>();
            item.lifetime = 15f;

            var configs = new List<PickupConfig>();
            foreach (var name in new[] { "Pickup_Bubblegum", "Pickup_TinFrog", "Pickup_Marble", "Pickup_Latiao" })
            {
                var config = AssetDatabase.LoadAssetAtPath<PickupConfig>(Data + name + ".asset");
                if (config != null) configs.Add(config);
            }

            spawner.configs = configs.ToArray();
            spawner.template = item;
            spawner.house = house;
            spawner.player = player.transform;
            spawner.maxOnField = 2;
        }

        private static void BuildWaves(Transform parent, EnemyPool pool, HouseController house)
        {
            var old = GameObject.Find("[Waves]");
            if (old != null) Object.DestroyImmediate(old);

            var root = new GameObject("[Waves]");
            root.transform.SetParent(parent, false);

            var spawns = new[]
            {
                Spawn(root.transform, "Spawn_1", new Vector3(-24f, 0.5f, -12f)),
                Spawn(root.transform, "Spawn_2", new Vector3(24f, 0.5f, -12f)),
                Spawn(root.transform, "Spawn_3", new Vector3(-21f, 0.5f, 6f)),
                Spawn(root.transform, "Spawn_4", new Vector3(21f, 0.5f, 6f)),
                Spawn(root.transform, "Spawn_5", new Vector3(0f, 0.5f, -19f))
            };

            var managerGo = new GameObject("WaveManager", typeof(WaveManager));
            managerGo.transform.SetParent(root.transform, false);
            var manager = managerGo.GetComponent<WaveManager>();
            manager.table = AssetDatabase.LoadAssetAtPath<WaveTable>(Data + "WaveTable.asset");
            manager.pool = pool;
            manager.house = house;
            manager.spawnPoints = spawns;
            manager.autoStart = true;
        }

        private static Transform Spawn(Transform parent, string name, Vector3 position)
        {
            var go = new GameObject(name, typeof(SpawnPoint));
            go.transform.SetParent(parent, false);
            go.transform.position = position;
            return go.transform;
        }

        // ---------------- UI 与服务 ----------------

        private static void BuildUI()
        {
            new GameObject("EventSystem", typeof(UnityEngine.EventSystems.EventSystem),
                typeof(UnityEngine.EventSystems.StandaloneInputModule));

            var uiGo = new GameObject("[UI]", typeof(UIManager));
            var ui = uiGo.GetComponent<UIManager>();
            ui.theme = AssetDatabase.LoadAssetAtPath<UITheme>(Data + "UITheme.asset");
        }

        private static void BuildServices()
        {
            new GameObject("[AudioManager]", typeof(AudioManager));
            new GameObject("[HitStop]", typeof(HitStop));
        }

        // ---------------- 工具 ----------------

        private static GameObject Cube(Transform parent, string name, Vector3 localPosition, Vector3 size,
            Material material, bool keepCollider = false)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPosition;
            go.transform.localScale = size;

            var renderer = go.GetComponent<Renderer>();
            renderer.sharedMaterial = material;

            var collider = go.GetComponent<BoxCollider>();
            if (!keepCollider) Object.DestroyImmediate(collider);

            return go;
        }

        private static GameObject CubeRot(Transform parent, string name, Vector3 localPosition, Vector3 size,
            Vector3 euler, Material material)
        {
            var go = Cube(parent, name, localPosition, size, material);
            go.transform.localEulerAngles = euler;
            return go;
        }

        private static int Layer(string name)
        {
            var layer = LayerMask.NameToLayer(name);
            return layer < 0 ? 0 : layer;
        }

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;
            var parts = path.Split('/');
            var current = parts[0];
            for (var i = 1; i < parts.Length; i++)
            {
                var next = current + "/" + parts[i];
                if (!AssetDatabase.IsValidFolder(next)) AssetDatabase.CreateFolder(current, parts[i]);
                current = next;
            }
        }

        private static void AddToBuildSettings()
        {
            var scenes = new[]
            {
                new EditorBuildSettingsScene("Assets/_Game/Scenes/Boot.unity", true),
                new EditorBuildSettingsScene("Assets/_Game/Scenes/MainMenu.unity", true),
                new EditorBuildSettingsScene(ScenePath, true)
            };
            EditorBuildSettings.scenes = scenes;
        }

        private static string PrepareUnsavedScene()
        {
            var active = SceneManager.GetActiveScene();
            if (!string.IsNullOrEmpty(active.path)) return null;

            const string tempPath = "Assets/_Game/Scenes/_3DTemp.unity";
            EditorSceneManager.SaveScene(active, tempPath, false);
            return tempPath;
        }
    }
}
