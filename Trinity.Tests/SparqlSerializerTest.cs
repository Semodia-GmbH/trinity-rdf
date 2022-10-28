using NUnit.Framework;
using System;

namespace Semiodesk.Trinity.Test
{
    [TestFixture]
    class SparqlSerializerTest
    {
        [TestCase]
        public void TestStringSerializeResource()
        {
            var r = new Resource("http://example.com/ex");
            r.AddProperty(Ontologies.dc.title, "MyResource");

            var res = SparqlSerializer.SerializeResource(r);
            var expected = "<http://example.com/ex> <http://purl.org/dc/elements/1.1/title> 'MyResource'. ";

            Assert.AreEqual(expected, res);
        }

        [TestCase]
        public void TestStringSerializeResourceWithMapping()
        {
            var contact = new PersonContact(new Uri("http://example.com/ex"));
            contact.NameGiven = "Peter";

            var res = SparqlSerializer.SerializeResource(contact);
            var expected = "<http://example.com/ex> <http://www.semanticdesktop.org/ontologies/2007/03/22/nco#nameGiven> 'Peter'; <http://www.w3.org/1999/02/22-rdf-syntax-ns#type> <http://www.semanticdesktop.org/ontologies/2007/03/22/nco#PersonContact>. ";
            
            Assert.AreEqual(expected, res);

            contact.Language = "DE";
            res = SparqlSerializer.SerializeResource(contact);

            Assert.AreEqual(expected, res);
        }

        [TestCase]
        public void TestStringSerializeResourceEmpty()
        {
            var empty = new Resource("http://test.com/ex");

            var res = SparqlSerializer.SerializeResource(empty);
            var expected = "";

            Assert.AreEqual(expected, res);
        }

        [TestCase]
        public void TestSerializeResourceWithBlankNode()
        {
            var r0 = new Resource(new UriRef("_:0", true));
            var r1 = new Resource(new UriRef("_:1", true));
            r1.AddProperty(new Property(new UriRef("http://schema.org/relatedTo")), r0);

            var s = SparqlSerializer.SerializeResource(r1);

            Assert.IsTrue(s.Contains("_:1 <http://schema.org/relatedTo> _:0"));
        }
    }
}
