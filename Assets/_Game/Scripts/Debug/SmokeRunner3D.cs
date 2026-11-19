using System.Collections;
using System.Reflection;
using OldAlley.Core;
using OldAlley.Enemy;
using OldAlley.Pickup;
using OldAlley.Player;
using OldAlley.Wave;
using UnityEngine;

namespace OldAlley.DebugTools
{
    /// <summary>临时自检：移动、开火、点地、砖墙拆补、波次限制。</summary>
    public class SmokeRunner3D : MonoBehaviour
    {
        private IEnumerator Start()
        {
            yield return new WaitForSeconds(1.2f);

            var player = GameObject.Find("Player");
            if (player == null)
            {
                Debug.LogError("[SMOKE] 找不到 Player");
                Finish();
                yield break;
            }

            var rb = player.GetComponent<Rigidbody>();
            var controller = player.GetComponent<PlayerController>();
            var slingshot = player.GetComponent<Slingshot>();
            var wave = FindObjectOfType<WaveManager>();
            var wall = WallController.Instance;

            var type = typeof(PlayerController);
            var repairMethod = type.GetMethod("TryRepairAt", BindingFlags.NonPublic | BindingFlags.Instance);

            Debug.Log("[SMOKE] timeScale=" + Time.timeScale +
                      " pos=" + player.transform.position +
                      " controller=" + (controller != null && controller.enabled) +
                      " config=" + (controller != null && controller.config != null) +
                      " wall=" + (wall != null ? wall.totalCount + "块" : "null"));

            // ---- 地面不再卡人 ----
            controller.enabled = false;
            var start = rb.position;
            rb.velocity = new Vector3(4f, 0f, 0f);
            yield return new WaitForSeconds(0.5f);
            Debug.Log("[SMOKE] 物理位移=" + (rb.position - start));
            controller.enabled = true;

            // ---- 点地移动 + 走路动作 ----
            var target = player.transform.position + new Vector3(3f, 0f, 3f);
            target.y = 0.5f;
            SetField(type, controller, "_moveTarget", target);
            SetField(type, controller, "_bestDistance", Vector3.Distance(target, player.transform.position));
            SetField(type, controller, "_hasMoveTarget", true);

            yield return new WaitForSeconds(0.4f);
            var legPivot = player.transform.Find("Model/LegPivotL");
            Debug.Log("[SMOKE] 走路 腿角=" +
                      (legPivot != null ? legPivot.localEulerAngles.x.ToString("0.0") : "null") +
                      " 速度=" + rb.velocity.magnitude.ToString("0.00"));

            yield return new WaitForSeconds(0.9f);
            Debug.Log("[SMOKE] 点地移动 距目标=" + Vector3.Distance(player.transform.position, target).ToString("0.00"));

            // ---- 波间补砖全链路（此刻是第 1 波准备期）----
            Debug.Log("[SMOKE] 准备期 CanRepair=" + (wave != null && wave.CanRepair));

            var house = FindObjectOfType<HouseController>();
            var hpBefore = house != null ? house.hp : -1;

            var breakBrick = wall.FindNearest(new Vector3(0f, 1f, 6.5f), 8f);
            breakBrick.TakeDamage(999, Vector3.zero);
            yield return new WaitForSeconds(0.3f);

            var drop = FindObjectOfType<BrickPickup>();
            Debug.Log("[SMOKE] 打掉一块 完好=" + wall.aliveCount +
                      " 老屋血=" + hpBefore + "→" + (house != null ? house.hp : -1) +
                      " 掉落=" + (drop != null));

            if (drop != null)
            {
                player.transform.position = new Vector3(drop.transform.position.x, 0.5f,
                    drop.transform.position.z + 0.2f);
                yield return new WaitForSeconds(0.6f);
                Debug.Log("[SMOKE] 拾取后携带砖=" + controller.Bricks);
            }

            // 本轮额度按「休整开始时缺少的砖数 / 3」算；测试里开局没缺砖，手动给 1 个额度
            SetField(type, controller, "_repairBudget", 1);

            var screen = Camera.main.WorldToScreenPoint(breakBrick.transform.position);
            var repaired = (bool)repairMethod.Invoke(controller, new object[] { screen });
            yield return new WaitForSeconds(0.7f);
            Debug.Log("[SMOKE] 波间补墙 clicked=" + repaired +
                      " 完好=" + wall.aliveCount + " 携带砖=" + controller.Bricks +
                      " 剩余额度=" + (int)typeof(PlayerController)
                          .GetField("_repairBudget", BindingFlags.NonPublic | BindingFlags.Instance)
                          .GetValue(controller));

            // 额度用尽后不能再补
            var secondBrick = wall.FindNearest(new Vector3(0f, 1f, 6.5f), 8f);
            secondBrick.TakeDamage(999, Vector3.zero);
            yield return new WaitForSeconds(0.2f);

            player.transform.position = new Vector3(secondBrick.transform.position.x, 0.5f,
                secondBrick.transform.position.z - 2.5f);
            yield return new WaitForFixedUpdate();
            controller.AddBrick(1);

            var overBudget = (bool)repairMethod.Invoke(controller,
                new object[] { Camera.main.WorldToScreenPoint(secondBrick.transform.position) });
            yield return new WaitForSeconds(0.6f);
            Debug.Log("[SMOKE] 额度用尽 补墙 clicked=" + overBudget +
                      " 完好(应不变)=" + wall.aliveCount);

            // ---- 贴墙开火（修复触发器吃子弹的问题）----
            player.transform.position = new Vector3(0f, 0.5f, 5.9f);
            yield return new WaitForFixedUpdate();

            var fake = new FakeInput { Move = Vector3.zero, FireHeld = true };
            controller.InputOverride = fake;

            yield return new WaitForSeconds(0.7f);
            var activeMid = slingshot != null && slingshot.pool != null
                ? slingshot.pool.Total - slingshot.pool.FreeCount : -1;

            yield return new WaitForSeconds(0.5f);
            controller.InputOverride = null;

            Debug.Log("[SMOKE] 贴墙开火 瞄准调用=" + fake.aimCalls + " 半途在用=" + activeMid);

            // ---- 战斗中禁止补砖 ----
            player.transform.position = new Vector3(0f, 0.5f, 1.5f);
            yield return new WaitForFixedUpdate();

            var guard = 0f;
            while (wave != null && wave.CanRepair && guard < 20f)
            {
                guard += Time.deltaTime;
                yield return null;
            }

            Debug.Log("[SMOKE] 开打后 CanRepair=" + (wave != null && wave.CanRepair));

            var testBrick = wall.FindNearest(new Vector3(0f, 1f, 6.5f), 8f);
            testBrick.TakeDamage(999, Vector3.zero);
            yield return new WaitForSeconds(0.3f);

            player.transform.position = new Vector3(testBrick.transform.position.x, 0.5f,
                testBrick.transform.position.z - 2.5f);
            yield return new WaitForFixedUpdate();

            var aliveBefore = wall.aliveCount;
            var blocked = (bool)repairMethod.Invoke(controller,
                new object[] { Camera.main.WorldToScreenPoint(testBrick.transform.position) });
            yield return new WaitForSeconds(0.7f);

            Debug.Log("[SMOKE] 波内补墙 clicked=" + blocked +
                      " 完好(应不变)=" + aliveBefore + "→" + wall.aliveCount);

            // ---- 野兽拆砖 ----
            player.transform.position = new Vector3(0f, 0.5f, 1.5f);
            yield return new WaitForFixedUpdate();

            // 等几只怪都刷出来再做命中测试
            yield return new WaitForSeconds(3.2f);

            var aliveEnemies = new System.Collections.Generic.List<EnemyBase>();
            foreach (var enemy in FindObjectsOfType<EnemyBase>())
                if (enemy != null && enemy.gameObject.activeSelf) aliveEnemies.Add(enemy);

            if (aliveEnemies.Count == 0)
            {
                Debug.LogError("[SMOKE] 没有存活的敌人");
            }
            else
            {
                // A. 空地命中
                var e1 = aliveEnemies[0];
                e1.transform.position = new Vector3(0f, 0.5f, 3f);
                player.transform.position = new Vector3(0f, 0.5f, -1f);
                yield return new WaitForFixedUpdate();

                var hpA = e1.Hp;
                controller.InputOverride = new FakeInput { FireHeld = true, aimTarget = e1.transform };
                yield return new WaitForSeconds(1.2f);
                controller.InputOverride = null;
                Debug.Log("[SMOKE] 空地命中 敌人血=" + hpA + "→" + e1.Hp);

                // B. 贴墙命中（玩家和敌人都在墙边）
                if (aliveEnemies.Count > 1)
                {
                    var e2 = aliveEnemies[1];
                    e2.transform.position = new Vector3(1.5f, 0.5f, 5.8f);
                    player.transform.position = new Vector3(0.2f, 0.5f, 5.4f);
                    yield return new WaitForFixedUpdate();

                    var hpB = e2.Hp;
                    controller.InputOverride = new FakeInput { FireHeld = true, aimTarget = e2.transform };
                    yield return new WaitForSeconds(1.2f);
                    controller.InputOverride = null;
                    Debug.Log("[SMOKE] 墙边命中 敌人血=" + hpB + "→" + e2.Hp +
                              " 砖=" + wall.aliveCount);
                }

                // C. 被挤进门洞后应被拉回墙前，并且能被打中
                if (aliveEnemies.Count > 2)
                {
                    var e3 = aliveEnemies[2];
                    e3.transform.position = new Vector3(0f, 0.5f, 7f);
                    yield return new WaitForSeconds(1.6f);
                    Debug.Log("[SMOKE] 门洞拉出 z=" + e3.transform.position.z.ToString("0.00"));

                    player.transform.position = new Vector3(0f, 0.5f, 4.4f);
                    yield return new WaitForFixedUpdate();

                    var hpC = e3.Hp;
                    controller.InputOverride = new FakeInput { FireHeld = true, aimTarget = e3.transform };
                    yield return new WaitForSeconds(1.2f);
                    controller.InputOverride = null;
                    Debug.Log("[SMOKE] 门洞命中 敌人血=" + hpC + "→" + e3.Hp);
                }

                // D. 野兽拆砖
                var probe = aliveEnemies[0];
                var before = wall.aliveCount;
                probe.transform.position = new Vector3(1.5f, 0.5f, 5.0f);
                yield return new WaitForSeconds(4f);

                Debug.Log("[SMOKE] 野兽拆砖 完好=" + before + "→" + wall.aliveCount +
                          " 掉落=" + (FindObjectOfType<BrickPickup>() != null));
            }

            Finish();
        }

        private static void SetField(System.Type type, object target, string name, object value)
        {
            type.GetField(name, BindingFlags.NonPublic | BindingFlags.Instance).SetValue(target, value);
        }

        private class FakeInput : IInputSource
        {
            public Vector3 Move { get; set; }
            public bool FireHeld { get; set; }
            public bool FirePressed { get; set; }
            public bool UseFacingAim { get; set; }

            public int aimCalls;
            public Transform aimTarget;

            public Vector3 AimWorld(Vector3 origin)
            {
                aimCalls++;

                if (aimTarget != null)
                {
                    var direction = aimTarget.position - origin;
                    direction.y = 0f;
                    if (direction.sqrMagnitude > 0.0001f) return direction.normalized;
                }

                return Vector3.back;
            }
        }

        private void Finish()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.Exit(0);
#else
            Application.Quit();
#endif
        }
    }
}
