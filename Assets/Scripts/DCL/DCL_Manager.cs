
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

[Serializable]
public class WearablesEncontrado
{
    public string wearablePointer;
    [SerializeField]
    public IEnumerator GetWearablePointer;

    public GameObject WearableContent;

    //public JSONNode dataWearablePoint;

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

    private string genero = "Male";

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

    //el objeto interno dentro del vrm donde se almacenaran los meshes importados
    public GameObject vrm_meshes_container;


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






    public void LoadProfile()
    {
        WearablesComplete = 0;

        if (!loadingProfile)
        {
            loadingProfile = true;
            hashUsser = inputHashUser.text;

            foreach (Transform hijo in ParentDCLLoaders)
            {
                Destroy(hijo.gameObject);
            }


            fbx.SetActive(true);

            foreach (Transform hijo in fbx_meshes_container.transform)
            {
                Destroy(hijo.gameObject);
            }
            


            foreach (Transform hijo in vrm_meshes_container.transform)
            {
                Destroy(hijo.gameObject);
            }


            


            GameObject VRMObjectOld = GameObject.Find("VRM");
            if(VRMObjectOld!= null)
            {
                Destroy(VRMObjectOld);
            }


            StartCoroutine(GetProfile(hashUsser)); 
        }
    }


    public void CompleteProfileAvatar()
    {
        loadingProfile = false;
    }




    /// <summary>
    /// Fetch user profile from decentraland catalys 
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
            string responseText = www.downloadHandler.text;
            var jsonString = System.Text.Encoding.UTF8.GetString(
                               www.downloadHandler.data,
                               3,
                               www.downloadHandler.data.Length - 3);

            dataProfile = JSON.Parse(System.Text.Encoding.UTF8.GetString(www.downloadHandler.data));

            dataProfileAvatar = dataProfile["avatars"][0]["avatar"];
            dataProfileAvatarWearables = dataProfile["avatars"][0]["avatar"]["wearables"];

            //Debug.Log(dataProfile["avatars"][0][2]);
            //Debug.Log(dataProfileAvatarWearables);
            //Debug.Log(dataProfileAvatarWearables.Count);
            GetListOfWearablesAndCheck();
        }
        else
        {
            Debug.Log(www.error);

            /*USUARIO NO EXISTE*/
        }

    }


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
            var jsonString = System.Text.Encoding.UTF8.GetString( www.downloadHandler.data, 3, www.downloadHandler.data.Length - 3);
            dataWearablePointer = JSON.Parse(System.Text.Encoding.UTF8.GetString(www.downloadHandler.data));

            string hashWearable = FindHashWearable(dataWearablePointer);
            //setea el objeto que contiene el wearable en si
            DCL_GLBLoader glbLoaderObj = WearableListProfiles[indexOnList].WearableContent.transform.GetComponent<DCL_GLBLoader>();
            glbLoaderObj.idWearable = hashWearable;
            glbLoaderObj.StartGLBLoader();
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


        Debug.Log("asdasdasdasd-----");
        

        //paso todos los meshe que trae el vrm

        SkinnedMeshRenderer[] meshRenderer = vrmLoaded.GetComponentsInChildren<SkinnedMeshRenderer>();
        for (int i = 0; i < meshRenderer.Length; i++)
        {
            updateSkinnedMesh.InicioUpdateBonesVRMtoVRM(meshRenderer[i].transform, meshRenderer[i], vrm_hips.transform);
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



}
