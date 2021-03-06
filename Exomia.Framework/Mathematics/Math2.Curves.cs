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

using System.Runtime.CompilerServices;

namespace Exomia.Framework.Mathematics
{
    /// <content>
    ///     The mathematics 2.
    /// </content>
    public static partial class Math2
    {
        /// <summary>
        ///     Hermite Curve.
        /// </summary>
        /// <param name="t"> t. </param>
        /// <returns>
        ///     A double.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float CurveHermite(float t)
        {
            return t * t * (3 - (2 * t));
        }

        /// <summary>
        ///     Hermite Curve.
        /// </summary>
        /// <param name="t"> t. </param>
        /// <returns>
        ///     A double.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double CurveHermite(double t)
        {
            return t * t * (3 - (2 * t));
        }

        /// <summary>
        ///     Quintic Curve.
        /// </summary>
        /// <param name="t"> t. </param>
        /// <returns>
        ///     A float.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float CurveQuintic(float t)
        {
            return t * t * t * ((t * ((t * 6) - 15)) + 10);
        }

        /// <summary>
        ///     Quintic Curve.
        /// </summary>
        /// <param name="t"> t. </param>
        /// <returns>
        ///     A double.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double CurveQuintic(double t)
        {
            return t * t * t * ((t * ((t * 6) - 15)) + 10);
        }
    }
}