using System;
using System.Collections.Generic;
using System.Reflection;
using Falcon.FalconAnalytics.Scripts.Models.Attributes;
using Falcon.FalconAnalytics.Scripts.Models.Messages.Interfaces;
using Falcon.FalconAnalytics.Scripts.Payloads;
using Falcon.FalconAnalytics.Scripts.Payloads.Flex;
using Falcon.FalconAnalytics.Scripts.Services;
using Falcon.FalconCore.Scripts.Repositories;
using Newtonsoft.Json;

namespace Falcon.FalconAnalytics.Scripts.Models.Messages.Abstracts
{
    [Serializable]
    public abstract class PlainLog : IDataLog
    {
        [FKey(Ignore = true)]public long clientCreateDate = FTime.CurrentTimeMillis();

        [JsonIgnore] public long CreatedTime => clientCreateDate;

        public virtual string Event => GetType().Name;

        public DataWrapper Wrap()
        {
            return new DataWrapper(this);
        }

        public virtual Dictionary<string, object> ToDictionary()
        {
            var result = new Dictionary<string, object>();
            var defaultAttribute = new FKeyAttribute();

            foreach (var fieldInfo in GetType().GetFields(BindingFlags.Instance | BindingFlags.Public))
            {
                var attribute = fieldInfo.GetCustomAttribute<FKeyAttribute>() ?? defaultAttribute;
                if (attribute.Ignore) continue;
                
                var propertyName = attribute.Name ?? fieldInfo.Name;

                var value = fieldInfo.GetValue(this);
                if (value != null || !attribute.RemoveIfNull) result[propertyName] = value;
            }

            return result;
        }

        public virtual void Send()
        {
            LogSendService.Instance.Enqueue(this);
            if(AnalyticConfigs.Instance.fCoreAnalyticTesting)
                TestLogSendService.Instance.Enqueue(this);
        }
    }
}