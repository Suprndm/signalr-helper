using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SignalRHelper.WebServerTester
{
    public class Logger
    {
        private readonly IList<string> _allLogs;

        public Logger()
        {
            _allLogs = new List<string>();
        }

        public void Log(string message)
        {
            _allLogs.Add($"{DateTimeOffset.UtcNow} : {message}");
        }

        public IList<string> GetAllLogs()
        {
            return _allLogs;
        }
    }
}
