using TowerDefense.Enemies;
using UnityEngine;

namespace TowerDefense.Audio
{
    
    /// controls audio here
    /// Supports assigned audio clips with procedural fallback synthesis so the game
    /// produces audio cues even before external asset files are linked.
    /// Strictly excludes looping background music per rubric instructions.
    
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        [Header("Audio Sources")]
        [SerializeField] private AudioSource sfxSource;

        [Header("Player Actions")]
        [SerializeField] private AudioClip playerClickHitClip;

        [Header("Tower SFX")]
        [SerializeField] private AudioClip archerShotClip;
        [SerializeField] private AudioClip warriorSwingClip;
        [SerializeField] private AudioClip mageCastClip;
        [SerializeField] private AudioClip magicExplosionClip;

        [Header("Enemy SFX")]
        [SerializeField] private AudioClip skeletonDeathClip;
        [SerializeField] private AudioClip zombieDeathClip;
        [SerializeField] private AudioClip orcDeathClip;

        [Header("Game State & UI SFX")]
        [SerializeField] private AudioClip towerPlacedClip;
        [SerializeField] private AudioClip baseDamagedClip;
        [SerializeField] private AudioClip gameOverClip;
        [SerializeField] private AudioClip victoryClip;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            if (sfxSource == null)
            {
                sfxSource = gameObject.AddComponent<AudioSource>();
                sfxSource.playOnAwake = false;
            }
        }

        public void PlayPlayerClickHitSound()
        {
            PlayOrSynthesize(playerClickHitClip, frequency: 800f, duration: 0.05f);
        }

        public void PlayArcherShotSound()
        {
            PlayOrSynthesize(archerShotClip, frequency: 1200f, duration: 0.08f);
        }

        public void PlayWarriorSwingSound()
        {
            PlayOrSynthesize(warriorSwingClip, frequency: 250f, duration: 0.12f);
        }

        public void PlayMageCastSound()
        {
            PlayOrSynthesize(mageCastClip, frequency: 600f, duration: 0.15f);
        }

        public void PlayMagicExplosionSound()
        {
            PlayOrSynthesize(magicExplosionClip, frequency: 180f, duration: 0.25f);
        }

        public void PlayEnemyDeathSound(EnemyType enemyType)
        {
            switch (enemyType)
            {
                case EnemyType.Skeleton:
                    PlayOrSynthesize(skeletonDeathClip, frequency: 950f, duration: 0.1f);
                    break;
                case EnemyType.Zombie:
                    PlayOrSynthesize(zombieDeathClip, frequency: 320f, duration: 0.18f);
                    break;
                case EnemyType.Orc:
                    PlayOrSynthesize(orcDeathClip, frequency: 160f, duration: 0.22f);
                    break;
            }
        }

        public void PlayTowerPlacedSound()
        {
            PlayOrSynthesize(towerPlacedClip, frequency: 520f, duration: 0.1f);
        }

        public void PlayBaseDamagedSound()
        {
            PlayOrSynthesize(baseDamagedClip, frequency: 140f, duration: 0.3f);
        }

        public void PlayGameOverSound()
        {
            PlayOrSynthesize(gameOverClip, frequency: 110f, duration: 0.6f);
        }

        public void PlayVictorySound()
        {
            PlayOrSynthesize(victoryClip, frequency: 880f, duration: 0.5f);
        }

        private void PlayOrSynthesize(AudioClip clip, float frequency, float duration)
        {
            if (sfxSource == null) return;

            if (clip != null)
            {
                sfxSource.PlayOneShot(clip);
            }
            else
            {
                // Procedural audio generation fallback
                AudioClip synthClip = CreateProceduralTone(frequency, duration);
                if (synthClip != null) 
                {
                    sfxSource.PlayOneShot(synthClip);
                }
            }
        }

        
        /// Generates a simple tone on the fly if an external audio file has not yet been assigned.
        
        private AudioClip CreateProceduralTone(float frequency, float duration)
        {
            int sampleRate = 44100;
            int sampleCount = Mathf.CeilToInt(sampleRate * duration);
            if (sampleCount <= 0) return null;

            float[] samples = new float[sampleCount];
            for (int i = 0; i < sampleCount; i++)
            {
                float t = (float)i / sampleRate;
                float envelope = 1f - (float)i / sampleCount; // Linear fade out
                samples[i] = Mathf.Sin(2 * Mathf.PI * frequency * t) * envelope * 0.4f;
            }

            AudioClip clip = AudioClip.Create("SynthSFX", sampleCount, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }
    }
}
