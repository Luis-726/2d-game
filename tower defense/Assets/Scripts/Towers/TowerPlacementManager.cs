using TowerDefense.Audio;
using TowerDefense.Core;
using UnityEngine;

namespace TowerDefense.Towers
{
    /// <summary>
    /// Coordinates tower purchasing, placement preview, grid/spot validation, and placement cancellation.
    /// Implements design document mitigation allowing optional restriction of placements to between waves.
    /// </summary>
    public class TowerPlacementManager : MonoBehaviour
    {
        public static TowerPlacementManager Instance { get; private set; }

        [Header("Placement Rules")]
        [Tooltip("If true, towers can only be placed during PreWave phase to avoid accidental clicks during combat.")]
        [SerializeField] private bool restrictPlacementToPreWave = false;

        [Tooltip("Layer mask for forbidden placement areas (such as the enemy path or obstacles).")]
        [SerializeField] private LayerMask blockedLayerMask;

        [Header("Preview Visuals")]
        [SerializeField] private Color validPlacementColor = new Color(0.2f, 1f, 0.2f, 0.65f);
        [SerializeField] private Color invalidPlacementColor = new Color(1f, 0.2f, 0.2f, 0.65f);

        private Tower selectedTowerPrefab;
        private GameObject previewInstance;
        private SpriteRenderer previewRenderer;
        private Camera mainCamera;

        public bool IsPlacingTower => selectedTowerPrefab != null;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            mainCamera = Camera.main;
        }

        private void Update()
        {
            if (!IsPlacingTower) return;

            // Right click cancels current placement choice (Per design doc spec)
            if (InputHelper.GetRightClickDown() || Input.GetKeyDown(KeyCode.Escape))
            {
                CancelPlacement();
                return;
            }

            UpdatePreviewPosition();

            // Left click to confirm placement
            if (InputHelper.GetLeftClickDown())
            {
                TryPlaceTower();
            }
        }

        /// <summary>
        /// Initiates placement mode with the selected tower prefab.
        /// </summary>
        public void SelectTowerToPlace(Tower towerPrefab)
        {
            if (towerPrefab == null) return;

            if (restrictPlacementToPreWave && GameManager.Instance != null && GameManager.Instance.CurrentState != GameState.PreWave)
            {
                Debug.LogWarning("[TowerPlacementManager] Placements are restricted to between waves!");
                return;
            }

            if (GameManager.Instance != null && GameManager.Instance.CurrentCoins < towerPrefab.BuildCost)
            {
                Debug.LogWarning($"[TowerPlacementManager] Not enough coins! Need {towerPrefab.BuildCost}, have {GameManager.Instance.CurrentCoins}.");
                return;
            }

            CancelPlacement();

            selectedTowerPrefab = towerPrefab;

            // Instantiate ghost preview
            previewInstance = new GameObject("TowerPlacementPreview");
            previewRenderer = previewInstance.AddComponent<SpriteRenderer>();

            SpriteRenderer sourceRenderer = selectedTowerPrefab.GetComponentInChildren<SpriteRenderer>();
            if (sourceRenderer != null)
            {
                previewRenderer.sprite = sourceRenderer.sprite;
                previewRenderer.sortingOrder = 10;
            }

            UpdatePreviewPosition();
        }

        private void UpdatePreviewPosition()
        {
            if (previewInstance == null) return;

            if (mainCamera == null) mainCamera = Camera.main;
            if (mainCamera == null) return;

            Vector3 mouseWorldPos = InputHelper.GetMouseWorldPosition(mainCamera);
            mouseWorldPos.z = 0;
            previewInstance.transform.position = mouseWorldPos;

            bool canPlace = CanPlaceAt(mouseWorldPos);
            if (previewRenderer != null)
            {
                previewRenderer.color = canPlace ? validPlacementColor : invalidPlacementColor;
            }
        }

        private bool CanPlaceAt(Vector3 position)
        {
            // Cannot place if pointer is hovering over UI elements
            if (InputHelper.IsPointerOverUI())
            {
                return false;
            }

            // Verify coins
            if (GameManager.Instance != null && GameManager.Instance.CurrentCoins < selectedTowerPrefab.BuildCost)
            {
                return false;
            }

            // Check if blocked by path, other towers, or obstacles
            Collider2D overlap = Physics2D.OverlapCircle(position, 0.45f, blockedLayerMask);
            return overlap == null;
        }

        private void TryPlaceTower()
        {
            if (InputHelper.IsPointerOverUI())
            {
                return;
            }

            if (previewInstance == null || selectedTowerPrefab == null) return;

            Vector3 placePos = previewInstance.transform.position;

            if (CanPlaceAt(placePos))
            {
                if (GameManager.Instance != null && GameManager.Instance.TrySpendCoins(selectedTowerPrefab.BuildCost))
                {
                    Instantiate(selectedTowerPrefab, placePos, Quaternion.identity);
                    AudioManager.Instance?.PlayTowerPlacedSound();
                    CancelPlacement();
                }
            }
        }

        /// <summary>
        /// Cancels the active placement mode and cleans up preview visuals.
        /// </summary>
        public void CancelPlacement()
        {
            selectedTowerPrefab = null;
            if (previewInstance != null)
            {
                Destroy(previewInstance);
                previewInstance = null;
            }
        }
    }
}
