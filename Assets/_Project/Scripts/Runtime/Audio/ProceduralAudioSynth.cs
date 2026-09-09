using System;
using UnityEngine;

namespace ReleaseTheArrow.Audio
{
    public enum WaveShape { Sine, Square, Triangle, Noise }

    /// Generates every game sound at runtime via simple waveform synthesis. This keeps the
    /// project's audio 100% original and license-free with zero external asset dependencies —
    /// appropriate for a casual puzzle game's short UI/feedback stingers. A future pass can swap
    /// these for professionally composed/licensed clips without touching any calling code, since
    /// everything is consumed through AudioManager by name.
    public static class ProceduralAudioSynth
    {
        private const int SampleRate = 44100;

        private struct Note
        {
            public float frequency;
            public float endFrequency;
            public float duration;
            public WaveShape shape;
            public float volume;
        }

        private static Note N(float freq, float duration, WaveShape shape = WaveShape.Sine, float volume = 1f, float? endFreq = null)
            => new Note { frequency = freq, endFrequency = endFreq ?? freq, duration = duration, shape = shape, volume = volume };

        public static AudioClip ButtonTap() => BuildSequence("sfx_tap", new[] { N(880f, 0.05f, WaveShape.Sine, 0.5f) }, 0f);

        public static AudioClip ArrowRelease() => BuildSequence("sfx_release",
            new[] { N(520f, 0.06f, WaveShape.Sine, 0.55f, 900f), N(900f, 0.08f, WaveShape.Sine, 0.4f, 1200f) }, 0.01f);

        public static AudioClip ArrowBlocked() => BuildSequence("sfx_blocked",
            new[] { N(180f, 0.14f, WaveShape.Square, 0.35f, 120f) }, 0f);

        public static AudioClip LifeLost() => BuildSequence("sfx_life_lost",
            new[] { N(500f, 0.09f, WaveShape.Triangle, 0.45f, 260f), N(260f, 0.12f, WaveShape.Triangle, 0.4f, 160f) }, 0.01f);

        public static AudioClip LevelComplete() => BuildSequence("sfx_level_complete",
            new[]
            {
                N(523.25f, 0.11f, WaveShape.Sine, 0.5f), N(659.25f, 0.11f, WaveShape.Sine, 0.5f),
                N(783.99f, 0.11f, WaveShape.Sine, 0.5f), N(1046.5f, 0.22f, WaveShape.Sine, 0.55f)
            }, 0.015f);

        public static AudioClip GameOver() => BuildSequence("sfx_game_over",
            new[] { N(392f, 0.16f, WaveShape.Triangle, 0.45f), N(329.63f, 0.16f, WaveShape.Triangle, 0.42f), N(261.63f, 0.28f, WaveShape.Triangle, 0.4f) }, 0.02f);

        public static AudioClip RewardedContinue() => BuildSequence("sfx_continue",
            new[] { N(659.25f, 0.09f, WaveShape.Sine, 0.5f), N(880f, 0.09f, WaveShape.Sine, 0.5f), N(1174.66f, 0.16f, WaveShape.Sine, 0.55f) }, 0.01f);

        /// Gentle 8-second looping arpeggio pad — deliberately understated so it can loop under
        /// gameplay without becoming fatiguing.
        public static AudioClip MusicLoop()
        {
            float[] scale = { 261.63f, 293.66f, 329.63f, 392f, 440f, 392f, 329.63f, 293.66f };
            var notes = new Note[scale.Length];
            for (int i = 0; i < scale.Length; i++) notes[i] = N(scale[i], 1f, WaveShape.Sine, 0.18f);
            return BuildSequence("music_loop", notes, 0f, loopFriendly: true);
        }

        private static AudioClip BuildSequence(string name, Note[] notes, float gapSeconds, bool loopFriendly = false)
        {
            int gapSamples = Mathf.RoundToInt(gapSeconds * SampleRate);
            int total = 0;
            foreach (var n in notes) total += Mathf.RoundToInt(n.duration * SampleRate) + gapSamples;

            var data = new float[Mathf.Max(1, total)];
            int cursor = 0;
            foreach (var n in notes)
            {
                int samples = Mathf.RoundToInt(n.duration * SampleRate);
                WriteNote(data, cursor, samples, n, loopFriendly);
                cursor += samples + gapSamples;
            }

            var clip = AudioClip.Create(name, data.Length, 1, SampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        private static void WriteNote(float[] buffer, int offset, int samples, Note note, bool loopFriendly)
        {
            float attack = Mathf.Min(0.01f, note.duration * 0.2f);
            float release = loopFriendly ? note.duration * 0.5f : Mathf.Min(0.05f, note.duration * 0.5f);
            var rng = new System.Random(unchecked(offset * 397 + samples));

            double phase = 0.0;
            for (int i = 0; i < samples; i++)
            {
                float t = i / (float)SampleRate;
                float freq = Mathf.Lerp(note.frequency, note.endFrequency, samples <= 1 ? 0f : i / (float)(samples - 1));
                phase += freq / SampleRate;

                float raw = note.shape switch
                {
                    WaveShape.Sine => Mathf.Sin((float)(2.0 * Math.PI * phase)),
                    WaveShape.Square => Mathf.Sign(Mathf.Sin((float)(2.0 * Math.PI * phase))),
                    WaveShape.Triangle => Mathf.PingPong((float)phase * 4f, 2f) - 1f,
                    WaveShape.Noise => (float)(rng.NextDouble() * 2.0 - 1.0),
                    _ => 0f
                };

                float env = Envelope(t, note.duration, attack, release);
                int index = offset + i;
                if (index >= 0 && index < buffer.Length) buffer[index] += raw * env * note.volume;
            }
        }

        private static float Envelope(float t, float duration, float attack, float release)
        {
            if (t < attack) return attack <= 0f ? 1f : t / attack;
            float timeLeft = duration - t;
            if (timeLeft < release) return release <= 0f ? 0f : Mathf.Clamp01(timeLeft / release);
            return 1f;
        }
    }
}
