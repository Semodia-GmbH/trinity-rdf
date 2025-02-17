﻿// LICENSE:
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
//
// AUTHORS:
//
//  Moritz Eberl <moritz@semiodesk.com>
//  Sebastian Faubel <sebastian@semiodesk.com>
//
// Copyright (c) Semiodesk GmbH 2015-2019

using Mono.Cecil;
using Mono.Cecil.Cil;
using Semiodesk.Trinity.CilGenerator.Extensions;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace Semiodesk.Trinity.CilGenerator.Tasks
{
    /// <summary>
    /// Helper class which encapsulates important information about a property definition.
    /// </summary>
    public class PropertyGeneratorTaskHelper
    {
        #region Members

        /// <summary>
        /// Property definition.
        /// </summary>
        public readonly PropertyDefinition Property;

        /// <summary>
        /// Backing field to store the current value of the property.
        /// </summary>
        public readonly FieldReference BackingField;

        /// <summary>
        /// Indicates if the property has a DefaultValue attribute.
        /// </summary>
        public bool HasDefaultValue;

        /// <summary>
        /// Default value attribute.
        /// </summary>
        public readonly CustomAttributeArgument DefaultValue;

        /// <summary>
        /// Indicates if the property has a defined RDF URI.
        /// </summary>
        public readonly bool HasUri;

        /// <summary>
        /// The RDF URI of the property.
        /// </summary>
        public readonly string Uri;

        /// <summary>
        /// Flag if the the property is language invariant.
        /// </summary>
        public readonly bool LanguageInvariant;

        private List<Instruction> _initializer;
 
        public List<Instruction> Initializer { get => _initializer; set { _initializer = value; HasDefaultValue = true; } }
        #endregion

        #region Constructors

        /// <summary>
        /// Create a new property task helper.
        /// </summary>
        /// <param name="property">A property definition.</param>
        /// <param name="backingField">A reference to the backing field of the property.</param>
        public PropertyGeneratorTaskHelper(PropertyDefinition property, FieldReference backingField)
        {
            Property = property;
            BackingField = backingField;

            var defaultValue = property.CustomAttributes.FirstOrDefault(a => a.Is(typeof(DefaultValueAttribute)));

            HasDefaultValue = defaultValue != null && defaultValue.HasConstructorArguments;

            if (HasDefaultValue)
            {
                DefaultValue = defaultValue.ConstructorArguments.FirstOrDefault();
            }

            HasUri = !string.IsNullOrEmpty(Uri);
            Uri = property.TryGetAttributeParameter("RdfPropertyAttribute");
            LanguageInvariant = property.TryGetSecondAttributeParameter("RdfPropertyAttribute", false);
        }

        #endregion
    }
}
