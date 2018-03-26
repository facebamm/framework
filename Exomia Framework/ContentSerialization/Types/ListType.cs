﻿using System;
using System.Collections.Generic;
using Exomia.Framework.ContentSerialization.Exceptions;

namespace Exomia.Framework.ContentSerialization.Types
{
    /// <summary>
    ///     ListType class
    /// </summary>
    internal sealed class ListType : IType
    {
        /// <summary>
        ///     constructor EnumType
        /// </summary>
        public ListType()
        {
            BaseType = typeof(List<>);
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
        ///     <see cref="IType.CreateType(string)" />
        /// </summary>
        public Type CreateType(string genericTypeInfo)
        {
            genericTypeInfo.GetInnerType(out string bti, out string gti);

            if (ContentSerializer.s_types.TryGetValue(bti, out IType it))
            {
                return BaseType.MakeGenericType(it.CreateType(gti));
            }
            return BaseType.MakeGenericType(bti.CreateType());
        }

        /// <summary>
        ///     <see cref="IType.CreateTypeInfo(Type)" />
        /// </summary>
        public string CreateTypeInfo(Type type)
        {
            Type[] gArgs = type.GetGenericArguments();
            if (gArgs.Length == 1)
            {
                string genericTypeInfo =
                    ContentSerializer.s_types.TryGetValue(gArgs[0].Name.ToUpper(), out IType it)
                    || ContentSerializer.s_types.TryGetValue(gArgs[0].BaseType.Name.ToUpper(), out it)
                        ? it.CreateTypeInfo(gArgs[0])
                        : gArgs[0].ToString();

                return $"{TypeName}<{genericTypeInfo}>";
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
                    $"ERROR: NO DIMENSION INFO FOUND EXPECTED: LIST<GENERIC_TYPE_INFO>(count) -> LIST{genericTypeInfo}{dimensionInfo}");
            }
            if (string.IsNullOrEmpty(genericTypeInfo))
            {
                throw new CSReaderException($"ERROR: NO GENERIC TYPE INFO DEFINED -> LIST<GENERIC_TYPE_INFO>");
            }

            genericTypeInfo.GetInnerType(out string bti, out string gti);

            Type elementType = null;
            Func<CSStreamReader, string, object> readCallback = null;
            if (ContentSerializer.s_types.TryGetValue(bti, out IType it))
            {
                elementType = it.CreateType(gti);
                readCallback = (s, d) =>
                {
                    try
                    {
                        return it.Read(stream, string.Empty, gti, d);
                    }
                    catch { throw; }
                };
            }
            else
            {
                elementType = bti.CreateType();
                readCallback = (s, d) =>
                {
                    return ContentSerializer.Read(stream, elementType, string.Empty);
                };
            }

            int count = GetListCount(dimensionInfo);

            Type list = BaseType.MakeGenericType(elementType);
            object obj = Activator.CreateInstance(list, count);
            AddListContent(stream, readCallback, (dynamic)obj, count);
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
            try
            {
                Write(writeHandler, tabSpace, key, (dynamic)content, useTypeInfo);
            }
            catch { throw; }
        }

        private void Write<T>(Action<string, string> writeHandler, string tabSpace, string key, List<T> content,
            bool useTypeInfo = true)
        {
            writeHandler(
                tabSpace,
                $"[{key}:{(useTypeInfo ? CreateTypeInfo(content.GetType()) : string.Empty)}({content.Count})]");
            ForeachListDimension(writeHandler, tabSpace + ContentSerializer.TABSPACE, content);
            writeHandler(tabSpace, $"[/{(useTypeInfo ? key : string.Empty)}]");
        }

        #region WriteHelper

        private static void ForeachListDimension<T>(Action<string, string> writeHandler, string tabSpace, List<T> list)
        {
            foreach (T entry in list)
            {
                Type elementType = entry.GetType();
                if (ContentSerializer.s_types.TryGetValue(elementType.Name.ToUpper(), out IType it) ||
                    ContentSerializer.s_types.TryGetValue(elementType.BaseType.Name.ToUpper(), out it))
                {
                    it.Write(writeHandler, tabSpace, string.Empty, entry, false);
                }
                else
                {
                    writeHandler(tabSpace, "[:]");
                    ContentSerializer.Write(writeHandler, tabSpace + ContentSerializer.TABSPACE, entry, elementType);
                    writeHandler(tabSpace, "[/]");
                }
            }
        }

        #endregion

        #region ReaderHelper

        private static int GetListCount(string listTypeInfo)
        {
            string start = "(";
            string end = ")";

            int sIndex = listTypeInfo.IndexOf(start);
            if (sIndex == -1)
            {
                throw new CSTypeException("No dimension start definition found in '" + listTypeInfo + "'");
            }
            sIndex += start.Length;

            int eIndex = listTypeInfo.LastIndexOf(end);
            if (eIndex == -1)
            {
                throw new CSTypeException("No dimension end definition found in '" + listTypeInfo + "'");
            }

            string dimensionInfo = listTypeInfo.Substring(sIndex, eIndex - sIndex).Trim();

            if (!int.TryParse(dimensionInfo, out int buffer))
            {
                throw new CSTypeException("Invalid dimension format found in '" + dimensionInfo + "'");
            }
            return buffer;
        }

        private static void AddListContent<T>(CSStreamReader stream, Func<CSStreamReader, string, object> readCallback,
            List<T> list, int count)
        {
            for (int i = 0; i < count; i++)
            {
                stream.ReadStartTag(out string key, out string dimensionInfo);

                list.Add((dynamic)readCallback(stream, dimensionInfo));
            }
        }

        #endregion
    }
}