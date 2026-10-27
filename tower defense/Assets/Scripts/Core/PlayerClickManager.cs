using TowerDefense.Audio;
using TowerDefense.Enemies;
using TowerDefense.Towers;
using UnityEngine;

namespace TowerDefense.Core
{
    /// <summary>
    /// Handles manual player click-damage interaction on enemies.
    /// Includes click-rate throttling to prevent auto-clicker exploits (as outlined in design doc),
    /// and ensures clicks do not penetrate UI elements or interfere with tower placement.
    /// </summary>
    public class PlayerClickManager : MonoBehaviour
    {
        [Header("Click Combat Settings")]
        [Tooltip("Damage dealt per manual click.")]
        [SerializeField] private int clickDamage = 1;

        [Tooltip("Minimum time (in seconds) between allowed manual clicks to prevent auto-clicker abuse.")]
        [SerializeField] private float clickCooldown = 0.15f;

        [Header("Layer Masks")]
        [Tooltip("Layer assigned to enemies.")]
        [SerializeField] private LayerMask enemyLayerMask;

        [Header("Visual Feedback")]
        [Tooltip("Prefab spawned at click location (hit marker / spark).")]
        [SerializeField] private GameObject clickHitMarkerPrefab;

        private Camera mainCamera;
        private float lastClickTime = -1f;

        private void Awake()
        {
            mainCamera = Camera.main;
        }

        private void Update()
        {
            // Do not process manual attack clicks if placing a tower
            if (TowerPlacementManager.Instance != null && TowerPlacementManager.Instance.IsPlacingTower)
            {
                return;
            }

            if (InputHelper.GetLeftClickDown())
            {
                TryClickAttack();
            }
        }

        private void TryClickAttack()
        {
            // Ignore click if clicking over UI (e.g. Tower tray buttons, Pause, etc.)
            if (InputHelper.IsPointerOverUI())
            {
                return;
            }

            // Rate-limiting check to mitigate auto-clickers
            if (Time.time - lastClickTime < clickCooldown)
            {
                return;
            }

            if (mainCamera == null)
            {
                mainCamera = Camera.main;
                if (mainCamera == null) return;
            }

            Vector3 worldPos = InputHelper.GetMouseWorldPosition(mainCamera);
            Vector2 mousePos2D = new Vector2(worldPos.x, worldPos.y);

            // Raycast or overlap point for 2D colliders
            Collider2D hitCollider = Physics2D.OverlapPoint(mousePos2D, enemyLayerMask);

            // If no specific layer mask was hit or mask was set to default, check any collider for Enemy component
            if (hitCollider == null)
            {
                hitCollider = Physics2D.OverlapPoint(mousePos2D);
            }

            if (hitCollider != null)
            {
                Enemy enemy = hitCollider.GetComponentInParent<Enemy>();
                if (enemy != null && enemy.CurrentHealth > 0)
                {
                    lastClickTime = Time.time;
                    enemy.TakeDamage(clickDamage, isPlayerClick: true);

                    // Spawn hit marker visual
                    SpawnHitMarker(mousePos2D);
                }
            }
        }

        private void SpawnHitMarker(Vector2 position)
        {
            if (clickHitMarkerPrefab != null)
            {
                GameObject marker = Instantiate(clickHitMarkerPrefab, position, Quaternion.identity);
                Destroy(marker, 0.35f); // Auto-cleanup
            }
        }
    }
}
