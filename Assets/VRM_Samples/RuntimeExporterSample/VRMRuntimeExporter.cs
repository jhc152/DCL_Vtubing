using System.IO;
using UnityEngine;
using VRMShaders;
using System.Threading.Tasks;
using UnityEngine.Events;

using System.Collections;
using System.Collections.Generic;

namespace VRM.RuntimeExporterSample
{

    [System.Serializable]
    public struct VRMMesehesObjects
    {
        public SkinnedMeshRenderer vrm_hair_male;
        public SkinnedMeshRenderer vrm_lower_body_male;
        public SkinnedMeshRenderer vrm_upper_body_male;
        public SkinnedMeshRenderer vrm_feet_male;

        public SkinnedMeshRenderer vrm_hair_female;
        public SkinnedMeshRenderer vrm_lower_body_female;
        public SkinnedMeshRenderer vrm_upper_body_female;
        public SkinnedMeshRenderer vrm_feet_female;
        public SkinnedMeshRenderer vrm_head;

        public GameObject wearablesParent;
        public GameObject vrm_hips;
    }




    public class VRMRuntimeExporter : MonoBehaviour
    {
        [SerializeField]
        public bool UseNormalize = true;
        public GameObject m_model;
        public GameObject m_model_import;
        public UnityEvent OnCompleteLoadVRM;

        public GameObject vrmVtubing;
        public GameObject vrmToExport;

        //public UpdateSkinnedMeshBones updateSkinnedMesh;

        public VRMMesehesObjects vrmsVtubing;
        public VRMMesehesObjects vrmsToExport;


        void OnGUI()
        {
            //if (GUILayout.Button("Cargame un vrm"))
            //{
            //    Load();
            //}

            //GUI.enabled = m_model != null;

            //if (GUILayout.Button("Add custom blend shape"))
            //{
            //    AddBlendShapeClip(m_model);
            //}

            //if (GUILayout.Button("Exportame un vrm"))
            //{
            //    Export(m_model, UseNormalize);
            //}
        }


        public void Exportando()
        {
            Export(m_model, UseNormalize);
            LoadVRMFaik();
        }
        public void ExportandoUser()
        {
            StartCoroutine(SetMeshesBase());

            //ExportUser(vrmToExport, UseNormalize);
        }




        async void Load()
        {
#if UNITY_STANDALONE_WIN
            var path = FileDialogForWindows.FileDialog("abir VRM", ".vrm");
#elif UNITY_EDITOR
            var path = UnityEditor.EditorUtility.OpenFilePanel("Open VRM", "", "vrm");
#else
        var path = Application.dataPath + "/default.vrm";
#endif
            if (string.IsNullOrEmpty(path))
            {
                return;
            }

            var loaded = await VrmUtility.LoadAsync(path);
            loaded.ShowMeshes();
            loaded.EnableUpdateWhenOffscreen();

            if (m_model != null)
            {
                GameObject.Destroy(m_model.gameObject);
            }

            m_model = loaded.gameObject;
        }






        private async void LoadVRMFaik()
        {           
            var path = Application.dataPath + "/Export/export.vrm";
            while (!File.Exists(path))
            {
                Debug.Log("Esperando archivo...");
                await Task.Delay(1000); // Esperar 1 segundo antes de verificar de nuevo
            }

            Debug.Log("El archivo existe.");                    

            var loaded = await VrmUtility.LoadAsync(path);
            loaded.ShowMeshes();
            loaded.EnableUpdateWhenOffscreen();

            if (m_model_import != null)
            {
                GameObject.Destroy(m_model_import.gameObject);
            }
            m_model.gameObject.SetActive(false);
            m_model_import = loaded.gameObject;
            //carga completa del vrm creado
            OnCompleteLoadVRM?.Invoke();
        }



