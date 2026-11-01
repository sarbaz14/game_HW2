using UnityEngine;
using HW2.Difficulty;

namespace HW2.VFX
{
    public class NeonLightController : MonoBehaviour
    {
        [Header("Underglow Lights")]
        [SerializeField] private Light[] underglowLights;
        [SerializeField] private float minIntensity = 1.2f;
        [SerializeField] private float maxIntensity = 5.0f;
        [SerializeField] private float minRange = 3f;
        [SerializeField] private float maxRange = 7f;

        [Header("Emissive Underglow Mesh / Renderer (Optional)")]
        [SerializeField] private Renderer neonStripRenderer;
        [SerializeField] private Color neonColor = new Color(0f, 0.85f, 1f, 1f); // Neon Cyan

        [Header("Headlights")]
        [SerializeField] private Light[] headlights;
        [SerializeField] private float headlightIntensity = 3.5f;

        private Material neonMaterialInstance;

        private void Start()
        {
            if (neonStripRenderer != null)
            {
                neonMaterialInstance = neonStripRenderer.material;
            }

            if (headlights != null)
            {
                for (int i = 0; i < headlights.Length; i++)
                {
                    if (headlights[i] != null)
                    {
                        headlights[i].intensity = headlightIntensity;
                    }
                }
            }
        }

        private void Update()
        {
            float speedRatio = DifficultyManager.Instance != null ? DifficultyManager.Instance.SpeedRatio : 0f;

            // Modulate intensity & range with speed
            float targetIntensity = Mathf.Lerp(minIntensity, maxIntensity, speedRatio);
            float targetRange = Mathf.Lerp(minRange, maxRange, speedRatio);

            if (underglowLights != null)
            {
                for (int i = 0; i < underglowLights.Length; i++)
                {
                    if (underglowLights[i] != null)
                    {
                        underglowLights[i].intensity = targetIntensity;
                        underglowLights[i].range = targetRange;
                        underglowLights[i].color = neonColor;
                    }
                }
            }

            // Also scale HDR emission for Bloom
            if (neonMaterialInstance != null)
            {
                float emissionScale = Mathf.Lerp(1.5f, 4.5f, speedRatio);
                Color hdrColor = neonColor * Mathf.LinearToGammaSpace(emissionScale);
                neonMaterialInstance.SetColor("_EmissionColor", hdrColor);
            }
        }
    }
}
