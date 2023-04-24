using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GLTFast.Loading;
using UnityEngine;
using UnityEngine.Networking;

public delegate bool AssetIdConverter(string uri, out string id);
public class DCL_DownloaderProvider : IDownloadProvider, IDisposable
{


    //private readonly IWebRequestController webRequestController;
    //private readonly AssetIdConverter fileToUrl;
    //private readonly IWebRequestController webRequestController;

    private string baseUrl;
    private readonly AssetIdConverter fileToUrl;
    private List<IDisposable> disposables = new();
    private readonly IWebRequestController webRequestController;

    public DCL_DownloaderProvider(string baseUrl)
    {
        this.baseUrl = baseUrl;
    }


    //public async System.Threading.Tasks.Task<(bool, byte[])> Request(string uri)
    //{
    //    Uri resolvedUri = new Uri(new Uri(_baseUri), uri);

    //    using (UnityWebRequest webRequest = UnityWebRequest.Get(resolvedUri.AbsoluteUri))
    //    {
    //        webRequest.downloadHandler = new DownloadHandlerBuffer();
    //        //await webRequest.SendWebRequest();

    //        //if (webRequest.result != UnityWebRequest.Result.Success)
    //        //{
    //        //    Debug.LogError($"Failed to download {resolvedUri}: {webRequest.error}");
    //        //    return (false, null);
    //        //}

    //        return (true, webRequest.downloadHandler.data);
    //    }
    //}


    public async Task<IDownload> Request(Uri uri)
    {
        string finalUrl = GetFinalUrl(uri);

        WebRequestAsyncOperation asyncOp = (WebRequestAsyncOperation)webRequestController.Get(
            url: finalUrl,
            downloadHandler: new DownloadHandlerBuffer(),
            timeout: 30,
            disposeOnCompleted: false,
            requestAttemps: 3);

        GltfDownloaderWrapper wrapper = new GltfDownloaderWrapper(asyncOp);
        disposables.Add(wrapper);

        while (wrapper.MoveNext()) { await Task.Yield(); }

        if (!wrapper.Success) { Debug.LogError($"<color=Red>[GLTFast WebRequest Failed]</color> {asyncOp.asyncOp.webRequest.url} {asyncOp.asyncOp.webRequest.error}"); }

        return wrapper;
    }

    private string GetFinalUrl(Uri uri)
    {
        var finalUrl = uri.OriginalString;

        finalUrl = finalUrl.Replace(baseUrl, "");

        //

        Debug.Log("<color=green>urlReturn </color> " + fileToUrl(finalUrl, out string cosa));

        string urlReturn =  fileToUrl(finalUrl, out string url) ? url: uri.OriginalString;
        

        return urlReturn;
    }

    public async Task<ITextureDownload> RequestTexture(Uri uri, bool nonReadable)
    {
        string finalUrl = GetFinalUrl(uri);

        var asyncOp = webRequestController.GetTexture(
            url: finalUrl,
            timeout: 30,
            disposeOnCompleted: false,
            requestAttemps: 3);

        GltfTextureDownloaderWrapper wrapper = new GltfTextureDownloaderWrapper(asyncOp);
        disposables.Add(wrapper);

        while (wrapper.MoveNext()) { await Task.Yield(); }

        if (!wrapper.Success) { Debug.LogError("[GLTFast Texture WebRequest Failed] " + asyncOp.asyncOp.webRequest.url); }

        return wrapper;
    }

    public void Dispose()
    {
        foreach (IDisposable disposable in disposables) { disposable.Dispose(); }

        disposables = null;
    }
}


