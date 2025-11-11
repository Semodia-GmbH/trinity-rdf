
using Semiodesk.Trinity.Store.Fuseki;
using Xunit;

namespace Semiodesk.Trinity.Test.Fuseki
{

    public class StoreProviderTest : SetupClass
    {

        [Fact]
        public void FusekiConfigurationStringTest()
        {

            string connectionString = string.Format("provider=fuseki;host=http://localhost:3000;dataset=ds");
            IStore anObject = StoreFactory.CreateStore(connectionString);
            Assert.IsNotNull(anObject);
            Assert.IsInstanceOf<FusekiStore>(anObject);
            anObject.Dispose();
        }
    }
}
