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


using Semiodesk.Trinity;
using System;
using System.IO;
using System.Linq;
using System.Text;
using Semiodesk.Trinity.Tests.dotnetrdf;
using Xunit;

namespace dotNetRDFStore.Test
{
    public class ModelTest: IDisposable
    {
        IStore Store;

        IModel Model;

        public ModelTest()
        {
            Store = StoreFactory.CreateMemoryStore();
            Model = Store.CreateModel(new Uri("ex:Test"));
        }
        
        public void Dispose()
        {
            Store.Dispose();
            Store = null;
        }

        [Fact]
        public void DeleteResourceTest()
        {
            var uri0 = new Uri("http://example.org/MyResource");
            var uri1 = new Uri("http://example.org/MyResource1");
            var p0 = new Property(new Uri("http://example.org/MyProperty"));
            var p1 = new Property(new Uri("http://example.org/MyProperty1"));

            var model_resource = Model.CreateResource(uri0);

            model_resource.AddProperty(p0, "in the jungle");
            model_resource.AddProperty(p0, 123);
            model_resource.AddProperty(p0, DateTime.Now);
            model_resource.Commit();

            Assert.True(Model.ContainsResource(uri0));

            Model.DeleteResource(uri0);

            Assert.False(Model.ContainsResource(uri0));


            var r0 = Model.CreateResource(uri0);
            r0.AddProperty(p0, "in the jungle");
            r0.AddProperty(p0, 123);
            r0.Commit();

            var r1 = Model.CreateResource(uri1);
            r1.AddProperty(p0, 123);
            r1.AddProperty(p1, r0);
            r1.Commit();

            Assert.True(Model.ContainsResource(r0));
            Assert.True(Model.ContainsResource(r1));

            Model.DeleteResource(r0);

            Assert.False(Model.ContainsResource(r0));
            Assert.True(Model.ContainsResource(r1));

            // Update the resource from the model.
            r1 = Model.GetResource(uri1);

            Assert.True(r1.HasProperty(p0, 123));
            Assert.False(r1.HasProperty(p1, r0));
        }

        [Fact]
        public void DeleteResourcesTest()
        {
            var uri0 = new Uri("http://example.org/MyResource");
            var uri1 = new Uri("http://example.org/MyResource1");
            var p0 = new Property(new Uri("http://example.org/MyProperty"));
            var p1 = new Property(new Uri("http://example.org/MyProperty1"));


            var model_resource = Model.CreateResource(uri0);

            model_resource.AddProperty(p0, "in the jungle");
            model_resource.AddProperty(p0, 123);
            model_resource.AddProperty(p0, DateTime.Now);
            model_resource.Commit();

            var r1 = Model.CreateResource(uri1);
            r1.AddProperty(p0, 123);
            r1.AddProperty(p1, new Resource(uri0));
            r1.Commit();

            Assert.True(Model.ContainsResource(uri0));
            Assert.True(Model.ContainsResource(uri1));

            r1 = Model.GetResource(uri1);
            var r0 = Model.GetResource(uri0);

            Model.DeleteResources(null, r0, r1);

            Assert.False(Model.ContainsResource(uri0));
            Assert.False(Model.ContainsResource(uri1));
        }

        [Fact]
        public void CreateResourceTest()
        {
            var literal = "var";
            var resourceUri = new Uri("ex:test:resource");
            var property = new Property(new Uri("ex:test:property"));
            var res = Model.CreateResource(resourceUri);
            res.AddProperty(property, literal);
            res.Commit();

            var result = Model.GetResource(resourceUri);
            Assert.Equal(resourceUri, result.Uri);
            var properties = result.ListProperties().ToList();
            Assert.Single(properties);
            Assert.Equal(property, properties[0]);
            Assert.Equal(literal, result.GetValue(property));
        }

        [Fact]
        public void CreateResourceWithBlankIdTest()
        {
            var label = new Property(new UriRef("ex:label"));

            var r0 = Model.CreateResource(new UriRef("_:0", true));
            r0.AddProperty(label, "0");
            r0.Commit();

            var r1 = Model.CreateResource(new UriRef("_:1", true));
            r1.AddProperty(label, "1");
            r1.Commit();

            Assert.False(Model.IsEmpty);
            Assert.Throws<ArgumentException>(() => Model.ContainsResource(r1.Uri));
            Assert.Throws<ArgumentException>(() => Model.GetResource(r1));
        }

