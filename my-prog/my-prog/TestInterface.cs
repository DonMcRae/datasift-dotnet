using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace my_prog
{
    interface Test { }

    interface Test0 : Test
    {
        Dictionary<string, object> Test();
    }

    interface Test1 : Test
    {
        Dictionary<string, object> Test(string a);
    }

    interface Test2 : Test
    {
        Dictionary<string, object> Test(string a, string b);
    }

    interface Test3 : Test
    {
        Dictionary<string, object> Test(string a, string b, string c);
    }

    interface Test4 : Test
    {
        Dictionary<string, object> Test(string a, string b, string c, string d);
    }

    interface Test5 : Test
    {
        Dictionary<string, object> Test(string a, string b, string c, string d, string e);
    }
}
