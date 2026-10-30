using TowerDefense.Audio;
using TowerDefense.Enemies;
using TowerDefense.Towers;
using UnityEngine;

namespace TowerDefense.Core
{
    
    /// keepes track of player's click damage
    /// prevent auto clickers
    /// prevents mouse from clicking outside of the UI
    
    public class PlayerClickManager : MonoBehaviour
    {
        [Header("Click Combat Settings")]
        [Tooltip("Damage dealt per manual click.")]
        [SerializeField] private int clickDamage = 1; // mouse click damage

        [Tooltip("Minimum time (in seconds) between allowed manual clicks to prevent auto-clicker abuse.")]
        [SerializeField] private float clickCooldown = 0.15f; // sets timer for clicks

        [Header("Layer Masks")]
        [Tooltip("Layer assigned to enemies.")]
        [SerializeField] private LayerMask enemyLayerMask; // gives enemies a tag so the towers knows if its an enemy or not

        [Header("Visual Feedback")]
        [Tooltip("Prefab spawned at click location (hit marker / spark).")] // creates a little hit marker for when the player clicks on an enemy
        [SerializeField] private GameObject clickHitMarkerPrefab;
        // helps the scene to go faster
        private Camera mainCamera;
        // checks for timing
        private float lastClickTime = -1f;
        // caches camera
        private void Awake()
        {
            mainCamera = Camera.main;
        }
        // checks if placing a tower or not
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
        // stops it from causing hit markers on UI
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
            Collider2D hitCollider = Physics2D.OverlapPoint(mousePos2D, enemyLayerMask); //checks for colliding projectiles

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
        // removes hit marker after a certain amount of time
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
