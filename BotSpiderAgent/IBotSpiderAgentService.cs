using System.IO;
using System.ServiceModel;

namespace BotSpiderAgent
{
    [ServiceContract(Namespace = "",
         Name = "BotSpiderAgentService", SessionMode = SessionMode.NotAllowed)]
    public interface IBotSpiderAgentService
    {
        [OperationContract(Name = "SendCommand")]
        string SendCommand(Stream jsonStream);
    }
}