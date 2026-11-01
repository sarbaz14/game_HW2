using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace HW2.Core
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        public enum GameState
        {
            Menu,
            Playing,
            Paused,
            GameOver
        }

        [Header("State")]
        [SerializeField] private GameState currentState = GameState.Playing;
        public GameState CurrentState => currentState;

        public event Action<GameState> OnStateChanged;
        public event Action OnGameRestart;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            // اگر در GameScene بودیم وضعیت Playing است، در غیر این صورت Menu
            string activeScene = SceneManager.GetActiveScene().name;
            if (activeScene.Equals("MainMenu", StringComparison.OrdinalIgnoreCase))
            {
                SetState(GameState.Menu);
            }
            else
            {
                SetState(GameState.Playing);
            }
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        public void SetState(GameState newState)
        {
            if (currentState == newState && Time.timeScale != 0f) return;

            currentState = newState;

            switch (currentState)
            {
                case GameState.Menu:
                    Time.timeScale = 1f;
                    break;
                case GameState.Playing:
                    Time.timeScale = 1f;
                    break;
                case GameState.Paused:
                    Time.timeScale = 0f;
                    break;
                case GameState.GameOver:
                    Time.timeScale = 0f;
                    break;
            }

            OnStateChanged?.Invoke(currentState);
        }

        public void StartGame()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene("GameScene");
        }

        public void LoadMainMenu()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene("MainMenu");
        }

        public void PauseGame()
        {
            if (currentState == GameState.Playing)
            {
                SetState(GameState.Paused);
            }
        }

        public void ResumeGame()
        {
            if (currentState == GameState.Paused)
            {
                SetState(GameState.Playing);
            }
        }

        public void TriggerGameOver()
        {
            if (currentState != GameState.GameOver)
            {
                SetState(GameState.GameOver);
            }
        }

        public void RestartGame()
        {
            Time.timeScale = 1f;
            OnGameRestart?.Invoke();
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        public void QuitGame()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
