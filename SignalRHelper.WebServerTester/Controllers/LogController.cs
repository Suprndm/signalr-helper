using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace SignalRHelper.WebServerTester.Controllers
{
    [Route("api/logger")]
    public class LogController : Controller
    {
        private readonly Logger _logger;

        public LogController(Logger logger)
        {
            _logger = logger;
        }

        [HttpGet("")]
        public ObjectResult Get()
        {
            return Ok(_logger.GetAllLogs());
        }
    }
}
