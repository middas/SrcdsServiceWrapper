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
#if !DEBUG
            ServiceBase[] ServicesToRun;
            ServicesToRun = new ServiceBase[]
            {
                new SrcdsService()
            };
            ServiceBase.Run(ServicesToRun);
#else
            var srv = new SrcdsService();
            srv.Start(args);
#endif
        }
    }
}