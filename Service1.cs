using System.ServiceProcess;

namespace DesktopOrganizer
{
    public partial class Service1 : ServiceBase
    {
        public Service1()
        {
            InitializeComponent();
        }

        private Engine engine;

        protected override void OnStart(string[] args)
        {
            engine = new Engine();
        }

        protected override void OnStop()
        {
            engine.Dispose();
        }
    }
}