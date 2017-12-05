using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Containers;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using TelegramBot.Properties;
using TelegramBot.Stuff;

namespace TelegramBot
{
    public partial class BotService
    {
        private void SendHelpMessage(long chatId)
        {
            _Bot.SendTextMessageAsync(
                                      chatId,
                                      "Command list:\n" +
                                      "/status - for status of service\n" +
                                      "/system - get status about all controlled services\n" +
                                      "/log - get values from end of Service log\n" +
                                      "/servicestart - to start service\n" +
                                      "/servicestop - to stop service\n" +
                                      "/servicerestart - to restart service\n\n\n");
            SendMenu(chatId);
        }

        private void SendListOfServices(string command, long chatId)
        {
            var listButtons = _Services.ToDictionary(
                                                     item => item.ServiceName + " (" +
                                                             (String.IsNullOrEmpty(item.RemoteKey)
                                                                 ? _LocalOctet
                                                                 : item.RemoteKey) + ")",
                                                     item => item.Alias);

            var keyboardMarkup = new InlineKeyboardMarkup(
                                                          Keyboards.GetInlineKeyboard(
                                                                                      command,
                                                                                      chatId,
                                                                                      listButtons));
            var task = _Bot.SendTextMessageAsync(
                                                 chatId,
                                                 Resources.SelectService,
                                                 false,
                                                 false,
                                                 0,
                                                 keyboardMarkup,
                                                 ParseMode.Markdown);
            Wait(task);
        }

        #region ServiceStart

        /// <summary>
        /// Callback line:
        /// Type | ChatId | Service Name
        /// </summary>
        /// <param name="chatId"></param>
        /// <param name="argsList"></param>
        private void ServiceStart(long chatId, IReadOnlyList<string> argsList)
        {
            switch (argsList.Count)
            {
                // Step 1, send list of services
                case 0:
                    SendListOfServices(CommandsList.ServiceStart, chatId);
                    break;

                // Step 2, do command
                case 1:
                    var service = GetService(argsList[0]);

                    #region Start

                    var result = _Bot.SendTextMessageAsync(
                                                           chatId,
                                                           "Trying to *START* service " + service.Name,
                                                           false,
                                                           false,
                                                           0,
                                                           null,
                                                           ParseMode.Markdown);
                    Wait(result);
                    var operationStatus = 0;

                    if (!String.IsNullOrEmpty(service.RemoteKey))
                    {
                        var remoteAgent = _RemoteAgents[service.RemoteKey];

                        try
                        {
                            var request = new DefaultCommandRequest
                            {
                                Command = CommandsList.ServiceStart,
                                ServiceName = service.Key
                            };

                            var sendResult = AgentTokenizer.SendCommand(remoteAgent.Ip, remoteAgent.PublicKey, request);
                            if (sendResult.Result == ResultCodes.Ok)
                            {
                                Log.Info("Send ok");
                                operationStatus = 1;
                            }
                            else if (sendResult.Result == ResultCodes.InvalidState)
                            {
                                Log.Info("Invalid state");
                                operationStatus = 2;
                            }
                            else
                            {
                                Log.Warn("Send failed. " + sendResult.Result);
                            }
                        }
                        catch (Exception exp)
                        {
                            Log.Error(exp, exp.Message);
                        }
                    }
                    else
                    {
                        bool runningState;
                        ServiceHelpers.GetProcInfo(service.Name, out runningState);

                        if (!runningState)
                        {
                            operationStatus = ServiceHelpers.StartService(service.Name, service.ServiceName) ? 1 : 0;
                        }
                        else
                        {
                            operationStatus = 2;
                        }
                    }

                    Task<Message> response;
                    if (operationStatus == 1)
                    {
                        response = _Bot.SendTextMessageAsync(
                                                             chatId,
                                                             "Start completed " + service.Name + " " + Emoji.Ok);
                    }
                    else if (operationStatus == 0)
                    {
                        response = _Bot.SendTextMessageAsync(
                                                             chatId,
                                                             "Start failed " + service.Name + " " + Emoji.Failed,
                                                             false,
                                                             false,
                                                             0,
                                                             null,
                                                             ParseMode.Markdown);
                    }
                    else
                    {
                        response = _Bot.SendTextMessageAsync(
                                                             chatId,
                                                             "Service " + service.Name + " is already running");
                    }
                    Wait(response);

                    #endregion

                    break;

                default:
                    SendMenu(chatId);
                    return;
            }
        }

        #endregion

        #region ServiceStop

