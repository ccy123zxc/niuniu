using System.IO;
using OldAlley.Core;
using UnityEditor;
using UnityEngine;

namespace OldAlley.EditorTools
{
    /// <summary>
    /// Stage 2：用代码画 22 张占位像素图，并生成 ArtLibrary.asset。
    /// 只使用 UITheme 的 8 色调色板。
    /// </summary>
    public static class PlaceholderArtGenerator
    {
        private const string Dir = "Assets/_Game/Art/Generated";
        private const string LibPath = "Assets/_Game/Data/ArtLibrary.asset";

        private static readonly Color Clear = new Color(0f, 0f, 0f, 0f);
        private static readonly Color Paper = new Color32(242, 232, 207, 255);
        private static readonly Color Wood = new Color32(140, 74, 50, 255);
        private static readonly Color Outline = new Color32(58, 36, 24, 255);
        private static readonly Color Ink = new Color32(46, 33, 24, 255);
        private static readonly Color Red = new Color32(201, 79, 61, 255);
        private static readonly Color Green = new Color32(110, 139, 74, 255);
        private static readonly Color Sky = new Color32(143, 163, 173, 255);

        // ---- 电影感扩展色板（应「参考图风格」新增；UI 仍用原 8 色）----
        private static readonly Color Haze = new Color32(232, 226, 208, 255);
        private static readonly Color StoneLight = new Color32(176, 174, 164, 255);
        private static readonly Color StoneMid = new Color32(140, 138, 130, 255);
        private static readonly Color StoneDark = new Color32(84, 82, 77, 255);
        private static readonly Color Moss = new Color32(85, 112, 58, 255);
        private static readonly Color LeafDark = new Color32(63, 90, 46, 255);
        private static readonly Color LeafLight = new Color32(143, 169, 107, 255);
        private static readonly Color Bark = new Color32(90, 62, 46, 255);
        private static readonly Color BarkDark = new Color32(52, 36, 27, 255);
        private static readonly Color Blossom = new Color32(217, 139, 160, 255);
        private static readonly Color BlossomDeep = new Color32(196, 107, 132, 255);
        private static readonly Color Gold = new Color32(217, 164, 65, 255);
        private static readonly Color DeepRed = new Color32(176, 58, 46, 255);

        // ---- 野物版角色色 ----
        private static readonly Color Skin = new Color32(242, 205, 170, 255);
        private static readonly Color ShirtBlue = new Color32(62, 98, 156, 255);
        private static readonly Color ShirtBlueDark = new Color32(44, 72, 120, 255);
        private static readonly Color ShortsNavy = new Color32(44, 50, 64, 255);
        private static readonly Color HairBlack = new Color32(32, 28, 26, 255);
        private static readonly Color ChickenBrown = new Color32(176, 118, 70, 255);
        private static readonly Color ChickenWing = new Color32(198, 142, 90, 255);
        private static readonly Color ChickenGreen = new Color32(58, 110, 96, 255);
        private static readonly Color CombRed = new Color32(200, 60, 50, 255);
        private static readonly Color DogTan = new Color32(150, 130, 110, 255);
        private static readonly Color DogLight = new Color32(188, 168, 148, 255);
        private static readonly Color BuffaloGray = new Color32(92, 90, 88, 255);
        private static readonly Color BuffaloLight = new Color32(122, 120, 116, 255);
        private static readonly Color Horn = new Color32(214, 206, 188, 255);
        private static readonly Color BoarBrown = new Color32(112, 80, 60, 255);
        private static readonly Color BoarDark = new Color32(84, 58, 44, 255);
        private static readonly Color Ivory = new Color32(238, 232, 214, 255);
        private static readonly Color BeakGold = new Color32(224, 176, 72, 255);

        // ---- 26×26 等距地图色 ----
        private static readonly Color GrassBase = new Color32(108, 134, 76, 255);
        private static readonly Color GrassAlt = new Color32(124, 150, 88, 255);
        private static readonly Color PaddyBase = new Color32(150, 160, 92, 255);
        private static readonly Color PaddyAlt = new Color32(128, 142, 78, 255);
        private static readonly Color DirtPath = new Color32(168, 140, 100, 255);
        private static readonly Color StonePath = new Color32(168, 166, 156, 255);
        private static readonly Color PathEdge = new Color32(120, 104, 80, 255);
        private static readonly Color Water = new Color32(112, 150, 170, 255);
        private static readonly Color WaterLight = new Color32(148, 182, 196, 255);

