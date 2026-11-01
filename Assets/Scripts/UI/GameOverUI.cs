using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;
using HW2.Core;

namespace HW2.UI
{
    public class GameOverUI : MonoBehaviour
    {
        [Header("Panel")]
        [SerializeField] private GameObject gameOverPanel;

        [Header("Texts")]
        [SerializeField] private TextMeshProUGUI currentScoreText;
        [SerializeField] private TextMeshProUGUI highScoreText;
        [SerializeField] private GameObject newRecordBadge;

        [Header("Buttons")]
        [SerializeField] private Button restartButton;
        [SerializeField] private Button mainMenuButton;

        private void Start()
        {
            if (gameOverPanel != null) gameOverPanel.SetActive(false);
            if (newRecordBadge != null) newRecordBadge.SetActive(false);

            if (restartButton != null) restartButton.onClick.AddListener(OnRestartClicked);
            if (mainMenuButton != null) mainMenuButton.onClick.AddListener(OnMainMenuClicked);

            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnStateChanged += HandleStateChanged;
            }
        }

        private void OnDestroy()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnStateChanged -= HandleStateChanged;
            }
        }

        private void Update()
        {
            if (gameOverPanel != null && gameOverPanel.activeSelf)
            {
                if (Keyboard.current != null)
                {
                    if (Keyboard.current.spaceKey.wasPressedThisFrame ||
                        Keyboard.current.enterKey.wasPressedThisFrame ||
                        Keyboard.current.numpadEnterKey.wasPressedThisFrame ||
                        Keyboard.current.rKey.wasPressedThisFrame)
                    {
                        OnRestartClicked();
                    }
                }
            }
        }

        private void HandleStateChanged(GameManager.GameState state)
        {
            if (state == GameManager.GameState.GameOver)
            {
                ShowGameOver();
            }
            else
            {
                if (gameOverPanel != null) gameOverPanel.SetActive(false);
            }
        }

        private void ShowGameOver()
        {
            if (gameOverPanel != null) gameOverPanel.SetActive(true);

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayGameOverSound();
            }

            int score = ScoreManager.Instance != null ? ScoreManager.Instance.CurrentScore : 0;
            int high = ScoreManager.Instance != null ? ScoreManager.Instance.HighScore : 0;

            if (currentScoreText != null)
            {
                currentScoreText.text = $"FINAL SCORE: {score:N0}";
            }

            if (highScoreText != null)
            {
                highScoreText.text = $"BEST RECORD: {high:N0}";
            }

            if (newRecordBadge != null)
            {
                newRecordBadge.SetActive(score >= high && score > 0);
            }
        }

        public void OnRestartClicked()
        {
            Time.timeScale = 1f;
            if (GameManager.Instance != null)
            {
                GameManager.Instance.RestartGame();
            }
            else
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
            }
        }

        public void OnMainMenuClicked()
        {
            Time.timeScale = 1f;
            if (GameManager.Instance != null)
            {
                GameManager.Instance.LoadMainMenu();
            }
            else
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
            }
        }
    }
}
