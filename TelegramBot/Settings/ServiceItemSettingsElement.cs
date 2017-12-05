using System.Configuration;

namespace TelegramBot.Settings
{
    public class ServiceItemSettingsElement : ConfigurationElement
    {
        #region Constructors

        static ServiceItemSettingsElement()
        {
            _Key = new ConfigurationProperty(
                                              "key",
                                              typeof(string),
                                              null,
                                              ConfigurationPropertyOptions.IsRequired
                                             );

            _Name = new ConfigurationProperty(
                                              "name",
                                              typeof(string),
                                              null,
                                              ConfigurationPropertyOptions.IsRequired
                                             );

            _ServiceName = new ConfigurationProperty(
                                                     "serviceName",
                                                     typeof(string),
                                                     null,
                                                     ConfigurationPropertyOptions.IsRequired
                                                    );

            _Alias = new ConfigurationProperty(
                                               "alias",
                                               typeof(string),
                                               null,
                                               ConfigurationPropertyOptions.None
                                              );

            _Path = new ConfigurationProperty(
                                              "path",
                                              typeof(string),
                                              null,
                                              ConfigurationPropertyOptions.None
                                             );

            _RemoteKey = new ConfigurationProperty(
                                              "remoteKey",
                                              typeof(string),
                                              null,
                                              ConfigurationPropertyOptions.None
                                             );

            _LogFiles = new ConfigurationProperty(
                                                  "logFiles",
                                                  typeof(string),
                                                  null,
                                                  ConfigurationPropertyOptions.None
                                                 );


            _Properties = new ConfigurationPropertyCollection
            {
                _Key,
                _Name,
                _ServiceName,
                _Alias,
                _Path,
                _RemoteKey,
                _LogFiles
            };
        }

        #endregion

        #region Fields

        private static ConfigurationPropertyCollection _Properties;
        private static ConfigurationProperty _Key;
        private static ConfigurationProperty _Name;
        private static ConfigurationProperty _ServiceName;
        private static ConfigurationProperty _Alias;
        private static ConfigurationProperty _Path;
        private static ConfigurationProperty _RemoteKey;
        private static ConfigurationProperty _LogFiles;

        #endregion

        #region Properties

        public string Key
        {
            get { return (string)base[_Key]; }
            set { base[_Key] = value; }
        }

        public string Name
        {
            get { return (string)base[_Name]; }
            set { base[_Name] = value; }
        }

        public string ServiceName
        {
            get { return (string)base[_ServiceName]; }
            set { base[_ServiceName] = value; }
        }

        public string Alias
        {
            get { return (string)base[_Alias]; }
            set { base[_Alias] = value; }
        }

        public string Path
        {
            get { return (string)base[_Path]; }
            set { base[_Path] = value; }
        }

        public string RemoteKey
        {
            get { return (string)base[_RemoteKey]; }
            set { base[_RemoteKey] = value; }
        }

        public string LogFiles
        {
            get { return (string)base[_LogFiles]; }
            set { base[_LogFiles] = value; }
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
