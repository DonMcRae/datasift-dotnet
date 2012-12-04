using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace my_prog {
    [TestFixture]
    class Nunit_JsonDots {
        const string jsonWithDots =
            "{\"start\":\"Tue, 04 Dec 2012 09:35:00 +0000\",\"end\":\"Tue, 04 Dec 2012 10:35:00 +0000\",\"streams\":{\"cfc973789e670fe91aceb4b91cbae4db\":{\"licenses\":{\"gender\":3754,\"interaction\":33134,\"klout.score\":30271,\"language\":31022,\"links\":8590,\"salience.sentiment\":30948,\"trends\":3612,\"twitter\":33134},\"seconds\":150},\"947b690ec9dca525fb8724645e088d79\":{\"licenses\":[],\"seconds\":136}}}";     

        [Test]
        public void JsonDotTest() {
            var decoder = new datasift.JSONdn(jsonWithDots);
            Assert.That(decoder.has("start"));
            Assert.That(decoder.has("zzbobbuilderxx"), Is.False);
            Assert.That(decoder.has("streams.cfc973789e670fe91aceb4b91cbae4db.licenses.gender"));
        }

        [Test]
        public void JsonDotTest2() {
            var decoder = new datasift.JSONdn(jsonWithDots);
            Assert.That(decoder.has("streams.cfc973789e670fe91aceb4b91cbae4db.licenses.klout.score"));
            //Assert.That(decoder.getLongVal("streams.cfc973789e670fe91aceb4b91cbae4db.licenses.klout.score"), Is.EqualTo(30271));
        }
        
    }
}
