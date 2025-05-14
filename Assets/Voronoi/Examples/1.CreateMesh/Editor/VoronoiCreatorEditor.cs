using System.IO;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(VoronoiCreator))]
public class VoronoiCreatorEditor : Editor
{
    private string SaveKey = "VoronioSaveKey";
    private string DefaultPath = "Assets/Meshes/";

    private string meshDataName;
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        var vc = (VoronoiCreator)target;

        GUILayout.Space(10);
        if (GUILayout.Button("Save Mesh"))
        {
            SaveMesh(vc);
        }
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label("Save Mesh Folder:", GUILayout.Width(120));
            var oldStr = EditorPrefs.GetString(SaveKey, DefaultPath);
            var newStr = GUILayout.TextField(oldStr);
            if (newStr != oldStr)
            {
                EditorPrefs.SetString(SaveKey, newStr);
            }
            GUILayout.EndHorizontal();
        }
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label("Mesh Name:", GUILayout.Width(120));
            meshDataName = GUILayout.TextField(meshDataName);
            GUILayout.EndHorizontal();
        }
    }

    void SaveMesh(VoronoiCreator vc)
    {

        if (Application.isPlaying)
        {
            var folder = EditorPrefs.GetString(SaveKey, DefaultPath);
            if (!folder.EndsWith("/") && !folder.EndsWith("\\\\")) folder += "/";
            if (!Directory.Exists(folder)) { Directory.CreateDirectory(folder); }

            var file = meshDataName;
            if (string.IsNullOrEmpty(meshDataName)) file = vc.textures[0].name + ".json";
            var path = folder + file;
            using (var sw = File.CreateText(path))
            {
                var data = vc.meshGroupData;
                var json = JsonUtility.ToJson(data);
                sw.Write(json);
                sw.Flush();
            }
            AssetDatabase.Refresh();
        }
        else
        {
            Debug.LogError("Can Only Save In Playing！！");
        }
    }
}
