﻿#region MIT License

// Copyright (c) 2018 exomia - Daniel Bätz
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
using Exomia.Framework.ContentSerialization.Exceptions;

namespace Exomia.Framework.ContentSerialization.Types
{
    /// <summary>
    ///     ListType class
    /// </summary>
    sealed class DictionaryType : IType
    {
        /// <summary>
        ///     typeof(Array)
        /// </summary>
        public Type BaseType { get; }

        /// <summary>
        ///     <see cref="IType.IsPrimitive()" />
        /// </summary>
        public bool IsPrimitive
        {
            get { return false; }
        }

        /// <summary>
        ///     TypeName without System
        ///     !ALL UPPER CASE!
        /// </summary>
        public string TypeName
        {
            get { return BaseType.Name.ToUpper(); }
        }

        /// <summary>
        ///     constructor EnumType
        /// </summary>
        public DictionaryType()
        {
            BaseType = typeof(Dictionary<,>);
        }

        /// <summary>
        ///     <see cref="IType.CreateType(string)" />
        /// </summary>
        public Type CreateType(string genericTypeInfo)
        {
            genericTypeInfo.GetKeyValueInnerType(
                out string keyBaseTypeInfo, out string valueBaseTypeInfo, out string valueGenericTypeInfo);

            if (!ContentSerializer.s_types.TryGetValue(keyBaseTypeInfo, out IType itk) || !itk.IsPrimitive)
            {
                throw new NotSupportedException($"ERROR: INVALID KEY TYPE FOUND IN -> '{genericTypeInfo}'");
            }

            return ContentSerializer.s_types.TryGetValue(valueBaseTypeInfo, out IType itv)
                ? BaseType.MakeGenericType(itk.CreateType(string.Empty), itv.CreateType(valueGenericTypeInfo))
                : BaseType.MakeGenericType(itk.CreateType(string.Empty), valueBaseTypeInfo.CreateType());
        }

        /// <summary>
        ///     <see cref="IType.CreateTypeInfo(Type)" />
        /// </summary>
        public string CreateTypeInfo(Type type)
        {
            Type[] gArgs = type.GetGenericArguments();
            if (gArgs.Length == 2)
            {
                string genericTypeInfo1 =
                    ContentSerializer.s_types.TryGetValue(gArgs[0].Name.ToUpper(), out IType itk)
                        ? itk.CreateTypeInfo(gArgs[0])
                        : throw new NotSupportedException(
                            $"the type of '{gArgs[0]}' is not supported as dictionary key");
                string genericTypeInfo2 =
                    ContentSerializer.s_types.TryGetValue(gArgs[1].Name.ToUpper(), out IType itv) ||
                    ContentSerializer.s_types.TryGetValue(
                        (gArgs[1].BaseType ?? throw new NullReferenceException()).Name.ToUpper(), out itv)
                        ? itv.CreateTypeInfo(gArgs[1])
                        : gArgs[1].ToString();

                return $"{TypeName}<{genericTypeInfo1}, {genericTypeInfo2}>";
            }
            throw new NotSupportedException(type.ToString());
        }

        /// <summary>
        ///     <see cref="IType.Read(CSStreamReader, string, string, string)" />
        /// </summary>
        public object Read(CSStreamReader stream, string key, string genericTypeInfo, string dimensionInfo)
        {
            if (string.IsNullOrEmpty(dimensionInfo))
            {
                throw new CSReaderException(
                    $"ERROR: NO DIMENSION INFO FOUND EXPECTED: DICTIONARY<GENERIC_TYPE_INFO1, GENERIC_TYPE_INFO2>(count) -> DICTIONARY{genericTypeInfo}{dimensionInfo}");
            }
            if (string.IsNullOrEmpty(genericTypeInfo))
            {
                throw new CSReaderException(
                    "ERROR: NO GENERIC TYPE INFO DEFINED -> DICTIONARY<GENERIC_TYPE_INFO1, GENERIC_TYPE_INFO2>");
            }

            genericTypeInfo.GetKeyValueInnerType(out string kbti, out string vbti, out string vgti);

            if (!ContentSerializer.s_types.TryGetValue(kbti, out IType itk) || !itk.IsPrimitive)
            {
                throw new NotSupportedException($"ERROR: INVALID KEY TYPE FOUND IN -> '{genericTypeInfo}'");
            }

