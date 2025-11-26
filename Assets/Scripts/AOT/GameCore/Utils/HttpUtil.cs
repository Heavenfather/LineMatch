using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace GameCore.Utils
{
    public static class HttpUtil
    {
        public const int MaxRetryCount = 5;

        /// <summary>
        /// GET请求与获取结果。
        /// </summary>
        /// <param name="url">网络URL。</param>
        /// <param name="timeout">超时时间。</param>
        /// <returns>请求结果。</returns>
        public static async UniTask<string> Get(string url, Dictionary<string, string> headers = null,
            float timeout = 5f)
        {
            UnityWebRequest RequestFunc()
            {
                UnityWebRequest unityWebRequest = UnityWebRequest.Get(url);
                if (headers != null)
                {
                    foreach (var header in headers)
                    {
                        unityWebRequest.SetRequestHeader(header.Key, header.Value);
                    }
                }
                return unityWebRequest;
            }

            return await SendWebRequest(RequestFunc, timeout);
        }

        /// <summary>
        /// GET请求获取网络纹理资源
        /// </summary>
        /// <param name="url"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        public static async UniTask<Texture2D> GetTexture(string url, float timeout = 5f)
        {
            var cts = new CancellationTokenSource();
            cts.CancelAfterSlim(TimeSpan.FromSeconds(timeout));

            return await SendWebRequestTexture(url, cts);
        }

        /// <summary>
        /// Post请求与获取结果.
        /// </summary>
        /// <param name="url">网络URL。</param>
        /// <param name="postData">Post数据。</param>
        /// <param name="timeout">超时时间。</param>
        /// <returns>请求结果。</returns>
        public static async UniTask<string> Post(string url, string postData, Dictionary<string, string> headers = null,
            float timeout = 5f)
        {
            UnityWebRequest RequestFunc()
            {
                UnityWebRequest unityWebRequest = UnityWebRequest.PostWwwForm(url, postData);
                if (headers != null)
                {
                    foreach (var header in headers)
                    {
                        unityWebRequest.SetRequestHeader(header.Key, header.Value);
                    }
                }
                return unityWebRequest;
            }

            return await SendWebRequest(RequestFunc, timeout);
        }

        /// <summary>
        /// Post请求与获取结果.
        /// </summary>
        /// <param name="url">网络URL。</param>
        /// <param name="formFields">Post数据。</param>
        /// <param name="timeout">超时时间。</param>
        /// <returns>请求结果。</returns>
        public static async UniTask<string> Post(string url, Dictionary<string, string> formFields, float timeout = 5f)
        {
            UnityWebRequest RequestFunc()
            {
                UnityWebRequest unityWebRequest = UnityWebRequest.Post(url, formFields);
                return unityWebRequest;
            }
            return await SendWebRequest(RequestFunc, timeout);
        }

        /// <summary>
        /// Post请求与获取结果.
        /// </summary>
        /// <param name="url">网络URL。</param>
        /// <param name="formData">Post数据。</param>
        /// <param name="timeout">超时时间。</param>
        /// <returns>请求结果。</returns>
        public static async UniTask<string> Post(string url, WWWForm formData, float timeout = 5f)
        {
            UnityWebRequest RequestFunc()
            {
                UnityWebRequest unityWebRequest = UnityWebRequest.Post(url, formData);
                return unityWebRequest;
            }
            
            return await SendWebRequest(RequestFunc, timeout);
        }

        /// <summary>
        /// 抛出网络请求。
        /// </summary>
        /// <param name="requestCreator">UnityWebRequest。</param>
        /// <param name="timeout">CancellationTokenSource。</param>
        /// <returns>请求结果。</returns>
        public static async UniTask<string> SendWebRequest(Func<UnityWebRequest> requestCreator, float timeout)
        {
            int retryCount = 0;
            UnityWebRequest unityWebRequest = null;

            try
            {
                while (retryCount <= MaxRetryCount)
                {
                    try
                    {
                        unityWebRequest?.Dispose();
                        unityWebRequest = requestCreator();
                        CancellationTokenSource timeoutToken = new CancellationTokenSource();
                        timeoutToken.CancelAfterSlim(TimeSpan.FromSeconds(timeout));
                        var (isCanceled, _) = await unityWebRequest.SendWebRequest()
                            .WithCancellation(timeoutToken.Token)
                            .SuppressCancellationThrow();

                        // 请求成功完成
                        if (!isCanceled && unityWebRequest.result == UnityWebRequest.Result.Success)
                        {
                            string result = unityWebRequest.downloadHandler.text;
                            return result;
                        }
                    }
                    catch (OperationCanceledException ex)
                    {
                        Debug.LogWarning("请求超时.");
                    }
                    catch (Exception ex)
                    {
                        // 非取消异常
                        Debug.LogError($"请求异常: {ex.Message}");
                    }

                    retryCount++;
                    Debug.Log($"请求重试:{retryCount} / {MaxRetryCount}.");
                    if (retryCount <= MaxRetryCount)
                    {
                        await UniTask.Delay(TimeSpan.FromSeconds(3));
                    }
                }
            }
            finally
            {
                unityWebRequest?.Dispose();
            }

            return string.Empty;
        }

        /// <summary>
        /// 抛出网络请求。
        /// </summary>
        public static UniTask<Texture2D> SendWebRequestTexture(string url, CancellationTokenSource cts)
        {
            UniTaskCompletionSource<Texture2D> tcs = new UniTaskCompletionSource<Texture2D>();
            UniTask.Create(async () =>
            {
                UnityWebRequest unityWebRequest = new UnityWebRequest(url);
                DownloadHandlerTexture downloadHandlerTexture = new DownloadHandlerTexture(true);
                unityWebRequest.downloadHandler = downloadHandlerTexture;
                try
                {
                    var (isCanceled, _) = await unityWebRequest.SendWebRequest().WithCancellation(cts.Token)
                        .SuppressCancellationThrow();
                    if (isCanceled)
                    {
                        tcs.TrySetResult(null);
                    }
                    else
                    {
                        tcs.TrySetResult(unityWebRequest.result == UnityWebRequest.Result.Success
                            ? downloadHandlerTexture.texture
                            : null);
                    }

                    unityWebRequest.Dispose();
                }
                catch (OperationCanceledException ex)
                {
                    if (ex.CancellationToken == cts.Token)
                    {
                        tcs.TrySetResult(null);
                        unityWebRequest.Dispose();
                    }
                }
            });
            return tcs.Task;
        }
    }
}