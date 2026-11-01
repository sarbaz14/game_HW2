using System;
using UnityEngine;
using HW2.Core;

namespace HW2.Player
{
    public class FuelSystem : MonoBehaviour
    {
        [Header("Fuel Settings")]
        [SerializeField] private float maxFuel = 100f;
        [SerializeField] private float burnRatePerSecond = 4.5f;
        [SerializeField] private float defaultRefuelAmount = 30f;

        private float currentFuel;

        public float CurrentFuel => currentFuel;
        public float MaxFuel => maxFuel;
        public float FuelNormalized => Mathf.Clamp01(currentFuel / maxFuel);

        public event Action<float, float> OnFuelChanged;
        public event Action OnFuelDepleted;

        private void Awake()
        {
            currentFuel = maxFuel;
        }

        private void Start()
        {
            OnFuelChanged?.Invoke(currentFuel, maxFuel);
        }

        private void Update()
        {
            if (GameManager.Instance != null && GameManager.Instance.CurrentState != GameManager.GameState.Playing)
            {
                return;
            }

            if (currentFuel > 0f)
            {
                currentFuel -= burnRatePerSecond * Time.deltaTime;
                if (currentFuel <= 0f)
                {
                    currentFuel = 0f;
                    OnFuelChanged?.Invoke(currentFuel, maxFuel);
                    OnFuelDepleted?.Invoke();

                    if (GameManager.Instance != null)
                    {
                        GameManager.Instance.TriggerGameOver();
                    }
                }
                else
                {
                    OnFuelChanged?.Invoke(currentFuel, maxFuel);
                }
            }
        }

        public void AddFuel(float amount = -1f)
        {
            if (amount <= 0f) amount = defaultRefuelAmount;

            currentFuel = Mathf.Min(maxFuel, currentFuel + amount);
            OnFuelChanged?.Invoke(currentFuel, maxFuel);

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayFuelSound();
            }
        }
    }
}
