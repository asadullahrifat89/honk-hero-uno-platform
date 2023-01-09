using System;

namespace HonkHeroGame
{
    public class Sound
    {
        #region Fields

        private readonly AudioElement _audioPlayer;

        #endregion

        public Sound(SoundType soundType, string soundSource, double volume = 1.0, bool loop = false, Action playback = null)
        {
            SoundType = soundType;
            SoundSource = soundSource;
            Volume = volume;

            var baseUrl = AssetHelper.GetBaseUrl();
            var source = $"{baseUrl}/{SoundSource}";

            _audioPlayer = new AudioElement(
                source: source,
                volume: volume,
                loop: loop,
                playback: playback);
        }

        #region Properties

        public SoundType SoundType { get; set; }

        public string SoundSource { get; set; }

        public bool IsPlaying { get; set; }

        public bool IsPaused { get; set; }

        public double Volume { get; set; } = 1.0;

        #endregion

        #region Methods

        public void Play()
        {
            _audioPlayer.Play();
            IsPlaying = true;
        }

        public void Stop()
        {
            _audioPlayer.Stop();
            IsPlaying = false;
        }

        public void Pause()
        {
            _audioPlayer.Pause();
            IsPaused = true;
        }

        public void Resume()
        {
            _audioPlayer.Resume();
        }

        public void SetVolume(double volume = 1.0)
        {
            if (volume < 0)
                volume = 0;

            if (volume > 1.0)
                volume = 1.0;

            Volume = volume;
            _audioPlayer.SetVolume(Volume);
        }

        public void VolumeUp(double level)
        {
            if (Volume < 0.9)
            {
                var levelTarget = level;

                if (Volume + level > 1.0)
                    levelTarget = Volume + level - 1;

                Volume += levelTarget;
                _audioPlayer.SetVolume(Volume);

#if DEBUG
                Console.WriteLine("VOLUME UP: " + Volume + " -> VOLUME LEVEL: " + levelTarget);
#endif
            }
        }

        public void VolumeUp()
        {
            if (Volume < 0.9)
            {
                Volume += 0.02;
                _audioPlayer.SetVolume(Volume);

#if DEBUG
                Console.WriteLine("VOLUME UP: " + Volume);
#endif
            }
        }

        public void VolumeDown()
        {
            if (Volume > 0.1)
            {
                Volume -= 0.02;
                _audioPlayer.SetVolume(Volume);

#if DEBUG
                Console.WriteLine("VOLUME DOWN: " + Volume);
#endif
            }
        }

        #endregion
    }

    public enum SoundType
    {
        MENU_SELECT,        
        AMBIENCE,
        SONG,
        HONK,
        HONK_BUST,
        GAME_OVER,
        POWER_UP,
        POWER_DOWN,
        COLLECTIBLE,
        LEVEL_UP,
        BOSS_ENTRY,
        BOSS_IDLING,        
        BOSS_HONK,
        BOSS_CLEAR,
    }
}
