﻿#region MIT License

// Copyright (c) 2019 exomia - Daniel Bätz
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

#endregion

using System;
using System.Collections.Generic;
using System.IO;
using SharpDX;
using SharpDX.Multimedia;
using SharpDX.X3DAudio;
using SharpDX.XAudio2;

namespace Exomia.Framework.Audio
{
    /// <summary>
    ///     Manager for audio. This class cannot be inherited.
    /// </summary>
    public sealed class AudioManager : IAudioManager
    {
        /// <summary>
        ///     Number of input channels.
        /// </summary>
        private readonly int _inputChannelCount;

        /// <summary>
        ///     The listener.
        /// </summary>
        private readonly Listener _listener;

        /// <summary>
        ///     The bgm volume.
        /// </summary>
        private float _bgmVolume = 1.0f;

        /// <summary>
        ///     The current bgm.
        /// </summary>
        private SourceVoice _currentBgm;

        /// <summary>
        ///     List of environment linked sounds.
        /// </summary>
        private LinkedSoundList _envLinkedSoundList;

        /// <summary>
        ///     The environment submix voice.
        /// </summary>
        private SubmixVoice _envSubmixVoice;

        /// <summary>
        ///     Information describing the environment voice send.
        /// </summary>
        private VoiceSendDescriptor _envVoiceSendDescriptor;

        /// <summary>
        ///     The environment volume.
        /// </summary>
        private float _envVolume = 1.0f;

        /// <summary>
        ///     List of effects linked sounds.
        /// </summary>
        private LinkedSoundList _fxLinkedSoundList;

        /// <summary>
        ///     The effects submix voice.
        /// </summary>
        private SubmixVoice _fxSubmixVoice;

        /// <summary>
        ///     Information describing the effects voice send.
        /// </summary>
        private VoiceSendDescriptor _fxVoiceSendDescriptor;

        /// <summary>
        ///     The effects volume.
        /// </summary>
        private float _fxVolume = 1.0f;

        /// <summary>
        ///     The mastering voice.
        /// </summary>
        private MasteringVoice _masteringVoice;

        /// <summary>
        ///     The master volume.
        /// </summary>
        private float _masterVolume = 0.5f;

        /// <summary>
        ///     Buffer for sound data.
        /// </summary>
        private Dictionary<int, SoundBuffer> _soundBuffer;

        /// <summary>
        ///     Zero-based index of the sound buffer.
        /// </summary>
        private int _soundBufferIndex;

        /// <summary>
        ///     The 3 d audio.
        /// </summary>
        private X3DAudio _x3DAudio;

        /// <summary>
        ///     The second x coordinate audio.
        /// </summary>
        private XAudio2 _xAudio2;

        /// <inheritdoc />
        public float BgmVolume
        {
            get { return _bgmVolume; }
            set
            {
                _bgmVolume = MathUtil.Clamp(value, 0.0f, 1.0f);
                if (_currentBgm != null && _currentBgm.State.BuffersQueued > 0)
                {
                    _currentBgm.SetVolume(_bgmVolume);
                }
            }
        }

        /// <inheritdoc />
        public float EnvironmentVolume
        {
            get { return _envVolume; }
            set
            {
                _envVolume = MathUtil.Clamp(value, 0.0f, 1.0f);
                _envSubmixVoice?.SetVolume(_envVolume);
            }
        }

        /// <inheritdoc />
        public float FxVolume
        {
            get { return _fxVolume; }
            set
            {
                _fxVolume = MathUtil.Clamp(value, 0.0f, 1.0f);
                _fxSubmixVoice?.SetVolume(_fxVolume);
            }
        }

        /// <inheritdoc />
        public float MasterVolume
        {
            get { return _masterVolume; }
            set
            {
                _masterVolume = MathUtil.Clamp(value, 0.0f, 1.0f);
                _masteringVoice?.SetVolume(_masterVolume);
            }
        }

