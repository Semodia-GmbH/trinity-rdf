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
using System.Text;
using NUnit.Framework;
using Semiodesk.Trinity.Ontologies;
using System.IO;
using System.Globalization;

namespace Semiodesk.Trinity.Test.Virtuoso
{
    [TestFixture]
    public class ModelTest : SetupClass
    {
        private IStore Store;

        private IModel Model;
        private IModel Model2;

        public ModelTest()
        {
        }

        [SetUp]
        public void SetUp()
        {
            var connectionString = SetupClass.ConnectionString;

            Store = StoreFactory.CreateStore(string.Format("{0};rule=urn:semiodesk/test/ruleset", connectionString));
            Store.InitializeFromConfiguration();

            Model = Store.GetModel(new Uri("http://example.org/TestModel"));

            if (!Model.IsEmpty)
            {
                Model.Clear();
            }

            Model2 = Store.GetModel(new Uri("semiodesk:Trinity:Test"));

            if (!Model2.IsEmpty)
            {
                Model2.Clear();
            }

            var model_resource = Model.CreateResource(new Uri("http://example.org/MyResource"));

            var property = new Property(new Uri("http://example.org/MyProperty"));
            model_resource.AddProperty(property, "in the jungle");
            model_resource.AddProperty(property, 123);
            model_resource.AddProperty(property, DateTime.Now);
            model_resource.Commit();

            var model_resource2 = Model.CreateResource(new Uri("ex:Resource"));
            model_resource2.AddProperty(property, "in the jungle");
            model_resource2.AddProperty(property, 123);
            model_resource2.AddProperty(property, DateTime.Now);
            model_resource2.Commit();


            var model2_resource = Model2.CreateResource(new Uri("http://example.org/MyResource"));
            model2_resource.AddProperty(property, "in the jungle");
            model2_resource.AddProperty(property, 123);
            model2_resource.AddProperty(property, DateTime.Now);
            model2_resource.Commit();

            var model2_resource2 = Model2.CreateResource(new Uri("ex:Resource"));
            model2_resource2.AddProperty(property, "in the jungle");
            model2_resource2.AddProperty(property, 123);
            model2_resource2.AddProperty(property, DateTime.Now);
            model2_resource2.Commit();
        }

        [TearDown]
        public new void TearDown()
        {
            Model.Clear();
            Model2.Clear();
            Store.Dispose();
        }

        public class Contact : Resource
        {
            // Type Mapping
            public override IEnumerable<Class> GetTypes()
            {
                return new List<Class> { nco.Contact };
            }

            protected PropertyMapping<string> FullnameProperty =
                   new PropertyMapping<string>("Fullname", nco.fullname);

            public string Fullname
            {
                get { return GetValue(FullnameProperty); }
                set { SetValue(FullnameProperty, value); }
            }

            protected PropertyMapping<DateTime> BirthdayProperty =
                   new PropertyMapping<DateTime>("Birthday", nco.birthDate);
            public DateTime Birthday
            {
                get { return GetValue(BirthdayProperty); }
                set { SetValue(BirthdayProperty, value); }
            }

            public Contact(Uri uri) : base(uri) { }
        }

        [Test]
        public void ModelNameTest()
        {
            var modelUri = new Uri("http://www.example.com");
            var modelUri2 = new Uri("http://www.example.com/");

            var m1 = Store.GetModel(modelUri);
            m1.Clear();

            Assert.IsTrue(m1.IsEmpty);

            var m2 = Store.GetModel(modelUri2);

            Assert.IsTrue(m2.IsEmpty);
            
            var c = m1.CreateResource<PersonContact>(new Uri("http://www.example.com/testResource"));
            c.NameFamily = "Doe";
            c.Commit();

            Assert.IsFalse(m1.IsEmpty);
            Assert.IsFalse(m2.IsEmpty);

            m1.Clear();

            Assert.IsTrue(m1.IsEmpty);
            Assert.IsTrue(m2.IsEmpty);

        }

        [Test]
        public void ContainsResourceTest()
        {
            Assert.IsTrue(Model.ContainsResource(new Uri("http://example.org/MyResource")));
            Assert.IsTrue(Model.ContainsResource(new Uri("ex:Resource")));
            Assert.IsTrue(Model2.ContainsResource(new Uri("http://example.org/MyResource")));
            Assert.IsTrue(Model2.ContainsResource(new Uri("ex:Resource")));
        }
        
        [Test]
        public void CreateResourceTest()
        {
            Assert.IsTrue(Model.ContainsResource(new Uri("http://example.org/MyResource")));
        }

