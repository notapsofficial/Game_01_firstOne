using UnityEngine;

namespace SimpleRPG
{
    /// <summary>
    /// Procedural BGM generator - creates a cheerful chiptune-style loop at runtime.
    /// No external audio files required.
    /// </summary>
    public class SimpleRPGBGM : MonoBehaviour
    {
        [Range(0f, 1f)]
        public float volume = 0.3f;

        private AudioSource audioSource;
        private const int sampleRate = 44100;
        private const float bpm = 140f;
        private const float beatDuration = 60f / bpm;

        // C Major pentatonic melody notes (MIDI numbers)
        private static readonly int[] melodyNotes = {
            60, 64, 67, 72, 71, 67, 64, 60,  // C4 E4 G4 C5 B4 G4 E4 C4
            62, 65, 69, 72, 69, 65, 62, 60,  // D4 F4 A4 C5 A4 F4 D4 C4
            64, 67, 72, 76, 72, 67, 64, 60,  // E4 G4 C5 E5 C5 G4 E4 C4
            60, 62, 64, 67, 72, 67, 64, 62   // C4 D4 E4 G4 C5 G4 E4 D4
        };

        // Bass line (root notes)
        private static readonly int[] bassNotes = {
            48, 48, 53, 53, 55, 55, 48, 48,  // C3 C3 F3 F3 G3 G3 C3 C3
            48, 48, 53, 53, 55, 55, 48, 48,
            48, 48, 53, 53, 55, 55, 48, 48,
            48, 48, 53, 53, 55, 55, 48, 48
        };

        private void Start()
        {
            // Ensure AudioListener exists in the scene (required for audio)
            if (FindObjectOfType<AudioListener>() == null)
            {
                Camera mainCam = Camera.main;
                if (mainCam != null)
                    mainCam.gameObject.AddComponent<AudioListener>();
                else
                    gameObject.AddComponent<AudioListener>();
            }

            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.loop = true;
            audioSource.volume = volume;
            audioSource.playOnAwake = false;

            AudioClip bgmClip = GenerateBGM();
            audioSource.clip = bgmClip;
            audioSource.Play();
        }

        private AudioClip GenerateBGM()
        {
            int totalNotes = melodyNotes.Length;
            float noteDuration = beatDuration * 0.5f; // 8th notes
            float totalDuration = totalNotes * noteDuration;
            int totalSamples = (int)(totalDuration * sampleRate);

            float[] samples = new float[totalSamples];

            for (int i = 0; i < totalSamples; i++)
            {
                float t = (float)i / sampleRate;
                int noteIndex = Mathf.Min((int)(t / noteDuration), totalNotes - 1);

                // Melody (square wave with slight pulse width for character)
                float melodyFreq = MidiToFreq(melodyNotes[noteIndex]);
                float melody = SquareWave(t, melodyFreq, 0.4f) * 0.3f;

                // Apply envelope to melody for cleaner sound
                float noteTime = t - (noteIndex * noteDuration);
                float envelope = MelodyEnvelope(noteTime, noteDuration);
                melody *= envelope;

                // Bass (triangle wave for warmth)
                int bassIndex = Mathf.Min(noteIndex / 1, bassNotes.Length - 1);
                float bassFreq = MidiToFreq(bassNotes[bassIndex]);
                float bass = TriangleWave(t, bassFreq) * 0.25f;

                // Simple drums (noise-based kick/hihat pattern)
                float drums = DrumPattern(t, noteDuration);

                // Mix
                samples[i] = Mathf.Clamp((melody + bass + drums) * 0.6f, -1f, 1f);
            }

            AudioClip clip = AudioClip.Create("ProceduralBGM", totalSamples, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        private float MidiToFreq(int midiNote)
        {
            return 440f * Mathf.Pow(2f, (midiNote - 69) / 12f);
        }

        private float SquareWave(float t, float freq, float pulseWidth)
        {
            float phase = (t * freq) % 1f;
            return phase < pulseWidth ? 1f : -1f;
        }

        private float TriangleWave(float t, float freq)
        {
            float phase = (t * freq) % 1f;
            return 4f * Mathf.Abs(phase - 0.5f) - 1f;
        }

        private float MelodyEnvelope(float noteTime, float noteDuration)
        {
            float attack = 0.01f;
            float release = noteDuration * 0.3f;

            if (noteTime < attack)
                return noteTime / attack;
            else if (noteTime > noteDuration - release)
                return (noteDuration - noteTime) / release;
            return 1f;
        }

        private float DrumPattern(float t, float noteDuration)
        {
            float beatPos = t % (noteDuration * 2f);
            float kick = 0f;
            float hihat = 0f;

            // Kick on beat 1 and 3
            if (beatPos < 0.05f)
            {
                float env = 1f - (beatPos / 0.05f);
                kick = Mathf.Sin(beatPos * 150f * Mathf.PI * 2f) * env * env * 0.4f;
            }

            // Hi-hat on every 8th note
            float hihatPos = t % noteDuration;
            if (hihatPos < 0.02f)
            {
                float env = 1f - (hihatPos / 0.02f);
                // Use pseudo-random noise
                hihat = (Mathf.PerlinNoise(t * 10000f, 0f) * 2f - 1f) * env * 0.15f;
            }

            return kick + hihat;
        }
    }
}
