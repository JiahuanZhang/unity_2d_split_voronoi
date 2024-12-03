using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class MeshGroupData
{
    public string Texture;
    public int Seed;
    public Vector2 Uvs;
    public Vector2 MeshSize;
    public int PointCountX, PointCountY;
    public MeshChipData[] ChipDatas;
}

[Serializable]
public class MeshChipData
{
    [NonSerialized]
    public long InstanceID;
    [NonSerialized]
    public Vector2 Center;

    public Vector3[] Vertices;
    public Vector2[] Uvs;
    public int[] Triangles;


    public MeshChipData(long id) { InstanceID = id; }
}