        [Test]
        public void CreateEmptyResourceTest()
        {
            var res = Model.CreateResource(new Uri("http://semiodesk.com/emptyResource"));
            res.Commit();
        }

        [Test]
        public void DeleteResourceTest()
        {
            var uri0 = new Uri("http://example.org/MyResource");
            var uri1 = new Uri("http://example.org/MyResource1");

            Assert.IsTrue(Model.ContainsResource(uri0));

            Model.DeleteResource(uri0);

            Assert.IsFalse(Model.ContainsResource(uri0));

            var p0 = new Property(new Uri("http://example.org/MyProperty"));
            var p1 = new Property(new Uri("http://example.org/MyProperty1"));

            var r0 = Model.CreateResource(uri0);
            r0.AddProperty(p0, "in the jungle");
            r0.AddProperty(p0, 123);
            r0.Commit();

            var r1 = Model.CreateResource(uri1);
            r1.AddProperty(p0, 123);
            r1.AddProperty(p1, r0);
            r1.Commit();

            Assert.IsTrue(Model.ContainsResource(r0));
            Assert.IsTrue(Model.ContainsResource(r1));

            Model.DeleteResource(r0);

            Assert.IsFalse(Model.ContainsResource(r0));
            Assert.IsTrue(Model.ContainsResource(r1));

            // Update the resource from the model.
            r1 = Model.GetResource(uri1);

            Assert.IsTrue(r1.HasProperty(p0, 123));
            Assert.IsFalse(r1.HasProperty(p1, r0));
        }

        [Test]
        public void DeleteResourcesTest()
        {
            var uri0 = new Uri("http://example.org/MyResource");
            var uri1 = new Uri("http://example.org/MyResource1");
            var p0 = new Property(new Uri("http://example.org/MyProperty"));
            var p1 = new Property(new Uri("http://example.org/MyProperty1"));


            var r1 = Model.CreateResource(uri1);
            r1.AddProperty(p0, 123);
            r1.AddProperty(p1, new Resource(uri0));
            r1.Commit();

            Assert.IsTrue(Model.ContainsResource(uri0));
            Assert.IsTrue(Model.ContainsResource(uri1));

            r1 = Model.GetResource(uri1);
            var r0 = Model.GetResource(uri0);

            Model.DeleteResources(null, r0, r1);

            Assert.IsFalse(Model.ContainsResource(uri0));
            Assert.IsFalse(Model.ContainsResource(uri1));
        }


        [Test]
        public void DeleteResourcesByUrisTest()
        {
            var uri0 = new Uri("http://example.org/MyResource");
            var uri1 = new Uri("http://example.org/MyResource1");
            var p0 = new Property(new Uri("http://example.org/MyProperty"));
            var p1 = new Property(new Uri("http://example.org/MyProperty1"));


            var r1 = Model.CreateResource(uri1);
            r1.AddProperty(p0, 123);
            r1.AddProperty(p1, new Resource(uri0));
            r1.Commit();

            Assert.IsTrue(Model.ContainsResource(uri0));
            Assert.IsTrue(Model.ContainsResource(uri1));

            Model.DeleteResources(new Uri[] { uri0, uri1 });

            Assert.IsFalse(Model.ContainsResource(uri0));
            Assert.IsFalse(Model.ContainsResource(uri1));
        }

        [Test]
        public void GetResourceWithBlankIdTest()
        {
            Model.Clear();

            var p = new Property(new Uri("http://example.org/MyProperty"));

            var x = Model.CreateResource(new BlankId());
            x.AddProperty(p, 123);
            x.Commit();

            Assert.Throws<ArgumentException>(() => Model.GetResource<Resource>(x.Uri));
        }

        [Test]
        public void GetResourceWithBlankIdPropertyTest()
        {
            Model.Clear();

            var label = new Property(new UriRef("ex:label"));
            var related = new Property(new UriRef("ex:related"));

            var r0 = Model.CreateResource(new UriRef("_:0", true));
            r0.AddProperty(label, "0");
            r0.Commit();

            var r1 = Model.CreateResource(new UriRef("_:1", true));
            r0.AddProperty(label, "1");
            r1.AddProperty(related, r0);
            r1.Commit();

            Assert.Throws<ArgumentException>(() => Model.ContainsResource(r1.Uri));
            Assert.Throws<ArgumentException>(() => Model.GetResource(r1.Uri));
            Assert.Throws<ArgumentException>(() => Model.GetResource(r1));

            var resources = Model.GetResources<Resource>().ToArray();

            Assert.AreEqual(2, resources.Length);

            foreach (var r in resources)
            {
                Assert.IsTrue(r.Uri.IsBlankId);

                foreach(var x in r.ListValues(related).OfType<Resource>())
                {
                    Assert.IsTrue(x.Uri.IsBlankId);
                }
            }
        }

