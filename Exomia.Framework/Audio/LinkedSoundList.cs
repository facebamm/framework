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
using System.Collections;
using System.Collections.Generic;
using SharpDX.X3DAudio;
using SharpDX.XAudio2;

namespace Exomia.Framework.Audio
{
    sealed class LinkedSoundList : IEnumerable<LinkedSoundList.Sound>
    {
        private readonly int _capacity;

        private readonly object _thisLock;
        private Sound _head;
        private int _count;

        public int Capacity
        {
            // ReSharper disable once InconsistentlySynchronizedField
            get { return _capacity; }
        }

        public int Count
        {
            // ReSharper disable once InconsistentlySynchronizedField
            get { return _count; }
        }

        public LinkedSoundList(int capacity = int.MaxValue)
        {
            _capacity = capacity;
            _count    = 0;
            _thisLock = new object();
        }

        public void Add(Sound sound)
        {
            lock (_thisLock)
            {
                if (_count + 1 >= _capacity) { return; }

                if (_head == null)
                {
                    sound.Next     = sound;
                    sound.Previous = sound;
                    _head          = sound;
                }
                else
                {
                    sound.Next     = _head;
                    sound.Previous = _head.Previous;

                    _head.Previous.Next = sound;
                    _head.Previous      = sound;
                }

                _count++;
            }
        }

        public void Clear()
        {
            lock (_thisLock)
            {
                _head  = null;
                _count = 0;
            }
        }

        public void Remove(Sound sound)
        {
            lock (_thisLock)
            {
                if (sound == null) { throw new ArgumentNullException(nameof(sound)); }
                if (sound.Next == sound) { _head = null; }
                else
                {
                    sound.Previous.Next = sound.Next;
                    sound.Next.Previous = sound.Previous;
                    if (_head == sound) { _head = sound.Next; }
                }
                sound.Invalidate();
                _count--;
            }
        }

        public Enumerator GetEnumerator()
        {
            return new Enumerator(this);
        }

        IEnumerator<Sound> IEnumerable<Sound>.GetEnumerator()
        {
            return GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public struct Enumerator : IEnumerator<Sound>
        {
            private readonly LinkedSoundList _list;
            private Sound _current, _node;

            /// <inheritdoc />
            public Sound Current
            {
                get { return _current; }
            }

            /// <inheritdoc />
            object IEnumerator.Current
            {
                get { return Current; }
            }

            public Enumerator(LinkedSoundList list)
            {
                _list    = list;
                _node    = list._head;
                _current = null;
            }

            /// <inheritdoc />
            public bool MoveNext()
            {
                if (_node == null)
                {
                    return false;
                }
                _current = _node;
                _node    = _node.Next;
                if (_node == _list._head)
                {
                    _node = null;
                }
                return true;
            }

            /// <inheritdoc />
            public void Reset()
            {
                _node    = _list._head;
                _current = null;
            }

            /// <inheritdoc />
            public void Dispose() { }
        }

        internal sealed class Sound
        {
            internal readonly Emitter Emitter;
            internal readonly SourceVoice SourceVoice;
            internal Sound Next;
            internal Sound Previous;

            /// <summary>
            ///     Initializes a new instance of the <see cref="Sound" /> class.
            /// </summary>
            public Sound(Emitter emitter, SourceVoice sourceVoice)
            {
                Emitter     = emitter;
                SourceVoice = sourceVoice;
            }

            internal void Invalidate()
            {
                Next     = null;
                Previous = null;
            }
        }
    }
}