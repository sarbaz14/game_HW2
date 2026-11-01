using UnityEngine;
using HW2.Core;

namespace HW2.Difficulty
{
    public class DifficultyManager : MonoBehaviour
    {
        public static DifficultyManager Instance { get; private set; }

        [Header("Speed Progression")]
        [SerializeField] private float initialSpeed = 28f;
        [SerializeField] private float maxSpeed = 56f;
        [SerializeField] private float speedAccelerationPerSecond = 0.35f;

        [Header("Spawn Rate Progression")]
        [SerializeField] private float initialSpawnInterval = 2.2f;
        [SerializeField] private float minSpawnInterval = 0.85f;

        private float currentSpeed;
        private float currentSpawnInterval;

        public float CurrentSpeed => currentSpeed;
        public float CurrentSpeedKmh => currentSpeed * 3.6f;
        public float CurrentSpawnInterval => currentSpawnInterval;
        public float SpeedRatio => Mathf.Clamp01((currentSpeed - initialSpeed) / (maxSpeed - initialSpeed));

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            currentSpeed = initialSpeed;
            currentSpawnInterval = initialSpawnInterval;
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        private void Update()
        {
            if (GameManager.Instance != null && GameManager.Instance.CurrentState != GameManager.GameState.Playing)
            {
                return;
            }

            if (currentSpeed < maxSpeed)
            {
                currentSpeed += speedAccelerationPerSecond * Time.deltaTime;
                if (currentSpeed > maxSpeed) currentSpeed = maxSpeed;
            }

            // Lerp spawn interval with speed progression
            currentSpawnInterval = Mathf.Lerp(initialSpawnInterval, minSpawnInterval, SpeedRatio);

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.UpdateEnginePitch(SpeedRatio);
            }
        }
    }
}
