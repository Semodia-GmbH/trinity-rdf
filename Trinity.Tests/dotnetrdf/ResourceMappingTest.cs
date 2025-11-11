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
using System.Linq;
using Semiodesk.Trinity;
using System.Reflection;

using System.IO;
using Semiodesk.Trinity.Ontologies;
using Semiodesk.Trinity.Test;
using Xunit;

namespace dotNetRDFStore.Test
{


    public class ResourceMappingTest: IDisposable
    {
        public static bool RegisteredOntology;

        IStore Store;
        IModel Model;

        public ResourceMappingTest()
        {
            if (ResourceMappingTest.RegisteredOntology == false)
            {
                OntologyDiscovery.AddAssembly(Assembly.GetExecutingAssembly());
                MappingDiscovery.RegisterAssembly(Assembly.GetExecutingAssembly());
                ResourceMappingTest.RegisteredOntology = true;
            }

            Store = StoreFactory.CreateStore("provider=dotnetrdf");

            var testModel = new Uri("ex:Test");
            Model = Store.CreateModel(testModel);
        }
        
        IModel GetModel()
        {
            return Model;
        }

        public void Dispose()
        {
            Store.Dispose();
        }

        
        //[Fact]
        // This test does not run, but it needs to.
        public void AddUnmappedType()
        {
            var m = GetModel();
            m.Clear();

            var t1Uri = new Uri("semio:test:testInstance1");
            var t2Uri = new Uri("semio:test:testInstance2");
            var t1 = m.CreateResource<MappingTestClass>(t1Uri);

            var r = m.CreateResource(t2Uri);
            r.AddProperty(rdf.type, TestOntology.TestClass2);

            t1.AddProperty(TestOntology.uniqueResourceTest, r);
            t1.AddProperty(TestOntology.resourceTest, r);

            Assert.Null(t1.uniqueResourceTest);
            Assert.Empty(t1.resourceTest);

            m.Clear();
        }

        [Fact(Skip = "This test poses an interesting problem. If we remove the type of a mapped resource, how should we handle that.")]
        public void RemoveTypeTest()
        {
            var m = GetModel();
            m.Clear();

            var t1Uri = new Uri("semio:test:testInstance1");
            var t1 = m.CreateResource<MappingTestClass>(t1Uri);

            t1.RemoveProperty(rdf.type, TestOntology.TestClass);

            Assert.DoesNotContain(rdf.type, t1.ListProperties());
            
            m.Clear();
        }

        [Fact]
        public void AddRemoveIntegerTest()
        {
            var m = GetModel();
            m.Clear();

            var t1Uri = new Uri("semio:test:testInstance1");
            var t1 = m.CreateResource<MappingTestClass>(t1Uri);

            // Add value using the mapping interface
            var value = 1;
            t1.uniqueIntTest = value;

            t1.Commit();

            var t_actual = m.GetResource<MappingTestClass>(t1Uri);

            // Test if value was stored
            Assert.Equal(value, t_actual.uniqueIntTest);


            // Test if property is present
            var l = t_actual.ListProperties();
            Assert.True(l.Contains(TestOntology.uniqueIntTest));
            Assert.Equal(2, l.Count());

            // Test if ListValues works
            Assert.Equal(typeof(int), t_actual.ListValues(TestOntology.uniqueIntTest).First().GetType());
            Assert.Equal(value, t_actual.ListValues(TestOntology.uniqueIntTest).First());

            // Remove with RemoveProperty
            t1.RemoveProperty(TestOntology.uniqueIntTest, value);
            t1.Commit();

            t_actual = m.GetResource<MappingTestClass>(t1Uri);

            // Test if ListProperties works
            l = t_actual.ListProperties();
            Assert.False(l.Contains(TestOntology.uniqueIntTest));

            // Test if ListValues works
            Assert.Equal(0, t_actual.ListValues(TestOntology.uniqueIntTest).Count());

            m.Clear();
        }

