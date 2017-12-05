using System.Configuration;

namespace TelegramBot.Settings
{
    public class UserSettingsElement : ConfigurationElement
    {
        #region Constructors

        static UserSettingsElement()
        {
            _Name = new ConfigurationProperty(
                                              "name",
                                              typeof(string),
                                              null,
                                              ConfigurationPropertyOptions.IsRequired
                                             );

            _AccessType = new ConfigurationProperty(
                                                    "accessType",
                                                    typeof(int),
                                                    null,
                                                    ConfigurationPropertyOptions.IsRequired
                                                   );


            _Properties = new ConfigurationPropertyCollection
            {
                _Name,
                _AccessType
            };
        }

        #endregion

        #region Fields

        private static ConfigurationPropertyCollection _Properties;
        private static ConfigurationProperty _Name;
        private static ConfigurationProperty _AccessType;

        #endregion

        #region Properties

        public string Name
        {
            get { return (string)base[_Name]; }
            set { base[_Name] = value; }
        }

        public int AccessType
        {
            get { return (int)base[_AccessType]; }
            set { base[_AccessType] = value; }
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
