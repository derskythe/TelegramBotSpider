using System;
using System.Net;
using System.ServiceProcess;
using System.Threading;
using Containers;
using NLog;
using TelegramBot;
using TelegramBotSpiderService.Properties;

namespace TelegramBotSpiderService
{
    public partial class TelegramBotSpiderService : ServiceBase
    {
        // ReSharper disable FieldCanBeMadeReadOnly.Local
        // ReSharper disable InconsistentNaming
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();
        // ReSharper restore InconsistentNaming
        // ReSharper restore FieldCanBeMadeReadOnly.Local
        private BotService _Bot;

        public TelegramBotSpiderService()
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

                _Bot = new BotService(
                                      "386060207:AAGfIbaGw00N27YBgy4IAp2_0sGRbjqD_84",
                                      "34",
                                      Settings.Default.PrivateCert);
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
            Log.Info("Shuting down");
            if (_Bot != null)
            {
                _Bot.Dispose();
            }
        }
    }
}
