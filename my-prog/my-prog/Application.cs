using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace my_prog {

    public delegate string getOptionDelegate(Dictionary<string, string> options);

    sealed class Application {
        #region Constructors
        public Application(Dictionary<string, string> options) {
            this._options = options;
            dispatcher = new Dispatcher(options);
            setupDispatch();
        }
        #endregion

        #region Private
        private Dictionary<string, string> _options;
        private Logger _theLogger = new LogFactory().theLogger();
        private Dispatcher dispatcher;

        class ListTests : Test0 {
            private Dispatcher dispatcher;
            public ListTests(Dispatcher dispatcher) {
                this.dispatcher = dispatcher;
            }
            public Dictionary<string, object> Test() {
                return new Dictionary<string,object>(){{"Test Names",dispatcher.Keys()}};
            }
        }
      
        #endregion

        #region Entry
        public void Run() {
            dispatcher.dispatchTest();
        }
        #endregion

        #region Config
        private readonly string[] compileOptions = new string[]{
            "csdl",
            "user-name",
            "user-key"
        };

        private void setupDispatch() {
            dispatcher.Add("list", 
                new TestDispatchTableValues(new ListTests(dispatcher), null));
            dispatcher.Add("validate", 
                new TestDispatchTableValues(
                    new TestValidate(),
                    compileOptions ));
            dispatcher.Add("compile",
                new TestDispatchTableValues(
                    new TestCompile(),
                    compileOptions));
            dispatcher.Add("dpuFromCsdl",
             new TestDispatchTableValues(
                 new TestDpuFromCsdl(),
                 compileOptions));
            dispatcher.Add("dpuFromHash",
               new TestDispatchTableValues(
                   new TestDpuFromHash(),
                   new string[]{
                        "hash",
                        "user-name",
                        "user-key"
                    }));
            dispatcher.Add("usage",
                new TestDispatchTableValues(
                    new TestUsage(),
                    new string[]{
                        "period",
                        "user-name",
                        "user-key"
                    }));
            dispatcher.Add("stream-csdl",
               new TestDispatchTableValues(
                   new TestStreamCsdl(),
                   new string[]{
                        "connection-type",
                        "csdl",
                        "user-name",
                        "user-key"
                    }));
            dispatcher.Add("stream-x",
               new TestDispatchTableValues(
                   new TestStreamX(),
                   new string[]{
                        "connection-type",
                        "csdl",
                        "hash",
                        "user-name",
                        "user-key"
                    }));
        }
        #endregion
    }
}
