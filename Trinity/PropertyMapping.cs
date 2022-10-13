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

using System;
using System.Collections.Generic;
using System.Collections;

namespace Semiodesk.Trinity
{
    /// <summary>
    /// This class does the heavy lifting of the property mapping mechanism. It stores the value and acts as intermediary for the resource.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class PropertyMapping<T> : IPropertyMapping
    {
        #region Members

        /// <summary>
        /// The value of the mapped property.
        /// </summary>
        private T _value;

        /// <summary>
        /// The datatype of the the mapped property.
        /// </summary>
        private readonly Type _dataType;

        /// <summary>
        /// The datatype of the the mapped property.
        /// </summary>
        Type IPropertyMapping.DataType 
        {
            get { return _dataType;  }
        }

        /// <summary>
        /// If the datatype is a collection, this contains the generic type.
        /// </summary>
        private readonly Type _genericType;

        /// <summary>
        /// If the datatype is a collection, this contains the generic type.
        /// </summary>
        Type IPropertyMapping.GenericType
        {
            get { return _genericType; }
        }

        /// <summary>
        /// True if the property is mapped to a collection.
        /// </summary>
        private readonly bool _isList;

        /// <summary>
        /// True if the property is mapped to a collection.
        /// </summary>
        bool IPropertyMapping.IsList
        {
            get { return _isList; }
        }

        /// <summary>
        /// True if the value has not been set.
        /// </summary>
        private bool _isUnsetValue = true;

        /// <summary>
        /// True if the value has not been set.
        /// </summary>
        bool IPropertyMapping.IsUnsetValue
        {
            get
            {
                if (_isList && _value != null)
                {
                    return (_value as IList).Count == 0;
                }
                else
                {
                    return _isUnsetValue;
                }
            }
        }

        /// <summary>
        /// Language of the value.
        /// </summary>
        public string Language { get; set; }

        private Property _property;

        /// <summary>
        /// Gets the mapped RDF property.
        /// </summary>
        Property IPropertyMapping.Property
        {
            get 
            {
                if (_property == null)
                {
                    _property = OntologyDiscovery.GetProperty(PropertyUri);
                }

                return _property;  
            }
        }

        /// <summary>
        /// Gets the URI of the mapped RDF property.
        /// </summary>
        public string PropertyUri { get; private set; }

        /// <summary>
        /// Gets the name of the mapped .NET property.
        /// </summary>
        public string PropertyName { get; private set; }

        /// <summary>
        /// Only valid if type or generic type is string. The mapping ignores the language setting and is always non-localized.
        /// </summary>
        public bool LanguageInvariant { get; private set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Creates a new property mapping.
        /// </summary>
        /// <param name="propertyName">Name of the property in the class</param>
        /// <param name="property">The RDF property that should be mapped</param>
        /// <param name="languageInvariant">This parameter is only valid if the type is string. Tells the mapping that the values should be treated as non-localized literals.</param>
        public PropertyMapping(string propertyName, Property property, bool languageInvariant=false)
        {
            if( string.IsNullOrEmpty(propertyName) )
            {
                throw new ArgumentException("Property name may not be empty in PropertyMapping object.");
            }

            _property = property;

            LanguageInvariant = languageInvariant;

            PropertyName = propertyName;

            _dataType = typeof(T);

            if (_dataType.GetInterface("IList") != null)
            {
                _isList = true;
                _genericType = _dataType.GetGenericArguments()[0];
                _value = (T)Activator.CreateInstance(typeof(T));
            }
            else
            {
                _isList = false;
                _genericType = null;
            }

#if DEBUG

            // Test if the given type is valid
            var allowed = new HashSet<Type>{ typeof(string), 
                                                 typeof(bool),typeof(bool?), 
                                                 typeof(float), typeof(float?), 
                                                 typeof(double), typeof(double?),
                                                 typeof(decimal), typeof(decimal?),
                                                 typeof(Int16), typeof(Int16?),
                                                 typeof(Int32), typeof(Int32?),
                                                 typeof(Int64), typeof(Int64?),
                                                 typeof(UInt16), typeof(UInt16?),
                                                 typeof(UInt32), typeof(UInt32?),
                                                 typeof(UInt64), typeof(UInt64?),
                                                 typeof(DateTime), typeof(DateTime?),
                                                 typeof(TimeSpan), typeof(TimeSpan?),
                                                 typeof(System.Uri), typeof(Tuple<string, string>)};

            if (!allowed.Contains(_dataType) && _dataType.GetInterface("IResource") == null && !typeof(Resource).IsAssignableFrom(_dataType))
            {
                // Test if type is IList interface and INotifyCollectionChanged
                if (_dataType.GetInterface("IList") != null )
                {
                    // Test containing Type
                    if (allowed.Contains(_genericType) || _genericType.GetInterface("IResource") != null || typeof(Resource).IsAssignableFrom(_genericType))
                    {
                        return;
                    }
                }

                throw new Exception(string.Format("The property '{0}' with type {1} mapped on RDF property '<{2}>' is not compatible.", propertyName, _dataType, property));
            }

#endif
        }

        /// <summary>
        /// Creates a new property mapping.
        /// </summary>
        /// <param name="propertyName">Name of the property in the class</param>
        /// <param name="property">The RDF property that should be mapped</param>
        /// <param name="defaultValue">The default value used to initialize this property</param>
        /// <param name="languageInvariant">This parameter is only valid if the type is string. Tells the mapping that the values should be treated as non-localized literals.</param>
        public PropertyMapping(string propertyName, Property property, T defaultValue, bool languageInvariant = false) : this(propertyName, property, languageInvariant)
        {
            SetValue(defaultValue);
        }

        /// <summary>
        /// Creates a new property mapping.
        /// </summary>
        /// <param name="propertyName">Name of the property in the class</param>
        /// <param name="propertyUri">The URI of the RDF property that should be mapped</param>
        /// <param name="languageInvariant">This parameter is only valid if the type is string. Tells the mapping that the values should be treated as non-localized literals.</param>
        public PropertyMapping(string propertyName, string propertyUri, bool languageInvariant = false)
            : this(propertyName, property: null, languageInvariant: languageInvariant)
        {
            PropertyUri = propertyUri;
        }

        /// <summary>
        /// Creates a new property mapping.
        /// </summary>
        /// <param name="propertyName">Name of the property in the class</param>
        /// <param name="propertyUri">The URI of the RDF property that should be mapped</param>
        /// <param name="defaultValue">The default value used to initialize this property</param>
        /// <param name="languageInvariant">This parameter is only valid if the type is string. Tells the mapping that the values should be treated as non-localized literals.</param>
        public PropertyMapping(string propertyName, string propertyUri, T defaultValue, bool languageInvariant = false)
            : this(propertyName, property: null, defaultValue: defaultValue, languageInvariant: languageInvariant)
        {
            PropertyUri = propertyUri;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Sets the property value.
        /// </summary>
        /// <param name="value">A value.</param>
        internal void SetValue(T value)
        {
            _isUnsetValue = false;
            _value = value;
        }

        /// <summary>
        /// Returns the property value.
        /// </summary>
        /// <returns>The value, if any.</returns>
        internal T GetValue()
        {
            return _value;
        }

        /// <summary>
        /// Sets a single literal value or adds a value to a property mapped to a value collection.
        /// </summary>
        /// <remarks>
        /// This method is meant to be called from the non-mapped interface. It replaces the current value if 
        /// it is mapped to one value, adds it if the property is mapped to a list.
        /// </remarks>
        /// <param name="value">The value.</param>
        void IPropertyMapping.SetOrAddMappedValue(object value)
        {
            if (_isList)
            {
                if (_value is IList list)
                {
                    var t = value.GetType();

                    if (t == _genericType || _genericType.IsAssignableFrom(t))
                    {
                        list.Add(value);
                        _isUnsetValue = false;

                        return;
                    }
                    else if (t.IsValueType && ((IPropertyMapping)this).IsTypeCompatible(t))
                    {
                        list.Add(Convert.ChangeType(value, _genericType));
                        _isUnsetValue = false;

                        return;
                    }
                    else if (_genericType == typeof(Uri) && typeof(Resource).IsAssignableFrom(t))
                    {
                        list.Add((value as Resource).Uri);
                        _isUnsetValue = false;

                        return;
                    }
                }
            }
            else
            {
                var t = value.GetType();

                if (t == _dataType || _dataType.IsAssignableFrom(t))
                {
                    _value = (T)value;
                    _isUnsetValue = false;

                    return;
                }
                else if(t.IsValueType && ((IPropertyMapping)this).IsTypeCompatible(t))
                {
                    _value = (T)Convert.ChangeType(value, _dataType);
                    _isUnsetValue = false;

                    return;
                }
                else if (_dataType == typeof(Uri) && typeof(Resource).IsAssignableFrom(t))
                {
                    _value = (T) (object)(value as Resource).Uri;
                    _isUnsetValue = false;

                    return;
                }
            }

            string typeString;

            if (_isList)
            {
                typeString = _genericType.ToString();
            }
            else
            {
                typeString = typeof(T).ToString();
            }

            var message = $"Provided argument value was not of type {typeString}";

            throw new Exception(message);
        }

        /// <summary>
        /// Deletes the containing value and sets the state to unset. In case of a collection, it tries to remove the value from it.
        /// </summary>
        /// <param name="value"></param>
        void IPropertyMapping.RemoveOrResetValue(object value)
        {
            if (_isList)
            {
                if (value.GetType().IsAssignableFrom(_genericType))
                {
                    ((IList)_value).Remove(value);
                    return;
                }
            }
            else
            {
                if (typeof(T).IsAssignableFrom(value.GetType()))
                {
                    _value = default(T);
                    _isUnsetValue = true;
                    return;
                }
            }

            string typeString;

            if (_isList)
            {
                typeString = _genericType.ToString();
            }
            else
            {
                typeString = typeof(T).ToString();
            }

            var message = $"Provided argument value was not of type {typeString}";
            
            throw new Exception(message);
        }

        /// <summary>
        /// Gets the value or values mapped to this property.
        /// </summary>
        /// <returns></returns>
        object IPropertyMapping.GetValueObject()
        {
            if (LanguageInvariant || string.IsNullOrEmpty(Language) && (_dataType != typeof(string) || _genericType != typeof(string)))
            {
                return _value;
            }
            else
            {
                if (_isList)
                {
                    return ToLanguageList();
                }
                else
                {
                    return new Tuple<string, string>(_value as string, Language);
                }
            }
        }

        /// <summary>
        /// Gets a list of strings as list of tuples containing the values and the language tags.
        /// </summary>
        /// <returns></returns>
        IList ToLanguageList()
        {
            var result = new List<Tuple<string, string>>();

            foreach (var v in _value as IList<string>)
            {
                result.Add(new Tuple<string, string>(v, Language));
            }

            return result;
        }

        /// <summary>
        /// Method to test if a type is compatible. In case of collection, the containing type is tested for compatibility.
        /// </summary>
        /// <param name="type">The type to test.</param>
        /// <returns>True if the type is compatible</returns>
        bool IPropertyMapping.IsTypeCompatible(Type type)
        {
            var mappingType = _dataType;

            if (_isList)
            {
                mappingType = _genericType;
            }

            if( IsNumericType(type) )
            {
                return IsPrecisionCompatible(type, mappingType);
            }
            else
            {
                return (mappingType.IsAssignableFrom(type) || typeof(Resource).IsAssignableFrom(mappingType) && typeof(Resource).IsAssignableFrom(type) || (typeof(Uri).IsAssignableFrom(mappingType) && typeof(Resource).IsAssignableFrom(type)) );
            }
        }

        /// <summary>
        /// Indicates if the mapped value is a numeric type.
        /// </summary>
        /// <param name="type">A .NET type object.</param>
        /// <returns><c>true</c> if the type is numeric, <c>false</c> otherwise.</returns>
        public static bool IsNumericType(Type type)
        {
            switch (Type.GetTypeCode(type))
            {
                case TypeCode.Byte:
                case TypeCode.SByte:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                case TypeCode.UInt64:
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                case TypeCode.Decimal:
                case TypeCode.Double:
                case TypeCode.Single:
                    return true;
                default:
                    return false;
            }
        }

        /// <summary>
        /// Indicates if the precision of a numeric target type is greater or equal to a given source type.
        /// </summary>
        /// <param name="source">The source type.</param>
        /// <param name="target">The target type.</param>
        /// <returns><c>true</c> if the types are precision compatible, <c>false</c> otherwise.</returns>
        public bool IsPrecisionCompatible(Type source, Type target)
        {
            if (target == typeof(Double))
            {
                return true;
            }
            
            if (target == typeof(Single))
            {
                if (source == typeof(Double))
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }

            if (target == typeof(Decimal))
            {
                if (source == typeof(Double) || source == typeof(Single))
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }

            return true;   
        }

        /// <summary>
        /// Clones the mapping of another resource.
        /// </summary>
        /// <param name="other"></param>
        void IPropertyMapping.CloneFrom(IPropertyMapping other)
        {
            if (_dataType != other.DataType)
            {
                return;
            }

            if (_value != null && _isList)
            {
                var collection = (IList)_value;

                collection.Clear();

                var otherCollection = (IList) other.GetValueObject();

                foreach (var v in otherCollection)
                {
                    collection.Add(v);
                }

                _isUnsetValue = other.IsUnsetValue;
            }
            else
            {
                _value = (T)other.GetValueObject();
                _isUnsetValue = other.IsUnsetValue;
            }
        }

        /// <summary>
        /// Clears the mapping and resets it.
        /// </summary>
        void IPropertyMapping.Clear()
        {
            if (_isList)
            {
                (_value as IList).Clear();
            }
            else
            {
                _value = default(T);
            }

            _isUnsetValue = true;
        }

        #endregion
    }
}
