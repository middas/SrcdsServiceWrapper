using CommandLine;
using System;
using System.IO;
using System.ServiceProcess;

namespace SrcdsServiceWrapper
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        private static void Main(string[] args)
        {
            SrcdsOptions options = new SrcdsOptions();
            if (Parser.Default.ParseArguments(args, options))
            {
                if (!File.Exists(options.SrcdsPath))
                {
                    Console.WriteLine(options.GetUsage());
                    Environment.Exit(1);
                }

                if (options.CheckForUpdates && ((string.IsNullOrEmpty(options.SteamCmdPath) && File.Exists(options.SteamCmdPath)) || string.IsNullOrEmpty(options.UpdateScriptPath)))
                {
                    Console.WriteLine(options.GetUsage());
                    Environment.Exit(1);
                }

                ServiceBase[] ServicesToRun;
                ServicesToRun = new ServiceBase[]
                {
                    new SrcdsService(options)
                };
                ServiceBase.Run(ServicesToRun);
            }
        }
    }
}