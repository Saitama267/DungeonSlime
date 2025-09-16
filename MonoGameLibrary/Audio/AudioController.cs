using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Media;
using System;
using System.Collections.Generic;

namespace MonoGameLibrary.Audio
{
    public class AudioController :  IDisposable
    {
        /// <summary>
        /// Tracks sound effect instances created so they can be paused, unpaused, and/or disposed.
        /// </summary>
        private readonly List<SoundEffectInstance> _activeSoundEffectInstance;

        /// <summary>
        /// Tracks the volume of the song playback when muting and unmuting.
        /// </summary>
        private float _previousSongVolume;

        /// <summary>
        /// Tracks the volume of the sound effect playback when muting and unmuting.
        /// </summary>
        public float _previousSoundEffectVolume;

        /// <summary>
        /// Gets a value that indicates if audio is muted.
        /// </summary>
        public bool IsMuted { get; private set; }

        /// <summary>
        /// Gets or Sets the global volume of songs.
        /// </summary>
        /// <remarks>
        /// If IsMuted is true, the getter will always return back 0.0f and the 
        /// setter will ignore setting the volume.
        /// </remarks>
        public float SongVolume
        {
            get 
            {
                if (IsMuted)
                {
                    return 0.0f;
                }

                return MediaPlayer.Volume;
            }
            set 
            {
                if (IsMuted)
                {
                    return;
                }

                MediaPlayer.Volume = Math.Clamp(value, 0.0f, 1.0f);
            }
        }

        /// <summary>
        /// Gets or Sets the global volume of sound effects.
        /// </summary>
        /// <remarks>
        /// If IsMuted is true, the getter will always return back 0.0f and the 
        /// setter will ignore setting the volume.
        /// </remarks>
        public float SoundEffectVolume
        {
            get
            {
                if (IsMuted)
                {
                    return 0.0f;
                }

                return SoundEffect.MasterVolume;
            }
            set
            {
                if (IsMuted)
                {
                    return;
                }

                SoundEffect.MasterVolume = Math.Clamp(value, 0.0f, 1.0f);
            }
        }

        /// <summary>
        /// Gets a value that indicates if this audio controller has been disposed.
        /// </summary>
        public bool IsDisposed { get; private set; }

        /// <summary>
        /// Creates a new audio controller instance.
        /// </summary>
        public AudioController()
        {
            _activeSoundEffectInstance = new List<SoundEffectInstance>();
        }

        ~AudioController() => Dispose(false);

        /// <summary>
        /// Updates this audio controller.
        /// </summary>
        public void Update()
        {
            for (int i = _activeSoundEffectInstance.Count - 1; i >= 0; i--)
            {
                SoundEffectInstance instance = _activeSoundEffectInstance[i];
                
                if (instance.State == SoundState.Stopped)
                {
                    if (!instance.IsDisposed)
                    {
                        instance.Dispose();
                    }

                    _activeSoundEffectInstance.RemoveAt(i);
                }
            }
        }

        /// <summary>
        /// Playes the given sound effect.
        /// </summary>
        /// <param name="soundEffect">The sound effect to play.</param>
        /// <returns>The sound effect instance created by this method.</returns>
        public SoundEffectInstance PlaySoundEffect(SoundEffect soundEffect)
        {
            return PlaySoundEffect(soundEffect, 1.0f, 0.0f, 0.0f, false);
        }

        /// <summary>
        /// Plays the given sound effect with the specified properties.
        /// </summary>
        /// <param name="soundEffect">The sound effect to play.</param>
        /// <param name="volume">The volume, ranging from 0.0 (silence) to 1.0 (full volume).</param>
        /// <param name="pitch">The pitch adjustment, ranging from -1.0 (down an octave) to 0.0 (no change) to 1.0 (up an octave)</param>
        /// <param name="pan">The pan,ranging from -1.0 (left speaker) to 0.0 (centered), 1.0f (right speaker).</param>
        /// <param name="isLooped">Wheter the sound effect should loop after playback.</param>
        /// <returns>The sound effect instance created by playing the sound effect.</returns>
        public SoundEffectInstance PlaySoundEffect(SoundEffect soundEffect, float volume, float pitch, float pan, bool isLooped)
        {
            SoundEffectInstance soundEffectInstance = soundEffect.CreateInstance();

            soundEffectInstance.Volume = volume;
            soundEffectInstance.Pitch = pitch;
            soundEffectInstance.Pan = pan;
            soundEffectInstance.IsLooped = isLooped;

            soundEffectInstance.Play();

            _activeSoundEffectInstance.Add(soundEffectInstance);

            return soundEffectInstance;
        }

        /// <summary>
        /// Plays the given song.
        /// </summary>
        /// <param name="song">The song to play.</param>
        /// <param name="isRepeating">Optionally specify if the song should repeat. Default is true.</param>
        public void PlaySong(Song song, bool isRepeating = true)
        {
            if (MediaPlayer.State == MediaState.Playing)
            {
                MediaPlayer.Stop();
            }

            MediaPlayer.Play(song);
            MediaPlayer.IsRepeating = isRepeating;
        }

        /// <summary>
        /// Pauses all audio.
        /// </summary>
        public void PauseAudio()
        {
            MediaPlayer.Pause();

            foreach (SoundEffectInstance soundEffectInstance in _activeSoundEffectInstance)
            {
                soundEffectInstance.Pause();
            }
        }

        /// <summary>
        /// Resumes play of all previous paused audio.
        /// </summary>
        public void ResumeAudio()
        {
            MediaPlayer.Resume();

            foreach (SoundEffectInstance soundEffectInstance in _activeSoundEffectInstance)
            {
                soundEffectInstance.Resume();
            }
        }

        /// <summary>
        /// Mutes all audio.
        /// </summary>
        public void MuteAudio()
        {
            _previousSongVolume = MediaPlayer.Volume;
            _previousSoundEffectVolume = SoundEffect.MasterVolume;

            MediaPlayer.Volume = 0.0f;
            SoundEffect.MasterVolume = 0.0f;

            IsMuted = true;
        }

        /// <summary>
        /// Unmutes all audio to the volume level prior to muting.
        /// </summary>
        public void UnmuteAudio()
        {
            MediaPlayer.Volume = _previousSongVolume;
            SoundEffect.MasterVolume = _previousSoundEffectVolume;

            IsMuted = false;
        }

        /// <summary>
        /// Toggles the current audio mute state.
        /// </summary>
        public void ToggleMute()
        {
            if (IsMuted)
            {
                UnmuteAudio();
            }
            else
            {
                MuteAudio();
            }
        }

        /// <summary>
        /// Disposes of this audio controller and clean up resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Disposes this audio controller and clean up resources.
        /// </summary>
        /// <param name="disposing">Indicates whether managed resources should be disposed.</param>
        protected void Dispose(bool disposing)
        {
            if (IsDisposed)
            {
                return;
            }

            if (disposing)
            {
                foreach (SoundEffectInstance soundEffectInstance in _activeSoundEffectInstance)
                {
                    soundEffectInstance.Dispose();
                }

                _activeSoundEffectInstance.Clear();
            }

            IsDisposed = true;
        }
    }
}
