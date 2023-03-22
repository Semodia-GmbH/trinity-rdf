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
using System.Text;
using NUnit.Framework;

namespace Semiodesk.Trinity.Test.Virtuoso
{
   
    [TestFixture]
    public class ResourceBindingTest
    {
        Uri contactListUri = new Uri("semio:test:contactList");
        IStore _store;

        [SetUp]
        public void SetUp()
        {
            _store = StoreFactory.CreateStore("provider=virtuoso;host=localhost;port=1111;uid=dba;pw=dba;rule=urn:semiodesk/test/ruleset");


        }

        IModel GetModel()
        {

            var testModelUri = new Uri("http://localhost:8899/model/TestModel");

            var model = _store.GetModel(testModelUri);
            return model;
        }

        void InitialiseModel(IModel m)
        {
            m.Clear();

            var l = m.CreateResource<ContactList>(contactListUri);

            l.ContainsContact.Add(CreateContact(m, "Hans", new List<string>{"Anton"}, "Meiser", new DateTime(1980, 11, 2), "meiser@test.de", "Deutschland", "Sackgasse 3", "85221", "Dachau"));
            l.ContainsContact.Add(CreateContact(m, "Peter", new List<string>{"Judith", "Ludwig"}, "Meiser", new DateTime(1981, 12, 7), "p.meiser@t-online.de", "Deutschland", "Blubweg 6", "12345", "München"));
            l.ContainsContact.Add(CreateContact(m, "Franz", new List<string> { "Hans", "Wurst" }, "Hubert", new DateTime(1976, 5, 11), "fhubert@t-online.de", "Deutschland", "Siemensstraße 183", "09876", "Berlin"));
            l.ContainsContact.Add(CreateContact(m, "Isabell", new List<string> { "Merlin"}, "Peters", new DateTime(1977, 1, 27), "isab.peters@aol.de", "Deutschland", "Walnussweg 4", "45637", "Bonn"));
            l.ContainsContact.Add(CreateContact(m, "Max", new List<string> (), "Benek", new DateTime(1989, 3, 22), "Max.Benek@aol.de", "Deutschland", "Traunweg 6", "48887", "Schweinfurt"));
            l.ContainsContact.Add(CreateContact(m, "Karsten", new List<string> { "Peter" }, "Oborn", new DateTime(1958, 7, 19), "Superchamp@gmx.de", "Deutschland", "Bei der Wurstfabrik 6", "37439", "Darmstadt"));
            l.ContainsContact.Add(CreateContact(m, "Sabrina", new List<string> { "Hans" }, "Neubert", new DateTime(1960, 8, 15), "Megabirne@gmx.net", "Deutschland", "Hanstraße 1", "55639", "Hanover"));
            l.ContainsContact.Add(CreateContact(m, "Rainer", new List<string> { "Maria" }, "Bader", new DateTime(1970, 4, 26), "Baderainer@web.de", "Deutschland", "Lalaweg 5", "86152", "Augsburg"));
            l.ContainsContact.Add(CreateContact(m, "Maria", new List<string> { "Franz" }, "Roßmann", new DateTime(1968, 10, 6), "Rossmann@web.de", "Deutschland", "Münchner Straße 9", "85123", "Odelzhausen"));
            l.ContainsContact.Add(CreateContact(m, "Helga", new List<string> { "Isabell" }, "Rößler", new DateTime(1988, 2, 1), "Roessler@gmx.de", "Deutschland", "Weiterweg 15", "12345", "München"));
            l.Commit();
        }

        void InitialiseRandomModel(IModel m, int count)
        {
            m.Clear();

            var l = m.CreateResource<ContactList>(contactListUri);

            for (var i = 0; i < count; i++)
            {
                l.ContainsContact.Add(GenerateContact(m));
            }

            l.Commit();
        }

        public class MarkovNameGenerator
        {
            //constructor
            public MarkovNameGenerator(IEnumerable<string> sampleNames, int order, int minLength)
            {
                //fix parameter values
                if (order < 1)
                    order = 1;
                if (minLength < 1)
                    minLength = 1;

                _order = order;
                _minLength = minLength;

                //split comma delimited lines
                foreach (var line in sampleNames)
                {
                    var tokens = line.Split(',');
                    foreach (var word in tokens)
                    {
                        var upper = word.Trim().ToUpper();
                        if (upper.Length < order + 1)
                            continue;
                        _samples.Add(upper);
                    }
                }

                //Build chains            
                foreach (var word in _samples)
                {
                    for (var letter = 0; letter < word.Length - order; letter++)
                    {
                        var token = word.Substring(letter, order);
                        List<char> entry = null;
                        if (_chains.ContainsKey(token))
                            entry = _chains[token];
                        else
                        {
                            entry = new List<char>();
                            _chains[token] = entry;
                        }
                        entry.Add(word[letter + order]);
                    }
                }
            }

