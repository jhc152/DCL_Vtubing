using System.Collections.Generic;
using System.IO;
using UnityEngine;
using VRM;

public class VRMExporter : MonoBehaviour
{
    public string VRMFilePath;
    public Texture2D texture;

    public void ExportVRM()
    {
        //// Obtener referencias a los componentes necesarios
        //VRMExporterSettings exporterSettings = ScriptableObject.CreateInstance<VRMExporterSettings>();
        //VRMExportSettings exportSettings = ScriptableObject.CreateInstance<VRMExportSettings>();
        //VRMHumanoidDescription humanoidDescription = gameObject.GetComponent<VRMHumanoidDescription>();
        //VRMBlendShapeProxy blendShapeProxy = gameObject.GetComponent<VRMBlendShapeProxy>();

        //// Configurar las opciones de exportación
        //exporterSettings.migrateToVrm0_0 = true;

        //// Agregar la textura como subobjeto en el paquete VRM
        //List<Texture2D> textures = new List<Texture2D>();
        //List<string> textureNames = new List<string>();
        //textures.Add(texture);
        //textureNames.Add(texture.name);

        //// Exportar el paquete VRM
        //byte[] bytes = VRMExporter.Export(gameObject, exporterSettings, exportSettings, humanoidDescription, blendShapeProxy, textures.ToArray(), textureNames.ToArray());
        //File.WriteAllBytes(VRMFilePath, bytes);
    }
}