            Type valueType;
            Func<CSStreamReader, string, object> readCallback;
            if (ContentSerializer.s_types.TryGetValue(vbti, out IType it))
            {
                valueType = it.CreateType(vgti);
                readCallback = (s, d) =>
                {
                    return it.Read(stream, string.Empty, vgti, d);
                };
            }
            else
            {
                valueType = vbti.CreateType();
                readCallback = (s, d) =>
                {
                    return ContentSerializer.Read(stream, valueType, string.Empty);
                };
            }

            int count = GetDictionaryCount(dimensionInfo);

            Type dic = BaseType.MakeGenericType(itk.CreateType(string.Empty), valueType);
            object obj = System.Activator.CreateInstance(dic, count);
            AddDictionaryContent(stream, readCallback, (dynamic)obj, count);
            stream.ReadTag($"/{key}");
            return obj;
        }

        /// <summary>
        ///     <see cref="IType.Write(Action{string, string}, string, string, object, bool)" />
        /// </summary>
        public void Write(Action<string, string> writeHandler, string tabSpace, string key, object content,
            bool useTypeInfo = true)
        {
            //[key:type]content[/key]
            Write(writeHandler, tabSpace, key, (dynamic)content, useTypeInfo);
        }

        #region WriteHelper

        private static void ForeachDictionaryDimension<TKey, TValue>(Action<string, string> writeHandler,
            string tabSpace, Dictionary<TKey, TValue> dic)
        {
            foreach (KeyValuePair<TKey, TValue> entry in dic)
            {
                Type elementType = entry.Value.GetType();
                if (ContentSerializer.s_types.TryGetValue(elementType.Name.ToUpper(), out IType it) ||
                    ContentSerializer.s_types.TryGetValue(
                        (elementType.BaseType ?? throw new NullReferenceException()).Name.ToUpper(), out it))
                {
                    it.Write(writeHandler, tabSpace, entry.Key.ToString(), entry.Value, false);
                }
                else
                {
                    writeHandler(tabSpace, $"[{entry.Key}:]");
                    ContentSerializer.Write(
                        writeHandler, tabSpace + ContentSerializer.TABSPACE, entry.Value, elementType);
                    writeHandler(tabSpace, "[/]");
                }
            }
        }

        #endregion

        private void Write<TKey, TValue>(Action<string, string> writeHandler, string tabSpace, string key,
            Dictionary<TKey, TValue> content, bool useTypeInfo = true)
        {
            writeHandler(
                tabSpace,
                $"[{key}:{(useTypeInfo ? CreateTypeInfo(content.GetType()) : string.Empty)}({content.Count})]");
            ForeachDictionaryDimension(writeHandler, tabSpace + ContentSerializer.TABSPACE, content);
            writeHandler(tabSpace, $"[/{(useTypeInfo ? key : string.Empty)}]");
        }

        #region ReaderHelper

        private static int GetDictionaryCount(string dictionaryTypeInfo)
        {
            const string START = "(";
            const string END = ")";

            int sIndex = dictionaryTypeInfo.IndexOf(START, StringComparison.Ordinal);
            if (sIndex == -1)
            {
                throw new CSTypeException("No dimension start definition found in '" + dictionaryTypeInfo + "'");
            }
            sIndex += START.Length;

            int eIndex = dictionaryTypeInfo.LastIndexOf(END, StringComparison.Ordinal);
            if (eIndex == -1)
            {
                throw new CSTypeException("No dimension end definition found in '" + dictionaryTypeInfo + "'");
            }

            string dimensionInfo = dictionaryTypeInfo.Substring(sIndex, eIndex - sIndex).Trim();

            if (!int.TryParse(dimensionInfo, out int buffer))
            {
                throw new CSTypeException("Invalid dimension format found in '" + dimensionInfo + "'");
            }
            return buffer;
        }

        private static void AddDictionaryContent<TKey, TValue>(CSStreamReader stream,
            Func<CSStreamReader, string, object> readCallback, Dictionary<TKey, TValue> dic, int count)
        {
            for (int i = 0; i < count; i++)
            {
                stream.ReadStartTag(out string key, out string dimensionInfo);

                dic.Add((TKey)Convert.ChangeType(key, typeof(TKey)), (dynamic)readCallback(stream, dimensionInfo));
            }
        }

        #endregion
    }
}