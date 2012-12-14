using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace my_prog
{
    sealed class Logger : IDisposable
    {
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

        public void log(object o)
        {
            Write(
                (is_first ? "" : ",") +
                Newtonsoft.Json.JsonConvert.SerializeObject(o)
            );
            is_first = false;
        }

        public void log(string s, object o)
        {
            log(new Dictionary<string, object>() { { s, o } });
        }

        public void logError(object o)
        {
            log("error", o);
        }

        public void logException(object o)
        {
            log("exception", o);
        }

        public void logProgramError(object o)
        {
            log("program error", o);
        }

        public void logEvent(string s, object o)
        {
            log("event:" + s, o);
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
}
