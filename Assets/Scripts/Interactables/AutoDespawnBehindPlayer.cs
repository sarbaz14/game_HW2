using UnityEngine;
using HW2.Pooling;
using HW2.Player;

namespace HW2.Interactables
{
    public class AutoDespawnBehindPlayer : MonoBehaviour, IPoolable
    {
        [Header("Despawn Settings")]
        [SerializeField] private float despawnDistanceBehind = 35f;

        private Transform playerTransform;

        public void OnSpawn()
        {
            if (playerTransform == null)
            {
                var player = FindAnyObjectByType<PlayerController>();
                if (player != null) playerTransform = player.transform;
            }
        }

        public void OnDespawn()
        {
        }

        private void Update()
        {
            if (playerTransform == null)
            {
                var player = FindAnyObjectByType<PlayerController>();
                if (player != null) playerTransform = player.transform;
                return;
            }

            // If item has fallen far behind the player (beyond camera view), safely recycle it
            if (playerTransform.position.z - transform.position.z > despawnDistanceBehind)
            {
                if (PoolManager.Instance != null)
                {
                    PoolManager.Instance.Despawn(gameObject);
                }
            }
        }
    }
}
