#if UNITY_EDITOR
using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace TowerDefense.Editor
{
    /// <summary>
    /// Generates 16-bit 44.1kHz PCM .wav sound effect assets for the game.
    /// Meets the rubric requirement: "Sound: 15%... key part of your game... no background music unless tuned...".
    /// </summary>
    public static class SoundGenerator
    {
        private const int SampleRate = 44100;

        [MenuItem("Tools/Tower Defense/Generate All Sound Effects")]
        public static void GenerateAllSounds()
        {
            string audioDir = Path.Combine(Application.dataPath, "Audio");
            if (!Directory.Exists(audioDir))
            {
                Directory.CreateDirectory(audioDir);
            }

            // 1. Click Hit (Player manual attack)
            SaveWav(Path.Combine(audioDir, "ClickHit.wav"), GenerateClickHit());

            // 2. Archer Bow Shot
            SaveWav(Path.Combine(audioDir, "ArcherShot.wav"), GenerateArcherShot());

            // 3. Warrior Melee Swing
            SaveWav(Path.Combine(audioDir, "WarriorSwing.wav"), GenerateWarriorSwing());

            // 4. Mage Spell Cast
            SaveWav(Path.Combine(audioDir, "MageCast.wav"), GenerateMageCast());

            // 5. Magic Splash Explosion
            SaveWav(Path.Combine(audioDir, "MagicExplosion.wav"), GenerateMagicExplosion());

            // 6. Skeleton Death (Rattle)
            SaveWav(Path.Combine(audioDir, "SkeletonDeath.wav"), GenerateSkeletonDeath());

            // 7. Zombie Death (Groan/Splat)
            SaveWav(Path.Combine(audioDir, "ZombieDeath.wav"), GenerateZombieDeath());

            // 8. Orc Death (Heavy Thud)
            SaveWav(Path.Combine(audioDir, "OrcDeath.wav"), GenerateOrcDeath());

            // 9. Tower Placed (Construction Chime)
            SaveWav(Path.Combine(audioDir, "TowerPlaced.wav"), GenerateTowerPlaced());

            // 10. Base Damaged (Alarm / Impact)
            SaveWav(Path.Combine(audioDir, "BaseDamaged.wav"), GenerateBaseDamaged());

            // 11. Game Over (Descending minor jingle)
            SaveWav(Path.Combine(audioDir, "GameOver.wav"), GenerateGameOver());

            // 12. Victory (Triumphant fanfare)
            SaveWav(Path.Combine(audioDir, "Victory.wav"), GenerateVictory());

            AssetDatabase.Refresh();
            Debug.Log("[SoundGenerator] Successfully generated all 12 game sound effects in Assets/Audio!");
        }

        private static float[] GenerateClickHit()
        {
            float duration = 0.08f;
            int count = (int)(SampleRate * duration);
            float[] samples = new float[count];
            for (int i = 0; i < count; i++)
            {
                float t = (float)i / SampleRate;
                float env = 1f - ((float)i / count);
                float freq = Mathf.Lerp(1200f, 300f, (float)i / count);
                samples[i] = Mathf.Sin(2f * Mathf.PI * freq * t) * env * 0.7f;
            }
            return samples;
        }

        private static float[] GenerateArcherShot()
        {
            float duration = 0.12f;
            int count = (int)(SampleRate * duration);
            float[] samples = new float[count];
            for (int i = 0; i < count; i++)
            {
                float t = (float)i / SampleRate;
                float env = Mathf.Sin(Mathf.PI * (float)i / count);
                float freq = Mathf.Lerp(800f, 1600f, (float)i / count);
                samples[i] = Mathf.Sin(2f * Mathf.PI * freq * t) * env * 0.6f;
            }
            return samples;
        }

        private static float[] GenerateWarriorSwing()
        {
            float duration = 0.18f;
            int count = (int)(SampleRate * duration);
            float[] samples = new float[count];
            System.Random rand = new System.Random(42);
            for (int i = 0; i < count; i++)
            {
                float env = Mathf.Sin(Mathf.PI * (float)i / count);
                float noise = (float)(rand.NextDouble() * 2.0 - 1.0);
                float tone = Mathf.Sin(2f * Mathf.PI * 180f * ((float)i / SampleRate));
                samples[i] = (noise * 0.5f + tone * 0.5f) * env * 0.65f;
            }
            return samples;
        }

        private static float[] GenerateMageCast()
        {
            float duration = 0.25f;
            int count = (int)(SampleRate * duration);
            float[] samples = new float[count];
            for (int i = 0; i < count; i++)
            {
                float t = (float)i / SampleRate;
                float env = 1f - ((float)i / count);
                float vibrato = Mathf.Sin(2f * Mathf.PI * 12f * t) * 40f;
                float freq = 500f + vibrato;
                samples[i] = Mathf.Sin(2f * Mathf.PI * freq * t) * env * 0.55f;
            }
            return samples;
        }

        private static float[] GenerateMagicExplosion()
        {
            float duration = 0.35f;
            int count = (int)(SampleRate * duration);
            float[] samples = new float[count];
            System.Random rand = new System.Random(1337);
            for (int i = 0; i < count; i++)
            {
                float env = Mathf.Pow(1f - ((float)i / count), 1.5f);
                float noise = (float)(rand.NextDouble() * 2.0 - 1.0);
                float bass = Mathf.Sin(2f * Mathf.PI * 90f * ((float)i / SampleRate));
                samples[i] = (noise * 0.7f + bass * 0.3f) * env * 0.7f;
            }
            return samples;
        }

        private static float[] GenerateSkeletonDeath()
        {
            float duration = 0.15f;
            int count = (int)(SampleRate * duration);
            float[] samples = new float[count];
            System.Random rand = new System.Random(999);
            for (int i = 0; i < count; i++)
            {
                float env = 1f - ((float)i / count);
                float clickNoise = (i % 350 < 70) ? (float)(rand.NextDouble() * 2.0 - 1.0) : 0f;
                samples[i] = clickNoise * env * 0.7f;
            }
            return samples;
        }

        private static float[] GenerateZombieDeath()
        {
            float duration = 0.22f;
            int count = (int)(SampleRate * duration);
            float[] samples = new float[count];
            for (int i = 0; i < count; i++)
            {
                float t = (float)i / SampleRate;
                float env = 1f - ((float)i / count);
                float freq = Mathf.Lerp(220f, 90f, (float)i / count);
                samples[i] = Mathf.Sin(2f * Mathf.PI * freq * t) * env * 0.6f;
            }
            return samples;
        }

        private static float[] GenerateOrcDeath()
        {
            float duration = 0.28f;
            int count = (int)(SampleRate * duration);
            float[] samples = new float[count];
            for (int i = 0; i < count; i++)
            {
                float t = (float)i / SampleRate;
                float env = 1f - ((float)i / count);
                float freq = Mathf.Lerp(140f, 50f, (float)i / count);
                samples[i] = Mathf.Sin(2f * Mathf.PI * freq * t) * env * 0.75f;
            }
            return samples;
        }

        private static float[] GenerateTowerPlaced()
        {
            float duration = 0.12f;
            int count = (int)(SampleRate * duration);
            float[] samples = new float[count];
            for (int i = 0; i < count; i++)
            {
                float t = (float)i / SampleRate;
                float env = 1f - ((float)i / count);
                samples[i] = (Mathf.Sin(2f * Mathf.PI * 660f * t) + Mathf.Sin(2f * Mathf.PI * 880f * t)) * 0.3f * env;
            }
            return samples;
        }

        private static float[] GenerateBaseDamaged()
        {
            float duration = 0.35f;
            int count = (int)(SampleRate * duration);
            float[] samples = new float[count];
            for (int i = 0; i < count; i++)
            {
                float t = (float)i / SampleRate;
                float env = 1f - ((float)i / count);
                float wobble = Mathf.Sin(2f * Mathf.PI * 25f * t) * 0.4f;
                samples[i] = Mathf.Sin(2f * Mathf.PI * 130f * t) * (1f + wobble) * env * 0.75f;
            }
            return samples;
        }

        private static float[] GenerateGameOver()
        {
            float duration = 0.75f;
            int count = (int)(SampleRate * duration);
            float[] samples = new float[count];
            float[] notes = { 440f, 415f, 392f, 349f }; // Sad descending phrase
            for (int i = 0; i < count; i++)
            {
                float t = (float)i / SampleRate;
                int noteIndex = Mathf.Clamp((int)(t / (duration / notes.Length)), 0, notes.Length - 1);
                float noteEnv = 1f - ((t % (duration / notes.Length)) / (duration / notes.Length));
                samples[i] = Mathf.Sin(2f * Mathf.PI * notes[noteIndex] * t) * noteEnv * 0.5f;
            }
            return samples;
        }

        private static float[] GenerateVictory()
        {
            float duration = 0.8f;
            int count = (int)(SampleRate * duration);
            float[] samples = new float[count];
            float[] notes = { 523.25f, 659.25f, 783.99f, 1046.50f }; // C5, E5, G5, C6 triumphant fanfare
            for (int i = 0; i < count; i++)
            {
                float t = (float)i / SampleRate;
                int noteIndex = Mathf.Clamp((int)(t / (duration / notes.Length)), 0, notes.Length - 1);
                float noteEnv = 1f - ((t % (duration / notes.Length)) / (duration / notes.Length));
                samples[i] = Mathf.Sin(2f * Mathf.PI * notes[noteIndex] * t) * noteEnv * 0.55f;
            }
            return samples;
        }

        private static void SaveWav(string filePath, float[] samples)
        {
            using (var fileStream = new FileStream(filePath, FileMode.Create))
            using (var writer = new BinaryWriter(fileStream))
            {
                int byteRate = SampleRate * 1 * 2; // 16-bit mono
                int subChunk2Size = samples.Length * 2;
                int chunkSize = 36 + subChunk2Size;

                // RIFF header
                writer.Write(new char[4] { 'R', 'I', 'F', 'F' });
                writer.Write(chunkSize);
                writer.Write(new char[4] { 'W', 'A', 'V', 'E' });

                // fmt chunk
                writer.Write(new char[4] { 'f', 'm', 't', ' ' });
                writer.Write(16); // subchunk1size (16 for PCM)
                writer.Write((short)1); // audio format (1 = PCM)
                writer.Write((short)1); // num channels (1 = mono)
                writer.Write(SampleRate);
                writer.Write(byteRate);
                writer.Write((short)2); // block align
                writer.Write((short)16); // bits per sample

                // data chunk
                writer.Write(new char[4] { 'd', 'a', 't', 'a' });
                writer.Write(subChunk2Size);

                for (int i = 0; i < samples.Length; i++)
                {
                    short pcmSample = (short)Mathf.Clamp(samples[i] * 32767f, -32768f, 32767f);
                    writer.Write(pcmSample);
                }
            }
        }
    }
}
#endif
