using UnityEditor;
using UnityEngine;

public class MeshClip : MonoBehaviour
{
    private static long IDGenerator = 1;
    public long InstanceID;
    public Vector3 Center;

    public void Init(MeshChipData data, Material material)
    {
        InstanceID = data.InstanceID;
        if (InstanceID <= 0) InstanceID = IDGenerator++;
        name = "m" + InstanceID;
        Center = data.Center;
        //transform.position = Center;

        var rd = gameObject.AddComponent<MeshRenderer>();
        var mf = gameObject.AddComponent<MeshFilter>();
        rd.sharedMaterial = material;


        var mesh = new Mesh();
        mesh.SetVertices(data.Vertices);
        mesh.SetUVs(0, data.Uvs);
        mesh.SetTriangles(data.Triangles, 0);
        mesh.RecalculateBounds();
        mesh.RecalculateNormals();
        mf.mesh = mesh;
    }

    void OnDrawGizmos()
    {
#if UNITY_EDITOR
        // 设置文本颜色
        Handles.color = Color.red;
        // 获取并设置显示文本的位置（这里是在物体的位置上方3个单位）
        Vector3 position = Center;
        // 使用Handles类来显示文本信息
        Handles.Label(position, InstanceID.ToString());
#endif
    }
}