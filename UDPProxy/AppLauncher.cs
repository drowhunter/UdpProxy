using System;
using System.Diagnostics;

using UDPProxy.Models;

namespace UDPProxy
{
    public interface IAppLauncher
    {
        Task StartAsync(CancellationToken cancellationToken = default);
    }

    public class AppLauncher : IAppLauncher
    {
        private readonly RunArgs args;

        public AppLauncher(RunArgs args)
        {
            this.args = args;
        }        

        
        public async Task StartAsync(CancellationToken cancellationToken = default)
        {
            string? processName = Path.GetFileNameWithoutExtension(args.ProcessName);
            Process? process = null;

            if (processName != null)
            {
               
                process = FindProcess(processName);

                if (process == null)
                {

                    if (!string.IsNullOrWhiteSpace(args.RunCommand))
                    {
                        if (args.RunCommand.EndsWith(".exe"))
                        {
                            
                            if (FindProcess(args.RunCommand) == null)
                                LaunchExe(args.RunCommand);
                        }
                        else
                        {
                            var x = LaunchUrl(args.RunCommand);
                            
                        }

                        process = await WaitForProcessToStartAsync(processName, timeout: TimeSpan.FromSeconds(30), cancellationToken: cancellationToken);
                    }
                    else
                    {
                        Console.WriteLine("No Run Command was provided");
                        process = await WaitForProcessToStartAsync(processName, timeout: TimeSpan.FromSeconds(30), cancellationToken: cancellationToken);
                    }
                }
            }
            else 
            {
                if (!string.IsNullOrWhiteSpace(args.RunCommand))
                {
                    if (args.RunCommand.EndsWith(".exe"))
                    {
                        process = FindProcess(Path.GetFileName(args.RunCommand));
                        if (process == null)
                        {
                            Console.WriteLine("Launching exe " + args.RunCommand);
                            process = LaunchExe(args.RunCommand);
                        }
                        else
                        {
                            Console.WriteLine("Process already running " + args.RunCommand);

                        }
                    }
                    else
                    {
                        Console.WriteLine("Run command was not an executable");
                    }
                }
                else
                {
                    Console.WriteLine("No Run Command was provided");

                }

            }

            if (process != null)
            {
                Console.WriteLine($"Found {processName} with PID {process.Id} waiting for exit...");
                try
                {
                    await process.WaitForExitAsync(cancellationToken).ConfigureAwait(false);
                    //await Task.Run(() => {
                    //    process.WaitForExit();
                    //    Console.WriteLine("Process has finished");
                        
                    //}, cancellationToken).ConfigureAwait(false);
                }
                catch(Exception x)
                {
                    Console.WriteLine(x);
                }
            }
        }


        private bool isUrl(string value)
        {
            return Uri.TryCreate(value, UriKind.Absolute, out _);
        }


        private void LaunchSteamUrl(string appId)
        {
            LaunchUrl($"steam://rungameid/{appId}");
        }

        private Process LaunchUrl(string url)
        {
            var procinfo = new ProcessStartInfo(url);
            procinfo.UseShellExecute = true;
            var p = Process.Start(procinfo);

            Console.WriteLine("Launching " + url);

            return p;
        }

        private Process LaunchExe(string exe)
        {
            var process = FindProcess(exe);
            if (process == null)
            {
                Log("Launching exe " + exe);
                process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = exe,
                        UseShellExecute = false,
                        RedirectStandardOutput = false,
                        RedirectStandardError = false,
                        CreateNoWindow = false
                    }

                };
                process.Start();
            }
            
            return process;
        }
        

        private Process? FindProcess(string processName)
        {
            if(processName.EndsWith(".exe"))
            {
                processName = Path.GetFileNameWithoutExtension(processName);
            }

            var process = Process.GetProcesses().FirstOrDefault(p => p.ProcessName.Equals(processName, StringComparison.OrdinalIgnoreCase));
            if (process != null)
            {
                Log("Found process " + processName);
            }
            else
            {
                Log("Process " + processName + " not found");
            }
            return process;
        }

        private Task<Process?> WaitForProcessToStartAsync(string processName, TimeSpan?  timeout = null, CancellationToken cancellationToken = default)
        {
            timeout ??= TimeSpan.FromSeconds(60);

            var now = DateTime.Now;

            return Task.Run(async() =>
            {
                Process? process = null;
                try
                {
                    while (!cancellationToken.IsCancellationRequested && DateTime.Now < now.Add(timeout.Value))
                    {
                        process = FindProcess(processName);                        
                        
                        if (process != null)
                        {
                            Log("Process found " + processName);
                            break;
                        }
                    
                        await Task.Delay(TimeSpan.FromSeconds(3)).ConfigureAwait(false);
                    }
                }
                catch (Exception x)
                {
                    LogError("WaitForProcessAsync:" + x.Message);
                }

                return process;
            });
        }

        private static void Log(string message)
        {
            Console.WriteLine("Process: " + message);
        }

        private static void LogError(string message)
        {
            var color = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Process: " + message);
            Console.ForegroundColor = color;
        }
    }
}