        private void ServiceStop(long chatId, IReadOnlyList<string> argsList)
        {
            switch (argsList.Count)
            {
                // Step 1, send list of services
                case 0:
                    SendListOfServices(CommandsList.ServiceStop, chatId);
                    break;

                // Step 2, do command
                case 1:
                    var service = GetService(argsList[0]);

                    #region Stop

                    var result = _Bot.SendTextMessageAsync(
                                                           chatId,
                                                           "Trying to *STOP* service " + service.Name,
                                                           false,
                                                           false,
                                                           0,
                                                           null,
                                                           ParseMode.Markdown);
                    Wait(result);

                    var operationStatus = 0;

                    if (!String.IsNullOrEmpty(service.RemoteKey))
                    {
                        var remoteAgent = _RemoteAgents[service.RemoteKey];

                        try
                        {
                            var request = new DefaultCommandRequest
                            {
                                Command = CommandsList.ServiceStop,
                                ServiceName = service.Key
                            };

                            var sendResult = AgentTokenizer.SendCommand(remoteAgent.Ip, remoteAgent.PublicKey, request);
                            if (sendResult.Result == ResultCodes.Ok)
                            {
                                Log.Info("Send ok");
                                operationStatus = 1;
                            }
                            else if (sendResult.Result == ResultCodes.InvalidState)
                            {
                                Log.Info("Invalid state");
                                operationStatus = 2;
                            }
                            else
                            {
                                Log.Warn("Send failed. " + sendResult.Result);
                            }
                        }
                        catch (Exception exp)
                        {
                            Log.Error(exp, exp.Message);
                        }
                    }
                    else
                    {
                        bool runningState;
                        ServiceHelpers.GetProcInfo(service.Name, out runningState);

                        if (runningState)
                        {
                            operationStatus = ServiceHelpers.StopService(service.Name, service.ServiceName) ? 1 : 0;
                        }
                        else
                        {
                            operationStatus = 2;
                        }
                    }

                    Task<Message> response;
                    if (operationStatus == 1)
                    {
                        response = _Bot.SendTextMessageAsync(
                                                             chatId,
                                                             "Stop completed " + service.Name + " " + Emoji.Ok);
                    }
                    else if (operationStatus == 0)
                    {
                        response = _Bot.SendTextMessageAsync(
                                                             chatId,
                                                             "Stop failed " + service.Name + " " + Emoji.Failed,
                                                             false,
                                                             false,
                                                             0,
                                                             null,
                                                             ParseMode.Markdown);
                    }
                    else
                    {
                        response = _Bot.SendTextMessageAsync(
                                                             chatId,
                                                             "Service " + service.Name + " is seems *NOT running*",
                                                             false,
                                                             false,
                                                             0,
                                                             null,
                                                             ParseMode.Markdown);
                    }
                    Wait(response);

                    #endregion

                    break;

                default:
                    SendMenu(chatId);
                    return;
            }
        }

        #endregion

        #region RestartService

        private void RestartService(long chatId, IReadOnlyList<string> argsList)
        {
            switch (argsList.Count)
            {
                // Step 1, send list of services
                case 0:
                    SendListOfServices(CommandsList.ServiceRestart, chatId);
                    break;

                // Step 2, do command
                case 1:
                    var service = GetService(argsList[0]);

                    #region Restart

                    bool runningState;
                    ServiceHelpers.GetProcInfo(service.Name, out runningState);

                    var result = _Bot.SendTextMessageAsync(
                                                           chatId,
                                                           "Trying to *RESTART* service " + service.Name,
                                                           false,
                                                           false,
                                                           0,
                                                           null,
                                                           ParseMode.Markdown);
                    Wait(result);

                    var operationStatus = false;

                    if (!String.IsNullOrEmpty(service.RemoteKey))
                    {
                        var remoteAgent = _RemoteAgents[service.RemoteKey];

                        try
                        {
                            var request = new DefaultCommandRequest
                            {
                                Command = CommandsList.ServiceRestart,
                                ServiceName = service.Key
                            };

                            var sendResult = AgentTokenizer.SendCommand(remoteAgent.Ip, remoteAgent.PublicKey, request);
                            if (sendResult.Result == ResultCodes.Ok)
                            {
                                Log.Info("Send ok");
                                operationStatus = true;
                            }
                            else
                            {
                                Log.Warn("Send failed. " + sendResult.Result);
                            }
                        }
                        catch (Exception exp)
                        {
                            Log.Error(exp, exp.Message);
                        }
                    }
                    else
                    {
                        operationStatus = ServiceHelpers.StopService(service.Name, service.ServiceName);
                    }

                    Task<Message> response;
                    if (operationStatus)
                    {
                        response = _Bot.SendTextMessageAsync(
                                                             chatId,
                                                             "Restart completed " + service.Name + " " + Emoji.Ok);
                    }
                    else
                    {
                        response = _Bot.SendTextMessageAsync(
                                                             chatId,
                                                             "Restart failed " + service.Name + " " + Emoji.Failed,
                                                             false,
                                                             false,
                                                             0,
                                                             null,
                                                             ParseMode.Markdown);
                    }

                    Wait(response);

                    #endregion

                    break;

                default:
                    SendMenu(chatId);
                    return;
            }
        }

