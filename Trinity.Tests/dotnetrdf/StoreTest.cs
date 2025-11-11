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
using Semiodesk.Trinity.Ontologies;
using System;
using System.IO;
using System.Linq;
using Xunit;

namespace dotNetRDFStore.Test
{
    public class StoreTest : SetupClass, IDisposable
    {
        private IStore _store;

        public StoreTest()
        {
            _store = StoreFactory.CreateStore("provider=dotnetrdf");
        }

        public void Dispose()
        {
            if (_store == null) return;
            _store.Dispose();
            _store = null;
        }

        [Fact]
        public void LoadOntologiesTest()
        {
            _store.InitializeFromConfiguration();

            var models0 = _store.ListModels().ToList();

            // Note: the NCO ontology contains a metadata graph.
            Assert.Equal(8, models0.Count);
        }

        [Fact]
        public void LoadOntologiesFromFileTest()
        {
            var configFile = Path.Combine(Environment.CurrentDirectory, "custom.config");

            _store.InitializeFromConfiguration(configFile);

            Assert.Equal(4, _store.ListModels().Count());

            configFile = Path.Combine(Environment.CurrentDirectory, "nonexistent.config");

            Assert.Throws<FileNotFoundException>(() =>
            {
                _store.InitializeFromConfiguration(configFile);
            });
        }

        [Fact]
        public void LoadOntologiesFromFileWithoutStoreTest()
        {
            var configFile = Path.Combine(Environment.CurrentDirectory, "without_store.config");

            _store.InitializeFromConfiguration(configFile);

            Assert.Equal(4, _store.ListModels().Count());
        }

        [Fact]
        public void AddModelTest()
        {
            var model = _store.CreateModel(new Uri("ex:Test"));

            Assert.NotNull(model);
        }

#pragma warning disable CS0618 // Type or member is obsolete
        [Fact]
        public void ContainsModelTest()
        {
            var testModel = new Uri("ex:Test");

            Assert.False(_store.ContainsModel(testModel));

            var model = _store.CreateModel(testModel);

            var r = model.CreateResource(new Uri("ex:test:resource"));
            r.AddProperty(new Property(new Uri("ex:test:property")), "var");
            r.Commit();

            Assert.True(_store.ContainsModel(testModel));
            Assert.False(_store.ContainsModel(new Uri("ex:NoTest")));
        }
#pragma warning restore CS0618 // Type or member is obsolete

        [Fact]
        public void GetModelTest()
        {
            var testModel = new Uri("ex:Test");

            var model0 = _store.CreateModel(testModel);

            var r = model0.CreateResource(new Uri("ex:test:resource"));
            r.AddProperty(new Property(new Uri("ex:test:property")), "var");
            r.Commit();

            var model1 = _store.GetModel(testModel);

            Assert.Equal(testModel, model1.Uri);
            Assert.True(model1.ContainsResource(r));
        }

        [Fact]
        public void RemoveModelTest()
        {
            var testModel = new Uri("ex:Test");

            var model0 = _store.CreateModel(testModel);

            var res = model0.CreateResource(new Uri("ex:test:resource"));
            res.AddProperty(new Property(new Uri("ex:test:property")), "var");
            res.Commit();

            var model1 = _store.GetModel(testModel);
            Assert.Equal(testModel, model1.Uri);

            _store.RemoveModel(testModel);

            model1 = _store.GetModel(testModel);

            Assert.True(model1.IsEmpty);
        }

        [Fact]
        public void ReadJsonLdContentTest()
        {
            var modelUri = new Uri("http://trinty-rdf.net/models/test/jsonld");

            var model = _store.GetModel(modelUri);

            Assert.True(model.IsEmpty);

            var content = @"
            [
              {
                '@type': ['http://www.w3.org/2002/07/owl#Class'],
                '@id': 'https://ontologies.semanticarts.com/gist/Message',
                'http://www.w3.org/2000/01/rdf-schema#label': 'Message',
                'http://schema.org/name': [{ '@value': 'Message', '@language': 'en' }],
                'http://schema.org/description': [
                  {
                    '@value': 'A specific instance of content sent from an Organization, Person, or Application to at least one other Organization, Person, or Application.',
                    '@language': 'en'
                  }
                ]
              }
            ]
            ";

            _store.Read(content, modelUri, RdfSerializationFormat.JsonLd, false);

            Assert.False(model.IsEmpty);

            var r = model.GetResource(new Uri("https://ontologies.semanticarts.com/gist/Message"));

            Assert.NotNull(r);
            Assert.Equal(r.GetValue(rdfs.label), "Message");
        }
    }
}