        /// <inheritdoc />
        public Vector3 ListenerPosition
        {
            get { return _listener.Position; }
            set
            {
                _listener.Position = value;
                RecalculateFxSounds();
                RecalculateEnvSounds();
            }
        }

        /// <inheritdoc />
        public Vector3 ListenerVelocity
        {
            get { return _listener.Velocity; }
            set
            {
                _listener.Velocity = value;
                RecalculateFxSounds();
                RecalculateEnvSounds();
            }
        }

        /// <inheritdoc />
        public AudioManager(Listener listener, int fxSoundPoolLimit, Speakers speakers, string deviceID = null)
        {
            _listener = listener;
            if (fxSoundPoolLimit <= 0)
            {
                throw new ArgumentException("fxSoundPoolLimit must be bigger than 0");
            }
            _soundBuffer        = new Dictionary<int, SoundBuffer>(128);
            _fxLinkedSoundList  = new LinkedSoundList(fxSoundPoolLimit);
            _envLinkedSoundList = new LinkedSoundList();

#if DEBUG
            _xAudio2 = new XAudio2(XAudio2Flags.DebugEngine, ProcessorSpecifier.AnyProcessor);
#else
            _xAudio2 = new XAudio2(XAudio2Flags.None, ProcessorSpecifier.AnyProcessor, XAudio2Version.Default);
#endif
            if (_xAudio2.Version == XAudio2Version.Version27)
            {
                if (int.TryParse(deviceID, out int deviceID27))
                {
                    _masteringVoice = new MasteringVoice(
                        _xAudio2, XAudio2.DefaultChannels, XAudio2.DefaultSampleRate, deviceID27);
                }
            }
            else
            {
                _masteringVoice = new MasteringVoice(
                    _xAudio2, XAudio2.DefaultChannels, XAudio2.DefaultSampleRate, deviceID);
            }
            if (_masteringVoice == null)
            {
                throw new Exception("can't create MasteringVoice");
            }
            _masteringVoice.SetVolume(_masterVolume);
            _x3DAudio = new X3DAudio(speakers, X3DAudio.SpeedOfSound / 1000f);

            _masteringVoice.GetVoiceDetails(out VoiceDetails details);
            _inputChannelCount = details.InputChannelCount;

            _fxSubmixVoice = new SubmixVoice(_xAudio2, _inputChannelCount, details.InputSampleRate);
            _fxSubmixVoice.SetVolume(_fxVolume);

            _envSubmixVoice = new SubmixVoice(_xAudio2, _inputChannelCount, details.InputSampleRate);
            _envSubmixVoice.SetVolume(_envVolume);

            _fxVoiceSendDescriptor  = new VoiceSendDescriptor(VoiceSendFlags.None, _fxSubmixVoice);
            _envVoiceSendDescriptor = new VoiceSendDescriptor(VoiceSendFlags.None, _envSubmixVoice);
        }

        /// <inheritdoc />
        public int LoadSound(Stream stream)
        {
            using (SoundStream soundStream = new SoundStream(stream))
            {
                AudioBuffer audioBuffer = new AudioBuffer
                {
                    Stream     = soundStream.ToDataStream(),
                    AudioBytes = (int)soundStream.Length,
                    Flags      = BufferFlags.EndOfStream
                };
                soundStream.Close();
                _soundBuffer.Add(
                    _soundBufferIndex,
                    new SoundBuffer
                    {
                        AudioBuffer        = audioBuffer,
                        Format             = soundStream.Format,
                        DecodedPacketsInfo = soundStream.DecodedPacketsInfo
                    });
            }
            return _soundBufferIndex++;
        }

        /// <inheritdoc />
        public void PauseBgm()
        {
            if (_currentBgm?.State.BuffersQueued > 0)
            {
                _currentBgm.Stop();
            }
        }

