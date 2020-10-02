using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SimConnectWebService.Clients;
using SimConnectWebService.Clients.SimVar;
using SimConnectWebService.Clients.SimVar.Model;
using SimConnectWebService.Util;

namespace SimConnectWebService.Controllers
{
    // https://www.prepar3d.com/SDKv4/sdk/simconnect_api/managed_simconnect_projects.html

    [ApiController]
    [Route("[controller]")]
    [EnableCors("AllowAllCORSPolicy")]
    public class WeatherForecastController : ControllerBase
    {
        private SimVarMappingUtil simVarMapping = new SimVarMappingUtil();
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
            return simConnectClient.IsConnected ? "Yes" : "Nope";
        }
        [HttpGet("GetOther")]
        public async Task<String> GetOther(string name, string units)
        {
            SimVarFieldGroup simVarFieldGroup = new SimVarFieldGroup();
            simVarFieldGroup.AddSimVarField(new SimVarField()
            {
                VarName = "title",
                Units = units,
                VarType = simVarMapping.GetSimVarTypeFromStringType("string")
            });
            simVarFieldGroup.AddSimVarField(new SimVarField()
            {
                VarName = "Plane Latitude",
                Units = "degree",
                VarType = simVarMapping.GetSimVarTypeFromStringType("float")
            });
            simVarFieldGroup.AddSimVarField(new SimVarField()
            {
                VarName = "Plane Longitude",
                Units = "degree",
                VarType = simVarMapping.GetSimVarTypeFromStringType("float")
            });
            simVarFieldGroup.AddSimVarField(new SimVarField()
            {
                VarName = "ATC ID",
                Units = units,
                VarType = simVarMapping.GetSimVarTypeFromStringType("string")
            });
            simVarFieldGroup.AddSimVarField(new SimVarField()
            {
                VarName = "PLANE ALTITUDE",
                Units = "Feet",
                VarType = simVarMapping.GetSimVarTypeFromStringType("float")
            });

            SimVarRequestClient requestClient = new SimVarRequestClient(simConnectClient);
            List<object> result = await requestClient.RequestDataOnSimObjectTypeAsync(simVarFieldGroup);
            string json = JsonSerializer.Serialize(result);
            return json;
        }

        [HttpGet("GetOne")]
        public async Task<String> GetOne(string name, string units, string varType, uint targetObjectId)
        {
            SimVarFieldGroup simVarFieldGroup = new SimVarFieldGroup("GetOneType",targetObjectId);
            simVarFieldGroup.AddSimVarField(new SimVarField()
            {
                VarName = "title",
                VarType = simVarMapping.GetSimVarTypeFromStringType("string")
            });
            simVarFieldGroup.AddSimVarField(new SimVarField()
            {
                VarName = "Plane Latitude",
                Units = "degree",
                VarType = simVarMapping.GetSimVarTypeFromStringType("float")
            });
            simVarFieldGroup.AddSimVarField(new SimVarField()
            {
                VarName = "Plane Longitude",
                Units = "degree",
                VarType = simVarMapping.GetSimVarTypeFromStringType("float")
            });
            simVarFieldGroup.AddSimVarField(new SimVarField()
            {
                VarName = "ATC ID",
                VarType = simVarMapping.GetSimVarTypeFromStringType("string")
            });

            if (!string.IsNullOrEmpty(name))
            {
                simVarFieldGroup.AddSimVarField(new SimVarField()
                {
                    VarName = name,
                    Units = units,
                    VarType = simVarMapping.GetSimVarTypeFromStringType(varType),
                });

            }

            SimVarRequestClient requestClient = new SimVarRequestClient(simConnectClient);
            object result = await requestClient.RequestDataOnSimObjectAsync(simVarFieldGroup);
            string json = JsonSerializer.Serialize(result);
            return json;
        }
    }
}
