// LICENSE:
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
using System.Globalization;
using System.Linq;
using NUnit.Framework;

// Notizen:
// - Mapping von resource listen sollten nur einen Itemsprovider zurückliefern, welcher auch virtualisiert genutzt werden kann
// - Probleme bestehen im Augenblick:
//   - Überschreiben von gemappten listen
//   - Hinzufügen von un-commiteten objecten darf nicht funktionieren
//   - Beim ändern von gemappten resource listen, wird der komplette Content abgefragt, obwohl vllt. nur ein element hinzugefügt werden muss.

namespace Semiodesk.Trinity.Test
{
    public class TestOntology
    {
        public static readonly Uri Namespace = new Uri("semio:test");
        public Uri GetNamespace() { return Namespace; }
        public static readonly string Prefix = "test";
        public string GetPrefix() { return Prefix; }

        public static readonly Class SingleMappingTestClass = new Class(new Uri(SingleMappingTestClassString));
        public const string SingleMappingTestClassString = "semio:test:SingleMappingTestClass";
        public static readonly Class SingleResourceMappingTestClass = new Class(new Uri("semio:test:SingleResourceMappingTestClass"));
        public static readonly Class ResourceMappingTestClass = new Class(new Uri("semio:test:ResourceMappingTestClass"));

        public const string SubMappingTestClassString = "semio:test:SubMappingTestClass";
        public static readonly Class SubMappingTestClass = new Class(new Uri(SubMappingTestClassString));

        public const string TestClassString = "semio:test:TestClass";
        public static readonly Class TestClass = new Class(new Uri(TestClassString));
        public static readonly Class TestClass2 = new Class(new Uri("semio:test:TestClass2"));
        public static readonly Class TestClass3 = new Class(new Uri("semio:test:TestClass3"));
        public static readonly Class TestClass4 = new Class(new Uri("semio:test:TestClass4"));

        public const string genericTestString = "semio:test:genericTest";
        public static readonly Property genericTest = new Property(new Uri(genericTestString));

        public const string intTestString = "semio:test:intTest";
        public static readonly Property intTest = new Property(new Uri(intTestString));
        public const string uniqueIntTestString = "semio:test:uniqueIntTest";
        public static readonly Property uniqueIntTest = new Property(new Uri(uniqueIntTestString));

        public const string uintTestString ="semio:test:uintTest";
        public static readonly Property uintTest = new Property(new Uri(uintTestString));
        public const string uniqueUintTestString = "semio:test:uniqueUintTest";
        public static readonly Property uniqueUintTest = new Property(new Uri(uniqueUintTestString));

        public const string stringTestString = "semio:test:stringTest";
        public static readonly Property stringTest = new Property(new Uri(stringTestString));

        public const  string uniqueStringTestString = "semio:test:uniqueStringTest";
        public static readonly Property uniqueStringTest = new Property(new Uri(uniqueStringTestString));

        public const string localizedStringTestString = "semio:test:localizedStringTest";
        public static readonly Property localizedStringTest = new Property(new Uri(localizedStringTestString));

        public const string uniqueLocalizedStringTestString = "semio:test:uniqueLocalizedStringTest";
        public static readonly Property uniqueLocalizedStringTest = new Property(new Uri(uniqueLocalizedStringTestString));

        public const string localizedStringCultureTestString = "semio:test:localizedStringCultureTest";
        public static readonly Property localizedStringCultureTest = new Property(new Uri(localizedStringCultureTestString));

        public const string uniqueLocalizedStringCultureTestString = "semio:test:uniqueLocalizedStringCultureTest";
        public static readonly Property uniqueLocalizedStringCultureTest = new Property(new Uri(uniqueLocalizedStringCultureTestString));

        public static readonly Property floatTest = new Property(new Uri("semio:test:floatTest"));
        public static readonly Property uniqueFloatTest = new Property(new Uri("semio:test:uniqueFloatTest"));

        public static readonly Property doubleTest = new Property(new Uri("semio:test:doubleTest"));
        public static readonly Property uniqueDoubleTest = new Property(new Uri("semio:test:uniqueDoubleTest"));

        public static readonly Property decimalTest = new Property(new Uri("semio:test:decimalTest"));
        public static readonly Property uniqueDecimalTest = new Property(new Uri("semio:test:uniqueDecimalTest"));

        public static readonly Property boolTest = new Property(new Uri("semio:test:boolTest"));
        public static readonly Property uniqueBoolTest = new Property(new Uri("semio:test:uniqueBoolTest"));

        public static readonly Property datetimeTest = new Property(new Uri("semio:test:datetimeTest"));
        public static readonly Property uniqueDatetimeTest = new Property(new Uri("semio:test:uniqueDatetimeTest"));

        public static readonly Property timespanTest = new Property(new Uri("semio:test:timespanTest"));
        public static readonly Property uniqueTimespanTest = new Property(new Uri("semio:test:uniqueTimespanTest"));

        public static readonly Property resourceTest = new Property(new Uri("semio:test:resourceTest"));
        public static readonly Property uniqueResourceTest = new Property(new Uri("semio:test:uniqueResourceTest"));

        public const string resTestString = "semio:test:resTest";
        public static readonly Property resTest = new Property(new Uri(resTestString));

