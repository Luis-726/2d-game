using TMPro;
using TowerDefense.Core;
using TowerDefense.Towers;
using UnityEngine;
using UnityEngine.UI;

namespace TowerDefense.UI
{
    /// <summary>
    /// Displays detailed stats for the actively selected tower.
    /// Provides interactive buttons to upgrade tower stats, change targeting priority, and sell the tower.
    /// </summary>
    public class TowerInfoUI : MonoBehaviour
    {
        [Header("Panel Root")]
        [SerializeField] private GameObject panelRoot;

        [Header("Text Fields")]
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI statsText;
        [SerializeField] private TextMeshProUGUI targetButtonText;
        [SerializeField] private TextMeshProUGUI upgradeButtonText;
        [SerializeField] private TextMeshProUGUI sellButtonText;

        [Header("Action Buttons")]
        [SerializeField] private Button targetButton;
        [SerializeField] private Button upgradeButton;
        [SerializeField] private Button sellButton;
        [SerializeField] private Button closeButton;

        private Tower activeTower;

        private void Start()
        {
            if (TowerSelectionManager.Instance != null)
            {
                TowerSelectionManager.Instance.OnTowerSelected += HandleTowerSelected;
                TowerSelectionManager.Instance.OnTowerDeselected += HandleTowerDeselected;
            }

            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnCoinsChanged += HandleCoinsChanged;
            }

            BindButtons();

            if (panelRoot != null)
            {
                panelRoot.SetActive(false);
            }
        }

        private void OnDestroy()
        {
            if (TowerSelectionManager.Instance != null)
            {
                TowerSelectionManager.Instance.OnTowerSelected -= HandleTowerSelected;
                TowerSelectionManager.Instance.OnTowerDeselected -= HandleTowerDeselected;
            }

            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnCoinsChanged -= HandleCoinsChanged;
            }

            if (activeTower != null)
            {
                activeTower.OnTowerUpdated -= RefreshUI;
            }
        }

        private void BindButtons()
        {
            if (targetButton != null)
            {
                targetButton.onClick.AddListener(OnTargetButtonClicked);
            }

            if (upgradeButton != null)
            {
                upgradeButton.onClick.AddListener(OnUpgradeButtonClicked);
            }

            if (sellButton != null)
            {
                sellButton.onClick.AddListener(OnSellButtonClicked);
            }

            if (closeButton != null)
            {
                closeButton.onClick.AddListener(OnCloseButtonClicked);
            }
        }

        private void HandleTowerSelected(Tower tower)
        {
            if (activeTower != null)
            {
                activeTower.OnTowerUpdated -= RefreshUI;
            }

            activeTower = tower;
            if (activeTower != null)
            {
                activeTower.OnTowerUpdated += RefreshUI;
                RefreshUI();
                if (panelRoot != null) panelRoot.SetActive(true);
            }
        }

        private void HandleTowerDeselected()
        {
            if (activeTower != null)
            {
                activeTower.OnTowerUpdated -= RefreshUI;
                activeTower = null;
            }
            if (panelRoot != null) panelRoot.SetActive(false);
        }

        private void HandleCoinsChanged(int currentCoins)
        {
            if (activeTower != null)
            {
                UpdateUpgradeButtonState();
            }
        }

        private void RefreshUI()
        {
            if (activeTower == null)
            {
                if (panelRoot != null) panelRoot.SetActive(false);
                return;
            }

            if (titleText != null)
            {
                titleText.text = $"{activeTower.TowerName} (Lv {activeTower.Level})";
            }

            if (statsText != null)
            {
                statsText.text = $"Dmg: {activeTower.Damage} | Range: {activeTower.Range:F1} | Rate: {activeTower.AttacksPerSecond:F1}/s";
            }

            if (targetButtonText != null)
            {
                targetButtonText.text = $"Target: {activeTower.Priority}";
            }

            UpdateUpgradeButtonState();

            if (sellButtonText != null)
            {
                sellButtonText.text = $"Sell (+{activeTower.SellValue}g)";
            }
        }

        private void UpdateUpgradeButtonState()
        {
            if (activeTower == null) return;

            if (activeTower.CanUpgrade)
            {
                int cost = activeTower.UpgradeCost;
                if (upgradeButtonText != null) upgradeButtonText.text = $"Upgrade ({cost}g)";

                int coins = (GameManager.Instance != null) ? GameManager.Instance.CurrentCoins : 0;
                if (upgradeButton != null) upgradeButton.interactable = coins >= cost;
            }
            else
            {
                if (upgradeButtonText != null) upgradeButtonText.text = "MAX LEVEL";
                if (upgradeButton != null) upgradeButton.interactable = false;
            }
        }

        private void OnTargetButtonClicked()
        {
            if (activeTower != null)
            {
                activeTower.CycleTargetPriority();
            }
        }

        private void OnUpgradeButtonClicked()
        {
            if (activeTower == null || !activeTower.CanUpgrade) return;

            int cost = activeTower.UpgradeCost;
            if (GameManager.Instance != null && GameManager.Instance.TrySpendCoins(cost))
            {
                activeTower.Upgrade();
                RefreshUI();
            }
        }

        private void OnSellButtonClicked()
        {
            if (activeTower != null)
            {
                Tower towerToSell = activeTower;
                TowerSelectionManager.Instance?.DeselectTower();
                towerToSell.Sell();
            }
        }

        private void OnCloseButtonClicked()
        {
            TowerSelectionManager.Instance?.DeselectTower();
        }
    }
}
