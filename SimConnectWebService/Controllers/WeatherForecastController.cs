using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SimConnectWebService.Clients;
using SimConnectWebService.Clients.SimVar;

namespace SimConnectWebService.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private readonly SimConnectClient simConnectClient;
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly ILogger<WeatherForecastController> _logger;

        public WeatherForecastController(ILogger<WeatherForecastController> logger, SimConnectClient simConnectClient)
        {
            _logger = logger;
            this.simConnectClient = simConnectClient;
        }

        [HttpGet]
        public IEnumerable<WeatherForecast> Get()
        {
            var rng = new Random();
            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateTime.Now.AddDays(index),
                TemperatureC = rng.Next(-20, 55),
                Summary = Summaries[rng.Next(Summaries.Length)]
            })
            .ToArray();
        }

        [HttpGet("Status")]
        public async Task<String> Status()
        {
            RecvSimobjectDataRequest<double> request = simConnectClient.RecvSimobjectDataRequestFactory.CreateRecvSimobjectDataRequest<double>("Plane Longitude", "degrees");
            double value = await request.RequestValueAsync();
            return simConnectClient.IsConnected ? value.ToString() : "Nope";
        }
        [HttpGet("RequestDataOnSimObject")]
        public async Task<String> RequestDataOnSimObject(string name, string units)
        {
            RecvSimobjectDataRequest<string> request = simConnectClient.RecvSimobjectDataRequestFactory.CreateRecvSimobjectDataRequest<string>(name, units);
            string value = await request.RequestValueAsync();
            return simConnectClient.IsConnected ? value : "Nope";
        }
    }
}
