using System;
using System.IO;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;
using BouncyCastleWrapper;
using Containers;
using Newtonsoft.Json;
using NLog;
using TelegramBot;

namespace BotSpiderAgent
{
    [ServiceBehavior(ConcurrencyMode = ConcurrencyMode.Multiple,
        IncludeExceptionDetailInFaults = false,
        InstanceContextMode = InstanceContextMode.PerSession,
        UseSynchronizationContext = true,
        ConfigurationName = "BotSpiderAgent.BotSpiderAgentService")]
    public partial class BotSpiderAgentService : IBotSpiderAgentService
    {
        // ReSharper disable InconsistentNaming
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();
        // ReSharper restore InconsistentNaming

        private const String CLIENT_PUBLIC_KEY = "-----BEGIN PUBLIC KEY-----\r\nMIICIjANBgkqhkiG9w0BAQEFAAOCAg8AMIICCgKCAgEAlSmt6CeRFltEagBCEAs6\r\n2vrIOB6dRvmvwpP+TgKgwY+mskDLd19Jsy2dJ1JgSwzZySA36TnM6ckDFq7yKqeG\r\nWymZhnXOhuWYcPPwWEe4NmZ3BIoneD2Nmh1YbjTHUNF7iUDZ3jOdlNrfcl/kgkxE\r\nDWmvPmQ77OHc6wXHXl/HOTd7DNKaz7x5940BLq7Cz5vDgIv8I9oFv1acIZWaEChH\r\nNI5x812eqNqM35yP8ZD0DteDuGhF8NYvHUFHAMDeQtdfZeTQ8aHNq/ggyPXcicO6\r\nX0lVHLcUNtXsl0z4568U45O1rpZYS7FAL1AJM6hbssKOLS6/dNq0l/jXQXB+qxYK\r\naXVVBluctyrgta4IimagfmrV8VV9/uFDLh+lpEmMP35BRjtwVgrbpf6WfsIv6U46\r\n8zzeMk0NA8AGIiT4KuItGJC+9FVMyS+y4LAb9s/zn/jHDOZh4LwGc4y7FtkG28n0\r\nat+W2qWBv5Fp37bV3A3+9Ji89reEbEAU3jzdtjJguug5ANi2TYMW4cgP74iyd+ub\r\nkQQR5DlgC8Km4coyaMenMui0fwUff7YZQDDFS5/GLUs22fH/hXfYsH8R7/71tfn5\r\ncjzA9srd4lwxUP/es1amiMIkKvz2ULFYPZ+eGQstOHa5/OegLEksWFGFN2rdV9xi\r\nrNVheHiTU4X+MLQGowRSjkkCAwEAAQ==\r\n-----END PUBLIC KEY-----";
        private static String _PrivateKey;

        public static void Init(string privateKey)
        {
            try
            {
                _PrivateKey = privateKey;
            }
            catch (Exception exp)
            {
                Log.Error(exp, exp.Message);
            }
        }

        [WebInvoke(Method = "POST", UriTemplate = "SendCommand/", BodyStyle = WebMessageBodyStyle.Bare, RequestFormat = WebMessageFormat.Json,
            ResponseFormat = WebMessageFormat.Json)]
        [OperationBehavior(AutoDisposeParameters = true)]
        public string SendCommand(Stream jsonStream)
        {
            var response = new DefaultCommandResponse();

            try
            {
                var s = jsonStream.CreateFromJsonStream<DefaultCommandRequest>();
                if (!CheckSign(s))
                {
                    throw new UnauthorizedAccessException();
                }

                Log.Info(s);

                if (s.Command.StartsWith(CommandsList.Status))
                {
                    response.Arguments = GetServiceStatus(s);
                    response.Result = ResultCodes.Ok;
                }
                else if (s.Command.StartsWith(CommandsList.System))
                {
                    // No, this command doesn't need
                }
                else if (s.Command.StartsWith(CommandsList.Log))
                {
                    response.Arguments = GetLog(s);
                    response.Result = ResultCodes.Ok;
                }
                else if (s.Command.StartsWith(CommandsList.ServiceStart))
                {
                    response.Result = ServiceStart(s);
                }
                else if (s.Command.StartsWith(CommandsList.ServiceStop))
                {
                    response.Result = ServiceStop(s);
                }
                else if (s.Command.StartsWith(CommandsList.ServiceRestart))
                {
                    response.Result = RestartService(s);
                }
                else if (s.Command.StartsWith(CommandsList.Start))
                {
                    Log.Info("Start update services");
                    lock (_Services)
                    {
                        _Services.Clear();
                        int i = 1;
                        foreach (var argument in s.Arguments)
                        {
                            var chunks = argument.Split(';');
                            var service =
                                    new ControlledService(
                                                          i++,
                                                          chunks[0],
                                                          chunks[1],
                                                          chunks[2],
                                                          chunks[3],
                                                          chunks[4]);
                            Log.Info("Add service " + service);
                            _Services.Add(service);
                        }
                    }

                    response.Result = ResultCodes.Ok;
                }

                response.Command = s.Command;
            }
            catch (ArgumentException)
            {
                Log.Warn("Invalid arguments");
                response.Result = ResultCodes.InvalidState;
            }
            catch (UnauthorizedAccessException)
            {
                Log.Warn("Invalid sign!");
                response.Result = ResultCodes.InvalidSign;
            }
            catch (Exception exp)
            {
                Log.Error(exp, exp.Message);
            }

            response.Sign = GenSign(response);
            var responseStr = JsonConvert.SerializeObject(response);
            Log.Info(responseStr);
            return responseStr;
        }

        private bool CheckSign(DefaultCommandRequest request)
        {
            try
            {
                var str = new StringBuilder();
                str.Append(request.Command).Append(request.RequestDate.Ticks);

                foreach (var argument in request.Arguments)
                {
                    str.Append(argument);
                }

                var s = Wrapper.VerifyPrivateKey(request.Sign, _PrivateKey);

                return s == str.ToString();
            }
            catch (Exception exp)
            {
                Log.Error(exp, exp.Message);
            }

            return false;
        }

        private string GenSign(DefaultCommandResponse response)
        {
            try
            {
                var str = new StringBuilder();
                str.Append(response.Command).Append(response.RequestDate.Ticks);

                /*foreach (var argument in response.Arguments)
                {
                    str.Append(argument);
                }*/

                return Wrapper.SignPublicKey(str.ToString(), CLIENT_PUBLIC_KEY);
            }
            catch (Exception exp)
            {
                Log.Error(exp, exp.Message);
            }

            return String.Empty;
        }
    }
}
