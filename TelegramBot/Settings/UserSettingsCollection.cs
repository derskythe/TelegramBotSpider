using System.Configuration;

namespace TelegramBot.Settings
{
    public class UserSettingsCollection : ConfigurationElementCollection
    {
        #region Constructor

        public UserSettingsCollection()
        {
        }

        #endregion

        #region Properties

        public override ConfigurationElementCollectionType CollectionType
        {
            get
            {
                return ConfigurationElementCollectionType.BasicMap;
            }
        }
        protected override string ElementName
        {
            get
            {
                return "grantedUser";
            }
        }

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                return new ConfigurationPropertyCollection();
            }
        }

        #endregion

        #region Indexers

        public UserSettingsElement this[int index]
        {
            get
            {
                return (UserSettingsElement)BaseGet(index);
            }
            set
            {
                if (BaseGet(index) != null)
                {
                    BaseRemoveAt(index);
                }
                BaseAdd(index, value);
            }
        }

        public UserSettingsElement this[string name]
        {
            get
            {
                return (UserSettingsElement)BaseGet(name);
            }
        }

        #endregion

        #region Methods

        public void Add(UserSettingsElement item)
        {
            BaseAdd(item);
        }

        public void Remove(UserSettingsElement item)
        {
            BaseRemove(item);
        }

        public void RemoveAt(int index)
        {
            BaseRemoveAt(index);
        }

        #endregion

        #region Overrides

        protected override ConfigurationElement CreateNewElement()
        {
            return new UserSettingsElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return (element as UserSettingsElement).Name;
        }

        #endregion
    }
}
