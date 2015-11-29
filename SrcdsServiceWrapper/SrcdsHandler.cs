using log4net;
using System.Diagnostics;
using System.Threading;

namespace SrcdsServiceWrapper
{
    public class SrcdsHandler
    {
        private const int DefaultProcessWaitTime = 5000;
        private ILog Log;
        private SrcdsOptions Options;

        public SrcdsHandler(SrcdsOptions options)
        {
            Log = LogManager.GetLogger(typeof(SrcdsHandler));
            Options = options;
        }

        public bool HasExited { get; private set; }

        public void StartSrcds(CancellationToken cancellationToken)
        {
            HasExited = false;
            Log.Debug("Starting Srcds");

            do
            {
                if (Options.CheckForUpdates && !cancellationToken.IsCancellationRequested)
                {
                    RunUpdates(cancellationToken);
                }

                if (!cancellationToken.IsCancellationRequested)
                {
                    RunSrcds(cancellationToken);
                }
            } while (Options.RestartOnCrash && !cancellationToken.IsCancellationRequested);

            Log.Debug("Stopping Srcds");
            HasExited = true;
        }

        private void RunProcess(CancellationToken cancellationToken, Process process)
        {
            if (!cancellationToken.IsCancellationRequested)
            {
                Log.Debug("Starting process...");
                if (process.Start())
                {
                    do
                    {
                        process.WaitForExit(DefaultProcessWaitTime);
                    } while (!cancellationToken.IsCancellationRequested && !process.HasExited);
                }
                else
                {
                    Log.Debug("Process failed to launch");
                }
            }

            if (!process.HasExited)
            {
                Log.Debug("Cancellation requested: killing process");
                process.Kill();
            }

            Log.Debug("Process closed");
        }

        private void RunSrcds(CancellationToken cancellationToken)
        {
            Log.Debug("Running Srcds");
            ProcessStartInfo startInfo = new ProcessStartInfo(Options.SrcdsPath, Options.SrcdsParameters);
            using (var srcdsProcess = new Process())
            {
                srcdsProcess.StartInfo = startInfo;
                RunProcess(cancellationToken, srcdsProcess);
            }
        }

        private void RunUpdates(CancellationToken cancellationToken)
        {
            Log.Debug("Running Updater");
            ProcessStartInfo startInfo = new ProcessStartInfo(Options.SteamCmdPath, string.Concat("+runscript \"", Options.UpdateScriptPath, "\""));
            using (var updateProcess = new Process())
            {
                updateProcess.StartInfo = startInfo;
                RunProcess(cancellationToken, updateProcess);
            }
        }
    }
}