        /// <inheritdoc />
        public void PauseEnvSounds()
        {
            foreach (LinkedSoundList.Sound sound in _envLinkedSoundList)
            {
                sound.SourceVoice.Stop();
            }
        }

        /// <inheritdoc />
        public void PauseFxSounds()
        {
            foreach (LinkedSoundList.Sound sound in _fxLinkedSoundList)
            {
                sound.SourceVoice.Stop();
            }
        }

        /// <inheritdoc />
        public void PlayEnvSound(int            soundID, Vector3 emitterPos, float volume, float maxDistance,
                                 Action<IntPtr> onFxEnd = null)
        {
            PlaySound(
                soundID, emitterPos, volume, maxDistance, _envLinkedSoundList, ref _envVoiceSendDescriptor, onFxEnd);
        }

        /// <inheritdoc />
        public void PlayFxSound(int            soundID, Vector3 emitterPos, float volume, float maxDistance,
                                Action<IntPtr> onFxEnd = null)
        {
            if (_fxLinkedSoundList.Count >= _fxLinkedSoundList.Capacity)
            {
                return;
            }
            PlaySound(
                soundID, emitterPos, volume, maxDistance, _fxLinkedSoundList, ref _fxVoiceSendDescriptor, onFxEnd);
        }

        /// <inheritdoc />
        public void PlayEnvSound(int soundID, Emitter emitter, float volume, Action<IntPtr> onFxEnd = null)
        {
            PlaySound(soundID, emitter, volume, _envLinkedSoundList, ref _envVoiceSendDescriptor, onFxEnd);
        }

        /// <inheritdoc />
        public void PlayFxSound(int soundID, Emitter emitter, float volume, Action<IntPtr> onFxEnd = null)
        {
            if (_fxLinkedSoundList.Count >= _fxLinkedSoundList.Capacity)
            {
                return;
            }
            PlaySound(soundID, emitter, volume, _fxLinkedSoundList, ref _fxVoiceSendDescriptor, onFxEnd);
        }

        /// <inheritdoc />
        public void ResumeBgm()
        {
            if (_currentBgm?.State.BuffersQueued > 0)
            {
                _currentBgm.Start();
            }
        }

        /// <inheritdoc />
        public void ResumeEnvSounds()
        {
            foreach (LinkedSoundList.Sound sound in _envLinkedSoundList)
            {
                sound.SourceVoice.Start();
            }
        }

        /// <inheritdoc />
        public void ResumeFxSounds()
        {
            foreach (LinkedSoundList.Sound sound in _fxLinkedSoundList)
            {
                sound.SourceVoice.Start();
            }
        }

        /// <inheritdoc />
        public void RunBgm(int songID, Action<IntPtr> onBgmEnd = null)
        {
            if (!_soundBuffer.TryGetValue(songID, out SoundBuffer buffer))
            {
                return;
            }
            if (_currentBgm?.State.BuffersQueued > 0)
            {
                _currentBgm.Stop();
            }

            _currentBgm = new SourceVoice(_xAudio2, buffer.Format, VoiceFlags.None, true);
            _currentBgm.SetVolume(_bgmVolume);
            _currentBgm.SubmitSourceBuffer(buffer.AudioBuffer, buffer.DecodedPacketsInfo);

            _currentBgm.BufferEnd += ptr =>
            {
                _currentBgm.DestroyVoice();
            };
            if (onBgmEnd != null)
            {
                _currentBgm.BufferEnd += onBgmEnd;
            }

            _currentBgm.SubmitSourceBuffer(buffer.AudioBuffer, buffer.DecodedPacketsInfo);
            _currentBgm.Start();
        }

        /// <inheritdoc />
        public void StopBgm()
        {
            if (_currentBgm?.State.BuffersQueued > 0)
            {
                _currentBgm.Stop();
                _currentBgm.DestroyVoice();
                _currentBgm.Dispose();
            }
        }