        [MenuItem("旧巷守卫/2-生成占位美术")]
        public static void Run()
        {
            var absDir = Path.Combine(Application.dataPath, "_Game/Art/Generated");
            Directory.CreateDirectory(absDir);

            SaveTex(BrickWall(), "brick_wall");
            SaveTex(ConcreteFloor(), "concrete_floor");
            SaveTex(RoofTile(), "roof_tile");
            SaveTex(RoofFar(), "roof_far");
            SaveTex(Door(), "door");
            SaveTex(Window(), "window");
            SaveTex(PlaneTree(), "plane_tree");
            SaveTex(UtilityPole(), "utility_pole");
            SaveTex(StreetLamp(), "street_lamp");
            SaveTex(TrashCan(), "trash_can");
            SaveTex(Bicycle(), "bicycle");
            SaveTex(OldHouse(), "old_house");
            SaveTex(Kid(0), "kid_a");
            SaveTex(Kid(1), "kid_b");
            SaveTex(Speeder(), "speeder");
            SaveTex(Fatty(), "fatty");
            SaveTex(King(), "king");
            SaveTex(Slingshot(), "slingshot");
            SaveTex(Stone(), "stone");
            SaveTex(Bubblegum(), "bubblegum");
            SaveTex(TinFrog(), "tin_frog");
            SaveTex(Marble(), "marble");
            SaveTex(Latiao(), "latiao");

            // ---- 2.5D 山区乡村老巷 ----
            SaveTex(Mountain(96, 32, Sky, 16f, 9f, 0.17f), "mountain_far");
            SaveTex(Mountain(96, 32, Green, 11f, 7f, 0.13f), "mountain_mid");
            SaveTex(Cloud(), "cloud");
            SaveTex(StoneWall(), "stone_wall");
            SaveTex(Cobblestone(), "cobblestone");
            SaveTex(TileRoof(), "tile_roof");
            SaveTex(RuralHouseBig(), "rural_house_big");
            SaveTex(RuralHouseSmall(), "rural_house_small");
            SaveTex(Well(), "well");
            SaveTex(CornRack(), "corn_rack");
            SaveTex(Lantern(), "lantern");
            SaveTex(Laundry(), "laundry");
            SaveTex(StoneStep(), "stone_step");
            SaveTex(Bamboo(), "bamboo");
            SaveTex(Banyan(), "banyan");
            SaveTex(Grass(), "grass");
            SaveTex(ShadowSprite(), "shadow");

            // ---- 电影感构图素材 ----
            SaveTex(TreeForeground(), "tree_foreground");
            SaveTex(BranchOverhead(), "branch_overhead");
            SaveTex(PagodaFar(), "pagoda_far");
            SaveTex(TempleRoof(), "temple_roof");
            SaveTex(StoneLantern(), "stone_lantern");
            SaveTex(RockCluster(), "rock_cluster");
            SaveTex(BambooGrove(), "bamboo_grove");
            SaveTex(MistBand(), "mist_band");
            SaveTex(Petal(), "petal");
            SaveTex(LightShaft(), "light_shaft");

            // ---- 野物版角色 ----
            SaveTex(PlayerBoy(), "player_boy");
            SaveTex(WildChicken(), "wild_chicken");
            SaveTex(WildDog(), "wild_dog");
            SaveTex(WaterBuffalo(), "water_buffalo");
            SaveTex(WildBoarKing(), "wild_boar_king");

            AssetDatabase.Refresh();
            BuildLibrary();
            Debug.Log("[Stage2] 占位美术生成完成（39 张，含 2.5D 山区素材）");
        }

        // ================= 环境 =================

        private static Texture2D BrickWall()
        {
            var t = New(16, 16);
            R(t, 0, 0, 16, 16, Wood);
            for (var x = 0; x < 16; x++)
            {
                P(t, x, 0, Outline);
                P(t, x, 8, Outline);
            }
            for (var y = 1; y < 8; y++) P(t, 0, y, Outline);
            for (var y = 9; y < 16; y++) P(t, 8, y, Outline);
            return t;
        }

        private static Texture2D ConcreteFloor()
        {
            var t = New(16, 16);
            R(t, 0, 0, 16, 16, Sky);
            for (var i = 0; i < 10; i++) P(t, (i * 5 + 3) % 16, (i * 7 + 2) % 16, Paper);
            for (var x = 0; x < 16; x++) P(t, x, 0, Wood);
            return t;
        }

        private static Texture2D RoofTile()
        {
            var t = New(16, 16);
            R(t, 0, 0, 16, 16, Outline);
            for (var y = 2; y < 16; y += 4) R(t, 0, y, 16, 2, Wood);
            for (var x = 2; x < 16; x += 4) R(t, x, 0, 1, 16, Outline);
            return t;
        }

        private static Texture2D RoofFar()
        {
            var t = New(32, 16);
            // 高低错落的屋顶剪影
            R(t, 0, 0, 12, 10, Outline);
            R(t, 12, 0, 8, 14, Outline);
            R(t, 20, 0, 12, 8, Outline);
            R(t, 2, 10, 8, 2, Outline);
            R(t, 22, 8, 8, 2, Outline);
            return t;
        }

        // ================= 道具 =================

        private static Texture2D Door()
        {
            var t = New(32, 48);
            R(t, 0, 0, 32, 48, Wood);
            Border(t, Outline);
            R(t, 5, 5, 22, 38, Paper);
            R(t, 8, 12, 16, 1, Outline);
            R(t, 8, 32, 16, 1, Outline);
            Ellipse(t, 25, 22, 2, 2, Red);
            return t;
        }

        private static Texture2D Window()
        {
            var t = New(16, 16);
            R(t, 0, 0, 16, 16, Wood);
            Border(t, Outline);
            R(t, 3, 3, 10, 10, Sky);
            R(t, 7, 3, 2, 10, Wood);
            R(t, 3, 7, 10, 2, Wood);
            return t;
        }

        private static Texture2D PlaneTree()
        {
            var t = New(32, 48);
            R(t, 13, 0, 6, 22, Wood);
            R(t, 12, 0, 1, 22, Outline);
            R(t, 19, 0, 1, 22, Outline);
            Ellipse(t, 16, 33, 15, 14, Outline);
            Ellipse(t, 16, 33, 13, 12, Green);
            Ellipse(t, 12, 38, 6, 5, Green);
            return t;
        }

        private static Texture2D UtilityPole()
        {
            var t = New(16, 64);
            R(t, 6, 0, 4, 64, Outline);
            R(t, 1, 52, 14, 3, Outline);
            R(t, 1, 60, 14, 1, Outline);
            return t;
        }

        private static Texture2D StreetLamp()
        {
            var t = New(16, 48);
            R(t, 7, 0, 3, 40, Outline);
            R(t, 3, 38, 10, 9, Paper);
            R(t, 2, 37, 12, 1, Outline);
            R(t, 2, 47, 12, 1, Outline);
            R(t, 2, 38, 1, 9, Outline);
            R(t, 13, 38, 1, 9, Outline);
            return t;
        }

