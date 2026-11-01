using System.Collections;
using UnityEngine;
using HW2.Player;

namespace HW2.VFX
{
    public class FireworksController : MonoBehaviour
    {
        [Header("Spawn Settings")]
        [SerializeField] private ParticleSystem[] fireworksPrefabs;
        [SerializeField] private float minInterval = 4.0f;
        [SerializeField] private float maxInterval = 8.0f;
        [SerializeField] private float spawnHeight = 22f;
        [SerializeField] private float forwardOffset = 70f;
        [SerializeField] private float sideSpread = 35f;

        [Header("Player Target")]
        [SerializeField] private Transform playerTransform;

        private void Start()
        {
            if (playerTransform == null)
            {
                var player = FindAnyObjectByType<PlayerController>();
                if (player != null) playerTransform = player.transform;
            }

            StartCoroutine(FireworksLoop());
        }

        private IEnumerator FireworksLoop()
        {
            while (true)
            {
                float waitTime = Random.Range(minInterval, maxInterval);
                yield return new WaitForSeconds(waitTime);

                LaunchFirework();
            }
        }

        public void LaunchFirework()
        {
            float playerZ = playerTransform != null ? playerTransform.position.z : transform.position.z;

            // Compute sky position ahead of the player
            float x = Random.Range(-sideSpread, sideSpread);
            float y = spawnHeight + Random.Range(-3f, 6f);
            float z = playerZ + forwardOffset + Random.Range(0f, 30f);

            Vector3 burstPos = new Vector3(x, y, z);

            if (fireworksPrefabs != null && fireworksPrefabs.Length > 0)
            {
                int idx = Random.Range(0, fireworksPrefabs.Length);
                if (fireworksPrefabs[idx] != null)
                {
                    ParticleSystem ps = Instantiate(fireworksPrefabs[idx], burstPos, Quaternion.identity);
                    Destroy(ps.gameObject, 4.0f);
                }
            }
        }
    }
}
