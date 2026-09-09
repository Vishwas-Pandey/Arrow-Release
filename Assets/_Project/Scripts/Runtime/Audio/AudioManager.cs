using System.Collections.Generic;
using UnityEngine;

namespace ReleaseTheArrow.Audio
{
    public enum Sfx { ButtonTap, ArrowRelease, ArrowBlocked, LifeLost, LevelComplete, GameOver, RewardedContinue }

    /// Singleton audio hub. Generates each clip once (lazily, on first use) via
    /// ProceduralAudioSynth and caches it — synthesis only costs a few milliseconds and only
    /// ever happens once per sound for the life of the process.
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        private AudioSource _sfxSource;
        private AudioSource _musicSource;
        private readonly Dictionary<Sfx, AudioClip> _clipCache = new Dictionary<Sfx, AudioClip>();

        public bool SoundOn { get; set; } = true;
        public bool MusicOn { get; set; } = true;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            _sfxSource = gameObject.AddComponent<AudioSource>();
            _sfxSource.playOnAwake = false;

            _musicSource = gameObject.AddComponent<AudioSource>();
            _musicSource.playOnAwake = false;
            _musicSource.loop = true;
            _musicSource.volume = 0.5f;
        }

        private void Start()
        {
            if (MusicOn) PlayMusic();
        }

        public void ApplySettings(bool soundOn, bool musicOn)
        {
            SoundOn = soundOn;
            MusicOn = musicOn;
            if (MusicOn && !_musicSource.isPlaying) PlayMusic();
            if (!MusicOn && _musicSource.isPlaying) _musicSource.Stop();
        }

        public void Play(Sfx sfx)
        {
            if (!SoundOn) return;
            _sfxSource.PlayOneShot(GetOrCreate(sfx));
        }

        private void PlayMusic()
        {
            _musicSource.clip = ProceduralAudioSynth.MusicLoop();
            _musicSource.Play();
        }

        private AudioClip GetOrCreate(Sfx sfx)
        {
            if (_clipCache.TryGetValue(sfx, out var clip) && clip != null) return clip;

            clip = sfx switch
            {
                Sfx.ButtonTap => ProceduralAudioSynth.ButtonTap(),
                Sfx.ArrowRelease => ProceduralAudioSynth.ArrowRelease(),
                Sfx.ArrowBlocked => ProceduralAudioSynth.ArrowBlocked(),
                Sfx.LifeLost => ProceduralAudioSynth.LifeLost(),
                Sfx.LevelComplete => ProceduralAudioSynth.LevelComplete(),
                Sfx.GameOver => ProceduralAudioSynth.GameOver(),
                Sfx.RewardedContinue => ProceduralAudioSynth.RewardedContinue(),
                _ => null
            };
            _clipCache[sfx] = clip;
            return clip;
        }
    }
}
