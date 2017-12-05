using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Containers;

namespace BotSpiderAgent
{
    public partial class BotSpiderAgentService
    {
        private static readonly List<ControlledService> _Services = new List<ControlledService>();

        private ControlledService GetService(String text)
        {
            lock (_Services)
            {
                foreach (var service in _Services)
                {
                    Log.Debug("Service " + service);
                    if (text.Contains(service.Key) ||
                        text.Contains(service.Alias))
                    {
                        return service;
                    }
                }
            }

            Log.Warn("Service " + text + " not found!");

            throw new ArgumentException();
        }

        private ResultCodes ServiceStart(DefaultCommandRequest request)
        {
            var service = GetService(request.ServiceName);
            bool runningState;
            ServiceHelpers.GetProcInfo(service.Name, out runningState);

            if (!runningState)
            {
                return ServiceHelpers.StartService(service.Name, service.ServiceName)
                    ? ResultCodes.Ok
                    : ResultCodes.Failed;
            }

            return ResultCodes.InvalidState;
        }

        private ResultCodes ServiceStop(DefaultCommandRequest messageEventArgs)
        {
            var service = GetService(messageEventArgs.ServiceName);
            bool runningState;
            ServiceHelpers.GetProcInfo(service.Name, out runningState);

            if (runningState)
            {
                return ServiceHelpers.StopService(service.Name, service.ServiceName)
                    ? ResultCodes.Ok
                    : ResultCodes.Failed;
            }

            return ResultCodes.InvalidState;
        }

        private ResultCodes RestartService(DefaultCommandRequest messageEventArgs)
        {
            var service = GetService(messageEventArgs.ServiceName);
            bool runningState;
            ServiceHelpers.GetProcInfo(service.Name, out runningState);

            return ServiceHelpers.RestartService(service.Name, service.ServiceName, runningState)
                ? ResultCodes.Ok
                : ResultCodes.Failed;
        }

        private List<String> GetSystemStatus()
        {
            var str = new List<String>();

            foreach (var service in _Services)
            {
                bool runningState;
                var ram = ServiceHelpers.GetProcInfo(service.Name, out runningState);
                str.Add(service.Name + " " + (runningState ? Emoji.Ok : Emoji.Failed) + " - " + ram);
            }

            return str;
        }

        private List<string> GetServiceStatus(DefaultCommandRequest messageEventArgs)
        {
            var service = GetService(messageEventArgs.ServiceName);

            bool runningState;
            var ram = ServiceHelpers.GetProcInfo(service.Name, out runningState);
            return new List<string>
            {
                String.Format(
                              "{0} {1} - {2}",
                              service.Name,
                              runningState ? Emoji.Ok : Emoji.Failed,
                              ram)
            };
        }

        private List<string> GetLog(DefaultCommandRequest messageEventArgs)
        {
            if (messageEventArgs.Arguments.Count != 2)
            {
                throw new ArgumentException();
            }

            var path = messageEventArgs.Arguments[0];
            int numLines;
            try
            {
                numLines = Convert.ToInt32(messageEventArgs.Arguments[1]);
            }
            catch (Exception)
            {
                throw new ArgumentException();
            }

            var resultList = new List<string>();

            try
            {
                if (File.Exists(path))
                {
                    string result;
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

                            var str = reader.ReadToEnd();
                            result = ServiceHelpers.Base64Encode(str.Replace("\r", ""));
                            reader.Close();
                        }

                        fs.Close();
                    }

                    resultList = new List<string> {result};
                }
            }
            catch (Exception e)
            {
                Log.Error(e, e.Message);
            }

            return resultList;
        }
    }
}
