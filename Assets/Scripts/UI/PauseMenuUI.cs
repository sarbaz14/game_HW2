using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using HW2.Core;

namespace HW2.UI
{
    public class PauseMenuUI : MonoBehaviour
    {
        [Header("Panels")]
        [SerializeField] private GameObject pausePanel;

        [Header("Buttons")]
        [SerializeField] private Button resumeButton;
        [SerializeField] private Button restartButton;
        [SerializeField] private Button mainMenuButton;

        private InputSystem_Actions inputActions;
        private float lastToggleTime = -1f;
        private const float ToggleCooldown = 0.25f;

        private void Awake()
        {
            inputActions = new InputSystem_Actions();
        }

        private void OnEnable()
        {
            if (inputActions != null)
            {
                inputActions.Player.Enable();
                inputActions.Player.Pause.performed += OnPauseActionPerformed;
            }
        }

        private void OnDisable()
        {
            if (inputActions != null)
            {
                inputActions.Player.Pause.performed -= OnPauseActionPerformed;
                inputActions.Player.Disable();
            }
        }

        private void Start()
        {
            if (pausePanel != null) pausePanel.SetActive(false);

            if (resumeButton != null) resumeButton.onClick.AddListener(OnResumeClicked);
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

            if (inputActions != null)
            {
                inputActions.Player.Pause.performed -= OnPauseActionPerformed;
                inputActions.Dispose();
                inputActions = null;
            }
        }

        private void OnPauseActionPerformed(InputAction.CallbackContext context)
        {
            TogglePause();
        }

        private void Update()
        {
            // Direct keyboard polling ensures Escape and P always respond immediately
            if (Keyboard.current != null)
            {
                if (Keyboard.current.escapeKey.wasPressedThisFrame || Keyboard.current.pKey.wasPressedThisFrame)
                {
                    TogglePause();
                }
            }
        }

        public void TogglePause()
        {
            if (Time.unscaledTime - lastToggleTime < ToggleCooldown) return;
            lastToggleTime = Time.unscaledTime;

            if (GameManager.Instance == null) return;

            // Never pause during game over or menu
            if (GameManager.Instance.CurrentState == GameManager.GameState.GameOver ||
                GameManager.Instance.CurrentState == GameManager.GameState.Menu)
            {
                return;
            }

            if (GameManager.Instance.CurrentState == GameManager.GameState.Playing)
            {
                GameManager.Instance.PauseGame();
            }
            else if (GameManager.Instance.CurrentState == GameManager.GameState.Paused)
            {
                GameManager.Instance.ResumeGame();
            }
        }

        private void HandleStateChanged(GameManager.GameState state)
        {
            if (pausePanel != null)
            {
                pausePanel.SetActive(state == GameManager.GameState.Paused);
            }
        }

        public void OnResumeClicked()
        {
            Time.timeScale = 1f;
            if (GameManager.Instance != null)
            {
                GameManager.Instance.ResumeGame();
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