        static void AddBlendShapeClip(GameObject go)
        {
            // get or create blendshape proxy
            var proxy = go.GetComponent<VRMBlendShapeProxy>();
            if (proxy == null)
            {
                proxy = go.AddComponent<VRMBlendShapeProxy>();
            }

            // get or create blendshapeavatar
            var avatar = proxy.BlendShapeAvatar;
            if (avatar == null)
            {
                avatar = ScriptableObject.CreateInstance<BlendShapeAvatar>();
                proxy.BlendShapeAvatar = avatar;
            }

            // add blendshape clip to avatar.Clips
            var clip = ScriptableObject.CreateInstance<BlendShapeClip>();
            var name = $"custom#{avatar.Clips.Count}";
            Debug.Log($"Add {name}");
            // unity asset name
            clip.name = name;
            // vrm export name
            clip.BlendShapeName = name;
            clip.Preset = BlendShapePreset.Unknown;

            clip.IsBinary = false;
            clip.Values = new BlendShapeBinding[]
            {
                new BlendShapeBinding
                {
                    RelativePath = "mesh/face", // target Renderer relative path from root 
                    Index = 0, // BlendShapeIndex in SkinnedMeshRenderer
                    Weight = 75f // BlendShape weight, range is [0-100]
                },
            };
            clip.MaterialValues = new MaterialValueBinding[]
            {
                new MaterialValueBinding
                {
                    MaterialName = "Alicia_body", // target_material_name
                    ValueName = "_Color", // target_material_property_name,
                    BaseValue = new Vector4(1, 1, 1, 1), // Target value when the Weight value of BlendShapeClip is 0
                    TargetValue = new Vector4(0, 0, 0, 1), // Target value when the Weight value of BlendShapeClip is 1
                },
            };
            avatar.Clips.Add(clip);

            // done
        }


        static void Export(GameObject model, bool useNormalize)
        {



            string folderPath = Application.dataPath + "/Export"; // Ruta de la carpeta que deseas crear

            if (!Directory.Exists(folderPath)) // Verificar si la carpeta ya existe
            {
                Directory.CreateDirectory(folderPath); // Crear la carpeta si no existe
                Debug.Log("La carpeta se creó correctamente.");
            }
            else
            {
                Debug.Log("La carpeta ya existe.");
            }




            //#if UNITY_STANDALONE_WIN
#if false
        var path = FileDialogForWindows.SaveDialog("save VRM", Application.dataPath + "/export.vrm");
#else
            var path = Application.dataPath + "/Export/export.vrm";
#endif
            if (string.IsNullOrEmpty(path))
            {
                return;
            }

            var bytes = useNormalize ? ExportCustom(model) : ExportSimple(model);

            File.WriteAllBytes(path, bytes);
            Debug.LogFormat("export to {0}", path);
        }

        static byte[] ExportSimple(GameObject model)
        {
            var vrm = VRMExporter.Export(new UniGLTF.GltfExportSettings(), model, new RuntimeTextureSerializer());
            var bytes = vrm.ToGlbBytes();
            return bytes;
        }

        static byte[] ExportCustom(GameObject exportRoot, bool forceTPose = false)
        {
            // normalize
            var target = VRMBoneNormalizer.Execute(exportRoot, forceTPose);

            try
            {
                return ExportSimple(target);
            }
            finally
            {
                // cleanup
                GameObject.Destroy(target);
            }
        }

        void OnExported(UniGLTF.glTF vrm)
        {
            Debug.LogFormat("exported");
        }



        static void ExportUser(GameObject model, bool useNormalize)
        {
            var path = FileDialogForWindows.SaveDialog("save VRM", Application.dataPath + "/DCL_vrm.vrm");
            if (string.IsNullOrEmpty(path))
            {
                return;
            }

            var bytes = useNormalize ? ExportCustom(model) : ExportSimple(model);
            File.WriteAllBytes(path, bytes);
            Debug.LogFormat("export to {0}", path);
        }






