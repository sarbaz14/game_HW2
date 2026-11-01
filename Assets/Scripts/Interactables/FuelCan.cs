using UnityEngine;
using HW2.Player;
using HW2.Core;

namespace HW2.Interactables
{
    public class FuelCan : Interactable
    {
        [Header("Fuel Can Settings")]
        [SerializeField] private float refuelAmount = 35f;
        [SerializeField] private float bobbingSpeed = 3f;
        [SerializeField] private float bobbingHeight = 0.2f;

        private Vector3 startLocalPosition;
        private float seed;

        public override void OnSpawn()
        {
            base.OnSpawn();
            startLocalPosition = transform.position;
            seed = Random.value * 10f;
        }

        private void Update()
        {
            if (GameManager.Instance != null && GameManager.Instance.CurrentState != GameManager.GameState.Playing)
            {
                return;
            }

            // Gentle bobbing effect
            float yOffset = Mathf.Sin((Time.time + seed) * bobbingSpeed) * bobbingHeight;
            transform.position = new Vector3(transform.position.x, startLocalPosition.y + yOffset, transform.position.z);
            transform.Rotate(Vector3.up, 60f * Time.deltaTime, Space.World);
        }

        public override void OnInteract(PlayerController player)
        {
            if (player != null && player.Fuel != null)
            {
                player.Fuel.AddFuel(refuelAmount);
            }
        }
    }
}