        /// <inheritdoc />
        public void StopEnvSounds()
        {
            foreach (LinkedSoundList.Sound sound in _envLinkedSoundList)
            {
                sound.SourceVoice.Stop();
                sound.SourceVoice.DestroyVoice();
                sound.SourceVoice.Dispose();
            }
        }

        /// <inheritdoc />
        public void StopFxSounds()
        {
            foreach (LinkedSoundList.Sound sound in _fxLinkedSoundList)
            {
                sound.SourceVoice.Stop();
                sound.SourceVoice.DestroyVoice();
                sound.SourceVoice.Dispose();
            }
        }

        /// <inheritdoc />
        public void UnloadAll()
        {
            StopBgm();
            _soundBuffer.Clear();
            _soundBufferIndex = 0;
        }

        /// <inheritdoc />
        public bool UnloadSound(int soundID)
        {
            return _soundBuffer.Remove(soundID);
        }

        /// <summary>
        ///     Play sound.
        /// </summary>
        /// <param name="soundID">             Identifier for the sound. </param>
        /// <param name="emitterPos">          The emitter position. </param>
        /// <param name="volume">              The volume. </param>
        /// <param name="maxDistance">         The maximum distance. </param>
        /// <param name="list">                The list. </param>
        /// <param name="voiceSendDescriptor"> [in,out] Information describing the voice send. </param>
        /// <param name="onFxEnd">             (Optional) The on effects end. </param>
        private void PlaySound(int                     soundID,             Vector3         emitterPos, float volume,
                               float                   maxDistance,         LinkedSoundList list,
                               ref VoiceSendDescriptor voiceSendDescriptor, Action<IntPtr>  onFxEnd = null)
        {
            PlaySound(
                soundID,
                new Emitter
                {
                    ChannelCount = 1,
                    VolumeCurve =
                        new[]
                        {
                            new CurvePoint { Distance = 0.0f, DspSetting = 1.0f },
                            new CurvePoint { Distance = 1.0f, DspSetting = 0.0f }
                        },
                    CurveDistanceScaler = maxDistance,
                    OrientFront         = Vector3.UnitZ,
                    OrientTop           = Vector3.UnitY,
                    Position            = emitterPos,
                    Velocity            = new Vector3(0, 0, 0)
                }, volume, list, ref voiceSendDescriptor, onFxEnd);
        }

        /// <summary>
        ///     Play sound.
        /// </summary>
        /// <param name="soundID">             Identifier for the sound. </param>
        /// <param name="emitter">             The emitter. </param>
        /// <param name="volume">              The volume. </param>
        /// <param name="list">                The list. </param>
        /// <param name="voiceSendDescriptor"> [in,out] Information describing the voice send. </param>
        /// <param name="onFxEnd">             (Optional) The on effects end. </param>
        private void PlaySound(int                     soundID, Emitter emitter, float volume,
                               LinkedSoundList         list,
                               ref VoiceSendDescriptor voiceSendDescriptor, Action<IntPtr> onFxEnd = null)
        {
            if (!_soundBuffer.TryGetValue(soundID, out SoundBuffer buffer))
            {
                return;
            }

            SourceVoice sourceVoice = new SourceVoice(_xAudio2, buffer.Format, VoiceFlags.None, true);
            sourceVoice.SetVolume(volume);
            sourceVoice.SubmitSourceBuffer(buffer.AudioBuffer, buffer.DecodedPacketsInfo);
            sourceVoice.SetOutputVoices(voiceSendDescriptor);

            LinkedSoundList.Sound sound = new LinkedSoundList.Sound(emitter, sourceVoice);
            list.Add(sound);

            sourceVoice.BufferEnd += _ =>
            {
                list.Remove(sound);
                sourceVoice.DestroyVoice();
            };

            if (onFxEnd != null)
            {
                sourceVoice.BufferEnd += onFxEnd;
            }
            sourceVoice.Start();

            DspSettings settings = _x3DAudio.Calculate(
                _listener,
                sound.Emitter,
                CalculateFlags.Matrix | CalculateFlags.Doppler,
                buffer.Format.Channels,
                _inputChannelCount);
            sound.SourceVoice.SetOutputMatrix(buffer.Format.Channels, _inputChannelCount, settings.MatrixCoefficients);
            sound.SourceVoice.SetFrequencyRatio(settings.DopplerFactor);
        }

