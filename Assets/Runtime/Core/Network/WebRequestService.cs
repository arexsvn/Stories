using System.Threading.Tasks;
using System;
using UnityEngine.Networking;
using UnityEngine;
using System.Threading;
using System.Web;

public class WebRequestService
{
    public Action<string> OnError;
    private readonly ISerializationOption _serializationOption;
    private readonly ConnectionConfiguration _connectionConfiguration;

    public WebRequestService(ISerializationOption serializationOption, ConnectionConfiguration connectionConfiguration)
    {
        _serializationOption = serializationOption;
        _connectionConfiguration = connectionConfiguration;
    }

    public async Task<T> Get<T>(string url, CancellationTokenSource cancellationTokenSource = null)
    {
        using UnityWebRequest unityWebRequest = UnityWebRequest.Get(url);
        return await SendWebRequest<T>(unityWebRequest, cancellationTokenSource);
    }

    public async Task<T> Put<T>(string url, string data, CancellationTokenSource cancellationTokenSource = null)
    {
        using UnityWebRequest unityWebRequest = UnityWebRequest.Put(url, data);
        return await SendWebRequest<T>(unityWebRequest, cancellationTokenSource);
    }

    public async Task<T> Post<T>(string url, string data, CancellationTokenSource cancellationTokenSource = null)
    {
        using UnityWebRequest unityWebRequest = UnityWebRequest.Post(url, data, _serializationOption.ContentType);
        return await SendWebRequest<T>(unityWebRequest, cancellationTokenSource);
    }

    public async Task<T> Delete<T>(string url, CancellationTokenSource cancellationTokenSource = null)
    {
        using UnityWebRequest unityWebRequest = UnityWebRequest.Delete(url);
        return await SendWebRequest<T>(unityWebRequest, cancellationTokenSource);
    }

    public async Task<T> SendWebRequest<T>(UnityWebRequest unityWebRequest, CancellationTokenSource cancellationTokenSource = null)
    {
        try
        {
            CancellationToken cancellationToken;

            if (cancellationTokenSource == null && _connectionConfiguration != null && _connectionConfiguration.timeOutMsecs != 0)
            {
                cancellationTokenSource = new CancellationTokenSource();
                cancellationTokenSource.CancelAfter(_connectionConfiguration.timeOutMsecs);
            }

            cancellationToken = cancellationTokenSource.Token;

            if (_connectionConfiguration.headers != null)
            {
                foreach(var kvp in _connectionConfiguration.headers)
                {
                    unityWebRequest.SetRequestHeader(kvp.Key, kvp.Value);
                }
            }

            if (_connectionConfiguration.urlParams != null)
            {
                //var queryString = string.Join("&", _connectionConfiguration.urlParams.Select(kvp => $"{kvp.Key}={kvp.Value}"));
                var query = HttpUtility.ParseQueryString(string.Empty);
                foreach (var dict in _connectionConfiguration.urlParams)
                {
                    query[dict.Key] = dict.Value;
                }
                unityWebRequest.url = string.Join("?", unityWebRequest.url, query.ToString());
            }

            UnityWebRequestAsyncOperation operation = unityWebRequest.SendWebRequest();
            while (!unityWebRequest.isDone && !cancellationToken.IsCancellationRequested)
            {
                await Awaitable.NextFrameAsync();
            }

            if (unityWebRequest.result == UnityWebRequest.Result.Success)
            {
                T result = _serializationOption.Deserialize<T>(unityWebRequest.downloadHandler.text);
                return result;
            }
            else
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    Debug.LogWarning("Get Cancelled!");
                }
                else
                {
                    Debug.LogError($"Failed : {unityWebRequest.error}");
                    OnError?.Invoke(unityWebRequest.error);
                }

                unityWebRequest.Abort();

                return default;
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"{nameof(Get)} failed: {ex.Message}");
            OnError?.Invoke(ex.Message);
            return default;
        }
    }
}