        [Fact]
        public void AddRemoveIntegerListTest()
        {
            var m = GetModel();
            m.Clear();
            var t1Uri = new Uri("semio:test:testInstance1");
            var t1 = m.CreateResource<MappingTestClass>(t1Uri);
            // Add value using the mapping interface
            var value = 2;
            t1.intTest.Add(value);

            t1.Commit();

            var t_actual = m.GetResource<MappingTestClass>(t1Uri);

            // Test if value was stored
            Assert.Equal(1, t_actual.intTest.Count());
            Assert.Equal(value, t_actual.intTest[0]);

            // Test if property is present
            var l = t_actual.ListProperties();
            Assert.True(l.Contains(TestOntology.intTest));
            Assert.Equal(2, l.Count());

            // Test if ListValues works
            Assert.Equal(typeof(int), t_actual.ListValues(TestOntology.intTest).First().GetType());
            Assert.Equal(value, t_actual.ListValues(TestOntology.intTest).First());

            // Add another value
            var value2 = -18583;
            t1.intTest.Add(value2);
            t1.Commit();
            t_actual = m.GetResource<MappingTestClass>(t1Uri);


            // Test if value was stored
            Assert.Equal(2, t_actual.intTest.Count());
            Assert.True(t_actual.intTest.Contains(value));
            Assert.True(t_actual.intTest.Contains(value2));

            // Test if property is present
            l = t_actual.ListProperties();
            Assert.True(l.Contains(TestOntology.intTest));
            Assert.Equal(2, l.Count());

            // Test if ListValues works
            var res = t_actual.ListValues(TestOntology.intTest).ToList();
            Assert.Equal(typeof(int), res[0].GetType());
            Assert.Equal(typeof(int), res[1].GetType());
            Assert.True(res.Contains(value));
            Assert.True(res.Contains(value2));

            // Remove value from mapped list
            t1.intTest.Remove(value2);
            t1.Commit();
            t_actual = m.GetResource<MappingTestClass>(t1Uri);

            // Test if removed
            Assert.Equal(1, t_actual.intTest.Count());

            // Test if ListProperties works
            l = t_actual.ListProperties();
            Assert.True(l.Contains(TestOntology.intTest));

            // Test if first added property is still present
            Assert.Equal(typeof(int), t_actual.ListValues(TestOntology.intTest).First().GetType());
            Assert.Equal(value, t_actual.ListValues(TestOntology.intTest).First());

            t1.intTest.Remove(value);
            t1.Commit();
            t_actual = m.GetResource<MappingTestClass>(t1Uri);

            l = t_actual.ListProperties();
            Assert.False(l.Contains(TestOntology.intTest));

            // Test if ListValues works
            Assert.Equal(0, t_actual.ListValues(TestOntology.intTest).Count());

            m.Clear();
        }

        /// <summary>
        /// This Test fails because the datatype "unsigned int" is not stored correctly in the database. 
        /// To be more specific the xsd type is missing although it is given at the insert.
        /// </summary>
        //[Fact]
        public void AddRemoveUnsignedIntegerTest()
        {
            var m = GetModel();
            m.Clear();
            var t1Uri = new Uri("semio:test:testInstance1");
            var t1 = m.CreateResource<MappingTestClass>(t1Uri);

            // Add value using the mapping interface
            uint uValue = 1;
            t1.uniqueUintTest = uValue;

            t1.Commit();

            var t_actual = m.GetResource<MappingTestClass>(t1Uri);

            // Test if value was stored
            Assert.Equal(uValue, t_actual.uniqueUintTest);


            // Test if property is present
            var l = t1.ListProperties();
            Assert.True(l.Contains(TestOntology.uniqueUintTest));
            Assert.Equal(2, l.Count());

            // Test if ListValues works
            Assert.Equal(typeof(uint), t_actual.ListValues(TestOntology.uniqueUintTest).First().GetType());
            Assert.Equal(uValue, t_actual.ListValues(TestOntology.uniqueUintTest).First());

            // Remove with RemoveProperty
            t1.RemoveProperty(TestOntology.uniqueUintTest, uValue);
            t1.Commit();

            t_actual = m.GetResource<MappingTestClass>(t1Uri);

            // Test if ListProperties works
            l = (List<Property>)t_actual.ListProperties();
            Assert.False(l.Contains(TestOntology.uniqueUintTest));

            // Test if ListValues works
            Assert.Equal(0, t_actual.ListValues(TestOntology.uniqueUintTest).Count());

            m.Clear();
        }

        //[Fact]
        public void AddRemoveUnsignedIntegerListTest()
        {
            var m = GetModel();
            m.Clear();
            var t1Uri = new Uri("semio:test:testInstance1");
            var t1 = m.CreateResource<MappingTestClass>(t1Uri);

            // Add value using the mapping interface
            uint uValue = 2;
            t1.uintTest.Add(uValue);

            t1.Commit();
            var t_actual = m.GetResource<MappingTestClass>(t1Uri);

            // Test if value was stored
            Assert.Equal(1, t_actual.uintTest.Count());
            Assert.Equal(uValue, t_actual.uintTest[0]);


            // Test if property is present
            var l = t1.ListProperties();
            Assert.True(l.Contains(TestOntology.uintTest));
            Assert.Equal(2, l.Count());

            // Test if ListValues works
            Assert.Equal(typeof(uint), t_actual.ListValues(TestOntology.uintTest).First().GetType());
            Assert.Equal(uValue, t_actual.ListValues(TestOntology.uintTest).First());

            // Remove value from mapped list
            t1.uintTest.Remove(uValue);
            t1.Commit();

            t_actual = m.GetResource<MappingTestClass>(t1Uri);

            // Test if removed
            Assert.Equal(0, t_actual.uintTest.Count());

            // Test if ListProperties works
            l = (List<Property>)t_actual.ListProperties();
            Assert.False(l.Contains(TestOntology.uintTest));

            // Test if ListValues works
            Assert.Equal(0, t_actual.ListValues(TestOntology.uintTest).Count());
            m.Clear();
        }

