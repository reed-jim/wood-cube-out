using System;
using System.Collections.Generic;

namespace Falcon.FalconAnalytics.Scripts.Payloads.Flex
{
    [Serializable]
    public class BatchTestWrapper : BatchWrapper
    {
        public BatchTestWrapper(List<DataWrapper> wrappers) : base(wrappers)
        {
        }

        public override string URL => AnalyticConfigs.Instance.fCoreAnalyticTestingBatchUrl;
    }
}