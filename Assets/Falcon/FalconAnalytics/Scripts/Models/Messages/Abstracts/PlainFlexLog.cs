// using System;
// using System.Collections.Generic;
// using Falcon.FalconAnalytics.Scripts.Models.Messages.Interfaces;
// using Falcon.FalconAnalytics.Scripts.Payloads.Flex;
// using Falcon.FalconAnalytics.Scripts.Services;
// using Falcon.FalconCore.Scripts.Repositories;
// using Newtonsoft.Json;
//
// namespace Falcon.FalconAnalytics.Scripts.Models.Messages.Abstracts
// {
//     [Serializable]
//     public abstract class PlainFlexLog : IDataLog
//     {
//         public long clientCreateDate = FTime.CurrentTimeMillis();
//
//         public DataWrapper Wrap()
//         {
//             return new DataWrapper(this);
//         }
//
//         [JsonIgnore] public long CreatedTime => clientCreateDate;
//
//         public string Event => GetType().Name;
//         public Dictionary<string, object> ToDictionary()
//         {
//             throw new NotImplementedException();
//         }
//
//         public void Send()
//         {
//             LogSendService.Instance.Enqueue(Wrap());
//         }
//     }
// }