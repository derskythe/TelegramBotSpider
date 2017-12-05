using System.ServiceProcess;

namespace BotSpiderAgentService
{
    static class Program
    {
        public const string AppName = "TelegramBotSpiderAgentServer"; 
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main()
        {
            var servicesToRun = new ServiceBase[] 
            { 
                new BotSpiderAgentService() 
            };
            ServiceBase.Run(servicesToRun);
        }
    }
}