        [Fact]
        public void AddRemoveStringTest()
        {
            var m = GetModel();
            m.Clear();
            var t1Uri = new Uri("semio:test:testInstance1");
            var t1 = m.CreateResource<MappingTestClass>(t1Uri);


            // Add value using the mapping interface
            var strValue = "Hallo Welt!";
            t1.uniqueStringTest = strValue;
            t1.Commit();

            var t_actual = m.GetResource<MappingTestClass>(t1Uri);

            // Test if value was stored
            Assert.Equal(strValue, t_actual.uniqueStringTest);


            // Test if property is present
            var l = t_actual.ListProperties();
            Assert.True(l.Contains(TestOntology.uniqueStringTest));
            Assert.Equal(2, l.Count());

            var x = t_actual.HasProperty(TestOntology.uniqueStringTest);
            Assert.True(x);

            x = t_actual.HasProperty(TestOntology.uniqueStringTest, strValue);
            Assert.True(x);

            // Test if ListValues works
            Assert.Equal(typeof(string), t_actual.ListValues(TestOntology.uniqueStringTest).First().GetType());
            Assert.Equal(strValue, t1.ListValues(TestOntology.uniqueStringTest).First());

            // Remove with RemoveProperty
            t1.RemoveProperty(TestOntology.uniqueStringTest, strValue);
            t1.Commit();
            t_actual = m.GetResource<MappingTestClass>(t1Uri);

            // Test if ListProperties works
            l = t_actual.ListProperties();
            Assert.False(l.Contains(TestOntology.uniqueStringTest));

            x = t_actual.HasProperty(TestOntology.uniqueStringTest);
            Assert.False(x);

            x = t_actual.HasProperty(TestOntology.uniqueStringTest, strValue);
            Assert.False(x);

            // Test if ListValues works
            Assert.Equal(0, t_actual.ListValues(TestOntology.uniqueStringTest).Count());
            m.Clear();
        }

        [Fact]
        public void AddRemoveLocalizedStringTest()
        {
            var m = GetModel();
            m.Clear();
            var t1Uri = new Uri("semio:test:testInstance1");
            var t1 = m.CreateResource<MappingTestClass>(t1Uri);

            t1.AddProperty(TestOntology.uniqueStringTest, "Hallo Welt", "de");
            t1.Commit();

            var tt = m.GetResource<MappingTestClass>(t1Uri);


        }


        [Fact]
        public void AddRemoveStringListTest()
        {
            var m = GetModel();
            m.Clear();
            var t1Uri = new Uri("semio:test:testInstance1");
            var t1 = m.CreateResource<MappingTestClass>(t1Uri);

            // Add value using the mapping interface
            var strValue = "（╯°□°）╯︵ ┻━┻";
            t1.stringTest.Add(strValue);

            t1.Commit();

            var t_actual = m.GetResource<MappingTestClass>(t1Uri);

            // Test if value was stored
            Assert.Equal(1, t_actual.stringTest.Count());
            Assert.Equal(strValue, t_actual.stringTest[0]);


            // Test if property is present
            var l = t_actual.ListProperties();
            Assert.True(l.Contains(TestOntology.stringTest));
            Assert.Equal(2, l.Count());

            var x = t_actual.HasProperty(TestOntology.stringTest);
            Assert.True(x);

            x = t_actual.HasProperty(TestOntology.stringTest, strValue);
            Assert.True(x);

            // Test if ListValues works
            Assert.Equal(typeof(string), t_actual.ListValues(TestOntology.stringTest).First().GetType());
            Assert.Equal(strValue, t_actual.ListValues(TestOntology.stringTest).First());


            // Remove value from mapped list
            t1.stringTest.Remove(strValue);
            t1.Commit();

            t_actual = m.GetResource<MappingTestClass>(t1Uri);

            // Test if removed
            Assert.Equal(0, t_actual.boolTest.Count());

            // Test if ListProperties works
            l = t_actual.ListProperties();
            Assert.False(l.Contains(TestOntology.stringTest));

            x = t_actual.HasProperty(TestOntology.stringTest);
            Assert.False(x);

            x = t_actual.HasProperty(TestOntology.stringTest, strValue);
            Assert.False(x);

            // Test if ListValues works
            Assert.Equal(0, t_actual.ListValues(TestOntology.stringTest).Count());

            m.Clear();
        }

