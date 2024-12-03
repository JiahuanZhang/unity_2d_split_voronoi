using System.Collections.Generic;
using UnityEngine;

public static class VoronoiMeshHelper
{
    public static MeshGroupData CreateMeshes(Dictionary<long, Cell> cells, Dictionary<long, CellVertex> vertexDic, Vector2 screenSize, int seed, string textureName, Vector2 uvs, Vector2 meshSize, int countX, int countY)
    {
        var clips = CreateMeshChipDatas(cells, vertexDic, screenSize);
        clips.Sort(SortMeshChips);

        var meshData = new MeshGroupData();
        meshData.Texture = textureName;
        meshData.Seed = seed;
        meshData.Uvs = uvs;
        meshData.MeshSize = meshSize;
        meshData.PointCountX = countX;
        meshData.PointCountY = countY;
        meshData.ChipDatas = clips.ToArray();
        return meshData;

    }

    public static MeshGroupData CreateMeshesWithTexture(Dictionary<long, Cell> cells, Dictionary<long, CellVertex> vertexDic, Vector2 screenSize, int seed, Texture2D texture, bool alphaTest, Vector2 uvs, Vector2 meshSize, int countX, int countY)
    {
        var clips = CreateMeshChipDatas(cells, vertexDic, screenSize);
        clips.Sort(SortMeshChips);
        if (alphaTest)
        {
            foreach (var cell in cells.Values)
            {
                bool isTransparent = true;
                foreach (var vertexId in cell.vertexIds)
                {
                    var pos = vertexDic[vertexId].Pos;
                    if (GetAlphaFromUV(pos.x, pos.y) >= 0.5f)
                    {
                        isTransparent = false;
                    }
                }
                if (isTransparent)
                {
                    var index = clips.FindIndex(e => e.InstanceID == cell.instanceID);
                    if (index >= 0)
                    {
                        clips.RemoveAt(index);
                    }
                }
            }
        }

        var meshData = new MeshGroupData();
        meshData.Texture = texture.name;
        meshData.Seed = seed;
        meshData.Uvs = uvs;
        meshData.MeshSize = meshSize;
        meshData.PointCountX = countX;
        meshData.PointCountY = countY;
        meshData.ChipDatas = clips.ToArray();
        return meshData;


        float GetAlphaFromUV(float u, float v)
        {
            if (texture == null)
            {
                Debug.LogError("Texture2D is not assigned!");
                return 0f;
            }

            int x = Mathf.FloorToInt(u * texture.width);
            int y = Mathf.FloorToInt(v * texture.height);
            Color color = texture.GetPixel(x, y);

            return color.a;
        }
    }

    public static List<MeshChipData> CreateMeshChipDatas(Dictionary<long, Cell> cells, Dictionary<long, CellVertex> vertexDic, Vector2 screenSize)
    {
        var tempChips = new List<MeshChipData>();

        foreach (var cell in cells.Values)
        {
            bool isOutofUv = false;

            float tolerantX = 0.2f, tolerantY = 0.2f;
            foreach (var vertexId in cell.vertexIds)
            {
                var pos = vertexDic[vertexId].Pos;
                if (pos.x < (0 - tolerantX) || pos.x > (1 + tolerantX) || pos.y < (0 - tolerantY) || pos.y > (1 + tolerantY))
                {
                    isOutofUv = true;
                    break;
                }
            }
            if (isOutofUv) continue;

            var chipData = new MeshChipData(cell.instanceID);
            tempChips.Add(chipData);

            var vertices = new List<Vector3>();
            foreach (var vertexId in cell.vertexIds)
            {
                var pos = vertexDic[vertexId].Pos;
                vertices.Add(new Vector3(pos.x * screenSize.x, pos.y * screenSize.y, 0));
            }
            chipData.Vertices = vertices.ToArray();

            var uvs = new List<Vector2>();
            var uvt = Vector2.zero;
            foreach (var vertexId in cell.vertexIds)
            {
                uvs.Add(vertexDic[vertexId].Pos);
                uvt += vertexDic[vertexId].Pos;
            }
            chipData.Uvs = uvs.ToArray();
            chipData.Center = uvt / uvs.Count;

            var triangles = new List<int>();
            foreach (var triangle in cell.triangles)
            {
                triangles.Add(triangle.x);
                triangles.Add(triangle.y);
                triangles.Add(triangle.z);
            }
            chipData.Triangles = triangles.ToArray();
        }

        return tempChips;
    }

    private static int SortMeshChips(MeshChipData a, MeshChipData b)
    {
        var interval = a.Center.y - b.Center.y;
        var absInterval = Mathf.Abs(interval);
        if (absInterval <= 0.01f)
        {
            if (a.Center.x < b.Center.x)
                return -1;
            else if (a.Center.x > b.Center.x)
                return 1;
            else return 0;
        }
        else if (interval < 0)
        {
            return -1;
        }
        else
        {
            return 1;
        }
    }
}