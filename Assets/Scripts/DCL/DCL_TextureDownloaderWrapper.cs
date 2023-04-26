using GLTFast.Loading;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DCL_TextureDownloaderWrapper : ITextureDownload
{



    private Texture2D _texture;

    public DCL_TextureDownloaderWrapper(Texture2D texture)
    {
        _texture = texture;
    }

    public Texture2D Texture
    {
        get { return _texture; }
    }


    public bool Success => _texture != null;
    

    public string Error => throw new System.NotImplementedException();

    public byte[] Data => throw new System.NotImplementedException();

    public string Text => throw new System.NotImplementedException();

    public bool? IsBinary => throw new System.NotImplementedException();

    public void Dispose()
    {
        if (_texture != null)
        {
            //UnityEngine.Object.Destroy(_texture);
            _texture = null;
        }
    }
}