        [Fact]
        public void AddRemoveBoolTest()
        {
            var m = GetModel();
            m.Clear();
            var t1Uri = new Uri("semio:test:testInstance1");
            var t1 = m.CreateResource<MappingTestClass>(t1Uri);


            // Add value using the mapping interface
            var bValue = true;
            t1.uniqueBoolTest = bValue;

            t1.Commit();

            var t_actual = m.GetResource<MappingTestClass>(t1Uri);

            // Test if value was stored
            Assert.Equal(bValue, t_actual.uniqueBoolTest);


            // Test if property is present
            var l = t_actual.ListProperties();
            Assert.True(l.Contains(TestOntology.uniqueBoolTest));
            Assert.Equal(2, l.Count());

            // Test if ListValues works
            Assert.Equal(typeof(bool), t_actual.ListValues(TestOntology.uniqueBoolTest).First().GetType());
            Assert.Equal(bValue, t_actual.ListValues(TestOntology.uniqueBoolTest).First());

            // Remove with RemoveProperty
            t1.RemoveProperty(TestOntology.uniqueBoolTest, bValue);
            t1.Commit();

            t_actual = m.GetResource<MappingTestClass>(t1Uri);

            // Test if ListProperties works
            l = t_actual.ListProperties();
            Assert.False(l.Contains(TestOntology.uniqueBoolTest));

            // Test if ListValues works
            Assert.Equal(0, t_actual.ListValues(TestOntology.uniqueBoolTest).Count());

            m.Clear();
        }

        [Fact]
        public void AddRemoveBoolListTest()
        {
            var m = GetModel();
            m.Clear();
            var t1Uri = new Uri("semio:test:testInstance1");
            var t1 = m.CreateResource<MappingTestClass>(t1Uri);


            // Add value using the mapping interface
            var value = true;
            t1.boolTest.Add(value);

            t1.Commit();
            var t_actual = m.GetResource<MappingTestClass>(t1Uri);

            // Test if value was stored
            Assert.Equal(1, t_actual.boolTest.Count());
            Assert.Equal(value, t_actual.boolTest[0]);


            // Test if property is present
            var l = t_actual.ListProperties();
            Assert.True(l.Contains(TestOntology.boolTest));
            Assert.Equal(2, l.Count());

            // Test if ListValues works
            Assert.Equal(typeof(bool), t_actual.ListValues(TestOntology.boolTest).First().GetType());
            Assert.Equal(value, t_actual.ListValues(TestOntology.boolTest).First());

            // Remove value from mapped list
            t1.boolTest.Remove(value);
            t1.Commit();

            t_actual = m.GetResource<MappingTestClass>(t1Uri);

            // Test if removed
            Assert.Equal(0, t_actual.boolTest.Count());

            // Test if ListProperties works
            l = t_actual.ListProperties();
            Assert.False(l.Contains(TestOntology.boolTest));

            // Test if ListValues works
            Assert.Equal(0, t_actual.ListValues(TestOntology.boolTest).Count());

            m.Clear();
        }

        /// <summary>
        /// Note: 
        /// Datetime precision in Virtuoso is not as high as native .net datetime precision.
        /// </summary>
        [Fact]
        public void AddRemoveDateTimeTest()
        {
            var m = GetModel();
            m.Clear();
            var t1Uri = new Uri("semio:test:testInstance1");
            var t1 = m.CreateResource<MappingTestClass>(t1Uri);


            // Add value using the mapping interface
            var Value = new DateTime(2012, 8, 15, 12, 3, 55, DateTimeKind.Local);
            t1.uniqueDateTimeTest = Value;
            t1.Commit();

            var t_actual = m.GetResource<MappingTestClass>(t1Uri);

            // Test if value was stored
            Assert.Equal(Value.ToUniversalTime(), t_actual.uniqueDateTimeTest.ToUniversalTime());


            // Test if property is present
            var l = t_actual.ListProperties();
            Assert.True(l.Contains(TestOntology.uniqueDatetimeTest));
            Assert.Equal(2, l.Count());

            // Test if ListValues works
            Assert.Equal(typeof(DateTime), t_actual.ListValues(TestOntology.uniqueDatetimeTest).First().GetType());
            var time = (DateTime)t_actual.ListValues(TestOntology.uniqueDatetimeTest).First();
            Assert.Equal(Value.ToUniversalTime(), time.ToUniversalTime());

            // Remove with RemoveProperty
            t1.RemoveProperty(TestOntology.uniqueDatetimeTest, Value);
            t1.Commit();

            t_actual = m.GetResource<MappingTestClass>(t1Uri);

            // Test if ListProperties works
            l = t_actual.ListProperties();
            Assert.False(l.Contains(TestOntology.uniqueBoolTest));

            // Test if ListValues works
            Assert.Equal(0, t_actual.ListValues(TestOntology.uniqueDatetimeTest).Count());


            var t = new DateTime();
            Assert.True(DateTime.TryParse("2013-01-21T16:27:23.000Z", out t));

            t1.uniqueDateTimeTest = t;
            t1.Commit();

            t_actual = m.GetResource<MappingTestClass>(t1Uri);
            Assert.Equal(t1.uniqueDateTimeTest.ToUniversalTime(), t_actual.uniqueDateTimeTest.ToUniversalTime());

            m.Clear();
        }

