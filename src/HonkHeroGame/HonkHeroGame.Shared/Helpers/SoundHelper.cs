using System;
using System.Collections.Generic;
using System.Linq;

namespace HonkHeroGame
{
    public static class SoundHelper
    {
        #region Fields

        private static readonly Random _random = new();

        private static Sound[] _sounds;
        private static List<Sound> _playingSounds;

        #endregion

        #region Methods

        public static void LoadGameSounds(Action completed = null)
        {
            if (_sounds is null)
            {
                _sounds = Constants.SOUND_TEMPLATES.Select(x =>
                {
                    Sound sound = null;

                    switch (x.Key)
                    {
                        case SoundType.AMBIENCE:
                            {
                                sound = new Sound(soundType: x.Key, soundSource: x.Value, volume: 1.0, playback: () =>
                                {
                                    RandomizeSound(SoundType.AMBIENCE);
                                    PlaySound(SoundType.AMBIENCE);

#if DEBUG
                                    Console.WriteLine("AUDIO PLAYBACK: " + SoundType.AMBIENCE);
#endif
                                });
                            }
                            break;
                        case SoundType.SONG:
                            {
                                sound = new Sound(soundType: x.Key, soundSource: x.Value, volume: 1.0, playback: () =>
                                {
                                    RandomizeSound(SoundType.SONG);
                                    PlaySound(SoundType.SONG);

#if DEBUG
                                    Console.WriteLine("AUDIO PLAYBACK: " + SoundType.SONG);
#endif
                                });
                            }
                            break;
                        case SoundType.INTRO:
                            {
                                sound = new Sound(soundType: x.Key, soundSource: x.Value, volume: 1.0, loop: true);
                            }
                            break;                       
                        default:
                            {
                                sound = new Sound(soundType: x.Key, soundSource: x.Value);
                            }
                            break;
                    }

                    return sound;

                }).ToArray();

                _playingSounds = new List<Sound>();

                // add all sounds except randomizable sounds as these will be randomized before playing
                _playingSounds.AddRange(_sounds.Where(x => x.SoundType is not SoundType.AMBIENCE and not SoundType.INTRO and not SoundType.SONG));

                completed?.Invoke();
            }
            else
            {
                completed?.Invoke();
            }

#if DEBUG
            Console.WriteLine("LOADED SOUNDS: " + _playingSounds.Count);
#endif
        }

        public static void RandomizeSound(SoundType soundType)
        {
            foreach (var sound in _playingSounds.Where(x => x.SoundType == soundType))
                sound.Stop();

            var sounds = _sounds.Where(x => x.SoundType == soundType).ToArray();
            var soundIndex = _random.Next(0, sounds.Length);
            var soundTaken = sounds[soundIndex];

            _playingSounds.RemoveAll(x => x.SoundType == soundType);
            _playingSounds.Add(soundTaken);

            soundTaken.SetVolume(1.0);

#if DEBUG
            Console.WriteLine("RANDOMIZE SOUND -> SOUND INDEX: " + soundIndex + " SOUND URI: " + soundTaken.SoundSource);
#endif
        }

        public static bool IsSoundPlaying(SoundType soundType)
        {
            return _playingSounds.Any(x => x.SoundType == soundType && x.IsPlaying);
        }

        public static void PlaySound(SoundType soundType)
        {
            if (_playingSounds.FirstOrDefault(x => x.SoundType == soundType) is Sound playingSound)
                playingSound.Play();
        }

        public static void PlaySound(SoundType soundType, int index)
        {
            if (_playingSounds.Where(x => x.SoundType == soundType).ElementAt(index) is Sound playingSound)
                playingSound.Play();
        }

        public static void PlayRandomSound(SoundType soundType)
        {
            var sounds = _playingSounds.Where(x => x.SoundType == soundType).ToArray();

            if (sounds.Length > 1)
            {
                var sound = sounds[_random.Next(0, sounds.Length)];
                sound.Play();
            }
            else
            {
                if (sounds.FirstOrDefault(x => x.SoundType == soundType) is Sound playingSound)
                    playingSound.Play();
            }
        }

        public static void StopSound(SoundType soundType)
        {
            if (_playingSounds.FirstOrDefault(x => x.SoundType == soundType) is Sound playingSound)
                playingSound.Stop();
        }

        public static void PauseSound(SoundType soundType)
        {
            if (_playingSounds.FirstOrDefault(x => x.SoundType == soundType && x.IsPlaying) is Sound playingSound)
                playingSound.Pause();
        }

        public static void ResumeSound(SoundType soundType)
        {
            if (_playingSounds.FirstOrDefault(x => x.SoundType == soundType && x.IsPaused) is Sound playingSound)
                playingSound.Resume();
        }

        public static void SetVolume(SoundType soundType, double level)
        {
            if (_playingSounds.FirstOrDefault(x => x.SoundType == soundType) is Sound playingSound)
                playingSound.SetVolume(level);
        }

        public static void VolumeUp(SoundType soundType)
        {
            if (_playingSounds.FirstOrDefault(x => x.SoundType == soundType) is Sound playingSound)
                playingSound.VolumeUp();
        }

        public static void VolumeUp(SoundType soundType, double level)
        {
            if (_playingSounds.FirstOrDefault(x => x.SoundType == soundType) is Sound playingSound)
                playingSound.VolumeUp(level);
        }

        public static void VolumeDown(SoundType soundType)
        {
            if (_playingSounds.FirstOrDefault(x => x.SoundType == soundType) is Sound playingSound)
                playingSound.VolumeDown();
        }

        #endregion
    }
}
