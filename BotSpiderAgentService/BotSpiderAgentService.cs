using System;
using System.Net;
using System.ServiceModel;
using System.ServiceProcess;
using System.Threading;
using BotSpiderAgent;
using BotSpiderAgentService.Properties;
using Containers;
using NLog;

namespace BotSpiderAgentService
{
    public partial class BotSpiderAgentService : ServiceBase
    {
        // ReSharper disable FieldCanBeMadeReadOnly.Local
        // ReSharper disable InconsistentNaming
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();
        // ReSharper restore InconsistentNaming
        // ReSharper restore FieldCanBeMadeReadOnly.Local
        private ServiceHost _WcfService;

        public BotSpiderAgentService()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            try
            {
                Log.Info("Started");
                ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
                WebRequest.DefaultWebProxy.Credentials = CredentialCache.DefaultNetworkCredentials;

                var info = new JitVersionInfo();
                Log.Info("JIT version: " + info.GetJitVersion());

                BotSpiderAgent.BotSpiderAgentService.Init(Settings.Default.PrivateCert);
                _WcfService = new ServiceHost(typeof(BotSpiderAgent.BotSpiderAgentService));
                _WcfService.Open();
                Log.Info("BotSpiderAgentService started");                
            }
            catch (Exception exp)
            {
                Log.Error(exp, exp.Message);
                Thread.Sleep(2000);
                throw;
            }
        }

        protected override void OnStop()
        {
            Log.Info("Shutdown");
            if (_WcfService != null)
            {
                _WcfService.Close();
            }
        }
    }
}