        [Fact]
        public void ModifyResourceTest()
        {
            var literal = "var";
            var resourceUri = new Uri("ex:test:resource");
            var property = new Property(new Uri("ex:test:property"));
            var res = Model.CreateResource(resourceUri);
            res.AddProperty(property, literal);
            res.Commit();

            var result = Model.GetResource(resourceUri);
            
            result.RemoveProperty(property, literal);
            literal = "var2";
            result.AddProperty(property, literal);
            result.Commit();

            result = Model.GetResource(resourceUri);

            Assert.Equal(resourceUri, result.Uri);
            var properties = result.ListProperties().ToList();
            Assert.Single(properties);
            Assert.Equal(property, properties[0]);
            Assert.Equal(literal, result.GetValue(property));

        }

        [Fact]
        public void RemoveResourceTest()
        {
            var literal = "var";
            var resourceUri = new Uri("ex:test:resource");
            var property = new Property(new Uri("ex:test:property"));
            var res = Model.CreateResource(resourceUri);
            res.AddProperty(property, literal);
            res.Commit();

            var result = Model.GetResource(resourceUri);

            Model.DeleteResource(result);

            Assert.False(Model.ContainsResource(result));

        }

        [Fact]
        public void GetResourceTest()
        {
            var literal = "var";
            var resourceUri = new Uri("ex:test:resource");
            var property = new Property(new Uri("ex:test:property"));
            var res = Model.CreateResource(resourceUri);
            res.AddProperty(property, literal);
            res.Commit();

            var result = Model.GetResource(resourceUri);
            Assert.Equal(resourceUri, result.Uri);
            var properties = result.ListProperties().ToList();
            Assert.Single(properties);
            Assert.Equal(property, properties[0]);
            Assert.Equal(literal, result.GetValue(property));
        }

        [Fact]
        public void GetResourcesTest()
        {
            var resourceUri1 = new Uri("ex:test:resource1");
            var property = new Property(new Uri("ex:test:property"));
            var res = Model.CreateResource(resourceUri1);
            res.AddProperty(property, "lit1");
            res.Commit();

            var resourceUri2 = new Uri("ex:test:resource2");
            res = Model.CreateResource(resourceUri2);
            res.AddProperty(property, "lit2");
            res.Commit();

            var result = Model.GetResources(new[] { resourceUri1, resourceUri2 }, typeof(Resource)).ToList();
            Assert.Equal(2, result.Count);

            var res1 = result[0] as IResource;
            Assert.Equal(resourceUri1, res1.Uri);
            var properties = res1.ListProperties().ToList();
            Assert.Single(properties);
            Assert.Equal(property, properties[0]);
            Assert.Equal("lit1", res1.GetValue(property));

            var res2 = result[1] as IResource;
            Assert.Equal(resourceUri2, res2.Uri);
            properties = res2.ListProperties().ToList();
            Assert.Single( properties);
            Assert.Equal(property, properties[0]);
            Assert.Equal("lit2", res2.GetValue(property));

        }

        [Fact]
        public void GetResourceFromJsonLD()
        {
            const string str = "{\"http://www.w3.org/1999/02/22-rdf-syntax-ns#type\":[{\"@id\":\"http://schema.org/Organization\"},{}],\"http://www.w3.org/2000/01/rdf-schema#label\":\"\",\"http://schema.org/name\":[{\"@value\":\"My Project\",\"@language\":\"en\"}],\"http://schema.org/alternateName\":[],\"http://schema.org/description\":[{\"@value\":\"Hello\",\"@language\":\"en\"}],\"http://schema.org/image\":[],\"http://schema.org/thumbnail\":[],\"http://schema.org/sameAs\":[],\"http://www.w3.org/ns/prov#\":[],\"http://schema.org/identifier\":\"my-new-project\"}\"";


            using (var stream = new MemoryStream())
            {
                var writer = new StreamWriter(stream);
                writer.Write(str);
                writer.Flush();
                stream.Position = 0;
                var store = StoreFactory.CreateMemoryStore();
                var modelUri = new Uri("urn:modom:default");
                store.Read(stream, modelUri, RdfSerializationFormat.JsonLd, true);

                var model = store.GetModel(modelUri);
                var res = model.GetResources<Resource>().ToList();

            }
        }

        [Fact]
        public void GetResourceWithBlankIdTest()
        {
            Model.Clear();

            var p = new Property(new Uri("http://example.org/MyProperty"));

            var x = Model.CreateResource(new BlankId());
            x.AddProperty(p, 123);
            x.Commit();

            Assert.Throws<ArgumentException>(() => Model.GetResource<Resource>(x.Uri));
        }

        [Fact]
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

            Assert.Equal(2, resources.Length);

            foreach (var r in resources)
            {
                Assert.True(r.Uri.IsBlankId);

                foreach(var x in r.ListValues(related).OfType<Resource>())
                {
                    Assert.True(x.Uri.IsBlankId);
                }
            }
        }

