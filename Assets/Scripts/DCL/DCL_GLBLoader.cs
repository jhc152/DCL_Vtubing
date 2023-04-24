using UnityEngine;
using UnityEngine.Networking;
using GLTFast;
using System.Collections;
using System.IO;
using System;
using SimpleJSON;

public class DCL_GLBLoader : MonoBehaviour
{

    public string idWearable = "QmYNsLEX6eF9yWYoF49LB8oqC4Lb45FTkLAf9cBdi469d2";

    public JSONNode info_tags;
    public JSONNode info_replaces;
    public JSONNode info_hides;
    public JSONNode info_category;

    public string mainFile = "";


    string url = $"https://peer.decentraland.org/content/contents/";

    public  UpdateSkinnedMeshBones skinnedMEshBones;



   

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


        //GLTFast.Loading.IDownloadProvider downloadProvider = new
        string baseUri = Path.GetDirectoryName(url); // Extraer la URL de la ra�z
        DCL_DownloaderProvider downloadProvider = new DCL_DownloaderProvider(baseUri);

        var gltf = new GltfImport(downloadProvider);
        
        bool success = await gltf.LoadGltfBinary(data, null);

        if (success)
        {

            var root = gltf.GetSourceRoot();

            Debug.Log("=== GLTFast.GltfImport ===");

           


            //for (int i = 0; i < root.textures.Length; i++)
            //{

            //    Debug.Log("<color=yellow>----+++--</color>");
            //    Debug.Log("BaseURL: " + gltf.GetSourceTexture(i).GetImageIndex());
            //    Debug.Log("BaseURL: " + gltf.GetSourceImage(i).uri);
            //    Debug.Log("<color=yellow>----***--</color>");


            //}


            Debug.Log("=== ======= ===");



            success = await gltf.InstantiateMainSceneAsync(transform);
            SkinnedMeshRenderer[] meshRenderer = transform.GetComponentsInChildren<SkinnedMeshRenderer>();



            // Obt�n la ra�z del objeto creado a partir del GLB
            GameObject rootObject = transform.gameObject;

            // Recorre todos los materiales del objeto y carga las texturas relativas
            //foreach (Material material in rootObject.GetComponentsInChildren<Material>())
            //{
            //    if (material.mainTexture != null && !Path.IsPathRooted(material.mainTexture.name))
            //    {
            //        string textureURL = "file://" + Path.Combine(Application.streamingAssetsPath, material.mainTexture.name);

            //        Debug.Log("<color=magenta>archivo local: </color> " + textureURL);
            //        //using (UnityWebRequest www = UnityWebRequestTexture.GetTexture(textureURL))
            //        //{
            //        //    await www.SendWebRequest();

            //        //    if (www.result == UnityWebRequest.Result.Success)
            //        //    {
            //        //        Texture2D texture = DownloadHandlerTexture.GetContent(www);
            //        //        material.mainTexture = texture;
            //        //    }
            //        //    else
            //        //    {
            //        //        Debug.Log(www.error);
            //        //    }
            //        //}
            //    }
            //}









            /**init setting material*/
            //Debug.Log("cuanto en este " + meshRenderer.Length + "    " + gltf.ImageCount);           

            //bool hayTextura = false;
            for (int i = 0; i < meshRenderer.Length; i++)
            {
                //seteo del nuevo material
                meshRenderer[i].material = SetMaterialMToon(meshRenderer[i]);
            }

            //Debug.Break();   
            skinnedMEshBones.InicioUpdateBones();

            // Carga las texturas del archivo GLB
            //foreach (var material in importer.Materials)
            //{
            //    foreach (var textureInfo in material.Value.TextureInfo)
            //    {
            //        // Obtiene la ruta relativa de la textura
            //        string textureURL = texturePath + Path.GetFileName(textureInfo.Texture.Source);

            //        // Carga la textura desde la URL
            //        StartCoroutine(LoadTexture(textureURL, texture => {
            //            // Asigna la textura al material
            //            material.Value.TextureInfo[0].Texture = texture;
            //        }));
            //    }
            //}


        }