        private static Texture2D TrashCan()
        {
            var t = New(16, 16);
            R(t, 2, 0, 12, 12, Wood);
            R(t, 1, 11, 14, 3, Outline);
            R(t, 2, 0, 12, 1, Outline);
            R(t, 2, 0, 1, 12, Outline);
            R(t, 13, 0, 1, 12, Outline);
            R(t, 6, 4, 1, 6, Outline);
            R(t, 10, 4, 1, 6, Outline);
            return t;
        }

        private static Texture2D Bicycle()
        {
            var t = New(24, 16);
            Ellipse(t, 5, 4, 4, 4, Outline);
            Ellipse(t, 5, 4, 2, 2, Clear);
            Ellipse(t, 18, 4, 4, 4, Outline);
            Ellipse(t, 18, 4, 2, 2, Clear);
            for (var i = 0; i < 8; i++) P(t, 5 + i, 4 + i / 2, Outline);
            for (var i = 0; i < 6; i++) P(t, 18 - i, 4 + i / 2, Outline);
            R(t, 11, 10, 3, 1, Outline);
            return t;
        }

        private static Texture2D OldHouse()
        {
            var t = New(96, 80);
            // 墙体
            R(t, 2, 0, 92, 56, Paper);
            Border(t, Outline);
            // 屋顶（上窄下宽的梯形）
            for (var i = 0; i < 26; i++)
            {
                var y = 54 + i;
                var inset = i;
                R(t, 2 + inset, y, 92 - inset * 2, 1, Red);
                P(t, 2 + inset, y, Outline);
                P(t, 93 - inset, y, Outline);
            }
            R(t, 27, 79, 42, 1, Outline);
            // 门
            R(t, 40, 2, 16, 32, Wood);
            R(t, 40, 2, 16, 1, Outline);
            R(t, 40, 33, 16, 1, Outline);
            R(t, 40, 2, 1, 32, Outline);
            R(t, 55, 2, 1, 32, Outline);
            Ellipse(t, 52, 16, 2, 2, Red);
            // 窗
            R(t, 14, 28, 16, 16, Sky);
            R(t, 14, 28, 16, 1, Outline);
            R(t, 14, 43, 16, 1, Outline);
            R(t, 14, 28, 1, 16, Outline);
            R(t, 29, 28, 1, 16, Outline);
            R(t, 66, 28, 16, 16, Sky);
            R(t, 66, 28, 16, 1, Outline);
            R(t, 66, 43, 16, 1, Outline);
            R(t, 66, 28, 1, 16, Outline);
            R(t, 81, 28, 1, 16, Outline);
            return t;
        }

        // ================= 角色 =================

        private static Texture2D Kid(int frame)
        {
            var t = New(16, 24);
            // 头
            R(t, 5, 15, 7, 7, Paper);
            R(t, 5, 21, 7, 2, Ink);
            P(t, 9, 19, Ink);
            P(t, 7, 19, Ink);
            // 身体
            R(t, 4, 6, 8, 9, Green);
            // 腿
            if (frame == 0)
            {
                R(t, 4, 0, 3, 6, Ink);
                R(t, 9, 0, 3, 6, Ink);
            }
            else
            {
                R(t, 3, 0, 3, 6, Ink);
                R(t, 10, 0, 3, 6, Ink);
            }
            return t;
        }

        private static Texture2D Speeder()
        {
            var t = New(16, 24);
            R(t, 6, 16, 6, 6, Paper);
            R(t, 6, 21, 6, 1, Ink);
            P(t, 8, 19, Ink);
            R(t, 5, 7, 6, 9, Sky);
            R(t, 5, 0, 2, 7, Ink);
            R(t, 9, 0, 2, 7, Ink);
            return t;
        }

        private static Texture2D Fatty()
        {
            var t = New(20, 28);
            R(t, 6, 19, 8, 8, Paper);
            R(t, 6, 26, 8, 1, Ink);
            P(t, 8, 23, Ink);
            P(t, 11, 23, Ink);
            R(t, 3, 7, 14, 12, Red);
            R(t, 4, 0, 4, 7, Ink);
            R(t, 12, 0, 4, 7, Ink);
            return t;
        }

        private static Texture2D King()
        {
            var t = New(24, 32);
            R(t, 7, 22, 10, 8, Paper);
            // 王冠
            R(t, 6, 30, 12, 1, Red);
            P(t, 7, 31, Red);
            P(t, 12, 31, Red);
            P(t, 16, 31, Red);
            P(t, 9, 28, Ink);
            P(t, 14, 28, Ink);
            R(t, 3, 8, 18, 14, Wood);
            R(t, 4, 0, 5, 8, Ink);
            R(t, 15, 0, 5, 8, Ink);
            return t;
        }

        // ================= 武器与拾取物 =================

        private static Texture2D Slingshot()
        {
            var t = New(16, 16);
            R(t, 7, 0, 2, 8, Wood);
            R(t, 4, 5, 2, 8, Wood);
            R(t, 10, 5, 2, 8, Wood);
            R(t, 4, 12, 2, 2, Red);
            R(t, 10, 12, 2, 2, Red);
            return t;
        }

        private static Texture2D Stone()
        {
            var t = New(4, 4);
            R(t, 0, 0, 4, 4, Sky);
            P(t, 0, 0, Outline);
            P(t, 3, 3, Paper);
            return t;
        }

        private static Texture2D Bubblegum()
        {
            var t = New(12, 12);
            Ellipse(t, 6, 6, 5, 5, Red);
            P(t, 4, 8, Paper);
            P(t, 5, 8, Paper);
            return t;
        }

        private static Texture2D TinFrog()
        {
            var t = New(12, 12);
            R(t, 2, 2, 8, 6, Green);
            R(t, 1, 1, 2, 2, Green);
            R(t, 9, 1, 2, 2, Green);
            P(t, 3, 6, Ink);
            P(t, 8, 6, Ink);
            return t;
        }

