using System.IO;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(VoronoiModifier))]
public class VoronoiModifierEditor : Editor
{
    private string meshDataName;
    bool modifyMesh, modifyVertex;
    public override void OnInspectorGUI()
    {
        var vc = (VoronoiModifier)target;
        modifyMesh = vc.ModifyMesh;
        modifyVertex = vc.ModifyVertex;
        base.OnInspectorGUI();

        if (modifyMesh != vc.ModifyMesh)
        {
            modifyMesh = vc.ModifyMesh;
            modifyVertex = !modifyMesh;
        }
        else if (modifyVertex != vc.ModifyVertex)
        {
            modifyVertex = vc.ModifyVertex;
            modifyMesh = !modifyVertex;
        }
        vc.ModifyMesh = modifyMesh;
        vc.ModifyVertex = modifyVertex;

        GUILayout.Space(10);
        if (GUILayout.Button("Save Mesh"))
        {
            VoronoiModifier.Instance.SaveMeshInEditor();
        }
    }

}
