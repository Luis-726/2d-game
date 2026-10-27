using TMPro;
using TowerDefense.Core;
using TowerDefense.Towers;
using UnityEngine;
using UnityEngine.UI;

namespace TowerDefense.UI
{
    /// <summary>
    /// Manages the heads-up display (HUD), tower selection tray, wave buttons, and game state panels.
    /// </summary>
    public class UIManager : MonoBehaviour
    {
        public static UIManager Instance { get; private set; }

        [Header("Top HUD Elements")]
        [SerializeField] private TextMeshProUGUI livesText;
        [SerializeField] private TextMeshProUGUI coinsText;
        [SerializeField] private TextMeshProUGUI waveText;
        [SerializeField] private Button nextWaveButton;

        [Header("Tower Purchase Buttons")]
        [SerializeField] private Button archerButton;
        [SerializeField] private Button warriorButton;
        [SerializeField] private Button mageButton;

        [Header("Tower Prefabs To Place")]
        [SerializeField] private Tower archerPrefab;
        [SerializeField] private Tower warriorPrefab;
        [SerializeField] private Tower magePrefab;

        [Header("State Panels")]
        [SerializeField] private GameObject startControlsHintPanel;
        [SerializeField] private GameObject gameOverPanel;
        [SerializeField] private GameObject victoryPanel;
        [SerializeField] private Button restartButtonGameOver;
        [SerializeField] private Button restartButtonVictory;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            BindButtonEvents();
        }

        private void Start()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnLivesChanged += UpdateLivesUI;
                GameManager.Instance.OnCoinsChanged += UpdateCoinsUI;
                GameManager.Instance.OnGameStateChanged += HandleGameStateChanged;

                UpdateLivesUI(GameManager.Instance.CurrentLives);
                UpdateCoinsUI(GameManager.Instance.CurrentCoins);
            }

            if (WaveManager.Instance != null)
            {
                WaveManager.Instance.OnWaveChanged += UpdateWaveUI;
            }

            if (gameOverPanel != null) gameOverPanel.SetActive(false);
            if (victoryPanel != null) victoryPanel.SetActive(false);
            if (startControlsHintPanel != null) startControlsHintPanel.SetActive(true);
        }

        private void OnDestroy()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnLivesChanged -= UpdateLivesUI;
                GameManager.Instance.OnCoinsChanged -= UpdateCoinsUI;
                GameManager.Instance.OnGameStateChanged -= HandleGameStateChanged;
            }

            if (WaveManager.Instance != null)
            {
                WaveManager.Instance.OnWaveChanged -= UpdateWaveUI;
            }
        }

        private void BindButtonEvents()
        {
            if (nextWaveButton != null)
            {
                nextWaveButton.onClick.AddListener(() =>
                {
                    if (startControlsHintPanel != null) startControlsHintPanel.SetActive(false);
                    WaveManager.Instance?.StartNextWave();
                });
            }

            if (archerButton != null)
            {
                archerButton.onClick.AddListener(() => TowerPlacementManager.Instance?.SelectTowerToPlace(archerPrefab));
            }

            if (warriorButton != null)
            {
                warriorButton.onClick.AddListener(() => TowerPlacementManager.Instance?.SelectTowerToPlace(warriorPrefab));
            }

            if (mageButton != null)
            {
                mageButton.onClick.AddListener(() => TowerPlacementManager.Instance?.SelectTowerToPlace(magePrefab));
            }

            if (restartButtonGameOver != null)
            {
                restartButtonGameOver.onClick.AddListener(() => GameManager.Instance?.RestartGame());
            }

            if (restartButtonVictory != null)
            {
                restartButtonVictory.onClick.AddListener(() => GameManager.Instance?.RestartGame());
            }
        }

        private void UpdateLivesUI(int lives)
        {
            if (livesText != null)
            {
                livesText.text = $"Lives: {lives}";
            }
        }

        private void UpdateCoinsUI(int coins)
        {
            if (coinsText != null)
            {
                coinsText.text = $"Coins: {coins}";
            }

            // Update affordability tint or interactability on tower buttons
            UpdateTowerButtonAffordability(coins);
        }

        private void UpdateTowerButtonAffordability(int coins)
        {
            if (archerButton != null && archerPrefab != null)
            {
                archerButton.interactable = coins >= archerPrefab.BuildCost;
            }
            if (warriorButton != null && warriorPrefab != null)
            {
                warriorButton.interactable = coins >= warriorPrefab.BuildCost;
            }
            if (mageButton != null && magePrefab != null)
            {
                mageButton.interactable = coins >= magePrefab.BuildCost;
            }
        }

        private void UpdateWaveUI(int currentWave, int totalWaves)
        {
            if (waveText != null)
            {
                waveText.text = $"Wave: {currentWave} / {totalWaves}";
            }
        }

        private void HandleGameStateChanged(GameState state)
        {
            switch (state)
            {
                case GameState.PreWave:
                    if (nextWaveButton != null) nextWaveButton.interactable = true;
                    break;

                case GameState.WaveInProgress:
                    if (nextWaveButton != null) nextWaveButton.interactable = false;
                    break;

                case GameState.GameOver:
                    if (gameOverPanel != null) gameOverPanel.SetActive(true);
                    break;

                case GameState.Victory:
                    if (victoryPanel != null) victoryPanel.SetActive(true);
                    break;
            }
        }
    }
}
