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

using NUnit.Framework;
using System;
using System.Diagnostics;

namespace Semiodesk.Trinity.Test.Linq
{
    [TestFixture]
    public class LinqModelGroupTest : LinqTestBase
    {
        [SetUp]
        public override void SetUp()
        {
            // DotNetRdf memory store.
            var connectionString = "provider=dotnetrdf";

            // Stardog store.
            //string connectionString = "provider=stardog;host=http://localhost:5820;uid=admin;pw=admin;sid=test";

            // OpenLink Virtoso store.
            //string connectionString = string.Format("{0};rule=urn:semiodesk/test/ruleset", SetupClass.ConnectionString);

            Store = StoreFactory.CreateStore(connectionString);
            Store.InitializeFromConfiguration();
            Store.Log = (l) => Debug.WriteLine(l);

            var model1 = Store.CreateModel(new Uri("http://test.com/test1"));
            model1.Clear();

            var model2 = Store.CreateModel(new Uri("http://test.com/test2"));
            model2.Clear();

            // Add an agent so we can check if types are correctly queried.
            var a1 = model1.CreateResource<Agent>(ex.John);
            a1.FirstName = "John";
            a1.LastName = "Doe";
            a1.Commit();

            var g1 = model1.CreateResource<Group>(ex.TheSpiders);
            g1.Name = "The Spiders";
            g1.Commit();

            var g2 = model2.CreateResource<Group>(ex.AlicaKeys);
            g2.Name = "Alicia Keys";
            g2.Commit();

            var p1 = model1.CreateResource<Person>(ex.Alice);
            p1.FirstName = "Alice";
            p1.LastName = "Cooper";
            p1.Age = 69;
            p1.Birthday = new DateTime(1948, 2, 4);
            p1.Group = g1;
            p1.Status = true;
            p1.AccountBalance = 100000f;
            p1.Commit();

            var p2 = model1.CreateResource<Person>(ex.Bob);
            p2.FirstName = "Bob";
            p2.LastName = "Dylan";
            p2.Age = 76;
            p2.Birthday = new DateTime(1941, 5, 24);
            p2.AccountBalance = 10000.1f;
            p2.Commit();

            var p3 = model2.CreateResource<Person>(ex.Eve);
            p3.FirstName = "Eve";
            p3.LastName = "Jeffers-Cooper";
            p3.Birthday = new DateTime(1978, 11, 10);
            p3.Age = 38;
            p3.Group = g2;
            p3.AccountBalance = 1000000.1f;
            p3.Commit();

            p1.KnownPeople.Add(p2);
            p1.Commit();

            p2.KnownPeople.Add(p1);
            p2.KnownPeople.Add(p2);
            p2.Commit();

            p3.Interests.Add(g2);
            p3.Interests.Add(p3);
            p3.Commit();

            var i1 = model1.CreateResource<Image>();
            i1.DepictedAgent = p1;
            i1.Commit();

            Model = Store.CreateModelGroup(model1, model2);
        }
    }
}