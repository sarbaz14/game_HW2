using System;
using System.Collections;
using UnityEngine;
using HW2.Core;

namespace HW2.Player
{
    public class PlayerHealth : MonoBehaviour
    {
        [Header("Health Settings")]
        [SerializeField] private int maxLives = 3;
        [SerializeField] private float invulnerabilityDuration = 1.5f;

        [Header("Visual Feedback")]
        [SerializeField] private Renderer[] carRenderers;
        [SerializeField] private Color damageFlashColor = Color.red;

        private int currentLives;
        private bool isInvulnerable = false;
        private Color[] originalColors;

        public int CurrentLives => currentLives;
        public int MaxLives => maxLives;
        public bool IsInvulnerable => isInvulnerable;

        public event Action<int> OnLivesChanged;
        public event Action OnDamaged;

        private void Awake()
        {
            currentLives = maxLives;
            StoreOriginalColors();
        }

        private void Start()
        {
            OnLivesChanged?.Invoke(currentLives);
        }

        private void StoreOriginalColors()
        {
            if (carRenderers == null || carRenderers.Length == 0)
            {
                carRenderers = GetComponentsInChildren<Renderer>();
            }

            if (carRenderers != null && carRenderers.Length > 0)
            {
                originalColors = new Color[carRenderers.Length];
                for (int i = 0; i < carRenderers.Length; i++)
                {
                    if (carRenderers[i].material != null && carRenderers[i].material.HasProperty("_BaseColor"))
                    {
                        originalColors[i] = carRenderers[i].material.GetColor("_BaseColor");
                    }
                    else if (carRenderers[i].material != null && carRenderers[i].material.HasProperty("_Color"))
                    {
                        originalColors[i] = carRenderers[i].material.color;
                    }
                }
            }
        }

        public void TakeDamage(int amount = 1)
        {
            if (isInvulnerable || currentLives <= 0) return;

            currentLives -= amount;
            if (currentLives < 0) currentLives = 0;

            OnLivesChanged?.Invoke(currentLives);
            OnDamaged?.Invoke();

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayHitSound();
            }

            if (currentLives <= 0)
            {
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.TriggerGameOver();
                }
            }
            else
            {
                StartCoroutine(InvulnerabilityRoutine());
            }
        }

        private IEnumerator InvulnerabilityRoutine()
        {
            isInvulnerable = true;
            float elapsed = 0f;
            float flickerInterval = 0.1f;
            bool isRed = false;

            while (elapsed < invulnerabilityDuration)
            {
                isRed = !isRed;
                ApplyFlashColor(isRed);
                yield return new WaitForSeconds(flickerInterval);
                elapsed += flickerInterval;
            }

            ApplyOriginalColors();
            isInvulnerable = false;
        }

        private void ApplyFlashColor(bool flash)
        {
            if (carRenderers == null) return;

            for (int i = 0; i < carRenderers.Length; i++)
            {
                if (carRenderers[i] == null || carRenderers[i].material == null) continue;

                Color c = flash ? damageFlashColor : (originalColors != null && i < originalColors.Length ? originalColors[i] : Color.white);
                if (carRenderers[i].material.HasProperty("_BaseColor"))
                    carRenderers[i].material.SetColor("_BaseColor", c);
                else if (carRenderers[i].material.HasProperty("_Color"))
                    carRenderers[i].material.color = c;
            }
        }

        private void ApplyOriginalColors()
        {
            ApplyFlashColor(false);
        }
    }
}
