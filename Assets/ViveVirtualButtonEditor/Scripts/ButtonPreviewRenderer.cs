using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class ButtonPreviewRenderer : MonoBehaviour {

    public string ButtonName;
    public float MeshSize = 0.02f;
    public float PolarAnglePerVertex = 15f;

    private MeshFilter _Filter;

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position - Vector3.forward * MeshSize / 2, transform.position + Vector3.forward * MeshSize / 2);
        Gizmos.DrawLine(transform.position - Vector3.right * MeshSize / 2, transform.position + Vector3.right * MeshSize / 2);
    }

	void Start () {
        _Filter = GetComponent<MeshFilter>();

        ResetMesh();
	}

    public void ResetMesh()
    {
        Mesh m = new Mesh();
        Vector2[] verts = null;
        var region = ViveVirtualButtonManager.Instance.GetButtonRegion(ButtonName);

        int len;
        region.MakeButtonMesh(ref verts, out len, PolarAnglePerVertex);

        Vector3[] verts3 = new Vector3[len];
        for (int x = 0; x < len; x++)
            verts3[x] = new Vector3(verts[x].x, verts[x].y, 0) * MeshSize;

        m.vertices = verts3;

        for (int x = 0; x < len; x++)
            verts[x] = (verts[x] + Vector2.one) / 2f;

        m.uv = verts;

        int[] triangles = new int[(len - 2) * 3];
        for(int x = 0; x < len - 2; x++)
        {
            if(x % 2 == 0)
            {
                triangles[x * 3] = x;
                triangles[x * 3 + 1] = x + 1;
                triangles[x * 3 + 2] = x + 2;
            } else
            {
                triangles[x * 3] = x + 1;
                triangles[x * 3 + 1] = x;
                triangles[x * 3 + 2] = x + 2;
            }
        }

        m.triangles = triangles;

        m.RecalculateBounds();
        m.RecalculateNormals();

        _Filter.mesh = m;
    }
}