        else
        {
            Debug.LogError("Failed to load GLB data.");
        }
    }




    private byte[] glbDataLoaded;


    async void LoadGltfBinaryFromMemory2(byte[] datas)
    {

        //Material newMaterial = new Material(Shader.Find("Standard"));
        

        // var filePath = "/path/to/file.glb";
        byte[] data = datas;
        var gltf = new GltfImport();
        bool success = await gltf.LoadGltfBinary(
            data,
            // The URI of the original data is important for resolving relative URIs within the glTF
            null
            );
        if (success)
        {
            
            success = await gltf.InstantiateMainSceneAsync(transform);                    

            SkinnedMeshRenderer[] meshRenderer = transform.GetComponentsInChildren<SkinnedMeshRenderer>();


            /**init setting material*/
            Debug.Log("cuanto en este " + meshRenderer.Length + "    " + gltf.ImageCount);
            Texture2D myTexture2D;
            Texture2D myTexture2DEmmissive;

            //bool hayTextura = false;

            for (int i = 0; i < meshRenderer.Length; i++)
            {
                Material newMaterial = new Material(Shader.Find("Standard"));
                newMaterial.name = meshRenderer[i].material.name;
                //Debug.Log(" --- - -- - este " + meshRenderer[i].name, meshRenderer[i].gameObject);

                //la que viene
                Texture2D texture2D = (Texture2D)meshRenderer[i].material.mainTexture;

                //hayTextura = false;


                if ((Texture2D)meshRenderer[i].material.mainTexture != null)
                {
                    //hayTextura = true;


                    // Create a temporary RenderTexture of the same size as the texture
                    RenderTexture tmp = RenderTexture.GetTemporary(
                                        texture2D.width,
                                        texture2D.height,
                                        0,
                                        RenderTextureFormat.Default,
                                        RenderTextureReadWrite.Linear);


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
                  // 
                }








                //get color base

                Color baseColor = meshRenderer[i].material.GetColor("baseColorFactor");
                newMaterial.color =  baseColor;



                /******sacando mas parametros del original****/



                newMaterial.SetInt("_SrcBlend", (int)meshRenderer[i].material.GetFloat("_SrcBlend"));
                newMaterial.SetInt("_DstBlend", (int)meshRenderer[i].material.GetFloat("_DstBlend"));
                newMaterial.SetInt("_ZWrite", (int)meshRenderer[i].material.GetFloat("_ZWrite"));


                newMaterial.SetFloat("_Mode", (float)meshRenderer[i].material.GetFloat("_CullMode"));


                //Shader shader = meshRenderer[i].material.shader;

                // Obtener el RenderType del shader
                int renderType = meshRenderer[i].material.renderQueue;

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


                newMaterial.SetFloat("_Metallic", (float)meshRenderer[i].material.GetFloat("roughnessFactor"));

                //newMaterial.SetFloat("_Mode", 2f);

                // Establecer el valor de Cutoff para definir el umbral de transparencia
                //float cutoffValue = 0.5f; // por ejemplo, un valor de 0.5f para un umbral de transparencia del 50%
                //newMaterial.SetFloat("_Cutoff", cutoffValue);
                //newMaterial.SetOverrideTag("RenderType", "TransparentCutout");




                /*emision  */



                newMaterial.EnableKeyword("_EMISSION");


                if ((Texture2D)meshRenderer[i].material.GetTexture("emissiveTexture") != null)
                {


                    Texture2D texture2DEmmisisve = (Texture2D)meshRenderer[i].material.GetTexture("emissiveTexture");


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
                    newMaterial.SetColor("_EmissionColor", (Color)meshRenderer[i].material.GetColor("emissiveFactor") * 8 );
                    newMaterial.SetTexture("_EmissionMap", myTexture2DEmmissive);                   

                }



                //seteo del nuevo material

                meshRenderer[i].material = newMaterial;
                meshRenderer[i].material = SetMaterialStandard (meshRenderer[i]);


            }



            //Debug.Break();           

            skinnedMEshBones.InicioUpdateBones();

           

           


        }
    }





    public void InicioUpdateBones()
    {
        //empezara a pasar los bones de este wearable a
        skinnedMEshBones.InicioUpdateBones();
    }






    public string texturePath = "textures/"; // Reemplaza esto con la ruta relativa a las texturas en el archivo GLB





    IEnumerator LoadTexture(string textureURL, System.Action<Texture2D> callback)
    {
        using (UnityWebRequest www = UnityWebRequestTexture.GetTexture(textureURL))
        {
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                // Crea una nueva textura desde los bytes de la solicitud
                Texture2D texture = new Texture2D(2, 2);
                texture.LoadImage(www.downloadHandler.data);

                // Devuelve la textura al llamador
                callback(texture);
            }
            else
            {
                Debug.Log(www.error);
            }
        }
    }



    public Material SetMaterialStandard(SkinnedMeshRenderer meshRenderer)
    {


        Texture2D myTexture2D;
        Texture2D myTexture2DEmmissive;

        Material newMaterial = new Material(Shader.Find("Standard"));
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
                                RenderTextureFormat.Default,
                                RenderTextureReadWrite.Linear);


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
                                                  // 
        }








        //get color base

        Color baseColor = meshRenderer.material.GetColor("baseColorFactor");
        newMaterial.color = baseColor;



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
            newMaterial.SetColor("_EmissionColor", (Color)meshRenderer.material.GetColor("emissiveFactor") * 8);
            newMaterial.SetTexture("_EmissionMap", myTexture2DEmmissive);

        }

        return newMaterial;



    }





    public Material SetMaterialMToon(SkinnedMeshRenderer meshRenderer)
    {


        Texture2D myTexture2D;
        Texture2D myTexture2DEmmissive;

        Material newMaterial = new Material(Shader.Find("VRM/MToon"));
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
                                RenderTextureFormat.Default,
                                RenderTextureReadWrite.Linear);


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
                                                  // 
        }








        //get color base

        Color baseColor = meshRenderer.material.GetColor("baseColorFactor");
        newMaterial.color = baseColor;



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

       // var gltf = new VRM.VRMExporter()

        //byte[] exportedGLB = GLTFast.Exporter.ExportBinary(gltf);

    }






}
