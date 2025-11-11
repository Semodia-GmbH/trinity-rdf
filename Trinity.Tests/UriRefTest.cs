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
using Xunit;

namespace Semiodesk.Trinity.Test
{

    public class UriRefTest
    {
        [Fact]
        public void EqualsTest()
        {
            var u0 = new UriRef("http://semiodesk.com/ontologies/ppo");
            var u1 = new UriRef("http://semiodesk.com/ontologies/ppo#UriScehma");

            Assert.False(ReferenceEquals(u0, u1));
            Assert.Equal(u0, u0);
            Assert.NotEqual(u0, u1);
            Assert.False(u0 == u1);

            var u2 = new UriRef("file://D:/Documents/x.doc");
            var u3 = new UriRef("file://D:/Documents/2012/../x.doc");

            Assert.False(ReferenceEquals(u2, u3));
            Assert.Equal(u2, u3);
            Assert.True(u2 == u3);

            var u4 = new UriRef("file://D:/Documents/x.doc#Metadata");
            var u5 = new UriRef("file://D:/Documents/2012/../x.doc#Metadata");

            Assert.False(ReferenceEquals(u4, u5));
            Assert.Equal(u4, u5);
            Assert.True(u4 == u5);

            var u6 = new UriRef("_:b0", true);
            var u7 = new UriRef("_:b0", true);
            var u8 = new UriRef("_:b1", true);

            Assert.False(ReferenceEquals(u6, u7));
            Assert.Equal(u6, u7);
            Assert.True(u6.Equals(u7));
            Assert.True(u6 == u7);

            Assert.False(ReferenceEquals(u7, u8));
            Assert.NotEqual(u7, u8);
            Assert.False(u7.Equals(u8));
            Assert.False(u7 == u8);
        }

        [Fact]
        public void ToStringTest()
        {
            var u0 = new UriRef("http://semiodesk.com/ontologies/ppo#UriScehma");
            var u1 = "http://semiodesk.com/ontologies/ppo#UriScehma";

            Assert.Equal(u1, u0.ToString());
        }

        [Fact]
        public void ToUriRefTest()
        {
            var uriString = "http://semiodesk.com/ontologies/ppo#UriScehma";
            var u0 = uriString.ToUriRef();
            Assert.Equal(uriString, u0.OriginalString);
        }

        [Fact]
        public void GetHashCodeTest()
        {
            var absoluteUri = new UriRef("http://trinity-rdf.net");

            Assert.Equal(671617664, absoluteUri.GetHashCode());

            var absoluteUriRef = new UriRef("http://trinity-rdf.net#download");

            Assert.Equal(671107712, absoluteUriRef.GetHashCode());

            var relativeUri = new UriRef("#download", UriKind.Relative);

            Assert.Throws<InvalidOperationException>(() => relativeUri.GetHashCode());

            var blankId = new UriRef("_:0", true);

            Assert.Equal(-1219930229, blankId.GetHashCode());
        }
    }
}
