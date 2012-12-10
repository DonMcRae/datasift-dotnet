using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace my_prog
{
    [TestFixture]
    class Nunit_DecodeInteraction
    {
        private const string Json_dict = "{\"a\":\"b\", \"b\":\"c\"}";                

        [TestCase(Json_dict)]
        public void jsonDict(string theJson)
        {
            datasift.JSONdn json = new datasift.JSONdn(theJson);
            var a = json.

        }
    }
}
