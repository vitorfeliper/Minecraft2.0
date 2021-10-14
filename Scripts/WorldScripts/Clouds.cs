using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Clouds : MonoBehaviour
{
    public int cloudHeight = 100;

    [SerializeField] private Texture2D cloudPattern = null;
    [SerializeField] private Material cloudMaterial = null;

    bool[,] cloudData;

    List<Vector3> vertices = new List<Vector3>();
    List<int> triangles = new List<int>();

    List<Vector3> normals = new List<Vector3>();

    int vertCount;
    int cloudTexWidth;

    private void Start()
    {
        cloudTexWidth = cloudPattern.width;

        transform.position = new Vector3(VoxelData.worldCentre, cloudHeight, VoxelData.worldCentre);
        MeshFilter mf = GetComponent<MeshFilter>();

        LoadCloudData();
        mf.mesh = GetCloudMesh();


    }

    private void LoadCloudData()
    {
        cloudData = new bool[cloudTexWidth, cloudTexWidth];
        Color[] cloudTex = cloudPattern.GetPixels();

        for(int x = 0; x < cloudTexWidth; x++)
        {
            for (int y = 0; y < cloudTexWidth; y++)
            {
                cloudData[x, y] = (cloudTex[y * cloudTexWidth + x].a > 0);
            }
        }
    }

    private Mesh GetCloudMesh()
    {
        for (int x = 0; x < cloudTexWidth; x++)
        {
            for (int y = 0; y < cloudTexWidth; y++)
            {
                if(cloudData[x, y])
                {
                    AddCloudMeshData(x, y);
                }
            }
        }

        Mesh mesh = new Mesh();
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.normals = normals.ToArray();
        return mesh;
    }

    private void AddCloudMeshData(int x, int z)
    {
        vertices.Add(new Vector3(x, 0, z));
        vertices.Add(new Vector3(x, 0, z + 1));
        vertices.Add(new Vector3(x + 1, 0, z + 1));
        vertices.Add(new Vector3(x + 1, 0, z));

        for(int i = 0; i < 4; i++)
        {
            normals.Add(Vector3.down);
        }

        //First T
        triangles.Add(vertCount + 1);
        triangles.Add(vertCount);
        triangles.Add(vertCount + 2);

        //Second T
        triangles.Add(vertCount + 2);
        triangles.Add(vertCount);
        triangles.Add(vertCount + 3);

        //Increment
        vertCount += 4;


    }
}
