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
using System.Diagnostics;
using System.Linq;
using System.Threading;
using NUnit.Framework;

namespace Semiodesk.Trinity.Test.Virtuoso
{
    //[TestFixture]
    class TransactionTest : SetupClass
    {
        #region Members

        private string _connectionString = SetupClass.ConnectionString;

        private IStore _store;

        private UriRef _model = new UriRef("ex:TransactionTest");

        #endregion

        #region Methods

        [SetUp]
        public void SetUp()
        {
            _store = StoreFactory.CreateStore(_connectionString);

            var model = _store.GetModel(_model);

            if(!model.IsEmpty)
            {
                model.Clear();
            }
        }

        [TearDown]
        public new void TearDown()
        {
            var model = _store.GetModel(_model);

            if (!model.IsEmpty)
            {
                model.Clear();
            }
        }

        public IModel GetModel(out IStore store)
        {
            store = StoreFactory.CreateStore(_connectionString);

            return store.GetModel(_model);
        }

        [Test]
        public void TestAddingElements()
        {
            Assert.Inconclusive();

            var list1 = new List<SingleMappingTestClass>();
            var list2 = new List<SingleMappingTestClass>();

            var sync = new Barrier(3);

            var worker1 = new Thread(() =>
            {
                IStore s;
                var m = GetModel(out s);
                var t = m.BeginTransaction(System.Data.IsolationLevel.ReadCommitted);
                sync.SignalAndWait();
                for (var i = 0; i < 50; i++)
                {
                    var res = m.CreateResource<SingleMappingTestClass>(new Uri("ex:Resource:Thread1#" + i), t);
                    res.stringTest.Add("Thread1 " + i.ToString());
                    res.Commit();
                    list1.Add(res);
                }
                t.Commit();
                s.Dispose();
            });

            var worker2 = new Thread(() =>
            {
                IStore s;
                var m = GetModel(out s);
                var t = m.BeginTransaction(System.Data.IsolationLevel.ReadCommitted);
                sync.SignalAndWait();
                for (var i = 0; i < 50; i++)
                {
                    var res = m.CreateResource<SingleMappingTestClass>(new Uri("ex:Resource:Thread2#" + i), t);
                    res.stringTest.Add("Thread2 " + i.ToString());
                    res.Commit();
                    list2.Add(res);
                }
                t.Commit();
                s.Dispose();
            });

            worker1.Start();
            worker2.Start();

            sync.SignalAndWait();

            worker1.Join();
            worker2.Join();

            Assert.AreEqual(50, list1.Count());
            Assert.AreEqual(50, list2.Count());

            var model = _store.GetModel(_model);

            foreach (var res in list1)
            {
                var actual = model.GetResource<SingleMappingTestClass>(res.Uri);
                Assert.AreEqual(res.stringTest.Count(), actual.stringTest.Count());
                Assert.AreEqual(res.stringTest[0], actual.stringTest[0]);
            }

            foreach (var res in list2)
            {
                var actual = model.GetResource<SingleMappingTestClass>(res.Uri);
                Assert.AreEqual(res.stringTest.Count(), actual.stringTest.Count());
                Assert.AreEqual(res.stringTest[0], actual.stringTest[0]);
            }
        }

        [Test]
        public void TestModifyElement()
        {
            Assert.Inconclusive();
            var model = _store.GetModel(_model);
            var newResource = model.CreateResource<SingleMappingTestClass>();
            newResource.stringTest.Add("Hello");
            newResource.stringTest.Add("my");
            newResource.stringTest.Add("dear");
            newResource.Commit();

            var sync = new Barrier(3);
            var sync2 = new Barrier(2);

            var worker1 = new Thread(() =>
            {
                IStore s;
                var m = GetModel(out s);

                try
                {
                    var t = m.BeginTransaction(System.Data.IsolationLevel.RepeatableRead);
                    sync.SignalAndWait();
                    var res = m.GetResource<SingleMappingTestClass>(newResource.Uri, t);
                    
                    res.stringTest.Add("Thread1");
                    res.stringTest.Remove("my");
                    
                    res.Commit();
                    t.Commit();
                    
                    s.Dispose();
                }
                catch (Exception)
                {
                    sync2.SignalAndWait();
                }
            });

            var worker2 = new Thread(() =>
            {
                try
                {
                    IStore s;
                    var m = GetModel(out s);
                    var t = m.BeginTransaction(System.Data.IsolationLevel.RepeatableRead);

                    
                    var res = m.GetResource<SingleMappingTestClass>(newResource.Uri, t);
                    sync.SignalAndWait();
                    res.stringTest.Add("Thread2");
                    res.stringTest.Remove("dear");
                    sync2.SignalAndWait();
                    res.Commit();
                    t.Commit();
                    s.Dispose();
                }
                catch (Exception) {}
            });

            // Start both threads
            worker1.Start();
            worker2.Start();

            // Wait to enter transaction
            sync.SignalAndWait();

            // Wait for threads to end
            worker2.Join();
            worker1.Join();
            

            var actualResource = model.GetResource<SingleMappingTestClass>(newResource.Uri);
            Assert.IsFalse(actualResource.stringTest.Contains("dear"));
            Assert.IsTrue(actualResource.stringTest.Contains("my"));
            Assert.IsTrue(actualResource.stringTest.Contains("Thread2"));

        }

