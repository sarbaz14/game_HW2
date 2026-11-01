using UnityEngine;
using HW2.Player;
using HW2.Core;

namespace HW2.Interactables
{
    public class Coin : Interactable
    {
        [Header("Coin Settings")]
        [SerializeField] private float rotationSpeed = 160f;
        [SerializeField] private int coinValue = 1;

        private void Update()
        {
            if (GameManager.Instance != null && GameManager.Instance.CurrentState != GameManager.GameState.Playing)
            {
                return;
            }

            transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime, Space.World);
        }

        public override void OnInteract(PlayerController player)
        {
            if (ScoreManager.Instance != null)
            {
                ScoreManager.Instance.AddCoin(coinValue);
            }

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayCoinSound();
            }
        }
    }
}
