using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using Falcon.FalconAnalytics.Scripts.Responses;
using Falcon.FalconCore.Scripts.Utils;
using Falcon.FalconCore.Scripts.Utils.FActions.Variances.Starts;
using UnityEngine;

namespace Falcon.FalconAnalytics.Scripts.Payloads.Flex
{
    public class BatchWrapper : LogWrapper
    {
        public BatchWrapper(List<DataWrapper> wrappers) : base(JsonUtil.ToJson(wrappers.Select(JsonUtil.ToJson).ToList()))
        {
        }

        public override string URL => "https://dwhapi-v2.data4game.com/batch/event-log-v2";
        protected override void ValidateResponse(string response)
        {
            try
            {
                BatchProcessResponse batchResponse = JsonUtil.FromJson<BatchProcessResponse>(response);
                if (batchResponse.errors.Count > 0)
                {
                    foreach (var messageProcessErrorInfo in batchResponse.errors)
                    {
                        Debug.LogError(messageProcessErrorInfo.data +
                                       " has been sent failed with the response of: " +
                                       messageProcessErrorInfo.exception);
                    }
                }
            }
            catch (Exception e)
            {
                AnalyticLogger.Instance.Error(e);
            }
        }

        protected override void TrySend()
        {
            var request = new HttpRequest
            {
                RequestType = HttpMethod.Post,
                URL = URL,
                JsonBody = data,
                Timeout = TimeSpan.FromSeconds(60)
            };
            if(AnalyticConfigs.Instance.fCoreAnalyticTestHttpV2) request.HttpVersion = HttpVersion.Version20;

            request.Invoke();
            if (string.IsNullOrEmpty(request.Result))
            {
                throw request.Exception;
            }
            ValidateResponse(new string(request.Result.Where(c => !char.IsControl(c)).ToArray()));
        }
    }
}