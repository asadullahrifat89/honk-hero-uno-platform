namespace HonkHeroGame
{
    public class Sound
    {
        #region Fields

        private readonly AudioPlayer _audioPlayer;

        #endregion

        public Sound(SoundType soundType, string soundSource, double volume = 1.0, bool loop = false)
        {
            SoundType = soundType;
            SoundSource = soundSource;
            Volume = volume;

            var baseUrl = AssetHelper.GetBaseUrl();
            var source = $"{baseUrl}/{SoundSource}";
            _audioPlayer = new AudioPlayer(source: source, volume: volume, loop: loop);
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

        public void VolumeUp()
        {
            if (Volume < 0.9)
            {
                Volume += 0.1;
                _audioPlayer.SetVolume(Volume);
            }
        }

        public void VolumeDown()
        {
            if (Volume > 0.1)
            {
                Volume -= 0.1;
                _audioPlayer.SetVolume(Volume);
            }
        }

        #endregion
    }

    public enum SoundType
    {
        MENU_SELECT,
        INTRO,
        AMBIENCE,
        SONG,
        HONK,
        HONK_BUST,
        GAME_OVER,
        POWER_UP,
        POWER_DOWN,
        HEALTH_GAIN,
        HEALTH_LOSS,
        COLLECTIBLE,
    }
}