        [Fact]
        public void GetResourcesEmptyTest()
        {
            var resourceUri1 = new Uri("ex:test:resource1");
            var property = new Property(new Uri("ex:test:property"));
            var res = Model.CreateResource(resourceUri1);
            res.AddProperty(property, "lit1");
            res.Commit();

            var resourceUri2 = new Uri("ex:test:resource2");
            res = Model.CreateResource(resourceUri2);
            res.AddProperty(property, "lit2");
            res.Commit();

            var result = Model.GetResources(new Uri[] {}, typeof(Resource)).ToList();
            Assert.Equal(2, result.Count);

            var res1 = result[0] as IResource;
            Assert.Equal(resourceUri1, res1.Uri);
            var properties = res1.ListProperties().ToList();
            Assert.Equal(1, properties.Count);
            Assert.Equal(property, properties[0]);
            Assert.Equal("lit1", res1.GetValue(property));

            var res2 = result[1] as IResource;
            Assert.Equal(resourceUri2, res2.Uri);
            properties = res2.ListProperties().ToList();
            Assert.Equal(1, properties.Count);
            Assert.Equal(property, properties[0]);
            Assert.Equal("lit2", res2.GetValue(property));

        }

        [Fact]
        public void UpdateResourceTest()
        {
            var property = new Property(new Uri("http://example.org/MyProperty"));
            var resourceUri = new Uri("http://example.org/MyResource");
            var resource = Model.CreateResource(resourceUri);
            resource.AddProperty(property, 123);
            resource.AddProperty(property, "in the jungle");
            resource.Commit();

            // Try to update resource with different properties then persisted
            var r2 = new Resource(resourceUri);
            r2.AddProperty(property, "in the jengle");

            r2.Model = Model;
            r2.Commit();
            var actual = Model.GetResource<Resource>(resourceUri);
            Assert.Equal(r2, actual);


            // Try to update resource without properties
            var r3 = new Resource(resourceUri);

            r3.Model = Model;
            r3.Commit();
            actual = Model.GetResource<Resource>(resourceUri);
            Assert.Equal(r3, actual);
        }

        [Fact]
        public void UpdateResourcesTest()
        {
            var intProperty = new Property(new Uri("http://example.org/int"));
            var stringProperty = new Property(new Uri("http://example.org/string"));
            var r1Uri = new Uri("http://example.org/r1");
            var r1 = Model.CreateResource<Resource>(r1Uri);
            r1.AddProperty(intProperty, 123);
            r1.AddProperty(stringProperty, "in the jungle");
            var r2Uri = new Uri("http://example.org/r2");
            var r2 = Model.CreateResource<Resource>(r2Uri);
            r2.AddProperty(intProperty, 111);
            r2.AddProperty(stringProperty, "in the jingle");

            var r3Uri = new Uri("http://example.org/r3");
            var r3 = Model.CreateResource<Resource>(r3Uri);
            r3.AddProperty(intProperty, 333);
            r3.AddProperty(stringProperty, "in the jongle");

            Model.UpdateResources(null, r1, r2, r3);

            var actual = Model.GetResources<Resource>().ToList();
            Assert.Contains(r1, actual);
            Assert.Contains(r2, actual);
            Assert.Contains(r3, actual);
            var r3Actual = actual.Where(x => x.Uri == r3Uri).FirstOrDefault();
            var r3ActualIntProp = (int)r3Actual.GetValue(intProperty);
            Assert.Equal(333, r3ActualIntProp);

            r1.RemoveProperty(intProperty, 123);
            r1.AddProperty(intProperty, 154);

            r2.RemoveProperty(stringProperty, "in the jingle");
            r2.AddProperty(stringProperty, "boo");

            r3.AddProperty(stringProperty, "hooo");
            Model.UpdateResources(null, r1, r2, r3);

            actual = Model.GetResources<Resource>().ToList();
            Assert.Contains(r1, actual);
            Assert.Contains(r2, actual);
            Assert.Contains(r3, actual);
            var r1Actual = actual.Where(x => x.Uri == r1Uri).FirstOrDefault();
            var r1ActualIntProp = (int)r1Actual.GetValue(intProperty);
            Assert.Equal(154, r1ActualIntProp);

        }

        [Fact]
        public void ContainsResourceTest()
        {
            var literal = "var";
            var resourceUri = new Uri("ex:test:resource");
            var property = new Property(new Uri("ex:test:property"));
            var res = Model.CreateResource(resourceUri);
            res.AddProperty(property, literal);
            res.Commit();

            var result = Model.GetResource(resourceUri);
            Assert.Equal(true, Model.ContainsResource(new UriRef("ex:test:resource")));
        }