        [Fact]
        public void TimeZoneTest()
        {
            var m = GetModel();
            m.Clear();

            var t1Uri = new Uri("semio:test:testInstance1");
            var t = new DateTime();
            Assert.True(DateTime.TryParse("2013-01-21T16:27:23.000Z", out t));

            var t1 = m.CreateResource<MappingTestClass>(t1Uri);
            t1.uniqueDateTimeTest = t;
            t1.Commit();

            var t_actual = m.GetResource<MappingTestClass>(t1Uri);
        }

        [Fact]
        public void AddRemoveDateTimeListTest()
        {
            var m = GetModel();
            m.Clear();
            var t1Uri = new Uri("semio:test:testInstance1");
            var t1 = m.CreateResource<MappingTestClass>(t1Uri);


            // Add value using the mapping interface
            var value = new DateTime(2012, 8, 15, 12, 3, 55, DateTimeKind.Local);
            t1.dateTimeTest.Add(value);
            t1.Commit();
            var t_actual = m.GetResource<MappingTestClass>(t1Uri);

            // Test if value was stored
            Assert.Equal(1, t1.dateTimeTest.Count());
            Assert.Equal(value, t1.dateTimeTest[0]);


            // Test if property is present
            var l = t1.ListProperties();
            Assert.True(l.Contains(TestOntology.datetimeTest));
            Assert.Equal(2, l.Count());

            // Test if ListValues works
            Assert.Equal(typeof(DateTime), t_actual.ListValues(TestOntology.datetimeTest).First().GetType());
            var time = (DateTime)t_actual.ListValues(TestOntology.datetimeTest).First();
            Assert.Equal(value.ToUniversalTime(), time.ToUniversalTime());

            // Remove value from mapped list
            t1.dateTimeTest.Remove(value);
            t1.Commit();

            t_actual = m.GetResource<MappingTestClass>(t1Uri);

            // Test if removed
            Assert.Equal(0, t_actual.dateTimeTest.Count());

            // Test if ListProperties works
            l = t_actual.ListProperties();
            Assert.False(l.Contains(TestOntology.datetimeTest));

            // Test if ListValues works
            Assert.Equal(0, t_actual.ListValues(TestOntology.datetimeTest).Count());
        }

        /// <summary>
        /// Note: 
        /// Datetime precision in Virtuoso is not as high as native .net datetime precision.
        /// </summary>
        [Fact]
        public void AddRemoveTimeSpanTest()
        {
            var m = GetModel();
            m.Clear();
            var t1Uri = new Uri("semio:test:testInstance1");
            var t1 = m.CreateResource<MappingTestClass>(t1Uri);


            // Add value using the mapping interface
            var Value = TimeSpan.FromSeconds(200);
            t1.uniqueTimeSpanTest = Value;
            t1.Commit();

            var t_actual = m.GetResource<MappingTestClass>(t1Uri);

            // Test if value was stored
            Assert.Equal(Value.TotalSeconds, t_actual.uniqueTimeSpanTest.TotalSeconds);


            // Test if property is present
            var l = t_actual.ListProperties();
            Assert.True(l.Contains(TestOntology.uniqueTimespanTest));
            Assert.Equal(2, l.Count());

            // Test if ListValues works
            Assert.Equal(typeof(TimeSpan), t_actual.ListValues(TestOntology.uniqueTimespanTest).First().GetType());
            var time = (TimeSpan)t_actual.ListValues(TestOntology.uniqueTimespanTest).First();
            Assert.Equal(Value.TotalSeconds, time.TotalSeconds);

            // Remove with RemoveProperty
            t1.RemoveProperty(TestOntology.uniqueTimespanTest, Value);
            t1.Commit();

            t_actual = m.GetResource<MappingTestClass>(t1Uri);

            // Test if ListProperties works
            l = t_actual.ListProperties();
            Assert.False(l.Contains(TestOntology.uniqueBoolTest));

            // Test if ListValues works
            Assert.Equal(0, t_actual.ListValues(TestOntology.uniqueTimespanTest).Count());


            t1.uniqueTimeSpanTest = TimeSpan.FromSeconds(200);
            t1.Commit();

            t_actual = m.GetResource<MappingTestClass>(t1Uri);
            Assert.Equal(t1.uniqueTimeSpanTest.TotalSeconds, t_actual.uniqueTimeSpanTest.TotalSeconds);

            m.Clear();
        }
        [Fact]
        public void AddRemoveTimeSpanListTest()
        {
            var m = GetModel();
            m.Clear();
            var t1Uri = new Uri("semio:test:testInstance1");
            var t1 = m.CreateResource<MappingTestClass>(t1Uri);


            // Add value using the mapping interface
            var value = TimeSpan.FromSeconds(100);
            t1.timeSpanTest.Add(value);
            t1.Commit();
            var t_actual = m.GetResource<MappingTestClass>(t1Uri);

            // Test if value was stored
            Assert.Equal(1, t1.timeSpanTest.Count());
            Assert.Equal(value, t1.timeSpanTest[0]);


            // Test if property is present
            var l = t1.ListProperties();
            Assert.True(l.Contains(TestOntology.timespanTest));
            Assert.Equal(2, l.Count());

            // Test if ListValues works
            Assert.Equal(typeof(TimeSpan), t_actual.ListValues(TestOntology.timespanTest).First().GetType());
            var time = (TimeSpan)t_actual.ListValues(TestOntology.timespanTest).First();
            Assert.Equal(value.TotalSeconds, time.TotalSeconds);

            // Remove value from mapped list
            t1.timeSpanTest.Remove(value);
            t1.Commit();

            t_actual = m.GetResource<MappingTestClass>(t1Uri);

            // Test if removed
            Assert.Equal(0, t_actual.timeSpanTest.Count());

            // Test if ListProperties works
            l = t_actual.ListProperties();
            Assert.False(l.Contains(TestOntology.timespanTest));

            // Test if ListValues works
            Assert.Equal(0, t_actual.ListValues(TestOntology.timespanTest).Count());
        }

