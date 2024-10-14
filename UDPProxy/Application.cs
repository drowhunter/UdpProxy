using UDPProxy.Models;

namespace UDPProxy;

partial class Program
{
    public class Application
    {
        
        
        private readonly IAppLauncher _appLauncher;
        private IUdpServer _udpServer;

        public Application(IAppLauncher appLauncher, IUdpServer udpServer)
        {            
            _appLauncher = appLauncher;
            _udpServer = udpServer;
        }

        public async Task RunAsync(CancellationToken cancellationToken = default)
        {
            var t1 = _udpServer.StartAsync(cancellationToken);
            var t2 = _appLauncher.StartAsync(cancellationToken);

            var t3 = await Task.WhenAny(t1, t2).ConfigureAwait(false);

            if(t3 == t1)
            {
                Console.WriteLine("UDP Server has stopped");
            }
            else
            {
               Console.WriteLine("AppLauncher has stopped");
            }
        }
    }
}