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
using System.Linq;
using System.Xml;
using System.Globalization;
#if NET35
using Semiodesk.Trinity.Utility;
#endif
using VDS.RDF;

namespace Semiodesk.Trinity
{
    /// <summary>
    /// Provides functionality for the serialization and deserialization of .NET 
    /// objects to XML Schema encoded strings.
    /// </summary>
    public class XsdTypeMapper
    {
        #region Fields

        /// <summary>
        /// XSD URI vocabulary.
        /// </summary>
        private struct xsd
        {
            //static string Prefix = "xsd";
            public static string _namespace = "http://www.w3.org/2001/XMLSchema#";
            public static Uri ns = new Uri("http://www.w3.org/2001/XMLSchema");
            public static Uri datetime = new Uri(ns, "#dateTime");
            public static Uri date = new Uri(ns, "#date");
            public static Uri duration = new Uri(ns, "#duration");
            public static Uri base64Binary = new Uri(ns, "#base64Binary");
            public static Uri boolean_ = new Uri(ns, "#boolean_");
            public static Uri boolean = new Uri(ns, "#boolean");
            public static Uri _byte = new Uri(ns, "#unsignedByte");
            public static Uri _sbyte = new Uri(ns, "#byte");
            public static Uri _double = new Uri(ns, "#double");
            public static Uri _float = new Uri(ns, "#float");
            public static Uri _short = new Uri(ns, "#short");
            public static Uri _int = new Uri(ns, "#int");
            public static Uri integer = new Uri(ns, "#integer");
            public static Uri _long = new Uri(ns, "#long");
            public static Uri _ushort = new Uri(ns, "#unsignedShort");
            public static Uri _uint = new Uri(ns, "#unsignedInt");
            public static Uri _ulong = new Uri(ns, "#unsignedLong");
            public static Uri _decimal = new Uri(ns, "#decimal");
            public static Uri nonNegativeInteger = new Uri(ns, "#nonNegativeInteger");
            public static Uri anyUri = new Uri(ns, "#anyURI");
        }

        /// <summary>
        /// Maps .NET types to XSD type URIs.
        /// </summary>
        protected static Dictionary<Type, Uri> NativeToXsd = new Dictionary<Type, Uri>()
        {
            {typeof(Byte), xsd._byte},
            {typeof(SByte), xsd._sbyte},
            {typeof(Int16), xsd._short},
            {typeof(Int32), xsd._int},
            {typeof(Int64), xsd._long},
            {typeof(UInt16), xsd._ushort},
            {typeof(UInt32), xsd._uint},
            {typeof(UInt64), xsd._ulong},
            {typeof(DateTime), xsd.datetime},
            {typeof(TimeSpan), xsd.duration},
            {typeof(Byte[]), xsd.base64Binary},
            {typeof(Boolean), xsd.boolean},
            {typeof(Decimal), xsd._decimal},
            {typeof(Double), xsd._double},
            {typeof(float), xsd._float},
            {typeof(Uri), xsd.anyUri},
        };

        /// <summary>
        /// Maps XSD type URIs to .NET types.
        /// </summary>
        protected static Dictionary<string, Type> XsdToNative = new Dictionary<string, Type>()
        {
            
            {xsd.nonNegativeInteger.AbsoluteUri, typeof(UInt64)},
            {xsd._byte.AbsoluteUri, typeof(Byte)},
            {xsd._sbyte.AbsoluteUri, typeof(SByte)},
            {xsd._short.AbsoluteUri, typeof(Int16)},
            {xsd._int.AbsoluteUri, typeof(Int32)},
            {xsd._long.AbsoluteUri, typeof(Int64)},
            {xsd._ushort.AbsoluteUri, typeof(UInt16)},
            {xsd._uint.AbsoluteUri, typeof(UInt32)},
            {xsd._ulong.AbsoluteUri, typeof(UInt64)},
            {xsd.datetime.AbsoluteUri,typeof(DateTime) },
            {xsd.duration.AbsoluteUri,typeof(TimeSpan) },
            {xsd.boolean.AbsoluteUri, typeof(Boolean)},
            {xsd.boolean_.AbsoluteUri, typeof(Boolean)},
            {xsd._decimal.AbsoluteUri, typeof(Decimal)},
            {xsd._double.AbsoluteUri, typeof(Double)},
            {xsd._float.AbsoluteUri, typeof(float)},
            {xsd.base64Binary.AbsoluteUri, typeof(byte[])},
            {xsd.anyUri.AbsoluteUri, typeof(Uri)},
        };

