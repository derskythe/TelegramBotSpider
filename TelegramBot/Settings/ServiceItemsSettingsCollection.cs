using System.Configuration;

namespace TelegramBot.Settings
{
    public class ServiceItemsSettingsCollection : ConfigurationElementCollection
    {
        #region Constructor

        public ServiceItemsSettingsCollection()
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
                return "serviceItem";
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

        public ServiceItemSettingsElement this[int index]
        {
            get
            {
                return (ServiceItemSettingsElement)BaseGet(index);
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

        public ServiceItemSettingsElement this[string name]
        {
            get
            {
                return (ServiceItemSettingsElement)BaseGet(name);
            }
        }

        #endregion

        #region Methods

        public void Add(ServiceItemSettingsElement item)
        {
            BaseAdd(item);
        }

        public void Remove(ServiceItemSettingsElement item)
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
            return new ServiceItemSettingsElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return (element as ServiceItemSettingsElement).Key;
        }

        #endregion
    }
}
