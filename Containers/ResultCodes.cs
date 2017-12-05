using System;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace Containers
{
    [Serializable]
    [DataContract(Name = "ResultCodes")]
    public enum ResultCodes
    {
        [Description("OK")]
        [EnumMember(Value = "0")]
        Ok = 0,

        [Description("InvalidState")]
        [EnumMember(Value = "16")]
        InvalidState = 16,

        [Description("InvalidSign")]
        [EnumMember(Value = "64")]
        InvalidSign = 64,

        [Description("UnknownError")]
        [EnumMember(Value = "128")]
        UnknownError = 128,

        [Description("Failed")]
        [EnumMember(Value = "256")]
        Failed = 256
    }
}
