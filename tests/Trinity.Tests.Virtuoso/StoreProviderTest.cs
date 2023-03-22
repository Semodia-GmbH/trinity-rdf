using NUnit.Framework;

namespace Semiodesk.Trinity.Test.Virtuoso
{
    [TestFixture]
    public class StoreProviderTest : SetupClass
    {

        [Test]
        public void VirtuosoConfigurationStringTest()
        {
            var components = SetupClass.HostAndPort.Split(':');
            var host = components[0];
            var port = components[1];
            var connectionString = string.Format("provider=virtuoso;host={0};port={1};uid=dba;pw=dba;rule=urn:semiodesk/test/ruleset", host, port);
            var anObject = StoreFactory.CreateStore(connectionString);
            Assert.IsNotNull(anObject);
            anObject.Dispose();
        }
    }
}