        /// <summary>
        /// Maps .NET types to object serialization delegates.
        /// </summary>
        protected static Dictionary<Type, ObjectSerializationDelegate> Serializers = new Dictionary<Type, ObjectSerializationDelegate>()
        {
            {typeof(Byte), SerializeByte},
            {typeof(SByte), SerializeSByte},
            {typeof(Int16), SerializeInt16},
            {typeof(Int32), SerializeInt32},
            {typeof(Int64), SerializeInt64},
            {typeof(UInt16), SerializeUInt16},
            {typeof(UInt32), SerializeUInt32},
            {typeof(UInt64), SerializeUInt64},
            {typeof(DateTime), SerializeDateTime},
            {typeof(TimeSpan), SerializeTimeSpan},
            {typeof(Boolean), SerializeBool},
            {typeof(Decimal), SerializeDecimal},
            {typeof(Double), SerializeDouble},
            {typeof(float), SerializeSingle},
            {typeof(IResource), SerializeIResource},
            {typeof(IModel), SerializeIResource},
            {typeof(string), SerializeString},
            {typeof(string[]), SerializeStringArray},
            {typeof(Tuple<string, CultureInfo>), SerializeStringCultureInfoTuple},
            {typeof(Uri), SerializeUri},
            {typeof(Byte[]), SerializeByteArray},
        };

        /// <summary>
        /// Maps XSD type URIs to object deserialization delegates.
        /// </summary>
        static Dictionary<string, ObjectDeserializationDelegate> Deserializers = new Dictionary<string, ObjectDeserializationDelegate>()
        {
            {xsd._byte.AbsoluteUri, DeserializeByte},
            {xsd._sbyte.AbsoluteUri, DeserializeSByte},
            {xsd._short.AbsoluteUri, DeserializeInt16},
            {xsd._int.AbsoluteUri, DeserializeInt32},
            {xsd._long.AbsoluteUri, DeserializeInt64},
            {xsd._ushort.AbsoluteUri, DeserializeUInt16},
            {xsd._uint.AbsoluteUri, DeserializeUInt32},
            {xsd._ulong.AbsoluteUri, DeserializeUInt64},
            {xsd.nonNegativeInteger.AbsoluteUri, DeserializeUInt64},
            {xsd.integer.AbsoluteUri, DeserializeInt32},
            {xsd.datetime.AbsoluteUri, DeserializeDateTime},
            {xsd.date.AbsoluteUri, DeserializeDateTime},
            {xsd.duration.AbsoluteUri, DeserializeTimeSpan},
            {xsd.boolean.AbsoluteUri, DeserializeBool},
            {xsd.boolean_.AbsoluteUri, DeserializeBool},
            {xsd._decimal.AbsoluteUri, DeserializeDecimal},
            {xsd._double.AbsoluteUri, DeserializeDouble},
            {xsd._float.AbsoluteUri, DeserializeSingle},
            {"http://www.w3.org/1999/02/22-rdf-syntax-ns#resource", DeserializeResource},
            {xsd.base64Binary.AbsoluteUri, DeserializeByteArray},
            {xsd.anyUri.AbsoluteUri, DeserializeUri},
        };

        #endregion

        #region Methods

        /// <summary>
        /// Provides the XML Schema type URI for a given .NET type.
        /// </summary>
        /// <param name="type">A .NET type object.</param>
        /// <returns>A XML Schema type URI.</returns>
        public static Uri GetXsdTypeUri(Type type)
        {
            return NativeToXsd[type];
        }

        /// <summary>
        /// Indicates if there is a registered XML Schema type URI for the given .NET type.
        /// </summary>
        /// <param name="type">A .NET type object.</param>
        /// <returns><c>true</c> if there is a XML schema type, <c>false</c> otherwise.</returns>
        public static bool HasXsdTypeUri(Type type)
        {
            return NativeToXsd.ContainsKey(type);
        }

