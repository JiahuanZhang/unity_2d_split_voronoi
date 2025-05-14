using System.Collections.Generic;
using UnityEngine;

public class VertexModifier : MonoBehaviour
{
    public int Index;
    public Vector3 Pos;
    public List<(long, int)> CellVertexInfos = new();
    private Vector3 curPos;

    public void Init(Vector3 pos, int index)
    {
        Pos = pos;
        curPos = pos;
        Index = index;
        transform.localPosition = pos;
        gameObject.SetActive(true);
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawSphere(transform.position, 0.05f);
    }

    private void LateUpdate()
    {
        var transPos = transform.localPosition;
        if (curPos.x != transPos.x || curPos.y != transPos.y)
        {
            curPos = transPos;
            curPos.z = 0;
            VoronoiModifier.Instance.MoveVertex(CellVertexInfos, curPos);
        }
    }
}