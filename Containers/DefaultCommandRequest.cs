using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace Containers
{
    [Serializable, XmlRoot("DefaultCommandRequest")]
    [DataContract(Name = "DefaultCommandRequest", Namespace = "urn:BankOfBaku")]
    public class DefaultCommandRequest
    {
        [XmlElement]
        [DataMember]
        public string ServiceName { get; set; }

        [XmlElement]
        [DataMember]
        public string Sign { get; set; }

        [XmlElement]
        [DataMember]
        public DateTime RequestDate { get; set; }

        [XmlElement]
        [DataMember]
        public string Command { get; set; }

        [XmlElement]
        [DataMember]
        public List<string> Arguments { get; set; }

        public DefaultCommandRequest()
        {
            ServiceName = String.Empty;
            Sign = String.Empty;
            RequestDate = DateTime.Now;
            Command = String.Empty;
            Arguments = new List<string>();
        }

        public DefaultCommandRequest(
            string serviceName,
            string sign,
            DateTime requestDate,
            string command,
            List<string> arguments)
        {
            ServiceName = serviceName;
            Sign = sign;
            RequestDate = requestDate;
            Command = command;
            Arguments = arguments;
        }

        public override string ToString()
        {
            return string.Format(
                                 "Command: {0}, Arguments: {1}, Service: {2}, Sign: {3}, RequestDate: {4}",
                                 Command,
                                 EnumEx.GetStringFromArray(Arguments),
                                 ServiceName,
                                 Sign,
                                 RequestDate);
        }
    }
}
