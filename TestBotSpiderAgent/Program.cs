using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using BouncyCastleWrapper;
using Containers;
using Newtonsoft.Json;
using NLog;
using TelegramBot;
using TestBotSpiderAgent.Properties;

namespace TestBotSpiderAgent
{
    class Program
    {
        // ReSharper disable FieldCanBeMadeReadOnly.Local
        // ReSharper disable InconsistentNaming
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();
        // ReSharper restore InconsistentNaming
        // ReSharper restore FieldCanBeMadeReadOnly.Local

        static void Main(string[] args)
        {
            //var serviceHost = new ServiceHost(typeof(BotSpiderAgent.BotSpiderAgentService));
            //serviceHost.Open();

            ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
            WebRequest.DefaultWebProxy.Credentials = CredentialCache.DefaultNetworkCredentials;

            DefaultCommandRequest request = new DefaultCommandRequest();
            request.Command = CommandsList.Start;
            request.Arguments = new List<string>
            {
                "BotServiceIb;BotService;BotService;ibot;C:\\BotService",
                "CallCenterCardInfoService;BotService;CallCenterCardInfoService;call;C:\\CallCenterCardInfoService",
                "DsmfPaymentServer;DsmfPaymentService;DsmfPaymentServer;dsmf;C:\\DsmfPaymentService",
                "MobileBanking;InternetBankingMobileService;MobileBanking;imobile;C:\\BotService",
                "PaymentServer;MultiPaymentService;PaymentServer;ib;C:\\PaymentService"

            };
            request.Sign = GenSign(request);

            var baseAddress = "http://192.168.0.220:6000/Services/BotSpiderAgent/SendCommand/";

            var http = (HttpWebRequest)WebRequest.Create(new Uri(baseAddress));
            http.Accept = "application/json";
            http.Method = "POST";

            string parsedContent = JsonConvert.SerializeObject(request);
            ASCIIEncoding encoding = new ASCIIEncoding();
            Byte[] bytes = encoding.GetBytes(parsedContent);

            var newStream = http.GetRequestStream();
            newStream.Write(bytes, 0, bytes.Length);
            newStream.Close();

            var response = http.GetResponse();

            var stream = response.GetResponseStream();
            var sr = new StreamReader(stream);
            var responseJson = sr.ReadToEnd().Substring(1);
            responseJson = responseJson.Substring(0, responseJson.Length - 1).Replace("\\\"", "\"");
            var content = JsonConvert.DeserializeObject<DefaultCommandResponse>(responseJson);

            if (content.Result == ResultCodes.Ok)
            {
                Console.WriteLine(CheckSign(content));
            }
            else
            {
                Console.WriteLine(content.Result);
            }

            Console.WriteLine("Press ENTER to exit");
            Console.ReadLine();

            //serviceHost.Close();
        }

        private static bool CheckSign(DefaultCommandResponse request)
        {
            try
            {
                var str = new StringBuilder();
                str.Append(request.Command).Append(request.RequestDate.Ticks);

                foreach (var argument in request.Arguments)
                {
                    str.Append(argument);
                }

                var s = Wrapper.VerifyPrivateKey(request.Sign, Settings.Default.PrivateKey);

                return s == str.ToString();
            }
            catch (Exception exp)
            {
                Log.Error(exp, exp.Message);
            }

            return false;
        }

        private static string GenSign(DefaultCommandRequest response)
        {
            try
            {
                StringBuilder str = new StringBuilder();
                str.Append(response.Command).Append(response.RequestDate.Ticks);

                foreach (var argument in response.Arguments)
                {
                    str.Append(argument);
                }

                return Wrapper.SignPublicKey(
                                             str.ToString(),
                                             "-----BEGIN PUBLIC KEY-----\r\nMIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEAnonwIlmu9odehkSOYwum\r\ninlssW1VMh/IEm+qutzBaRTKQbK8HxnY18ltFoJJCoqivXPZWwPI2wQxMamkY/yz\r\n1JcVGmb50d6t0FK14l1rNatrAvVXtcl7tG/tH5vh/8VIsGZIQ991SfiAqzPEl97m\r\nSHpGpB/wb4JZop4JEk81j4qmE3l1Qdx7zXbdwBBz1qSt7dhL744y1OzMX6/1GE7l\r\n+bK4tj8CCN8QPdVkc9ZRNQ0L0Ltrx8eqkDblcR33r32XN/VyL/sRoCRVZYO4qUKr\r\nXmo/qipkJUbNWTH3YseTdNYDIN+IGKnQJmaipZy2E8pZmPPM7PjT7P0nMNbeKL3u\r\nSQIDAQAB\r\n-----END PUBLIC KEY-----");
            }
            catch (Exception exp)
            {
                Log.Error(exp, exp.Message);
            }

            return String.Empty;
        }
    }
}
