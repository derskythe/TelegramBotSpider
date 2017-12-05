using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using Containers;
using NLog;
using Telegram.Bot.Args;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using TelegramBot.Properties;
using TelegramBot.Settings;
using TelegramBot.Stuff;
using File = System.IO.File;

namespace TelegramBot
{
    public partial class BotService : IDisposable
    {
        // ReSharper disable FieldCanBeMadeReadOnly.Local
        // ReSharper disable InconsistentNaming
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();
        // ReSharper restore InconsistentNaming
        // ReSharper restore FieldCanBeMadeReadOnly.Local

        private readonly Telegram.Bot.TelegramBotClient _Bot;
        private readonly List<ControlledService> _Services = new List<ControlledService>();
        private readonly List<GrantedUser> _Users = new List<GrantedUser>();
        private readonly List<string> _KnownUsers = new List<string>();
        private readonly FileSystemWatcher _Watcher = new FileSystemWatcher();
        private readonly Dictionary<string, RemoteAgent> _RemoteAgents = new Dictionary<string, RemoteAgent>();
        private readonly Timer _UpdateServiceListTimer;
        private const int TIMEOUT = 5 * 60 * 1000;
        private readonly String _LocalOctet;
        private static readonly int[] _LogNumLines = { 100, 500, 1000, 5000, 10000, 50000 };

        public BotService(string apiId, string localOctet, string privateCert)
        {
            _LocalOctet = localOctet;
            AgentTokenizer.Init(privateCert);

            var config = ConfigurationManager.OpenExeConfiguration(Assembly.GetExecutingAssembly().Location);
            // Load Controlled Services
            LoadControlledServices(config);
            LoadGrantedUsers(config);

            _Bot = new Telegram.Bot.TelegramBotClient(apiId);
            var me = _Bot.GetMeAsync();
            while (!me.IsCompleted)
            {
                Thread.Sleep(100);
            }

            Log.Info("Init bot: " + me.Result.FirstName);

            _Bot.OnMessage += BotOnOnMessage;
            _Bot.OnCallbackQuery += BotOnOnCallbackQuery;
            _Bot.StartReceiving();


            if (!String.IsNullOrEmpty(Properties.Settings.Default.ChatUsers))
            {
                Log.Info("Known chat users: " + Properties.Settings.Default.ChatUsers);
                _KnownUsers = new List<string>(Properties.Settings.Default.ChatUsers.Split(';'));
            }

            foreach (var user in _KnownUsers)
            {
                try
                {
                    var response = _Bot.SendTextMessageAsync(
                                              Convert.ToInt64(user),
                                              "Bot started!");
                    Wait(response);
                    SendMenu(Convert.ToInt64(user));
                }
                catch (Exception exp)
                {
                    Log.Error(exp, exp.Message);
                }
            }

            InitFileWatcher();
            _UpdateServiceListTimer = new Timer(UpdateServiceListTimer, null, 0, TIMEOUT);
        }

        private void UpdateServiceListTimer(object state)
        {
            try
            {
                foreach (var remoteAgent in _RemoteAgents)
                {
                    try
                    {
                        var request = new DefaultCommandRequest { Command = CommandsList.Start };

                        foreach (var service in remoteAgent.Value.Services)
                        {
                            var str = new StringBuilder();
                            str.Append(service.Key).Append(";").Append(service.Name).Append(";")
                                    .Append(service.ServiceName).Append(";")
                                    .Append(service.Alias).Append(";").Append(service.Path);
                            request.Arguments.Add(str.ToString());
                        }
                        var result = AgentTokenizer.SendCommand(remoteAgent.Value.Ip, remoteAgent.Value.PublicKey, request);
                        if (result.Result == ResultCodes.Ok)
                        {
                            Log.Info("Send ok");
                        }
                        else
                        {
                            Log.Warn("Send failed. " + result.Result);
                        }
                    }
                    catch (Exception exp)
                    {
                        Log.Error(exp, exp.Message);
                    }
                }
            }
            catch (Exception exp)
            {
                Log.Error(exp, exp.Message);
            }
        }

