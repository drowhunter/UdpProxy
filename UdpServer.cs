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
                Console.WriteLine("No ListenPort was provided");
                
            }

            using var sendSocket = new UdpClient();
            

            using var boundSocket = new UdpClient(_args.ListenPort);
            

            while (!cancellationToken.IsCancellationRequested)
            {
                try { 
                    var result = await boundSocket.ReceiveAsync(cancellationToken).ConfigureAwait(false);
                
                    var buffer = result.Buffer;
                    var remoteEP = result.RemoteEndPoint;

                    //Console.WriteLine($"Received data from {remoteEP} {buffer.Length} bytes");
                    Console.Write($".");

                    //if (!clients.Any(c => remoteEP.Equals(c.Key)))
                    //{
                    //    var dt = DateTime.Now;
                    //    //clients.Add(remoteEP, dt);
                    //    Console.WriteLine($"[{dt:u}] {remoteEP} sent data. {clients.Count} game connected.");
                    //}
                    //if (clients.Any())
                    //{
                    foreach (var port in _args.FwdPorts)
                    {
                        try
                        {

                            var ep = new IPEndPoint(IPAddress.Loopback, port);
                            int s = await sendSocket.SendAsync(buffer, ep, cancellationToken);
                        
                            if (s == 0)
                            {
                                Console.Write("-");
                            }
                            else
                            {
                                //clients.Add(ep, DateTime.Now);
                                Console.Write("o");
                            }
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine("xx");
                            
                        }
                    }
                } 
                catch(Exception x)
                {
                    Console.WriteLine("xx");
                }
            }

            Console.WriteLine("UDP Server Ended");
        }

        private void RemoveClient(IPEndPoint remoteEP)
        {
            if (clients.ContainsKey(remoteEP))
            {
                {
                    var ep = clients.First(_ => _.Key.Equals(_.Key));
                    Console.WriteLine($"disconnection {ep.Key} after {DateTime.Now.Subtract(ep.Value).ToHuman()}");
                    if (this.clients.Remove(ep.Key))
                    {
                        Console.WriteLine($"Removed {ep.Key}. {clients.Count} clients left.");
                    }
                }
            }
        }
    }
}
