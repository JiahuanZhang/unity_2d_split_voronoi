using System.Collections.Generic;
using UnityEngine;

public class Demo2 : MonoBehaviour
{

    public Material foodMat, shadowMat;
    public TextAsset meshData;


    private List<Demo1Clip> foodClips = new();
    private List<Demo1Clip> foodShadowClips = new();

    void Start()
    {
        if (meshData != null)
        {
            var data = JsonUtility.FromJson(meshData.text, typeof(MeshGroupData)) as MeshGroupData;
            CreateMeshes(data);
        }
    }

    public void CreateMeshes(MeshGroupData data)
    {
        for (int i = 0; i < data.ChipDatas.Length; i++)
        {
            var chipData = data.ChipDatas[i];
            var mesh = new Mesh();

            mesh.SetVertices(chipData.Vertices);
            mesh.SetUVs(0, chipData.Uvs);
            mesh.SetTriangles(chipData.Triangles, 0);
            mesh.RecalculateBounds();
            mesh.RecalculateNormals();

            //
            var go = new GameObject($"chip_" + i);
            go.transform.parent = transform;
            var rd = go.AddComponent<MeshRenderer>();
            var mf = go.AddComponent<MeshFilter>();
            rd.sharedMaterial = foodMat;
            mf.mesh = mesh;
            var clip = go.AddComponent<Demo1Clip>();
            foodClips.Add(clip);

            var shadowGo = GameObject.Instantiate(go);
            shadowGo.name = $"clip_{i}_s";
            shadowGo.transform.parent = transform;
            shadowGo.transform.localPosition += new Vector3(0, 0, 0.1f);
            shadowGo.GetComponent<MeshRenderer>().material = shadowMat;
            var shadowClip = shadowGo.GetComponent<Demo1Clip>();
            foodShadowClips.Add(shadowClip);
        }

    }

    int index = 0;
    float time = 0;
    private void Update()
    {
        time += Time.deltaTime;
        if (time >= 0.5f)
        {
            time = 0;
            index++;
        }

        for (int i = 0; i < index; i++)
        {
            foodClips[i].UpdatePos2(Time.deltaTime);
        }
        for (int i = 0; i < index - 10; i++)
        {
            foodShadowClips[i].UpdatePos2(Time.deltaTime);
        }
    }
}