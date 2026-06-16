// using System.Collections.Generic;
// using Falcon.FalconAnalytics.Scripts.Models.Messages.Interfaces;
// using Falcon.FalconAnalytics.Scripts.Payloads.Flex;
// using JetBrains.Annotations;
//
// namespace Falcon.FalconAnalytics.Scripts.Models.Messages
// {
//     public class MessageBatch : List<IDataLog>
//     {
//         public MessageBatch()
//         {
//         }
//
//         public MessageBatch([NotNull] IEnumerable<IDataLog> collection) : base(collection)
//         {
//         }
//
//         public BatchWrapper Wrap()
//         {
//             List<DataWrapper> logWrappers = new List<DataWrapper>(Count);
//             foreach (IDataLog message in this)
//             {
//                 DataWrapper logWrapper = new DataWrapper(message);
//                 logWrappers.Add(logWrapper);
//             }
//             
//             return new BatchWrapper(logWrappers);
//         }
//         
//         public void Send()
//         {
//             Wrap().Send();
//         }
//     }
// }