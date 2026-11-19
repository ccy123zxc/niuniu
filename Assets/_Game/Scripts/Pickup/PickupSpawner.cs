using System.Collections;
using System.Collections.Generic;
using OldAlley.Core;
using OldAlley.Wave;
using UnityEngine;

namespace OldAlley.Pickup
{
    /// <summary>3D 道具刷新：每波 8/20/32 秒尝试，同屏最多 2 个。</summary>
    public class PickupSpawner : MonoBehaviour
    {
        public PickupConfig[] configs;
        public PickupItem template;
        public HouseController house;
        public Transform player;
        public int maxOnField = 2;

        private readonly List<PickupItem> _live = new List<PickupItem>();
        private int _lastWave = -1;

        private void Update()
        {
            var manager = WaveManager.Instance;
            if (manager == null || !manager.IsRunning) return;

            if (manager.CurrentWave == _lastWave) return;
            _lastWave = manager.CurrentWave;
            OnWaveStarted();
        }

        private void OnWaveStarted()
        {
            StopAllCoroutines();
            StartCoroutine(SpawnAfter(8f));
            StartCoroutine(SpawnAfter(20f));
            StartCoroutine(SpawnAfter(32f));
        }

        private IEnumerator SpawnAfter(float delay)
        {
            yield return new WaitForSeconds(delay);
            TrySpawn();
        }

        public void TrySpawn()
        {
            _live.RemoveAll(item => item == null);
            if (_live.Count >= maxOnField) return;
            if (configs == null || configs.Length == 0 || template == null) return;

            var config = PickRandom();

            for (var attempt = 0; attempt < 24; attempt++)
            {
                var position = new Vector3(Random.Range(-18f, 18f), 0.5f, Random.Range(-8f, 8f));
                if (!IsValid(position)) continue;

                var item = Instantiate(template, transform);
                item.transform.position = position;
                item.Configure(config);
                _live.Add(item);
                return;
            }
        }

        private bool IsValid(Vector3 position)
        {
            if (house != null && Vector3.Distance(position, house.DoorPosition) < 3f) return false;
            if (player != null && Vector3.Distance(position, player.position) < 3f) return false;
            if (Physics.OverlapSphere(position, 1.2f, LayerMask.GetMask("Wall")).Length > 0) return false;
            return true;
        }

        private PickupConfig PickRandom()
        {
            var total = 0f;
            foreach (var config in configs) total += config.weight;

            var roll = Random.value * total;
            foreach (var config in configs)
            {
                roll -= config.weight;
                if (roll <= 0f) return config;
            }

            return configs[0];
        }
    }
}