        public static readonly Property uriTest = new Property(new Uri("semio:test:uriTest"));
        public static readonly Property uniqueUriTest = new Property(new Uri("semio:test:uniqueUriTest"));

        public const string JsonTestClassUri = "http://localhost/JsonTestClass";
        public static readonly Class JsonTestClass = new Class(new Uri(JsonTestClassUri));
    }

    /// <summary>
    /// 
    /// </summary>
    [TestFixture]
    public class ResourceTest
    {
        [Test]
        public void Equal()
        {
            var t1 = new Resource(new Uri("http://test.com"));
            var t1a = new Resource(new Uri("http://test.com"));
            var u1 = new Uri("http://test.com");
            var t2 = new Resource(new Uri("http://test.com#frag"));
            var u2 = new Uri("http://test.com#frag");
            var t3 = new Resource(new Uri("http://test.com#frag2"));
            var u3 = new Uri("http://test.com#frag2");

            Assert.AreEqual(t1, t1a);
            Assert.AreNotEqual(t1, t2);
            //Assert.AreNotEqual(u1, u2);
            Assert.AreNotEqual(t1, t3);
            //Assert.AreNotEqual(u1, u3);
            Assert.AreNotEqual(t2, t3);
            //Assert.AreNotEqual(u2, u3);
        }

        [Test]
        public void Property()
        {
            var myProperty = new Property(new Uri("ex:myProperty"));
            var myPropertyCopy = new Property(new Uri("ex:myProperty"));
            var t1 = new Resource(new Uri("ex:myResource"));
            var t2 = new Resource(new Uri("ex:mySecondResource"));
            var sValue = "test";
            var iValue = 123;
            var iNegValue = -123;
            var fValue = (float)2.0234;
            var fNegValue = (float)-2.123;
            var dValue = 3.123;
            var dNegValue = -4.5234;
            var dtValue = new DateTime(2010, 1, 1);
            var bValue = true;
            var uriValue = new Uri("ex:myUri");

            Assert.IsFalse(t1.HasProperty(myProperty));
            Assert.IsFalse(t1.HasProperty(myProperty, sValue));
            try
            {
                t1.AddProperty(myProperty, sValue);
            }
            catch
            {
                Assert.Fail("Exception was raised during adding of property.");
            }
            Assert.IsTrue(t1.HasProperty(myProperty));
            Assert.IsTrue(t1.HasProperty(myProperty, sValue));

            Assert.AreEqual(sValue, t1.ListValues(myProperty).First());
            Assert.AreEqual(t1.ListValues(myProperty).First(), t1.ListValues(myPropertyCopy).First());

            Assert.IsFalse(t1.HasProperty(myProperty, iValue));
            try
            {
                t1.AddProperty(myProperty, iValue);
            }
            catch
            {
                Assert.Fail("Exception was raised during adding of property.");
            }
            Assert.IsTrue(t1.HasProperty(myProperty));
            Assert.IsTrue(t1.HasProperty(myProperty, iValue));
            
            Assert.IsTrue(t1.ListValues(myProperty).Contains(iValue));

            Assert.IsFalse(t1.HasProperty(myProperty, t2));
            t1.AddProperty(myProperty, t2);
            Assert.IsTrue(t1.HasProperty(myProperty, t2));

            Assert.IsFalse(t1.HasProperty(myProperty, iNegValue));
            t1.AddProperty(myProperty, iNegValue);
            Assert.IsTrue(t1.HasProperty(myProperty, iNegValue));

            Assert.IsFalse(t1.HasProperty(myProperty, fValue));
            t1.AddProperty(myProperty, fValue);
            Assert.IsTrue(t1.HasProperty(myProperty, fValue));

            Assert.IsFalse(t1.HasProperty(myProperty, fNegValue));
            t1.AddProperty(myProperty, fNegValue);
            Assert.IsTrue(t1.HasProperty(myProperty, fNegValue));

            Assert.IsFalse(t1.HasProperty(myProperty, dValue));
            t1.AddProperty(myProperty, dValue);
            Assert.IsTrue(t1.HasProperty(myProperty, dValue));

            Assert.IsFalse(t1.HasProperty(myProperty, dNegValue));
            t1.AddProperty(myProperty, dNegValue);
            Assert.IsTrue(t1.HasProperty(myProperty, dNegValue));

            Assert.IsFalse(t1.HasProperty(myProperty, dtValue));
            t1.AddProperty(myProperty, dtValue);
            Assert.IsTrue(t1.HasProperty(myProperty, dtValue));

            Assert.IsFalse(t1.HasProperty(myProperty, bValue));
            t1.AddProperty(myProperty, bValue);
            Assert.IsTrue(t1.HasProperty(myProperty, bValue));

            Assert.IsFalse(t1.HasProperty(myProperty, uriValue));
            t1.AddProperty(myProperty, uriValue);
            Assert.IsTrue(t1.HasProperty(myProperty, uriValue));

            t1.RemoveProperty(myProperty, t2);
            Assert.IsTrue(t1.HasProperty(myProperty));
            Assert.IsFalse(t1.HasProperty(myProperty, t2));

            t1.RemoveProperty(myProperty, sValue);
            Assert.IsTrue(t1.HasProperty(myProperty));
            Assert.IsFalse(t1.HasProperty(myProperty, sValue));

            t1.RemoveProperty(myProperty, iValue);
            Assert.IsTrue(t1.HasProperty(myProperty));
            Assert.IsFalse(t1.HasProperty(myProperty, iValue));

            t1.RemoveProperty(myProperty, iNegValue);
            Assert.IsTrue(t1.HasProperty(myProperty));
            Assert.IsFalse(t1.HasProperty(myProperty, iNegValue));

            t1.RemoveProperty(myProperty, fValue);
            Assert.IsTrue(t1.HasProperty(myProperty));
            Assert.IsFalse(t1.HasProperty(myProperty, fValue));

            t1.RemoveProperty(myProperty, fNegValue);
            Assert.IsTrue(t1.HasProperty(myProperty));
            Assert.IsFalse(t1.HasProperty(myProperty, fNegValue));

            t1.RemoveProperty(myProperty, dValue);
            Assert.IsTrue(t1.HasProperty(myProperty));
            Assert.IsFalse(t1.HasProperty(myProperty, dValue));

            t1.RemoveProperty(myProperty, dNegValue);
            Assert.IsTrue(t1.HasProperty(myProperty));
            Assert.IsFalse(t1.HasProperty(myProperty, dNegValue));

            t1.RemoveProperty(myProperty, dtValue);
            Assert.IsTrue(t1.HasProperty(myProperty));
            Assert.IsFalse(t1.HasProperty(myProperty, dtValue));

            t1.RemoveProperty(myProperty, bValue);
            Assert.IsTrue(t1.HasProperty(myProperty));
            Assert.IsFalse(t1.HasProperty(myProperty, bValue));

            t1.RemoveProperty(myProperty, uriValue);
            Assert.IsFalse(t1.HasProperty(myProperty, uriValue));

            Assert.IsFalse(t1.HasProperty(myProperty));
        }


