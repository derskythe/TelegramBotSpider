using System;
using Containers;
using TelegramBot;
using TelegramBotSpider.Properties;

namespace TelegramBotSpider
{
    static class Program
    {
        static void Main()
        {
            BotService bot;
#if DEBUG
            bot = new BotService(
                                 "387574122:AAGcSL1BNU2enJhjOKSAU1tZ2-fEM6JlUko",
                                 ServiceHelpers.GetLocalOctet(),
                                 Settings.Default.PrivateCert);
#else
            bot = new BotService(
                                      "386060207:AAGfIbaGw00N27YBgy4IAp2_0sGRbjqD_84",
                                      ServiceHelpers.GetLocalOctet(),
                                      Settings.Default.PrivateCert);
#endif
            Console.WriteLine(@"Press ENTER to exit");
            Console.ReadLine();

            bot.Dispose();
        }
    }
}
