using UnityEngine;
using UnityEngine.Networking;
using GLTFast;
using System.Collections;
using System.IO;
using System;
using SimpleJSON;

public class DCL_GLBLoader : MonoBehaviour
{
    // hash del glb
    public string idWearable = "QmYNsLEX6eF9yWYoF49LB8oqC4Lb45FTkLAf9cBdi469d2";

    // hash de la textura
    public string hashTexture = "";

    public bool wearableBase = false;

    public JSONNode info_tags;
    public JSONNode info_replaces;
    public JSONNode info_hides;
    public JSONNode info_category;

    public string mainFile = "";

    string url = $"https://peer.decentraland.org/content/contents/";
    string urlTest = $"https://peer.decentraland.org/content/contents/QmYNsLEX6eF9yWYoF49LB8oqC4Lb45FTkLAf9cBdi469d2";

    public  UpdateSkinnedMeshBones skinnedMEshBones;
    private byte[] glbDataLoaded;


    public void StartGLBLoader()
    {
        if (idWearable != "")
        {
            url += idWearable;
            Debug.Log("main_file ::: " + mainFile + " url::" + url);
            StartCoroutine(LoadGLB());
        }
        else
        {
            skinnedMEshBones.InicioUpdateBonesComplete();
        }

    }


    public void StartGLBLoaderFinished()
    {
        skinnedMEshBones.InicioUpdateBonesComplete();
    }

    private IEnumerator LoadGLB()
    {

        UnityWebRequest request = UnityWebRequest.Get(url);
        request.SetRequestHeader("Accept-Encoding", "identity");
        yield return request.SendWebRequest();
        if (request.result == UnityWebRequest.Result.Success)
        {
            byte[] glbData = request.downloadHandler.data;
            glbDataLoaded = glbData;          
            LoadGltfBinaryFromMemory(glbData);
        }
        else
        {
            Debug.LogError(request.error);
        }
    }



    async void LoadGltfBinaryFromMemory(byte[] datas)
    {
        byte[] data = datas;      
        string baseUri = Path.GetDirectoryName(Application.dataPath);
        
       // Debug.Log("<color=yellow>:::::::</color>base uri "+baseUri );
        DCL_DownloaderProvider downloadProvider = new DCL_DownloaderProvider(baseUri, hashTexture);
        var gltf = new GltfImport(downloadProvider);        
        bool success = await gltf.LoadGltfBinary(data, null);
        if (success)
        {
            var root = gltf.GetSourceRoot();   
            success = await gltf.InstantiateMainSceneAsync(transform);
            SkinnedMeshRenderer[] meshRenderer = transform.GetComponentsInChildren<SkinnedMeshRenderer>();
            // Obtén la raíz del objeto creado a partir del GLB
            GameObject rootObject = transform.gameObject;           

            /**init setting material*/
            for (int i = 0; i < meshRenderer.Length; i++)
            {
                //seteo del nuevo material
              meshRenderer[i].material = SetMaterialMToon(meshRenderer[i]);
            }           
           
            
            skinnedMEshBones.InicioUpdateBones();  
        }
        else
        {
            Debug.LogError("Failed to load GLB data.");
        }
    }








    public void InicioUpdateBones()
    {
        //empezara a pasar los bones de este wearable a
        skinnedMEshBones.InicioUpdateBones();
    }






    public string texturePath = "textures/"; // Reemplaza esto con la ruta relativa a las texturas en el archivo GLB



