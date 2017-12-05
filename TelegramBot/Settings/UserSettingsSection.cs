using System.Configuration;

namespace TelegramBot.Settings
{
    public class UserSettingsSection : ConfigurationSection
    {
        #region Constructors

        static UserSettingsSection()
        {
            _PropName = new ConfigurationProperty(
                                                  "name",
                                                  typeof(string),
                                                  null,
                                                  ConfigurationPropertyOptions.IsRequired
                                                 );

            _PropElements = new ConfigurationProperty(
                                                      "",
                                                      typeof(UserSettingsCollection),
                                                      null,
                                                      ConfigurationPropertyOptions.IsRequired |
                                                      ConfigurationPropertyOptions.IsDefaultCollection
                                                     );

            _Properties = new ConfigurationPropertyCollection { _PropName, _PropElements };
        }

        #endregion

        #region Fields

        private static ConfigurationPropertyCollection _Properties;
        private static ConfigurationProperty _PropName;
        private static ConfigurationProperty _PropElements;

        #endregion

        #region Properties

        public string Name
        {
            get { return (string)base[_PropName]; }
            set { base[_PropName] = value; }
        }

        public UserSettingsCollection Channels
        {
            get { return (UserSettingsCollection)base[_PropElements]; }
        }

        protected override ConfigurationPropertyCollection Properties
        {
            get { return _Properties; }
        }

        #endregion
    }
}
