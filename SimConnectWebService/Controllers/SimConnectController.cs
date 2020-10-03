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
    public class SimConnectController : ControllerBase
    {
        private SimVarMappingUtil simVarMapping = new SimVarMappingUtil();
        private readonly SimConnectClient simConnectClient;


        private readonly ILogger<SimConnectController> _logger;

        public SimConnectController(ILogger<SimConnectController> logger, SimConnectClient simConnectClient)
        {
            _logger = logger;
            this.simConnectClient = simConnectClient;
        }

        [HttpGet]
        [HttpGet("GetStatus")]
        public string GetStatus()
        {
            return JsonSerializer.Serialize(new { IsConnected = simConnectClient.IsConnected });
        }

        [HttpGet("GetBasic")]
        public async Task<String> GetBasic(uint targetObjectId)
        {

            SimVarFieldGroup simVarFieldGroup = GetBasicSimVarFieldGroup();
            SimVarRequestClient requestClient = new SimVarRequestClient(simConnectClient);
            object result = await requestClient.RequestDataOnSimObjectAsync(simVarFieldGroup, targetObjectId);
            string json = JsonSerializer.Serialize(result);
            return json;

        }


        [HttpGet("GetOther")]
        public async Task<String> GetOther(string name, string units)
        {
            SimVarFieldGroup simVarFieldGroup = GetBasicSimVarFieldGroup();

            SimVarRequestClient requestClient = new SimVarRequestClient(simConnectClient);
            List<object> result = await requestClient.RequestDataOnSimObjectTypeAsync(simVarFieldGroup);
            string json = JsonSerializer.Serialize(result);
            return json;
        }

        [HttpGet("GetOne")]
        public async Task<String> GetOne(string name, string units, string varType, uint targetObjectId)
        {
            SimVarFieldGroup simVarFieldGroup;
            if (string.IsNullOrEmpty(name))
            {
                simVarFieldGroup = GetBasicSimVarFieldGroup();
            }
            else
            {
                simVarFieldGroup = new SimVarFieldGroup("GetOneType");
                simVarFieldGroup.AddSimVarField(new SimVarField()
                {
                    VarName = name,
                    Units = units,
                    VarType = simVarMapping.GetSimVarTypeFromStringType(varType),
                });
            }
            SimVarRequestClient requestClient = new SimVarRequestClient(simConnectClient);
            object result = await requestClient.RequestDataOnSimObjectAsync(simVarFieldGroup, targetObjectId);
            string json = JsonSerializer.Serialize(result);
            return json;
        }

        private SimVarFieldGroup GetBasicSimVarFieldGroup()
        {
            SimVarFieldGroup simVarFieldGroup = new SimVarFieldGroup("BasicSimVarFieldGroup");
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
            simVarFieldGroup.AddSimVarField(new SimVarField()
            {
                VarName = "Plane Altitude",
                Units = "Feet",
                VarType = simVarMapping.GetSimVarTypeFromStringType("float")
            });
            simVarFieldGroup.AddSimVarField(new SimVarField()
            {
                VarName = "Plane Heading Degrees Magnetic",
                Units = "degree",
                VarType = simVarMapping.GetSimVarTypeFromStringType("float")
            });
            simVarFieldGroup.AddSimVarField(new SimVarField()
            {
                VarName = "MagVar",
                Units = "degree",
                VarType = simVarMapping.GetSimVarTypeFromStringType("float")
            });
            return simVarFieldGroup;
        }
    }
}