        #region Datatype fidelity Test

        [Test]
        public void TestBool()
        {
            var myProperty = new Property(new Uri("ex:myProperty"));
            var r1 = new Resource(new Uri("ex:myResource"));

            var val = true;
            r1.AddProperty(myProperty, val);

            var res = r1.ListValues(myProperty).First();

            Assert.AreEqual(res.GetType(), typeof(bool));
            Assert.AreEqual((bool)res, val);
        }

        [Test]
        public void TestInt()
        {
            var myProperty = new Property(new Uri("ex:myProperty"));
            var r1 = new Resource(new Uri("ex:myResource"));


            var val1 = 123;
            r1.AddProperty(myProperty, val1);
            var res1 = r1.ListValues(myProperty).First();
            Assert.AreEqual(val1.GetType(), res1.GetType());
            Assert.AreEqual(res1, val1);
            r1.RemoveProperty(myProperty, val1);
        }

        [Test]
        public void TestInt16()
        {
            var myProperty = new Property(new Uri("ex:myProperty"));
            var r = new Resource(new Uri("ex:myResource"));
            Int16 val = 124;
            r.AddProperty(myProperty, val);
            var res = r.ListValues(myProperty).First();
            Assert.AreEqual(val.GetType(), res.GetType());
            Assert.AreEqual(val, res);
            r.RemoveProperty(myProperty, val);

        }

        [Test]
        public void TestInt32()
        {
            var myProperty = new Property(new Uri("ex:myProperty"));
            var r = new Resource(new Uri("ex:myResource"));
            var val = 125;
            r.AddProperty(myProperty, val);
            var res = r.ListValues(myProperty).First();
            Assert.AreEqual(val.GetType(), res.GetType());
            Assert.AreEqual(val, res);
            r.RemoveProperty(myProperty, val);

        }

        [Test]
        public void TestInt64()
        {
            var myProperty = new Property(new Uri("ex:myProperty"));
            var r = new Resource(new Uri("ex:myResource"));
            Int64 val = 126;
            r.AddProperty(myProperty, val);
            var res = r.ListValues(myProperty).First();
            Assert.AreEqual(val.GetType(), res.GetType());
            Assert.AreEqual(val, res);
            r.RemoveProperty(myProperty, val);
        }

        [Test]
        public void TestUint()
        {
            var myProperty = new Property(new Uri("ex:myProperty"));
            var r = new Resource(new Uri("ex:myResource"));
            uint val = 126;
            r.AddProperty(myProperty, val);
            var res = r.ListValues(myProperty).First();
            Assert.AreEqual(val.GetType(), res.GetType());
            Assert.AreEqual(val, res);
            r.RemoveProperty(myProperty, val);
        }

        [Test]
        public void TestUint16()
        {
            var myProperty = new Property(new Uri("ex:myProperty"));
            var r = new Resource(new Uri("ex:myResource"));
            UInt16 val = 126;
            r.AddProperty(myProperty, val);
            var res = r.ListValues(myProperty).First();
            Assert.AreEqual(val.GetType(), res.GetType());
            Assert.AreEqual(val, res);
            r.RemoveProperty(myProperty, val);
        }

        [Test]
        public void TestUint32()
        {
            var myProperty = new Property(new Uri("ex:myProperty"));
            var r = new Resource(new Uri("ex:myResource"));
            UInt32 val = 126;
            r.AddProperty(myProperty, val);
            var res = r.ListValues(myProperty).First();
            Assert.AreEqual(val.GetType(), res.GetType());
            Assert.AreEqual(val, res);
            r.RemoveProperty(myProperty, val);
        }