        /// <summary>
        ///     Calculates the environment sounds.
        /// </summary>
        private void RecalculateEnvSounds()
        {
            foreach (LinkedSoundList.Sound sound in _envLinkedSoundList)
            {
                DspSettings settings = _x3DAudio.Calculate(
                    _listener,
                    sound.Emitter,
                    CalculateFlags.Matrix | CalculateFlags.Doppler,
                    sound.SourceVoice.VoiceDetails.InputChannelCount,
                    _inputChannelCount);
                sound.SourceVoice.SetOutputMatrix(
                    sound.SourceVoice.VoiceDetails.InputChannelCount, _inputChannelCount,
                    settings.MatrixCoefficients);
                sound.SourceVoice.SetFrequencyRatio(settings.DopplerFactor);
            }
        }

        /// <summary>
        ///     Calculates the effects sounds.
        /// </summary>
        private void RecalculateFxSounds()
        {
            foreach (LinkedSoundList.Sound sound in _fxLinkedSoundList)
            {
                DspSettings settings = _x3DAudio.Calculate(
                    _listener,
                    sound.Emitter,
                    CalculateFlags.Matrix | CalculateFlags.Doppler,
                    sound.SourceVoice.VoiceDetails.InputChannelCount,
                    _inputChannelCount);
                sound.SourceVoice.SetOutputMatrix(
                    sound.SourceVoice.VoiceDetails.InputChannelCount, _inputChannelCount,
                    settings.MatrixCoefficients);
                sound.SourceVoice.SetFrequencyRatio(settings.DopplerFactor);
            }
        }

        /// <summary>
        ///     Buffer for sound.
        /// </summary>
        private struct SoundBuffer
        {
            /// <summary>
            ///     Buffer for audio data.
            /// </summary>
            public AudioBuffer AudioBuffer;

            /// <summary>
            ///     Describes the format to use.
            /// </summary>
            public WaveFormat Format;

            /// <summary>
            ///     Information describing the decoded packets.
            /// </summary>
            public uint[] DecodedPacketsInfo;
        }

        #region IDisposable Support

        /// <summary>
        ///     True if disposed.
        /// </summary>
        private bool _disposed;

        /// <summary>
        ///     Releases the unmanaged resources used by the Exomia.Framework.Audio.AudioManager and
        ///     optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing">
        ///     True to release both managed and unmanaged resources; false to
        ///     release only unmanaged resources.
        /// </param>
        private void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    /* USER CODE */
                    StopFxSounds();
                    StopEnvSounds();

                    _currentBgm?.DestroyVoice();
                    _currentBgm?.Dispose();
                    _currentBgm = null;

                    _fxLinkedSoundList?.Clear();
                    _fxLinkedSoundList = null;

                    _envLinkedSoundList?.Clear();
                    _envLinkedSoundList = null;

                    UnloadAll();
                    _soundBuffer = null;

                    _fxSubmixVoice?.DestroyVoice();
                    _fxSubmixVoice?.Dispose();
                    _fxSubmixVoice = null;

                    _envSubmixVoice?.DestroyVoice();
                    _envSubmixVoice?.Dispose();
                    _envSubmixVoice = null;

                    _masteringVoice?.DestroyVoice();
                    _masteringVoice?.Dispose();
                    _masteringVoice = null;

                    _xAudio2?.Dispose();
                    _xAudio2  = null;
                    _x3DAudio = null;
                }

                _disposed = true;
            }
        }

        /// <inheritdoc />
        ~AudioManager()
        {
            Dispose(false);
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}