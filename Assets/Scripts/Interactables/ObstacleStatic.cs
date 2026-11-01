using UnityEngine;
using HW2.Player;

namespace HW2.Interactables
{
    public class ObstacleStatic : Interactable
    {
        [Header("Damage")]
        [SerializeField] private int damageAmount = 1;

        public override void OnInteract(PlayerController player)
        {
            if (player != null && player.Health != null)
            {
                player.Health.TakeDamage(damageAmount);
            }
        }
    }
}