        [Test]
        public void TestUint64()
        {
            var myProperty = new Property(new Uri("ex:myProperty"));
            var r = new Resource(new Uri("ex:myResource"));
            UInt32 val = 126;
            r.AddProperty(myProperty, val);
            var res = r.ListValues(myProperty).First();
            Assert.AreEqual(val.GetType(), res.GetType());
            Assert.AreEqual(val, res);
            r.RemoveProperty(myProperty, val);
        }

        [Test]
        public void TestFloat()
        {
            var myProperty = new Property(new Uri("ex:myProperty"));
            var r = new Resource(new Uri("ex:myResource"));
            var val = 1.234F;
            r.AddProperty(myProperty, val);
            var res = r.ListValues(myProperty).First();
            Assert.AreEqual(val.GetType(), res.GetType());
            Assert.AreEqual(val, res);
            r.RemoveProperty(myProperty, val);
        }

        [Test]
        public void TestDouble()
        {
            var myProperty = new Property(new Uri("ex:myProperty"));
            var r = new Resource(new Uri("ex:myResource"));
            var val = 1.223;
            r.AddProperty(myProperty, val);
            var res = r.ListValues(myProperty).First();
            Assert.AreEqual(val.GetType(), res.GetType());
            Assert.AreEqual(val, res);
            r.RemoveProperty(myProperty, val);
        }

        [Test]
        public void TestSingle()
        {
            var myProperty = new Property(new Uri("ex:myProperty"));
            var r = new Resource(new Uri("ex:myResource"));
            var val = 1.223F;
            r.AddProperty(myProperty, val);
            var res = r.ListValues(myProperty).First();
            Assert.AreEqual(val.GetType(), res.GetType());
            Assert.AreEqual(val, res);
            r.RemoveProperty(myProperty, val);
        }

        [Test]
        public void TestString()
        {
            var myProperty = new Property(new Uri("ex:myProperty"));
            var r = new Resource(new Uri("ex:myResource"));
            var val = "Hello World!";
            r.AddProperty(myProperty, val);
            var res = r.ListValues(myProperty).First();
            Assert.AreEqual(val.GetType(), res.GetType());
            Assert.AreEqual(val, res);
            r.RemoveProperty(myProperty, val);
        }


        [Test]
        public void TestLocalizedString()
        {
            var myProperty = new Property(new Uri("ex:myProperty"));
            var r = new Resource(new Uri("ex:myResource"));
            var val = "Hello World!";
            var ci = "en";
            r.AddProperty(myProperty, val, ci);
            var res = r.ListValues(myProperty).First();
            Assert.AreEqual(typeof(Tuple<string, string>), res.GetType());
            var v = res as Tuple<string, string>;
            Assert.AreEqual(val, v.Item1);
            Assert.AreEqual(ci.ToLower(), v.Item2.ToLower());
            r.RemoveProperty(myProperty, val, ci);
           
        }

        [Test]
        public void TestDateTime()
        {
            var myProperty = new Property(new Uri("ex:myProperty"));
            var r = new Resource(new Uri("ex:myResource"));
            var val = DateTime.Today;
            r.AddProperty(myProperty, val);
            var res = r.ListValues(myProperty).First();
            Assert.AreEqual(val.GetType(), res.GetType());
            Assert.AreEqual(val, res);
            r.RemoveProperty(myProperty, val);
        }

        [Test]
        public void TestByteArray()
        {
            var myProperty = new Property(new Uri("ex:myProperty"));
            var r = new Resource(new Uri("ex:myResource"));

            var val = new byte[] { 1, 2, 3, 4, 5 };

            r.AddProperty(myProperty, val);
            var res = r.ListValues(myProperty).First();
            Assert.AreEqual(val.GetType(), res.GetType());
            Assert.AreEqual(val, res);
            r.RemoveProperty(myProperty, val);

        }

        [Test]
        public void TestUri()
        {
            var myProperty = new Property(new Uri("ex:myProperty"));
            var r = new Resource(new Uri("ex:myResource"));
            var val = new Uri("ex:myUri");
            r.AddProperty(myProperty, val);
            var res = r.ListValues(myProperty).First();
            Assert.AreEqual(val.GetType(), res.GetType());
            Assert.AreEqual(val, res);
            r.RemoveProperty(myProperty, val);
        }

        #endregion

        #region List*() Tests

