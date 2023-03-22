using NUnit.Framework;
using System.IO;

namespace dotNetRDFStore.Test
{
    [SetUpFixture]
    public class SetupClass
    {


        [OneTimeSetUp]
        public void Setup()
        {

            Directory.SetCurrentDirectory(TestContext.CurrentContext.TestDirectory);

        }
    }
}