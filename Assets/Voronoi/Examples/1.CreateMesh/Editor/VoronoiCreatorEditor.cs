using System.IO;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(VoronoiCreator))]
public class VoronoiCreatorEditor : Editor
{
    private string SaveKey = "VoronioSaveKey";
    private string DefaultPath = "Assets/Meshes/";


    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        var vc = (VoronoiCreator)target;

        GUILayout.Space(10);
        if (GUILayout.Button("Save Mesh"))
        {
            if (Application.isPlaying)
            {
                var folder = EditorPrefs.GetString(SaveKey, DefaultPath);
                if (!Directory.Exists(folder)) { Directory.CreateDirectory(folder); }

                var data = vc.meshGroupData;
                var json = JsonUtility.ToJson(data);
                var path = folder + $"mesh_{data.Texture}_{data.Seed}.json";
                using (var sw = File.CreateText(path))
                {
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
    }
}