        private static Texture2D Marble()
        {
            var t = New(12, 12);
            Ellipse(t, 6, 6, 5, 5, Sky);
            P(t, 4, 8, Paper);
            return t;
        }

        private static Texture2D Latiao()
        {
            var t = New(12, 12);
            R(t, 3, 1, 6, 10, Red);
            Border(t, Outline);
            R(t, 3, 8, 6, 2, Wood);
            return t;
        }

        // ================= 2.5D 山区乡村老巷 =================

        private static Texture2D Mountain(int w, int h, Color color, float baseH, float amp, float freq)
        {
            var t = New(w, h);
            for (var x = 0; x < w; x++)
            {
                var hh = baseH
                         + Mathf.Sin(x * freq) * amp
                         + Mathf.Sin(x * freq * 2.3f + 1.7f) * amp * 0.45f
                         + Mathf.Sin(x * freq * 5.1f + 0.4f) * amp * 0.2f;

                var top = Mathf.Clamp(Mathf.RoundToInt(hh), 0, h - 1);
                R(t, x, 0, 1, top + 1, color);
            }
            return t;
        }

        private static Texture2D Cloud()
        {
            var t = New(32, 12);
            Ellipse(t, 8, 6, 7, 4, Paper);
            Ellipse(t, 16, 7, 9, 5, Paper);
            Ellipse(t, 24, 5, 7, 4, Paper);
            return t;
        }

        private static Texture2D StoneWall()
        {
            var t = New(16, 16);
            R(t, 0, 0, 16, 16, Sky);
            for (var x = 0; x < 16; x++)
            {
                P(t, x, 0, Outline);
                P(t, x, 8, Outline);
            }
            for (var y = 1; y < 8; y++) P(t, 0, y, Outline);
            for (var y = 9; y < 16; y++) P(t, 8, y, Outline);
            P(t, 3, 4, Paper);
            P(t, 11, 12, Paper);
            P(t, 6, 11, Paper);
            return t;
        }

        private static Texture2D Cobblestone()
        {
            // 低对比、不规则的石板：以石板灰为底，零星石块与缝隙，避免棋盘格感。
            var t = New(16, 16);
            R(t, 0, 0, 16, 16, Sky);
            for (var gy = 0; gy < 4; gy++)
            for (var gx = 0; gx < 4; gx++)
            {
                var x = gx * 4;
                var y = gy * 4;
                if ((gx * 7 + gy * 11) % 3 == 0) R(t, x + 1, y + 1, 2, 2, Paper);
                if ((gx * 5 + gy * 3) % 4 == 0) P(t, x + 2, y, Outline);
                if ((gx * 3 + gy * 7) % 5 == 0) P(t, x, y + 2, Wood);
            }
            return t;
        }

        private static Texture2D TileRoof()
        {
            var t = New(16, 16);
            R(t, 0, 0, 16, 16, Outline);
            for (var y = 1; y < 16; y += 4) R(t, 0, y, 16, 2, Sky);
            for (var y = 1; y < 16; y += 4)
            for (var x = 2; x < 16; x += 4)
                R(t, x, y, 1, 2, Outline);
            return t;
        }

        private static Texture2D RuralHouseBig()
        {
            var t = New(112, 96);
            R(t, 2, 0, 108, 10, Sky);
            R(t, 2, 10, 108, 48, Paper);

            R(t, 2, 0, 108, 1, Outline);
            R(t, 2, 57, 108, 1, Outline);
            R(t, 2, 0, 1, 58, Outline);
            R(t, 109, 0, 1, 58, Outline);
            R(t, 2, 9, 108, 1, Outline);

            for (var i = 0; i < 38; i++)
            {
                var y = 58 + i;
                var inset = i;
                R(t, 2 + inset, y, 108 - inset * 2, 1, Outline);
                if (i % 4 == 1)
                    for (var xx = 2 + inset; xx < 110 - inset; xx += 4)
                        R(t, xx, y, 2, 1, Sky);
            }
            R(t, 40, 95, 32, 1, Outline);

            R(t, 50, 10, 14, 32, Wood);
            R(t, 50, 10, 14, 1, Outline);
            R(t, 50, 41, 14, 1, Outline);
            R(t, 50, 10, 1, 32, Outline);
            R(t, 63, 10, 1, 32, Outline);
            Ellipse(t, 61, 26, 2, 2, Red);

            R(t, 44, 12, 3, 28, Red);
            R(t, 68, 12, 3, 28, Red);

            WindowBox(t, 20, 30);
            WindowBox(t, 78, 30);

            Ellipse(t, 46, 47, 4, 5, Red);
            Ellipse(t, 71, 47, 4, 5, Red);

            R(t, 8, 10, 3, 48, Wood);
            R(t, 101, 10, 3, 48, Wood);
            return t;
        }

        private static Texture2D RuralHouseSmall()
        {
            var t = New(64, 64);
            R(t, 2, 0, 60, 6, Sky);
            R(t, 2, 6, 60, 27, Paper);

            R(t, 2, 0, 60, 1, Outline);
            R(t, 2, 32, 60, 1, Outline);
            R(t, 2, 0, 1, 33, Outline);
            R(t, 61, 0, 1, 33, Outline);
            R(t, 2, 5, 60, 1, Outline);

            for (var i = 0; i < 31; i++)
            {
                var y = 33 + i;
                var inset = i;
                R(t, 2 + inset, y, 60 - inset * 2, 1, Outline);
                if (i % 4 == 1)
                    for (var xx = 2 + inset; xx < 62 - inset; xx += 4)
                        R(t, xx, y, 2, 1, Sky);
            }
            R(t, 24, 63, 16, 1, Outline);

            R(t, 27, 6, 10, 20, Wood);
            R(t, 27, 6, 10, 1, Outline);
            R(t, 27, 25, 10, 1, Outline);
            R(t, 27, 6, 1, 20, Outline);
            R(t, 36, 6, 1, 20, Outline);

            R(t, 22, 8, 2, 18, Red);
            R(t, 42, 8, 2, 18, Red);

            WindowBox(t, 10, 16);
            WindowBox(t, 44, 16);
            return t;
        }

