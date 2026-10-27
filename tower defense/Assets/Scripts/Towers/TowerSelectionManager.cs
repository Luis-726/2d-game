using System;
using TowerDefense.Core;
using TowerDefense.Enemies;
using UnityEngine;

namespace TowerDefense.Towers
{
    /// <summary>
    /// Coordinates clicking on placed towers in the game world to inspect, upgrade, or re-target them.
    /// Handles range circle visualizer toggling and deselects upon right-click or clicking empty terrain.
    /// </summary>
    public class TowerSelectionManager : MonoBehaviour
    {
        public static TowerSelectionManager Instance { get; private set; }

        public event Action<Tower> OnTowerSelected;
        public event Action OnTowerDeselected;

        private Tower currentSelectedTower;
        private Camera mainCamera;

        public Tower SelectedTower => currentSelectedTower;

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
            // Do not process tower inspection if actively in placement mode
            if (TowerPlacementManager.Instance != null && TowerPlacementManager.Instance.IsPlacingTower)
            {
                if (currentSelectedTower != null) DeselectTower();
                return;
            }

            // Right click or Escape deselects
            if (InputHelper.GetRightClickDown() || Input.GetKeyDown(KeyCode.Escape))
            {
                DeselectTower();
                return;
            }

            // Left click selection
            if (InputHelper.GetLeftClickDown())
            {
                TrySelectOrDeselect();
            }
        }

        private void TrySelectOrDeselect()
        {
            if (InputHelper.IsPointerOverUI()) return;

            if (mainCamera == null) mainCamera = Camera.main;
            if (mainCamera == null) return;

            Vector3 worldPos = InputHelper.GetMouseWorldPosition(mainCamera);
            Vector2 mousePos2D = new Vector2(worldPos.x, worldPos.y);

            // Check what is under the mouse
            Collider2D[] hits = Physics2D.OverlapPointAll(mousePos2D);

            // If an enemy is clicked, prioritize player manual click attack over tower selection
            foreach (var hit in hits)
            {
                if (hit.GetComponentInParent<Enemy>() != null)
                {
                    return;
                }
            }

            // Check if a placed tower was clicked
            Tower hitTower = null;
            foreach (var hit in hits)
            {
                hitTower = hit.GetComponentInParent<Tower>();
                if (hitTower != null) break;
            }

            if (hitTower != null)
            {
                SelectTower(hitTower);
            }
            else
            {
                // Clicked on empty ground
                DeselectTower();
            }
        }

        public void SelectTower(Tower tower)
        {
            if (tower == null)
            {
                DeselectTower();
                return;
            }

            if (currentSelectedTower != null)
            {
                currentSelectedTower.SetRangeCircleVisible(false);
            }

            currentSelectedTower = tower;
            currentSelectedTower.SetRangeCircleVisible(true);
            OnTowerSelected?.Invoke(currentSelectedTower);
        }

        public void DeselectTower()
        {
            if (currentSelectedTower != null)
            {
                currentSelectedTower.SetRangeCircleVisible(false);
                currentSelectedTower = null;
                OnTowerDeselected?.Invoke();
            }
        }
    }
}
