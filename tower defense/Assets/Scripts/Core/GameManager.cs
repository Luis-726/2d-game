using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TowerDefense.Core
{
    
    /// tells player the level states of the game.
    
    public enum GameState
    {
        PreWave,        // Waits for the player to start the wave / placing towers
        WaveInProgress, // Enemies are spawning and moving
        GameOver,       // Player lives reached 0
        Victory         // Beat all the waves
    }

    /// Keeps track of lives and money
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("Starting Stats (Rubric & Design Specs)")]
        [Tooltip("Starting player lives. Specified as 25 in design document.")] // tells player how many lives they have
        [SerializeField] private int startingLives = 25;

        [Tooltip("Starting player coins. Specified as 0 in design document (players click enemies first).")] // tells player how to earn points
        [SerializeField] private int startingCoins = 0;

        [Header("Runtime Stats")]
        [SerializeField] private int currentLives;
        [SerializeField] private int currentCoins;
        [SerializeField] private GameState currentState = GameState.PreWave;

        // Events for decoupled UI updates
        // allows for ui to look neat
        public event Action<int> OnLivesChanged;
        public event Action<int> OnCoinsChanged;
        public event Action<GameState> OnGameStateChanged;

        public int CurrentLives => currentLives;
        public int CurrentCoins => currentCoins;
        public GameState CurrentState => currentState;

        // helps prevent multiple copies from exisiting in one place
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

        // allows game to start
        private void Start()
        {
            SetState(GameState.PreWave);
            OnLivesChanged?.Invoke(currentLives);
            OnCoinsChanged?.Invoke(currentCoins);
        }

        
        /// Updates the current game state and triggers state change listeners.
        /// keeps track whether the game is progressing or not
        
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

        
        /// Adds coins to the player's balance (e.g. from defeating enemies).
        /// 
        
        public void AddCoins(int amount)
        {
            if (amount <= 0) return;
            currentCoins += amount;
            OnCoinsChanged?.Invoke(currentCoins);
        }

        
        /// Attempts to spend coins. Returns true if successful, false if insufficient funds.
        /// allows player to spend coin and if not enough funds then it wont allow the player to buy the tower
        
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

        
        /// Deducts lives when an enemy reaches the end of the path.
        /// lose health
        
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

        
        /// Restarts the active scene and restores time scale.
        /// lets user to restart when losing or beating the game
        
        public void RestartGame()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }
}
