
using System;
using Xunit;

namespace Semiodesk.Trinity.Test
{
    public class SparqlSerializerTest
    {
        [Fact]
        public void TestStringSerializeResource()
        {
            var r = new Resource("http://example.com/ex");
            r.AddProperty(Ontologies.dc.title, "MyResource");

            var res = SparqlSerializer.SerializeResource(r);
            const string expected = "<http://example.com/ex> <http://purl.org/dc/elements/1.1/title> 'MyResource'. ";

            Assert.Equal(expected, res);
        }

        [Fact]
        public void TestStringSerializeResourceWithMapping()
        {
            var contact = new PersonContact(new Uri("http://example.com/ex"));
            contact.NameGiven = "Peter";

            var res = SparqlSerializer.SerializeResource(contact);
            var expected = "<http://example.com/ex> <http://www.semanticdesktop.org/ontologies/2007/03/22/nco#nameGiven> 'Peter'; <http://www.w3.org/1999/02/22-rdf-syntax-ns#type> <http://www.semanticdesktop.org/ontologies/2007/03/22/nco#PersonContact>. ";
            
            Assert.Equal(expected, res);

            contact.Language = "DE";
            res = SparqlSerializer.SerializeResource(contact);

            Assert.Equal(expected, res);
        }

        [Fact]
        public void TestStringSerializeResourceEmpty()
        {
            var empty = new Resource("http://test.com/ex");

            var res = SparqlSerializer.SerializeResource(empty);
            var expected = "";

            Assert.Equal(expected, res);
        }

        [Fact]
        public void TestSerializeResourceWithBlankNode()
        {
            var r0 = new Resource(new UriRef("_:0", true));
            var r1 = new Resource(new UriRef("_:1", true));
            r1.AddProperty(new Property(new UriRef("http://schema.org/relatedTo")), r0);

            var s = SparqlSerializer.SerializeResource(r1);

            Assert.Contains("_:1 <http://schema.org/relatedTo> _:0", s);
        }
    }
}
