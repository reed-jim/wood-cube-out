using Falcon.FalconAnalytics.Scripts.Models.Messages.Interfaces;

namespace Falcon.FalconAnalytics.Scripts.Payloads.Flex
{
    public class TestWrapper : DataWrapper
    {
        public TestWrapper(string data, long clientSendTime, string @event) : base(data, clientSendTime, @event)
        {
        }

        public TestWrapper(IDataLog message) : base(message)
        {
        }

        public override string URL => AnalyticConfigs.Instance.fCoreAnalyticTestingSingleUrl;
    }
}