        /// <summary>
        /// Provides the XML Schema type URI for a given .NET type.
        /// </summary>
        /// <param name="uri">A xsd type uri.</param>
        /// <returns>A XML Schema type URI.</returns>
        public static Type GetTypeFromXsd(Uri uri)
        {
            return XsdToNative[uri.AbsoluteUri];
        }

        #endregion

        #region Serialization

        /// <summary>
        /// The object serialization delegate
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public delegate string ObjectSerializationDelegate(object obj);

        /// <summary>
        /// Serializes an object to an XML Schema encoded string.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static string SerializeObject(object obj)
        {
            var type = obj.GetType();

            if (Serializers.ContainsKey(type))
            {
                return Serializers[type](obj);
            }
            else if (type.GetInterface("IResource") != null)
            {
                return SerializeObject(obj, typeof(IResource));
            }
            else
            {
                var msg = $"No serializer available be for object of type {obj.GetType()}.";
                throw new ArgumentException(msg);
            }
        }

        /// <summary>
        /// Serializes an object force to a given type to an XML Schema encoded string.
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static string SerializeObject(object obj, Type type)
        {
            return Serializers[type](obj);
        }

        /// <summary>
        /// Serialize an IResource
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static string SerializeIResource(object obj)
        {

            if (obj is IResource resource)
            {
                // The .NET Uri class makes the host lower case, this is a problem for OpenLink Virtuoso
                return resource.Uri.OriginalString;
            }
            else
            {
                throw new ArgumentException("Argument 1 must be of type Semiodesk.Trinity.IResource");
            }
        }

        /// <summary>
        /// Serialize an Uri
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static string SerializeUri(object obj)
        {
            var uri = obj as Uri;

            if (uri != null)
            {
                return uri.OriginalString;
            }
            else
            {
                throw new ArgumentException("Argument 1 must be of type System.Uri");
            }
        }

        /// <summary>
        /// Serialize a string
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static string SerializeString(object obj)
        {
            return "\"" + obj + "\"";
        }

        /// <summary>
        /// Serialize an array of strings
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static string SerializeStringArray(object obj)
        {

            if (obj is string[] array)
            {
                return array.First();
            }
            else
            {
                throw new ArgumentException("Argument 1 must be of type string[]");
            }
        }

        /// <summary>
        /// Serialize a tuple consisting of a string and its associated culture
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static string SerializeStringCultureInfoTuple(object obj)
        {

            if (obj is Tuple<string, CultureInfo> tuple)
            {
                return tuple.Item1;
            }
            else
            {
                throw new ArgumentException("Argument 1 must be of type System.Tuple<string, System.Globalization.CultureInfo>");
            }
        }

        /// <summary>
        /// Serialize a DateTime
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static string SerializeDateTime(object obj)
        {
            return XmlConvert.ToString((DateTime)obj, XmlDateTimeSerializationMode.Utc);
        }

        /// <summary>
        /// Serialize a TimeSpan
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static string SerializeTimeSpan(object obj)
        {
            return XmlConvert.ToString((TimeSpan)obj);
        }


        /// <summary>
        /// Serialize a byte array
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static string SerializeByteArray(object obj)
        {

            if (obj is byte[] array)
            {
                return Convert.ToBase64String(array);
            }
            else
            {
                throw new ArgumentException("Argument 1 must be of type byte[]");
            }
        }

        /// <summary>
        /// Serialize a bool
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static string SerializeBool(object obj)
        {
            return XmlConvert.ToString((bool)obj);
        }

        /// <summary>
        /// Serialize an Byte
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static string SerializeByte(object obj)
        {
            return XmlConvert.ToString((byte)obj);
        }
        
        /// <summary>
        /// Serialize an SByte
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static string SerializeSByte(object obj)
        {
            return XmlConvert.ToString((sbyte)obj);
        }


        /// <summary>
        /// Serialize an Int16
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static string SerializeInt16(object obj)
        {
            return XmlConvert.ToString((Int16)obj);
        }

        /// <summary>
        /// Serialize an Int32
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static string SerializeInt32(object obj)
        {
            return XmlConvert.ToString((Int32)obj);
        }

