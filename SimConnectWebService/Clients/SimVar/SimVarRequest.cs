namespace SimConnectWebService.Clients.SimVar
{
    public class SimVarRequest
    {
        public DEFINITION DefinitionId { get ; set;}
        public REQUEST RequestId { get;  set; } 
        public double Value  { get; set; }
        public string Name { get; set; }
        public string Units { get; set; }
    };
    public enum DEFINITION
    {
        Dummy = 0
    };

    public enum REQUEST
    {
        Dummy = 0
    };
}