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
    /// Includes self-construction fallback if the panel was not pre-baked in the scene.
    /// </summary>
    public class TowerInfoUI : MonoBehaviour
    {
        public static TowerInfoUI Instance { get; private set; }

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

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
        }

        private void Start()
        {
            EnsurePanelConstructed();

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

        public void EnsurePanelConstructed()
        {
            if (panelRoot != null) return;

            Canvas canvas = GetComponentInParent<Canvas>() ?? FindFirstObjectByType<Canvas>();
            if (canvas == null) return;

            panelRoot = new GameObject("TowerInfoPanel");
            panelRoot.transform.SetParent(canvas.transform, false);

            RectTransform rt = panelRoot.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(1, 0.5f);
            rt.anchorMax = new Vector2(1, 0.5f);
            rt.pivot = new Vector2(1, 0.5f);
            rt.anchoredPosition = new Vector2(-20, 0);
            rt.sizeDelta = new Vector2(340, 260);

            Image bg = panelRoot.AddComponent<Image>();
            bg.color = new Color(0.12f, 0.12f, 0.18f, 0.95f);

            titleText = CreateText(panelRoot.transform, "InfoTitle", "Archer Tower (Lv 1)", 24, new Vector2(0.42f, 0.85f), new Vector2(240, 35), TextAlignmentOptions.Left);
            closeButton = CreateButton(panelRoot.transform, "CloseBtn", "X", new Vector2(0.92f, 0.85f), new Vector2(36, 36), new Color(0.6f, 0.2f, 0.2f));

            statsText = CreateText(panelRoot.transform, "StatsText", "Dmg: 3 | Range: 4.0 | Rate: 1.2/s", 18, new Vector2(0.5f, 0.65f), new Vector2(310, 40), TextAlignmentOptions.Center);

            targetButton = CreateButton(panelRoot.transform, "TargetBtn", "Target: First", new Vector2(0.5f, 0.44f), new Vector2(290, 42), new Color(0.25f, 0.45f, 0.7f));
            targetButtonText = targetButton.GetComponentInChildren<TextMeshProUGUI>();

            upgradeButton = CreateButton(panelRoot.transform, "UpgradeBtn", "Upgrade (11g)", new Vector2(0.32f, 0.18f), new Vector2(160, 48), new Color(0.2f, 0.65f, 0.3f));
            upgradeButtonText = upgradeButton.GetComponentInChildren<TextMeshProUGUI>();

            sellButton = CreateButton(panelRoot.transform, "SellBtn", "Sell (+9g)", new Vector2(0.78f, 0.18f), new Vector2(120, 48), new Color(0.65f, 0.3f, 0.2f));
            sellButtonText = sellButton.GetComponentInChildren<TextMeshProUGUI>();

            BindButtons();
            panelRoot.SetActive(false);
        }

        private TextMeshProUGUI CreateText(Transform parent, string name, string text, float fontSize, Vector2 anchorRatio, Vector2 size, TextAlignmentOptions align)
        {
            GameObject obj = new GameObject(name);
            obj.transform.SetParent(parent, false);
            RectTransform rt = obj.AddComponent<RectTransform>();
            rt.anchorMin = anchorRatio;
            rt.anchorMax = anchorRatio;
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = Vector2.zero;
            rt.sizeDelta = size;

            TextMeshProUGUI tmp = obj.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.alignment = align;
            tmp.color = Color.white;
            return tmp;
        }

        private Button CreateButton(Transform parent, string name, string label, Vector2 anchorRatio, Vector2 size, Color color)
        {
            GameObject obj = new GameObject(name);
            obj.transform.SetParent(parent, false);
            RectTransform rt = obj.AddComponent<RectTransform>();
            rt.anchorMin = anchorRatio;
            rt.anchorMax = anchorRatio;
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = Vector2.zero;
            rt.sizeDelta = size;

            Image img = obj.AddComponent<Image>();
            img.color = color;

            Button btn = obj.AddComponent<Button>();

            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(obj.transform, false);
            RectTransform trt = textObj.AddComponent<RectTransform>();
            trt.anchorMin = Vector2.zero;
            trt.anchorMax = Vector2.one;
            trt.sizeDelta = Vector2.zero;

            TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
            tmp.text = label;
            tmp.fontSize = 20;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = Color.white;

            return btn;
        }

        private void BindButtons()
        {
            if (targetButton != null)
            {
                targetButton.onClick.RemoveAllListeners();
                targetButton.onClick.AddListener(OnTargetButtonClicked);
            }

            if (upgradeButton != null)
            {
                upgradeButton.onClick.RemoveAllListeners();
                upgradeButton.onClick.AddListener(OnUpgradeButtonClicked);
            }

            if (sellButton != null)
            {
                sellButton.onClick.RemoveAllListeners();
                sellButton.onClick.AddListener(OnSellButtonClicked);
            }

            if (closeButton != null)
            {
                closeButton.onClick.RemoveAllListeners();
                closeButton.onClick.AddListener(OnCloseButtonClicked);
            }
        }

        private void HandleTowerSelected(Tower tower)
        {
            EnsurePanelConstructed();

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
