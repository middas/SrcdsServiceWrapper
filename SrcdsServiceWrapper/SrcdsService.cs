using CommandLine;
using log4net;
using log4net.Config;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using System.ServiceProcess;
using System.Threading;
using System.Threading.Tasks;

namespace SrcdsServiceWrapper
{
    public partial class SrcdsService : ServiceBase
    {
        private CancellationTokenSource CancellationSource;
        private SrcdsHandler Handler;
        private ILog Log;
        private SrcdsOptions Options;

        public SrcdsService()
        {
            XmlConfigurator.Configure();
            Log = LogManager.GetLogger(typeof(SrcdsService));
            InitializeComponent();
        }

        public void Start(string[] args)
        {
            OnStart(args);
        }

        protected override void OnStart(string[] args)
        {
            Log.Debug("Start service called");
            Log.Debug(string.Concat("Args: ", string.Join(",", args)));

            if (args != null && args.Length > 0)
            {
                var argList = args.ToList();
                int pIndex = argList.IndexOf("-p");
                if (pIndex < 0)
                {
                    pIndex = argList.IndexOf("--params");
                }

                if (pIndex >= 0)
                {
                    args[pIndex + 1] = args[pIndex + 1].Replace('-', '}');
                }
                Options = new SrcdsOptions();
                if (!Parser.Default.ParseArguments(args, Options))
                {
                    Options = null;
                }
            }

            if (Options == null)
            {
                Log.Debug("No args passed, attempting to load setup.json file.");

                if (File.Exists("setup.json"))
                {
                    try
                    {
                        using (FileStream fs = new FileStream("setup.json", FileMode.Open))
                        using (StreamReader sr = new StreamReader(fs))
                        {
                            Options = JsonConvert.DeserializeObject<SrcdsOptions>(sr.ReadToEnd());
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Error("Could not parse setup.json", ex);
                    }
                }
                else
                {
                    Log.Debug("setup.json could not be found.");
                }
            }

            if (Options != null)
            {
                Options.SrcdsParameters = Options.SrcdsParameters.Replace('}', '-');
                if (!File.Exists(Options.SrcdsPath))
                {
                    Log.Debug("Srcds path was not found");
                    Log.Debug(Options.GetUsage());
                    Environment.Exit(1);
                }

                if (Options.CheckForUpdates && ((string.IsNullOrEmpty(Options.SteamCmdPath) && File.Exists(Options.SteamCmdPath)) || string.IsNullOrEmpty(Options.UpdateScriptPath)))
                {
                    Log.Debug("Invalid update settings");
                    Log.Debug(Options.GetUsage());
                    Environment.Exit(1);
                }
                CancellationSource = new CancellationTokenSource();

                Task.Run(() =>
                {
                    Handler = new SrcdsHandler(Options);
                    Handler.StartSrcds(CancellationSource.Token);
                }, CancellationSource.Token);
            }
            else
            {
                Log.Debug("Invalid command line args");
                Log.Debug(Options.GetUsage());
            }
        }

        protected override void OnStop()
        {
            if (CancellationSource != null)
            {
                Log.Debug("Stop requested");

                CancellationSource.Cancel();
                CancellationSource.Dispose();
            }

            CancellationSource = null;

            while (Handler != null && !Handler.HasExited)
            {
                Thread.Sleep(250);
            }
        }
    }
}