        private static void WindowBox(Texture2D t, int x, int y)
        {
            R(t, x, y, 16, 14, Sky);
            R(t, x, y, 16, 1, Outline);
            R(t, x, y + 13, 16, 1, Outline);
            R(t, x, y, 1, 14, Outline);
            R(t, x + 15, y, 1, 14, Outline);
            R(t, x + 7, y, 2, 14, Wood);
            R(t, x, y + 6, 16, 2, Wood);
        }

        private static Texture2D Well()
        {
            var t = New(24, 24);
            R(t, 4, 0, 16, 8, Sky);
            R(t, 4, 0, 16, 1, Outline);
            R(t, 4, 7, 16, 1, Outline);
            R(t, 4, 0, 1, 8, Outline);
            R(t, 19, 0, 1, 8, Outline);
            R(t, 5, 8, 2, 13, Wood);
            R(t, 17, 8, 2, 13, Wood);
            R(t, 2, 21, 20, 2, Outline);
            R(t, 2, 23, 20, 1, Outline);
            return t;
        }

        private static Texture2D CornRack()
        {
            var t = New(24, 32);
            R(t, 3, 0, 2, 30, Wood);
            R(t, 19, 0, 2, 30, Wood);
            R(t, 3, 22, 18, 2, Wood);
            R(t, 3, 12, 18, 2, Wood);
            R(t, 5, 24, 4, 6, Red);
            R(t, 10, 24, 4, 6, Green);
            R(t, 15, 24, 4, 6, Red);
            return t;
        }

        private static Texture2D Lantern()
        {
            var t = New(12, 16);
            Ellipse(t, 6, 8, 5, 6, Red);
            R(t, 5, 14, 2, 2, Outline);
            R(t, 5, 0, 2, 2, Outline);
            P(t, 6, 8, Paper);
            return t;
        }

        private static Texture2D Laundry()
        {
            var t = New(32, 16);
            R(t, 0, 12, 32, 1, Outline);
            R(t, 3, 4, 8, 8, Paper);
            R(t, 13, 5, 6, 7, Red);
            R(t, 21, 4, 8, 8, Sky);
            R(t, 3, 11, 8, 1, Outline);
            R(t, 21, 11, 8, 1, Outline);
            return t;
        }

        private static Texture2D StoneStep()
        {
            var t = New(16, 8);
            R(t, 0, 0, 16, 7, Sky);
            R(t, 0, 6, 16, 1, Paper);
            R(t, 0, 0, 16, 1, Outline);
            return t;
        }

        private static Texture2D Bamboo()
        {
            var t = New(16, 64);
            R(t, 6, 0, 3, 64, Green);
            for (var y = 8; y < 64; y += 10) R(t, 5, y, 5, 1, Outline);
            R(t, 2, 50, 4, 2, Green);
            R(t, 10, 56, 4, 2, Green);
            R(t, 1, 58, 3, 2, Green);
            return t;
        }

        private static Texture2D Banyan()
        {
            var t = New(64, 72);
            R(t, 28, 0, 8, 24, Wood);
            R(t, 27, 0, 1, 24, Outline);
            R(t, 36, 0, 1, 24, Outline);
            Ellipse(t, 32, 50, 30, 22, Outline);
            Ellipse(t, 32, 50, 27, 19, Green);
            Ellipse(t, 20, 58, 12, 8, Green);
            Ellipse(t, 44, 58, 12, 8, Green);
            return t;
        }

        private static Texture2D Grass()
        {
            var t = New(16, 8);
            int[] xs = { 2, 5, 8, 11, 14 };
            foreach (var x in xs)
            {
                var h = 3 + (x * 3) % 4;
                R(t, x, 0, 1, h, Green);
                P(t, x, 0, Outline);
            }
            return t;
        }

        private static Texture2D ShadowSprite()
        {
            var t = New(16, 8);
            Ellipse(t, 8, 4, 7, 3, new Color(0f, 0f, 0f, 0.25f));
            return t;
        }

        // ================= 电影感构图素材 =================

        private static Texture2D TreeForeground()
        {
            var t = New(320, 384);
            var rng = new System.Random(20261110);
            Branch(t, 150f, 0f, Mathf.PI * 0.5f, 95f, 28f, 6, rng);
            return t;
        }

        private static Texture2D BranchOverhead()
        {
            var t = New(512, 128);
            var rng = new System.Random(777);
            Branch(t, 8f, 98f, 0.08f, 130f, 20f, 5, rng);
            return t;
        }

        private static void Branch(Texture2D t, float x, float y, float angle, float len, float width,
            int depth, System.Random rng)
        {
            var steps = Mathf.Max(4, Mathf.RoundToInt(len));
            for (var i = 0; i < steps; i++)
            {
                var px = x + Mathf.Cos(angle) * i;
                var py = y + Mathf.Sin(angle) * i;
                var w = Mathf.Max(1, Mathf.RoundToInt(width * (1f - i / (float)steps)));
                R(t, Mathf.RoundToInt(px), Mathf.RoundToInt(py), w, w, Bark);
            }

            if (depth <= 0) return;

            var ex = x + Mathf.Cos(angle) * len;
            var ey = y + Mathf.Sin(angle) * len;
            var spread = 0.22f + (float)rng.NextDouble() * 0.22f;

            Branch(t, ex, ey, angle - spread, len * 0.74f, Mathf.Max(1f, width * 0.68f), depth - 1, rng);
            Branch(t, ex, ey, angle + spread, len * 0.70f, Mathf.Max(1f, width * 0.66f), depth - 1, rng);

            if (depth <= 3)
            {
                var clusters = rng.Next(2, 5);
                for (var i = 0; i < clusters; i++)
                {
                    var cx = Mathf.RoundToInt(ex + (float)(rng.NextDouble() * 44 - 22));
                    var cy = Mathf.RoundToInt(ey + (float)(rng.NextDouble() * 44 - 22));
                    Ellipse(t, cx, cy, rng.Next(5, 11), rng.Next(4, 8),
                        rng.Next(0, 2) == 0 ? Blossom : BlossomDeep);
                }
            }
        }

