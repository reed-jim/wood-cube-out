#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using GplayCdn.Model;
using Newtonsoft.Json;
using Unity.EditorCoroutines.Editor;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

namespace GplayCdn
{
    /// <summary>
    /// Editor-only utility to build AssetBundles from sprites and upload them to GplayCDN.
    /// Follows the same pattern as Fashion Dressup's ResourceServer + AssetBundleHelper + CdnController.
    /// </summary>
    public static class CdnUploader
    {
        private static readonly string TempAssetBundlePath =
            Path.GetFullPath(Path.Combine(Application.dataPath, "..", "TempAssetBundle"));

        /// <summary>
        /// Upload a single sprite to CDN. Returns the fileId via callback.
        /// </summary>
        public static void UploadSprite(Sprite sprite, Action<string> onSuccess, Action<string> onError)
        {
            EditorCoroutineUtility.StartCoroutineOwnerless(UploadSpriteCoroutine(sprite, onSuccess, onError));
        }

        /// <summary>
        /// Upload multiple sprites in batch. Calls onEachComplete for each sprite with (index, fileId).
        /// </summary>
        public static void UploadSprites(Sprite[] sprites, Action<int, string> onEachComplete, Action<string> onError, Action onAllComplete)
        {
            EditorCoroutineUtility.StartCoroutineOwnerless(UploadSpriteBatchCoroutine(sprites, onEachComplete, onError, onAllComplete));
        }

        private static IEnumerator UploadSpriteCoroutine(Sprite sprite, Action<string> onSuccess, Action<string> onError)
        {
            if (sprite == null)
            {
                onError?.Invoke("Sprite is null");
                yield break;
            }

            string assetPath = AssetDatabase.GetAssetPath(sprite);
            if (string.IsNullOrEmpty(assetPath))
            {
                onError?.Invoke($"Cannot find asset path for sprite: {sprite.name}");
                yield break;
            }

            // Build AssetBundle
            string assetId = assetPath.Split("/").Last().ToLower();
            byte[] bundleBytes = BuildAssetBundle(assetPath, assetId);

            if (bundleBytes == null || bundleBytes.Length == 0)
            {
                onError?.Invoke($"Failed to build AssetBundle for: {sprite.name}");
                yield break;
            }

            // Upload to CDN
            yield return UploadBytesToCdn(bundleBytes, assetId, onSuccess, onError);
        }

        private static IEnumerator UploadSpriteBatchCoroutine(Sprite[] sprites, Action<int, string> onEachComplete, Action<string> onError, Action onAllComplete)
        {
            for (int i = 0; i < sprites.Length; i++)
            {
                if (sprites[i] == null)
                {
                    Debug.LogWarning($"[CdnUploader] Sprite at index {i} is null, skipping.");
                    continue;
                }

                int index = i;
                bool isDone = false;
                string resultId = null;

                yield return UploadSpriteCoroutine(sprites[i], (fileId) =>
                {
                    resultId = fileId;
                    isDone = true;
                }, (error) =>
                {
                    Debug.LogError($"[CdnUploader] Error uploading sprite {sprites[index].name}: {error}");
                    onError?.Invoke(error);
                    isDone = true;
                });

                yield return new WaitUntil(() => isDone);

                if (!string.IsNullOrEmpty(resultId))
                {
                    onEachComplete?.Invoke(index, resultId);
                    Debug.Log($"[CdnUploader] [{index + 1}/{sprites.Length}] Uploaded: {sprites[index].name} -> {resultId}");
                }

                // Small delay between uploads to avoid overwhelming the server
                yield return new WaitForSecondsRealtime(0.2f);
            }

            onAllComplete?.Invoke();
            Debug.Log($"[CdnUploader] Batch upload complete. Total: {sprites.Length}");
        }

        /// <summary>
        /// Build an AssetBundle from an asset path and return the bytes.
        /// </summary>
        private static byte[] BuildAssetBundle(string assetPath, string assetId)
        {
            try
            {
                Directory.CreateDirectory(TempAssetBundlePath);

                var assetBundleBuild = new AssetBundleBuild
                {
                    assetBundleName = assetId,
                    assetNames = new[] { assetPath }
                };

                BuildPipeline.BuildAssetBundles(
                    TempAssetBundlePath,
                    new[] { assetBundleBuild },
                    BuildAssetBundleOptions.None,
                    EditorUserBuildSettings.activeBuildTarget);

                string bundlePath = Path.Combine(TempAssetBundlePath, assetId).Replace("\\", "/");

                if (File.Exists(bundlePath))
                {
                    return File.ReadAllBytes(bundlePath);
                }
                else
                {
                    Debug.LogError($"[CdnUploader] Built AssetBundle file not found: {bundlePath}");
                    return null;
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[CdnUploader] BuildAssetBundle error: {e.Message}");
                return null;
            }
        }

        /// <summary>
        /// Upload bytes to CDN endpoint and return the fileId.
        /// </summary>
        private static IEnumerator UploadBytesToCdn(byte[] bytes, string assetId, Action<string> onSuccess, Action<string> onError)
        {
            string apiKey = CdnConfig.GetApiKey();
            if (string.IsNullOrEmpty(apiKey))
            {
                onError?.Invoke("API key is empty. Check C:/GPlaySDK/ApiKey.txt");
                yield break;
            }

            var uploadForm = new List<IMultipartFormSection>
            {
                new MultipartFormFileSection("File", bytes, assetId, "application/octet-stream")
            };

            var request = UnityWebRequest.Post(CdnConfig.CdnUploadEndpoint, uploadForm);
            request.SetRequestHeader("Api-key", apiKey);
            request.certificateHandler = new BypassCertificateHandler();

            yield return request.SendWebRequest();

            // Wait for download handler
            float timeout = 30f;
            float elapsed = 0f;
            while (!request.downloadHandler.isDone && elapsed < timeout)
            {
                elapsed += 0.25f;
                yield return new WaitForSecondsRealtime(0.25f);
            }

            if (request.result != UnityWebRequest.Result.Success)
            {
                onError?.Invoke($"Upload failed: {request.error}");
                request.Dispose();
                yield break;
            }

            string responseText = request.downloadHandler.text;
            request.Dispose();

            try
            {
                var response = JsonConvert.DeserializeObject<UploadFileResponse>(responseText);
                if (response != null && !string.IsNullOrEmpty(response.FileId))
                {
                    onSuccess?.Invoke(response.FileId);
                }
                else
                {
                    onError?.Invoke($"Invalid response: {responseText}");
                }
            }
            catch (Exception e)
            {
                onError?.Invoke($"Parse response error: {e.Message}, Response: {responseText}");
            }
        }

        /// <summary>
        /// Bypass SSL certificate validation (same as Fashion game).
        /// </summary>
        private class BypassCertificateHandler : CertificateHandler
        {
            protected override bool ValidateCertificate(byte[] certificateData) => true;
        }
    }
}
#endif
