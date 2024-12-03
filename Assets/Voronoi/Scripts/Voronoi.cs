using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
 
public class Voronoi
{
    public List<Vector2> featurePoints;
    public List<Edge> trianglesEdgeList;
    public List<EdgeV> voronoiEdges;
    public List<Triangle> allTriangles;

    public Dictionary<long, CellVertex> vertexDic;
    public Dictionary<long, Cell> cellDic = new();
    public Dictionary<long, Cell> errorcellDic = new();

    //初始超级三角形的顶点
    private Vector2 pointA;
    private Vector2 pointB;
    private Vector2 pointC;


    public void StartGenerate(List<Vector2> points)
    {
        featurePoints = points;
        voronoiEdges = new List<EdgeV>();
        trianglesEdgeList = new List<Edge>();
        allTriangles = new List<Triangle>();
        CreateDelaunay();
        CreateVoronoi();
        CreateCells();
    }

    private void CreateDelaunay()
    {
        float minX, maxX, minY, maxY, dx, dy, deltaMax, midX, midY;
        minX = featurePoints[0].x;
        minY = featurePoints[0].y;
        maxX = minX;
        maxY = minY;
        
        for (int i = 0; i < featurePoints.Count; i++)
        {
            if (featurePoints[i].x < minX) minX = featurePoints[i].x;
            if (featurePoints[i].y < minY) minY = featurePoints[i].y;
            if (featurePoints[i].x > maxX) maxX = featurePoints[i].x;
            if (featurePoints[i].y > maxY) maxY = featurePoints[i].y;
        }
        
        dx = maxX - minX;
        dy = maxY - minY;
        deltaMax = Mathf.Max(dx, dy);
        midX = (minX + maxX) / 2;
        midY = (minY + maxY) / 2;
        pointA = new Vector2(midX - 20 * deltaMax, midY - 20 * deltaMax);
        pointB = new Vector2(midX, midY + 20 * deltaMax);
        pointC = new Vector2(midX + 20 * deltaMax, midY - 20 * deltaMax);
        var tri = new Triangle(pointA, pointB, pointC);
        
        allTriangles.Add(tri);
        SetDelaunayTriangle(allTriangles, featurePoints);
        returnEdgesofTriangleList(allTriangles, out trianglesEdgeList);
    }

    private void CreateVoronoi()
    {
        var result = SetVoronoi(allTriangles);
        voronoiEdges = result.Item1;
        vertexDic = result.Item2;
    }

    private void CreateCells()
    {
        cellDic.Clear();
        foreach (var kv in vertexDic)
        {
            long id = kv.Key;
            CellVertex v = kv.Value;

            var firstEdge = v.edges[0];
            var firstAngle = v.GetEdageAngle(firstEdge);
            if (firstAngle > 180) continue;

            var edgeList = new List<(float, EdgeV)>() { (firstAngle, firstEdge) };
            var edgeIDList = new List<long>() { firstEdge.InstanceID };
            var vertexIdList = new List<long>() { firstEdge.triangleIdA };
            EdgeV curEdge = firstEdge;
            long startVertexID = curEdge.triangleIdA, endVertexID = curEdge.triangleIdB;
            float curAngle = v.GetEdageAngle(curEdge);
            bool fail = false, noLegal = false;
            while (startVertexID != endVertexID)
            {
                var vertex = vertexDic[endVertexID];
                var (nextEdge, tempTrueAngle, tempCheckAngle) = vertex.GetEdgeAngleBiggerThan(curAngle, edgeIDList);
                if (tempCheckAngle - curAngle >= 180)
                {
                    noLegal = true;
                    break;
                }
                if (nextEdge == null)
                {
                    noLegal = true;
                    Debug.LogWarning("cannot find next edge in vertex:" + vertex.instanceID);
                    break;
                }
                curAngle = vertex.GetEdageAngle(nextEdge);
                edgeList.Add((tempTrueAngle, nextEdge));
                edgeIDList.Add(nextEdge.InstanceID);
                vertexIdList.Add(endVertexID);
                if (edgeList.Count >= 20)
                {
                    fail = true;
                    Debug.LogError("cell has too many edges:" + edgeList);
                    break;
                }
                if (endVertexID == nextEdge.triangleIdA)
                    endVertexID = nextEdge.triangleIdB;
                else
                    endVertexID = nextEdge.triangleIdA;
                curEdge = nextEdge;
            }
            if (noLegal)
            {
                continue;
            }
            edgeList.Sort((a, b) =>
            {
                if (a.Item1 < b.Item1) return -1;
                else if (a.Item1 > b.Item1) return 1;
                else return 0;
            });
            var edges = new List<EdgeV>();
            edgeList.ForEach(e => edges.Add(e.Item2));
            var cell = new Cell(edges, vertexIdList);

            if (fail)
            {
                if (errorcellDic.ContainsKey(cell.instanceID))
                {
                    Debug.LogError($"repeate add cell：" + cell.instanceID);
                    continue;
                }
                errorcellDic.Add(cell.instanceID, cell);
                break;
            }
            else
            {
                if (cellDic.ContainsKey(cell.instanceID))
                {
                    //var oldcell = cellDic[cell.instanceID];
                    //var newcell = cell;
                    //errorcellDic.Add(cell.instanceID + 1, oldcell);
                    //errorcellDic.Add(cell.instanceID, newcell);
                    //break;
                    continue;
                }
                cellDic.Add(cell.instanceID, cell);
            }
        }
    }
     