    public Material SetMaterialMToonOld(SkinnedMeshRenderer meshRenderer)
    {


        Texture2D myTexture2D;
        Texture2D myTexture2DEmmissive;

        //  Material newMaterial = new Material(Shader.Find("VRM/MToon"));
        Material newMaterial = new Material(Shader.Find("glTF/Unlit"));
        newMaterial.name = meshRenderer.material.name;




        //Debug.Log(" --- - -- - este " + meshRenderer[i].name, meshRenderer[i].gameObject);

        //la que viene
        Texture2D texture2D = (Texture2D)meshRenderer.material.mainTexture;

        //hayTextura = false;


        if ((Texture2D)meshRenderer.material.mainTexture != null)
        {
            //hayTextura = true;


            // Create a temporary RenderTexture of the same size as the texture
            RenderTexture tmp = RenderTexture.GetTemporary(
                                texture2D.width,
                                texture2D.height,
                                0,
                                RenderTextureFormat.ARGB32,
                                RenderTextureReadWrite.sRGB);


            // Blit the pixels on texture to the RenderTexture
            Graphics.Blit(texture2D, tmp);


            // Backup the currently set RenderTexture
            RenderTexture previous = RenderTexture.active;


            // Set the current RenderTexture to the temporary one we created
            RenderTexture.active = tmp;


            // Create a new readable Texture2D to copy the pixels to it
            myTexture2D = new Texture2D(texture2D.width, texture2D.height);


            // Copy the pixels from the RenderTexture to the new Texture
            myTexture2D.ReadPixels(new Rect(0, 0, tmp.width, tmp.height), 0, 0);
            myTexture2D.Apply();


            // Reset the active RenderTexture
            RenderTexture.active = previous;

            // Release the temporary RenderTexture
            RenderTexture.ReleaseTemporary(tmp);

            newMaterial.mainTexture = myTexture2D;//(Texture2D)meshRenderer[i].material.mainTexture;

            newMaterial.SetTexture("_ShadeTexture", myTexture2D);
            // 
        }








        //get color base

        Color baseColor = meshRenderer.material.GetColor("baseColorFactor");
        newMaterial.color = baseColor;


        newMaterial.SetColor("_ShadeColor", baseColor);



        /******sacando mas parametros del original****/



        newMaterial.SetInt("_SrcBlend", (int)meshRenderer.material.GetFloat("_SrcBlend"));
        newMaterial.SetInt("_DstBlend", (int)meshRenderer.material.GetFloat("_DstBlend"));
        newMaterial.SetInt("_ZWrite", (int)meshRenderer.material.GetFloat("_ZWrite"));


        newMaterial.SetFloat("_Mode", (float)meshRenderer.material.GetFloat("_CullMode"));


        //Shader shader = meshRenderer[i].material.shader;

        // Obtener el RenderType del shader
        int renderType = meshRenderer.material.renderQueue;

        newMaterial.renderQueue = renderType;

        // Debug.Log("<color=yellow> ----------------    </color>" + renderType, gameObject);

        if (renderType <= 2000)
        {

            newMaterial.SetInt("_ZWrite", 1);
            newMaterial.DisableKeyword("_ALPHATEST_ON");
            newMaterial.DisableKeyword("_ALPHABLEND_ON");
            newMaterial.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            // newMaterial.renderQueue = -1;
        }
        else if (renderType <= 2450)


        {
            newMaterial.SetInt("_ZWrite", 1);
            newMaterial.EnableKeyword("_ALPHATEST_ON");
            newMaterial.DisableKeyword("_ALPHABLEND_ON");
            newMaterial.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            newMaterial.renderQueue = 2450;
            newMaterial.SetOverrideTag("RenderType", "TransparentCutout");

        }


        else if (renderType <= 3000)


        {
            newMaterial.SetInt("_ZWrite", 0);
            newMaterial.DisableKeyword("_ALPHATEST_ON");
            newMaterial.EnableKeyword("_ALPHABLEND_ON");
            newMaterial.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            newMaterial.renderQueue = 3000;
            newMaterial.SetOverrideTag("RenderType", "Transparent");

        }


        else


        {
            newMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
            newMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            newMaterial.SetInt("_ZWrite", 0);
            newMaterial.DisableKeyword("_ALPHATEST_ON");
            newMaterial.DisableKeyword("_ALPHABLEND_ON");
            newMaterial.EnableKeyword("_ALPHAPREMULTIPLY_ON");
            newMaterial.renderQueue = 3000;
            newMaterial.SetOverrideTag("RenderType", "Transparent");

        }


        newMaterial.SetFloat("_Metallic", (float)meshRenderer.material.GetFloat("roughnessFactor"));

        //newMaterial.SetFloat("_Mode", 2f);

        // Establecer el valor de Cutoff para definir el umbral de transparencia
        //float cutoffValue = 0.5f; // por ejemplo, un valor de 0.5f para un umbral de transparencia del 50%
        //newMaterial.SetFloat("_Cutoff", cutoffValue);
        //newMaterial.SetOverrideTag("RenderType", "TransparentCutout");




        /*emision  */



        newMaterial.EnableKeyword("_EMISSION");


        if ((Texture2D)meshRenderer.material.GetTexture("emissiveTexture") != null)
        {


            Texture2D texture2DEmmisisve = (Texture2D)meshRenderer.material.GetTexture("emissiveTexture");


            // Create a temporary RenderTexture of the same size as the texture
            RenderTexture tmpEmmisive = RenderTexture.GetTemporary(
                                texture2DEmmisisve.width,
                                texture2DEmmisisve.height,
                                0,
                                RenderTextureFormat.Default,
                                RenderTextureReadWrite.Linear);


            // Blit the pixels on texture to the RenderTexture
            Graphics.Blit(texture2DEmmisisve, tmpEmmisive);

            // Backup the currently set RenderTexture
            RenderTexture previousEmmisive = RenderTexture.active;

            // Set the current RenderTexture to the temporary one we created
            RenderTexture.active = tmpEmmisive;

            // Create a new readable Texture2D to copy the pixels to it
            myTexture2DEmmissive = new Texture2D(texture2DEmmisisve.width, texture2DEmmisisve.height);

            // Copy the pixels from the RenderTexture to the new Texture
            myTexture2DEmmissive.ReadPixels(new Rect(0, 0, tmpEmmisive.width, tmpEmmisive.height), 0, 0);
            myTexture2DEmmissive.Apply();

            // Reset the active RenderTexture
            RenderTexture.active = previousEmmisive;

            // Release the temporary RenderTexture
            RenderTexture.ReleaseTemporary(tmpEmmisive);

            /*************************************/
            // Obtener el factor emissive  
            //// Obtener la textura emissive                   
            newMaterial.SetColor("_EmissionColor", (Color)meshRenderer.material.GetColor("emissiveFactor") * 25);
            newMaterial.SetTexture("_EmissionMap", myTexture2DEmmissive);

        }

        newMaterial.SetFloat("_OutlineWidthMode", 1);
        newMaterial.SetFloat("_OutlineCullMode", 10);

        newMaterial.SetOverrideTag("RenderType", "Cutout");


        return newMaterial;



    }