        private void InitFileWatcher()
        {
            var watcherPath = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) + Path.DirectorySeparatorChar +
                              "broadcast" + Path.DirectorySeparatorChar;
            Log.Info("Watcher Path: {0}", watcherPath);
            if (!Directory.Exists(watcherPath))
            {
                Directory.CreateDirectory(watcherPath);
            }
            _Watcher.Path = watcherPath;
            _Watcher.NotifyFilter = NotifyFilters.FileName;
            _Watcher.Filter = "*.txt";
            _Watcher.Created += WatcherOnChanged;
            _Watcher.Changed += WatcherOnChanged;
            _Watcher.EnableRaisingEvents = true;
        }

        private void LoadGrantedUsers(Configuration config)
        {
            var grantedUsersSection = config.GetSection("grantedUsers") as UserSettingsSection;

            if (grantedUsersSection == null)
            {
                throw new Exception("Can't find settings grantedUsersSection!");
            }

            foreach (UserSettingsElement item in grantedUsersSection.Channels)
            {
                Log.Info("Add granted user {0}, {1}", item.Name, item.AccessType);
                _Users.Add(new GrantedUser(item.Name, (AccessType)item.AccessType));
            }
        }

        private void LoadControlledServices(Configuration config)
        {
            var remoteKeySettingsSection = config.GetSection("spiderAgents") as RemoteKeySettingsSection;

            if (remoteKeySettingsSection == null)
            {
                throw new Exception("Can't find settings remoteKeySettingsSection!");
            }
            var agents = new List<RemoteAgent>();

            foreach (RemoteKeySettingsElement item in remoteKeySettingsSection.Channels)
            {
                Log.Info("Add remote agent {0}, IP: {1}", item.Key, item.Ip);
                if (!AgentTokenizer.CheckCert(item.PublicCert))
                {
                    throw new Exception("Can't add cert for " + item.Key);
                }
                agents.Add(new RemoteAgent(item.Key, item.PublicCert, item.Ip, new List<ControlledService>()));
            }

            var serviceItemSettingsSection = config.GetSection("controlledServices") as ServiceItemSettingsSection;

            if (serviceItemSettingsSection == null)
            {
                throw new Exception("Can't find settings serviceItemSettingsSection!");
            }

            var str = new StringBuilder();
            int i = 1;
            foreach (ServiceItemSettingsElement item in serviceItemSettingsSection.Channels)
            {
                var logFiles = String.IsNullOrEmpty(item.LogFiles)
                    ? new List<string>()
                    : new List<string>(item.LogFiles.Split(';'));

                var service = new ControlledService(
                                                    i++,
                                                    item.Key,
                                                    item.Name,
                                                    item.ServiceName,
                                                    item.Alias,
                                                    item.Path,
                                                    item.RemoteKey,
                                                    logFiles);
                Log.Info("Add controlled service " + service);
                _Services.Add(service);
                str.Append(item.Key).Append(" (").Append(item.Alias).Append(")\n");

                if (!String.IsNullOrEmpty(service.RemoteKey))
                {
                    var agent = agents.Find(remoteAgent => remoteAgent.Key == service.RemoteKey);
                    agent.Services.Add(service);
                }
            }

            foreach (var agent in agents)
            {
                _RemoteAgents.Add(agent.Key, agent);
            }
        }

        private static bool FileIsReady(string path)
        {
            try
            {
                using (var file = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    return true;
                }
            }
            catch (IOException)
            {
                return false;
            }
        }

        private void WatcherOnChanged(object sender, FileSystemEventArgs fileSystemEventArgs)
        {
            try
            {
                Log.Info("Find file " + fileSystemEventArgs.FullPath);
                if (!FileIsReady(fileSystemEventArgs.FullPath))
                {
                    Log.Debug("FileIsReady");
                    return;
                }
                if (!File.Exists(fileSystemEventArgs.FullPath))
                {
                    Log.Debug("!File.Exists");
                    return;
                }
                Thread.Sleep(1000);
                Log.Info("Find file " + fileSystemEventArgs.FullPath);
                var content = File.ReadAllText(fileSystemEventArgs.FullPath);
                Log.Debug("File content: " + content);
                foreach (var user in _KnownUsers)
                {
                    _Bot.SendTextMessageAsync(
                                              Convert.ToInt64(user),
                                              content);
                }

                File.Delete(fileSystemEventArgs.FullPath);
            }
            catch (Exception e)
            {
                Log.Error(e, e.Message);
            }
        }



