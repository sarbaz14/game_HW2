using UnityEngine;
using HW2.Player;
using HW2.Core;

namespace HW2.Interactables
{
    public class ObstacleMobile : Interactable
    {
        [Header("Movement")]
        [SerializeField] private float speed = 8f;
        [SerializeField] private int damageAmount = 1;

        public override void OnInteract(PlayerController player)
        {
            if (player != null && player.Health != null)
            {
                player.Health.TakeDamage(damageAmount);
            }
        }

        private void Update()
        {
            if (GameManager.Instance != null && GameManager.Instance.CurrentState != GameManager.GameState.Playing)
            {
                return;
            }

            // Move forward along the road
            transform.Translate(Vector3.forward * (speed * Time.deltaTime), Space.World);
        }
    }
}