        [Fact]
        public void AddRemoveResourceTest()
        {
            var m = GetModel();
            m.Clear();
            var t1Uri = new Uri("semio:test:testInstance1");
            var t1 = m.CreateResource<MappingTestClass>(t1Uri);

            var testClass2Uri = new Uri("semio:test:testInstance2");
            var t2 = new MappingTestClass2(testClass2Uri);

            t1.uniqueResourceTest = t2;
            t1.Commit();
            var t_actual = m.GetResource<MappingTestClass>(t1Uri);


            Assert.Equal(t2, t_actual.uniqueResourceTest);

            var l = t_actual.ListProperties().ToList();
            Assert.Contains(TestOntology.uniqueResourceTest, l);
            Assert.Equal(2, l.Count());

            var x = t_actual.HasProperty(TestOntology.uniqueResourceTest);
            Assert.True(x);

            x = t_actual.HasProperty(TestOntology.uniqueResourceTest, t2);
            Assert.True(x);

            t_actual = m.GetResource<MappingTestClass>(t1Uri);
            var values = t_actual.ListValues().ToList();
            Assert.Contains(new Tuple<Property, object>(TestOntology.uniqueResourceTest, t2), values);


            Assert.True(typeof(Resource).IsAssignableFrom(t_actual.ListValues(TestOntology.uniqueResourceTest).First().GetType()));
            //Assert.Equal(t2, t_actual.ListValues(TestOntology.uniqeResourceTest).First());

            t1.RemoveProperty(TestOntology.uniqueResourceTest, t2);
            t1.Commit();
            t_actual = m.GetResource<MappingTestClass>(t1Uri);


            l = t_actual.ListProperties().ToList();
            Assert.False(l.Contains(TestOntology.uniqueResourceTest));

            x = t_actual.HasProperty(TestOntology.uniqueResourceTest);
            Assert.False(x);

            x = t_actual.HasProperty(TestOntology.uniqueResourceTest, t2);
            Assert.False(x);

            // Test if ListValues works
            Assert.Equal(0, t_actual.ListValues(TestOntology.uniqueResourceTest).Count());
        }

        [Fact]
        public void MappedResourceValueTest()
        {
            var m = GetModel();
            m.Clear();
            var t1Uri = new Uri("semio:test:testInstance1");
            var t1 = m.CreateResource<MappingTestClass>(t1Uri);

            var testClass2Uri = new Uri("semio:test:testInstance2");
            var t2 = m.CreateResource<MappingTestClass2>(testClass2Uri);
            t2.uniqueStringTest = "TestString";
            t2.Commit();

            t1.uniqueResourceTest = t2;
            t1.Commit();
            var t_actual = m.GetResource<MappingTestClass>(t1Uri);
            Assert.Equal("TestString", t_actual.uniqueResourceTest.uniqueStringTest);

        }


        [Fact]
        public void AddRemoveResourceListTest()
        {
            var m = GetModel();
            m.Clear();
            var t1Uri = new Uri("semio:test:testInstance1");
            var t1 = m.CreateResource<MappingTestClass>(t1Uri);


            // Add value using the mapping interface
            var t2 = new MappingTestClass2(new Uri("semio:test:testInstance2"));
            t1.resourceTest.Add(t2);
            t1.Commit();
            var t_actual = m.GetResource<MappingTestClass>(t1Uri);

            Assert.Equal(1, t_actual.resourceTest.Count);
            Assert.Equal(t2, t_actual.resourceTest[0]);

            var l = t_actual.ListProperties();
            Assert.Equal(2, l.Count());
            Assert.True(l.Contains(TestOntology.resourceTest));

            var x = t_actual.HasProperty(TestOntology.resourceTest);
            Assert.True(x);

            x = t_actual.HasProperty(TestOntology.resourceTest, t2);
            Assert.True(x);

            var v = t_actual.ListValues(TestOntology.resourceTest);
            Assert.Equal(2, l.Count());
            Assert.True(v.Contains(t2));

            Assert.Equal(t2.GetType(), v.First().GetType());

            t1.resourceTest.Remove(t2);
            t1.Commit();
            t_actual = m.GetResource<MappingTestClass>(t1Uri);

            x = t_actual.HasProperty(TestOntology.resourceTest);
            Assert.False(x);

            x = t_actual.HasProperty(TestOntology.resourceTest, t2);
            Assert.False(x);


            Assert.Equal(0, t_actual.resourceTest.Count);
        }


