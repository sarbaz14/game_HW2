using System.Collections;
using UnityEngine;

namespace HW2.Player
{
    public class CameraFollow : MonoBehaviour
    {
        public static CameraFollow Instance { get; private set; }

        [Header("Target & Offset")]
        [SerializeField] private Transform target;
        [SerializeField] private Vector3 offset = new Vector3(0f, 4.2f, -7.5f);
        [SerializeField] private float smoothSpeed = 10f;
        [SerializeField] private float lookAheadDistance = 15f;

        private Vector3 shakeOffset = Vector3.zero;
        private Coroutine shakeCoroutine;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void Start()
        {
            if (target == null)
            {
                var player = FindAnyObjectByType<PlayerController>();
                if (player != null) target = player.transform;
            }

            if (target != null)
            {
                var health = target.GetComponent<PlayerHealth>();
                if (health != null)
                {
                    health.OnDamaged += TriggerShake;
                }
            }
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        private void LateUpdate()
        {
            if (target == null) return;

            Vector3 targetPos = target.position + offset + shakeOffset;
            // Smoothly interpolate position (especially X lane transitions)
            Vector3 smoothPos = Vector3.Lerp(transform.position, targetPos, smoothSpeed * Time.deltaTime);
            transform.position = smoothPos;

            // Look towards road ahead
            Vector3 lookTarget = target.position + Vector3.forward * lookAheadDistance + Vector3.up * 1f;
            transform.LookAt(lookTarget);
        }

        public void TriggerShake()
        {
            TriggerShake(0.35f, 0.45f);
        }

        public void TriggerShake(float duration, float magnitude)
        {
            if (shakeCoroutine != null) StopCoroutine(shakeCoroutine);
            shakeCoroutine = StartCoroutine(ShakeRoutine(duration, magnitude));
        }

        private IEnumerator ShakeRoutine(float duration, float magnitude)
        {
            float elapsed = 0f;
            while (elapsed < duration)
            {
                float x = (Random.value * 2f - 1f) * magnitude;
                float y = (Random.value * 2f - 1f) * magnitude;
                shakeOffset = new Vector3(x, y, 0f);

                elapsed += Time.unscaledDeltaTime;
                yield return null;
            }
            shakeOffset = Vector3.zero;
        }
    }
}
