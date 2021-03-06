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

using Exomia.Framework.Mathematics;

namespace Exomia.Framework.Noise
{
    /// <summary>
    ///     A value noise.
    /// </summary>
    public class ValueNoise : NoiseBase
    {
        /// <inheritdoc />
        public ValueNoise(int                    seed, float frequency, int octaves,
                          NoiseInterpolationType noiseInterpolationType = NoiseInterpolationType.Linear,
                          NoiseFractalType       noiseFractalType       = NoiseFractalType.BrownianMotion)
            : base(
                seed, frequency, octaves,
                noiseInterpolationType, noiseFractalType) { }

        /// <inheritdoc />
        public ValueNoise(int                    seed, float frequency, int octaves, float lacunarity, float gain,
                          NoiseInterpolationType noiseInterpolationType = NoiseInterpolationType.Linear,
                          NoiseFractalType       noiseFractalType       = NoiseFractalType.BrownianMotion)
            : base(
                seed, frequency, octaves, lacunarity, gain,
                noiseInterpolationType, noiseFractalType) { }

        /// <inheritdoc />
        protected override float Single(int seed, double x)
        {
            int x0 = Math2.Floor(x);

            double xs;
            switch (_noiseInterpolationType)
            {
                default:
                case NoiseInterpolationType.Linear:
                    xs = x - x0;
                    break;
                case NoiseInterpolationType.Hermite:
                    xs = Math2.CurveHermite(x - x0);
                    break;
                case NoiseInterpolationType.Quintic:
                    xs = Math2.CurveQuintic(x - x0);
                    break;
            }

            return Math2.Lerp(
                ValueCoord1D(seed, x0),
                ValueCoord1D(seed, x0 + 1),
                xs);
        }

        /// <inheritdoc />
        protected override float Single(int seed, double x, double y)
        {
            int x0 = Math2.Floor(x);
            int y0 = Math2.Floor(y);
            int x1 = x0 + 1;
            int y1 = y0 + 1;

            double xs, ys;
            switch (_noiseInterpolationType)
            {
                default:
                case NoiseInterpolationType.Linear:
                    xs = x - x0;
                    ys = y - y0;
                    break;
                case NoiseInterpolationType.Hermite:
                    xs = Math2.CurveHermite(x - x0);
                    ys = Math2.CurveHermite(y - y0);
                    break;
                case NoiseInterpolationType.Quintic:
                    xs = Math2.CurveQuintic(x - x0);
                    ys = Math2.CurveQuintic(y - y0);
                    break;
            }

            return Math2.Lerp(
                Math2.Lerp(
                    ValueCoord2D(seed, x0, y0),
                    ValueCoord2D(seed, x1, y0),
                    xs),
                Math2.Lerp(
                    ValueCoord2D(seed, x0, y1),
                    ValueCoord2D(seed, x1, y1),
                    xs), ys);
        }

        /// <inheritdoc />
        protected override float Single(int seed, double x, double y, double z)
        {
            int x0 = Math2.Floor(x);
            int y0 = Math2.Floor(y);
            int z0 = Math2.Floor(z);
            int x1 = x0 + 1;
            int y1 = y0 + 1;
            int z1 = z0 + 1;

            double xs, ys, zs;
            switch (_noiseInterpolationType)
            {
                default:
                case NoiseInterpolationType.Linear:
                    xs = x - x0;
                    ys = y - y0;
                    zs = z - z0;
                    break;
                case NoiseInterpolationType.Hermite:
                    xs = Math2.CurveHermite(x - x0);
                    ys = Math2.CurveHermite(y - y0);
                    zs = Math2.CurveHermite(z - z0);
                    break;
                case NoiseInterpolationType.Quintic:
                    xs = Math2.CurveQuintic(x - x0);
                    ys = Math2.CurveQuintic(y - y0);
                    zs = Math2.CurveQuintic(z - z0);
                    break;
            }

            return Math2.Lerp(
                Math2.Lerp(
                    Math2.Lerp(
                        ValueCoord3D(seed, x0, y0, z0),
                        ValueCoord3D(seed, x1, y0, z0),
                        xs),
                    Math2.Lerp(
                        ValueCoord3D(seed, x0, y1, z0),
                        ValueCoord3D(seed, x1, y1, z0),
                        xs),
                    ys),
                Math2.Lerp(
                    Math2.Lerp(
                        ValueCoord3D(seed, x0, y0, z1),
                        ValueCoord3D(seed, x1, y0, z1),
                        xs),
                    Math2.Lerp(
                        ValueCoord3D(seed, x0, y1, z1),
                        ValueCoord3D(seed, x1, y1, z1),
                        xs),
                    ys),
                zs);
        }
    }
}