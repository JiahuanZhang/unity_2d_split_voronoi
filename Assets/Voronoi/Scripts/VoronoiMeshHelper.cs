using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public static class VoronoiMeshHelper
{
    /// <summary>
    /// create meshes
    /// </summary>
    public static MeshGroupData CreateMeshes(Dictionary<long, Cell> cells, Dictionary<long, CellVertex> vertexDic, Vector2 screenSize, int seed, string texture, Vector2 uvs, Vector2 meshSize, int countX, int countY)
    {
        var clips = CreateMeshChipDatas(cells, vertexDic, screenSize);
        clips.Sort(SortMeshChips);

        var meshData = new MeshGroupData();
        meshData.Texture = texture;
        meshData.Seed = seed;
        meshData.Uvs = uvs;
        meshData.MeshSize = meshSize;
        meshData.PointCountX = countX;
        meshData.PointCountY = countY;
        meshData.ChipDatas = clips;
        return meshData;

    }

#if UNITY_EDITOR
    /// <summary>
    /// create meshes with texture, alpha test
    /// </summary>
    public static MeshGroupData CreateMeshesWithTexture(Dictionary<long, Cell> cells, Dictionary<long, CellVertex> vertexDic, Vector2 screenSize, int seed, Texture2D[] textures, bool alphaTest, Vector2 uvs, Vector2 meshSize, int countX, int countY)
    {
        var clips = CreateMeshChipDatas(cells, vertexDic, screenSize);
        clips.Sort(SortMeshChips);
        if (alphaTest)
        {
            foreach (var cell in cells.Values)
            {
                // alpha test
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
        meshData.Texture = AssetDatabase.GetAssetPath(textures[0]);
        meshData.Seed = seed;
        meshData.Uvs = uvs;
        meshData.MeshSize = meshSize;
        meshData.PointCountX = countX;
        meshData.PointCountY = countY;
        meshData.ChipDatas = clips;
        return meshData;


        // get alpha value from uv
        float GetAlphaFromUV(float u, float v)
        {
            if (textures == null || textures.Length <= 0)
            {
                Debug.LogError("Texture2D is not assigned!");
                return 0f;
            }

            var alphas = 0f;
            for (int i = 0; i < textures.Length; i++)
            {
                int x = Mathf.FloorToInt(u * textures[i].width);
                int y = Mathf.FloorToInt(v * textures[i].height);
                Color color = textures[i].GetPixel(x, y);
                alphas += color.a;
            }
            return alphas;
        }
    }
#endif

    public static List<MeshChipData> CreateMeshChipDatas(Dictionary<long, Cell> cells, Dictionary<long, CellVertex> vertexDic, Vector2 screenSize)
    {
        var tempChips = new List<MeshChipData>();

        foreach (var cell in cells.Values)
        {
            // remove meshes out of uv
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

            // set vertices
            var vertices = new List<Vector3>();
            var vertexTemp = Vector3.zero;
            foreach (var vertexId in cell.vertexIds)
            {
                var pos = vertexDic[vertexId].Pos;
                var scaledPos = new Vector3(pos.x * screenSize.x, pos.y * screenSize.y, 0);
                vertices.Add(scaledPos);
                vertexTemp += scaledPos;
            }
            chipData.Vertices = vertices.ToArray();
            chipData.Center = vertexTemp / vertices.Count;

            // set uvs
            var uvs = new List<Vector2>();
            foreach (var vertexId in cell.vertexIds)
            {
                uvs.Add(vertexDic[vertexId].Pos);
            }
            chipData.Uvs = uvs.ToArray();

            // set triangles
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