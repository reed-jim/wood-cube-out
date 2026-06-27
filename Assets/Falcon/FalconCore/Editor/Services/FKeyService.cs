using System.Collections;
using System.Net.Http;
using System.Threading;
using Falcon.FalconCore.Editor.Models;
using Falcon.FalconCore.Editor.Repositories;
using Falcon.FalconCore.Scripts.Logs;
using Falcon.FalconCore.Scripts.Utils;
using Falcon.FalconCore.Scripts.Utils.Sequences.Entity;
using UnityEditor;

namespace Falcon.FalconCore.Editor.Services
{
    public static class FKeyService
    {
        private const string ValidateURL = "https://partner-api.data4game.com/d4g-api/falcon-sdk/validate?fkey=";
        private static long _validatingCount;

        public static bool Validating => Interlocked.Read(ref _validatingCount) > 0;

        public static bool FKeyValid()
        {
            return FKeyRepo.HasFalconKey();
        }

        public static void ValidateFKey(string fKey)
        {
            Interlocked.Increment(ref _validatingCount);
            new EditorSequence(ValidateFalconKey(fKey), e =>
            {
                Interlocked.Decrement(ref _validatingCount);
                CoreLogger.Instance.Error(e);
            }).Start();
        }

        public static void RemoveFKey()
        {
            FKeyRepo.DeleteFalconKey();
        }

        private static IEnumerator ValidateFalconKey(string fKey)
        {
            var httpSequence = new HttpSequence(HttpMethod.Get, ValidateURL + fKey);
            while (httpSequence.MoveNext()) yield return null;

            var response = httpSequence.Current;

            if (response == null)
            {
                EditorUtility.DisplayDialog("Notification", "Information invalid, please retry!", "Ok");
            }
            else
            {
                var validateResponse = JsonUtil.FromJson<FKeyValidateResponse>(response);
                var code = (int)validateResponse.code;
                if (code / 100 != 2)
                {
                    EditorUtility.DisplayDialog("Notification", "Information invalid: " + validateResponse.message, "Ok");
                }
                else
                {
                    
                    FKeyRepo.SaveFalconKey(fKey);
                }
            }
            Interlocked.Decrement(ref _validatingCount);
        }
    }
}