        #endregion

        #region GetSystemStatus

        private void GetSystemStatus(long chatId)
        {
            _Bot.SendTextMessageAsync(
                                      chatId,
                                      "Trying to get system status",
                                      false,
                                      false,
                                      0,
                                      null,
                                      ParseMode.Markdown);

            var str = new StringBuilder();

            foreach (var service in _Services)
            {
                Log.Debug("Check service " + service);
                if (!String.IsNullOrEmpty(service.RemoteKey))
                {
                    var remoteAgent = _RemoteAgents[service.RemoteKey];

                    try
                    {
                        var request = new DefaultCommandRequest
                        {
                            Command = CommandsList.Status,
                            ServiceName = service.Key
                        };

                        var sendResult = AgentTokenizer.SendCommand(remoteAgent.Ip, remoteAgent.PublicKey, request);
                        if (sendResult.Result == ResultCodes.Ok)
                        {
                            Log.Info("Send ok");
                            foreach (var item in sendResult.Arguments)
                            {
                                str.Append(" (").Append(service.RemoteKey).Append(") ").Append(item).Append(" (")
                                        .Append(service.Alias).Append(")").Append("\n");
                            }
                        }
                        else
                        {
                            Log.Warn("Send failed. " + sendResult.Result);
                            str.Append(" (").Append(service.RemoteKey).Append(") ").Append(service.Key).Append(" ")
                                    .Append(Emoji.Failed).Append(" (").Append(service.Alias).Append(")").Append("\n");
                        }
                    }
                    catch (Exception exp)
                    {
                        Log.Error(exp, exp.Message);
                    }
                }
                else
                {
                    bool runningState;
                    var ram = ServiceHelpers.GetProcInfo(service.Name, out runningState);
                    str.Append(" (").Append(_LocalOctet).Append(")").Append(" ").Append(service.Key).Append(" ")
                            .Append(runningState ? Emoji.Ok : Emoji.Failed).Append(" - ")
                            .Append(ram).Append(" (").Append(service.Alias).Append(")").Append("\n");
                }
            }

            var response = _Bot.SendTextMessageAsync(
                                                     chatId,
                                                     str.ToString(),
                                                     false,
                                                     false,
                                                     0,
                                                     null,
                                                     ParseMode.Markdown);
            Wait(response);
            SendMenu(chatId);
        }

        #endregion

        #region GetServiceStatus

        private void GetServiceStatus(long chatId, IReadOnlyList<string> argsList)
        {
            switch (argsList.Count)
            {
                // Step 1, send list of services
                case 0:
                    SendListOfServices(CommandsList.Status, chatId);
                    break;

                // Step 2, do command
                case 1:
                    var service = GetService(argsList[0]);

                    #region Status

                    if (!String.IsNullOrEmpty(service.RemoteKey))
                    {
                        var remoteAgent = _RemoteAgents[service.RemoteKey];

                        try
                        {
                            var request = new DefaultCommandRequest
                            {
                                Command = CommandsList.Status,
                                ServiceName = service.Key
                            };

                            var sendResult = AgentTokenizer.SendCommand(remoteAgent.Ip, remoteAgent.PublicKey, request);
                            var str = new StringBuilder();
                            if (sendResult.Result == ResultCodes.Ok)
                            {
                                Log.Info("Send ok");
                                foreach (var item in sendResult.Arguments)
                                {
                                    str.Append(" (").Append(service.RemoteKey).Append(") ").Append(item).Append("\n");
                                }
                            }
                            else
                            {
                                Log.Warn("Send failed. " + sendResult.Result);
                                str.Append(" (").Append(service.RemoteKey).Append(") ").Append(service.Name).Append(" ")
                                        .Append(Emoji.Failed).Append(" - ")
                                        .Append(String.Empty).Append("\n");
                            }

                            var response = _Bot.SendTextMessageAsync(
                                                                     chatId,
                                                                     str.ToString(),
                                                                     false,
                                                                     false,
                                                                     0,
                                                                     null,
                                                                     ParseMode.Markdown
                                                                    );
                            Wait(response);
                        }
                        catch (Exception exp)
                        {
                            Log.Error(exp, exp.Message);
                        }
                    }
                    else
                    {
                        bool runningState;
                        var ram = ServiceHelpers.GetProcInfo(service.Name, out runningState);
                        var response = _Bot.SendTextMessageAsync(
                                                                 chatId,
                                                                 String.Format(
                                                                               "({0}) {1} {2} - {3}",
                                                                               _LocalOctet,
                                                                               service.Name,
                                                                               runningState ? Emoji.Ok : Emoji.Failed,
                                                                               ram),
                                                                 false,
                                                                 false,
                                                                 0,
                                                                 null,
                                                                 ParseMode.Markdown
                                                                );
                        Wait(response);
                    }

                    #endregion

                    break;

                default:
                    SendMenu(chatId);
                    return;
            }
        }