            //Get the next random name
            public string NextName
            {
                get
                {
                    //get a random token somewhere in middle of sample word                
                    var s = "";
                    do
                    {
                        var n = _rnd.Next(_samples.Count);
                        var nameLength = _samples[n].Length;
                        s = _samples[n].Substring(_rnd.Next(0, _samples[n].Length - _order), _order);
                        while (s.Length < nameLength)
                        {
                            var token = s.Substring(s.Length - _order, _order);
                            var c = GetLetter(token);
                            if (c != '?')
                                s += GetLetter(token);
                            else
                                break;
                        }

                        if (s.Contains(" "))
                        {
                            var tokens = s.Split(' ');
                            s = "";
                            for (var t = 0; t < tokens.Length; t++)
                            {
                                if (tokens[t] == "")
                                    continue;
                                if (tokens[t].Length == 1)
                                    tokens[t] = tokens[t].ToUpper();
                                else
                                    tokens[t] = tokens[t].Substring(0, 1) + tokens[t].Substring(1).ToLower();
                                if (s != "")
                                    s += " ";
                                s += tokens[t];
                            }
                        }
                        else
                            s = s.Substring(0, 1) + s.Substring(1).ToLower();
                    }
                    while (_used.Contains(s) || s.Length < _minLength);
                    _used.Add(s);
                    return s;
                }
            }

            //Reset the used names
            public void Reset()
            {
                _used.Clear();
            }

            //private members
            private Dictionary<string, List<char>> _chains = new Dictionary<string, List<char>>();
            private List<string> _samples = new List<string>();
            private List<string> _used = new List<string>();
            private Random _rnd = new Random();
            private int _order;
            private int _minLength;

            //Get a random letter from the chain
            private char GetLetter(string token)
            {
                if (!_chains.ContainsKey(token))
                    return '?';
                var letters = _chains[token];
                var n = _rnd.Next(letters.Count);
                return letters[n];
            }
        }

        Contact GenerateContact(IModel m)
        {
            var rng = new Random();

            var firstNameGenerator = new MarkovNameGenerator( new List<string>{"Hans", "Peter", "Marie", "Maria", "Tina", "Tim", "Lukas", "Emma", "Tom", "Alina", "Mia", "Emma", "Siegfried", "Judith", "Karl", "Stefan", "Markus", "Martin", "Alfred", "Anton"}, 3, 5);
            var firstName = firstNameGenerator.NextName;

            var additionalNames = new List<string>();
            for (var i = rng.Next(0, 3); i > 0; i--)
            {
                additionalNames.Add(firstNameGenerator.NextName);
            }

            var lastNameGenerator = new MarkovNameGenerator(new List<string> { "Maier", "Meier", "Schmied", "Schmidt", "Schulz", "Roßman", "Müller", "Klein", "Fischer", "Schwarz", "Weber", "Hofman", "Hartman", "Braun", "Koch", "Krüger", "Schröder", "Wolf", "Mayer", "Jung", "Vogel", "Lang", "Fuchs", "Huber" }, 3, 5);
            var lastName = lastNameGenerator.NextName;

            var start = new DateTime(1950, 1, 1);
            var range = ((TimeSpan)(new DateTime(1995, 1, 1) - start)).Days;
            var birthDate = start.AddDays(rng.Next(range));

            var emailAddress = string.Format("{0}.{1}@gmx.de", firstName, lastName);

            return CreateContact(m, firstName, additionalNames, lastName, birthDate, emailAddress, "Deutschland", "Teststraße 27", "123456", "Testhausen"); 
        }

        Contact CreateContact(IModel m, string nameGiven, List<string> nameAdditional, string nameFamily, DateTime birthDate, string emailAddress, string country, string street, string pocode, string city)
        {
            var contactUri = new Uri("semio:"+nameGiven+":" + Guid.NewGuid().ToString());
            var c = m.CreateResource<PersonContact>(contactUri);
            var b = new StringBuilder();
            foreach( var n in nameAdditional )
            {
                b.Append(n);
                b.Append(" ");
                c.NameAdditional.Add(n);
            }
            if (b.Length > 1)
            {
                b.Remove(b.Length - 1, 1);
            }
            c.Fullname = string.Format("{0} {1} {2}", nameGiven, b, nameFamily) ;
            c.NameGiven = nameGiven;
            c.NameFamily = nameFamily;
            c.BirthDate = birthDate;
            c.EmailAddresses.Add(CreateEmailAddress(m, emailAddress));
            c.PostalAddresses.Add(CreatePostalAddress(m, country, street, pocode, city));

            c.Commit();
            return c;
        }

        EmailAddress CreateEmailAddress(IModel m, string emailAddress)
        {
            var contactUri = new Uri("semio:" + Guid.NewGuid().ToString());
            var c = m.CreateResource<EmailAddress>(contactUri);
            c.Address = emailAddress;
            c.Commit();
            return c;
        }

        PostalAddress CreatePostalAddress(IModel m, string country, string street, string pocode, string city)
        {
            var contactUri = new Uri("semio:" + Guid.NewGuid().ToString());
            var c = m.CreateResource<PostalAddress>(contactUri);
            c.Country = country;
            c.StreetAddress = street;
            c.PostalCode = pocode;
            c.City = city;
            c.Commit();
            return c;
        }
    }
}
