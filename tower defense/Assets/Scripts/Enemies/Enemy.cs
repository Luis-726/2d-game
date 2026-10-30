using System;
using System.Collections;
using System.Collections.Generic;
using TowerDefense.Audio;
using TowerDefense.Core;
using UnityEngine;

namespace TowerDefense.Enemies
{
    
    /// Base class for all enemies walking the path.
    /// Handles waypoint movement, health, manual click damage reception, and death rewards.
    
    [RequireComponent(typeof(Collider2D))]
    public class Enemy : MonoBehaviour
    {
        // Centralized list of active enemies for efficient tower targeting without FindObjectsByType
        public static readonly List<Enemy> ActiveEnemies = new List<Enemy>();

        [Header("Enemy Identity & Stats")]
        [SerializeField] private EnemyType enemyType = EnemyType.Zombie;
        [SerializeField] private int maxHealth = 10;
        [SerializeField] private float moveSpeed = 2.5f;
        [SerializeField] private int coinReward = 5;
        [SerializeField] private int baseDamage = 1;

        [Header("Visual Feedback")]
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private Color hitFlashColor = Color.white;
        [SerializeField] private float hitFlashDuration = 0.08f;
        [SerializeField] private GameObject hitEffectPrefab;
        [SerializeField] private GameObject deathEffectPrefab;

        private int currentHealth;
        private WaypointPath path;
        private int currentWaypointIndex = 0;
        private Color originalColor;
        private Coroutine flashCoroutine;
        private bool isDead = false;

        public EnemyType Type => enemyType;
        public int CurrentHealth => currentHealth;
        public int MaxHealth => maxHealth;
        public float RemainingDistance { get; private set; }

        public event Action<int, int> OnHealthChanged; // (current, max)

        private void Awake()
        {
            if (spriteRenderer == null)
            {
                spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            }
            if (spriteRenderer != null)
            {
                originalColor = spriteRenderer.color;
            }
        }

        private void OnEnable()
        {
            ActiveEnemies.Add(this);
            isDead = false;
            currentHealth = maxHealth;
        }

        private void OnDisable()
        {
            ActiveEnemies.Remove(this);
        }

        
        /// Initializes the enemy's path and starting waypoint.
        
        public void Initialize(WaypointPath assignedPath)
        {
            path = assignedPath;
            currentWaypointIndex = 0;
            currentHealth = maxHealth;
            isDead = false;

            if (path != null && path.WaypointCount > 0)
            {
                transform.position = path.GetWaypointPosition(0);
                UpdateRemainingDistance();
            }
        }

        private void Update()
        {
            if (isDead || path == null || currentWaypointIndex >= path.WaypointCount) return;

            MoveAlongPath();
            UpdateRemainingDistance();
        }

        private void MoveAlongPath()
        {
            Vector3 targetPos = path.GetWaypointPosition(currentWaypointIndex);
            float step = moveSpeed * Time.deltaTime;
            transform.position = Vector3.MoveTowards(transform.position, targetPos, step);

            // Rotate or flip sprite according to movement direction
            Vector3 direction = (targetPos - transform.position).normalized;
            if (direction.x > 0.01f && spriteRenderer != null)
            {
                spriteRenderer.flipX = false;
            }
            else if (direction.x < -0.01f && spriteRenderer != null)
            {
                spriteRenderer.flipX = true;
            }

            if (Vector3.Distance(transform.position, targetPos) < 0.05f)
            {
                currentWaypointIndex++;
                if (currentWaypointIndex >= path.WaypointCount)
                {
                    ReachBase();
                }
            }
        }

        private void UpdateRemainingDistance()
        {
            if (path != null)
            {
                RemainingDistance = path.GetRemainingDistance(currentWaypointIndex, transform.position);
            }
        }

        
        /// Inflicts damage on the enemy. Can be called by towers or player manual clicks.
        
        public void TakeDamage(int damage, bool isPlayerClick = false)
        {
            if (isDead) return;

            currentHealth -= damage;
            OnHealthChanged?.Invoke(currentHealth, maxHealth);

            // Visual hit flash
            if (spriteRenderer != null && gameObject.activeInHierarchy)
            {
                if (flashCoroutine != null) StopCoroutine(flashCoroutine);
                flashCoroutine = StartCoroutine(HitFlashRoutine());
            }

            // Spawn hit marker / impact effect if assigned
            if (hitEffectPrefab != null)
            {
                Instantiate(hitEffectPrefab, transform.position, Quaternion.identity);
            }

            // Audio cues
            if (isPlayerClick)
            {
                AudioManager.Instance?.PlayPlayerClickHitSound();
            }

            if (currentHealth <= 0)
            {
                Die();
            }
        }

        private IEnumerator HitFlashRoutine()
        {
            spriteRenderer.color = hitFlashColor;
            yield return new WaitForSeconds(hitFlashDuration);
            spriteRenderer.color = originalColor;
        }

        private void Die()
        {
            if (isDead) return;
            isDead = true;

            // Reward player with coins
            GameManager.Instance?.AddCoins(coinReward);

            // Play enemy death audio based on type
            AudioManager.Instance?.PlayEnemyDeathSound(enemyType);

            // Spawn death particle effect
            if (deathEffectPrefab != null)
            {
                Instantiate(deathEffectPrefab, transform.position, Quaternion.identity);
            }

            Destroy(gameObject);
        }

        private void ReachBase()
        {
            if (isDead) return;
            isDead = true;

            GameManager.Instance?.DeductLives(baseDamage);
            AudioManager.Instance?.PlayBaseDamagedSound();

            Destroy(gameObject);
        }
    }
}