    private (List<EdgeV>, Dictionary<long, CellVertex>) SetVoronoi(List<Triangle> allTriangle)
    {
        var voronoiEdgeList = new List<EdgeV>();
        var voronoiIDList = new List<long>();
        var vertexDic = new Dictionary<long, CellVertex>();
        for (int i = 0; i < allTriangle.Count; i++)
        {
            List<Edge> neighborEdgeList = new List<Edge>();
            for (int j = 0; j < allTriangle.Count; j++)
            {
                if (j == i) continue;
                Edge neighborEdge = findCommonEdge(allTriangle[i], allTriangle[j]);
                if (neighborEdge == null) continue;

                neighborEdgeList.Add(neighborEdge);
                
                var voronoiEdge = new EdgeV(allTriangle[i], allTriangle[j]);
                if (!voronoiIDList.Contains(voronoiEdge.InstanceID))
                {
                    voronoiEdgeList.Add(voronoiEdge);
                    voronoiIDList.Add(voronoiEdge.InstanceID);
                }

                long idA = voronoiEdge.triangleIdA;
                if (!vertexDic.ContainsKey(idA)) vertexDic.Add(idA, new CellVertex(idA, voronoiEdge._a));
                vertexDic[idA].AddEdge(voronoiEdge);

                long idB = voronoiEdge.triangleIdB;
                if (!vertexDic.ContainsKey(idB)) vertexDic.Add(idB, new CellVertex(idB, voronoiEdge._b));
                vertexDic[idB].AddEdge(voronoiEdge);
            }
             
            //if (neighborEdgeList.Count == 2)
            //{
            //    Vector2 midPoint = Vector2.zero;
            //    Edge rayEdge;
            //    if (isPointOnEdge(neighborEdgeList[0], allTriangle[i].m_Point1) && isPointOnEdge(neighborEdgeList[1], allTriangle[i].m_Point1))
            //    {
            //        midPoint = findMidPoint(allTriangle[i].m_Point2, allTriangle[i].m_Point3);
            //        bool IsObtuseAngle = isPointOnEdge(allTriangle[i].longEdge, allTriangle[i].m_Point2) && isPointOnEdge(allTriangle[i].longEdge, allTriangle[i].m_Point3);
            //        rayEdge = produceRayEdge(allTriangle[i].center, midPoint, allTriangle[i].IsCenterOut, IsObtuseAngle);
            //        voronoiEdgeList.Add(rayEdge);
            //    }
            //    if (isPointOnEdge(neighborEdgeList[0], allTriangle[i].m_Point2) && isPointOnEdge(neighborEdgeList[1], allTriangle[i].m_Point2))
            //    {
            //        midPoint = findMidPoint(allTriangle[i].m_Point1, allTriangle[i].m_Point3);
            //        bool IsObtuseAngle = isPointOnEdge(allTriangle[i].longEdge, allTriangle[i].m_Point1) && isPointOnEdge(allTriangle[i].longEdge, allTriangle[i].m_Point3);
            //        rayEdge = produceRayEdge(allTriangle[i].center, midPoint, allTriangle[i].IsCenterOut, IsObtuseAngle);
            //        voronoiEdgeList.Add(rayEdge);
            //    }
            //    if (isPointOnEdge(neighborEdgeList[0], allTriangle[i].m_Point3) && isPointOnEdge(neighborEdgeList[1], allTriangle[i].m_Point3))
            //    {
            //        midPoint = findMidPoint(allTriangle[i].m_Point2, allTriangle[i].m_Point1);
            //        bool IsObtuseAngle = isPointOnEdge(allTriangle[i].longEdge, allTriangle[i].m_Point2) && isPointOnEdge(allTriangle[i].longEdge, allTriangle[i].m_Point1);
            //        rayEdge = produceRayEdge(allTriangle[i].center, midPoint, allTriangle[i].IsCenterOut, IsObtuseAngle);
            //        voronoiEdgeList.Add(rayEdge);
            //    }
            //}
        }

        foreach (var v in vertexDic) v.Value.SortEdge();
        return (voronoiEdgeList, vertexDic);
    }
 