        [Test]
        public void GetResourceTest()
        {
            var hans = Model.GetResource(new Uri("http://example.org/MyResource"));
            Assert.NotNull(hans);
            Assert.NotNull(hans.Model);

            hans = Model.GetResource<Resource>(new Uri("http://example.org/MyResource"));
            Assert.NotNull(hans);
            Assert.NotNull(hans.Model);

            hans = Model.GetResource(new Uri("http://example.org/MyResource"), typeof(Resource)) as Resource;
            Assert.NotNull(hans);
            Assert.NotNull(hans.Model);

            try
            {
                Model.GetResource<Resource>(new Uri("http://example.org/None"));

                Assert.Fail();
            }
            catch(ArgumentException)
            {
            }
        }

        [Test]
        public void GetResourcesTest()
        {
            var query = new SparqlQuery("DESCRIBE <http://example.org/MyResource>");

            var resources = new List<Resource>();
            resources.AddRange(Model.GetResources(query));

            Assert.Greater(resources.Count, 0);

            foreach (IResource res in resources)
            {
                Assert.NotNull(res.Model);
            }
        }

        [Test]
        public void UpdateResourceTest()
        {
            var property = new Property(new Uri("http://example.org/MyProperty"));

            var resourceUri = new Uri("http://example.org/MyResource");

            var resource = Model.GetResource(resourceUri);
            resource.RemoveProperty(property, 123);
            resource.Commit();

            var actual = Model.GetResource(resourceUri);

            Assert.AreEqual(resource, actual);

            actual = Model.GetResource<Resource>(resourceUri);

            Assert.AreEqual(resource, actual);

            // Try to update resource with different properties then persisted
            var r2 = new Resource(resourceUri);
            r2.AddProperty(property, "in the jengle");

            r2.Model = Model;
            r2.Commit();
            actual = Model.GetResource<Resource>(resourceUri);
            Assert.AreEqual(r2, actual);
        }

        [Test]
        public void DateTimeResourceTest()
        {
            var resUri = new Uri("http://example.org/DateTimeTest");
            var res = Model.CreateResource(resUri);

            var property = new Property(new Uri("http://example.org/MyProperty"));

            var t = new DateTime();
            Assert.IsTrue(DateTime.TryParse("2013-01-21T16:27:23.000Z", out t));

            res.AddProperty(property, t);
            res.Commit();

            var actual = Model.GetResource(resUri);
            var o = actual.GetValue(property);
            Assert.AreEqual(typeof(DateTime), o.GetType());
            var actualDateTime = (DateTime)actual.GetValue(property);

            Assert.AreEqual(t.ToUniversalTime(), actualDateTime.ToUniversalTime());
        }

        [Test]
        public void TimeSpanResourceTest()
        {
            var resUri = new Uri("http://example.org/DateTimeTest");
            var res = Model.CreateResource(resUri);

            var property = new Property(new Uri("http://example.org/MyProperty"));

            var t = TimeSpan.FromMinutes(5);

            res.AddProperty(property, t);
            res.Commit();

            var actual = Model.GetResource(resUri);
            var o = actual.GetValue(property);
            Assert.AreEqual(typeof(TimeSpan), o.GetType());
            var actualDateTime = (TimeSpan)actual.GetValue(property);

            Assert.AreEqual(t.TotalMinutes, actualDateTime.TotalMinutes);
        }

        [Test]
        public void LiteralWithHyphenTest()
        {
            Model.Clear();

            var property = new Property(new Uri("http://example.org/MyProperty"));

            var model2_resource2 = Model.CreateResource(new Uri("ex:Resource"));
            model2_resource2.AddProperty(property, "\"in the jungle\"");
            model2_resource2.Commit();

            var r = Model.GetResource(new Uri("ex:Resource"));
            var o = r.GetValue(property);
            Assert.AreEqual(typeof(string), o.GetType());
            Assert.AreEqual("\"in the jungle\"", o);
        }

        [Test]
        public void LiteralWithLangTagTest()
        {
            Model.Clear();

            var property = new Property(new Uri("http://example.org/MyProperty"));

            var model2_resource2 = Model.CreateResource(new Uri("ex:Resource"));
            model2_resource2.AddProperty(property, "in the jungle", "EN");
            model2_resource2.Commit();

            var r = Model.GetResource(new Uri("ex:Resource"));
            var o = r.GetValue(property);

            Assert.AreEqual(typeof(Tuple<string, CultureInfo>), o.GetType());

            var val = o as Tuple<string, CultureInfo>;

            Assert.AreEqual("in the jungle", val.Item1);
        }

