using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace UDPProxy.Models
{
    public class RunArgs
    {
        [Required]
        [Description("the port to bind (listen) on for incoming data")]
        public int ListenPort { get; set; }

        [Required]
        [Description("The ports to forward data to")]
        public int[] FwdPorts { get; set; } = [];

        [Description("The command to run the application")]
        public string? RunCommand { get; set; }

        [Description("The name of the proccess to monitor")]
        public string? ProcessName { get; set; }

        public bool DebugMode { get; set; }
    }
}
