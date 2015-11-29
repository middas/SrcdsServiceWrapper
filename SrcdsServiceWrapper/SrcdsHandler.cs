using System.Diagnostics;
using System.Threading;

namespace SrcdsServiceWrapper
{
    public class SrcdsHandler
    {
        private const int DefaultProcessWaitTime = 5000;
        private SrcdsOptions Options;

        public SrcdsHandler(SrcdsOptions options)
        {
            Options = options;
        }

        public void StartSrcds(CancellationToken cancellationToken)
        {
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
        }

        private void RunProcess(CancellationToken cancellationToken, Process process)
        {
            if (!cancellationToken.IsCancellationRequested)
            {
                process.Start();

                do
                {
                    process.WaitForExit(DefaultProcessWaitTime);
                } while (!cancellationToken.IsCancellationRequested && !process.HasExited);
            }

            if (!process.HasExited)
            {
                process.Kill();
            }
        }

        private void RunSrcds(CancellationToken cancellationToken)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo(Options.SrcdsPath, Options.SrcdsParameters);
            using (var srcdsProcess = new Process())
            {
                srcdsProcess.StartInfo = startInfo;
                RunProcess(cancellationToken, srcdsProcess);
            }
        }

        private void RunUpdates(CancellationToken cancellationToken)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo(Options.SteamCmdPath, string.Concat("+runscript \"", Options.UpdateScriptPath, "\""));
            using (var updateProcess = new Process())
            {
                updateProcess.StartInfo = startInfo;
                RunProcess(cancellationToken, updateProcess);
            }
        }
    }
}