using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

[System.Serializable]
public struct NamesBones
{
    public int i;
    //name bone on glb DCL
    public string nameDCL;
    //name bone on VRM
    public string nameVRM;
}

public class UpdateSkinnedMeshBones : MonoBehaviour
{

    public SkinnedMeshRenderer targetSkin;
    public Transform rootBone;
    public GameObject fbxParent;
    public GameObject fbxParent_meshes_container;

    public Transform rootBoneOriginales;
    public bool includeInactive;
    public bool dclTVrM = false;
    public bool inverse = false;   

    public List <NamesBones> namesBones = new List<NamesBones>();

    //array de los skinnedMeshRenderers
    SkinnedMeshRenderer[] skinnedMeshRenderers;


    public void InicioUpdateBonesComplete()
    {
        //cuando no hay glb por cargar
        DCL_Manager.Instance.ExportVRM();
    }

    public void InicioUpdateBones()
    {
        //identificando el bone aasignaodo al principal de la escan
        rootBone  = DCL_Manager.Instance.rootBoneFBX;
        fbxParent = DCL_Manager.Instance.fbx;
        fbxParent_meshes_container = DCL_Manager.Instance.fbx_meshes_container;

        SkinnedMeshRenderer[] skinnedMeshRenderers = gameObject.GetComponentsInChildren<SkinnedMeshRenderer>();
        Transform skinnedTransormCurrent;

        for (int i = 0; i < skinnedMeshRenderers.Length; i++)
        {            
            _UpdateBones(skinnedMeshRenderers[i].rootBone, skinnedMeshRenderers[i]);

            //Verifica que su parent no tenga SkinnedMeshRenderer para moverlo de parent
            skinnedTransormCurrent = skinnedMeshRenderers[i].transform;
            if (skinnedTransormCurrent.parent.GetComponent< SkinnedMeshRenderer>()== null)
            {
                skinnedTransormCurrent.parent = fbxParent_meshes_container.transform;
            }
        }


        DCL_Manager.Instance.ExportVRM();

    }




    public void InicioUpdateBonesVRMtoVRM(Transform _skinnedTransormCurrent, SkinnedMeshRenderer _targetSkin,  Transform _rootBone)
    {

        Debug.Log("_targetSkin " + _targetSkin.name, _targetSkin.gameObject);
        Debug.Log("_rootBone " + _rootBone.name, _rootBone.gameObject);
      
        _UpdateBones(_rootBone, _targetSkin);

        if (_skinnedTransormCurrent.parent.GetComponent<SkinnedMeshRenderer>() == null)
        {
            _skinnedTransormCurrent.parent = fbxParent_meshes_container.transform;
        }


    }


    public void _UpdateBones(Transform _rootBoneOriginales, SkinnedMeshRenderer _targetSkin)
    {
        //Look for root bone
        string rootName = "";
        if (_targetSkin.rootBone != null)
        {
            rootName = _targetSkin.rootBone.name;
        }
        Transform newRoot = null;

        // Reassing new bones
        Transform[] newBones = new Transform[_targetSkin.bones.Length];
        Transform[] existingBones = rootBone.GetComponentsInChildren<Transform>(includeInactive);
        Transform[] existingBonesOriginales = _rootBoneOriginales.GetComponentsInChildren<Transform>(includeInactive);
        int missingBones = 0;
        for (int i = 0; i < _targetSkin.bones.Length; i++)
        {
            if (_targetSkin.bones[i] == null)
            {
                //statusText += System.Environment.NewLine + "";
                Debug.Log("WARN: Do not delete the old bones before the skinned mesh is processed!");
                missingBones++;
                continue;
            }

            //name of bone current
            string boneName = _targetSkin.bones[i].name;
           // Debug.Log("finding " + boneName);

            if (dclTVrM)
            {
                if (!inverse)
                {
                    boneName = ReturnEquparableName(boneName);
                }
                else
                {
                    boneName = ReturnEquparableNameiNVERSE(boneName);
                }
            }



            bool found = false;
            foreach (var newBone in existingBones)
            {
                // Debug.Log("rootName ");
                if (newBone.name == rootName)
                {
                    newRoot = newBone;
                }
                if (newBone.name == boneName)
                {
                   // Debug.Log("<color=green> ENCONTRO VRM </color> " + newBone.name + " found!");
                    newBones[i] = newBone;
                   // Debug.Log("i : " + newBones[i].rotation.eulerAngles);
                    found = true;
                }

            }//end foreach

            if (!found)
            {
                Debug.Log("<color=yellow> ----- </color> " + boneName + " missing");
                foreach (var newBone in existingBonesOriginales)
                {
                    // Debug.Log("rootName ");
                    if (newBone.name == rootName)
                    {
                        newRoot = newBone;
                    }
                    if (newBone.name == boneName)
                    {
                        Debug.Log("<color=cyan> ----- </color> " + newBone.name + " found!");
                        newBones[i] = newBone;
                        found = true;
                    }

                }//end foreach

                if (!found)
                {
                    Debug.Log(boneName + "  aun SIN ENCONTRAR");
                    missingBones++;
                }

            }
        }

        _targetSkin.bones = newBones;
      //  Debug.Log("Done! Missing bones: " + missingBones);
        if (newRoot != null)
        {
            Debug.Log("· Setting " + rootName + " as root bone.");
            _targetSkin.rootBone = newRoot;
        }
    }



    [Button("Update Bones")]
    public void UpdateBones()
    {
        _UpdateBones(rootBoneOriginales, targetSkin);
    }



    #region ReturnEquparableName
    private string ReturnEquparableName(string boneCurrent)
    {
        string nameToReturn = boneCurrent;
        foreach (var item in namesBones)
        {
            if(item.nameDCL == boneCurrent)
            {
                nameToReturn = item.nameVRM;
                return nameToReturn;
            }
        }

        return nameToReturn;
    }
    #endregion

    #region ReturnEquparableNameiNVERSE
    private string ReturnEquparableNameiNVERSE(string boneCurrent)
    {
        string nameToReturn = "";
        foreach (var item in namesBones)
        {
            if (item.nameVRM == boneCurrent)
            {
                nameToReturn = item.nameDCL;
                return nameToReturn;
            }
        }

        return nameToReturn;
    }
    #endregion

    #region ShowBones

    [Button("Show Bones")]
    public void ShowBones()
    {
        Transform[] newBones = new Transform[targetSkin.bones.Length];
        for (int i = 0; i < targetSkin.bones.Length; i++)
        {
            Debug.Log(targetSkin.bones[i].name);
        }
    }

    #endregion






}
