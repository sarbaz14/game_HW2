using System.Collections;
using UnityEngine;

namespace HW2.Core
{
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        [Header("Audio Sources")]
        [SerializeField] private AudioSource bgmSource;
        [SerializeField] private AudioSource engineSource;
        [SerializeField] private AudioSource sfxSource;

        [Header("Audio Clips (Optional - Fallbacks generated procedurally if null)")]
        [SerializeField] private AudioClip bgmClip;
        [SerializeField] private AudioClip engineClip;
        [SerializeField] private AudioClip coinClip;
        [SerializeField] private AudioClip fuelClip;
        [SerializeField] private AudioClip hitClip;
        [SerializeField] private AudioClip gameOverClip;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            EnsureAudioSources();
            SetupFallbackClips();
            PlayBGM();
            StartEngineSound();
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        private void EnsureAudioSources()
        {
            if (bgmSource == null)
            {
                bgmSource = gameObject.AddComponent<AudioSource>();
                bgmSource.loop = true;
                bgmSource.volume = 0.4f;
            }

            if (engineSource == null)
            {
                engineSource = gameObject.AddComponent<AudioSource>();
                engineSource.loop = true;
                engineSource.volume = 0.5f;
            }

            if (sfxSource == null)
            {
                sfxSource = gameObject.AddComponent<AudioSource>();
                sfxSource.loop = false;
                sfxSource.volume = 0.8f;
            }
        }

        private void SetupFallbackClips()
        {
            if (coinClip == null) coinClip = GenerateBeepClip(880f, 0.15f, true); // Chime
            if (fuelClip == null) fuelClip = GenerateBeepClip(520f, 0.25f, false); // Fuel tone
            if (hitClip == null) hitClip = GenerateNoiseClip(0.35f); // Crash noise
            if (gameOverClip == null) gameOverClip = GenerateGameOverClip();
            if (engineClip == null) engineClip = GenerateEngineHumClip();
        }

        public void PlayBGM()
        {
            if (bgmClip != null && bgmSource != null)
            {
                bgmSource.clip = bgmClip;
                bgmSource.Play();
            }
        }

        public void StartEngineSound()
        {
            if (engineSource != null && engineClip != null)
            {
                engineSource.clip = engineClip;
                engineSource.pitch = 0.8f;
                engineSource.Play();
            }
        }

        public void UpdateEnginePitch(float speedRatio)
        {
            if (engineSource != null)
            {
                // Pitch varies between 0.8 (idle) and 2.2 (top speed)
                engineSource.pitch = Mathf.Lerp(0.8f, 2.2f, Mathf.Clamp01(speedRatio));
            }
        }

        public void PlayCoinSound()
        {
            if (sfxSource != null && coinClip != null)
            {
                sfxSource.PlayOneShot(coinClip, 0.7f);
            }
        }

        public void PlayFuelSound()
        {
            if (sfxSource != null && fuelClip != null)
            {
                sfxSource.PlayOneShot(fuelClip, 0.9f);
            }
        }

        public void PlayHitSound()
        {
            if (sfxSource != null && hitClip != null)
            {
                sfxSource.PlayOneShot(hitClip, 1.0f);
            }
        }

        public void PlayGameOverSound()
        {
            if (engineSource != null) engineSource.Stop();
            if (sfxSource != null && gameOverClip != null)
            {
                sfxSource.PlayOneShot(gameOverClip, 1.0f);
            }
        }

        #region Procedural Audio Generators (Zero External Dependencies)
        private AudioClip GenerateBeepClip(float frequency, float length, bool ascending)
        {
            int sampleRate = 44100;
            int samples = Mathf.CeilToInt(sampleRate * length);
            float[] data = new float[samples];

            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / sampleRate;
                float currentFreq = ascending ? frequency + (t / length * frequency * 0.5f) : frequency;
                float env = 1f - ((float)i / samples);
                data[i] = Mathf.Sin(2 * Mathf.PI * currentFreq * t) * env;
            }

            AudioClip clip = AudioClip.Create("Chime_" + frequency, samples, 1, sampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        private AudioClip GenerateNoiseClip(float length)
        {
            int sampleRate = 44100;
            int samples = Mathf.CeilToInt(sampleRate * length);
            float[] data = new float[samples];
            System.Random rand = new System.Random(42);

            for (int i = 0; i < samples; i++)
            {
                float env = Mathf.Exp(-5f * ((float)i / samples));
                data[i] = ((float)rand.NextDouble() * 2f - 1f) * env;
            }

            AudioClip clip = AudioClip.Create("CrashNoise", samples, 1, sampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        private AudioClip GenerateGameOverClip()
        {
            int sampleRate = 44100;
            float length = 1.0f;
            int samples = Mathf.CeilToInt(sampleRate * length);
            float[] data = new float[samples];

            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / sampleRate;
                float freq = Mathf.Lerp(400f, 120f, t / length);
                float env = 1f - (t / length);
                data[i] = Mathf.Sin(2 * Mathf.PI * freq * t) * env * 0.8f;
            }

            AudioClip clip = AudioClip.Create("GameOverSfx", samples, 1, sampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        private AudioClip GenerateEngineHumClip()
        {
            int sampleRate = 44100;
            float length = 0.5f; // Short loopable buffer
            int samples = Mathf.CeilToInt(sampleRate * length);
            float[] data = new float[samples];

            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / sampleRate;
                float fundamental = Mathf.Sin(2 * Mathf.PI * 55f * t);
                float harmonic = Mathf.Sin(2 * Mathf.PI * 110f * t) * 0.5f;
                float rumble = (Mathf.PerlinNoise(t * 100f, 0f) * 2f - 1f) * 0.3f;
                data[i] = (fundamental + harmonic + rumble) * 0.5f;
            }

            AudioClip clip = AudioClip.Create("EngineHum", samples, 1, sampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }
        #endregion
    }
}
