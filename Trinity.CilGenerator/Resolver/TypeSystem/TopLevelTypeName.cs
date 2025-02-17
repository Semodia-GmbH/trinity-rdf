﻿// Copyright (c) 2010-2013 AlphaSierraPapa for the SharpDevelop Team
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this
// software and associated documentation files (the "Software"), to deal in the Software
// without restriction, including without limitation the rights to use, copy, modify, merge,
// publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons
// to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or
// substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED,
// INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR
// PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE
// FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR
// OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
// DEALINGS IN THE SOFTWARE.

using System;
using System.Text;

namespace ICSharpCode.Decompiler.TypeSystem
{
    /// <summary>
    /// Holds the name of a top-level type.
    /// This struct cannot refer to nested classes.
    /// </summary>
    [Serializable]
    public struct TopLevelTypeName : IEquatable<TopLevelTypeName>
    {
        readonly string namespaceName;
        readonly string name;
        readonly int typeParameterCount;

        public TopLevelTypeName(string namespaceName, string name, int typeParameterCount = 0)
        {
            if (namespaceName == null)
                throw new ArgumentNullException("namespaceName");
            if (name == null)
                throw new ArgumentNullException("name");
            this.namespaceName = namespaceName;
            this.name = name;
            this.typeParameterCount = typeParameterCount;
        }

        public TopLevelTypeName(string reflectionName)
        {
            var pos = reflectionName.LastIndexOf('.');
            if (pos < 0)
            {
                namespaceName = string.Empty;
                name = reflectionName;
            }
            else
            {
                namespaceName = reflectionName.Substring(0, pos);
                name = reflectionName.Substring(pos + 1);
            }
            name = SRMExtensions.SplitTypeParameterCountFromReflectionName(name, out typeParameterCount);
        }

        public string Namespace => namespaceName;

        public string Name => name;

        public int TypeParameterCount => typeParameterCount;

        public string ReflectionName
        {
            get
            {
                var b = new StringBuilder();
                if (!string.IsNullOrEmpty(namespaceName))
                {
                    b.Append(namespaceName);
                    b.Append('.');
                }
                b.Append(name);
                if (typeParameterCount > 0)
                {
                    b.Append('`');
                    b.Append(typeParameterCount);
                }
                return b.ToString();
            }
        }

        public override string ToString()
        {
            return this.ReflectionName;
        }

        public override bool Equals(object obj)
        {
            return (obj is TopLevelTypeName) && Equals((TopLevelTypeName)obj);
        }

        public bool Equals(TopLevelTypeName other)
        {
            return this.namespaceName == other.namespaceName && this.name == other.name && this.typeParameterCount == other.typeParameterCount;
        }

        public override int GetHashCode()
        {
            return (name != null ? name.GetHashCode() : 0) ^ (namespaceName != null ? namespaceName.GetHashCode() : 0) ^ typeParameterCount;
        }

        public static bool operator ==(TopLevelTypeName lhs, TopLevelTypeName rhs)
        {
            return lhs.Equals(rhs);
        }

        public static bool operator !=(TopLevelTypeName lhs, TopLevelTypeName rhs)
        {
            return !lhs.Equals(rhs);
        }
    }

}