        private static Texture2D PagodaFar()
        {
            var t = New(96, 192);
            var col = StoneDark;
            for (var i = 0; i < 5; i++)
            {
                var y = 30 + i * 28;
                var w = 60 - i * 8;
                var x = (96 - w) / 2;
                R(t, x, y, w, 14, col);
                R(t, x - 6, y + 14, w + 12, 4, col);
                P(t, x - 8, y + 17, col);
                P(t, x + w + 7, y + 17, col);
            }
            R(t, 44, 0, 8, 30, col);
            R(t, 46, 170, 4, 22, col);
            R(t, 42, 168, 12, 3, col);
            return t;
        }

        private static Texture2D TempleRoof()
        {
            var t = New(192, 128);
            R(t, 24, 0, 144, 60, StoneDark);
            for (var i = 0; i < 40; i++)
            {
                var y = 60 + i;
                var inset = Mathf.RoundToInt(i * 1.6f);
                R(t, 10 + inset, y, 172 - inset * 2, 1, StoneDark);
            }
            R(t, 0, 96, 12, 5, StoneDark);
            R(t, 180, 96, 12, 5, StoneDark);
            R(t, 60, 99, 72, 3, StoneDark);
            R(t, 14, 92, 164, 1, Gold);
            // 门洞
            R(t, 84, 0, 24, 34, Haze);
            return t;
        }

        private static Texture2D StoneLantern()
        {
            var t = New(48, 72);
            R(t, 18, 0, 12, 8, StoneMid);
            R(t, 21, 8, 6, 30, StoneMid);
            R(t, 8, 36, 32, 6, StoneDark);
            R(t, 13, 42, 22, 16, StoneMid);
            R(t, 19, 45, 10, 10, Gold);
            R(t, 8, 58, 32, 5, StoneDark);
            R(t, 3, 63, 42, 4, StoneDark);
            R(t, 22, 67, 4, 5, StoneDark);
            return t;
        }

        private static Texture2D RockCluster()
        {
            var t = New(96, 64);
            Ellipse(t, 26, 22, 22, 16, StoneMid);
            Ellipse(t, 52, 26, 18, 14, StoneMid);
            Ellipse(t, 73, 20, 14, 12, StoneMid);
            Ellipse(t, 26, 18, 18, 10, StoneDark);
            Ellipse(t, 52, 22, 14, 9, StoneDark);
            Ellipse(t, 20, 31, 10, 4, Moss);
            Ellipse(t, 58, 33, 8, 4, Moss);
            Ellipse(t, 76, 28, 6, 3, Moss);
            return t;
        }

        private static Texture2D BambooGrove()
        {
            var t = New(96, 192);
            var rng = new System.Random(99);
            for (var s = 0; s < 16; s++)
            {
                var x = rng.Next(4, 92);
                var h = rng.Next(120, 190);
                R(t, x, 0, 2, h, s % 3 == 0 ? LeafDark : Green);
                for (var y = 10; y < h; y += 14) P(t, x, y, Outline);
            }
            for (var i = 0; i < 70; i++)
            {
                var x = rng.Next(0, 96);
                var y = rng.Next(115, 192);
                Ellipse(t, x, y, 4, 2, rng.Next(0, 2) == 0 ? Green : LeafLight);
            }
            return t;
        }

        private static Texture2D MistBand()
        {
            var t = New(256, 64);
            var rng = new System.Random(4242);
            for (var y = 0; y < 64; y++)
            {
                var fall = 1f - Mathf.Abs(y - 32) / 32f;
                for (var x = 0; x < 256; x++)
                {
                    if (rng.NextDouble() < fall * 0.32f)
                    {
                        var a = (byte)Mathf.RoundToInt(36 + fall * 58);
                        P(t, x, y, new Color32(245, 242, 232, a));
                    }
                }
            }
            return t;
        }

        private static Texture2D Petal()
        {
            var t = New(6, 6);
            Ellipse(t, 3, 3, 2, 3, Blossom);
            P(t, 4, 4, Paper);
            return t;
        }

        private static Texture2D LightShaft()
        {
            var t = New(96, 256);
            var rng = new System.Random(31);
            for (var y = 0; y < 256; y++)
            {
                var fall = y / 255f;
                for (var x = 0; x < 96; x++)
                {
                    var edge = 1f - Mathf.Abs(x - 48) / 48f;
                    if (rng.NextDouble() < fall * edge * 0.16f)
                    {
                        var a = (byte)Mathf.RoundToInt(24 + fall * edge * 56);
                        P(t, x, y, new Color32(255, 250, 235, a));
                    }
                }
            }
            return t;
        }

        // ================= 野物版角色（粗描边 · 平涂） =================

        private static void Blob(Texture2D t, int cx, int cy, int rx, int ry, Color fill)
        {
            Ellipse(t, cx, cy, rx + 1, ry + 1, Outline);
            Ellipse(t, cx, cy, rx, ry, fill);
        }

        private static void Box(Texture2D t, int x, int y, int w, int h, Color fill)
        {
            R(t, x - 1, y - 1, w + 2, h + 2, Outline);
            R(t, x, y, w, h, fill);
        }

