using System.Collections.Generic;
using System.Linq;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

[RequireComponent(typeof(MeshFilter), typeof(MeshCollider), typeof(MeshRenderer))]
public class RampTool : MonoBehaviour
{
    [SerializeField] int count = 5;
    [SerializeField] float length = 2;
    [SerializeField] float angle = 90;



    void Start()
    {
        UpdateMesh();
    }


    private Mesh GenerateMesh()
    {
        Vector3[] verts = new Vector3[(count+1)*2];
        Vector2[] uv = new Vector2[(count+1)*2];
        int[] tris = new int[count*2*3];

        float partLength = length/count;
        float partAngleDiff = angle/count;

        verts[0] = Vector3.right;
        verts[1] = Vector3.left;
        uv[0] = new Vector2(0, 0);
        uv[1] = new Vector2(1, 0);
        for (int i = 1; i <= count; i++)
        {
            float angle = partAngleDiff * i * Mathf.Deg2Rad;
            Vector3 forward = Vector3.forward * partLength * Mathf.Cos(angle);
            Vector3 up = Vector3.up * partLength * Mathf.Sin(angle);
            verts[2*i] = verts[2*i-2] + forward + up;
            verts[2*i+1] = verts[2*i-1] + forward + up;
            uv[2*i] = new Vector2(0, (float)i/count);
            uv[2*i+1] = new Vector2(1, (float)i/count);
            int trii = i-1;
            int tri0idx = 3*2*(i-1);
            tris[tri0idx] = 2*trii;
            tris[tri0idx+1] = 2*trii+1;
            tris[tri0idx+2] = 2*trii+2;
            tris[tri0idx+3] = 2*trii+1;
            tris[tri0idx+4] = 2*trii+3;
            tris[tri0idx+5] = 2*trii+2;
        }

        Mesh mesh = new();
        mesh.vertices = verts;
        mesh.triangles = tris;
        mesh.uv = uv;
        return mesh;
    }


    private void UpdateMesh()
    {
        Mesh mesh = GenerateMesh();
        GetComponent<MeshFilter>().mesh = mesh;
        GetComponent<MeshCollider>().sharedMesh = mesh;
    }


    private void OnValidate()
    {
    #if UNITY_EDITOR
        if (!EditorApplication.isPlayingOrWillChangePlaymode)
        {
            EditorApplication.delayCall += UpdateMesh;
        }
    #endif
    }
}