        /// <summary>
        ///Ein Test für "ListValues"
        ///</summary>
        [Test()]
        public void ListValuesTest()
        {
            var baseUri = new Uri("http://example.com/test");
            var relativeUri = "#myResource";

            var target = new Resource(new Uri(baseUri, relativeUri));
            var b = target.Uri.Fragment;

            var property = new Property(new Uri(baseUri, "#related"));
            var list = new List<object>();

            var v1 = 12;
            list.Add(v1);
            target.AddProperty(property, v1);

            var v2 = "Hello World";
            target.AddProperty(property, v2);
            list.Add(v2);

            v2 = "All your base are belong to us!";
            target.AddProperty(property, v2);
            list.Add(v2);

            var v3 = 0.234F;
            target.AddProperty(property, v3);
            list.Add(v3);

            var v4 = new DateTime(1292, 1, 1);
            target.AddProperty(property, v4);
            list.Add(v4);

            var v5 = new Tuple<string, string>("Hallo Welt!", "de");
            target.AddProperty(property, v5.Item1, v5.Item2);
            list.Add(v5);

            IResource v6 = new Resource(new Uri(baseUri, "#mySecondResource"));
            target.AddProperty(property, v6);
            list.Add(v6);

            var v7 = 0.123;
            target.AddProperty(property, v7);
            list.Add(v7);

            var v8 = true;
            target.AddProperty(property, v8);
            list.Add(v8);

            var v9 = new Uri("ex:myUri");
            target.AddProperty(property, v9);
            list.Add(v9);

            IEnumerable<object> expected = list;

            var actual = target.ListValues(property);
            foreach (var obj in actual)
            {
                if (obj.GetType() == typeof(string[]))
                {
                    var tmp = (Tuple<string, string>)obj;
                    Assert.AreEqual(v5, tmp);

                }
                else
                {
                    Assert.AreEqual(true, expected.Contains(obj), string.Format("Object {0} not in expected list.", obj));
                }
            }


        }

        /// <summary>
        ///Ein Test für "ListProperties"
        ///</summary>
        [Test()]
        public void ListPropertiesTest()
        {
            var baseUri = new Uri("http://example.com/test");
            var relativeUri = "#myResource";
            var target = new Resource(new Uri(baseUri, relativeUri));
            var property1 = new Property(new Uri(baseUri, "#related"));
            var v1 = true;
            var property2 = new Property(new Uri(baseUri, "#related2"));
            var v2 = false;
            var property3 = new Property(new Uri(baseUri, "#related3"));
            var v3 = true;
            var expected = new List<Property> { property1, property2, property3 };
            target.AddProperty(property1, v1);
            target.AddProperty(property2, v2);
            target.AddProperty(property3, v3);
            var actual = target.ListProperties();
            foreach (var prop in actual)
            {
                Assert.AreEqual(true, expected.Contains(prop));
            }

            target.RemoveProperty(property1, v1);
            expected.Remove(property1);
            actual = target.ListProperties();
            foreach (var prop in actual)
            {
                Assert.AreEqual(true, expected.Contains(prop));
            }

            target.RemoveProperty(property2, v2);
            expected.Remove(property2);
            actual = target.ListProperties();
            foreach (var prop in actual)
            {
                Assert.AreEqual(true, expected.Contains(prop));
            }


            target.RemoveProperty(property3, v3);
            expected.Remove(property3);
            actual = target.ListProperties();
            Assert.AreEqual(actual.Count(), 0);




        }

        #endregion

        #region HasProperty() Tests

        /// <summary>
        ///Ein Test für "HasProperty"
        ///</summary>
        [Test()]
        public void HasPropertyTest2()
        {
            var baseUri = new Uri("http://example.com/test");
            var relativeUri = "#myResource";
            var target = new Resource(new Uri(baseUri, relativeUri)); // TODO: Passenden Wert initialisieren
            var property = new Property(new Uri(baseUri, "#related"));
            var value1 = "Hallo Welt";
            var lang1 = CultureInfo.GetCultureInfo("DE");
            var value2 = "Hello World";
            var lang2 = CultureInfo.GetCultureInfo("en-US");
            var value3 = "Hello";

            Assert.AreEqual(false, target.HasProperty(property, value1, lang1));
            target.AddProperty(property, value1, lang1);
            // Current interpretation -> Value+Language != Value
            Assert.AreEqual(true, target.HasProperty(property, value1, lang1));
            Assert.AreEqual(false, target.HasProperty(property, value1));
            Assert.AreEqual(false, target.HasProperty(property, value2, lang2));
            Assert.AreEqual(false, target.HasProperty(property, value3));

            target.AddProperty(property, value2, lang2);
            Assert.AreEqual(true, target.HasProperty(property, value1, lang1));
            Assert.AreEqual(true, target.HasProperty(property, value2, lang2));
            Assert.AreEqual(false, target.HasProperty(property, value3));

            target.AddProperty(property, value3);
            Assert.AreEqual(true, target.HasProperty(property, value1, lang1));
            Assert.AreEqual(true, target.HasProperty(property, value2, lang2));
            Assert.AreEqual(true, target.HasProperty(property, value3));


            target.RemoveProperty(property, value3);
            Assert.AreEqual(true, target.HasProperty(property, value1, lang1));
            Assert.AreEqual(true, target.HasProperty(property, value2, lang2));
            Assert.AreEqual(false, target.HasProperty(property, value3));

            target.RemoveProperty(property, value2, lang2);
            Assert.AreEqual(true, target.HasProperty(property, value1, lang1));
            Assert.AreEqual(false, target.HasProperty(property, value2, lang2));
            Assert.AreEqual(false, target.HasProperty(property, value2));
        }