        [Fact]
        public void LazyLoadResourceTest()
        {
            MappingDiscovery.RegisterCallingAssembly();
            var model = GetModel();
            model.Clear();

            var testRes1 = new Uri("semio:test:testInstance");
            var testRes2 = new Uri("semio:test:testInstance2");
            var t1 = model.CreateResource<MappingTestClass>(testRes1);
            var t2 = model.CreateResource<MappingTestClass2>(new Uri("semio:test:testInstance2"));

            t1.uniqueResourceTest = t2;
            // TODO: Debug messsage, because t2 was not commited
            t1.Commit();

            var p1 = model.GetResource<MappingTestClass>(testRes1);
            //Assert.Equal(null, p1.uniqueResourceTest);

            var v = p1.ListValues(TestOntology.uniqueResourceTest);
            Assert.Equal(t2.Uri.OriginalString, (v.First() as IResource).Uri.OriginalString);

            model.DeleteResource(t1);

            model.DeleteResource(t2);

            t1 = model.CreateResource<MappingTestClass>(testRes1);

            t2 = model.CreateResource<MappingTestClass2>(new Uri("semio:test:testInstance2"));
            t2.Commit();

            t1.uniqueResourceTest = t2;
            t1.Commit();

            IResource tr1 = model.GetResource(testRes1, t1.GetType()) as Resource;
            Assert.Equal(typeof(MappingTestClass), tr1.GetType());
            var p2 = model.GetResource<MappingTestClass2>(testRes2);

            Assert.Equal(t2, p1.uniqueResourceTest);

            model.Clear();
        }


        [Fact]
        public void IterateOverEmptyModel()
        {
            var store = StoreFactory.CreateMemoryStore();
            var modelUri = new Uri("urn:modom:default");

            var model = store.GetModel(modelUri);
            var res = model.GetResources<MappingTestClass>().FirstOrDefault();
            Assert.Null(res);
        }

        [Fact]
        public void IterateOverJsonLdModel()
        {
            const string s = "{ \"@type\": \"semio:test:TestClass\", \"@id\": \"urn:ex:test\", \"http://schema.modom.io/path\": \"test\" }";

            using (var stream = new MemoryStream())
            {
                var writer = new StreamWriter(stream);
                writer.Write(s);
                writer.Flush();
                stream.Position = 0;
                var store = StoreFactory.CreateMemoryStore();
                var modelUri = new Uri("urn:modom:default");
                store.Read(stream, modelUri, RdfSerializationFormat.JsonLd, true);

                var model = store.GetModel(modelUri);
                var res = model.GetResources<MappingTestClass>().FirstOrDefault();

            }
        }

        [Fact]
        public void IterateOverFaultyJsonLdModel()
        {

            var s = "{ \"@type\": \"semio:test:TestClass\", \"@i\": \"urn:ex:test\", \"http://schema.modom.io/path\": \"test\" }";

            using (var stream = new MemoryStream())
            {
                var writer = new StreamWriter(stream);
                writer.Write(s);
                writer.Flush();
                stream.Position = 0;
                var store = StoreFactory.CreateMemoryStore();
                var modelUri = new Uri("urn:modom:default");
                store.Read(stream, modelUri, RdfSerializationFormat.JsonLd, true);

                var model = store.GetModel(modelUri);
                var res = model.GetResources<MappingTestClass>().FirstOrDefault();

            }
        }

        [Fact]
        public void MappingTypeTest()
        {
            var m = GetModel();
            m.Clear();
            var t1Uri = new Uri("semio:test:testInstance1");
            var t1 = m.CreateResource<MappingTestClass2>(t1Uri);
            t1.uniqueStringTest = "testing 1";
            t1.Commit();

            var t2Uri = new Uri("semio:test:testInstance2");
            var t2 = m.CreateResource<MappingTestClass3>(t2Uri);
            t2.uniqueStringTest = "testing 2";
            t2.Commit();

            var t3Uri = new Uri("semio:test:testInstance3");
            var t3 = m.CreateResource<MappingTestClass4>(t3Uri);
            t3.uniqueStringTest = "testing 3";
            t3.Commit();

            var r1 = m.GetResource<Resource>(t1Uri);
            Assert.Equal(t1, r1);

            var r2 = m.GetResource<Resource>(t2Uri);
            Assert.Equal(t2, r2);

            var r3 = m.GetResource<Resource>(t3Uri);
            Assert.Equal(t3, r3);
        }


