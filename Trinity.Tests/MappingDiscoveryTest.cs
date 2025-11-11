
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Semiodesk.Trinity.Test
{
    public class MappingTestOntology
    {
        public static readonly Uri Namespace = new Uri("semio:mapping:test");

        public static readonly string Prefix = "mapping";

        public Uri GetNamespace() { return Namespace; }

        public string GetPrefix() { return Prefix; }

        public static readonly Class BaseClass = new Class(new UriRef(BaseClassString));
        public const string BaseClassString = "semio:mapping:test:baseclass";

        public static readonly Class SubClass = new Class(new UriRef(SubClassString));
        public const string SubClassString = "semio:mapping:test:subclass";

        public static readonly Class SubSubClass = new Class(new UriRef(SubSubClassString));
        public const string SubSubClassString = "semio:mapping:test:subsubclass";

        public static readonly Class AnotherBaseClass = new Class(new UriRef(AnotherBaseClassString));
        public const string AnotherBaseClassString = "semio:mapping:test:anotherBaseClass";
    }

    public class BaseClass : Resource
    {
        public BaseClass(UriRef uri) : base(uri) { }

        public override IEnumerable<Class> GetTypes()
        {
            return new List<Class> { MappingTestOntology.BaseClass };
        }
    }

    public class SubClass : BaseClass
    {
        public SubClass(UriRef uri) : base(uri) { }

        public override IEnumerable<Class> GetTypes()
        {
            return new List<Class> { MappingTestOntology.SubClass };
        }
    }

    public class SubSubClass : SubClass
    {
        public SubSubClass(UriRef uri) : base(uri) { }

        public override IEnumerable<Class> GetTypes()
        {
            return new List<Class> { MappingTestOntology.SubSubClass };
        }
    }

    public class AnotherBaseClas : Resource
    {
        public AnotherBaseClas(UriRef uri) : base(uri) { }

        public override IEnumerable<Class> GetTypes()
        {
            return new List<Class> { MappingTestOntology.BaseClass, MappingTestOntology.AnotherBaseClass };
        }
    }

    /// <summary>
    /// This class should test more, if we don't derive from SetupClass we can compare before and after discovery also.
    /// </summary>

    public class MappingDiscoveryTest : SetupClass
    {

        [Fact]
        public void TestGetRdfClasses()
        {
            var classTypes = MappingDiscovery.GetRdfClasses(typeof(BaseClass)).ToList();
            Assert.Equal(1, classTypes.Count());
            Assert.Contains(MappingTestOntology.BaseClass, classTypes);

            classTypes = MappingDiscovery.GetRdfClasses(typeof(SubClass)).ToList();
            Assert.Equal(1, classTypes.Count());
            Assert.Contains(MappingTestOntology.SubClass, classTypes);

            classTypes = MappingDiscovery.GetRdfClasses(typeof(AnotherBaseClas)).ToList();
            Assert.Equal(2, classTypes.Count());
            Assert.Contains(MappingTestOntology.BaseClass, classTypes);
            Assert.Contains(MappingTestOntology.BaseClass, classTypes);
        }

        [Fact]
        public void TestGetBaseClasses()
        {
            var baseTypes = new List<Class>();
            MappingDiscovery.GetBaseTypes(typeof(BaseClass), ref baseTypes);
            Assert.Empty(baseTypes);

            baseTypes = new List<Class>();
            MappingDiscovery.GetBaseTypes(typeof(SubClass), ref baseTypes);
            Assert.Single(baseTypes);
            Assert.Contains(MappingTestOntology.BaseClass, baseTypes);

            baseTypes = new List<Class>();
            MappingDiscovery.GetBaseTypes(typeof(SubSubClass), ref baseTypes);
            Assert.Equal(2, baseTypes.Count());
            Assert.Contains(MappingTestOntology.BaseClass, baseTypes);
            Assert.Contains(MappingTestOntology.SubClass, baseTypes);
        }

        [Fact]
        public void TestGetMatchingTypes()
        {
            var types = new[] { MappingTestOntology.BaseClass };
            var res = MappingDiscovery.GetMatchingTypes(types, typeof(BaseClass), false);

            Assert.Single(res);
            Assert.Equal(typeof(BaseClass), res.First());

            types = new[] { MappingTestOntology.BaseClass, MappingTestOntology.SubClass };
            res = MappingDiscovery.GetMatchingTypes(types, typeof(BaseClass), true);

            //Assert.Equal(1, res.Length);
            Assert.Equal(typeof(SubClass), res.First());

            types = new[] { MappingTestOntology.SubClass, MappingTestOntology.BaseClass };
            res = MappingDiscovery.GetMatchingTypes(types, typeof(BaseClass), true);

            //Assert.Equal(1, res.Length);
            Assert.Equal(typeof(SubClass), res.First());
        }
    }
}
