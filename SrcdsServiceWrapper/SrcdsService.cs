using System.ServiceProcess;
using System.Threading;

namespace SrcdsServiceWrapper
{
    public partial class SrcdsService : ServiceBase
    {
        private CancellationTokenSource CancellationSource;
        private SrcdsOptions Options;

        public SrcdsService(SrcdsOptions options)
        {
            InitializeComponent();

            Options = options;
        }

        protected override void OnStart(string[] args)
        {
            CancellationSource = new CancellationTokenSource();

            SrcdsHandler handler = new SrcdsHandler(Options);
            handler.StartSrcds(CancellationSource.Token);
        }

        protected override void OnStop()
        {
            if (CancellationSource != null)
            {
                CancellationSource.Cancel();
                CancellationSource.Dispose();
            }

            CancellationSource = null;
        }
    }
}