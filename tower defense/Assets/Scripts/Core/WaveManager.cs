using System;
using System.Collections;
using System.Collections.Generic;
using TowerDefense.Audio;
using TowerDefense.Enemies;
using UnityEngine;

namespace TowerDefense.Core
{
    [System.Serializable]
    public class EnemySpawnGroup
    {
        public Enemy enemyPrefab;
        public int count = 5;
        public float spawnInterval = 1.0f;
        public float delayBeforeGroup = 0.5f;
    }

    [System.Serializable]
    public class WaveData
    {
        public string waveName = "Wave 1";
        public List<EnemySpawnGroup> spawnGroups = new List<EnemySpawnGroup>();
    }

    /// <summary>
    /// Controls wave progression, spawning enemies along the WaypointPath,
    /// tracking active wave enemies, and triggering wave completion / victory.
    /// </summary>
    public class WaveManager : MonoBehaviour
    {
        public static WaveManager Instance { get; private set; }

        [Header("Path Assignment")]
        [SerializeField] private WaypointPath assignedPath;

        [Header("Wave Configurations")]
        [SerializeField] private List<WaveData> waves = new List<WaveData>();

        [Header("Wave State")]
        [SerializeField] private int currentWaveIndex = 0;
        private int enemiesRemainingInWave = 0;
        private bool isSpawning = false;

        public event Action<int, int> OnWaveChanged; // (currentWave, totalWaves)
        public event Action<int> OnEnemiesRemainingChanged;

        public int CurrentWaveNumber => currentWaveIndex + 1;
        public int TotalWaves => waves.Count;
        public bool IsWaveActive => isSpawning || enemiesRemainingInWave > 0;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void Start()
        {
            OnWaveChanged?.Invoke(currentWaveIndex + 1, waves.Count);
        }

        /// <summary>
        /// Starts the next wave if currently in PreWave state.
        /// </summary>
        public void StartNextWave()
        {
            if (IsWaveActive) return;
            if (currentWaveIndex >= waves.Count) return;

            GameManager.Instance?.SetState(GameState.WaveInProgress);
            StartCoroutine(SpawnWaveRoutine(waves[currentWaveIndex]));
        }

        private IEnumerator SpawnWaveRoutine(WaveData wave)
        {
            isSpawning = true;
            enemiesRemainingInWave = 0;

            // Calculate total enemy count in this wave
            foreach (var group in wave.spawnGroups)
            {
                enemiesRemainingInWave += group.count;
            }
            OnEnemiesRemainingChanged?.Invoke(enemiesRemainingInWave);

            foreach (var group in wave.spawnGroups)
            {
                yield return new WaitForSeconds(group.delayBeforeGroup);

                for (int i = 0; i < group.count; i++)
                {
                    if (GameManager.Instance != null && GameManager.Instance.CurrentState == GameState.GameOver)
                    {
                        yield break;
                    }

                    SpawnEnemy(group.enemyPrefab);
                    yield return new WaitForSeconds(group.spawnInterval);
                }
            }

            isSpawning = false;
        }

        private void SpawnEnemy(Enemy prefab)
        {
            if (prefab == null || assignedPath == null) return;

            Vector3 spawnPos = assignedPath.GetWaypointPosition(0);
            Enemy enemy = Instantiate(prefab, spawnPos, Quaternion.identity);
            enemy.Initialize(assignedPath);

            // Hook into destruction to know when the enemy is resolved (killed or reached base)
            EnemyWatcher watcher = enemy.gameObject.AddComponent<EnemyWatcher>();
            watcher.OnDestroyed += HandleEnemyResolved;
        }

        private void HandleEnemyResolved()
        {
            enemiesRemainingInWave--;
            OnEnemiesRemainingChanged?.Invoke(Mathf.Max(0, enemiesRemainingInWave));

            if (!isSpawning && enemiesRemainingInWave <= 0)
            {
                OnWaveCompleted();
            }
        }

        private void OnWaveCompleted()
        {
            currentWaveIndex++;

            if (currentWaveIndex >= waves.Count)
            {
                // Cleared all waves!
                GameManager.Instance?.SetState(GameState.Victory);
                AudioManager.Instance?.PlayVictorySound();
            }
            else
            {
                // Return to preparation phase for next wave
                GameManager.Instance?.SetState(GameState.PreWave);
                OnWaveChanged?.Invoke(currentWaveIndex + 1, waves.Count);
            }
        }
    }

    /// <summary>
    /// Helper component to notify WaveManager when an enemy is destroyed.
    /// </summary>
    internal class EnemyWatcher : MonoBehaviour
    {
        public event Action OnDestroyed;

        private void OnDestroy()
        {
            OnDestroyed?.Invoke();
        }
    }
}
