using System;
using System.Diagnostics;
using System.Linq;
using Xunit;


namespace Semiodesk.Trinity.Test.Linq
{
    public class ResourceWithMultipleTypesTest : IDisposable

    {
        private IStore _store;
        private IModel Model;

        Songwriter sw;

        public ResourceWithMultipleTypesTest()
        {
            SetUp();
        }


        private void SetUp()
        {
            // Directory.SetCurrentDirectory(TestContext.CurrentContext.TestDirectory);

            // DotNetRdf memory store.
            var connectionString = "provider=dotnetrdf";

            // Stardog store.
            //string connectionString = "provider=stardog;host=http://localhost:5820;uid=admin;pw=admin;sid=test";

            // OpenLink Virtoso store.
            //string connectionString = string.Format("{0};rule=urn:semiodesk/test/ruleset", SetupClass.ConnectionString);


            _store = StoreFactory.CreateStore(connectionString);
            _store.InitializeFromConfiguration();
            _store.Log = (l) => Debug.WriteLine(l);

            Model = _store.CreateModel(ex.Namespace);
            Model.Clear();

            var typeProperty = new Property(new Uri("rdf:type"));

            var p1 = Model.CreateResource<Person>(ex.JohnLennon);
            p1.AddProperty(typeProperty, new Uri("http://www.example.org/music/Songwriter"));
            p1.AddProperty(typeProperty, new Uri("http://www.example.org/music/SoloArtist"));
            p1.Commit();

            var sw1 = Model.CreateResource<Songwriter>(ex.PaulMcCartney);
            sw1.FirstName = "Paul";
            sw1.LastName = "McCartney";
            sw1.Age = 77;
            sw1.Birthday = new DateTime(1942, 06, 18);
            sw1.AccountBalance = 10000.1f;
            sw1.Commit();

            var b1 = Model.CreateResource<Band>(ex.TheBeatles);
            b1.Name = "The Beatles";
            b1.Members.Add(new SoloArtist(p1.Uri));
            b1.Commit();

            var s1 = Model.CreateResource<Song>(ex.BackInTheUSSR);
            sw = new Songwriter(p1.Uri);
            s1.Writers.Add(sw);
            s1.Writers.Add(sw1);
            s1.Length = 163;
            s1.Commit();

            var al1 = Model.CreateResource<Album>(ex.TheBeatlesAlbum);
            al1.Name = "The Beatles (Album)";
            al1.ReleaseDate = new DateTime(1968, 11, 22);
            al1.Artist = b1;
            al1.Tracks.Add(s1);
            al1.Commit();
        }

        public void Dispose()
        {
            _store.Dispose();
            _store = null;
        }

        [Fact]
        public void CanSelectResourcesWithMultipleTypes()
        {
            var actual = Model.AsQueryable<Band>().ToList();
            Assert.True(1 == actual.Count, "bands found");

            var b = actual[0];

            Assert.True(ex.TheBeatles == b.Uri, "The Beatles are the band");
            Assert.True(1 == b.Members.Count, "band member count");
            Assert.True(ex.JohnLennon == b.Members[0].Uri, "john lennon is the band member");

            var albums = (from album in Model.AsQueryable<Album>() where album.Artist.Uri == ex.TheBeatles select album).ToList();
            Assert.True(1 == albums.Count, "album count");

            var a = albums[0];

            Assert.True(ex.TheBeatlesAlbum == a.Uri, "The Beatles Album is the album");
            Assert.True(1 == a.Tracks.Count, "Album track count");

            var s = a.Tracks[0];
            Assert.True(ex.BackInTheUSSR == s.Uri, "'Back in the USSR' is the song");

            Assert.True(2 == s.Writers.Count, "Song writers count");
            Assert.True(s.Writers.Contains(sw), "Song writers' collection contains Lennon");
        }
    }
}