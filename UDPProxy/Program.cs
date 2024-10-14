using Microsoft.Extensions.DependencyInjection;

using UDPProxy.CommandLine;
using UDPProxy.Models;

namespace UDPProxy;

partial class Program
{
    
    static async Task Main(string[] args)
    {
        var runargs = ArgumentParser<RunArgs>.Parse(args);

        if (runargs == null)
        {
            return;
        }

        CancellationTokenSource cts = new();

        Console.CancelKeyPress += async (s, e) =>
        {
            e.Cancel = true;
            await cts.CancelAsync();
            Environment.Exit(0);
        };

        //handle unhandled exceptions
        AppDomain.CurrentDomain.UnhandledException += (s, e) =>
        {
            //LogLine("Unhandled Exception");
            Console.WriteLine(((Exception)e.ExceptionObject).Message);
            cts.Cancel();
        };


        var serviceProvider = CreateServices();

        await serviceProvider.GetRequiredService<Application>().RunAsync(cts.Token).ConfigureAwait(false);

        if(!cts.IsCancellationRequested)
        {
            await cts.CancelAsync();
        }
        Console.ReadLine();

        ServiceProvider CreateServices()
        {
            var serviceProvider = new ServiceCollection()
                //.AddLogging()
                .AddSingleton<Application>()
                .AddSingleton(cts)               
                .AddSingleton(sp => runargs)
                .AddSingleton<IUdpServer, UdpServer>() 
                .AddTransient<IAppLauncher, AppLauncher>()
                .BuildServiceProvider();

            return serviceProvider;
        }

        
    }
}