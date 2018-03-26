﻿using System;
using System.IO;
using Exomia.Framework.Graphics;
using SharpDX.Direct3D11;

namespace Exomia.Framework.Content
{
    /// <summary>
    ///     An interface to handle the <see cref="Texture2" /> class
    /// </summary>
    public interface ITexture2ContentManager : IDisposable
    {
        /// <summary>
        ///     returns true if the texture is invalid
        /// </summary>
        bool IsTextureInvalid { get; }

        /// <summary>
        ///     add a new texture from a given asset
        /// </summary>
        /// <param name="assetName">asset to load</param>
        /// <param name="startIndex">start index</param>
        /// <returns>a instance of <see cref="Texture2" /></returns>
        Texture2 AddTexture(string assetName, int startIndex = 0);

        /// <summary>
        ///     add a new texture from a given stream
        /// </summary>
        /// <param name="stream">texture stream</param>
        /// <param name="assetName">asset name</param>
        /// <param name="startIndex">start index</param>
        /// <returns>a instance of <see cref="Texture2" /></returns>
        Texture2 AddTexture(Stream stream, string assetName, int startIndex = 0);

        /// <summary>
        ///     add a new texture from a given stream
        /// </summary>
        /// <param name="stream">texture stream</param>
        /// <param name="assetName">out assetname</param>
        /// <param name="startIndex">start index</param>
        /// <returns>a instance of <see cref="Texture2" /></returns>
        Texture2 AddTexture(Stream stream, out string assetName, int startIndex = 0);

        /// <summary>
        ///     generate a texture array from all loaded <see cref="Texture2" /> textures.
        /// </summary>
        /// <param name="device">device</param>
        /// <param name="texture">out <see cref="Texture" /> array</param>
        /// <returns></returns>
        bool GenerateTexture2DArray(Device5 device, out Texture texture);

        /// <summary>
        ///     resets the content manager
        /// </summary>
        void Reset();
    }
}