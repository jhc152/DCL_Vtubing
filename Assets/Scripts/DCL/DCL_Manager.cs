
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
    public SkinnedMeshRenderer vrm_lower_body_male;
    public SkinnedMeshRenderer vrm_upper_body_male;
    public SkinnedMeshRenderer vrm_feet_male;
   
    public SkinnedMeshRenderer vrm_hair_female;
    public SkinnedMeshRenderer vrm_lower_body_female;
    public SkinnedMeshRenderer vrm_upper_body_female;
    public SkinnedMeshRenderer vrm_feet_female;
    public SkinnedMeshRenderer vrm_head;





    bool show_hair = false;
    bool show_head = false;
    bool show_body = false;

    bool show_lower_body = false; 
    bool show_upper_body = false;
    bool show_feet = false;

    //contabilizar cuantos wearables se han cargado su data
    int WearablesDataCompletes = 0;



    List<string> hideFull = new List<string>();
    List<string> replaceFull = new List<string>();

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
        //resete paraemeters
        show_hair = true;
        show_head = true;
        show_body = true;

        show_lower_body = true;
        show_upper_body = true;
        show_feet = true;

        hideFull = new List<string>();
        replaceFull = new List<string>();


        WearablesComplete = 0;
        WearablesDataCompletes = 0;

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


            /*reiniciando coroutinas*/
            if(_GetProfile == null)
            {
                _GetProfile = GetProfile(hashUsser);
            }
            else
            {
                StopCoroutine(_GetProfile);
                _GetProfile = null;
                _GetProfile = GetProfile(hashUsser);
            }

            StartCoroutine(_GetProfile);
        }


        //protect to fail reload
        else
        {
            if (_GetProfile != null)
            {
                StopCoroutine(_GetProfile);
                 _GetProfile = null;
            }


            for (int i = 0; i < WearableListProfiles.Count; i++)
            {
                if (WearableListProfiles[i].GetWearablePointer != null)
                {
                    StopCoroutine(WearableListProfiles[i].GetWearablePointer);
                }
            }

            loadingProfile = false;
            LoadProfile();

        }
    }


    public void CompleteProfileAvatar()
    {
        loadingProfile = false;
    }


    IEnumerator _GetProfile =  null;

    /// <summary>
    /// Fetch user profile from decentraland catalyst 
    /// </summary>
    /// <param name="address">hash user</param>
    /// <returns></returns>
    public IEnumerator GetProfile(string address)
    {
        WearableListProfiles.Clear();



        WearableListProfiles = new List<WearablesEncontrado>();


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

            ///colorizando hair eyes skin
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

        //string[] textobuscadoParaOmitir = { "hair", "head", "eyes", "eyebrows", "facial_hair", "mouth", "upper_body", "lower_body", "feet", "hat", "helmet", "mask", "tiara", "top_head", "earring", "eyewear.skin" };
        string[] textobuscadoParaOmitir = { "eyes", "eyebrows", "facial_hair", "mouth", "mask" };

        //a omitir
        //https://docs.decentraland.org/contributor/content/entity-types/wearables/
        //eyes, eyebrows, facial_hair, mouth, mask

        //Debug.Log("<color=yellow> buscando </color> " + wearableProfile);       
        if (wearableProfile.Contains(textobuscado))
        {
            // Debug.Log("La cadena buscada se encuentra en el texto");
            ///off-chain
            bool contiene = false;
            foreach (string texto in textobuscadoParaOmitir)
            {
                if (wearableProfile.Contains(texto))
                {
                    contiene = true;
                    break;
                }
            }                      


            return contiene;//"La cadena buscada se encuentra en el texto"
            //return true;//"La cadena buscada se encuentra en el texto"
        }
        else
        {
            // Debug.Log("La cadena buscada no se encuentra en el texto");
            return false;
        }
    }
    #endregion CheckWearableString


    public List<WearablesEncontrado> WearableToRemove = new List<WearablesEncontrado>();

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
                Debug.Log(API_DCL_WEARABLE_POINTER+wearableCurrent);
                WearablesEncontrado newWearable = new WearablesEncontrado();
                newWearable.wearablePointer = wearableCurrent;              
                WearableListProfiles.Add(newWearable);                            
            }
        }


        /*analizar cuales seran eliminados de la lista anates de ser cargados**/

        WearableToRemove = new List<WearablesEncontrado>();
        for (int i = 0; i < WearableListProfiles.Count; i++)
        {

        }


       

        /*instanciar lo wearabls que si pasaron a ser cargados*/

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


  


    /// <summary>
    /// 
    /// </summary>
    public void CleanListWearables()
    {
        //for (int i = 0; i < WearableListProfiles.Count; i++)
        //{

        //}


        //foreach (WearablesEncontrado obj in WearableListProfiles)
        //{
        //    if (obj.transform.position.y < 0)
        //    {
        //        objectsToClean.Add(obj);
        //        WearableListProfiles.Remove(obj);
        //    }
        //}
    }

    List<string> skinHides = new List<string> { "upper_body", "feet", "lower_body", "head", "helmet", "mask", "facial_hair", "mouth", "eyes", "eyewear", "earrings" };

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




            JSONNode metadataWearableCurrent = dataWearablePointer[0]["metadata"]["data"];
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


            ///aqui ya se obtiene el hash del avatar
            string hashWearable = FindHashWearableByFile(mainFile);


            ///aqui ya se obtiene el hash del avatar

            // string hashWearable = FindHashWearable(dataWearablePointer);

            //setea el objeto que contiene el wearable en si
            DCL_GLBLoader glbLoaderObj = WearableListProfiles[indexOnList].WearableContent.transform.GetComponent<DCL_GLBLoader>();
            glbLoaderObj.info_replaces = metadataWearableCurrent["replaces"];
            glbLoaderObj.info_hides = metadataWearableCurrent["hides"];
            glbLoaderObj.info_tags = metadataWearableCurrent["tags"];
            glbLoaderObj.info_category = metadataWearableCurrent["category"];


            Debug.Log("<color=green>info_category :  </color>" + glbLoaderObj.info_category);

            for (int i = 0; i < metadataWearableCurrent["hides"].Count; i++)
            {
                Debug.Log(i + " H: " + metadataWearableCurrent["hides"][i]);
            }

            for (int i = 0; i < metadataWearableCurrent["replaces"].Count; i++)
            {
                Debug.Log(i + " R: " + metadataWearableCurrent["replaces"][i]);
            }

            string category_cur = metadataWearableCurrent["category"];

            if (category_cur == "skin")
            {
                hideFull.AddRange(skinHides);
            }

            glbLoaderObj.mainFile = mainFile;
            glbLoaderObj.idWearable = hashWearable;

            /*checando los hide de cabeza y o pelo*/
            string hideCurrent = "";



            glbLoaderObj.wearableBase = false;


            string tagCurr = "";
            for (int i = 0; i < metadataWearableCurrent["tags"].Count; i++) {
                tagCurr = metadataWearableCurrent["tags"][i];
                if(tagCurr == "base-wearable")
                {
                    glbLoaderObj.wearableBase = true;
                }
            }


            //verifica si la categoria es wearable base

            //for (int i = 0; i < metadataWearableCurrent["hides"].Count; i++)
            //{
            //    hideCurrent = metadataWearableCurrent["hides"][i];
            //    //Debug.Log("h___ "+ hideCurrent);

            //    hideFull.Add(hideCurrent);
            //    if (hideCurrent ==  "head")
            //    {
            //        Debug.Log("<color=yellow>Debe ocultar la cabeza</color>");
            //        show_head = false;
            //    }

            //    if (hideCurrent == "hair")
            //    {
            //        Debug.Log("<color=yellow>Debe ocultar el pelo</color>");
            //        show_hair = false;
            //    }
            //}



                //for (int i = 0; i < metadataWearableCurrent["replaces"].Count; i++)
                //{
                //    hideCurrent = metadataWearableCurrent["replaces"][i];
                //    //Debug.Log("r___ " + hideCurrent);

                //    replaceFull.Add(hideCurrent);
                //    if (hideCurrent == "head")
                //    {
                //        Debug.Log("<color=yellow>Debe ocultar la cabeza</color>");
                //        show_head = false;
                //    }

                //    if (hideCurrent == "hair")
                //    {
                //        Debug.Log("<color=yellow>Debe ocultar el pelo</color>");
                //        show_hair = false;
                //    }
                //}


                //analizar wearables por caragar    
                //es glb buscar el hash para la textura
                if (mainFile.Contains(".glb") || mainFile.Contains(".gltf"))
            {
                Debug.Log("<color=yellow>mainFile::</color>" + mainFile); 
                glbLoaderObj.hashTexture =  FindHashTextureWearableByFile();
            }           

            CheckWearablesHides();
        }
        else
        {
            Debug.Log(www.error);
            /*WEARABLE POINER NO EXISTE*/
        }

    }




    /// <summary>
    /// analizar los hides y replaces
    /// </summary>
    public void CheckWearablesHides()
    {

        WearablesDataCompletes++;
        if (WearablesDataCompletes < WearableListProfiles.Count) return; //si aun no termina de cargar todod los datas

        replaceFull = replaceFull.Distinct().ToList();
        hideFull = hideFull.Distinct().ToList();

        DCL_GLBLoader glbLoaderObj = null;
        string categoryCurr = "";
        List<WearablesEncontrado> wearableToRemove = new List<WearablesEncontrado>();

        //buscar lo objetos que seran hide y quitarlos de la lista original para no cargara realmente el glb
        foreach (var hide in hideFull)
        {
            Debug.Log("<color=cyan>hideFull</color> " + hide);
            foreach (WearablesEncontrado obj in WearableListProfiles)
            {
                glbLoaderObj = obj.WearableContent.transform.GetComponent<DCL_GLBLoader>();
                categoryCurr = glbLoaderObj.info_category;  
                if (categoryCurr == hide)
                {                   
                    wearableToRemove.Add(obj);
                }
            }
        }
        
        WearableListProfiles.RemoveAll(obj => wearableToRemove.Contains(obj));

        //ahora si con los finales cargarle su glb
        string mainFileCurr = "";
        string hideCurr ="";
        string replaceCurr = "";
        foreach (WearablesEncontrado obj in WearableListProfiles)
        {
            glbLoaderObj = obj.WearableContent.transform.GetComponent<DCL_GLBLoader>();
            mainFileCurr = glbLoaderObj.mainFile;
            categoryCurr = glbLoaderObj.info_category;
            //ese glb
            if (mainFileCurr.Contains(".glb") )
            //if (mainFileCurr.Contains(".glb") || mainFileCurr.Contains(".gltf"))
            {
                //ocultando feet
                if (categoryCurr == "feet")  show_feet = false;               
                if (categoryCurr == "lower_body") show_lower_body = false;
                if (categoryCurr == "upper_body") show_upper_body = false;

                //checando su hides 
                for (int i = 0; i < glbLoaderObj.info_hides.Count; i++)
                {
                    hideCurr = glbLoaderObj.info_hides[i];
                    if (hideCurr == "feet") show_feet = false;
                    if (hideCurr == "lower_body") show_lower_body = false;
                    if (hideCurr == "upper_body") show_upper_body = false;
                    if (hideCurr == "head") show_head = false;
                    if (hideCurr == "hair") show_hair = false;
                    
                }

                for (int i = 0; i < glbLoaderObj.info_replaces.Count; i++)
                {
                    replaceCurr = glbLoaderObj.info_replaces[i];
                    if (replaceCurr == "feet") show_feet = false;
                    if (replaceCurr == "lower_body") show_lower_body = false;
                    if (replaceCurr == "upper_body") show_upper_body = false;
                    if (replaceCurr == "head") show_head = false;
                    if (replaceCurr == "hair") show_hair = false;
                }


                if(glbLoaderObj.wearableBase && categoryCurr== "hair")
                {
                    show_hair = false;
                }

                glbLoaderObj.StartGLBLoader();
            }
            else
            {
                //glbLoaderObj.idWearable = hashWearable;
                glbLoaderObj.StartGLBLoaderFinished();
            }

        }

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



    public string FindHashTextureWearableByFile()
    {
        string hashToReturn = "";
        string fileName = "";
        for (int i = 0; i < dataWearablePointer[0]["content"].Count; i++)
        {
            fileName = dataWearablePointer[0]["content"][i]["file"];
            if (!fileName.Contains("thumbnail") && !fileName.Contains("glb") && !fileName.Contains("gltf"))
            {
                Debug.Log("<color=red>file::</color> " + dataWearablePointer[0]["content"][i]["file"]);
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
                //if (materialsInMeshCurrent[i].name.Contains ("AvatarSkin_MAT"))
                if (materialsInMeshCurrent[i].name.Contains ("AvatarSkin"))
                {
                    // Cambiar el color del material
                    materialsInMeshCurrent[i].color = avatarInfo.skin;

                }

                // if (materialsInMeshCurrent[i].name.Contains("Hair_MAT") )
                 if (materialsInMeshCurrent[i].name.Contains("Hair") )
                {
                    // Cambiar el color del material
                    materialsInMeshCurrent[i].color = avatarInfo.hair;
                }



                //materialsInMeshCurrent[i].SetFloat("_Mode", 2f);

                //// Establecer el valor de Cutoff para definir el umbral de transparencia
                //float cutoffValue = 0.5f; // por ejemplo, un valor de 0.5f para un umbral de transparencia del 50%
                //materialsInMeshCurrent[i].SetFloat("_Cutoff", cutoffValue);



                 materialsInMeshCurrent[i].SetOverrideTag("RenderType", "Cutout");






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


        vrm_lower_body_male.enabled = false;
        vrm_upper_body_male.enabled = false;
        vrm_feet_male.enabled = false;

        vrm_hair_female.enabled = false;
        vrm_lower_body_female.enabled = false;
        vrm_upper_body_female.enabled = false;
        vrm_feet_female.enabled = false;


        Debug.Log("show_hair "+  show_hair + " "+ avatarInfo.genero);

        if (avatarInfo.genero == AvatarGenero.Male)
        {
            vrm_hair_male.enabled = show_hair;

            vrm_lower_body_male.enabled = show_lower_body;
            vrm_upper_body_male.enabled = show_upper_body;
            vrm_feet_male.enabled = show_feet;
        }

        if (avatarInfo.genero == AvatarGenero.Female)
        {
            vrm_hair_female.enabled = show_hair;

            vrm_lower_body_female.enabled = show_lower_body;
            vrm_upper_body_female.enabled = show_upper_body;
            vrm_feet_female.enabled = show_feet;
        }

        vrm_head.enabled = show_head;




    }



}
