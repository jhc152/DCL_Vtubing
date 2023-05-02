using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using GLTFast.Loading;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

public delegate bool AssetIdConverter(string uri, out string id);
public class DCL_DownloaderProvider : IDownloadProvider, IDisposable
{       
    private string baseUrl;
    private readonly AssetIdConverter fileToUrl;
    private List<IDisposable> disposables = new();
    private readonly IWebRequestController webRequestController;
    private string hash;

    string urlDCL = $"https://peer.decentraland.org/content/contents/";

    public DCL_DownloaderProvider(string baseUrl, string hash)
    {
        this.baseUrl = baseUrl;
        this.hash = hash;
    }        

    public async Task<IDownload> Request(Uri uri)
    {
        Debug.Log("Request ---- " + uri);
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
        string urlReturn =  fileToUrl(finalUrl, out string url) ? url: uri.OriginalString;  
        return urlReturn;
    }


   

    public async Task<ITextureDownload> RequestTexture(Uri uri, bool nonReadable)
    {
        Debug.Log("RequestTexture ---- " + uri);
        string nameFile = uri.ToString(); 
        string path = "";
        Texture2D texture = null;
        if (nameFile == "Avatar_FemaleSkinBase.png" || nameFile == "Avatar_MaleSkinBase.png")
        {
            //path = "Assets/Resources/textures/" + nameFile;
            //texture = AssetDatabase.LoadAssetAtPath<Texture2D>(path);


            string nameFilWithoutExtension = Path.GetFileNameWithoutExtension(nameFile);

            path = "textures/" + nameFilWithoutExtension;
            texture = Resources.Load<Texture2D>(path);


        }
        else if (nameFile.Contains("AvatarWearables"))
        {
            //path = "Assets/Resources/textures/AvatarWearables_TX.png";
            //texture = AssetDatabase.LoadAssetAtPath<Texture2D>(path);

            path = "textures/AvatarWearables_TX";
            texture = Resources.Load<Texture2D>(path);
        }
        else
        {
            byte[] textureData = await LoadTextureAsync();
            Texture2D textureByHash = new Texture2D(10,10);// = await LoadTextureAsync(textureData);
            textureByHash.LoadImage(textureData);           
            texture = textureByHash;
        }

        if (texture == null)
        {
            Debug.LogError($"AR Texture not found at path {path}");
            return null;
        }
        else
        { 
            return new DCL_TextureDownloaderWrapper(texture);
        }
    }

    public void Dispose()
    {
        foreach (IDisposable disposable in disposables) { disposable.Dispose(); }
        disposables = null;
    }

   






    string urlDCLTest = $"https://peer.decentraland.org/content/contents/QmWg88zQZcXkx7RYhHpgpNBTskmWb58VatqmcDEBxTJDoK";
    public async Task<byte[]> LoadTextureAsync()
    {
        // Crea una tarea que se completará cuando la carga del GLB termine
        var tcs = new TaskCompletionSource<byte[]>();

        //UnityWebRequest request = UnityWebRequest.Get(urlDCLTest);
        UnityWebRequest request = UnityWebRequest.Get(urlDCL+ hash);
        request.SetRequestHeader("Accept-Encoding", "identity");
        request.downloadHandler = new DownloadHandlerBuffer();
        var operation = request.SendWebRequest();

        // Utiliza el método AsyncOperation.Completed para detectar cuando la carga del GLB termine
        operation.completed += (AsyncOperation op) =>
        {
            // Si hubo algún error en la solicitud, marca la tarea como fallida
            if (((UnityWebRequestAsyncOperation)op).webRequest.result != UnityWebRequest.Result.Success)
            {
                tcs.SetException(new System.Exception(((UnityWebRequestAsyncOperation)op).webRequest.error));
                return;
            }

            // Si la carga del GLB fue exitosa, marca la tarea como completada
            tcs.SetResult(((UnityWebRequestAsyncOperation)op).webRequest.downloadHandler.data);
        };

        // Espera a que la tarea se complete y devuelve los datos del GLB
        return await tcs.Task;
    }



   




}


