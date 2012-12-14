﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//using datasift; //be explicit
using NUnit.Framework;

namespace my_prog
{
#if false
    [TestFixture]
    class ValidateUnitTest
    {
        const string goodName = "bob";
        const string goodKey = "xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx";
        const string goodCsdl = "interaction.content contains \"datasift\"";

        [Test]
        [ExpectedException("datasift.AccessDeniedException")]
        public void ValidateWithInvalidUser()
        {
            _Validate(userName: "zzzz");
        }
        [Test]
        [ExpectedException("datasift.AccessDeniedException")]
        public void ValidateWithInvalidApiKey()
        {
            _Validate(userKey: "1234567890abcdef1234567890abcdef");
        }

        [Test]
        public void ValidateWithAllOk()
        {
            _Validate();
        }

        [Test]
        [ExpectedException( "datasift.CompileFailedException" )]
        public void ValidateWithInvalidCsdl()
        {
            //miss spelled word
            _Validate("intction.content contains \"datasift\"");
        }

        #region helpers
        private void _Validate(
          string csdl = goodCsdl,
          string userName = goodName,
          string userKey = goodKey)
        {
            new TestValidate().Test(csdl, userName, userKey);
        }
        #endregion
   
        [Test]
        public void CompileWithAllOk() {
            new TestCompile().Test(goodCsdl, goodName, goodKey);
        }
    }
#endif

    #region compile/validate
    class TestValidate :Test3{
        public Dictionary<string, object> Test(string csdl, string userName, string userKey){
            return TestHelper.validateOrCompile(CompileMode.ValidateOnly, csdl, userName, userKey);
        }
    }
    class TestCompile :Test3 {
        public Dictionary<string, object> Test(string csdl, string userName, string userKey) {
            return TestHelper.validateOrCompile(CompileMode.Compile, csdl, userName, userKey);
        }
    }
    #endregion

    #region dpu
    class TestDpuFromHash : Test3 {
        public Dictionary<string, object> Test(string csdlhash, string userName, string userKey) {
            var user = new datasift.User(userName, userKey);
            var def  = new datasift.Definition(user, hash:csdlhash);  
            return JsonHelpers.DpuBreakdownAsDictionary(def.getDpuBreakdown());
        }
    }
    class TestDpuFromCsdl : Test3 {
        public Dictionary<string, object> Test(string csdl, string userName, string userKey) {
            var def = TestHelper.DefinitionFromCsdl(csdl, userName, userKey);
            return JsonHelpers.DpuBreakdownAsDictionary(def.getDpuBreakdown());
        }
    }
    #endregion

    #region usage
    class TestUsage : Test3 {
        public Dictionary<string, object> Test(string period, string userName, string userKey) {
            var user = new datasift.User(userName, userKey);
            var usage = user.getUsage(period);
            var Result = new Dictionary<string, object>();
            Result.Add("start", usage.getStartDate());
            Result.Add("end", usage.getEndDate());
            var HashsInfo = new Dictionary<string, object>();
            foreach (var hash in usage.getStreamHashes()) {
                var HashInfo = new Dictionary<string, object>();
                var TypesInfo = new Dictionary<string, object>();
                foreach (var type in usage.getLicenseTypes(hash)) {
                    TypesInfo.Add(type, usage.getLicenseUsage(hash, type));
                }
                HashInfo.Add("licenses", TypesInfo);
                HashInfo.Add("seconds", usage.getSeconds(hash));
                HashsInfo.Add(hash, HashInfo);
            }
            Result.Add("streams", HashsInfo);
            return Result;
        }
    }
    #endregion

    #region stream
    class TestStreamCsdl : TestStreamHelper, Test4 
    {
        public Dictionary<string, object> Test(string connectionType, string csdl, string userName, string userKey) 
        {
            var def = TestHelper.DefinitionFromCsdl(csdl, userName, userKey);
            return TestStream(connectionType, def);
        }
    }

    class TestStreamX : TestStreamHelper, Test5
    {
        public Dictionary<string, object> Test(string connectionType, string csdl, string hash, string userName, string userKey)
        {
            var def = TestHelper.DefinitionFromX(csdl, hash, userName, userKey);
            return TestStream(connectionType, def);
        }
    }

    class TestStreamHelper
    {
        public Dictionary<string, object> TestStream(string connectionType, datasift.Definition def)
        {
            var consumer = def.getConsumer(new EventHandlers(), connectionType);
            consumer.consume();
            //can get here?

            var Result = new Dictionary<string, object>();
            return Result;
        }
    }

    #endregion

    class TestHelper {
        #region compile/validate
        public delegate void compileDeligate(datasift.Definition definition);
        public static Dictionary<string, object> validateOrCompile(
            CompileMode mode, string csdl, string userName, string userKey) 
        {
            var def = DefinitionFromCsdl(csdl, userName, userKey);
            switch (mode)
            {
                case CompileMode.Compile:
                    def.compile();
                    break;
                case CompileMode.ValidateOnly:
                    def.validate();
                    break;
            }
            var Result = JsonHelpers.definitionAsDictionary(def,mode);
            return Result;
        }
        #endregion

        #region definition
        public static datasift.Definition DefinitionFromCsdl(string csdl, string userName, string userKey) {
            var Result = new datasift.User(userName, userKey).createDefinition(csdl);
            Assert.That(Result, Is.TypeOf<datasift.Definition>());
            Assert.That(Result, Is.Not.Null);
            return Result;
        }

        public static datasift.Definition DefinitionFromX(string csdl, string hash, string userName, string userKey)
        {
            var Result = new datasift.Definition(new datasift.User(userName, userKey), csdl, hash);
            Assert.That(Result, Is.TypeOf<datasift.Definition>());
            Assert.That(Result, Is.Not.Null);
            return Result;
        }
        #endregion
    }

