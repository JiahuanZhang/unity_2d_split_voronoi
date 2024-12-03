using System.Collections.Generic;
using UnityEngine;

public class Cell
{
    public long instanceID;
    public List<EdgeV> edges;
    public HashSet<long> edgeIds;
    public List<long> vertexIds;
    public List<TriangleIndex> triangles;
    public Cell(List<EdgeV> initEdges, List<long> vertexs)
    {
        edges = initEdges;
        vertexIds = vertexs;
        edgeIds = new HashSet<long>();
        foreach (var edge in edges) edgeIds.Add(edge.InstanceID);
        instanceID = edges[0].InstanceID;
        triangles = SplitTriangles();
    }

    private List<TriangleIndex> SplitTriangles()
    {
        var triangles = new List<TriangleIndex>();
        var vlist = new List<int>();
        for (int i = 0; i < vertexIds.Count; i++) vlist.Add(i);
        while (vlist.Count >= 3)
        {
            var t = new TriangleIndex();
            t.x = vlist[0];
            t.y = vlist[1];
            t.z = vlist[vlist.Count - 1];
            triangles.Add(t);
            vlist.RemoveAt(0);
        }
        return triangles;
    }
}

public class CellVertex
{
    public long instanceID;
    public Vector2 Pos;
    public List<EdgeV> edges = new();
    public HashSet<long> edgeIds = new();

    public CellVertex(long id, Vector2 pos)
    {
        instanceID = id;
        this.Pos = pos;
    }
    public void AddEdge(EdgeV edge)
    {
        if (!edgeIds.Contains(edge.InstanceID))
        {
            edges.Add(edge);
            edgeIds.Add(edge.InstanceID);
        }
    }

    public void SortEdge()
    {
        edges.Sort((a, b) =>
        {
            if (GetEdageAngle(a) >= GetEdageAngle(b))
                return 1;
            else
                return -1;
        });
    }

    public (EdgeV, float, float) GetEdgeAngleBiggerThan(float angle, List<long> exceptedEdges, bool loop = false)
    {
        EdgeV tempEdge = null;
        float tempTrueAngle = 0, tempCheckAngle = -1;
        foreach (var edge in edges)
        {
            if (exceptedEdges.Contains(edge.InstanceID)) continue;
            var trueAngle = GetEdageAngle(edge);
            var checkAngle = trueAngle;
            if (angle > 180 && checkAngle < 180)
            {
                checkAngle += 360;
            }

            if (checkAngle >= angle)
            {
                if (tempEdge == null || trueAngle < tempCheckAngle)
                {
                    tempEdge = edge;
                    tempTrueAngle = trueAngle;
                    tempCheckAngle = checkAngle;
                    continue;
                }
            }
        }
        //if (tempEdge == null && loop == false)
        //{
        //    GetEdgeAngleBiggerThan(angle, exceptedEdges, true);
        //}
        return (tempEdge, tempTrueAngle, tempCheckAngle);
    }
    public float GetEdageAngle(EdgeV edgedd)
    {
        if (edgedd.triangleIdA == this.instanceID)
            return edgedd.angle;
        else
            return edgedd.angle + 180;
    }
}


public struct TriangleIndex
{
    public int x, y, z;
}