        [Fact]
        public void RollbackTest()
        {
            var m = GetModel();
            var t1Uri = new Uri("semio:test:testInstance1");
            var t1 = m.CreateResource<MappingTestClass>(t1Uri);


            // Add value using the mapping interface
            var strValue = "Hallo Welt!";
            t1.uniqueStringTest = strValue;
            t1.Commit();

            t1.uniqueStringTest = "HelloWorld!";

            t1.Rollback();


            Assert.Equal(strValue, t1.uniqueStringTest);

            var newRef = m.GetResource<MappingTestClass>(t1Uri);
            newRef.stringTest.Add("Hi");
            newRef.stringTest.Add("Blub");
            newRef.Commit();

            t1.Rollback();


            Assert.Equal(2, t1.stringTest.Count);
            Assert.True(t1.stringTest.Contains("Hi"));
            Assert.True(t1.stringTest.Contains("Blub"));


            var t2Uri = new Uri("semio:test:testInstance2");
            var p = m.CreateResource<MappingTestClass2>(t2Uri);
            p.uniqueStringTest = "blub";
            p.Commit();

            newRef = m.GetResource<MappingTestClass>(t1Uri);
            newRef.resourceTest.Add(p);
            newRef.Commit();

            t1.Rollback();


            Assert.True(t1.resourceTest.Count == 1);
            Assert.True(t1.resourceTest.Contains(p));

        }


        [Fact]
        public void RollbackMappedResourcesTest()
        {
            var m = GetModel();
            m.Clear();

            var t1Uri = new Uri("semio:test:testInstance1");
            var t1 = m.CreateResource<SingleResourceMappingTestClass>(t1Uri);
            t1.Commit();

            Assert.True(t1.ResourceTest.Count == 0);

            var t2Uri = new Uri("semio:test:testInstance2");
            var t2 = m.CreateResource<SingleMappingTestClass>(t2Uri);
            t2.stringTest.Add("blub");
            t2.Commit();

            var newRef = m.GetResource<SingleResourceMappingTestClass>(t1Uri);
            newRef.ResourceTest.Add(t2);
            newRef.Commit();

            t1.Rollback();

            Assert.True(t1.ResourceTest.Count == 1);
            Assert.True(t1.ResourceTest.Contains(t2));
        }

        [Fact]
        public void ListValuesTest()
        {
            var m = GetModel();
            m.Clear();
            var t1Uri = new Uri("semio:test:testInstance1");
            var t1 = m.CreateResource<MappingTestClass>(t1Uri);


            // Add value using the mapping interface
            var strValue = "Hallo Welt!";
            t1.uniqueStringTest = strValue;
            t1.Commit();

            t1.stringTest.Add("Hi");
            t1.stringTest.Add("Blub");
            t1.Commit();

            var x = t1.ListValues(TestOntology.stringTest);

            var actual = m.GetResource<MappingTestClass>(t1.Uri);
            var x2 = actual.ListValues(TestOntology.stringTest);

            var res1 = x.ToList();
            var res2 = x2.ToList();
            Assert.Equal(res1.Count, res2.Count);
            Assert.True(res2.Contains(res1[0]));
            Assert.True(res2.Contains(res1[1]));

        }


        [Fact]
        public void KeepListsAfterRollbackTest()
        {
            var m = GetModel();
            m.Clear();

            var t1Uri = new Uri("semio:test:testInstance8");

            var t1 = m.CreateResource<SingleMappingTestClass>(t1Uri);
            t1.AddProperty(TestOntology.uniqueStringTest, "Hello");

            t1.Commit();
            t1.Rollback();

            t1.stringTest.Add("Hi");
            t1.stringTest.Add("Blub");

            var x = t1.ListValues(TestOntology.stringTest).ToList();
            Assert.Equal(2, x.Count);
            t1.Commit();

            var t2 = m.GetResource<SingleMappingTestClass>(t1Uri);

            var x2 = t2.ListValues(TestOntology.stringTest).ToList();

            Assert.Equal(x.Count, x2.Count);
            Assert.True(x2.Contains(x[0]));
            Assert.True(x2.Contains(x[1]));

        }

        [Fact]
        public void TestEquality()
        {
            var c1 = new Resource(new Uri("http://www.semanticdesktop.org/ontologies/2007/04/02/ncal#cancelledStatus"));
            var c2 = new Resource(new Uri("http://www.semanticdesktop.org/ontologies/2007/04/02/ncal#cancelledStatus"));

            Assert.True(c1.Equals(c2));
            Assert.False(c1 == c2);
        }
    }

}
