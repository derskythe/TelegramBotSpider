using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Containers;
using Telegram.Bot.Args;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using File = System.IO.File;

namespace TelegramBot
{
    public partial class BotService : IDisposable
    {
        private void BotOnOnCallbackQuery(object sender, CallbackQueryEventArgs callbackQueryEventArgs)
        {
            try
            {
                if (String.IsNullOrEmpty(callbackQueryEventArgs.CallbackQuery.Data))
                {
                    SendInvalidResponse(callbackQueryEventArgs);
                    return;
                }

                var cmdList = callbackQueryEventArgs.CallbackQuery.Data.Split(';');

                if (cmdList.Length <= 2)
                {
                    SendInvalidResponse(callbackQueryEventArgs);
                    return;
                }

                _Bot.AnswerCallbackQueryAsync(
                                              callbackQueryEventArgs.CallbackQuery.Id,
                                              "Response received");
                long chatId = Convert.ToInt64(cmdList[1]);
                var argsList = new List<string>();
                for (int i = 2; i < cmdList.Length; i++)
                {
                    argsList.Add(cmdList[i]);
                }
                switch (cmdList[0])
                {
                    case CommandsList.Log:
                        GetLog(chatId, argsList);
                        break;

                    case CommandsList.ServiceRestart:
                        RestartService(chatId, argsList);
                        break;

                    case CommandsList.ServiceStart:
                        ServiceStart(chatId, argsList);
                        break;

                    case CommandsList.ServiceStop:
                        ServiceStop(chatId, argsList);
                        break;

                    case CommandsList.Status:
                        GetServiceStatus(chatId, argsList);
                        break;
                }
            }
            catch (Exception exp)
            {
                Log.Error(exp, exp.Message);
            }
        }

        private void SendLog(long chatId, ControlledService service, int numLines, string fileName)
        {
            try
            {
                var result = String.Empty;

                var bPath = new StringBuilder();
                bPath.Append(service.Path).Append(Path.DirectorySeparatorChar)
                        .Append("log").Append(Path.DirectorySeparatorChar).Append(fileName).Append(".txt");

                var path = bPath.ToString();

                _Bot.SendTextMessageAsync(
                                          chatId,
                                          String.Format("Trying to view *{0}* of file *{1}*", numLines, path),
                                          false,
                                          false,
                                          0,
                                          null,
                                          ParseMode.Markdown);


                if (!String.IsNullOrEmpty(service.RemoteKey))
                {
                    var remoteAgent = _RemoteAgents[service.RemoteKey];

                    try
                    {
                        var request = new DefaultCommandRequest
                        {
                            Command = CommandsList.Log,
                            ServiceName = service.Key
                        };
                        request.Arguments.Add(path);
                        request.Arguments.Add(numLines.ToString());

                        var sendResult = AgentTokenizer.SendCommand(remoteAgent.Ip, remoteAgent.PublicKey, request);
                        var str = new StringBuilder();
                        if (sendResult.Result == ResultCodes.Ok)
                        {
                            Log.Info("Send ok");
                            foreach (var item in sendResult.Arguments)
                            {
                                str.Append(ServiceHelpers.Base64Decode(item)).Append("\n");
                            }
                        }
                        else
                        {
                            Log.Warn("Send failed. " + sendResult.Result);
                        }

                        result = str.ToString();
                    }
                    catch (Exception exp)
                    {
                        Log.Error(exp, exp.Message);
                    }
                }
                else if (File.Exists(path) && String.IsNullOrEmpty(service.RemoteKey))
                {
                    using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                    {
                        using (var reader = new StreamReader(fs, Encoding.UTF8))
                        {
                            reader.BaseStream.Seek(0, SeekOrigin.End);
                            int count = 0;
                            while (count < numLines &&
                                   reader.BaseStream.Position > 0)
                            {
                                reader.BaseStream.Position--;
                                int c = reader.BaseStream.ReadByte();
                                if (reader.BaseStream.Position > 0)
                                {
                                    reader.BaseStream.Position--;
                                }
                                if (c == Convert.ToInt32('\n'))
                                {
                                    ++count;
                                }
                            }

                            string str = reader.ReadToEnd();
                            result = str.Replace("\r", "");
                            reader.Close();
                        }

                        fs.Close();
                    }
                }
                else
                {
                    _Bot.SendTextMessageAsync(
                                              chatId,
                                              "Log file not found! Path: " + path);
                }
                /*
                if (!String.IsNullOrEmpty(result) && result.Length < 4000)
                {
                    var response = _Bot.SendTextMessageAsync(
                                                             messageEventArgs.CallbackQuery.Message.Chat.Id,
                                                             result);
                    while (!response.IsCompleted &&
                           !response.IsFaulted &&
                           !response.IsCanceled)
                    {
                        Thread.Sleep(100);
                    }

                    if (response.IsFaulted ||
                        response.IsCanceled)
                    {
                        _Bot.SendTextMessageAsync(
                                                  messageEventArgs.CallbackQuery.Message.Chat.Id,
                                                  "Error occured while sending file " + path,
                                                  false,
                                                  false,
                                                  messageEventArgs.CallbackQuery.Message.MessageId);
                    }
                }
                else */
                if (!String.IsNullOrEmpty(result))
                {
                    /*
                    var size = Convert.ToInt32(Math.Ceiling(result.Length / 4000M));

                    for (int i = 0; i < size; i++)
                    {
                        var chunk = 4000 * i + 4000 > result.Length
                            ? result.Substring(4000 * i)
                            : result.Substring(4000 * i, 4000);
                        var response = _Bot.SendTextMessageAsync(
                                                                 messageEventArgs.Message.Chat.Id,
                                                                 chunk);
                        while (!response.IsCompleted &&
                               !response.IsFaulted &&
                               !response.IsCanceled)
                        {
                            Thread.Sleep(100);
                        }
                    }*/

                    using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(result)))
                    {
                        var fts = new FileToSend
                        {
                            Content = stream,
                            Filename = path.Split('\\').Last()
                        };
                        var response = _Bot.SendDocumentAsync(
                                                              chatId,
                                                              fts,
                                                              "log_" +
                                                              service.Name.Replace(" ", "_").ToLowerInvariant() + "_" +
                                                              fileName + "_" + numLines + ".txt");
                        Wait(response);
                    }
                }

            }
            catch (Exception e)
            {
                var result = _Bot.SendTextMessageAsync(
                                                       chatId,
                                                       "Error occured while reading file!");
                Wait(result);
                Log.Error(e, e.Message);
            }
        }

        private void SendInvalidResponse(CallbackQueryEventArgs callbackQueryEventArgs)
        {
            var response = _Bot.AnswerCallbackQueryAsync(
                                                         callbackQueryEventArgs.CallbackQuery.Id,
                                                         "Invalid response received");
            Wait(response);
        }
    }
}