        [Test]
        public void TestModifyAndAddElement()
        {
            Assert.Inconclusive();

            var model = _store.GetModel(_model);
            var newResource = model.CreateResource<SingleMappingTestClass>();
            newResource.stringTest.Add("Hello");
            newResource.stringTest.Add("my");
            newResource.stringTest.Add("dear");
            newResource.Commit();

            var addedResourceUri = new Uri("ex:blub");

            var sync = new Barrier(3);

            var worker1 = new Thread(() =>
            {
                IStore s;
                var m = GetModel(out s);
                var t = m.BeginTransaction(System.Data.IsolationLevel.ReadCommitted);
                Debug.WriteLine("Worker 1: Started Transaction");
                sync.SignalAndWait();
                var res = m.CreateResource<SingleMappingTestClass>(addedResourceUri);
                res.stringTest.Add("Thread1");
                
                res.Commit();
                Debug.WriteLine("Worker 1: Commit Resource");
                t.Commit();
                Debug.WriteLine("Worker 1: Commit Transaction");
                s.Dispose();
            });

            var worker2 = new Thread(() =>
            {
                IStore s;
                var m = GetModel(out s);
                var t = m.BeginTransaction(System.Data.IsolationLevel.RepeatableRead);
                Debug.WriteLine("Worker 2: started Transaction");
                sync.SignalAndWait();

                try
                {
                    ModifyData(m, newResource.Uri, t);
                }catch(Exception)
                {
                    // Retry
                    ModifyData(m, newResource.Uri, t);
                }

                Debug.WriteLine("Worker 2: Commit Resource");
                t.Commit();
                Debug.WriteLine("Worker 2: Commit Transcation");
                s.Dispose();

            });

            worker1.Start();
            worker2.Start();
            sync.SignalAndWait();

            worker1.Join();
            worker2.Join();

            var actualResource = model.GetResource<SingleMappingTestClass>(newResource.Uri);
            var actualResource2 = model.GetResource<SingleMappingTestClass>(addedResourceUri);


        }

        [Test]
        public void TestCreateResourceMemberVariables()
        {
            Assert.Inconclusive();

            var faulted = false;

            var sync = new Barrier(2);
            var sync2 = new Barrier(3);

            var createWorker = new Thread(() =>
            {
                IStore store;
                var model = GetModel(out store);

                model.Clear();

                using (var tx = model.BeginTransaction(System.Data.IsolationLevel.ReadCommitted))
                {
                    var r1 = model.CreateResource<ResourceMappingTestClass>(new Uri("ex:r1"), tx);
                    r1.IntegerValue = 1;
                    r1.Commit();

                    var r0 = model.CreateResource<ResourceMappingTestClass>(new Uri("ex:r0"), tx);
                    r0.Resource = r1;
                    r0.Commit();

                    tx.Commit();
                }

                store.Dispose();

                sync.SignalAndWait();
            });

            var getWorker = new Thread(() =>
            {
                try
                {
                    IStore store;
                    var model = GetModel(out store);

                    var r3 = model.GetResource<ResourceMappingTestClass>(new Uri("ex:r0"));

                    Assert.NotNull(r3.Resource);
                    Assert.AreEqual(r3.Resource.IntegerValue, 1);

                    store.Dispose();
                }
                catch(Exception)
                {
                    faulted = true;
                }

                sync2.SignalAndWait();
            });

            var getWorkerTx = new Thread(() =>
            {
                try
                {
                    IStore store;
                    var model = GetModel(out store);

                    using (var tx = model.BeginTransaction(System.Data.IsolationLevel.ReadCommitted))
                    {
                        var r2 = model.GetResource<ResourceMappingTestClass>(new Uri("ex:r0"), tx);

                        Assert.NotNull(r2.Resource);
                        Assert.AreEqual(r2.Resource.IntegerValue, 1);
                    }

                    store.Dispose();
                }
                catch(Exception)
                {
                    faulted = true;
                }

                sync2.SignalAndWait();
            });

            createWorker.Start();

            sync.SignalAndWait();

            getWorker.Start();
            getWorkerTx.Start();

            sync2.SignalAndWait();

            Assert.IsFalse(faulted);
        }

        protected void ModifyData(IModel m, Uri uri, ITransaction t)
        {
            var res = m.GetResource<SingleMappingTestClass>(uri, t);
            res.stringTest.Add("Thread2");
            res.stringTest.Remove("dear");
            res.Commit();
        }

        #endregion
    }
}