        /// <summary>
        ///Ein Test für "HasProperty"
        ///</summary>
        [Test()]
        public void HasPropertyTest1()
        {
            var baseUri = new Uri("http://example.com/test");
            var relativeUri = "#myResource";
            var target = new Resource(new Uri(baseUri, relativeUri));
            var property = new Property(new Uri(baseUri, "#related"));
            var value1 = 1;
            var value2 = 2;

            Assert.AreEqual(false, target.HasProperty(property, value1));
            target.AddProperty(property, value1);
            Assert.AreEqual(true, target.HasProperty(property, value1));
            Assert.AreEqual(false, target.HasProperty(property, value2));
            target.AddProperty(property, value2);
            Assert.AreEqual(true, target.HasProperty(property, value1));
            Assert.AreEqual(true, target.HasProperty(property, value2));

            target.RemoveProperty(property, value1);
            Assert.AreEqual(false, target.HasProperty(property, value1));
            Assert.AreEqual(true, target.HasProperty(property, value2));
            target.RemoveProperty(property, value2);
            Assert.AreEqual(false, target.HasProperty(property, value1));
            Assert.AreEqual(false, target.HasProperty(property, value2));


        }

        /// <summary>
        ///Ein Test für "HasProperty"
        ///</summary>
        [Test()]
        public void HasPropertyTest()
        {
            var baseUri = new Uri("http://example.com/test");
            var relativeUri = "#myResource";
            var target = new Resource(new Uri(baseUri, relativeUri));
            var property = new Property(new Uri(baseUri, "#related"));
            var value1 = 1;
            var value2 = 2;

            Assert.AreEqual(false, target.HasProperty(property));
            target.AddProperty(property, value1);
            Assert.AreEqual(true, target.HasProperty(property));
            target.AddProperty(property, value2);
            Assert.AreEqual(true, target.HasProperty(property));

            target.RemoveProperty(property, value1);
            target.RemoveProperty(property, value2);

            Assert.AreEqual(false, target.HasProperty(property));

        }

        #endregion

        #region Get*() Tests

        /// <summary>
        ///Ein Test für "GetUri"
        ///</summary>
        [Test()]
        public void GetUriTest()
        {
            var baseUri = new Uri("http://example.com");
            var target = new Resource(new Uri(baseUri, "test"));
            var expected = new Uri("http://example.com/test");
            Uri actual = target.Uri;
            Assert.AreEqual(expected, actual);

            baseUri = new Uri("http://example.com/test");
            target = new Resource(new Uri(baseUri, "#Fragment"));
            expected = new Uri("http://example.com/test#Fragment");
            actual = target.Uri;
            Assert.AreEqual(expected, actual);

        }
        #endregion

        #region AddProperty() Tests

        /// <summary>
        ///Ein Test für "AddProperty"
        ///</summary>
        [Test()]
        public void AddPropertyTest9()
        {
            var baseUri = new Uri("http://example.com/test");
            var relativeUri = "#myResource";
            var target = new Resource(new Uri(baseUri, relativeUri));
            var property = new Property(new Uri(baseUri, "#related"));
            var value = new Uri(baseUri, "ex:test#myUriProp");
            target.AddProperty(property, value);
            Assert.IsTrue(target.HasProperty(property));
            Assert.AreEqual(value, target.ListValues(property).First());
            Assert.AreEqual(value.GetType(), target.ListValues(property).First().GetType());
        }

        /// <summary>
        ///Ein Test für "AddProperty"
        ///</summary>
        [Test()]
        public void AddPropertyTest8()
        {
            var baseUri = new Uri("http://example.com/test");
            var relativeUri = "#myResource";
            var target = new Resource(new Uri(baseUri, relativeUri));
            var property = new Property(new Uri(baseUri, "#related"));
            var value = "Hallo Welt!";
            var language = CultureInfo.GetCultureInfo("DE");
            target.AddProperty(property, value, language);

            Assert.IsTrue(target.HasProperty(property));
            Assert.AreEqual(typeof(Tuple<string, string>), target.ListValues(property).First().GetType());
            var res = (Tuple<string, string>)target.ListValues(property).First();
            Assert.AreEqual(value, res.Item1);
            Assert.AreEqual(language.Name.ToLower(), res.Item2.ToLower());
            Assert.AreEqual(value.GetType(), res.Item1.GetType());
            Assert.AreEqual(typeof(string), res.Item2.GetType());
        }

        /// <summary>
        ///Ein Test für "AddProperty"
        ///</summary>
        [Test()]
        public void AddPropertyTest7()
        {
            var baseUri = new Uri("http://example.com/test");
            var relativeUri = "#myResource";
            var target = new Resource(new Uri(baseUri, relativeUri));
            var property = new Property(new Uri(baseUri, "#related"));
            var value = 17;
            target.AddProperty(property, value);
            Assert.IsTrue(target.HasProperty(property));
            Assert.AreEqual(value, target.ListValues(property).First());
            Assert.AreEqual(value.GetType(), target.ListValues(property).First().GetType());
        }

        /// <summary>
        ///Ein Test für "AddProperty"
        ///</summary>
        [Test()]
        public void AddPropertyTest6()
        {
            var baseUri = new Uri("http://example.com/test");
            var relativeUri = "#myResource";
            var target = new Resource(new Uri(baseUri, relativeUri));
            var property = new Property(new Uri(baseUri, "#related"));
            IResource value = new Resource(new Uri(baseUri, "#mySecondResource"));
            target.AddProperty(property, value);
            Assert.IsTrue(target.HasProperty(property));
            Assert.AreEqual(value, target.ListValues(property).First());
            Assert.AreEqual(value.GetType(), target.ListValues(property).First().GetType());
        }