        /// <summary>
        /// Serialize an Int64
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static string SerializeInt64(object obj)
        {
            return XmlConvert.ToString((Int64)obj);
        }

        /// <summary>
        /// Serialize an Uint16
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static string SerializeUInt16(object obj)
        {
            return XmlConvert.ToString((UInt16)obj);
        }

        /// <summary>
        /// Serialize an Uint32
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static string SerializeUInt32(object obj)
        {
            return XmlConvert.ToString((UInt32)obj);
        }

        /// <summary>
        /// Serialize an Uint64
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static string SerializeUInt64(object obj)
        {
            return XmlConvert.ToString((UInt64)obj);
        }

        /// <summary>
        /// Serialize a decimal
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static string SerializeDecimal(object obj)
        {
            return XmlConvert.ToString((Decimal)obj);
        }

        /// <summary>
        /// Serialize a double
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static string SerializeDouble(object obj)
        {
            return XmlConvert.ToString((double)obj);
        }

        /// <summary>
        /// Serialize a float
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static string SerializeSingle(object obj)
        {
            return XmlConvert.ToString((float)obj);
        }

        #endregion

        #region Deserialization

        /// <summary>
        /// Deserialization delegate, format for deserialization functions. 
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public delegate object ObjectDeserializationDelegate(string str);

        /// <summary>
        /// Deserialize string, nothing to do.
        /// </summary>
        /// <param name="str">The string</param>
        /// <returns>The string</returns>
        public static object DeserializeString(string str)
        {
            return str;
        }

        /// <summary>
        /// Deserialize string with given type uri.
        /// </summary>
        /// <param name="str">The value as string.</param>
        /// <param name="typeUri">The xsd type.</param>
        /// <returns>The value in its correct type.</returns>
        public static object DeserializeString(string str, Uri typeUri)
        {
            return Deserializers.ContainsKey(typeUri.AbsoluteUri) ? Deserializers[typeUri.AbsoluteUri](str) : str;
        }

        /// <summary>
        /// Deserialize an byte from a string.
        /// </summary>
        /// <param name="str">The serialized byte</param>
        /// <returns>A byte</returns>
        public static object DeserializeByte(string str)
        {
            return XmlConvert.ToByte(str);
        }
        
        /// <summary>
        /// Deserialize an sbyte from a string.
        /// </summary>
        /// <param name="str">The serialized sbyte</param>
        /// <returns>A sbyte</returns>
        public static object DeserializeSByte(string str)
        {
            return XmlConvert.ToSByte(str);
        }
        

        /// <summary>
        /// Deserialize an int16 from a string.
        /// </summary>
        /// <param name="str">The serialized int16</param>
        /// <returns>An int16</returns>
        public static object DeserializeInt16(string str)
        {
            return XmlConvert.ToInt16(str);
        }

        /// <summary>
        /// Deserialize an int32 from a string.
        /// </summary>
        /// <param name="str">The serialized int32</param>
        /// <returns>a int32 value</returns>
        public static object DeserializeInt32(string str)
        {
            return XmlConvert.ToInt32(str);
        }

        /// <summary>
        /// Deserialize an int64 from a string.
        /// </summary>
        /// <param name="str">The serialized int64</param>
        /// <returns>A int64 value</returns>
        public static object DeserializeInt64(string str)
        {
            return XmlConvert.ToInt64(str);
        }

        /// <summary>
        /// Deserialize an uint6 from a string.
        /// </summary>
        /// <param name="str">The serialized int64</param>
        /// <returns>A uint16 value</returns>
        public static object DeserializeUInt16(string str)
        {
            return XmlConvert.ToUInt16(str);
        }

        /// <summary>
        /// Deserialize an int32 from a string.
        /// </summary>
        /// <param name="str">The serialized int32</param>
        /// <returns>A int32 value</returns>
        public static object DeserializeUInt32(string str)
        {
            return XmlConvert.ToUInt32(str);
        }

        /// <summary>
        /// Deserialize an uint64 from a string.
        /// </summary>
        /// <param name="str">The serialized uint64</param>
        /// <returns>A uint64 value</returns>
        public static object DeserializeUInt64(string str)
        {
            return XmlConvert.ToUInt64(str);
        }

