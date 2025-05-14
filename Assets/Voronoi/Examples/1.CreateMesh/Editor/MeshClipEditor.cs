using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

[CanEditMultipleObjects]
[CustomEditor(typeof(MeshClip))]
public class MeshClipEditor : Editor
{

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        GUILayout.Space(10);
        if (GUILayout.Button("Delete Mesh"))
        {
            foreach (var t in targets)
            {
                var mc = t as MeshClip;
                mc.gameObject.SetActive(false);
                if (VoronoiCreator.Instance != null)
                    VoronoiCreator.Instance.DelteClip(mc.InstanceID);
                if (VoronoiModifier.Instance != null)
                    VoronoiModifier.Instance?.DelteClip(mc.InstanceID);
            }
        }
        if (targets.Length > 1)
        {
            if (GUILayout.Button("Combine Meshes"))
            {
                var ids = new long[targets.Length];
                for (int i = 0; i < targets.Length; i++)
                {
                    var temp = targets[i] as MeshClip;
                    ids[i] = temp.InstanceID;
                }
                if (VoronoiCreator.Instance != null)
                    VoronoiCreator.Instance.CombinMeshes(ids);
                if (VoronoiModifier.Instance != null)
                    VoronoiModifier.Instance.CombinMeshes(ids);
            }
        }
    }

}
