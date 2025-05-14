using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

[Serializable]
public class MeshGroupData
{
    public string Texture;
    public int Seed;
    public Vector2 Uvs;
    public Vector2 MeshSize;
    public int PointCountX, PointCountY;
    public List<MeshChipData> ChipDatas;

    public MeshChipData CombineMeshes(long[] ids)
    {
        var mc = ChipDatas.Find(e => e.InstanceID == ids[0]);
        for (var i = 1; i < ids.Length; i++)
        {
            var temp = ChipDatas.Find(e => e.InstanceID == ids[i]);
            if (temp == null)
            {
                throw new Exception($"error cannot find ChipData:{ids[i]}");
            }
            ChipDatas.Remove(temp);
            mc.Combine(temp);
        }
        return mc;
    }

    public Dictionary<long, MeshChipSortData> SortByDir(Vector3 dir)
    {
        Vector3 targetAxis = dir.normalized; // 新的Y轴方向
        var rotation = Quaternion.FromToRotation(targetAxis, Vector3.up);

        var formatPos = new List<MeshChipSortData>(ChipDatas.Count);
        var tempList = new List<Vector3>();
        foreach (var chip in ChipDatas)
        {
            tempList.Clear();
            foreach (var v in chip.Vertices)
            {
                Vector3 newPos = rotation * v;
                tempList.Add(newPos);
            }
            tempList.Sort((a, b) =>
            {
                if (a.y < b.y) return -1;
                else if (a.y > b.y) return 1;
                else return 0;
            });
            var sortData = new MeshChipSortData();
            sortData.InstanceID = chip.InstanceID;
            sortData.SortVertex = tempList[0];
            formatPos.Add(sortData);
        }
        formatPos.Sort((a, b) =>
        {
            if (a.SortVertex.y < b.SortVertex.y) return -1;
            else if (a.SortVertex.y > b.SortVertex.y) return 1;
            else return 0;
        });

        var priorityDic = new Dictionary<long, MeshChipSortData>(formatPos.Count);
        for (int i = 0; i < formatPos.Count; i++)
        {
            var tempData = formatPos[i];
            tempData.SortIndex = i;
            priorityDic[tempData.InstanceID] = tempData;
        }
        ChipDatas.Sort((a, b) =>
        {
            var ap = priorityDic[a.InstanceID];
            var bp = priorityDic[b.InstanceID];
            if (ap.SortIndex < bp.SortIndex) return -1;
            else if (ap.SortIndex > bp.SortIndex) return 1;
            else return 0;
        });
        return priorityDic;
    }
}

[Serializable]
public class MeshChipData
{
    public long InstanceID;

    public Vector3[] Vertices;
    public Vector3 Center;
    public Vector2[] Uvs;
    public int[] Triangles;


    public MeshChipData(long id) { InstanceID = id; }

    public Vector3[] GetVerticesInCenter()
    {
        var vx = new Vector3[Vertices.Length];
        for (int i = 0; i < Vertices.Length; i++)
        {
            vx[i] = Vertices[i] - Center;
        }
        return vx;
    }

    public void Combine(MeshChipData data)
    {
        var newVertices = new List<Vector3>(Vertices);
        var newUvs = new List<Vector2>(Uvs);
        var newTriangles = new List<int>(Triangles);
        var indexDic = new Dictionary<int, int>();
        var tempCenter = Center * newVertices.Count;
        for (int i = 0; i < data.Vertices.Length; i++)
        {
            var index = newVertices.IndexOf(data.Vertices[i]);
            if (index >= 0)
            {
                indexDic.Add(i, index);
            }
            else
            {
                newVertices.Add(data.Vertices[i]);
                newUvs.Add(data.Uvs[i]);
                tempCenter += data.Vertices[i];
                indexDic.Add(i, newVertices.Count - 1);
            }
        }
        for (int i = 0; i < data.Triangles.Length; i++)
        {
            newTriangles.Add(indexDic[data.Triangles[i]]);
        }

        Vertices = newVertices.ToArray();
        Uvs = newUvs.ToArray();
        Triangles = newTriangles.ToArray();
        Center = tempCenter / newVertices.Count;
    }

    private List<EdgeIndex> CachedEdgeIndex;
    public List<EdgeIndex> GetOutLineEdge()
    {
        if (CachedEdgeIndex == null)
        {
            var edges = new List<EdgeIndex>();
            for (int i = 0; i < Triangles.Length; i += 3)
            {
                var triangle0 = Triangles[i];
                var triangle1 = Triangles[i + 1];
                var triangle2 = Triangles[i + 2];

                AddEdge(triangle0, triangle1);
                AddEdge(triangle1, triangle2);
                AddEdge(triangle2, triangle0);
            }
            edges.FiliterInSelf(e => e.Count == 0);
            CachedEdgeIndex = edges;

            void AddEdge(int x, int y)
            {
                var index0 = edges.FindIndex(e => e.Equal(x, y));
                if (index0 > 0)
                    edges[index0] = edges[index0].AddRef();
                else
                    edges.Add(new EdgeIndex(x, y));
            }
        }

        return CachedEdgeIndex;
    }
    public struct EdgeIndex
    {
        public int X, Y, Count;
        public EdgeIndex(int x, int y) { X = x; Y = y; Count = 0; }
        public bool Equal(EdgeIndex other)
        {
            return (other.X == X && other.Y == Y) || (other.X == Y && other.Y == X);
        }
        public bool Equal(int x, int y)
        {
            return (x == X && y == Y) || (x == Y && y == X);
        }
        public EdgeIndex AddRef()
        {
            Count++;
            return this;
        }
    }
}


public struct MeshChipSortData
{
    public long InstanceID;
    //public Vector3[] Vertices;
    public Vector3 SortVertex;
    public int SortIndex;
}
