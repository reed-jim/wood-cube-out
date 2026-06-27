using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

namespace GplayCdn
{
    /// <summary>
    /// Runtime CDN downloader - downloads AssetBundles from GplayCDN and loads assets.
    /// Follows the same pattern as Fashion Dressup's StaticController.
    /// </summary>
    public class CdnDownloader : MonoBehaviour
    {
        public static CdnDownloader Instance { get; private set; }

        private readonly Dictionary<string, AssetBundle> _loadedBundles = new Dictionary<string, AssetBundle>();
        private readonly Dictionary<string, Sprite> _spriteCache = new Dictionary<string, Sprite>();
        private readonly HashSet<string> _downloadingIds = new HashSet<string>();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void AutoInit()
        {
            if (Instance == null)
            {
                var go = new GameObject("[CdnDownloader]");
                DontDestroyOnLoad(go);
                Instance = go.AddComponent<CdnDownloader>();
            }
        }

        /// <summary>
        /// Check if a resource is already cached locally.
        /// </summary>
        public bool IsResourceCached(string resourceId)
        {
            if (string.IsNullOrEmpty(resourceId)) return false;
            return _spriteCache.ContainsKey(resourceId) || File.Exists(CdnConfig.GetLocalPath(resourceId));
        }

        /// <summary>
        /// Get a cached sprite if available.
        /// </summary>
        public Sprite GetCachedSprite(string resourceId)
        {
            if (_spriteCache.TryGetValue(resourceId, out var sprite))
                return sprite;
            return null;
        }

        /// <summary>
        /// Download and load a sprite from CDN AssetBundle.
        /// If already cached, returns from cache immediately.
        /// </summary>
        public void LoadSprite(string resourceId, Action<Sprite> onSuccess, Action onError = null, int retryCount = 3)
        {
            if (string.IsNullOrEmpty(resourceId))
            {
                onError?.Invoke();
                return;
            }

            // Check memory cache
            if (_spriteCache.TryGetValue(resourceId, out var cachedSprite))
            {
                onSuccess?.Invoke(cachedSprite);
                return;
            }

            StartCoroutine(LoadSpriteCoroutine(resourceId, onSuccess, onError, retryCount));
        }

        /// <summary>
        /// Download and load multiple sprites. Calls onAllComplete when all are done.
        /// </summary>
        public void LoadSprites(string[] resourceIds, Action onAllComplete, Action<string> onError = null)
        {
            StartCoroutine(LoadSpriteBatchCoroutine(resourceIds, onAllComplete, onError));
        }

        private IEnumerator LoadSpriteBatchCoroutine(string[] resourceIds, Action onAllComplete, Action<string> onError)
        {
            int completed = 0;
            int total = resourceIds.Length;

            foreach (var id in resourceIds)
            {
                if (string.IsNullOrEmpty(id))
                {
                    completed++;
                    continue;
                }

                bool done = false;
                LoadSprite(id, (sprite) =>
                {
                    done = true;
                    completed++;
                }, () =>
                {
                    onError?.Invoke(id);
                    done = true;
                    completed++;
                });

                yield return new WaitUntil(() => done);
            }

            onAllComplete?.Invoke();
        }