        /// <summary>
        /// Deserialize a bool from a string.
        /// </summary>
        /// <param name="str">The serialized bool</param>
        /// <returns>A bool value</returns>
        public static object DeserializeBool(string str)
        {
            return XmlConvert.ToBoolean(str);
        }

        /// <summary>
        /// Deserialize a decimal from a string.
        /// </summary>
        /// <param name="str">The serialized decimal</param>
        /// <returns>A decimal value</returns>
        public static object DeserializeDecimal(string str)
        {
            return XmlConvert.ToDecimal(str);
        }

        /// <summary>
        /// Deserialize a double from a string.
        /// </summary>
        /// <param name="str">The serialized double</param>
        /// <returns>A double value</returns>
        public static object DeserializeDouble(string str)
        {
            return XmlConvert.ToDouble(str);
        }

        /// <summary>
        /// Deserialize a single from a string.
        /// </summary>
        /// <param name="str">The serialized single</param>
        /// <returns>A single value</returns>
        public static object DeserializeSingle(string str)
        {
            return XmlConvert.ToSingle(str);
        }

        /// <summary>
        /// Deserialize a DateTime from a string.
        /// </summary>
        /// <param name="str">The serialized DateTime</param>
        /// <returns>A DateTime value</returns>
        public static object DeserializeDateTime(string str)
        {
            return XmlConvert.ToDateTime(str, XmlDateTimeSerializationMode.Utc);
        }



        /// <summary>
        /// Deserialize a TimeSpan from a string.
        /// </summary>
        /// <param name="str">The serialized TimeSpan</param>
        /// <returns>A TimeSpan value</returns>
        public static object DeserializeTimeSpan(string str)
        {
            return XmlConvert.ToTimeSpan(str);
        }

        /// <summary>
        /// Deserialize a Resource from a string.
        /// </summary>
        /// <param name="str">The serialized Resource</param>
        /// <returns>A Resource value</returns>
        public static object DeserializeResource(string str)
        {
            return new Resource(new Uri(str));
        }

        /// <summary>
        /// Deserialize a uri from a string.
        /// </summary>
        /// <param name="str">The serialized uri</param>
        /// <returns>A uri value</returns>
        public static object DeserializeUri(string str)
        {
            return new Uri(str);
        }

        /// <summary>
        /// Deserialize a ByteArray from a string.
        /// </summary>
        /// <param name="str">The serialized ByteArray</param>
        /// <returns>A ByteArray value</returns>
        public static object DeserializeByteArray(string str)
        {
            return Convert.FromBase64String(str);
        }

        /// <summary>
        /// Deserialize a XmlNode from a string.
        /// </summary>
        /// <param name="node">The serialized XmlNode</param>
        /// <returns>A XmlNode value</returns>
        public static object DeserializeXmlNode(XmlNode node)
        {
            object result = node.InnerText;

            var resource = node.Attributes["rdf:resource"];
            var dataType = node.Attributes["rdf:datatype"];
            var lang = node.Attributes["xml:lang"];

            if (dataType != null)
            {
                try
                {

                    var key = dataType.Value;
                    result = Deserializers[key](node.InnerText);
                }
                catch
                {
                    var msg = $"No converter found for following type: {dataType.Value}";
                    throw new ArgumentException(msg);
                }
            }
            else if (resource != null)
            {
                try
                {
                    result = new Resource(new Uri(resource.Value));
                }
                catch
                {
                    result = resource.Value;
                }
            }
            else if (lang != null)
            {
                return new string[] { result.ToString(), lang.Value };
            }

            return result;
        }

        /// <summary>
        /// Deserialize a LiteralNode from a string.
        /// </summary>
        /// <param name="node">The serialized LiteralNode</param>
        /// <returns>A LiteralNode value</returns>
        public static object DeserializeLiteralNode(BaseLiteralNode node)
        {
            if (node.DataType != null)
            {
                try
                {
                    return Deserializers[node.DataType.OriginalString](node.Value);
                }catch
                {
                    return node.Value;
                }
            }
            return node.Value;
        }

        #endregion
    }
}