        [Test]
        public void LiteralWithNewLineTest()
        {
            Model.Clear();

            var p0 = new Property(new Uri("http://example.org/MyProperty"));

            var r0 = Model.CreateResource(new Uri("ex:Resource"));
            r0.AddProperty(p0, "in the\n jungle");
            r0.Commit();

            r0 = Model.GetResource(new Uri("ex:Resource"));

            var o = r0.GetValue(p0);

            Assert.AreEqual(typeof(string), o.GetType());
            Assert.AreEqual("in the\n jungle", o);
        }

        [Test]
        public void AddResourceTest()
        {
            var uriResource = new Uri("http://example.org/AddResourceTest");
            IResource resource = new Resource (uriResource);

            var property = new Property(new Uri("http://example.org/MyProperty"));
            resource.AddProperty(property, "in the jungle");
            resource.AddProperty(property, 123);
            resource.AddProperty(property, DateTime.Now);

            Model.AddResource(resource);

            var actual = Model.GetResource(uriResource);

            Assert.AreEqual(uriResource, uriResource);
            Assert.AreEqual(resource.ListValues(property).Count(), actual.ListValues(property).Count());


            uriResource = new Uri("http://example.org/AddResourceTest2");
            var contact = new Contact(uriResource);
            contact.Fullname = "Peter";

            Model.AddResource<Contact>(contact);

            var actualContact = Model.GetResource<Contact>(uriResource);

            Assert.AreEqual(uriResource, uriResource);
            Assert.AreEqual(contact.Fullname, actualContact.Fullname);
        }

        [Test]
        public void GetTypedResourcesTest()
        {
            var uriResource = new Uri("http://example.org/Peter");
            var contact = Model.CreateResource<Contact>(uriResource);
            contact.Fullname = "Peter";
            contact.Commit();

            uriResource = new Uri("http://example.org/Hans");
            var contact2 = Model.CreateResource<Contact>(uriResource);
            contact2.Fullname = "Hans";
            contact2.Commit();

            var r = Model.GetResources<Contact>();

            Assert.AreEqual(2, r.Count());
            Assert.IsTrue(r.Contains(contact));
            Assert.IsTrue(r.Contains(contact2));

            Model.Clear();

            var personContact = Model.CreateResource<PersonContact>(uriResource);
            personContact.Fullname = "Peter";
            personContact.Commit();

            r = Model.GetResources<Contact>();
            Assert.AreEqual(0, r.Count());

            r = Model.GetResources<Contact>(true);
            Assert.AreEqual(1, r.Count());

            var x = Model.GetResource(uriResource);

            Assert.AreEqual(typeof(PersonContact), x.GetType());
        }

        [Test]
        public void WriteTest()
        {
            Model.Clear();

            var property = new Property(new Uri("http://example.org/MyProperty"));

            var model2_resource2 = Model.CreateResource(new Uri("ex:Resource"));
            model2_resource2.AddProperty(property, "in the\n jungle");
            model2_resource2.Commit();

            var wr = new MemoryStream();
            Model.Write(wr, RdfSerializationFormat.RdfXml);
            var myString = Encoding.UTF8.GetString(wr.ToArray());
        }

        [Test]
        public void WriteWithBaseUriTest()
        {
            Model.Clear();

            var r = Model.CreateResource(new Uri("http://example.org/test"));
            r.AddProperty(new Property(new Uri("http://example.org/name")), "test");
            r.Commit();

            using (var stream = new MemoryStream())
            {
                Model.Write(stream, RdfSerializationFormat.Turtle, null, new Uri("http://example.org/"), true);

                stream.Seek(0, SeekOrigin.Begin);

                var result = Encoding.UTF8.GetString(stream.ToArray());

                Assert.IsFalse(string.IsNullOrEmpty(result));
                Assert.IsTrue(result.StartsWith("@base <http://example.org/>"));
            }
        }

        [Test]
        public void ReadTest()
        {
            Model.Clear();

            var fi = new FileInfo("Models\\test-ntriples.nt");
            var fileUri = fi.ToUriRef();

            Assert.IsTrue(Model.IsEmpty);
            Assert.IsTrue(Model.Read(fileUri, RdfSerializationFormat.NTriples, false));
            Assert.IsFalse(Model.IsEmpty);

            Model.Clear();

            Assert.IsTrue(Model.IsEmpty);
            Assert.IsTrue(Model.Read(new Uri("http://www.w3.org/1999/02/22-rdf-syntax-ns#"), RdfSerializationFormat.RdfXml, false));
            Assert.IsFalse(Model.IsEmpty);

            Model.Clear();

            fi = new FileInfo("Models\\test-tmo.trig");
            fileUri = fi.ToUriRef();

            Assert.IsTrue(Model.IsEmpty);
            Assert.Throws(typeof(ArgumentException), () => { Model.Read(fileUri, RdfSerializationFormat.Trig, false); });

        }