    public Shader customShader;
    public Material SetMaterialMToon(SkinnedMeshRenderer meshRenderer)
    {


        Texture2D myTexture2D;
        Texture2D myTexture2DEmmissive;

       //Material newMaterial = new Material(Shader.Find("UniGLTF/UniUnlit"));

       Material newMaterial = new Material(Shader.Find("VRM/MToon"));
        //Material newMaterial = new Material(customShader);
        newMaterial.name = meshRenderer.material.name;

       


        //Debug.Log(" --- - -- - este " + meshRenderer[i].name, meshRenderer[i].gameObject);

        //la que viene
        Texture2D texture2D = (Texture2D)meshRenderer.material.mainTexture;

        //hayTextura = false;


        if ((Texture2D)meshRenderer.material.mainTexture != null)
        {
            //hayTextura = true;


            // Create a temporary RenderTexture of the same size as the texture
            //RenderTexture tmp = RenderTexture.GetTemporary(
            //                    texture2D.width,
            //                    texture2D.height,
            //                    0,
            //                    RenderTextureFormat.Default,
            //                    RenderTextureReadWrite.Linear);

            RenderTexture tmp = RenderTexture.GetTemporary(
                            texture2D.width,
                            texture2D.height,
                            0,
                            RenderTextureFormat.ARGB32,
                            RenderTextureReadWrite.sRGB);


            // Blit the pixels on texture to the RenderTexture
            Graphics.Blit(texture2D, tmp);


            // Backup the currently set RenderTexture
            RenderTexture previous = RenderTexture.active;


            // Set the current RenderTexture to the temporary one we created
            RenderTexture.active = tmp;


            // Create a new readable Texture2D to copy the pixels to it
            myTexture2D = new Texture2D(texture2D.width, texture2D.height);


            // Copy the pixels from the RenderTexture to the new Texture
            myTexture2D.ReadPixels(new Rect(0, 0, tmp.width, tmp.height), 0, 0);
            myTexture2D.Apply();


            // Reset the active RenderTexture
            RenderTexture.active = previous;

            // Release the temporary RenderTexture
            RenderTexture.ReleaseTemporary(tmp);

            newMaterial.mainTexture = myTexture2D;//(Texture2D)meshRenderer[i].material.mainTexture;

            newMaterial.SetTexture("_ShadeTexture", myTexture2D);
            // 
        }








        //get color base

        Color baseColor = meshRenderer.material.GetColor("baseColorFactor");
        newMaterial.color = baseColor;


        newMaterial.SetColor("_ShadeColor", baseColor);



        /******sacando mas parametros del original****/



        newMaterial.SetInt("_SrcBlend", (int)meshRenderer.material.GetFloat("_SrcBlend"));
        newMaterial.SetInt("_DstBlend", (int)meshRenderer.material.GetFloat("_DstBlend"));
        newMaterial.SetInt("_ZWrite", (int)meshRenderer.material.GetFloat("_ZWrite"));


        newMaterial.SetFloat("_Mode", (float)meshRenderer.material.GetFloat("_CullMode"));


        //Shader shader = meshRenderer[i].material.shader;

        // Obtener el RenderType del shader
        int renderType = meshRenderer.material.renderQueue;

        newMaterial.renderQueue = renderType;

        // Debug.Log("<color=yellow> ----------------    </color>" + renderType, gameObject);

        if (renderType <= 2000)
        {

            newMaterial.SetInt("_ZWrite", 1);
            newMaterial.DisableKeyword("_ALPHATEST_ON");
            newMaterial.DisableKeyword("_ALPHABLEND_ON");
            newMaterial.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            // newMaterial.renderQueue = -1;
        }
        else if (renderType <= 2450)


        {
            newMaterial.SetInt("_ZWrite", 1);
            newMaterial.EnableKeyword("_ALPHATEST_ON");
            newMaterial.DisableKeyword("_ALPHABLEND_ON");
            newMaterial.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            newMaterial.renderQueue = 2450;
            newMaterial.SetOverrideTag("RenderType", "TransparentCutout");

        }


        else if (renderType <= 3000)


        {
            newMaterial.SetInt("_ZWrite", 0);
            newMaterial.DisableKeyword("_ALPHATEST_ON");
            newMaterial.EnableKeyword("_ALPHABLEND_ON");
            newMaterial.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            newMaterial.renderQueue = 3000;
            newMaterial.SetOverrideTag("RenderType", "Transparent");

        }


        else


        {
            newMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
            newMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            newMaterial.SetInt("_ZWrite", 0);
            newMaterial.DisableKeyword("_ALPHATEST_ON");
            newMaterial.DisableKeyword("_ALPHABLEND_ON");
            newMaterial.EnableKeyword("_ALPHAPREMULTIPLY_ON");
            newMaterial.renderQueue = 3000;
            newMaterial.SetOverrideTag("RenderType", "Transparent");

        }


        newMaterial.SetFloat("_Metallic", (float)meshRenderer.material.GetFloat("roughnessFactor"));

        //newMaterial.SetFloat("_Mode", 2f);

        // Establecer el valor de Cutoff para definir el umbral de transparencia
        //float cutoffValue = 0.5f; // por ejemplo, un valor de 0.5f para un umbral de transparencia del 50%
        //newMaterial.SetFloat("_Cutoff", cutoffValue);
        //newMaterial.SetOverrideTag("RenderType", "TransparentCutout");




        /*emision  */



        newMaterial.EnableKeyword("_EMISSION");


        if ((Texture2D)meshRenderer.material.GetTexture("emissiveTexture") != null)
        {


            Texture2D texture2DEmmisisve = (Texture2D)meshRenderer.material.GetTexture("emissiveTexture");


            // Create a temporary RenderTexture of the same size as the texture
            RenderTexture tmpEmmisive = RenderTexture.GetTemporary(
                                texture2DEmmisisve.width,
                                texture2DEmmisisve.height,
                                0,
                                RenderTextureFormat.Default,
                                RenderTextureReadWrite.Linear);


            // Blit the pixels on texture to the RenderTexture
            Graphics.Blit(texture2DEmmisisve, tmpEmmisive);

            // Backup the currently set RenderTexture
            RenderTexture previousEmmisive = RenderTexture.active;

            // Set the current RenderTexture to the temporary one we created
            RenderTexture.active = tmpEmmisive;

            // Create a new readable Texture2D to copy the pixels to it
            myTexture2DEmmissive = new Texture2D(texture2DEmmisisve.width, texture2DEmmisisve.height);

            // Copy the pixels from the RenderTexture to the new Texture
            myTexture2DEmmissive.ReadPixels(new Rect(0, 0, tmpEmmisive.width, tmpEmmisive.height), 0, 0);
            myTexture2DEmmissive.Apply();

            // Reset the active RenderTexture
            RenderTexture.active = previousEmmisive;

            // Release the temporary RenderTexture
            RenderTexture.ReleaseTemporary(tmpEmmisive);

            /*************************************/
            // Obtener el factor emissive  
            //// Obtener la textura emissive                   
            newMaterial.SetColor("_EmissionColor", (Color)meshRenderer.material.GetColor("emissiveFactor") * 25);
            newMaterial.SetTexture("_EmissionMap", myTexture2DEmmissive);

        }

        newMaterial.SetFloat("_OutlineWidthMode", 1);
        newMaterial.SetFloat("_OutlineCullMode", 10);

        newMaterial.SetOverrideTag("RenderType", "Cutout");

       
        return newMaterial;



    }







    public void DCL_ExportGLB()
    {

    }


    public void DCL_ExportVRM()
    {      

    }






}
