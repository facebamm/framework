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
using System.IO;
using Exomia.Framework.Content;
using SharpDX;
using SharpDX.Direct3D11;

namespace Exomia.Framework.Graphics
{
    /// <summary>
    ///     A texture. This class cannot be inherited.
    /// </summary>
    [ContentReadable(typeof(TextureContentReader))]
    public sealed class Texture : IDisposable
    {
        /// <summary>
        ///     The texture view.
        /// </summary>
        private ShaderResourceView1 _textureView;

        /// <summary>
        ///     Height.
        /// </summary>
        /// <value>
        ///     The height.
        /// </value>
        public int Height { get; }

        /// <summary>
        ///     TextureView1.NativePointer.
        /// </summary>
        /// <value>
        ///     The texture pointer.
        /// </value>
        public IntPtr TexturePointer
        {
            get { return _textureView.NativePointer; }
        }

        /// <summary>
        ///     ShaderResourceView1.
        /// </summary>
        /// <value>
        ///     The texture view.
        /// </value>
        public ShaderResourceView1 TextureView
        {
            get { return _textureView; }
        }

        /// <summary>
        ///     Width.
        /// </summary>
        /// <value>
        ///     The width.
        /// </value>
        public int Width { get; }

        /// <summary>
        ///     Texture constructor.
        /// </summary>
        /// <param name="textureView"> ShaderResourceView1. </param>
        /// <param name="width">       width. </param>
        /// <param name="height">      height. </param>
        public Texture(ShaderResourceView1 textureView, int width, int height)
        {
            _textureView = textureView;
            Width        = width;
            Height       = height;
        }

        /// <summary>
        ///     Texture destructor.
        /// </summary>
        ~Texture()
        {
            Dispose(false);
        }

        /// <summary>
        ///     load a Texture from a given source stream.
        /// </summary>
        /// <param name="device"> device. </param>
        /// <param name="stream"> data stream. </param>
        /// <returns>
        ///     new texture.
        /// </returns>
        public static Texture Load(Device5 device, Stream stream)
        {
            try
            {
                using (Texture2D texture2D = TextureHelper.LoadTexture2D(device, stream))
                {
                    return new Texture(
                        new ShaderResourceView1(device, texture2D), texture2D.Description.Width,
                        texture2D.Description.Height);
                }
            }
            catch { return null; }
        }

        #region IDisposable Support

        /// <summary>
        ///     True if disposed.
        /// </summary>
        private bool _disposed;

        /// <summary>
        ///     Releases the unmanaged resources used by the Exomia.Framework.Graphics.Texture and
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
                    Utilities.Dispose(ref _textureView);
                }

                _disposed = true;
            }
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion
    }

    /// <summary>
    ///     A texture 2. This class cannot be inherited.
    /// </summary>
    [ContentReadable(typeof(Texture2ContentReader))]
    public sealed class Texture2 : IDisposable
    {
        /// <summary>
        ///     AssetName.
        /// </summary>
        /// <value>
        ///     The name of the asset.
        /// </value>
        public string AssetName { get; }

        /// <summary>
        ///     AtlasIndex.
        /// </summary>
        /// <value>
        ///     The atlas index.
        /// </value>
        public int AtlasIndex { get; }

        /// <summary>
        ///     Height.
        /// </summary>
        /// <value>
        ///     The height.
        /// </value>
        public int Height
        {
            get { return SourceRectangle.Height; }
        }

        /// <summary>
        ///     SourceRectangle.
        /// </summary>
        /// <value>
        ///     The source rectangle.
        /// </value>
        public Rectangle SourceRectangle { get; private set; }

        /// <summary>
        ///     Width.
        /// </summary>
        /// <value>
        ///     The width.
        /// </value>
        public int Width
        {
            get { return SourceRectangle.Width; }
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="Texture2" /> class.
        /// </summary>
        /// <param name="atlasIndex">      AtlasIndex. </param>
        /// <param name="assetName">       AssetName. </param>
        /// <param name="sourceRectangle"> SourceRectangle. </param>
        internal Texture2(int atlasIndex, string assetName, Rectangle sourceRectangle)
        {
            AtlasIndex      = atlasIndex;
            AssetName       = assetName;
            SourceRectangle = sourceRectangle;
        }

        /// <summary>
        ///     Texture2 destructor.
        /// </summary>
        ~Texture2()
        {
            Dispose(false);
        }

        #region IDisposable Support

        /// <summary>
        ///     True if disposed.
        /// </summary>
        private bool _disposed;

        /// <summary>
        ///     Releases the unmanaged resources used by the Exomia.Framework.Graphics.Texture2 and
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
                    SourceRectangle = Rectangle.Empty;
                }
                _disposed = true;
            }
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