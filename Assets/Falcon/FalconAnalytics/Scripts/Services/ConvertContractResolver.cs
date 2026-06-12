// using System;
// using Newtonsoft.Json.Serialization;
//
// namespace Falcon.FalconAnalytics.Scripts.Services
// {
//     public class ConvertContractResolver : DefaultContractResolver
//     {
//         private readonly Type _type;
//
//         public ConvertContractResolver(Type type)
//         {
//             _type = type;
//         }
//         
//         protected override string ResolvePropertyName(string fieldName)
//         {
//             var field = _type.GetField(fieldName);
//
//             if (field == null)
//                 return base.ResolvePropertyName(fieldName);
//
//             if (field.GetCustomAttributes(typeof(FSortKeyAttribute), false).Length > 0)
//             {
//                 return fieldName + "$";
//             }
//             
//             if (field.GetCustomAttributes(typeof(FDistKeyAttribute), false).Length > 0)
//             {
//                 return fieldName + "$$";
//             }
//             
//             if (field.GetCustomAttributes(typeof(FAllKeyAttribute), false).Length > 0)
//             {
//                 return fieldName + "$$$";
//             }
//             
//             return base.ResolvePropertyName(fieldName);
//         }
//     }
// }
//
