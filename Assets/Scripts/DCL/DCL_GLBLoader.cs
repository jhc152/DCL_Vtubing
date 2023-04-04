using UnityEngine;
using UnityEngine.Networking;
using GLTFast;
using System.Collections;
using System.IO;
using System;

public class DCL_GLBLoader : MonoBehaviour
{

    public string idWearable = "QmYNsLEX6eF9yWYoF49LB8oqC4Lb45FTkLAf9cBdi469d2";
    string url = $"https://peer.decentraland.org/content/contents/";

    public  UpdateSkinnedMeshBones skinnedMEshBones;

   

    public void StartGLBLoader()
    {

        if (idWearable != "")
        {
            url += idWearable;
            StartCoroutine(LoadGLB());



        }
        else
        {
            skinnedMEshBones.InicioUpdateBonesComplete();
        }



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


    private byte[] glbDataLoaded;


    async void LoadGltfBinaryFromMemory(byte[] datas)
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
            //Debug.Log("sdasdasd");
            success = await gltf.InstantiateMainSceneAsync(transform);          
           

            SkinnedMeshRenderer[] meshRenderer = transform.GetComponentsInChildren<SkinnedMeshRenderer>();

           // Debug.Log("cuanto en este " + meshRenderer.Length + "    " + gltf.ImageCount);

            Texture2D myTexture2D;
            Texture2D myTexture2DEmmissive;


            bool hayTextura = false;

            for (int i = 0; i < meshRenderer.Length; i++)
            {


                Material newMaterial = new Material(Shader.Find("Standard"));




                //Debug.Log(" --- - -- - este " + meshRenderer[i].name, meshRenderer[i].gameObject);

                //la que viene
                Texture2D texture2D = (Texture2D)meshRenderer[i].material.mainTexture;

                hayTextura = false;


                if ((Texture2D)meshRenderer[i].material.mainTexture != null)
                {
                    hayTextura = true;


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


                 newMaterial.SetFloat("_Mode", (float)meshRenderer[i].material.GetFloat("_Mode"));


                //Shader shader = meshRenderer[i].material.shader;

                // Obtener el RenderType del shader
                int renderType = meshRenderer[i].material.renderQueue;

               newMaterial.renderQueue = renderType;

                Debug.Log("<color=yellow> ----------------    </color>" + renderType);

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



                newMaterial.SetOverrideTag("RenderType", "Transparent");















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
                    // 

                }



                //seteo del nuevo material

                meshRenderer[i].material = newMaterial;

            }




           
            //Debug.Break();           

           skinnedMEshBones.InicioUpdateBones();

           

           


        }
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
