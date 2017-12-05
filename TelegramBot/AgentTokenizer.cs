using System;
using System.IO;
using System.Net;
using System.Text;
using BouncyCastleWrapper;
using Containers;
using Newtonsoft.Json;
using NLog;

namespace TelegramBot
{
    internal static class AgentTokenizer
    {
        // ReSharper disable FieldCanBeMadeReadOnly.Local
        // ReSharper disable InconsistentNaming
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();
        // ReSharper restore InconsistentNaming
        // ReSharper restore FieldCanBeMadeReadOnly.Local

        private static string _PrivateKey = String.Empty;

        public static void Init(String privateKey)
        {
            _PrivateKey = privateKey;
        }

        public static DefaultCommandResponse SendCommand(String url, String cert, DefaultCommandRequest request)
        {
            Log.Info("Sending to {0},\n{1}", url, request);
            if (String.IsNullOrEmpty(_PrivateKey))
            {
                throw new Exception("Private cert doesn't initialized!");
            }

            request.Sign = GenSign(request, cert);

            var baseAddress = "http://" + url + ":6000/Services/BotSpiderAgent/SendCommand/";

            var http = (HttpWebRequest)WebRequest.Create(new Uri(baseAddress));
            http.Accept = "application/json";
            http.Method = "POST";

            string parsedContent = JsonConvert.SerializeObject(request);
            UTF8Encoding encoding = new UTF8Encoding();
            var bytes = encoding.GetBytes(parsedContent);

            var newStream = http.GetRequestStream();
            newStream.Write(bytes, 0, bytes.Length);
            newStream.Close();

            var response = http.GetResponse();

            var stream = response.GetResponseStream();
            if (stream == null)
            {
                throw new NullReferenceException("Stream is NULL");
            }
            var sr = new StreamReader(stream);
            var responseJson = sr.ReadToEnd().Substring(1);
            responseJson = responseJson.Substring(0, responseJson.Length - 1).Replace("\\\"", "\"");

            var result = JsonConvert.DeserializeObject<DefaultCommandResponse>(responseJson);
            if (!CheckSign(result))
            {
                throw new Exception("Sign invalid!");
            }

            return result;
        }

        private static bool CheckSign(DefaultCommandResponse request)
        {
            try
            {
                var str = new StringBuilder();
                str.Append(request.Command).Append(request.RequestDate.Ticks);
                var s = Wrapper.VerifyPrivateKey(request.Sign, _PrivateKey);

                return s == str.ToString();
            }
            catch (Exception exp)
            {
                Log.Error(exp, exp.Message);
            }

            return false;
        }

        private static string GenSign(DefaultCommandRequest response, String cert)
        {
            try
            {
                cert = cert.Replace("\\r\\n", "\r\n");
                Log.Debug(cert);
                var str = new StringBuilder();
                str.Append(response.Command).Append(response.RequestDate.Ticks);

                foreach (var argument in response.Arguments)
                {
                    str.Append(argument);
                }

                return Wrapper.SignPublicKey(str.ToString(), cert);
            }
            catch (Exception exp)
            {
                Log.Error(exp, exp.Message);
            }

            return String.Empty;
        }

        public static bool CheckCert(String cert)
        {
            try
            {
                cert = cert.Replace("\\r\\n", "\r\n");
                Wrapper.SignPublicKey("TEST MESSAGE", cert);
                return true;
            }
            catch (Exception exp)
            {
                Log.Error(exp, exp.Message);
                Log.Error("\n");
            }

            return false;
        }
    }
}