    public enum CompileMode { Compile, ValidateOnly };
    class JsonHelpers {
        #region definition
        public static Dictionary<string, object> definitionAsDictionary(
            datasift.Definition def, CompileMode type) {
            var Result = new Dictionary<string, object>();
            {
                Result.Add("createdAt", def.getCreatedAt());
                Result.Add("user", userAsDictionary(def.getUser()));
                Result.Add("csdl", def.get());
                Result.Add("dpuTotal", def.getTotalDpu().ToString());
                if (type==CompileMode.Compile) { 
                    Result.Add("hash", def.getHash());
                }
            }
            return Result;
        }
        #endregion

        #region dpu
        public static Dictionary<string, object> DpuBreakdownAsDictionary(datasift.Dpu dpu) {
            var Result = new Dictionary<string, object>();
            {
                var breakDown = new Dictionary<string, object>();
                {
                    breakDown.Add("dpu", dpu.getTotal());
                    breakDown.Add("detail", DpuItemDictionaryAsDictionary(dpu.getDpu()));
                }
                Result.Add("dpuBreakdown", breakDown);
            }
            return Result;
        }

        //:warning: recursion without tail-call optimisation
        public static Dictionary<string, object> DpuItemDictionaryAsDictionary(Dictionary<string, datasift.DpuItem> dpuItems) {
            var Result = new Dictionary<string, object>();
            {
                foreach (var key in dpuItems.Keys) {
                    Result.Add(key, DpuItemAsDictionary(dpuItems[key]));
                }
            }
            return Result;
        }

        //:warning: recursion without tail-call optimisation
        public static Dictionary<string, object> DpuItemAsDictionary(datasift.DpuItem dpuItem) {
            var Result = new Dictionary<string, object>();
            {
                Result.Add("count", dpuItem.getCount());
                Result.Add("dpu", dpuItem.getDpu());
                Result.Add("targets", DpuItemDictionaryAsDictionary(dpuItem.getTargets()));
            }
            return Result;
        }
        #endregion

        #region user
        public static Dictionary<string, object> userAsDictionary(datasift.User user) {
            var Result = new Dictionary<string, object>();
            {
                Result.Add("name", user.getUsername());
                Result.Add("apiKey", user.getApiKey());
                Result.Add("rateLimit", user.getRateLimit());
                Result.Add("rateLimitRemaining", user.getRateLimitRemaining());
                //...etc...
            }
            return Result;
        }
        #endregion

        #region interaction
        public static Dictionary<string, object> interactionAsDictionary(datasift.JSONdn interaction)
        {
            return a(interaction.getJVal(""));
        }

        public static Dictionary<string, object> a(Newtonsoft.Json.Linq.JToken token)
        {

            return new Dictionary<string, object>();
        }

        public static Dictionary<string, object> _interactionAsDictionary(datasift.JSONdn interaction, string root) {
            var Result = new Dictionary<string, object>();
            {
                var keys = interaction.getKeys();
                Result["keys"] = keys;
                foreach (var key in keys)
                {
                    var t = interaction.getJVal("");
                    
                    
                }
                
            }
            return Result;
        }
        #endregion

        #region consumer
        public static Dictionary<string, object> consumerAsDictionary(datasift.StreamConsumer consumer) {
            var Result = new Dictionary<string, object>();
            {
                
            }
            return Result;
        }
        #endregion

        public static  Dictionary<string, object> objectAsDictionary(string s,object o) {
            var Result = new Dictionary<string, object>();
            {
                Result.Add(s, o);
            }
            return Result;
        }
    }

    class EventHandlers : datasift.IEventHandler {
        private Logger _theLogger = new LogFactory().theLogger();
        private JsonHelpers JsonHelpers = new JsonHelpers();

        #region IEventHandler Members

        public void onConnect(datasift.StreamConsumer consumer) {
            _theLogger.logEvent("connected",
                new object[] {
                    JsonHelpers.consumerAsDictionary(consumer)
                });  
        }

        public void onDeleted(datasift.StreamConsumer consumer, datasift.Interaction interaction, string hash) {
            _theLogger.logEvent( "delete",
                new object[] {
                    JsonHelpers.objectAsDictionary("hash",hash),
                    JsonHelpers.interactionAsDictionary(interaction),
                    JsonHelpers.consumerAsDictionary(consumer)
                });  
        }

        public void onDisconnect(datasift.StreamConsumer consumer) {
            _theLogger.logEvent("dissconneted",
                new object[] {
                    JsonHelpers.consumerAsDictionary(consumer)
                });  
        }

        public void onError(datasift.StreamConsumer consumer, string message) {
            _theLogger.logEvent("error",
                new object[] {
                    JsonHelpers.objectAsDictionary("message",message),
                    JsonHelpers.consumerAsDictionary(consumer)
                });  
        }

        public void onInteraction(datasift.StreamConsumer consumer, datasift.Interaction interaction, string hash) {
            _theLogger.logEvent("interaction",
                new object[] {
                    JsonHelpers.objectAsDictionary("hash",hash),
                    JsonHelpers.interactionAsDictionary(interaction),
                    JsonHelpers.consumerAsDictionary(consumer)
                });  
        }

        public void onWarning(datasift.StreamConsumer consumer, string message) {
            _theLogger.logEvent("warning",
               new object[] {
                    JsonHelpers.objectAsDictionary("message",message),
                    JsonHelpers.consumerAsDictionary(consumer)
                });  
        }

        #endregion
    }
}
