using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TowerDefense.Core
{
    /// <summary>
    /// Represents the high-level states of the game.
    /// </summary>
    public enum GameState
    {
        PreWave,        // Waiting for the player to start the wave / placing towers
        WaveInProgress, // Enemies are actively spawning and moving
        GameOver,       // Player lives reached 0
        Victory         // All waves successfully cleared
    }

    /// <summary>
    /// Central manager overseeing game state, player economy (coins), and base health.
    /// Follows the Singleton pattern for accessible global event broadcasting.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("Starting Stats (Rubric & Design Specs)")]
        [Tooltip("Starting player lives. Specified as 25 in design document.")]
        [SerializeField] private int startingLives = 25;

        [Tooltip("Starting player coins. Specified as 0 in design document (players click enemies first).")]
        [SerializeField] private int startingCoins = 0;

        [Header("Runtime Stats")]
        [SerializeField] private int currentLives;
        [SerializeField] private int currentCoins;
        [SerializeField] private GameState currentState = GameState.PreWave;

        // Events for decoupled UI updates
        public event Action<int> OnLivesChanged;
        public event Action<int> OnCoinsChanged;
        public event Action<GameState> OnGameStateChanged;

        public int CurrentLives => currentLives;
        public int CurrentCoins => currentCoins;
        public GameState CurrentState => currentState;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            currentLives = startingLives;
            currentCoins = startingCoins;
        }

        private void Start()
        {
            SetState(GameState.PreWave);
            OnLivesChanged?.Invoke(currentLives);
            OnCoinsChanged?.Invoke(currentCoins);
        }

        /// <summary>
        /// Updates the current game state and triggers state change listeners.
        /// </summary>
        public void SetState(GameState newState)
        {
            if (currentState == newState) return;

            currentState = newState;
            Debug.Log($"[GameManager] Game State changed to: {newState}");
            OnGameStateChanged?.Invoke(currentState);

            if (currentState == GameState.GameOver)
            {
                Time.timeScale = 0f;
            }
        }

        /// <summary>
        /// Adds coins to the player's balance (e.g. from defeating enemies).
        /// </summary>
        public void AddCoins(int amount)
        {
            if (amount <= 0) return;
            currentCoins += amount;
            OnCoinsChanged?.Invoke(currentCoins);
        }

        /// <summary>
        /// Attempts to spend coins. Returns true if successful, false if insufficient funds.
        /// </summary>
        public bool TrySpendCoins(int amount)
        {
            if (amount < 0) return false;
            if (currentCoins >= amount)
            {
                currentCoins -= amount;
                OnCoinsChanged?.Invoke(currentCoins);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Deducts lives when an enemy reaches the end of the path.
        /// </summary>
        public void DeductLives(int amount)
        {
            if (currentState == GameState.GameOver) return;

            currentLives = Mathf.Max(0, currentLives - amount);
            OnLivesChanged?.Invoke(currentLives);

            if (currentLives <= 0)
            {
                SetState(GameState.GameOver);
            }
        }

        /// <summary>
        /// Restarts the active scene and restores time scale.
        /// </summary>
        public void RestartGame()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }
}
