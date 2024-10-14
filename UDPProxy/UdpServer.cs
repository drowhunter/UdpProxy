using System.Net;
using System.Net.Sockets;

using UDPProxy.Models;
using UDPProxy.Extensions;

namespace UDPProxy;

partial class Program
{
    public interface IUdpServer
    {
        Task StartAsync(CancellationToken cancellationToken = default);
    }

    public class UdpServer : IUdpServer
    {
        private RunArgs _args;

        private CancellationTokenSource cts = new();

        Dictionary<IPEndPoint, DateTime> clients = new();

        public UdpServer(RunArgs args)
        {
            _args = args;
        }



        public async Task StartAsync(CancellationToken cancellationToken = default)
        {

            if(_args.ListenPort == 0)
            {
                LogLine("No ListenPort was provided");
                return;
            }

            
            //outSocket.Connect(IPAddress.Loopback, 0);

            using var inSocket = new UdpClient(_args.ListenPort);


            LogLine($"Listening on port {_args.ListenPort}...");

            while (!cancellationToken.IsCancellationRequested)
            {
                try { 
                    var result = await inSocket.ReceiveAsync(cancellationToken).ConfigureAwait(false);
                
                    var buffer = result.Buffer;
                    var remoteEP = result.RemoteEndPoint;

                    //Log($"Received data from {remoteEP} {buffer.Length} bytes");
                    if(_args.DebugMode)
                    {
                        Log(".");
                    }
                    

                    
                    foreach (var port in _args.FwdPorts)
                    {
                        try
                        {
                            using var outSocket = new UdpClient();
                            var ep = new IPEndPoint(IPAddress.Loopback, port);
                            int s = await outSocket.SendAsync(buffer, ep, cancellationToken);
                        
                            if (s == 0)
                            {
                                if (_args.DebugMode)
                                    Log("-");
                            }
                            else
                            {
                                //clients.Add(ep, DateTime.Now);
                                if (_args.DebugMode)
                                    Log("o");
                            }
                        }
                        catch (Exception e)
                        {
                            if (_args.DebugMode)
                                Log("!");
                            
                        }
                    }
                } 
                catch(Exception x)
                {
                    if (_args.DebugMode)
                        Log("!");
                }
            }

            LogLine("Stopped Listening");
        }

        //private void RemoveClient(IPEndPoint remoteEP)
        //{
        //    if (clients.ContainsKey(remoteEP))
        //    {
        //        {
        //            var ep = clients.First(_ => _.Key.Equals(_.Key));
        //            LogLine($"disconnection {ep.Key} after {DateTime.Now.Subtract(ep.Value).ToHuman()}");
        //            if (this.clients.Remove(ep.Key))
        //            {
        //                LogLine($"Removed {ep.Key}. {clients.Count} clients left.");
        //            }
        //        }
        //    }
        //}

        private static void Log(string message)
        {
            Console.Write(message);
        }

        private static void LogLine(string message)
        {
            Console.WriteLine("UDP: " + message);
        }

        private static void LogError(string message)
        {
            var color = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Red;
            LogLine(message);
            Console.ForegroundColor = color;
        }
    }
}
