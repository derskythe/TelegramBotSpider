using System.Configuration;

namespace TelegramBot.Settings
{
    public class RemoteKeyCollection : ConfigurationElementCollection
    {
        #region Constructor

        public RemoteKeyCollection()
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
                return "remoteKey";
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

        public RemoteKeySettingsElement this[int index]
        {
            get
            {
                return (RemoteKeySettingsElement)BaseGet(index);
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

        public RemoteKeySettingsElement this[string name]
        {
            get
            {
                return (RemoteKeySettingsElement)BaseGet(name);
            }
        }

        #endregion

        #region Methods

        public void Add(RemoteKeySettingsElement item)
        {
            BaseAdd(item);
        }

        public void Remove(RemoteKeySettingsElement item)
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
            return new RemoteKeySettingsElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return (element as RemoteKeySettingsElement).Key;
        }

        #endregion
    }
}