        #endregion

        #region GetLog

        /// <summary>
        /// Callback line:
        /// Type | ChatId | Service Name | Num Lines | Log Name
        /// </summary>
        /// <param name="chatId"></param>
        /// <param name="argsList"></param>
        private void GetLog(long chatId, IReadOnlyList<string> argsList)
        {
            Dictionary<string, string> listButtons;
            InlineKeyboardMarkup keyboardMarkup;
            String sendText;
            ControlledService service;
            switch (argsList.Count)
            {
                // Step 1, send list of services
                case 0:
                    listButtons = _Services.ToDictionary(
                                                         item => item.ServiceName + " (" +
                                                                 (String.IsNullOrEmpty(item.RemoteKey)
                                                                     ? _LocalOctet
                                                                     : item.RemoteKey) + ")",
                                                         item => item.Alias);

                    keyboardMarkup =
                            new InlineKeyboardMarkup(
                                                     Keyboards.GetInlineKeyboard(
                                                                                 CommandsList.Log,
                                                                                 chatId,
                                                                                 listButtons));
                    sendText = Resources.SelectService;
                    break;

                // Step 2, select number of lines
                case 1:
                    service = GetService(argsList[0]);
                    if (service.LogFiles.Count == 0)
                    {
                        var msg = String.Format(
                                                "Sorry there is no files for this service *{0}*",
                                                service.Key);
                        var fastReply = _Bot.SendTextMessageAsync(
                                                                  chatId,
                                                                  msg,
                                                                  false,
                                                                  false,
                                                                  0,
                                                                  null,
                                                                  ParseMode.Markdown);
                        Wait(fastReply);
                        return;
                    }

                    listButtons = new Dictionary<string, string>();
                    for(var i = 0; i< _LogNumLines.Length; i++)
                    {
                        listButtons.Add(_LogNumLines[i].ToString(), service.Id + ";" + i);
                    }

                    keyboardMarkup =
                            new InlineKeyboardMarkup(
                                                     Keyboards.GetInlineKeyboardSingleLine(
                                                                                           CommandsList.Log,
                                                                                           chatId,
                                                                                           listButtons));
                    sendText = Resources.SelectNumberOfLines;
                    break;

                // Step 3, select file
                case 2:
                    service = GetService(Convert.ToInt32(argsList[0]));
                    var numLines = argsList[1];
                    var prefix = service.Id + ";" + numLines + ";";
                    listButtons = service.LogFiles.Select((v, index) => new {index, v})
                            .ToDictionary(x => x.v, x => prefix + x.index);

                    keyboardMarkup = new InlineKeyboardMarkup(
                                                              Keyboards.GetInlineKeyboard(
                                                                                          CommandsList.Log,
                                                                                          chatId,
                                                                                          listButtons));
                    sendText = String.Format(
                                             "Ok to *{0}* we have such log files",
                                             service.Key);
                    break;

                // Step 4, get file
                case 3:
                    service = GetService(Convert.ToInt32(argsList[0]));
                    SendLog(
                            chatId,
                            service,
                            _LogNumLines[Convert.ToInt32(argsList[1])],
                            service.LogFiles.Select((v, index) => new {index, v})
                                    .FirstOrDefault(v => v.index == Convert.ToInt32(argsList[2])).v);
                    return;

                default:
                    SendMenu(chatId);
                    return;
            }

            var response = _Bot.SendTextMessageAsync(
                                                     chatId,
                                                     sendText,
                                                     false,
                                                     false,
                                                     0,
                                                     keyboardMarkup,
                                                     ParseMode.Markdown);
            Wait(response);
        }
        

        #endregion

        #region  Wait

        private static void Wait(Task<Message> selectButton)
        {
            int i = 0;
            while (!selectButton.IsCompleted &&
                   !selectButton.IsFaulted &&
                   !selectButton.IsCanceled)
            {
                Thread.Sleep(10);
                i++;
                if (i > 100)
                {
                    Log.Warn("Something going wrong! Exit sending");
                    break;
                }
            }
        }

        private static void Wait(Task selectButton)
        {
            int i = 0;
            while (!selectButton.IsCompleted &&
                   !selectButton.IsFaulted &&
                   !selectButton.IsCanceled)
            {
                Thread.Sleep(10);
                i++;
                if (i > 100)
                {
                    Log.Warn("Something going wrong! Exit sending");
                    break;
                }
            }
        }

        #endregion
    }
}