    private void SetDelaunayTriangle(List<Triangle> allTriangle, List<Vector2> points)
    {
        for (int i = 0; i < points.Count; i++)
        {
            List<Triangle> tempTriList = new List<Triangle>();
            
            for (int j = 0; j < allTriangle.Count; j++)
            {
                tempTriList.Add(allTriangle[j]);
            }
            
            List<Triangle> influencedTriangle = new List<Triangle>();
            
            List<Triangle> newTriangle = new List<Triangle>();
            
            List<Edge> commonEdges = new List<Edge>();
            for (int j = 0; j < tempTriList.Count; j++)
            {
                double lengthToCenter = EuclidianDistance(tempTriList[j].center, points[i]);
                if (lengthToCenter <= tempTriList[j].radius)
                {
                    influencedTriangle.Add(tempTriList[j]);
                    allTriangle.Remove(tempTriList[j]);
                }
            }
            
            for (int j = 0; j < influencedTriangle.Count; j++)
            {
                commonEdges.Add(new Edge(influencedTriangle[j].m_Point1, influencedTriangle[j].m_Point2));
                commonEdges.Add(new Edge(influencedTriangle[j].m_Point1, influencedTriangle[j].m_Point3));
                commonEdges.Add(new Edge(influencedTriangle[j].m_Point2, influencedTriangle[j].m_Point3));
            }
            
            if (commonEdges.Count > 0)
            {
                remmoveEdges(commonEdges);
            }
            
            for (int j = 0; j < commonEdges.Count; j++)
            {
                allTriangle.Add(new Triangle(commonEdges[j]._a, commonEdges[j]._b, points[i]));
            }
        }
    }
     
    private double EuclidianDistance(Vector2 p, Vector2 p2)
    {
        return Mathf.Sqrt(Mathf.Abs((p.x - p2.x)) * Mathf.Abs((p.x - p2.x)) + Mathf.Abs((p.y - p2.y)) * Mathf.Abs((p.y - p2.y)));
    }

     
    public Edge findCommonEdge(Triangle chgTri1, Triangle chgTri2)
    {
        Edge edge;
        List<Vector2> commonPoints = new List<Vector2>();
        if (PointIsEqual(chgTri1.m_Point1, chgTri2.m_Point1) || PointIsEqual(chgTri1.m_Point1, chgTri2.m_Point2) || PointIsEqual(chgTri1.m_Point1, chgTri2.m_Point3))
        {
            commonPoints.Add(chgTri1.m_Point1);
        }
        if (PointIsEqual(chgTri1.m_Point2, chgTri2.m_Point1) || PointIsEqual(chgTri1.m_Point2, chgTri2.m_Point2) || PointIsEqual(chgTri1.m_Point2, chgTri2.m_Point3))
        {
            commonPoints.Add(chgTri1.m_Point2);
        }
        if (PointIsEqual(chgTri1.m_Point3, chgTri2.m_Point1) || PointIsEqual(chgTri1.m_Point3, chgTri2.m_Point2) || PointIsEqual(chgTri1.m_Point3, chgTri2.m_Point3))
        {
            commonPoints.Add(chgTri1.m_Point3);
        }
        if (commonPoints.Count == 2)
        {
            edge = new Edge(commonPoints[0], commonPoints[1]);
            return edge;
        }
        return null;
    }
    
