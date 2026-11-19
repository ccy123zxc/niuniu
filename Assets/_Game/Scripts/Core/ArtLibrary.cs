using UnityEngine;

namespace OldAlley.Core
{
    /// <summary>
    /// 占位美术的 Sprite 引用表。换正式美术时只替换 PNG、保持文件名与 PPU 不变。
    /// </summary>
    [CreateAssetMenu(menuName = "旧巷守卫/ArtLibrary", fileName = "ArtLibrary")]
    public class ArtLibrary : ScriptableObject
    {
        [Header("2.5D 远景")]
        public Sprite mountainFar;
        public Sprite mountainMid;
        public Sprite cloud;
        public Sprite pagodaFar;
        public Sprite templeRoof;

        [Header("电影感构图")]
        public Sprite treeForeground;
        public Sprite branchOverhead;
        public Sprite stoneLantern;
        public Sprite rockCluster;
        public Sprite bambooGrove;
        public Sprite mistBand;
        public Sprite petal;
        public Sprite lightShaft;

        [Header("第一关等距地图")]
        public Sprite mapGround;
        public Sprite gateSouth;
        public Sprite pigPen;
        public Sprite stoneMill;
        public Sprite woodPile;
        public Sprite waterVat;
        public Sprite orchardTree;

        [Header("地面与墙")]
        public Sprite cobblestone;
        public Sprite stoneWall;
        public Sprite tileRoof;

        [Header("乡村建筑")]
        public Sprite ruralHouseBig;
        public Sprite ruralHouseSmall;
        public Sprite well;
        public Sprite cornRack;
        public Sprite lantern;
        public Sprite laundry;
        public Sprite stoneStep;

        [Header("植被与道具")]
        public Sprite bamboo;
        public Sprite banyan;
        public Sprite grass;
        public Sprite utilityPole;
        public Sprite trashCan;
        public Sprite bicycle;

        [Header("角色（野物版）")]
        public Sprite playerBoy;
        public Sprite wildChicken;
        public Sprite wildDog;
        public Sprite waterBuffalo;
        public Sprite wildBoarKing;

        [Header("角色")]
        public Sprite kidA;
        public Sprite kidB;
        public Sprite speeder;
        public Sprite fatty;
        public Sprite king;

        [Header("武器与拾取物")]
        public Sprite slingshot;
        public Sprite stone;
        public Sprite bubblegum;
        public Sprite tinFrog;
        public Sprite marble;
        public Sprite latiao;

        [Header("通用")]
        public Sprite shadow;
        public Sprite oldHouse;
        public Sprite concreteFloor;
        public Sprite brickWall;
        public Sprite door;
        public Sprite window;
        public Sprite planeTree;
        public Sprite streetLamp;
        public Sprite roofFar;
        public Sprite roofTile;
    }
}
