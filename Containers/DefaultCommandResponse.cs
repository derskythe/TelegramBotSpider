using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace Containers
{
    [Serializable, XmlRoot("DefaultCommandResponse")]
    [DataContract(Name = "DefaultCommandResponse", Namespace = "urn:BankOfBaku")]
    public class DefaultCommandResponse
    {
        [XmlElement]
        [DataMember]
        public ResultCodes Result { get; set; }

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

        public DefaultCommandResponse()
        {
            Result = ResultCodes.UnknownError;
            Sign = String.Empty;
            RequestDate = DateTime.Now;
            Command = String.Empty;
            Arguments = new List<string>();
        }

        public DefaultCommandResponse(string sign, DateTime requestDate, string command, List<string> arguments)
        {
            Result = ResultCodes.UnknownError;
            Sign = sign;
            RequestDate = requestDate;
            Command = command;
            Arguments = arguments;
        }

        public override string ToString()
        {
            return string.Format(
                                 "Result: {0}, RequestDate: {1}, Command: {2}, Arguments: {3}, Sign: {4}",
                                 Result,
                                 RequestDate,
                                 Command,
                                 EnumEx.GetStringFromArray(Arguments),
                                 Sign);
        }
    }
}
