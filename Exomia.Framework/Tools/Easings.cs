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

namespace Exomia.Framework.Tools
{
    /// <summary>
    ///     EasingFunction delegate.
    /// </summary>
    /// <param name="t"> time (0 - duration) </param>
    /// <param name="b"> initial value. </param>
    /// <param name="c"> change in value or difference between final and initial value. </param>
    /// <param name="d"> duration. </param>
    /// <returns>
    ///     float value.
    /// </returns>
    public delegate float EasingFunction(float t, float b, float c, float d);

    /// <summary>
    ///     An easing.
    /// </summary>
    public static class Easing
    {
        /// <summary>
        ///     A bounce.
        /// </summary>
        public static class Bounce
        {
            /// <summary>
            ///     The first b.
            /// </summary>
            private const float B1 = 1.0f / 2.75f;

            /// <summary>
            ///     The second b.
            /// </summary>
            private const float B2 = 1.5f / 2.75f;

            /// <summary>
            ///     The third b.
            /// </summary>
            private const float B3 = 2.0f / 2.75f;

            /// <summary>
            ///     The fourth b.
            /// </summary>
            private const float B4 = 2.25f / 2.75f;

            /// <summary>
            ///     The fifth b.
            /// </summary>
            private const float B5 = 2.5f / 2.75f;

            /// <summary>
            ///     The b 6.
            /// </summary>
            private const float B6 = 2.625f / 2.75f;

            /// <summary>
            ///     Ease in.
            /// </summary>
            /// <param name="t"> The float to process. </param>
            /// <param name="b"> The float to process. </param>
            /// <param name="c"> The float to process. </param>
            /// <param name="d"> The float to process. </param>
            /// <returns>
            ///     A float.
            /// </returns>
            public static float EaseIn(float t, float b, float c, float d)
            {
                return (c - EaseOut(d - t, 0, c, d)) + b;
            }

            /// <summary>
            ///     Ease in out.
            /// </summary>
            /// <param name="t"> The float to process. </param>
            /// <param name="b"> The float to process. </param>
            /// <param name="c"> The float to process. </param>
            /// <param name="d"> The float to process. </param>
            /// <returns>
            ///     A float.
            /// </returns>
            public static float EaseInOut(float t, float b, float c, float d)
            {
                if (t < d / 2) { return (EaseIn(t * 2, 0, c, d) * .5f) + b; }
                return (EaseOut((t * 2) - d, 0, c, d) * .5f) + (c * .5f) + b;
            }

            /// <summary>
            ///     Ease out.
            /// </summary>
            /// <param name="t"> The float to process. </param>
            /// <param name="b"> The float to process. </param>
            /// <param name="c"> The float to process. </param>
            /// <param name="d"> The float to process. </param>
            /// <returns>
            ///     A float.
            /// </returns>
            public static float EaseOut(float t, float b, float c, float d)
            {
                if ((t /= d) < B1)
                {
                    return (c * 7.5625f * t * t) + b;
                }
                if (t < B3)
                {
                    return (c * ((7.5625f * (t -= B2) * t) + .75f)) + b;
                }
                if (t < B5)
                {
                    return (c * ((7.5625f * (t -= B4) * t) + .9375f)) + b;
                }
                return (c * ((7.5625f * (t -= B6) * t) + .984375f)) + b;
            }
        }

        /// <summary>
        ///     A circle.
        /// </summary>
        public static class Circle
        {
            /// <summary>
            ///     Ease in.
            /// </summary>
            /// <param name="t"> The float to process. </param>
            /// <param name="b"> The float to process. </param>
            /// <param name="c"> The float to process. </param>
            /// <param name="d"> The float to process. </param>
            /// <returns>
            ///     A float.
            /// </returns>
            public static float EaseIn(float t, float b, float c, float d)
            {
                return (-c * ((float)Math.Sqrt(1 - ((t /= d) * t)) - 1)) + b;
            }

            /// <summary>
            ///     Ease in out.
            /// </summary>
            /// <param name="t"> The float to process. </param>
            /// <param name="b"> The float to process. </param>
            /// <param name="c"> The float to process. </param>
            /// <param name="d"> The float to process. </param>
            /// <returns>
            ///     A float.
            /// </returns>
            public static float EaseInOut(float t, float b, float c, float d)
            {
                if ((t /= d / 2) < 1) { return ((-c / 2) * ((float)Math.Sqrt(1 - (t * t)) - 1)) + b; }
                return ((c                               / 2) * ((float)Math.Sqrt(1 - ((t -= 2) * t)) + 1)) + b;
            }

            /// <summary>
            ///     Ease out.
            /// </summary>
            /// <param name="t"> The float to process. </param>
            /// <param name="b"> The float to process. </param>
            /// <param name="c"> The float to process. </param>
            /// <param name="d"> The float to process. </param>
            /// <returns>
            ///     A float.
            /// </returns>
            public static float EaseOut(float t, float b, float c, float d)
            {
                return (c * (float)Math.Sqrt(1 - ((t = (t / d) - 1) * t))) + b;
            }
        }

