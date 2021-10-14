using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class VoxelData
{
    [Header("Voxel Data Configs")]

    public static readonly int ChunkWidth = 16;
    public static readonly int ChunkHeight = 128;
    public static readonly int WorldSizeChunks = 100;

    public static readonly int TextureAtlasSizeInBlocks = 16;

    //Lighting Values
    public static float minLightLevel = 0.1f;
    public static float maxLightLevel = 0.9f;
    public static float lightFalloff = 0.08f;

    public static int seed;

    public static int worldCentre
    {
        get { return (WorldSizeChunks * ChunkWidth) / 2; }
    }

    public static int WorldSizeInVoxels
    {
        get { return WorldSizeChunks * ChunkWidth; }
    }

    //public static readonly int ViewDistanceInChunks = 5;

    public static float NormalizedblockTextureSize
    {
        get { return 1f / (float)TextureAtlasSizeInBlocks; }
    }


    public static readonly Vector3[] voxelVerts = new Vector3[8]
    {
        new Vector3(0.0f, 0.0f, 0.0f),//Point[0]
        new Vector3(1.0f, 0.0f, 0.0f),//Point[1]
        new Vector3(1.0f, 1.0f, 0.0f),//Point[2]
        new Vector3(0.0f, 1.0f, 0.0f),//Point[3]
        new Vector3(0.0f, 0.0f, 1.0f),//Point[4]
        new Vector3(1.0f, 0.0f, 1.0f),//Point[5]
        new Vector3(1.0f, 1.0f, 1.0f),//Point[6]
        new Vector3(0.0f, 1.0f, 1.0f),//Point[7]
        
    };

    //CHECK FACES
    public static readonly Vector3[] faceChecks = new Vector3[6]
    {
        new Vector3(0.0f, 0.0f, -1.0f),     //Point[0]  //Back Face
        new Vector3(0.0f, 0.0f, 1.0f),     //Point[1]  //Front Face
        new Vector3(0.0f, 1.0f, 0.0f),    //Point[2]  //Top face of Quad + 2 Triangles vertices
        new Vector3(0.0f, -1.0f, 0.0f),  //Point[3]  //Bottom Face
        new Vector3(-1.0f, 0.0f, 0.0f), //Point[4]  //Left Face
        new Vector3(1.0f, 0.0f, 0.0f)  //Point[5]  //Right Face
    };

    //FACES CUBE
    public static readonly int[,] voxelTris = new int[6, 4]
    {
        //{ 0, 1, 2, 2, 1, 3}

        //Back, Front, Top, Bottom, Left, Right

        { 0, 3, 1, 2 }, //Back Face
        { 5, 6, 4, 7 }, //Front Face
        { 3, 7, 2, 6 }, //Top face of Quad + 2 Triangles vertices
        { 1, 5, 0, 4 }, //Bottom Face
        { 4, 7, 0, 3 }, //Left Face
        { 1, 2, 5, 6 } //Right Face
    };

    //UV FACES
    public static readonly Vector2[] voxelUvs = new Vector2[4]
    {
        new Vector2(0.0f, 0.0f), //Point[0]
        new Vector2(0.0f, 1.0f), //Point[1]
        new Vector2(1.0f, 0.0f), //Point[2]
        new Vector2(1.0f, 1.0f), //Point[5]
    };
}