        private static Texture2D PlayerBoy()
        {
            var t = New(24, 36);

            // 腿与鞋
            Box(t, 8, 2, 3, 8, Skin);
            Box(t, 13, 2, 3, 8, Skin);
            R(t, 6, 0, 6, 3, Outline);
            R(t, 12, 0, 6, 3, Outline);

            // 短裤
            Box(t, 7, 8, 10, 6, ShortsNavy);

            // 衬衫
            Box(t, 6, 13, 12, 11, ShirtBlue);
            R(t, 11, 15, 2, 8, ShirtBlueDark);
            R(t, 9, 17, 2, 2, Paper);
            R(t, 9, 21, 2, 2, Paper);

            // 手臂
            Box(t, 3, 14, 3, 10, ShirtBlue);
            Box(t, 18, 14, 3, 10, ShirtBlue);
            R(t, 3, 12, 3, 3, Skin);
            R(t, 18, 12, 3, 3, Skin);

            // 头
            Blob(t, 12, 25, 6, 6, Skin);

            // 头发（锅盖头）
            Ellipse(t, 12, 30, 6, 4, Outline);
            Ellipse(t, 12, 30, 5, 3, HairBlack);
            R(t, 6, 27, 2, 4, HairBlack);
            R(t, 16, 27, 2, 4, HairBlack);

            // 五官
            P(t, 10, 24, Ink);
            P(t, 14, 24, Ink);
            P(t, 11, 21, Ink);
            P(t, 12, 20, Ink);
            P(t, 13, 21, Ink);

            // 手里的木棍（弹弓柄）
            R(t, 19, 8, 2, 15, Wood);
            R(t, 18, 21, 4, 2, Wood);

            return t;
        }

        private static Texture2D WildChicken()
        {
            var t = New(30, 30);

            // 尾羽
            Ellipse(t, 5, 17, 5, 3, Outline);
            Ellipse(t, 5, 17, 4, 2, ChickenGreen);
            R(t, 1, 14, 7, 2, Outline);
            R(t, 2, 15, 6, 1, ChickenGreen);

            // 身体与翅膀
            Blob(t, 13, 12, 8, 7, ChickenBrown);
            Ellipse(t, 13, 11, 5, 4, ChickenWing);

            // 脖子与头
            Blob(t, 22, 20, 3, 5, ChickenGreen);
            Blob(t, 24, 24, 3, 3, ChickenGreen);
            R(t, 23, 27, 2, 2, CombRed);
            R(t, 26, 23, 3, 2, BeakGold);
            P(t, 25, 24, Ink);
            P(t, 23, 21, CombRed);

            // 腿
            R(t, 11, 0, 1, 6, BeakGold);
            R(t, 16, 0, 1, 6, BeakGold);
            R(t, 9, 0, 4, 1, BeakGold);
            R(t, 15, 0, 4, 1, BeakGold);

            return t;
        }

        private static Texture2D WildDog()
        {
            var t = New(40, 26);

            // 尾巴
            R(t, 2, 14, 7, 2, Outline);
            R(t, 3, 15, 6, 1, DogTan);

            // 身体
            Blob(t, 18, 11, 11, 6, DogTan);
            Ellipse(t, 18, 9, 8, 4, DogLight);

            // 四条腿
            Box(t, 9, 0, 4, 7, DogTan);
            Box(t, 15, 0, 4, 7, DogTan);
            Box(t, 23, 0, 4, 7, DogTan);
            Box(t, 29, 0, 4, 7, DogTan);

            // 头与嘴
            Blob(t, 31, 15, 6, 6, DogTan);
            R(t, 36, 13, 4, 4, Outline);
            R(t, 37, 14, 3, 2, DogLight);
            R(t, 39, 15, 1, 1, Outline);

            // 耳朵、眼睛、舌头
            R(t, 28, 20, 4, 4, Outline);
            P(t, 32, 17, Ink);
            R(t, 33, 11, 3, 1, CombRed);

            return t;
        }

        private static Texture2D WaterBuffalo()
        {
            var t = New(44, 30);

            // 身体
            Blob(t, 20, 12, 13, 8, BuffaloGray);
            Ellipse(t, 20, 10, 10, 5, BuffaloLight);

            // 腿
            Box(t, 9, 0, 5, 7, BuffaloGray);
            Box(t, 16, 0, 5, 7, BuffaloGray);
            Box(t, 25, 0, 5, 7, BuffaloGray);
            Box(t, 32, 0, 5, 7, BuffaloGray);

            // 头
            Blob(t, 35, 14, 7, 6, BuffaloGray);

            // 牛角
            Ellipse(t, 31, 21, 6, 2, Outline);
            R(t, 29, 22, 8, 2, Horn);
            Ellipse(t, 39, 20, 5, 2, Outline);
            R(t, 38, 21, 6, 2, Horn);

            // 口鼻与眼睛
            Ellipse(t, 41, 11, 4, 3, BuffaloLight);
            P(t, 40, 11, Ink);
            P(t, 42, 11, Ink);
            P(t, 36, 16, Ink);

            return t;
        }

        private static Texture2D WildBoarKing()
        {
            var t = New(52, 34);

            // 身体
            Blob(t, 24, 13, 15, 9, BoarBrown);
            Ellipse(t, 24, 11, 11, 6, BoarDark);

            // 背刺
            for (var i = 0; i < 6; i++)
            {
                var x = 12 + i * 5;
                R(t, x, 21, 3, 4, Outline);
                R(t, x + 1, 22, 1, 3, BoarDark);
            }

            // 腿
            Box(t, 10, 0, 5, 7, BoarDark);
            Box(t, 18, 0, 5, 7, BoarDark);
            Box(t, 28, 0, 5, 7, BoarDark);
            Box(t, 36, 0, 5, 7, BoarDark);

            // 头与鼻
            Blob(t, 41, 13, 9, 8, BoarBrown);
            Ellipse(t, 48, 10, 4, 3, BoarDark);
            P(t, 47, 10, Ink);
            P(t, 49, 10, Ink);

            // 獠牙
            R(t, 44, 6, 3, 4, Ivory);
            R(t, 49, 7, 2, 3, Ivory);

            // 耳朵与眼睛
            R(t, 37, 20, 4, 4, Outline);
            P(t, 43, 16, Ink);

            // 卷尾
            R(t, 6, 12, 3, 6, Outline);
            R(t, 5, 11, 3, 1, Outline);

            return t;
        }

