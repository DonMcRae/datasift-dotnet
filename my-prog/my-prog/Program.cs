using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace my_prog
{
    sealed class Program {
        private readonly Logger _logger = new LogFactory().theLogger();

        private Dictionary<string, string> lineToDict(string line)
        {
            var options = new Dictionary<string, string>(); ;
            try
            {
                options = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, string>>(line);
                options = (options == null)?new Dictionary<string,string>():options;
            }
            catch (Newtonsoft.Json.JsonReaderException)
            {
                var errorMessage=new Dictionary<string,string>();
                errorMessage.Add("bad input", line);
                _logger.logError(errorMessage);         
            }

            return options;
        }

        private Dictionary<string, string> ReadLine()
        {
            var line = System.Console.ReadLine();
            if (line == null )
            {
                return null;
            }
            else
            {
                return lineToDict(line);
            }
        }

        public void start() {
            bool is_finished=false;
            while (! is_finished) {
                Dictionary<string, string> options = ReadLine();
                is_finished = (options == null);
                if (!is_finished && options.Count > 0)
                {
                    new Application(options).Run();
                }
            }
        }

        static void Main(string[] args) {
            new Program().start();
        }
    }

}
