using System;
using UnityEngine;

namespace HW2.Core
{
    public class ScoreManager : MonoBehaviour
    {
        public static ScoreManager Instance { get; private set; }

        private const string HIGH_SCORE_KEY = "HW2_HighScore";

        [Header("Score Values")]
        [SerializeField] private int pointsPerCoin = 100;
        [SerializeField] private float scorePerMeter = 1f;

        private float distanceTraveled = 0f;
        private int coinsCollected = 0;
        private int currentScore = 0;
        private int highScore = 0;

        public int CurrentScore => currentScore;
        public int HighScore => highScore;
        public int CoinsCollected => coinsCollected;
        public float DistanceTraveled => distanceTraveled;

        public event Action<int> OnScoreChanged;
        public event Action<int> OnCoinsChanged;
        public event Action<int> OnHighScoreUpdated;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            LoadHighScore();
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        public void ResetScore()
        {
            distanceTraveled = 0f;
            coinsCollected = 0;
            currentScore = 0;
            OnScoreChanged?.Invoke(currentScore);
            OnCoinsChanged?.Invoke(coinsCollected);
        }

        public void AddDistance(float deltaDistance)
        {
            if (deltaDistance <= 0f) return;
            distanceTraveled += deltaDistance;
            RecalculateScore();
        }

        public void AddCoin(int count = 1)
        {
            coinsCollected += count;
            OnCoinsChanged?.Invoke(coinsCollected);
            RecalculateScore();
        }

        private void RecalculateScore()
        {
            int newScore = Mathf.FloorToInt(distanceTraveled * scorePerMeter) + (coinsCollected * pointsPerCoin);
            if (newScore != currentScore)
            {
                currentScore = newScore;
                OnScoreChanged?.Invoke(currentScore);

                if (currentScore > highScore)
                {
                    highScore = currentScore;
                    SaveHighScore(highScore);
                    OnHighScoreUpdated?.Invoke(highScore);
                }
            }
        }

        public void SaveHighScore(int score)
        {
            PlayerPrefs.SetInt(HIGH_SCORE_KEY, score);
            PlayerPrefs.Save();
        }

        private void LoadHighScore()
        {
            highScore = PlayerPrefs.GetInt(HIGH_SCORE_KEY, 0);
        }
    }
}