    public Vector2 findMidPoint(Vector2 a, Vector2 b)
    {
        return new Vector2((a.x + b.x) / 2.0f, (a.y + b.y) / 2.0f);
    }
    
    public bool PointIsEqual(Vector2 a, Vector2 b)
    {
        if (a.x == b.x && a.y == b.y)
            return true;
        return false;
    }
    
    public Edge produceRayEdge(Vector2 start, Vector2 direction, bool IsCenterOut, bool IsObtuseAngle)
    {
        Vector2 end = Vector2.zero;
        Edge longEdge;

        if (!IsCenterOut)
        {
            end = 2000 * (direction - start);
        }
        else
        {
            if (IsObtuseAngle)
                end = 2000 * (start - direction);
            else end = 2000 * (direction - start);
        }
        longEdge = new Edge(start, end);
        return longEdge;
    }
    
    public bool isPointOnEdge(Edge edge, Vector2 Point)
    {
        if (edge == null) return false;
        if (PointIsEqual(Point, edge._a) || PointIsEqual(Point, edge._b))
            return true;
        return false;
    }
    
    public void remmoveEdges(List<Edge> edges)
    {
        List<Edge> tmpEdges = new List<Edge>();
        
        for (int i = 0; i < edges.Count; i++)
        {
            tmpEdges.Add(edges[i]);
        }

        for (int i = 0; i < tmpEdges.Count; i++)
        {
            for (int j = i + 1; j < tmpEdges.Count; j++)
            {
                if (IsEdgeEqual(tmpEdges[i], tmpEdges[j]))
                {
                    tmpEdges[i].IsBad = true;
                    tmpEdges[j].IsBad = true;
                }
            }
        }
        edges.RemoveAll((Edge edge) => { return edge.IsBad; });
    }

    public bool IsEdgeEqual(Edge edge1, Edge edge2)
    {
        int samePointNum = 0;
        if (PointIsEqual(edge1._a, edge2._a) || PointIsEqual(edge1._a, edge2._b))
            samePointNum++;
        if (PointIsEqual(edge1._b, edge2._a) || PointIsEqual(edge1._b, edge2._b))
            samePointNum++;
        if (samePointNum == 2)
            return true;
        return false;
    }

    
    private bool isInCircle(Triangle triangle, Vector2 Point)
    {
        double lengthToCenter;
        lengthToCenter = EuclidianDistance(triangle.center, Point);
        if (lengthToCenter < triangle.radius)
        {
            return true;
        }
        return false;
    }
    
    private void returnEdgesofTriangleList(List<Triangle> allTriangle, out List<Edge> edges)
    {
        List<Edge> commonEdges = new List<Edge>();
        List<Triangle> tempTri = new List<Triangle>();
        for (int i = 0; i < allTriangle.Count; i++)
        {
            tempTri.Add(allTriangle[i]);
        }
       
        //for (int i = 0; i < tempTri.Count; i++)
        //{
        //    if (PointIsEqual(tempTri[i].m_Point1, pointA) || PointIsEqual(tempTri[i].m_Point1, pointB) || PointIsEqual(tempTri[i].m_Point1, pointC))
        //        allTriangle.Remove(tempTri[i]);
        //    else if (PointIsEqual(tempTri[i].m_Point2, pointA) || PointIsEqual(tempTri[i].m_Point2, pointB) || PointIsEqual(tempTri[i].m_Point2, pointC))
        //        allTriangle.Remove(tempTri[i]);
        //    else if (PointIsEqual(tempTri[i].m_Point3, pointA) || PointIsEqual(tempTri[i].m_Point3, pointB) || PointIsEqual(tempTri[i].m_Point3, pointC))
        //        allTriangle.Remove(tempTri[i]);
        //}
        for (int i = 0; i < allTriangle.Count; i++)
        {
            commonEdges.Add(new Edge(allTriangle[i].m_Point1, allTriangle[i].m_Point2));

            commonEdges.Add(new Edge(allTriangle[i].m_Point1, allTriangle[i].m_Point3));

            commonEdges.Add(new Edge(allTriangle[i].m_Point2, allTriangle[i].m_Point3));
        }
        edges = commonEdges;
    }


}

