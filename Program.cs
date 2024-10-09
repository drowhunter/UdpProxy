﻿using Microsoft.Extensions.DependencyInjection;

using UDPProxy.CommandLine;
using UDPProxy.Models;

namespace UDPProxy;

partial class Program
{
    
    static async Task Main(string[] args)
    {
        CancellationTokenSource cts = new();

        Console.CancelKeyPress += async (s, e) =>
        {
            e.Cancel = true;
            await cts.CancelAsync();
            Environment.Exit(0);
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
                .AddSingleton(sp => CommandLineParser<RunArgs>.Parse(args))
                .AddSingleton<IUdpServer, UdpServer>() 
                .AddTransient<IAppLauncher, AppLauncher>()
                .BuildServiceProvider();

            return serviceProvider;
        }
    }
}