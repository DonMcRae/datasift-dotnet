using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace my_prog {

    #region logging

    sealed class LogFactory {
        private static Logger _theLogger=null;
        public Logger theLogger() {
            //evaluate once (per class/system)
            if (_theLogger == null) {
                _theLogger = new Logger();
            }
            return _theLogger;
        }
    }

    sealed class Logger :IDisposable{
        private bool is_first;
        
        public Logger()
        {
            Write("[");
            is_first = true;
        }

        private void Write(string message)
        {
            System.Console.Write(message);
        }

        public void log(object o) {
            Write(
                (is_first?"":",") +
                Newtonsoft.Json.JsonConvert.SerializeObject(o)
            );
            is_first = false;
        }

        public void log(string s,object o) {
            log(new Dictionary<string, object>() { { s, o } });
        }

        public void logError(object o) {
            log("error", o);
        }

        public void logException(object o) {
            log("exception", o);
        }

        public void logEvent(string s,object o) {
            log("event:"+s, o);
        }

        #region IDisposable Members

        public void Dispose()
        {
            Write("]");
        }

        #endregion

        ~Logger()
        {
            Dispose();
        }
    }
    
    #endregion

    public delegate string getOptionDelegate(Dictionary<string, string> options);

    sealed class TestDispatchTableValues {
        public delegate object testRoutineDelegate();

        public TestDispatchTableValues(
            Test routine,
            string[] requiredOptions) {
            this.routine = routine;
            this.requiredOptions = (requiredOptions == null) ? null : new List<string>(requiredOptions);
        }

        public Test routine;
        public List<string> requiredOptions;
    }

    class Dispatcher {
        private Logger _theLogger = new LogFactory().theLogger();
        private Dictionary<string, string> _options;

        private Dictionary<string, TestDispatchTableValues> dispatchTable =
           new Dictionary<string, TestDispatchTableValues>();
      

        #region create
        public Dispatcher(Dictionary<string, string> options){
            this._options=options;
        }
        #endregion

        public List<string> Keys() {
            return dispatchTable.Keys.ToList();
        }

        public void Add(string key, TestDispatchTableValues value) {
            dispatchTable.Add(key, value);
        }

        public void dispatchTest() {
            if (!dispatchTable.ContainsKey(this._options["test"])) {
                _theLogger.log("invalid test name -- run test list to get a list");
                Environment.Exit(1);
            } else {
                var test = dispatchTable[this._options["test"]];
                System.Diagnostics.Debug.Assert(test != null);

                if (!areAllOptionsSet(test)) {
                    _theLogger.logError("missing options");
                    Environment.Exit(1);
                } else {
                    runTest(this._options["test"], test);
                }
            }
        }

        private void runTest(string testName, TestDispatchTableValues test) {
            var numargs=test.requiredOptions.Count;

            _theLogger.log("test-args", _options);
            try {
                var o = (numargs == 0) ? (test.routine as Test0).Test() :
                        (numargs == 1) ? (test.routine as Test1).Test(_options[test.requiredOptions[0]]) :
                        (numargs == 2) ? (test.routine as Test2).Test(_options[test.requiredOptions[0]], _options[test.requiredOptions[1]]) :
                        (numargs == 3) ? (test.routine as Test3).Test(_options[test.requiredOptions[0]], _options[test.requiredOptions[1]], _options[test.requiredOptions[2]]) :
                        (numargs == 4) ? (test.routine as Test4).Test(_options[test.requiredOptions[0]], _options[test.requiredOptions[1]], _options[test.requiredOptions[2]], _options[test.requiredOptions[3]]) :
                        (numargs == 5) ? (test.routine as Test5).Test(_options[test.requiredOptions[0]], _options[test.requiredOptions[1]], _options[test.requiredOptions[2]], _options[test.requiredOptions[3]], _options[test.requiredOptions[4]]) :
                        new Dictionary<string, object>() {{"program-error", "in Dispatcher.runTests" }};
                _theLogger.log("test-result", o);
            }
            catch (datasift.AccessDeniedException) {
                _theLogger.logException("AccessDenied");
            }
            catch (datasift.CompileFailedException) {
                _theLogger.logException("CompileFailed");
            }
            catch (datasift.InvalidDataException) {
                _theLogger.logException("InvalidData");
            }
        }

        private bool hasRequiredOptions(TestDispatchTableValues test) {
            return test.requiredOptions != null;
        }

        private bool areAllOptionsSet(TestDispatchTableValues test) {
            return !hasRequiredOptions(test) || test.requiredOptions.TrueForAll((i) => (_options.ContainsKey(i)));
        }
    }

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
                return new Dictionary<string,object>(){{"",dispatcher.Keys()}};
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