        /// <summary>
        ///     A cubic.
        /// </summary>
        public static class Cubic
        {
            /// <summary>
            ///     Ease in.
            /// </summary>
            /// <param name="t"> The float to process. </param>
            /// <param name="b"> The float to process. </param>
            /// <param name="c"> The float to process. </param>
            /// <param name="d"> The float to process. </param>
            /// <returns>
            ///     A float.
            /// </returns>
            public static float EaseIn(float t, float b, float c, float d)
            {
                return (c * (t /= d) * t * t) + b;
            }

            /// <summary>
            ///     Ease in out.
            /// </summary>
            /// <param name="t"> The float to process. </param>
            /// <param name="b"> The float to process. </param>
            /// <param name="c"> The float to process. </param>
            /// <param name="d"> The float to process. </param>
            /// <returns>
            ///     A float.
            /// </returns>
            public static float EaseInOut(float t, float b, float c, float d)
            {
                if ((t /= d / 2) < 1) { return ((c / 2) * t * t * t) + b; }
                return ((c                                      / 2) * (((t -= 2) * t * t) + 2)) + b;
            }

            /// <summary>
            ///     Ease out.
            /// </summary>
            /// <param name="t"> The float to process. </param>
            /// <param name="b"> The float to process. </param>
            /// <param name="c"> The float to process. </param>
            /// <param name="d"> The float to process. </param>
            /// <returns>
            ///     A float.
            /// </returns>
            public static float EaseOut(float t, float b, float c, float d)
            {
                return (c * (((t = (t / d) - 1) * t * t) + 1)) + b;
            }
        }

        /// <summary>
        ///     An expo.
        /// </summary>
        public static class Expo
        {
            /// <summary>
            ///     Ease in.
            /// </summary>
            /// <param name="t"> The float to process. </param>
            /// <param name="b"> The float to process. </param>
            /// <param name="c"> The float to process. </param>
            /// <param name="d"> The float to process. </param>
            /// <returns>
            ///     A float.
            /// </returns>
            public static float EaseIn(float t, float b, float c, float d)
            {
                return t == 0 ? b : (c * (float)Math.Pow(2, 10 * ((t / d) - 1))) + b;
            }

            /// <summary>
            ///     Ease in out.
            /// </summary>
            /// <param name="t"> The float to process. </param>
            /// <param name="b"> The float to process. </param>
            /// <param name="c"> The float to process. </param>
            /// <param name="d"> The float to process. </param>
            /// <returns>
            ///     A float.
            /// </returns>
            public static float EaseInOut(float t, float b, float c, float d)
            {
                if (t            == 0) { return b; }
                if (t            == d) { return b                                           + c; }
                if ((t /= d / 2) < 1) { return ((c / 2) * (float)Math.Pow(2, 10 * (t - 1))) + b; }
                return ((c                              / 2) * (-(float)Math.Pow(2, -10 * --t) + 2)) + b;
            }

            /// <summary>
            ///     Ease out.
            /// </summary>
            /// <param name="t"> The float to process. </param>
            /// <param name="b"> The float to process. </param>
            /// <param name="c"> The float to process. </param>
            /// <param name="d"> The float to process. </param>
            /// <returns>
            ///     A float.
            /// </returns>
            public static float EaseOut(float t, float b, float c, float d)
            {
                return t == d ? b + c : (c * (-(float)Math.Pow(2, (-10 * t) / d) + 1)) + b;
            }
        }

        /// <summary>
        ///     A linear.
        /// </summary>
        public static class Linear
        {
            /// <summary>
            ///     Ease none.
            /// </summary>
            /// <param name="t"> The float to process. </param>
            /// <param name="b"> The float to process. </param>
            /// <param name="c"> The float to process. </param>
            /// <param name="d"> The float to process. </param>
            /// <returns>
            ///     A float.
            /// </returns>
            public static float EaseNone(float t, float b, float c, float d)
            {
                return ((c * t) / d) + b;
            }
        }

        /// <summary>
        ///     A quad.
        /// </summary>
        public static class Quad
        {
            /// <summary>
            ///     Ease in.
            /// </summary>
            /// <param name="t"> The float to process. </param>
            /// <param name="b"> The float to process. </param>
            /// <param name="c"> The float to process. </param>
            /// <param name="d"> The float to process. </param>
            /// <returns>
            ///     A float.
            /// </returns>
            public static float EaseIn(float t, float b, float c, float d)
            {
                return (c * (t /= d) * t) + b;
            }

            /// <summary>
            ///     Ease in out.
            /// </summary>
            /// <param name="t"> The float to process. </param>
            /// <param name="b"> The float to process. </param>
            /// <param name="c"> The float to process. </param>
            /// <param name="d"> The float to process. </param>
            /// <returns>
            ///     A float.
            /// </returns>
            public static float EaseInOut(float t, float b, float c, float d)
            {
                if ((t /= d / 2) < 1) { return ((c / 2) * t * t) + b; }
                return ((-c                                 / 2) * ((--t * (t - 2)) - 1)) + b;
            }

