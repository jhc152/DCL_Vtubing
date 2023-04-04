using UnityEngine;
using Sirenix.OdinInspector;

public class CombineSkinnedMeshes : MonoBehaviour
{
    public SkinnedMeshRenderer mesh1;
    public SkinnedMeshRenderer mesh2;

    public SkinnedMeshRenderer skinedMEshBones;


    [Button("Combina meshes")]
    public void CombninaMEs()
    {
        // Create a new SkinnedMeshRenderer to hold the combined mesh
        SkinnedMeshRenderer combinedMesh = gameObject.AddComponent<SkinnedMeshRenderer>();
        // Obtén los bones del sourceRenderer
        Transform[] bones = skinedMEshBones.bones;

        // Crea un nuevo array de bones para el SkinnedMeshRenderer del objeto vacío
        Transform[] newBones = new Transform[bones.Length];

        // Copia los bones del sourceRenderer al nuevo array
        for (int i = 0; i < bones.Length; i++)
        {
            newBones[i] = bones[i];
        }







        // Asigna los nuevos bones al SkinnedMeshRenderer del objeto vacío
        combinedMesh.bones = newBones;

        // Combine the meshes into one
        Mesh combined = new Mesh();
        mesh1.BakeMesh(combined);
        //mesh2.BakeMesh(combined);
        combinedMesh.sharedMesh = combined;


        // Copiar los pesos de los huesos y las poses de los huesos del SkinnedMeshRenderer de origen
        combined.boneWeights = skinedMEshBones.sharedMesh.boneWeights;
        combined.bindposes = skinedMEshBones.sharedMesh.bindposes;


        // dondeGuaradr.sharedMesh = combined;

        // Set the materials for the combined mesh
        Material[] materials = new Material[mesh1.sharedMaterials.Length ];
        mesh1.sharedMaterials.CopyTo(materials, 0);
        //mesh2.sharedMaterials.CopyTo(materials, mesh1.sharedMaterials.Length);
        combinedMesh.sharedMaterials = materials;


        // dondeGuaradr.sharedMaterials = materials;


        

       
    }
}