        public IEnumerator  SetMeshesBase()
        {

            foreach (Transform hijo in vrmsToExport.wearablesParent.transform)
            {
                Destroy(hijo.gameObject);
            }


            yield return new WaitForSeconds(.5f);

            vrmsToExport.vrm_hair_male.enabled = vrmsVtubing.vrm_hair_male.enabled;
            vrmsToExport.vrm_lower_body_male.enabled = vrmsVtubing.vrm_lower_body_male.enabled;
            vrmsToExport.vrm_upper_body_male.enabled = vrmsVtubing.vrm_upper_body_male.enabled;
            vrmsToExport.vrm_feet_male.enabled = vrmsVtubing.vrm_feet_male.enabled;

            vrmsToExport.vrm_hair_female.enabled = vrmsVtubing.vrm_hair_female.enabled;
            vrmsToExport.vrm_lower_body_female.enabled = vrmsVtubing.vrm_lower_body_female.enabled;
            vrmsToExport.vrm_upper_body_female.enabled = vrmsVtubing.vrm_upper_body_female.enabled;
            vrmsToExport.vrm_feet_female.enabled = vrmsVtubing.vrm_feet_female.enabled;
            vrmsToExport.vrm_head.enabled = vrmsVtubing.vrm_head.enabled;

            if (vrmsVtubing.wearablesParent != null && vrmsToExport.wearablesParent != null)
            {
                // Copia el GameObject fuente y sus hijos recursivamente
                GameObject newObject = CopyObjectRecursively(vrmsVtubing.wearablesParent, vrmsToExport.wearablesParent.transform);
                newObject.gameObject.layer = LayerMask.NameToLayer(newLayer);
            }

            yield return new WaitForSeconds(.1f);


            SkinnedMeshRenderer[] meshRenderer = vrmsToExport.wearablesParent.GetComponentsInChildren<SkinnedMeshRenderer>();
            for (int i = 0; i < meshRenderer.Length; i++)
            {
                InicioUpdateBonesVRMtoVRM(meshRenderer[i].transform, meshRenderer[i], vrmsToExport.vrm_hips.transform);

                meshRenderer[i].gameObject.layer = LayerMask.NameToLayer(newLayer);
            }

            yield return new WaitForSeconds(.5f);

            ExportUser(vrmToExport, UseNormalize);
        }


         string newLayer = "NoShowToCamera";
        GameObject CopyObjectRecursively(GameObject source, Transform parent)
        {
            // Crea una copia del GameObject fuente y establece su padre en el GameObject de destino
            GameObject newObject = Instantiate(source, parent);
            


            // Copia los componentes del GameObject fuente al nuevo GameObject
            Component[] components = source.GetComponents<Component>();
            foreach (Component component in components)
            {
                // Si el componente no es el Transform, se copia al nuevo GameObject
                if (!(component is Transform))
                {
                    //Component newComponent = newObject.AddComponent(component.GetType());
                    //UnityEditorInternal.ComponentUtility.CopyComponent(component);
                    //UnityEditorInternal.ComponentUtility.PasteComponentValues(newComponent);
                    CopyComponent(component , newObject);

                }
            }

            // Recorre los hijos del GameObject fuente y los copia al nuevo GameObject de manera recursiva
            for (int i = 0; i < source.transform.childCount; i++)
            {
                GameObject childObject = source.transform.GetChild(i).gameObject;
                CopyObjectRecursively(childObject, newObject.transform);
            }

            return newObject;
        }



        public void CopyComponent(Component source, GameObject target)
        {
            System.Type type = source.GetType();
            var newComponent = target.AddComponent(type);
            var fields = type.GetFields();

            foreach (var field in fields)
            {
                field.SetValue(newComponent, field.GetValue(source));
            }

            var sourceFields = source.GetType().GetFields();
            var targetFields = fields;

            foreach (var sourceField in sourceFields)
            {
                foreach (var targetField in targetFields)
                {
                    if (sourceField.Name == targetField.Name && sourceField.FieldType == targetField.FieldType)
                    {
                        targetField.SetValue(target, sourceField.GetValue(source));
                        break;
                    }
                }
            }

        }



        public void InicioUpdateBonesVRMtoVRM(Transform _skinnedTransormCurrent, SkinnedMeshRenderer _targetSkin, Transform _rootBone)
        {
            _UpdateBones(_rootBone, _targetSkin);
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
            Transform[] existingBones = vrmsToExport.vrm_hips.GetComponentsInChildren<Transform>(false);
            Transform[] existingBonesOriginales = _rootBoneOriginales.GetComponentsInChildren<Transform>(false);
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

                boneName = ReturnEquparableName(boneName);
                    
                   
                



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




        public List<NamesBones> namesBones = new List<NamesBones>();




        #region ReturnEquparableName
        private string ReturnEquparableName(string boneCurrent)
        {
            string nameToReturn = boneCurrent;
            foreach (var item in namesBones)
            {
                if (item.nameDCL == boneCurrent)
                {
                    nameToReturn = item.nameVRM;
                    return nameToReturn;
                }
            }

            return nameToReturn;
        }
        #endregion




        [System.Serializable]
        public struct NamesBones
        {
            public int i;
            //name bone on glb DCL
            public string nameDCL;
            //name bone on VRM
            public string nameVRM;
        }






    }


















}
