using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskbarCustomizer.Helpers {
    public class DebugLogger {
        private const string FILE_NAME = "debug.log";
        private bool LogExists => File.Exists(FILE_NAME);
        
        public void AppendLog(Exception exception) {
            if (LogExists) {
                using (StreamWriter sw = new StreamWriter(FILE_NAME)) {
                    sw.WriteLine(exception);
                }
            } 
        }
    }
}