        [Fact]
        public void AskQueryTest()
        {
            var literal = "var";
            var resourceUri = new Uri("ex:test:resource");
            var property = new Property(new Uri("ex:test:property"));
            var res = Model.CreateResource(resourceUri);
            res.AddProperty(property, literal);
            res.Commit();

            var q = new SparqlQuery("ASK { <ex:test:resource> ?p ?o . }");
            var b = Model.ExecuteQuery(q);
        }

        [Fact]
        public void SparqlQueryTest()
        {

        }

        [Fact]
        public void ReadFromStringTest()
        {
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

            using( var s = GenerateStreamFromString(turtle))
            {
                Assert.True(Model.Read(s, RdfSerializationFormat.Turtle, false));
            }

            var r = Model.GetResource(new Uri("http://example.org/#green-goblin"));
            var name = r.GetValue(new Property(new Uri("http://xmlns.com/foaf/0.1/name"))) as string;
            Assert.Equal("Green Goblin", name);

            var turtle2 = @"@base <http://example.org/> .
@prefix xsd: <http://www.w3.org/2001/XMLSchema#> .
@prefix foaf: <http://xmlns.com/foaf/0.1/> .


<#green-goblin> foaf:age ""27""^^xsd:int .";

            using (var s = GenerateStreamFromString(turtle2))
            {
                Assert.True(Model.Read(s, RdfSerializationFormat.Turtle, true));
            }

            r = Model.GetResource(new Uri("http://example.org/#green-goblin"));
            var age = (int) r.GetValue(new Property(new Uri("http://xmlns.com/foaf/0.1/age")));
            Assert.Equal(27, age);

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
                Assert.True(Model.Read(s, RdfSerializationFormat.Turtle, false));
            }

            r = Model.GetResource(new Uri("http://example.org/#green-goblin"));
            name = r.GetValue(new Property(new Uri("http://xmlns.com/foaf/0.1/name"))) as string;
            Assert.Equal("Green Gobo", name);
        }

        [Fact]
        public void ReadLocalizedFromStringTest()
        {
            var turtle = @"@base <http://example.org/> .
@prefix rdf: <http://www.w3.org/1999/02/22-rdf-syntax-ns#> .
@prefix rdfs: <http://www.w3.org/2000/01/rdf-schema#> .
@prefix foaf: <http://xmlns.com/foaf/0.1/> .


<#spiderman> a foaf:Person ;
    foaf:name ""Spiderman"", ""Человек-паук""@ru .";

            using (var s = GenerateStreamFromString(turtle))
            {
                Assert.True(Model.Read(s, RdfSerializationFormat.Turtle, false));
            }

            var r = Model.GetResource(new Uri("http://example.org/#spiderman"));
            var name = r.GetValue(new Property(new Uri("http://xmlns.com/foaf/0.1/name"))) as string;

          
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

        [Fact]
        public void WriteTest()
        {
            Model.Clear();

            INamespaceMap namespaces = new NamespaceMap()
            {
                { "ex2", new Uri("http://example.org") }
            };

            var property = new Property(new Uri("http://example.org/MyProperty"));

            var model2_resource2 = Model.CreateResource(new Uri("ex:Resource"));
            model2_resource2.AddProperty(property, "in the\n jungle");
            model2_resource2.Commit();

            using (var wr = new MemoryStream())
            {
                Model.Write(wr, RdfSerializationFormat.RdfXml, namespaces, null, true);

                var result = Encoding.UTF8.GetString(wr.ToArray());
            }
        }

        [Fact]
        public void WriteWithWriterTest()
        {
            Model.Clear();

            var p0 = new Property(new Uri("http://example.org/property"));

            var r0 = Model.CreateResource(new Uri("http://example.org/r0"));
            r0.AddProperty(p0, 0);
            r0.AddProperty(p0, 1);
            r0.AddProperty(p0, 2);
            r0.Commit();

            using (var stream = new MemoryStream())
            {
                var writer = new TestFormatWriter();

                Model.Write(stream, writer, true);

                stream.Position = 0;

                var n = 0;

                using (var reader = new StreamReader(stream))
                {
                    string line;

                    while ((line = reader.ReadLine()) != null)
                    {
                        var triple = line.Split(' ');

                        Assert.Equal(r0.Uri.AbsoluteUri, triple[0]);
                        Assert.Equal(p0.Uri.AbsoluteUri, triple[1]);
                        Assert.True(triple[2].StartsWith(n.ToString()));

                        n++;
                    }
                }

                Assert.Equal(3, n);
            }
        }

        [Fact]
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

                Assert.False(string.IsNullOrEmpty(result));
                Assert.StartsWith("@base <http://example.org/>", result);
            }
        }
    }
}
