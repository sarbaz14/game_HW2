using System.Collections;
using UnityEngine;
using HW2.Pooling;

namespace HW2.VFX
{
    public class ParticleAutoReturn : MonoBehaviour, IPoolable
    {
        [SerializeField] private float duration = 1.5f;
        private Coroutine returnCoroutine;

        public void OnSpawn()
        {
            var ps = GetComponent<ParticleSystem>();
            if (ps != null)
            {
                ps.Clear();
                ps.Play();
            }

            if (returnCoroutine != null) StopCoroutine(returnCoroutine);
            returnCoroutine = StartCoroutine(ReturnRoutine());
        }

        public void OnDespawn()
        {
            if (returnCoroutine != null)
            {
                StopCoroutine(returnCoroutine);
                returnCoroutine = null;
            }
        }

        private IEnumerator ReturnRoutine()
        {
            yield return new WaitForSeconds(duration);
            if (PoolManager.Instance != null)
            {
                PoolManager.Instance.Despawn(gameObject);
            }
        }
    }
}
