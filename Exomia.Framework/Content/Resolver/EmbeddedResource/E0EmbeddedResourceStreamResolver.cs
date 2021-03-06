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
using System.Reflection;
using Exomia.Framework.ContentSerialization;

namespace Exomia.Framework.Content.Resolver.EmbeddedResource
{
    /// <summary>
    ///     A 0 embedded resource stream resolver.
    /// </summary>
    [ContentResolver(int.MinValue + 1)]
    class E0EmbeddedResourceStreamResolver : IEmbeddedResourceResolver
    {
        /// <inheritdoc />
        public bool Exists(Type assetType, string assetName, out Assembly assembly)
        {
            if (Path.GetExtension(assetName) == ContentSerializer.DEFAULT_EXTENSION)
            {
                assembly = assetType.Assembly;
                string name = $"{assembly.GetName().Name}.{assetName}";
                foreach (string resourceName in assembly.GetManifestResourceNames())
                {
                    if (resourceName == name)
                    {
                        return true;
                    }
                }

                assembly = Assembly.GetEntryAssembly();
                name     = $"{assembly.GetName().Name}.{assetName}";
                foreach (string resourceName in assembly.GetManifestResourceNames())
                {
                    if (resourceName == name)
                    {
                        return true;
                    }
                }
            }
            assembly = null;
            return false;
        }

        /// <inheritdoc />
        public Stream Resolve(Assembly assembly, string assetName)
        {
            return assembly.GetManifestResourceStream($"{assembly.GetName().Name}.{assetName}");
        }
    }
}