        private IEnumerator LoadSpriteCoroutine(string resourceId, Action<Sprite> onSuccess, Action onError, int retryCount)
        {
            string localPath = CdnConfig.GetLocalPath(resourceId);

            // Step 1: Download if not on disk
            if (!File.Exists(localPath))
            {
                if (_downloadingIds.Contains(resourceId))
                {
                    // Wait for another download of same resource
                    yield return new WaitUntil(() => !_downloadingIds.Contains(resourceId));

                    if (_spriteCache.TryGetValue(resourceId, out var sprite))
                    {
                        onSuccess?.Invoke(sprite);
                        yield break;
                    }
                }

                _downloadingIds.Add(resourceId);

                bool downloadSuccess = false;
                yield return DownloadResource(resourceId, localPath, () => downloadSuccess = true, () =>
                {
                    if (retryCount > 0)
                    {
                        Debug.LogWarning($"[CdnDownloader] Retry download: {resourceId}, remaining: {retryCount}");
                    }
                });

                _downloadingIds.Remove(resourceId);

                if (!downloadSuccess)
                {
                    if (retryCount > 0)
                    {
                        yield return LoadSpriteCoroutine(resourceId, onSuccess, onError, retryCount - 1);
                    }
                    else
                    {
                        Debug.LogError($"[CdnDownloader] Failed to download: {resourceId}");
                        onError?.Invoke();
                    }
                    yield break;
                }
            }

            // Step 2: Load AssetBundle from disk
            Sprite resultSprite = null;
            yield return LoadSpriteFromBundle(resourceId, localPath, (sprite) =>
            {
                resultSprite = sprite;
            });

            if (resultSprite != null)
            {
                onSuccess?.Invoke(resultSprite);
            }
            else
            {
                Debug.LogError($"[CdnDownloader] Failed to load sprite from bundle: {resourceId}");
                // Delete corrupted file and retry
                if (File.Exists(localPath))
                {
                    File.Delete(localPath);
                }

                if (retryCount > 0)
                {
                    yield return LoadSpriteCoroutine(resourceId, onSuccess, onError, retryCount - 1);
                }
                else
                {
                    onError?.Invoke();
                }
            }
        }

        private IEnumerator DownloadResource(string resourceId, string localPath, Action onSuccess, Action onError)
        {
            string url = CdnConfig.GetDownloadUrl(resourceId);

            using (var request = UnityWebRequest.Get(url))
            {
                request.SendWebRequest();

                float timeout = 30f;
                float elapsed = 0f;

                while (!request.isDone)
                {
                    elapsed += Time.unscaledDeltaTime;
                    if (elapsed > timeout)
                    {
                        request.Abort();
                        Debug.LogError($"[CdnDownloader] Download timeout: {resourceId}");
                        onError?.Invoke();
                        yield break;
                    }
                    yield return null;
                }

                if (request.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError($"[CdnDownloader] Download error: {resourceId} - {request.error}");
                    onError?.Invoke();
                    yield break;
                }

                // Save to disk
                try
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(localPath));
                    File.WriteAllBytes(localPath, request.downloadHandler.data);
                    onSuccess?.Invoke();
                }
                catch (Exception e)
                {
                    Debug.LogError($"[CdnDownloader] Save file error: {resourceId} - {e.Message}");
                    onError?.Invoke();
                }
            }
        }

        private IEnumerator LoadSpriteFromBundle(string resourceId, string localPath, Action<Sprite> onLoaded)
        {
            // Load AssetBundle
            var bundleRequest = AssetBundle.LoadFromFileAsync(localPath);
            yield return bundleRequest;

            AssetBundle bundle = bundleRequest.assetBundle;
            if (bundle == null)
            {
                onLoaded?.Invoke(null);
                yield break;
            }

            // Load first asset as Texture2D (same as Fashion game's ResourceServer.DownloadSprite)
            string[] assetNames = bundle.GetAllAssetNames();
            if (assetNames.Length == 0)
            {
                bundle.Unload(true);
                onLoaded?.Invoke(null);
                yield break;
            }

            var assetRequest = bundle.LoadAssetAsync<Texture2D>(assetNames[0]);
            yield return assetRequest;

            if (assetRequest.asset is Texture2D texture)
            {
                Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f), 100);
                _spriteCache[resourceId] = sprite;

                // Unload bundle but keep the loaded asset
                bundle.Unload(false);

                onLoaded?.Invoke(sprite);
            }
            else
            {
                // Try loading as Sprite directly
                var spriteRequest = bundle.LoadAssetAsync<Sprite>(assetNames[0]);
                yield return spriteRequest;

                if (spriteRequest.asset is Sprite directSprite)
                {
                    _spriteCache[resourceId] = directSprite;
                    bundle.Unload(false);
                    onLoaded?.Invoke(directSprite);
                }
                else
                {
                    bundle.Unload(true);
                    onLoaded?.Invoke(null);
                }
            }
        }

        /// <summary>
        /// Purge a cached resource.
        /// </summary>
        public void PurgeResource(string resourceId)
        {
            _spriteCache.Remove(resourceId);

            if (_loadedBundles.TryGetValue(resourceId, out var bundle))
            {
                if (bundle != null) bundle.Unload(true);
                _loadedBundles.Remove(resourceId);
            }
        }
    }
}