        private void StopBroadcast(MessageEventArgs messageEventArgs)
        {
            _KnownUsers.Remove(messageEventArgs.Message.Chat.Id.ToString());
            Properties.Settings.Default.ChatUsers = String.Join(";", _Users);
            Properties.Settings.Default.Save();

            _Bot.SendTextMessageAsync(
                                      messageEventArgs.Message.Chat.Id,
                                      "Broadcast was stopped");
        }

        private void StartBroadcast(GrantedUser user, MessageEventArgs messageEventArgs)
        {
            var chatId = messageEventArgs.Message.Chat.Id.ToString();
            if (_KnownUsers.Contains(chatId))
            {
                _Bot.SendTextMessageAsync(
                                          messageEventArgs.Message.Chat.Id,
                                          "Already added. Your access level is " + user.AccessType);
            }
            else
            {
                _Bot.SendTextMessageAsync(
                                          messageEventArgs.Message.Chat.Id,
                                          "Welcome to broadcast! Your access level is " + user.AccessType);
                _KnownUsers.Add(messageEventArgs.Message.Chat.Id.ToString());

                Properties.Settings.Default.ChatUsers = String.Join(";", _KnownUsers);
                Properties.Settings.Default.Save();
            }
        }

        private ControlledService GetService(String text)
        {
            foreach (var service in _Services)
            {
                if (text.Contains(service.Name) ||
                    text.Contains(service.Key) ||
                    text.Contains(service.Alias))
                {
                    return service;
                }
            }

            throw new ArgumentException();
        }

        private ControlledService GetService(int id)
        {
            foreach (var service in _Services)
            {
                if (id == service.Id)
                {
                    return service;
                }
            }

            throw new ArgumentException();
        }        

        private void SendMenu(long chatId)
        {
            var list = new List<ButtonType>
            {
                new ButtonType(Emoji.SystemStatus + " " + Resources.SystemStatus, false, false),
                new ButtonType(Emoji.ServiceStatus + " " + Resources.ServiceStatus, false, false),
                new ButtonType(Emoji.Log + " " + Resources.Log, false, false),
                new ButtonType(Emoji.ServiceUp + " " + Resources.StartService, false, false),
                new ButtonType(Emoji.ServiceDown + " " + Resources.StopService, false, false),
                new ButtonType(Emoji.ServiceRestart + " " + Resources.RestartService, false, false)
            };

            var keyboardMarkup =
                    new ReplyKeyboardMarkup(Keyboards.GetKeyboard(list), true);


            var selectButton = _Bot.SendTextMessageAsync(
                                                         chatId,
                                                         Resources.SelectAction,
                                                         false,
                                                         false,
                                                         0,
                                                         keyboardMarkup,
                                                         ParseMode.Markdown);
            Wait(selectButton);
        }

        private void SendNoAccess(MessageEventArgs messageEventArgs)
        {
            var response = _Bot.SendTextMessageAsync(
                                                     messageEventArgs.Message.Chat.Id,
                                                     Resources.AccessDenied);
            Wait(response);
        }

        private void SendDefaultMessage(MessageEventArgs messageEventArgs)
        {
            var response = _Bot.SendTextMessageAsync(
                                                     messageEventArgs.Message.Chat.Id,
                                                     String.Format(
                                                                   Resources.DefaultMessage,
                                                                   messageEventArgs.Message.From.Username));
            Wait(response);
            SendMenu(messageEventArgs.Message.Chat.Id);
        }

        public void Dispose()
        {
            foreach (var user in _KnownUsers)
            {
                var result = _Bot.SendTextMessageAsync(
                                                       Convert.ToInt64(user),
                                                       "Bot was stopped!");
                long timer = 0;
                while (!result.IsCompleted && timer < 1000)
                {
                    Thread.Sleep(100);
                    timer += 100;
                }
            }

            _Bot.StopReceiving();

            if (_Watcher != null)
            {
                _Watcher.EnableRaisingEvents = false;
                _Watcher.Dispose();
            }

            if (_UpdateServiceListTimer != null)
            {
                _UpdateServiceListTimer.Dispose();
            }
        }
    }
}
