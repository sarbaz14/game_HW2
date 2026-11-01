using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;
using HW2.Core;

namespace HW2.UI
{
    public class MainMenuUI : MonoBehaviour
    {
        [Header("Buttons")]
        [SerializeField] private Button playButton;
        [SerializeField] private Button quitButton;

        [Header("Text")]
        [SerializeField] private TextMeshProUGUI highScoreText;

        private void Start()
        {
            if (playButton != null) playButton.onClick.AddListener(OnPlayClicked);
            if (quitButton != null) quitButton.onClick.AddListener(OnQuitClicked);

            int high = PlayerPrefs.GetInt("HW2_HighScore", 0);
            if (highScoreText != null)
            {
                highScoreText.text = $"★ BEST RECORD: {high:N0} ★";
            }
        }

        private void Update()
        {
            if (Keyboard.current != null)
            {
                if (Keyboard.current.spaceKey.wasPressedThisFrame ||
                    Keyboard.current.enterKey.wasPressedThisFrame ||
                    Keyboard.current.numpadEnterKey.wasPressedThisFrame)
                {
                    OnPlayClicked();
                }
            }
        }

        public void OnPlayClicked()
        {
            Time.timeScale = 1f;
            if (GameManager.Instance != null)
            {
                GameManager.Instance.StartGame();
            }
            else
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene("GameScene");
            }
        }

        public void OnQuitClicked()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.QuitGame();
            }
            else
            {
#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#else
                Application.Quit();
#endif
            }
        }
    }
}