        /// <summary>
        ///Ein Test für "AddProperty"
        ///</summary>
        [Test()]
        public void AddPropertyTest5()
        {
            var baseUri = new Uri("http://example.com/test");
            var relativeUri = "#myResource";
            var target = new Resource(new Uri(baseUri, relativeUri));
            var property = new Property(new Uri(baseUri, "#related"));
            var value = "All your base are belong to us!";
            target.AddProperty(property, value);
            Assert.IsTrue(target.HasProperty(property));
            Assert.AreEqual(value, target.ListValues(property).First());
            Assert.AreEqual(value.GetType(), target.ListValues(property).First().GetType());
        }

        /// <summary>
        ///Ein Test für "AddProperty"
        ///</summary>
        [Test()]
        public void AddPropertyTest4()
        {
            var baseUri = new Uri("http://example.com/test");
            var relativeUri = "#myResource";
            var target = new Resource(new Uri(baseUri, relativeUri));
            var property = new Property(new Uri(baseUri, "#related"));
            var value = 21.345F;
            target.AddProperty(property, value);
            Assert.IsTrue(target.HasProperty(property));
            Assert.AreEqual(value, target.ListValues(property).First());
            Assert.AreEqual(value.GetType(), target.ListValues(property).First().GetType());
        }

        /// <summary>
        ///Ein Test für "AddProperty"
        ///</summary>
        [Test()]
        public void AddPropertyTest3()
        {
            var baseUri = new Uri("http://example.com/test");
            var relativeUri = "#myResource";
            var target = new Resource(new Uri(baseUri, relativeUri));
            var property = new Property(new Uri(baseUri, "#related"));
            var value = new DateTime(2010, 3, 17);
            target.AddProperty(property, value);
            Assert.IsTrue(target.HasProperty(property));
            Assert.AreEqual(value, target.ListValues(property).First());
            Assert.AreEqual(value.GetType(), target.ListValues(property).First().GetType());
        }


        /// <summary>
        ///Ein Test für "AddProperty"
        ///</summary>
        [Test()]
        public void AddPropertyTest1()
        {
            var baseUri = new Uri("http://example.com/test");
            var relativeUri = "#myResource";
            var target = new Resource(new Uri(baseUri, relativeUri));
            var property = new Property(new Uri(baseUri, "#related"));
            double value = 0.234F;
            target.AddProperty(property, value);
            Assert.IsTrue(target.HasProperty(property));
            Assert.AreEqual(value, target.ListValues(property).First());
            Assert.AreEqual(value.GetType(), target.ListValues(property).First().GetType());
        }

        /// <summary>
        ///Ein Test für "AddProperty"
        ///</summary>
        [Test()]
        public void AddPropertyTest()
        {
            var baseUri = new Uri("http://example.com/test");
            var relativeUri = "#myResource";
            var target = new Resource(new Uri(baseUri, relativeUri));
            var property = new Property(new Uri(baseUri, "#related"));
            var value = false;
            target.AddProperty(property, value);
            Assert.IsTrue(target.HasProperty(property));
            Assert.AreEqual(value, target.ListValues(property).First());
            Assert.AreEqual(value.GetType(), target.ListValues(property).First().GetType());
        }

        #endregion

        #region Constructor Tests
        /// <summary>
        ///Ein Test für "Resource-Konstruktor"
        ///</summary>
        [Test()]
        public void ResourceConstructorTest()
        {

            var uri = new Uri("http://example.com/ex");

            var t1 = new Resource(uri);
            var t2 = new Resource(uri);
            var a = t1.Equals(t2);
            Assert.AreEqual(t1, t2);
            Assert.AreEqual(uri, t1.Uri.ToString());

            var ns = new Uri("http://example.com/");
            var res = new Resource(new Uri(ns, "John"));
            Assert.AreEqual(res.Uri, new Uri("http://example.com/John"));


            uri = new Uri("http://example.com/ex#fragment");

            t1 = new Resource(uri);
            t2 = new Resource(uri);
            a = t1.Equals(t2);
            Assert.AreEqual(t1, t2);
            Assert.AreEqual(uri, t1.Uri.ToString());

            //Assert.AreNotEqual(t1.Uri, new Uri("http://example.com/ex"));
            //Assert.AreNotEqual(t1, new Uri("http://example.com/ex"));
            Assert.AreNotEqual(t1, new Resource(new Uri("http://example.com/ex")));



        }
        #endregion

        #region RemoveProperty() Tests

        /// <summary>
        ///Ein Test für "RemoveProperty"
        ///</summary>
        [Test()]
        public void RemovePropertyTest8()
        {
            var baseUri = new Uri("http://example.com/test");
            var relativeUri = "#myResource";
            var target = new Resource(new Uri(baseUri, relativeUri));
            var property = new Property(new Uri(baseUri, "#related"));
            var value = new Uri("ex:test#myUriProp");
            target.AddProperty(property, value);
            Assert.AreEqual(true, target.HasProperty(property, value));
            target.RemoveProperty(property, value);
            Assert.AreEqual(false, target.HasProperty(property, value));
            Assert.AreEqual(false, target.HasProperty(property));
        }
        
