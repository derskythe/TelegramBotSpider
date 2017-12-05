using System;
using System.Collections.Generic;
using Containers;

namespace TelegramBot
{
    internal class RemoteAgent
    {
        public String Key { get; set; }
        public String PublicKey { get; set; }
        public String Ip { get; set; }
        public List<ControlledService> Services { get; set; }

        public RemoteAgent()
        {
        }

        public RemoteAgent(string key, string publicKey, string ip, List<ControlledService> services)
        {
            Key = key;
            PublicKey = publicKey;
            Ip = ip;
            Services = services;
        }

        public override string ToString()
        {
            return string.Format("Key: {0}, Ip: {1}, Services: {2}", Key, Ip, EnumEx.GetStringFromArray(Services));
        }
    }
}
