using System;
using System.Collections.Generic;
using Containers;
using Telegram.Bot.Args;
using Telegram.Bot.Types.Enums;

namespace TelegramBot
{
    public partial class BotService : IDisposable
    {
        private void CheckAccessLevel(GrantedUser user, IEnumerable<AccessType> access)
        {
            foreach (var type in access)
            {
                if (type == user.AccessType)
                {
                    return;
                }
            }

            throw new MemberAccessException();
        }


        private void BotOnOnMessage(object sender, MessageEventArgs messageEventArgs)
        {
            try
            {
                var user = _Users.Find(
                                       s => s.Username.Equals(
                                                              messageEventArgs.Message.From.Username,
                                                              StringComparison.InvariantCultureIgnoreCase));
                if (user == null ||
                    String.IsNullOrEmpty(user.Username))
                {
                    _Bot.SendTextMessageAsync(
                                              messageEventArgs.Message.Chat.Id,
                                              "You are not auth user! " + messageEventArgs.Message.From.Username);
                }
                else
                {
                    if (messageEventArgs.Message.Type != MessageType.TextMessage)
                    {
                        SendDefaultMessage(messageEventArgs);
                    }                    
                    else
                    {
                        Log.Info(
                                 "Received from: {0}, AccessLevel: {1}\nMessage: {2}",
                                 messageEventArgs.Message.From.Username,
                                 user.AccessType,
                                 messageEventArgs.Message.Text);

                        var text = messageEventArgs.Message.Text;
                        if (text.StartsWith(CommandsList.Help) || text.StartsWith(Emoji.Help))
                        {
                            SendHelpMessage(messageEventArgs.Message.Chat.Id);
                        }
                        else if (text.StartsWith(CommandsList.Status) || text.StartsWith(Emoji.ServiceStatus))
                        {
                            CheckAccessLevel(user, new List<AccessType> { AccessType.All, AccessType.SystemStatus });
                            GetServiceStatus(messageEventArgs.Message.Chat.Id, new List<string>());
                        }
                        else if (text.StartsWith(CommandsList.System) || text.StartsWith(Emoji.SystemStatus))
                        {
                            CheckAccessLevel(user, new List<AccessType> { AccessType.All, AccessType.SystemStatus });
                            GetSystemStatus(messageEventArgs.Message.Chat.Id);
                        }
                        else if (text.StartsWith(CommandsList.Log) || text.StartsWith(Emoji.Log))
                        {
                            CheckAccessLevel(user, new List<AccessType> { AccessType.All });
                            GetLog(messageEventArgs.Message.Chat.Id, new List<string>());
                        }
                        else if (text.StartsWith(CommandsList.ServiceStart) || text.StartsWith(Emoji.ServiceUp))
                        {
                            CheckAccessLevel(user, new List<AccessType> { AccessType.All });
                            ServiceStart(messageEventArgs.Message.Chat.Id, new List<string>());
                        }
                        else if (text.StartsWith(CommandsList.ServiceStop) || text.StartsWith(Emoji.ServiceDown))
                        {
                            CheckAccessLevel(user, new List<AccessType> { AccessType.All });
                            ServiceStop(messageEventArgs.Message.Chat.Id, new List<string>());
                        }
                        else if (text.StartsWith(CommandsList.ServiceRestart) || text.StartsWith(Emoji.ServiceRestart))
                        {
                            CheckAccessLevel(user, new List<AccessType> { AccessType.All });
                            RestartService(messageEventArgs.Message.Chat.Id, new List<string>());
                        }
                        else if (text.StartsWith(CommandsList.Start))
                        {
                            StartBroadcast(user, messageEventArgs);
                            SendMenu(messageEventArgs.Message.Chat.Id);
                        }
                        else if (text.StartsWith(CommandsList.Stop))
                        {
                            StopBroadcast(messageEventArgs);
                        }
                        else
                        {
                            SendHelpMessage(messageEventArgs.Message.Chat.Id);
                        }
                    }
                }
            }
            catch (MemberAccessException)
            {
                SendNoAccess(messageEventArgs);
            }
            catch (ArgumentException)
            {
                try
                {
                    _Bot.SendTextMessageAsync(
                                              messageEventArgs.Message.Chat.Id,
                                              "Invalid arguments");
                }
                catch (Exception e)
                {
                    Log.Error(e, e.Message);
                }
            }
            catch (Exception exp)
            {
                Log.Error(exp, exp.Message);
            }
        }
    }
}
