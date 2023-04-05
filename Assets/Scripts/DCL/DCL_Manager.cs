
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Networking;
//using Siccity.GLTFUtility;
using SimpleJSON;
using System.Linq;
using VRM.RuntimeExporterSample;
using System.Threading.Tasks;
using TMPro;

using System.IO;
using UnityEditor;

using Newtonsoft.Json;

[Serializable]
public class WearablesEncontrado
{
    public string wearablePointer;
    [SerializeField]
    public IEnumerator GetWearablePointer;

    public GameObject WearableContent;

    //public JSONNode dataWearablePoint;

}


[Serializable]
public enum AvatarGenero{ Unknow, Male, Female}



[Serializable ]
public class AvatarInfo
{
    public AvatarGenero genero = AvatarGenero.Unknow;
    public string generoStr = "";
    public Color eyes = Color.white; //Eyebrows_MAT
    public Color hair = Color.white; //Hair_MAT
    public Color skin = Color.white; //AvatarSkin_MAT
}


/// <summary>
/// 
/// </summary>
public class DCL_Manager : MonoBehaviour
{
    //datos de perfil
    private const string API_DCL_PROFILE = "https://peer.decentraland.org/lambdas/profile/";

    //datos de un wearable
    private const string API_DCL_WEARABLE_POINTER = "https://peer.decentraland.org/content/entities/wearables/?pointer=";

    public static DCL_Manager Instance;

    public string hashUsser = "";

    [HideInInspector]
    public JSONNode dataProfile;

    [HideInInspector]
    public JSONNode dataProfileAvatar;

    [HideInInspector]
    public JSONNode dataProfileAvatarWearables;

    /*almacena la info del avatar*/
    public AvatarInfo avatarInfo = new AvatarInfo();



    public List<WearablesEncontrado> WearableListProfiles = new List<WearablesEncontrado>();

    //vars POINTER receiver//
    [HideInInspector]
    public JSONNode dataWearablePointer;
    public GameObject PrefabDCLLoader;
    public Transform ParentDCLLoaders;
   


    //el objeto fbx que cargara los glbs desde dcl
    public GameObject fbx;
    //el objeto interno dentro del fbx donde se almacenaran los meshes
    public GameObject fbx_meshes_container;
    //el objeto que tiene los bones dentro del fbx
    public Transform rootBoneFBX;

    //el objeto del vrm full
    public GameObject vrm_full;
    //el objeto interno dentro del vrm donde se almacenaran los meshes importados
    public GameObject vrm_meshes_container;


    public SkinnedMeshRenderer vrm_hair_male;
    public SkinnedMeshRenderer vrm_hair_female;
    public SkinnedMeshRenderer vrm_head;


    bool show_hair = false;
    bool show_head = false;
    bool show_body = false;

    public TMP_InputField inputHashUser;



    public UpdateSkinnedMeshBones updateSkinnedMesh;


    public bool loadingProfile = false;

