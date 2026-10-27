using System.Collections.Generic;
using TowerDefense.Audio;
using TowerDefense.Enemies;
using UnityEngine;

namespace TowerDefense.Towers
{
    /// <summary>
    /// Base class representing all placeable defensive towers.
    /// Supports Archer (single-target ranged), Warrior (omnidirectional melee cleave), and Mage (AoE splash).
    /// </summary>
    public class Tower : MonoBehaviour
    {
        [Header("Identity & Cost")]
        [SerializeField] private TowerType towerType = TowerType.Archer;
        [SerializeField] private string towerName = "Archer Tower";
        [SerializeField] private int buildCost = 15;

        [Header("Combat Attributes")]
        [SerializeField] private float range = 3.5f;
        [SerializeField] private float attacksPerSecond = 1.0f;
        [SerializeField] private int damage = 3;
        [SerializeField] private TargetPriority priority = TargetPriority.First;

        [Header("Archer / Mage Projectile Setup")]
        [SerializeField] private GameObject projectilePrefab;
        [SerializeField] private Transform firePoint;
        [SerializeField] private float aoeRadius = 1.5f; // For Mage

        [Header("Warrior Melee Cleave Setup")]
        [SerializeField] private GameObject meleeSlashEffectPrefab;

        [Header("Range Visualizer")]
        [SerializeField] private LineRenderer rangeCircleRenderer;
        [SerializeField] private int circleSegments = 40;

        private float attackCooldown = 0f;
        private Transform currentTarget;

        public TowerType Type => towerType;
        public string TowerName => towerName;
        public int BuildCost => buildCost;
        public float Range => range;
        public TargetPriority Priority => priority;

        private void Start()
        {
            SetupRangeCircle();
            SetRangeCircleVisible(false);
        }

        private void Update()
        {
            attackCooldown -= Time.deltaTime;

            if (towerType == TowerType.Warrior)
            {
                // Warrior checks for any enemies in melee range and performs an omnidirectional cleave
                if (attackCooldown <= 0f)
                {
                    TryWarriorCleave();
                }
            }
            else
            {
                // Archer and Mage target an individual enemy (Mage's projectile explodes on impact)
                UpdateTarget();

                if (currentTarget != null && attackCooldown <= 0f)
                {
                    ShootProjectile();
                    attackCooldown = 1f / attacksPerSecond;
                }
            }
        }

        /// <summary>
        /// Warrior attacks all enemies within range simultaneously (omnidirectional cleave).
        /// </summary>
        private void TryWarriorCleave()
        {
            List<Enemy> inRangeEnemies = new List<Enemy>();
            foreach (Enemy enemy in Enemy.ActiveEnemies)
            {
                if (enemy == null || enemy.CurrentHealth <= 0) continue;

                if (Vector3.Distance(transform.position, enemy.transform.position) <= range)
                {
                    inRangeEnemies.Add(enemy);
                }
            }

            if (inRangeEnemies.Count > 0)
            {
                // Play swing sound
                AudioManager.Instance?.PlayWarriorSwingSound();

                // Spawn melee sweep visual
                if (meleeSlashEffectPrefab != null)
                {
                    GameObject slash = Instantiate(meleeSlashEffectPrefab, transform.position, Quaternion.identity);
                    Destroy(slash, 0.3f);
                }

                // Damage all targets in range
                foreach (Enemy enemy in inRangeEnemies)
                {
                    if (enemy != null)
                    {
                        enemy.TakeDamage(damage);
                    }
                }

                attackCooldown = 1f / attacksPerSecond;
            }
        }

        /// <summary>
        /// Selects the best target according to the chosen TargetPriority.
        /// </summary>
        private void UpdateTarget()
        {
            Enemy bestEnemy = null;
            float bestMetric = (priority == TargetPriority.First) ? float.MaxValue : float.MinValue;

            foreach (Enemy enemy in Enemy.ActiveEnemies)
            {
                if (enemy == null || enemy.CurrentHealth <= 0) continue;

                float distToTower = Vector3.Distance(transform.position, enemy.transform.position);
                if (distToTower > range) continue;

                switch (priority)
                {
                    case TargetPriority.First:
                        // Lowest remaining distance to base
                        if (enemy.RemainingDistance < bestMetric)
                        {
                            bestMetric = enemy.RemainingDistance;
                            bestEnemy = enemy;
                        }
                        break;

                    case TargetPriority.Last:
                        // Highest remaining distance (furthest from base)
                        if (enemy.RemainingDistance > bestMetric)
                        {
                            bestMetric = enemy.RemainingDistance;
                            bestEnemy = enemy;
                        }
                        break;

                    case TargetPriority.Strongest:
                        // Highest current HP
                        if (enemy.CurrentHealth > bestMetric)
                        {
                            bestMetric = enemy.CurrentHealth;
                            bestEnemy = enemy;
                        }
                        break;

                    case TargetPriority.Closest:
                        if (distToTower < bestMetric)
                        {
                            bestMetric = distToTower;
                            bestEnemy = enemy;
                        }
                        break;
                }
            }

            currentTarget = (bestEnemy != null) ? bestEnemy.transform : null;
        }

        private void ShootProjectile()
        {
            if (projectilePrefab == null) return;

            Vector3 spawnPos = (firePoint != null) ? firePoint.position : transform.position;
            GameObject projObj = Instantiate(projectilePrefab, spawnPos, Quaternion.identity);
            Projectile projectile = projObj.GetComponent<Projectile>();

            if (projectile != null)
            {
                bool isAoE = (towerType == TowerType.Mage);
                projectile.Seek(currentTarget, damage, isAoE, aoeRadius);

                if (towerType == TowerType.Archer)
                {
                    AudioManager.Instance?.PlayArcherShotSound();
                }
                else if (towerType == TowerType.Mage)
                {
                    AudioManager.Instance?.PlayMageCastSound();
                }
            }
        }

        /// <summary>
        /// Changes the active targeting priority mode.
        /// </summary>
        public void SetTargetPriority(TargetPriority newPriority)
        {
            priority = newPriority;
        }

        public void SetRangeCircleVisible(bool visible)
        {
            if (rangeCircleRenderer != null)
            {
                rangeCircleRenderer.enabled = visible;
            }
        }

        private void SetupRangeCircle()
        {
            if (rangeCircleRenderer == null)
            {
                rangeCircleRenderer = GetComponentInChildren<LineRenderer>();
            }
            if (rangeCircleRenderer == null) return;

            rangeCircleRenderer.useWorldSpace = false;
            rangeCircleRenderer.positionCount = circleSegments + 1;
            rangeCircleRenderer.loop = true;

            float deltaTheta = (2f * Mathf.PI) / circleSegments;
            float theta = 0f;

            for (int i = 0; i <= circleSegments; i++)
            {
                float x = range * Mathf.Cos(theta);
                float y = range * Mathf.Sin(theta);
                rangeCircleRenderer.SetPosition(i, new Vector3(x, y, 0));
                theta += deltaTheta;
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = (towerType == TowerType.Warrior) ? Color.red : (towerType == TowerType.Mage ? Color.magenta : Color.green);
            Gizmos.DrawWireSphere(transform.position, range);
        }
    }
}
