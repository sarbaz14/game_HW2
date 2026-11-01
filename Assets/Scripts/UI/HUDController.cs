using UnityEngine;
using UnityEngine.UI;
using TMPro;
using HW2.Core;
using HW2.Player;
using HW2.Difficulty;

namespace HW2.UI
{
    public class HUDController : MonoBehaviour
    {
        [Header("Speed Display")]
        [SerializeField] private TextMeshProUGUI speedText;

        [Header("Fuel Display")]
        [SerializeField] private Slider fuelSlider;
        [SerializeField] private Image fuelFillImage;
        [SerializeField] private TextMeshProUGUI fuelPercentText;
        [SerializeField] private Color fuelHighColor = new Color(0.1f, 0.95f, 0.35f);
        [SerializeField] private Color fuelLowColor = new Color(1f, 0.2f, 0.1f);

        [Header("Lives Display")]
        [SerializeField] private GameObject[] lifeIcons;
        [SerializeField] private TextMeshProUGUI livesText;

        [Header("Score Display")]
        [SerializeField] private TextMeshProUGUI scoreText;
        [SerializeField] private TextMeshProUGUI highScoreText;

        [Header("Player References")]
        [SerializeField] private PlayerHealth playerHealth;
        [SerializeField] private FuelSystem fuelSystem;

        private static readonly Color ColorGreen = new Color(0.05f, 0.95f, 0.35f, 1f); // 3 lives
        private static readonly Color ColorYellow = new Color(1.0f, 0.88f, 0.1f, 1f);  // 2 lives
        private static readonly Color ColorRed = new Color(1.0f, 0.15f, 0.2f, 1f);     // 1 life

        private void Start()
        {
            if (playerHealth == null)
            {
                playerHealth = FindAnyObjectByType<PlayerHealth>();
            }

            if (fuelSystem == null)
            {
                fuelSystem = FindAnyObjectByType<FuelSystem>();
            }

            if (playerHealth != null)
            {
                playerHealth.OnLivesChanged += UpdateLivesDisplay;
                UpdateLivesDisplay(playerHealth.CurrentLives);
            }

            if (fuelSystem != null)
            {
                fuelSystem.OnFuelChanged += UpdateFuelDisplay;
                UpdateFuelDisplay(fuelSystem.CurrentFuel, fuelSystem.MaxFuel);
            }

            if (ScoreManager.Instance != null)
            {
                ScoreManager.Instance.OnScoreChanged += UpdateScoreDisplay;
                ScoreManager.Instance.OnHighScoreUpdated += UpdateHighScoreDisplay;
                UpdateScoreDisplay(ScoreManager.Instance.CurrentScore);
                UpdateHighScoreDisplay(ScoreManager.Instance.HighScore);
            }
        }

        private void OnDestroy()
        {
            if (playerHealth != null) playerHealth.OnLivesChanged -= UpdateLivesDisplay;
            if (fuelSystem != null) fuelSystem.OnFuelChanged -= UpdateFuelDisplay;
            if (ScoreManager.Instance != null)
            {
                ScoreManager.Instance.OnScoreChanged -= UpdateScoreDisplay;
                ScoreManager.Instance.OnHighScoreUpdated -= UpdateHighScoreDisplay;
            }
        }

        private void Update()
        {
            if (speedText != null && DifficultyManager.Instance != null)
            {
                speedText.text = $"{Mathf.RoundToInt(DifficultyManager.Instance.CurrentSpeedKmh)} <size=55%>KM/H</size>";
            }
        }

        private void UpdateFuelDisplay(float current, float max)
        {
            float ratio = max > 0f ? current / max : 0f;
            if (fuelSlider != null)
            {
                fuelSlider.value = ratio;
            }

            if (fuelFillImage != null)
            {
                fuelFillImage.color = Color.Lerp(fuelLowColor, fuelHighColor, ratio);
            }

            if (fuelPercentText != null)
            {
                fuelPercentText.text = $"FUEL: {Mathf.CeilToInt(ratio * 100f)}%";
            }
        }

        private void UpdateLivesDisplay(int lives)
        {
            Color activeColor;
            if (lives >= 3) activeColor = ColorGreen;
            else if (lives == 2) activeColor = ColorYellow;
            else if (lives == 1) activeColor = ColorRed;
            else activeColor = Color.gray;

            if (lifeIcons != null)
            {
                for (int i = 0; i < lifeIcons.Length; i++)
                {
                    if (lifeIcons[i] != null)
                    {
                        bool shouldBeActive = i < lives;
                        lifeIcons[i].SetActive(shouldBeActive);
                        if (shouldBeActive)
                        {
                            var img = lifeIcons[i].GetComponent<Image>();
                            if (img != null) img.color = activeColor;
                        }
                    }
                }
            }

            if (livesText != null)
            {
                livesText.text = $"LIVES: {Mathf.Max(0, lives)}/3";
                livesText.color = activeColor;
            }
        }

        private void UpdateScoreDisplay(int score)
        {
            if (scoreText != null)
            {
                scoreText.text = $"SCORE: {score:N0}";
            }
        }

        private void UpdateHighScoreDisplay(int highScore)
        {
            if (highScoreText != null)
            {
                highScoreText.text = $"BEST: {highScore:N0}";
            }
        }
    }
}