    public void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        LoadProfile();
    }


    public void ReloadProfile()
    {
        LoadProfile();
    }


    
    //////////////////////////////// ------------------------             PASO 1               ----------------------------------------------
    

    /* TRAER PERFIL DE USUARIO*/

    public void LoadProfile()

    {

        show_hair = true;
        show_head = true;
        show_body = true;


        WearablesComplete = 0;

        if (!loadingProfile)
        {
            loadingProfile = true;
            hashUsser = inputHashUser.text;


            /*DESTRUYE OBJETOS POR SI HABIA*/
            foreach (Transform hijo in ParentDCLLoaders)
            {
                Destroy(hijo.gameObject);
            }


            /* PRENDE EL FBX TEMPORAL Y DESTRUYE SUS HIJOS*/
            fbx.SetActive(true);
            foreach (Transform hijo in fbx_meshes_container.transform)
            {
                Destroy(hijo.gameObject);
            }


            /* PRENDE EL VRM FINAL Y DESTRUYE SUS HIJOS*/
            foreach (Transform hijo in vrm_meshes_container.transform)
            {
                Destroy(hijo.gameObject);
            }

            /*este es el que reimporta falso , el que no ayuda a cargar los glbs en vrm*/
            GameObject VRMObjectOld = GameObject.Find("VRM");
            if(VRMObjectOld!= null)
            {
                Destroy(VRMObjectOld);
            }

            /*ahora si empieza el get del usuario de decentraland**/
            StartCoroutine(GetProfile(hashUsser)); 
        }
    }


    public void CompleteProfileAvatar()
    {
        loadingProfile = false;
    }




    /// <summary>
    /// Fetch user profile from decentraland catalyst 
    /// </summary>
    /// <param name="address">hash user</param>
    /// <returns></returns>
    public IEnumerator GetProfile(string address)
    {
        WearableListProfiles.Clear();
        UnityWebRequest www = UnityWebRequest.Get( API_DCL_PROFILE + address);
        yield return www.SendWebRequest();
        if (www.result == UnityWebRequest.Result.Success)
        {           
           // string responseText = www.downloadHandler.text;
            //var jsonString = System.Text.Encoding.UTF8.GetString(www.downloadHandler.data, 3,www.downloadHandler.data.Length - 3);

            dataProfile = JSON.Parse(System.Text.Encoding.UTF8.GetString(www.downloadHandler.data));

            dataProfileAvatar = dataProfile["avatars"][0]["avatar"];
            dataProfileAvatarWearables = dataProfile["avatars"][0]["avatar"]["wearables"];


            Debug.Log(dataProfileAvatar["bodyShape"]);

            /*guardando la info del avatar*/


            /* PRIMERO EL GENERO */
            string dataProfileAvatarBodyShape = dataProfileAvatar["bodyShape"];

            if (dataProfileAvatarBodyShape.Contains("Male"))
            {
                Debug.Log("El string contiene 'Male'");
                avatarInfo.genero = AvatarGenero.Male;
                avatarInfo.generoStr = "Male" ;
            }
            if (dataProfileAvatarBodyShape.Contains("Female"))
            {
                Debug.Log("El string contiene 'Female'");
                avatarInfo.genero = AvatarGenero.Female;
                avatarInfo.generoStr = "Female";
            }


            /*CreateColorByRGB*/

            //Dictionary<string, object> colorData = JsonConvert.DeserializeObject<Dictionary<string, object>>(dataProfileAvatar["hair"]["color"]);
            avatarInfo.hair = CreateColorByRGB("hair");
            avatarInfo.eyes = CreateColorByRGB("eyes");
            avatarInfo.skin = CreateColorByRGB("skin");





            GetListOfWearablesAndCheck();
        }
        else
        {
            Debug.Log(www.error);

            /*USUARIO NO EXISTE*/
        }

    }



   /* sacando los colres necesarios del data*/
    Color CreateColorByRGB(string tipo)
    {
        var colorData = dataProfileAvatar[tipo]["color"];      
        float r = (float)(double)colorData["r"].AsDouble;
        float g = (float)(double)colorData["g"].AsDouble;
        float b = (float)(double)colorData["b"].AsDouble;
        float a = (float)(double)colorData["a"].AsDouble;   
        return  new Color(r, g, b,a);       

    }


    /*si el wearable sera omitido o no*/

    #region CheckWearableString
    private bool CheckWearableString(string wearableProfile)
    {
        string textobuscado = "base-avatars";

        //a omitir
        //https://docs.decentraland.org/contributor/content/entity-types/wearables/
        //eyes, eyebrows, facial_hair, mouth, mask

        //Debug.Log("<color=yellow> buscando </color> " + wearableProfile);       
        if (wearableProfile.Contains(textobuscado))
        {
            // Debug.Log("La cadena buscada se encuentra en el texto");
            return true;//"La cadena buscada se encuentra en el texto"
        }
        else
        {
            // Debug.Log("La cadena buscada no se encuentra en el texto");
            return false;
        }
    }
    #endregion CheckWearableString




    #region GetListOfWearablesAndCheck
    public void GetListOfWearablesAndCheck()
    {
        // API_DCL_WEARABLE_POINTER
        string wearableCurrent = "";

        for (int i = 0; i < dataProfileAvatarWearables.Count; i++)
        {
            wearableCurrent = dataProfileAvatarWearables[i];
            //leyendo los wearables elegibles
            if (!CheckWearableString (wearableCurrent))
            {
                Debug.Log(wearableCurrent);
                WearablesEncontrado newWearable = new WearablesEncontrado();
                newWearable.wearablePointer = wearableCurrent;              
                WearableListProfiles.Add(newWearable);                            
            }
        }


        Quaternion rotat = new Quaternion(0, 0, 0, 0);
        for (int i = 0; i < WearableListProfiles.Count; i++)
        {
           //WearableListProfiles[i].WearableContent = Instantiate(PrefabDCLLoader, Vector3.zero, rotat, ParentDCLLoaders);
            WearableListProfiles[i].WearableContent = Instantiate(PrefabDCLLoader, Vector3.zero, rotat, ParentDCLLoaders);
            //WearableListProfiles[i].WearableContent.transform.parent = ParentDCLLoaders;
            WearableListProfiles[i].GetWearablePointer = GetWearablePointer(WearableListProfiles[i].wearablePointer , i);
            StartCoroutine(WearableListProfiles[i].GetWearablePointer);
        }

    }
    #endregion




    #region GetWearablePointer
    public IEnumerator GetWearablePointer(string wearablePointer, int indexOnList)
    {
        UnityWebRequest www = UnityWebRequest.Get(API_DCL_WEARABLE_POINTER + wearablePointer);

        yield return www.SendWebRequest();
        if (www.result == UnityWebRequest.Result.Success)
        {

            string responseText = www.downloadHandler.text;
            //var jsonString = System.Text.Encoding.UTF8.GetString( www.downloadHandler.data, 3, www.downloadHandler.data.Length - 3);
            dataWearablePointer = JSON.Parse(System.Text.Encoding.UTF8.GetString(www.downloadHandler.data));



            /////https://peer.decentraland.org/content/entities/wearables/?pointer=urn:decentraland:matic:collections-v2:0x7a80e0b9992d1a5d2a10082d30ed653b9ac061f0:0
            /**data == metadata.data ***/
            /**tags == metadata.tags ***/
            /**replaces == metadata.replaces ***/
            /**hides == metadata.hides ***/
            /**category == metadata.category ***/


            /**representations == metadata.representations ***/
            /**bodyShapes == metadata.representations.bodyShapes ***/  //genero buscar el genero del avatar
            /**mainFile == metadata.representations.mainFile ***/  //mainFile para buscarlo en el array de  metadata.content.file y sacar el hash
            /**overrideHides == metadata.representations.overrideHides ***/ //
            /**overrideReplaces == metadata.representations.overrideReplaces ***/




            JSONNode metadataWearableCurrent =  dataWearablePointer[0]["metadata"]["data"];          

            JSONNode representationsWearableCurrent = metadataWearableCurrent["representations"];

          

            /**buscando el correspondiente al genero y saca su mainfile*/
            string mainFile = "";
            string bodyShapeTmp = "";
            for (int i = 0; i < representationsWearableCurrent.Count; i++)
            {
                bodyShapeTmp = representationsWearableCurrent[i]["bodyShapes"][0];                
                if (bodyShapeTmp.Contains(avatarInfo.generoStr))
                {
                    //encontro la representacion basada en el genero del avatar male-female y saca su mainfile 
                    mainFile = representationsWearableCurrent[i]["mainFile"];
                    break;
                }
            }

            /**ya que encontro el mainfile ahora busca el hash de ese mainfile en la lista de content **/


            Debug.Log("mainFile " + mainFile);
            string hashWearable = FindHashWearableByFile (mainFile);
          




            //Debug.Log("_hash:: "+ hashWearable);
            //Debug.Break();





            ///aui ya se obtiene el hash del avatar

           // string hashWearable = FindHashWearable(dataWearablePointer);



            //setea el objeto que contiene el wearable en si
            DCL_GLBLoader glbLoaderObj = WearableListProfiles[indexOnList].WearableContent.transform.GetComponent<DCL_GLBLoader>();

           
            glbLoaderObj.info_replaces = metadataWearableCurrent["replaces"];
            glbLoaderObj.info_hides = metadataWearableCurrent["hides"];
            glbLoaderObj.info_tags = metadataWearableCurrent["tags"];
            glbLoaderObj.info_category = metadataWearableCurrent["category"];



            /*checando los hide de cabeza y o pelo*/
            string hideCurrent = "";

            for (int i = 0; i < metadataWearableCurrent["hides"].Count; i++)
            {
                hideCurrent = metadataWearableCurrent["hides"][i];
                if (hideCurrent ==  "head")
                {
                    Debug.Log("<color=yellow>Debe ocultar la cabeza</color>");
                    show_head = false;
                }

                if (hideCurrent == "hair")
                {
                    Debug.Log("<color=yellow>Debe ocultar el pelo</color>");
                    show_hair = false;
                }
            }
            //ese glb
            if (mainFile.Contains(".glb"))
            {
                glbLoaderObj.idWearable = hashWearable;
                glbLoaderObj.StartGLBLoader();
            }
            else
            {
                glbLoaderObj.idWearable = hashWearable;
                glbLoaderObj.StartGLBLoaderFinished();
            }

            //
        }
        else
        {
            Debug.Log(www.error);
            /*WEARABLE POINER NO EXISTE*/
        }

    }

    #endregion




    #region FindHashWearable
    public string FindHashWearable(JSONNode _dataWearablePointer)
    {
        string hashToReturn = "";       
        string fileName = "";
        string[] fileNameSplip;
        for (int i = 0; i < dataWearablePointer[0]["content"].Count; i++)
        {
            fileName = dataWearablePointer[0]["content"][i]["file"];
            fileNameSplip = fileName.Split(".");
            if (fileNameSplip[fileNameSplip.Length-1] == "glb" ) {
                Debug.Log(dataWearablePointer[0]["content"][i]["hash"]);
                hashToReturn = dataWearablePointer[0]["content"][i]["hash"];
                break;
            }
        }
       
        Debug.Log("_________________________________");
        return hashToReturn;
    }
    #endregion

    public string FindHashWearableByFile(string mainfile)
    {
        string hashToReturn = "";
        string fileName = "";        
        for (int i = 0; i < dataWearablePointer[0]["content"].Count; i++)
        {
            fileName = dataWearablePointer[0]["content"][i]["file"];            
            if (fileName == mainfile)
            {
                Debug.Log(dataWearablePointer[0]["content"][i]["hash"]);
                hashToReturn = dataWearablePointer[0]["content"][i]["hash"];
                break;
            }
        }

        Debug.Log("_________________________________");
        return hashToReturn;
    }


    public VRMRuntimeExporter vRMRuntimeExporter;
    private int WearablesComplete = 0;
    public void ExportVRM()
    {
        WearablesComplete++;
        if (WearablesComplete >= WearableListProfiles.Count)
        {
            vRMRuntimeExporter.Exportando();

            
        }
    }





    async void VRMLoadComplete()
    {       
    
        // Esperar a que se encuentre el objeto
        GameObject vrmLoaded = await BuscarObjetoPorNombre("VRM");

      


        //Transform vrm_hips = vrmLoaded.transform.Find("hips");

        GameObject vrm_armature = await BuscarObjetoPorNombre(vrmLoaded, "Armature");
        GameObject vrm_hips = await BuscarObjetoPorNombre(vrm_armature, "hips");


        Debug.Log("VRM LOADED COMPLETE-----");
        

        //paso todos los meshe que trae el vrm

        SkinnedMeshRenderer[] meshRenderer = vrmLoaded.GetComponentsInChildren<SkinnedMeshRenderer>();
        for (int i = 0; i < meshRenderer.Length; i++)
        {
            updateSkinnedMesh.InicioUpdateBonesVRMtoVRM(meshRenderer[i].transform, meshRenderer[i], vrm_hips.transform);
        }




        /*colorizando */

        

        SkinnedMeshRenderer[] meshRendererFull = vrm_full.GetComponentsInChildren<SkinnedMeshRenderer>();

        
        for (int j = 0; j < meshRendererFull.Length; j++)
        {
            Material[] materialsInMeshCurrent = meshRendererFull[j].materials;
            //Debug.Log(meshRendererFull[j].name, meshRendererFull[j]);

            for (int i = 0; i < materialsInMeshCurrent.Length; i++)
            {
                if (materialsInMeshCurrent[i].name.Contains ("AvatarSkin_MAT"))
                {
                    // Cambiar el color del material
                    materialsInMeshCurrent[i].color = avatarInfo.skin;

                }

                 if (materialsInMeshCurrent[i].name.Contains("Hair_MAT"))
                {
                    // Cambiar el color del material
                    materialsInMeshCurrent[i].color = avatarInfo.hair;
                }



                //materialsInMeshCurrent[i].SetFloat("_Mode", 2f);

                //// Establecer el valor de Cutoff para definir el umbral de transparencia
                //float cutoffValue = 0.5f; // por ejemplo, un valor de 0.5f para un umbral de transparencia del 50%
                //materialsInMeshCurrent[i].SetFloat("_Cutoff", cutoffValue);



                // materialsInMeshCurrent[i].SetOverrideTag("RenderType", "Transparent");







                /******/

                //materialsInMeshCurrent[i].SetInt("_SrcBlend", (int)meshRenderer[i].material.GetFloat("_SrcBlend"));
                //materialsInMeshCurrent[i].SetInt("_DstBlend", (int)meshRenderer[i].material.GetFloat("_DstBlend"));
                //materialsInMeshCurrent[i].SetInt("_ZWrite", (int)meshRenderer[i].material.GetFloat("_ZWrite"));


                //materialsInMeshCurrent[i].SetFloat("_Mode", (float)meshRenderer[i].material.GetFloat("_Mode"));


                //int renderType = meshRenderer[i].material.renderQueue;

                //materialsInMeshCurrent[i].renderQueue = renderType;

              

                //if (renderType <= 2000)
                //{

                //    materialsInMeshCurrent[i].SetInt("_ZWrite", 1);
                //    materialsInMeshCurrent[i].DisableKeyword("_ALPHATEST_ON");
                //    materialsInMeshCurrent[i].DisableKeyword("_ALPHABLEND_ON");
                //    materialsInMeshCurrent[i].DisableKeyword("_ALPHAPREMULTIPLY_ON");
                //    materialsInMeshCurrent[i].renderQueue = -1;
                //}
                //else if (renderType <= 2450)


                //{
                //    materialsInMeshCurrent[i].SetInt("_ZWrite", 1);
                //    materialsInMeshCurrent[i].EnableKeyword("_ALPHATEST_ON");
                //    materialsInMeshCurrent[i].DisableKeyword("_ALPHABLEND_ON");
                //    materialsInMeshCurrent[i].DisableKeyword("_ALPHAPREMULTIPLY_ON");
                //    materialsInMeshCurrent[i].renderQueue = 2450;
                //    materialsInMeshCurrent[i].SetOverrideTag("RenderType", "TransparentCutout");

                //}


                //else if (renderType <= 3000)


                //{
                //    materialsInMeshCurrent[i].SetInt("_ZWrite", 0);
                //    materialsInMeshCurrent[i].DisableKeyword("_ALPHATEST_ON");
                //    materialsInMeshCurrent[i].EnableKeyword("_ALPHABLEND_ON");
                //    materialsInMeshCurrent[i].DisableKeyword("_ALPHAPREMULTIPLY_ON");
                //    materialsInMeshCurrent[i].renderQueue = 3000;
                //    materialsInMeshCurrent[i].SetOverrideTag("RenderType", "Transparent");

                //}


                //else


                //{
                //    materialsInMeshCurrent[i].SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                //    materialsInMeshCurrent[i].SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                //    materialsInMeshCurrent[i].SetInt("_ZWrite", 0);
                //    materialsInMeshCurrent[i].DisableKeyword("_ALPHATEST_ON");
                //    materialsInMeshCurrent[i].DisableKeyword("_ALPHABLEND_ON");
                //    materialsInMeshCurrent[i].EnableKeyword("_ALPHAPREMULTIPLY_ON");
                //    materialsInMeshCurrent[i].renderQueue = 3000;
                //    materialsInMeshCurrent[i].SetOverrideTag("RenderType", "Transparent");

                //}







                //materialsInMeshCurrent[i].SetFloat("_Metallic", (float)meshRenderer[i].material.GetFloat("_Metallic"));


                /*****/



















            }

            // Asignar los materiales modificados al SkinnedMeshRenderer
            meshRendererFull[j].materials = materialsInMeshCurrent;
        }






        // Verificar si el objeto fue encontrado
        if (vrmLoaded != null)
        {
            Debug.Log("Se encontró el objeto: " + vrmLoaded.name);
        }
        else
        {
            Debug.Log("No se encontró el objeto");
        }


        ActivandoMeshes();

        CompleteProfileAvatar();


    }

    async Task<GameObject> BuscarObjetoPorNombre(string nombreObjeto)
    {
        // Buscar el objeto por nombre en un bucle
        while (true)
        {
            GameObject objetoEncontrado = GameObject.Find(nombreObjeto);
            if (objetoEncontrado != null)
            {
                return objetoEncontrado;
            }
            await Task.Yield();
        }
    }


    async Task<GameObject> BuscarObjetoPorNombre(GameObject contenedor, string nombreObjeto)
    {
        // Buscar el objeto por nombre en un bucle
        while (true)
        {
            Transform objetoEncontrado = contenedor.transform.Find(nombreObjeto);
            if (objetoEncontrado != null)
            {
                return objetoEncontrado.gameObject;
            }
            await Task.Yield();
        }
    }



    public void ExportVRMFnct()
    {
        vRMRuntimeExporter.ExportandoUser();    


    }


    public void ActivandoMeshes()
    {
        //activando meshes en el vrm

        vrm_hair_male.enabled = false;
        vrm_hair_female.enabled = false;

        if (avatarInfo.genero == AvatarGenero.Male)
        {
            vrm_hair_male.enabled = show_hair;
        }

        if (avatarInfo.genero == AvatarGenero.Female)
        {
            vrm_hair_female.enabled = show_hair;
        }

        vrm_head.enabled = show_hair;

    }



}