        // ================= 工具 =================

        private static Texture2D New(int w, int h)
        {
            var t = new Texture2D(w, h, TextureFormat.RGBA32, false);
            var px = new Color[w * h];
            for (var i = 0; i < px.Length; i++) px[i] = Clear;
            t.SetPixels(px);
            return t;
        }

        private static void R(Texture2D t, int x, int y, int w, int h, Color c)
        {
            for (var yy = y; yy < y + h; yy++)
            for (var xx = x; xx < x + w; xx++)
                if (xx >= 0 && yy >= 0 && xx < t.width && yy < t.height)
                    t.SetPixel(xx, yy, c);
        }

        private static void P(Texture2D t, int x, int y, Color c)
        {
            if (x >= 0 && y >= 0 && x < t.width && y < t.height) t.SetPixel(x, y, c);
        }

        private static void Border(Texture2D t, Color c)
        {
            for (var x = 0; x < t.width; x++)
            {
                P(t, x, 0, c);
                P(t, x, t.height - 1, c);
            }
            for (var y = 0; y < t.height; y++)
            {
                P(t, 0, y, c);
                P(t, t.width - 1, y, c);
            }
        }

        private static void Ellipse(Texture2D t, int cx, int cy, int rx, int ry, Color c)
        {
            for (var y = -ry; y <= ry; y++)
            for (var x = -rx; x <= rx; x++)
            {
                var nx = (float)x / rx;
                var ny = (float)y / ry;
                if (nx * nx + ny * ny <= 1f) P(t, cx + x, cy + y, c);
            }
        }

        private static void SaveTex(Texture2D tex, string name)
        {
            tex.Apply();
            var abs = Path.Combine(Application.dataPath, "_Game/Art/Generated/" + name + ".png");
            File.WriteAllBytes(abs, tex.EncodeToPNG());
            Object.DestroyImmediate(tex);
        }

        private static Sprite Load(string name)
        {
            var path = Dir + "/" + name + ".png";
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (sprite == null) Debug.LogError("[Stage2] 导入 Sprite 失败: " + path);
            return sprite;
        }

        private static void BuildLibrary()
        {
            var lib = AssetDatabase.LoadAssetAtPath<ArtLibrary>(LibPath);
            if (lib == null)
            {
                lib = ScriptableObject.CreateInstance<ArtLibrary>();
                AssetDatabase.CreateAsset(lib, LibPath);
                Debug.Log("[Stage2] 新建 ArtLibrary.asset");
            }

            lib.brickWall = Load("brick_wall");
            lib.concreteFloor = Load("concrete_floor");
            lib.roofTile = Load("roof_tile");
            lib.roofFar = Load("roof_far");
            lib.door = Load("door");
            lib.window = Load("window");
            lib.planeTree = Load("plane_tree");
            lib.utilityPole = Load("utility_pole");
            lib.streetLamp = Load("street_lamp");
            lib.trashCan = Load("trash_can");
            lib.bicycle = Load("bicycle");
            lib.oldHouse = Load("old_house");
            lib.kidA = Load("kid_a");
            lib.kidB = Load("kid_b");
            lib.speeder = Load("speeder");
            lib.fatty = Load("fatty");
            lib.king = Load("king");
            lib.slingshot = Load("slingshot");
            lib.stone = Load("stone");
            lib.bubblegum = Load("bubblegum");
            lib.tinFrog = Load("tin_frog");
            lib.marble = Load("marble");
            lib.latiao = Load("latiao");

            lib.mountainFar = Load("mountain_far");
            lib.mountainMid = Load("mountain_mid");
            lib.cloud = Load("cloud");
            lib.stoneWall = Load("stone_wall");
            lib.cobblestone = Load("cobblestone");
            lib.tileRoof = Load("tile_roof");
            lib.ruralHouseBig = Load("rural_house_big");
            lib.ruralHouseSmall = Load("rural_house_small");
            lib.well = Load("well");
            lib.cornRack = Load("corn_rack");
            lib.lantern = Load("lantern");
            lib.laundry = Load("laundry");
            lib.stoneStep = Load("stone_step");
            lib.bamboo = Load("bamboo");
            lib.banyan = Load("banyan");
            lib.grass = Load("grass");
            lib.shadow = Load("shadow");

            lib.treeForeground = Load("tree_foreground");
            lib.branchOverhead = Load("branch_overhead");
            lib.pagodaFar = Load("pagoda_far");
            lib.templeRoof = Load("temple_roof");
            lib.stoneLantern = Load("stone_lantern");
            lib.rockCluster = Load("rock_cluster");
            lib.bambooGrove = Load("bamboo_grove");
            lib.mistBand = Load("mist_band");
            lib.petal = Load("petal");
            lib.lightShaft = Load("light_shaft");

            lib.playerBoy = Load("player_boy");
            lib.wildChicken = Load("wild_chicken");
            lib.wildDog = Load("wild_dog");
            lib.waterBuffalo = Load("water_buffalo");
            lib.wildBoarKing = Load("wild_boar_king");

            EditorUtility.SetDirty(lib);
            AssetDatabase.SaveAssets();
        }
    }
}
