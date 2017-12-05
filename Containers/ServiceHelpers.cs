using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using NLog;

namespace Containers
{
    public static class ServiceHelpers
    {
        // ReSharper disable FieldCanBeMadeReadOnly.Local
        // ReSharper disable InconsistentNaming
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();
        // ReSharper restore InconsistentNaming
        // ReSharper restore FieldCanBeMadeReadOnly.Local
        private static readonly TimeSpan _LongServiceRun = new TimeSpan(0, 1, 0);

        public static string Base64Decode(string base64EncodedData)
        {
            var base64EncodedBytes = Convert.FromBase64String(base64EncodedData);
            return Encoding.UTF8.GetString(base64EncodedBytes);
        }

        public static string Base64Encode(string plainText)
        {
            var plainTextBytes = Encoding.UTF8.GetBytes(plainText);
            return Convert.ToBase64String(plainTextBytes);
        }

        public static bool StopService(String procName, String serviceName)
        {
            try
            {
                Log.Info(
                         "Trying to stop -> Service: {0}, ServiceName: {1}",
                         procName,
                         serviceName);

                using (var theController = new ServiceController(serviceName))
                {
                    var procInfo = Process.GetProcessesByName(procName);

                    if (procInfo.Length > 0)
                    {
                        theController.Stop();

                        procInfo = Process.GetProcessesByName(procName);
                        var startTime = DateTime.Now;
                        while (procInfo.Length <= 0)
                        {
                            Thread.Sleep(250);
                            procInfo = Process.GetProcessesByName(procName);

                            if (DateTime.Now - startTime > _LongServiceRun)
                            {
                                Log.Warn("Long service run occured while stop!");
                                return false;
                            }
                        }

                        return true;
                    }
                }
            }
            catch (Exception exp)
            {
                Log.Error(exp, exp.Message);
            }

            return false;
        }

        public static bool StartService(String procName, String serviceName)
        {
            try
            {
                Log.Info(
                         "Trying to start -> Service: {0}",
                         procName);

                using (var theController = new ServiceController(serviceName))
                {
                    theController.Start();
                    Thread.Sleep(500);

                    var procInfo = Process.GetProcessesByName(procName);
                    var startTime = DateTime.Now;
                    while (procInfo.Length <= 0)
                    {
                        Thread.Sleep(250);
                        procInfo = Process.GetProcessesByName(procName);

                        if (DateTime.Now - startTime > _LongServiceRun)
                        {
                            Log.Warn("Long service run occured while start!");
                            return false;
                        }
                    }
                }

                return true;
            }
            catch (Exception exp)
            {
                Log.Error(exp, exp.Message);
            }

            return false;
        }

        public static bool RestartService(String procName, String serviceName, bool needToStop)
        {
            try
            {
                Log.Info(
                         "Trying to restart -> Service: {0}, needToStop: {1}",
                         procName,
                         needToStop);

                using (var theController = new ServiceController(serviceName))
                {
                    DateTime startTime;
                    var procInfo = Process.GetProcessesByName(procName);

                    if (needToStop && procInfo.Length > 0)
                    {
                        theController.Stop();

                        procInfo = Process.GetProcessesByName(procName);
                        startTime = DateTime.Now;
                        while (procInfo.Length <= 0)
                        {
                            Thread.Sleep(300);
                            procInfo = Process.GetProcessesByName(procName);

                            if (DateTime.Now - startTime > _LongServiceRun)
                            {
                                Log.Warn("Long service run occured while stop!");
                                return false;
                            }
                        }
                    }

                    procInfo = Process.GetProcessesByName(procName);

                    if (procInfo.Length <= 0)
                    {
                        theController.Start();
                        Thread.Sleep(500);
                    }

                    procInfo = Process.GetProcessesByName(procName);
                    startTime = DateTime.Now;
                    while (procInfo.Length <= 0)
                    {
                        Thread.Sleep(250);
                        procInfo = Process.GetProcessesByName(procName);

                        if (DateTime.Now - startTime > _LongServiceRun)
                        {
                            Log.Warn("Long service run occured while start!");
                            return false;
                        }
                    }
                }

                return true;
            }
            catch (Exception exp)
            {
                Log.Error(exp, exp.Message);
            }

            return false;
        }


        public static string GetProcInfo(string procName, out bool runningState)
        {
            var procInfo = Process.GetProcessesByName(procName);

            if (procInfo.Length <= 0)
            {
                runningState = false;
                return String.Empty;
            }

            runningState = true;

            foreach (var process in procInfo)
            {
                var ram = process.WorkingSet64 / 1024 / 1024 + " Mb";
                return ram;
            }

            return String.Empty;
        }

        public static string GetLocalOctet()
        {
            var list = GetLocalIpAddress();
            if (list == null || list.Count == 0)
            {
                return String.Empty;
            }

            return list.LastOrDefault().Split('.')[3];
        }


        private static List<string> GetLocalIpAddress()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            var address = new List<string>();
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    address.Add(ip.ToString());
                }
            }

            return address;
        }
    }
}