            /// <summary>
            ///     Ease out.
            /// </summary>
            /// <param name="t"> The float to process. </param>
            /// <param name="b"> The float to process. </param>
            /// <param name="c"> The float to process. </param>
            /// <param name="d"> The float to process. </param>
            /// <returns>
            ///     A float.
            /// </returns>
            public static float EaseOut(float t, float b, float c, float d)
            {
                return (-c * (t /= d) * (t - 2)) + b;
            }
        }

        /// <summary>
        ///     A quart.
        /// </summary>
        public static class Quart
        {
            /// <summary>
            ///     Ease in.
            /// </summary>
            /// <param name="t"> The float to process. </param>
            /// <param name="b"> The float to process. </param>
            /// <param name="c"> The float to process. </param>
            /// <param name="d"> The float to process. </param>
            /// <returns>
            ///     A float.
            /// </returns>
            public static float EaseIn(float t, float b, float c, float d)
            {
                return (c * (t /= d) * t * t * t) + b;
            }

            /// <summary>
            ///     Ease in out.
            /// </summary>
            /// <param name="t"> The float to process. </param>
            /// <param name="b"> The float to process. </param>
            /// <param name="c"> The float to process. </param>
            /// <param name="d"> The float to process. </param>
            /// <returns>
            ///     A float.
            /// </returns>
            public static float EaseInOut(float t, float b, float c, float d)
            {
                if ((t /= d / 2) < 1) { return ((c / 2) * t * t * t * t) + b; }
                return ((-c                                         / 2) * (((t -= 2) * t * t * t) - 2)) + b;
            }

            /// <summary>
            ///     Ease out.
            /// </summary>
            /// <param name="t"> The float to process. </param>
            /// <param name="b"> The float to process. </param>
            /// <param name="c"> The float to process. </param>
            /// <param name="d"> The float to process. </param>
            /// <returns>
            ///     A float.
            /// </returns>
            public static float EaseOut(float t, float b, float c, float d)
            {
                return (-c * (((t = (t / d) - 1) * t * t * t) - 1)) + b;
            }
        }

        /// <summary>
        ///     A quint.
        /// </summary>
        public static class Quint
        {
            /// <summary>
            ///     Ease in.
            /// </summary>
            /// <param name="t"> The float to process. </param>
            /// <param name="b"> The float to process. </param>
            /// <param name="c"> The float to process. </param>
            /// <param name="d"> The float to process. </param>
            /// <returns>
            ///     A float.
            /// </returns>
            public static float EaseIn(float t, float b, float c, float d)
            {
                return (c * (t /= d) * t * t * t * t) + b;
            }

            /// <summary>
            ///     Ease in out.
            /// </summary>
            /// <param name="t"> The float to process. </param>
            /// <param name="b"> The float to process. </param>
            /// <param name="c"> The float to process. </param>
            /// <param name="d"> The float to process. </param>
            /// <returns>
            ///     A float.
            /// </returns>
            public static float EaseInOut(float t, float b, float c, float d)
            {
                if ((t /= d / 2) < 1) { return ((c / 2) * t * t * t * t * t) + b; }
                return ((c                                              / 2) * (((t -= 2) * t * t * t * t) + 2)) + b;
            }

            /// <summary>
            ///     Ease out.
            /// </summary>
            /// <param name="t"> The float to process. </param>
            /// <param name="b"> The float to process. </param>
            /// <param name="c"> The float to process. </param>
            /// <param name="d"> The float to process. </param>
            /// <returns>
            ///     A float.
            /// </returns>
            public static float EaseOut(float t, float b, float c, float d)
            {
                return (c * (((t = (t / d) - 1) * t * t * t * t) + 1)) + b;
            }
        }

        /// <summary>
        ///     A sine.
        /// </summary>
        public static class Sine
        {
            /// <summary>
            ///     Ease in.
            /// </summary>
            /// <param name="t"> The float to process. </param>
            /// <param name="b"> The float to process. </param>
            /// <param name="c"> The float to process. </param>
            /// <param name="d"> The float to process. </param>
            /// <returns>
            ///     A float.
            /// </returns>
            public static float EaseIn(float t, float b, float c, float d)
            {
                return (-c * (float)Math.Cos((t / d) * (Math.PI / 2))) + c + b;
            }

            /// <summary>
            ///     Ease in out.
            /// </summary>
            /// <param name="t"> The float to process. </param>
            /// <param name="b"> The float to process. </param>
            /// <param name="c"> The float to process. </param>
            /// <param name="d"> The float to process. </param>
            /// <returns>
            ///     A float.
            /// </returns>
            public static float EaseInOut(float t, float b, float c, float d)
            {
                return ((-c / 2) * ((float)Math.Cos((Math.PI * t) / d) - 1)) + b;
            }

            /// <summary>
            ///     Ease out.
            /// </summary>
            /// <param name="t"> The float to process. </param>
            /// <param name="b"> The float to process. </param>
            /// <param name="c"> The float to process. </param>
            /// <param name="d"> The float to process. </param>
            /// <returns>
            ///     A float.
            /// </returns>
            public static float EaseOut(float t, float b, float c, float d)
            {
                return (c * (float)Math.Sin((t / d) * (Math.PI / 2))) + b;
            }
        }
    }
}