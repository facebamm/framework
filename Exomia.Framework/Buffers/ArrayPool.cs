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
using System.Diagnostics;
using System.Threading;

#pragma warning disable CS1591

namespace Exomia.Framework.Buffers
{
    /// <summary>
    ///     ArrayPool class.
    /// </summary>
    /// <typeparam name="T"> any. </typeparam>
    public sealed class ArrayPool<T>
    {
        /// <summary>
        ///     Length of the buffer.
        /// </summary>
        private readonly int _bufferLength;

        /// <summary>
        ///     The buffers.
        /// </summary>
        private readonly T[][] _buffers;

        /// <summary>
        ///     The lock.
        /// </summary>
        private readonly SpinLock _lock;

        /// <summary>
        ///     The index.
        /// </summary>
        private int _index;

        /// <summary>
        ///     Initializes a new instance of the <see cref="ArrayPool{T}" /> class.
        /// </summary>
        /// <param name="bufferLength">    Length of the buffer. </param>
        /// <param name="numberOfBuffers"> (Optional) Number of buffers. </param>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when one or more arguments are outside
        ///     the required range.
        /// </exception>
        public ArrayPool(int bufferLength, int numberOfBuffers = 10)
        {
            if (bufferLength    <= 0) { throw new ArgumentOutOfRangeException(nameof(bufferLength)); }
            if (numberOfBuffers <= 0) { throw new ArgumentOutOfRangeException(nameof(numberOfBuffers)); }

            _bufferLength = bufferLength;
            _lock         = new SpinLock(Debugger.IsAttached);
            _buffers      = new T[numberOfBuffers][];
        }

        /// <summary>
        ///     Gets the rent.
        /// </summary>
        /// <returns>
        ///     A T[].
        /// </returns>
        public T[] Rent()
        {
            T[] buffer = null;

            bool lockTaken = false;
            try
            {
                _lock.Enter(ref lockTaken);

                if (_index < _buffers.Length)
                {
                    buffer             = _buffers[_index];
                    _buffers[_index++] = null;
                }
            }
            finally
            {
                if (lockTaken)
                {
                    _lock.Exit(false);
                }
            }

            return buffer ?? new T[_bufferLength];
        }

        /// <summary>
        ///     Returns.
        /// </summary>
        /// <param name="array">      The array. </param>
        /// <param name="clearArray"> True to clear array. </param>
        /// <exception cref="ArgumentException">
        ///     Thrown when one or more arguments have unsupported or
        ///     illegal values.
        /// </exception>
        public void Return(T[] array, bool clearArray)
        {
            if (array.Length != _bufferLength)
            {
                throw new ArgumentException(nameof(array));
            }

            bool lockTaken = false;
            try
            {
                _lock.Enter(ref lockTaken);

                if (_index != 0)
                {
                    if (clearArray) { Array.Clear(array, 0, array.Length); }
                    _buffers[--_index] = array;
                }
            }
            finally
            {
                if (lockTaken)
                {
                    _lock.Exit(false);
                }
            }
        }
    }
}