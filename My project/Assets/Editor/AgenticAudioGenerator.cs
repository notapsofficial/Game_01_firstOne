using UnityEngine;
using UnityEditor;
using System.IO;
using System.Text;

public class AgenticAudioGenerator
{
    [MenuItem("Agentic/Generate Audio")]
    public static void GenerateExplosionSound()
    {
        string path = "Assets/Resources";
        if (!Directory.Exists(path)) Directory.CreateDirectory(path);

        string filePath = Path.Combine(path, "Explosion.wav");
        
        // Audio Params
        int sampleRate = 44100;
        float duration = 0.5f;
        int sampleCount = (int)(sampleRate * duration);
        float[] samples = new float[sampleCount];

        // Generate Noise with Decay
        System.Random rng = new System.Random();
        for (int i = 0; i < sampleCount; i++)
        {
            float t = (float)i / sampleCount; // 0 to 1
            float noise = (float)(rng.NextDouble() * 2.0 - 1.0);
            
            // Exponential decay for "Boom"
            float envelope = Mathf.Exp(-5.0f * t);
            
            samples[i] = noise * envelope;
        }

        SaveWav(filePath, samples, sampleRate);
        AssetDatabase.Refresh();
        Debug.Log($"Generated Explosion Sound at: {filePath}");
    }

    static void SaveWav(string filepath, float[] samples, int sampleRate)
    {
        using (FileStream fs = new FileStream(filepath, FileMode.Create))
        {
            using (BinaryWriter bw = new BinaryWriter(fs))
            {
                int channelCount = 1;
                int byteRate = sampleRate * channelCount * 2; // 16 bit
                int blockAlign = channelCount * 2;

                // RIFF Header
                bw.Write(Encoding.ASCII.GetBytes("RIFF"));
                bw.Write(36 + samples.Length * 2);
                bw.Write(Encoding.ASCII.GetBytes("WAVE"));

                // fmt Chunk
                bw.Write(Encoding.ASCII.GetBytes("fmt "));
                bw.Write(16); // Subchunk1Size
                bw.Write((short)1); // AudioFormat (PCM)
                bw.Write((short)channelCount);
                bw.Write(sampleRate);
                bw.Write(byteRate);
                bw.Write((short)blockAlign);
                bw.Write((short)16); // BitsPerSample

                // data Chunk
                bw.Write(Encoding.ASCII.GetBytes("data"));
                bw.Write(samples.Length * 2);

                // Data
                foreach (float sample in samples)
                {
                    short shortSample = (short)(Mathf.Clamp(sample, -1f, 1f) * 32767);
                    bw.Write(shortSample);
                }
            }
        }
    }
}