public class Edge
{
    public Vector2 _a, _b;
    public Edge(Vector2 a, Vector2 b)
    {
        _a = a;
        _b = b;
    }
    public bool IsBad;
}
public class EdgeV
{
    public long InstanceID;
    public Vector2 _a, _b;
    public long triangleIdA, triangleIdB;
    public Vector2 normalize;
    public float angle;
    public EdgeV(Triangle a, Triangle b)
    {
        var change = false;
        if (a.center.x > b.center.x)
        {
            change = true;
        }
        else if (a.center.x == b.center.x)
        {
            if (a.center.y > b.center.y)
            {
                change = true;
            }
            else if (a.center.y > b.center.y)
            {
                throw new System.Exception($"Exist same edge：{a.center}");
            }
        }
        if (change)
        {
            var temp = a;
            a = b; b = temp;
        }
        _a = a.center;
        _b = b.center;
        triangleIdA = a.instanceID;
        triangleIdB = b.instanceID;
        normalize = (_b - _a).normalized;
        if (normalize.x != 0)
            angle = GetAngle();
        else
            System.Diagnostics.Debugger.Break();
        InstanceID = triangleIdA * 1000000 + triangleIdB;
    }
    public bool IsBad;


    private float GetAngle()
    {
        float angle = Mathf.Acos(Vector2.Dot(normalize, Vector2.up)) * Mathf.Rad2Deg;
        if (float.IsNaN(angle)) throw new Exception("not a number:" + normalize);

        var crossProduct = Vector3.Cross(normalize, Vector2.up);

        bool isClockwise = Vector3.Dot(crossProduct, Vector3.forward) < 0;
        if (isClockwise)
            return angle + 180;
        else
            return angle;
    }
}

public class Triangle
{
    private static long idgenerator;
    public long instanceID;
    public Vector2 m_Point1, m_Point2, m_Point3;
    public Vector2 center;
    public double radius;
    public List<Triangle> adjoinTriangle;
     
    public bool IsCenterOut; 
    public Edge longEdge; 

    public Triangle(Vector2 point1, Vector2 point2, Vector2 point3)
    {
        instanceID = ++idgenerator;
        m_Point1 = point1;
        m_Point2 = point2;
        m_Point3 = point3;
        center = GetCenter(m_Point1, m_Point2, m_Point3);
    }

    private Vector2 GetCenter(Vector2 p1, Vector2 p2, Vector2 p3)
    {
        //(x-a)^2+(y-b)^2=r^2
        Vector2 center = Vector2.zero;
        center.x = ((p2.y - p1.y) * (p3.y * p3.y - p1.y * p1.y + p3.x * p3.x - p1.x * p1.x) - (p3.y - p1.y) * (p2.y * p2.y - p1.y * p1.y + p2.x * p2.x - p1.x * p1.x)) / (2 * (p3.x - p1.x) * (p2.y - p1.y) - 2 * ((p2.x - p1.x) * (p3.y - p1.y)));
        center.y = ((p2.x - p1.x) * (p3.x * p3.x - p1.x * p1.x + p3.y * p3.y - p1.y * p1.y) - (p3.x - p1.x) * (p2.x * p2.x - p1.x * p1.x + p2.y * p2.y - p1.y * p1.y)) / (2 * (p3.y - p1.y) * (p2.x - p1.x) - 2 * ((p2.y - p1.y) * (p3.x - p1.x)));

        radius = Mathf.Sqrt(Mathf.Abs(p1.x - center.x) * Mathf.Abs(p1.x - center.x) + Mathf.Abs(p1.y - center.y) * Mathf.Abs(p1.y - center.y));

        float L1 = Vector2.Distance(p1, p2);
        float L2 = Vector2.Distance(p1, p3);
        float L3 = Vector2.Distance(p3, p2);
        if (L1 > L2 && L1 > L3)
            longEdge = new Edge(p1, p2);
        if (L2 > L1 && L2 > L3)
            longEdge = new Edge(p1, p3);
        if (L3 > L2 && L3 > L1)
            longEdge = new Edge(p2, p3);

        IsCenterOut = L1 * L1 > (L2 * L2 + L3 * L3) || L2 * L2 > (L1 * L1 + L3 * L3) || L3 * L3 > (L2 * L2 + L1 * L1);
        return center;
    }
}

public class SiteSorterXY : IComparer<Vector2>
{
    public int Compare(Vector2 p1, Vector2 p2)
    {
        if (p1.x > p2.x) return 1;
        if (p1.x < p2.x) return -1;
        return 0;
    }
}
