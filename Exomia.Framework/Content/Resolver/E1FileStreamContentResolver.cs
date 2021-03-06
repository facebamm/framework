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

using System.IO;
using Exomia.Framework.ContentSerialization.Compression;

namespace Exomia.Framework.Content.Resolver
{
    /// <summary>
    ///     A 1 file stream content resolver. This class cannot be inherited.
    /// </summary>
    [ContentResolver(int.MinValue)]
    sealed class E1FileStreamContentResolver : IContentResolver
    {
        /// <inheritdoc />
        public bool Exists(string assetName)
        {
            return Path.GetExtension(assetName) == ContentCompressor.DEFAULT_COMPRESSED_EXTENSION &&
                   File.Exists(assetName);
        }

        /// <inheritdoc />
        public Stream Resolve(string assetName)
        {
            using (FileStream stream = new FileStream(assetName, FileMode.Open, FileAccess.Read))
            {
                return ContentCompressor.DecompressStream(stream, out Stream stream2) ? stream2 : null;
            }
        }
    }
}