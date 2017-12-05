using System.ServiceProcess;

namespace TelegramBotSpiderService
{
    static class Program
    {
        public const string AppName = "TelegramBotSpiderServer"; 
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main()
        {
            var servicesToRun = new ServiceBase[] 
            { 
                new TelegramBotSpiderService() 
            };
            ServiceBase.Run(servicesToRun);
        }
    }
}
