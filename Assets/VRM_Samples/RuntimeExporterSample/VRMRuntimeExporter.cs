using System.IO;
using UnityEngine;
using VRMShaders;
using System.Threading.Tasks;
using UnityEngine.Events;

namespace VRM.RuntimeExporterSample
{

    public class VRMRuntimeExporter : MonoBehaviour
    {
        [SerializeField]
        public bool UseNormalize = true;

        public GameObject m_model;

        public GameObject m_model_import;



        public UnityEvent OnCompleteLoadVRM;


        public GameObject vrmToExport;


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
            ExportUser(vrmToExport, UseNormalize);

            
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




    }
}