        /// <summary>
        ///Ein Test für "RemoveProperty"
        ///</summary>
        [Test()]
        public void RemovePropertyTest7()
        {
            var baseUri = new Uri("http://example.com/test");
            var relativeUri = "#myResource";
            var target = new Resource(new Uri(baseUri, relativeUri));
            var property = new Property(new Uri(baseUri, "#related"));
            var value = "Hello";
            var language = CultureInfo.GetCultureInfo("en-US");
            target.AddProperty(property, value, language);
            Assert.AreEqual(true, target.HasProperty(property, value, language));
            target.RemoveProperty(property, value, language);
            Assert.AreEqual(false, target.HasProperty(property, value, language));
            Assert.AreEqual(false, target.HasProperty(property));
        }

        /// <summary>
        ///Ein Test für "RemoveProperty"
        ///</summary>
        [Test()]
        public void RemovePropertyTest6()
        {
            var baseUri = new Uri("http://example.com/test");
            var relativeUri = "#myResource";
            var target = new Resource(new Uri(baseUri, relativeUri));
            var property = new Property(new Uri(baseUri, "#related"));
            var value = 12;
            target.AddProperty(property, value);
            Assert.AreEqual(true, target.HasProperty(property, value));
            target.RemoveProperty(property, value);
            Assert.AreEqual(false, target.HasProperty(property, value));
            Assert.AreEqual(false, target.HasProperty(property));

        }

        /// <summary>
        ///Ein Test für "RemoveProperty"
        ///</summary>
        [Test()]
        public void RemovePropertyTest5()
        {
            var baseUri = new Uri("http://example.com/test");
            var relativeUri = "#myResource";
            var target = new Resource(new Uri(baseUri, relativeUri));
            var property = new Property(new Uri(baseUri, "#related"));
            var value = "Cheeseburgers are nice.";
            target.AddProperty(property, value);
            Assert.AreEqual(true, target.HasProperty(property, value));
            target.RemoveProperty(property, value);
            Assert.AreEqual(false, target.HasProperty(property, value));
            Assert.AreEqual(false, target.HasProperty(property));

        }

        /// <summary>
        ///Ein Test für "RemoveProperty"
        ///</summary>
        [Test()]
        public void RemovePropertyTest4()
        {
            var baseUri = new Uri("http://example.com/test");
            var relativeUri = "#myResource";
            var target = new Resource(new Uri(baseUri, relativeUri));
            var property = new Property(new Uri(baseUri, "#related"));
            IResource value = new Resource(new Uri(baseUri, "#mySecondResource"));
            target.AddProperty(property, value);
            Assert.AreEqual(true, target.HasProperty(property, value));
            target.RemoveProperty(property, value);
            Assert.AreEqual(false, target.HasProperty(property, value));
            Assert.AreEqual(false, target.HasProperty(property));
        }

        /// <summary>
        ///Ein Test für "RemoveProperty"
        ///</summary>
        [Test()]
        public void RemovePropertyTest3()
        {
            var baseUri = new Uri("http://example.com/test");
            var relativeUri = "#myResource";
            var target = new Resource(new Uri(baseUri, relativeUri));
            var property = new Property(new Uri(baseUri, "#related"));
            var value = false;
            target.AddProperty(property, value);
            Assert.AreEqual(true, target.HasProperty(property, value));
            target.RemoveProperty(property, value);
            Assert.AreEqual(false, target.HasProperty(property, value));
            Assert.AreEqual(false, target.HasProperty(property));
        }

        /// <summary>
        ///Ein Test für "RemoveProperty"
        ///</summary>
        [Test()]
        public void RemovePropertyTest2()
        {
            var baseUri = new Uri("http://example.com/test");
            var relativeUri = "#myResource";
            var target = new Resource(new Uri(baseUri, relativeUri));
            var property = new Property(new Uri(baseUri, "#related"));
            var value = new DateTime(1999, 1, 3);
            target.RemoveProperty(property, value);
            target.AddProperty(property, value);
            Assert.AreEqual(true, target.HasProperty(property, value));
            target.RemoveProperty(property, value);
            Assert.AreEqual(false, target.HasProperty(property, value));
            Assert.AreEqual(false, target.HasProperty(property));
        }

        /// <summary>
        ///Ein Test für "RemoveProperty"
        ///</summary>
        [Test()]
        public void RemovePropertyTest1()
        {
            var baseUri = new Uri("http://example.com/test");
            var relativeUri = "#myResource";
            var target = new Resource(new Uri(baseUri, relativeUri));
            var property = new Property(new Uri(baseUri, "#related"));
            var value = 0.211F;
            target.AddProperty(property, value);
            Assert.AreEqual(true, target.HasProperty(property, value));
            target.RemoveProperty(property, value);
            Assert.AreEqual(false, target.HasProperty(property, value));
            Assert.AreEqual(false, target.HasProperty(property));
        }

        /// <summary>
        ///Ein Test für "RemoveProperty"
        ///</summary>
        [Test()]
        public void RemovePropertyTest()
        {
            var baseUri = new Uri("http://example.com/test");
            var relativeUri = "#myResource";
            var target = new Resource(new Uri(baseUri, relativeUri));
            var property = new Property(new Uri(baseUri, "#related"));
            double value = 0.12345632F; // TODO: Passenden Wert initialisieren
            target.AddProperty(property, value);
            Assert.AreEqual(true, target.HasProperty(property, value));
            target.RemoveProperty(property, value);
            Assert.AreEqual(false, target.HasProperty(property, value));
            Assert.AreEqual(false, target.HasProperty(property));
        }

        #endregion
    }
}
