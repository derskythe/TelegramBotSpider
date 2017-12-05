using System.Configuration;

namespace TelegramBot.Settings
{
    public class RemoteKeySettingsElement : ConfigurationElement
    {
        #region Constructors

        static RemoteKeySettingsElement()
        {
            _Key = new ConfigurationProperty(
                                              "key",
                                              typeof(string),
                                              null,
                                              ConfigurationPropertyOptions.IsRequired
                                             );

            _PublicCert = new ConfigurationProperty(
                                                     "publicCert",
                                                     typeof(string),
                                                     null,
                                                     ConfigurationPropertyOptions.IsRequired
                                                    );
            _Ip = new ConfigurationProperty(
                                                    "ip",
                                                    typeof(string),
                                                    null,
                                                    ConfigurationPropertyOptions.IsRequired
                                                   );

            _Properties = new ConfigurationPropertyCollection
            {
                _Key,
                _PublicCert,
                _Ip
            };
        }

        #endregion

        #region Fields

        private static ConfigurationPropertyCollection _Properties;
        private static ConfigurationProperty _Key;
        private static ConfigurationProperty _PublicCert;
        private static ConfigurationProperty _Ip;

        #endregion

        #region Properties

        public string Key
        {
            get { return (string)base[_Key]; }
            set { base[_Key] = value; }
        }

        public string PublicCert
        {
            get { return (string)base[_PublicCert]; }
            set { base[_PublicCert] = value; }
        }

        public string Ip
        {
            get { return (string)base[_Ip]; }
            set { base[_Ip] = value; }
        }

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                return _Properties;
            }
        }

        #endregion
    }
}
