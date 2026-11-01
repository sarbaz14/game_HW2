using UnityEngine;
using HW2.Pooling;
using HW2.Player;

namespace HW2.Interactables
{
    [RequireComponent(typeof(Collider))]
    public abstract class Interactable : MonoBehaviour, IPoolable
    {
        [Header("Base Interactable Settings")]
        [SerializeField] protected bool despawnOnInteract = true;
        [SerializeField] protected GameObject pickupEffectPrefab;

        protected bool hasInteracted = false;

        public virtual void OnSpawn()
        {
            hasInteracted = false;
            gameObject.SetActive(true);
        }

        public virtual void OnDespawn()
        {
            hasInteracted = false;
        }

        public abstract void OnInteract(PlayerController player);

        protected virtual void OnTriggerEnter(Collider other)
        {
            if (hasInteracted) return;

            PlayerController player = other.GetComponentInParent<PlayerController>();
            if (player != null)
            {
                hasInteracted = true;
                OnInteract(player);

                if (pickupEffectPrefab != null && PoolManager.Instance != null)
                {
                    PoolManager.Instance.Spawn(pickupEffectPrefab, transform.position, Quaternion.identity);
                }

                if (despawnOnInteract && PoolManager.Instance != null)
                {
                    PoolManager.Instance.Despawn(gameObject);
                }
            }
        }
    }
}
