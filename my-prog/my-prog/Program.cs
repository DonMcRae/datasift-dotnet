using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace my_prog
{
    sealed class Program {
        private readonly Logger _logger = new LogFactory().theLogger();

        public void start() {
            bool is_finished=false;
            while (! is_finished) {
                var line = System.Console.ReadLine();
                if (line == null || line == "") {
                    is_finished = true;
                } else {
                    try {
                        Dictionary<string, string> options = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, string>>(line);
                        new Application(options).Run();
                    }
                    catch (Newtonsoft.Json.JsonSerializationException) {
                        _logger.logError("input error");
                    }
                }
                System.Console.WriteLine();
            }
        }

        static void Main(string[] args) {
            new Program().start();
        }
    }

}