        [Test]
        public void ReadFromStringTest()
        {
            Model.Clear();

            var turtle = @"@base <http://example.org/> .
@prefix rdf: <http://www.w3.org/1999/02/22-rdf-syntax-ns#> .
@prefix rdfs: <http://www.w3.org/2000/01/rdf-schema#> .
@prefix foaf: <http://xmlns.com/foaf/0.1/> .
@prefix rel: <http://www.perceive.net/schemas/relationship/> .

<#green-goblin>
    rel:enemyOf <#spiderman> ;
    a foaf:Person ;    # in the context of the Marvel universe
    foaf:name ""Green Goblin"" .
<#spiderman>
    rel:enemyOf <#green-goblin> ;
    a foaf:Person ;
    foaf:name ""Spiderman"", ""Человек-паук""@ru .";

            using (var s = GenerateStreamFromString(turtle))
            {
                Assert.IsTrue(Model.Read(s, RdfSerializationFormat.Turtle, false));
            }

            var r = Model.GetResource(new Uri("http://example.org/#green-goblin"));
            var name = r.GetValue(new Property(new Uri("http://xmlns.com/foaf/0.1/name"))) as string;
            Assert.AreEqual("Green Goblin", name);

            var turtle2 = @"@base <http://example.org/> .
@prefix xsd: <http://www.w3.org/2001/XMLSchema#> .
@prefix foaf: <http://xmlns.com/foaf/0.1/> .


<#green-goblin> foaf:age ""27""^^xsd:int .";

            using (var s = GenerateStreamFromString(turtle2))
            {
                Assert.IsTrue(Model.Read(s, RdfSerializationFormat.Turtle, true));
            }

            r = Model.GetResource(new Uri("http://example.org/#green-goblin"));
            var age = (int)r.GetValue(new Property(new Uri("http://xmlns.com/foaf/0.1/age")));
            name = r.GetValue(new Property(new Uri("http://xmlns.com/foaf/0.1/name"))) as string;
            Assert.AreEqual(27, age);

            turtle = @"@base <http://example.org/> .
@prefix rdf: <http://www.w3.org/1999/02/22-rdf-syntax-ns#> .
@prefix rdfs: <http://www.w3.org/2000/01/rdf-schema#> .
@prefix foaf: <http://xmlns.com/foaf/0.1/> .
@prefix rel: <http://www.perceive.net/schemas/relationship/> .

<#green-goblin>
    rel:enemyOf <#spiderman> ;
    a foaf:Person ;    # in the context of the Marvel universe
    foaf:name ""Green Gobo"" .
<#spiderman>
    rel:enemyOf <#green-goblin> ;
    a foaf:Person ;
    foaf:name ""Spiderman"", ""Человек-паук""@ru .";

            using (var s = GenerateStreamFromString(turtle))
            {
                Assert.IsTrue(Model.Read(s, RdfSerializationFormat.Turtle, false));
            }

            r = Model.GetResource(new Uri("http://example.org/#green-goblin"));
            name = r.GetValue(new Property(new Uri("http://xmlns.com/foaf/0.1/name"))) as string;
            Assert.AreEqual("Green Gobo", name);
        }

        [Test]
        public void WriteToStringTest()
        {
            Model.Clear();

            
            var r = Model.CreateResource(new Uri("http://example.org/test"));
            r.AddProperty(new Property(new Uri("http://xmlns.com/foaf/0.1/name")), "test");
            r.Commit();
            var stream = new MemoryStream();

            Model.Write(stream, RdfSerializationFormat.Turtle, null, null, true);

            stream.Seek(0, SeekOrigin.Begin);
            var res = Encoding.UTF8.GetString(stream.ToArray());

            Assert.IsFalse(string.IsNullOrEmpty(res));

        }


        [Test]
        public void TestAddMultipleResources()
        {
            Assert.Inconclusive("This test should work, it just takes too long.");
            Model.Clear();
            for (var j = 1; j < 7; j++)
            {
                for (var i = 1; i < 1000; i++)
                {
                    using (var pers = Model.CreateResource<PersonContact>())
                    {
                        pers.Fullname = string.Format("Name {0}", i * j);
                        pers.Commit();
                    }
                }
                

            }
        }

        public Stream GenerateStreamFromString(string s)
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(s);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }


    }

    
}
