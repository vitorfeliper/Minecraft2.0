﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Chunk
{
    [Header("Chunks Configs")]

    public ChunkCoord coord;

    GameObject chunckObject;

    MeshRenderer meshRenderer;
    MeshFilter meshFilter;
    MeshCollider meshCollider;

    int vertexIndex = 0;
    //-----------LISTS--------------------------//
    List<Vector3> vertices = new List<Vector3>();
    List<int> triangles = new List<int>();
    List<int> transparentTriangles = new List<int>();
    Material[] materials = new Material[2];
    List<Vector2> Uvs = new List<Vector2>();
    List<Color> colors = new List<Color>();
    List<Vector3> normals = new List<Vector3>();
    //----------------------------------------//

    public Vector3 position;

    public VoxelState[,,] voxelMap = new VoxelState[VoxelData.ChunkWidth, VoxelData.ChunkHeight, VoxelData.ChunkWidth];

    public Queue<VoxelMod> modifications = new Queue<VoxelMod>();

    World world;


    private bool _isActive;

   private bool isVoxelMapPopulated = false;


    public Chunk (ChunkCoord _coord,  World _world)
    {
        coord = _coord;
        world = _world;
        
    }

    public void Init()
    {
        chunckObject = new GameObject();
        meshFilter = chunckObject.AddComponent<MeshFilter>();
        meshRenderer = chunckObject.AddComponent<MeshRenderer>();
        meshCollider = chunckObject.AddComponent<MeshCollider>();

        materials[0] = world.material;
        materials[1] = world.transparentMaterial;
        meshRenderer.materials = materials;

        //meshRenderer.material = world.material;
        chunckObject.transform.SetParent(world.transform);
        chunckObject.transform.position = new Vector3(coord.x * VoxelData.ChunkWidth, 0f, coord.z * VoxelData.ChunkWidth);
        chunckObject.name = "Chunk " + "(" + coord.x + ", " + coord.z + ")";
        position = chunckObject.transform.position;

        PopulateVoxelMap();

        //PopulateVoxelMap();
        //UpdateChunk();

        //meshCollider.sharedMesh = meshFilter.mesh;
    }

    public void PopulateVoxelMap()
    {
        for (int y = 0; y < VoxelData.ChunkHeight; y++)
        {
            for (int x = 0; x < VoxelData.ChunkWidth; x++)
            {
                for (int z = 0; z < VoxelData.ChunkWidth; z++)
                {
                    voxelMap[x, y, z] = new VoxelState(world.GetVoxel(new Vector3(x, y, z) + position));
                }
            }
        }

        isVoxelMapPopulated = true;

        lock (world.ChunkUpdateThreadLock)
        {
            world.chunksToUpdate.Add(this);
        }

        if(world.settings.enableAnimatedChunks)
            chunckObject.AddComponent<ChunkLoadAnimation>();
    }

    public void UpdateChunk()
    {

        while(modifications.Count > 0)
        {
            VoxelMod v = modifications.Dequeue();
            Vector3 pos = v.position -= position;
            voxelMap[(int)pos.x, (int)pos.y, (int)pos.z].id = v.id;
        }


        ClearMeshData();
        CalculateLight();

        for (int y = 0; y < VoxelData.ChunkHeight; y++)
        {
            for (int x = 0; x < VoxelData.ChunkWidth; x++)
            {
                for (int z = 0; z < VoxelData.ChunkWidth; z++)
                {
                    if(world.blockTypes[voxelMap[x, y, z].id].isSolid)
                    {
                        UpdateMeshData(new Vector3(x, y, z));
                    }      
                }
            }
        }

        lock (world.chunksToDraw)
        {
            world.chunksToDraw.Enqueue(this);
        }

    }

    public void CalculateLight()
    {
        Queue<Vector3Int> litVoxels = new Queue<Vector3Int>();

        for (int x = 0; x < VoxelData.ChunkWidth; x++)
        {
            for (int z = 0; z < VoxelData.ChunkWidth; z++)
            {
                float lightRay = 1f;

                for(int y = VoxelData.ChunkHeight - 1; y >= 0; y--)
                {
                    VoxelState thisVoxel = voxelMap[x, y, z];

                    if(thisVoxel.id > 0 && world.blockTypes[thisVoxel.id].transparency < lightRay)
                    {
                        lightRay = world.blockTypes[thisVoxel.id].transparency;
                    }

                    thisVoxel.globalLightPercent = lightRay;
                    voxelMap[x, y, z] = thisVoxel;

                    if(lightRay > VoxelData.lightFalloff)
                    {
                        litVoxels.Enqueue(new Vector3Int(x, y, z));
                    }
                }
            }
        }

        while(litVoxels.Count > 0)
        {
            Vector3Int v = litVoxels.Dequeue();

            for(int p = 0; p < 6; p++)
            {
                Vector3 currentVoxel = v + VoxelData.faceChecks[p];
                Vector3Int neighboor = new Vector3Int((int)currentVoxel.x, (int)currentVoxel.y, (int)currentVoxel.z);

                if(IsVoxelInChunk(neighboor.x, neighboor.y, neighboor.z))
                {
                    if(voxelMap[neighboor.x, neighboor.y, neighboor.z].globalLightPercent < voxelMap[v.x, v.y, v.z].globalLightPercent - VoxelData.lightFalloff)
                    {
                        voxelMap[neighboor.x, neighboor.y, neighboor.z].globalLightPercent = voxelMap[v.x, v.y, v.z].globalLightPercent - VoxelData.lightFalloff;

                        if(voxelMap[neighboor.x, neighboor.y, neighboor.z].globalLightPercent > VoxelData.lightFalloff)
                        {
                            litVoxels.Enqueue(neighboor);
                        }

                    }
                }
            }
        }
    }

    public void ClearMeshData()
    {
        vertexIndex = 0;
        vertices.Clear();
        triangles.Clear();
        transparentTriangles.Clear();
        Uvs.Clear();
        colors.Clear();
        normals.Clear();
    }

    public bool IsActive
    {
        get { return _isActive; }
        set 
        {
            _isActive = value;    
            if(chunckObject != null)
            {
                chunckObject.SetActive(value);
            }
        }
    }

/*    public Vector3 position
    {
        get { return chunckObject.transform.position; }
    }
*/
    public bool isEditable
    {
        get
        {
            if(!isVoxelMapPopulated)
            {
                return false;
            }
            else
            {
                return true;
            }
        }
    }

    public bool IsVoxelInChunk(int x, int y, int z)
    {
        if (x < 0 || x > VoxelData.ChunkWidth - 1 || y < 0 || y > VoxelData.ChunkHeight - 1 || z < 0 || z > VoxelData.ChunkWidth - 1)
        {
            return false;
        }
        else
        {
            return true;
        }
    }

    public void EditVoxel(Vector3 pos, byte newID)
    {
        int xCheck = Mathf.FloorToInt(pos.x);
        int yCheck = Mathf.FloorToInt(pos.y);
        int zCheck = Mathf.FloorToInt(pos.z);

        xCheck -= Mathf.FloorToInt(chunckObject.transform.position.x);
        zCheck -= Mathf.FloorToInt(chunckObject.transform.position.z);

        voxelMap[xCheck, yCheck, zCheck].id = newID;

        lock (world.ChunkUpdateThreadLock)
        {
            world.chunksToUpdate.Insert(0, this);
            UpdateSurroudingVoxels(xCheck, yCheck, zCheck);
        }

        //UpdateChunk();
    }

    public void UpdateSurroudingVoxels(int x, int y, int z)
    {
        Vector3 thisVoxel = new Vector3(x, y, z);

        for (int p = 0; p < 6; p++)
        {
            Vector3 currentVoxel = thisVoxel + VoxelData.faceChecks[p];

            if (!IsVoxelInChunk((int)currentVoxel.x, (int)currentVoxel.y, (int)currentVoxel.z))
            {
                world.chunksToUpdate.Insert(0, world.GetChunkFromVector3(thisVoxel + position));/*.UpdateChunk();*/
            }
        }

    }

    public VoxelState CheckVoxel(Vector3 pos)
    {

        int x = Mathf.FloorToInt(pos.x);
        int y = Mathf.FloorToInt(pos.y);
        int z = Mathf.FloorToInt(pos.z);

        if (!IsVoxelInChunk(x, y, z))
            return world.GetVoxelState(pos + position);

        return voxelMap[x, y, z];

    }

    public VoxelState GetVOxelFromGlobalVector3(Vector3 pos)
    {
        int xCheck = Mathf.FloorToInt(pos.x);
        int yCheck = Mathf.FloorToInt(pos.y);
        int zCheck = Mathf.FloorToInt(pos.z);

        xCheck -= Mathf.FloorToInt(position.x);
        zCheck -= Mathf.FloorToInt(position.z);

        return voxelMap[xCheck, yCheck, zCheck];
    }

    public void UpdateMeshData(Vector3 pos)
    {
        int x = Mathf.FloorToInt(pos.x);
        int y = Mathf.FloorToInt(pos.y);
        int z = Mathf.FloorToInt(pos.z);

        byte blockID = voxelMap[x, y, z].id;
        //bool isTransparent = world.blockTypes[blockID].renderNeighborFaces;

        //--------Triangles loop----------------//
        for (int p = 0; p < 6; p++)
        {

            VoxelState neighboor = CheckVoxel(pos + VoxelData.faceChecks[p]);

            if(neighboor != null && world.blockTypes[neighboor.id].renderNeighborFaces)
            {
                vertices.Add(pos + VoxelData.voxelVerts[VoxelData.voxelTris[p, 0]]);
                vertices.Add(pos + VoxelData.voxelVerts[VoxelData.voxelTris[p, 1]]);
                vertices.Add(pos + VoxelData.voxelVerts[VoxelData.voxelTris[p, 2]]);
                vertices.Add(pos + VoxelData.voxelVerts[VoxelData.voxelTris[p, 3]]);

                for(int i = 0; i < 4; i++)
                {
                    normals.Add(VoxelData.faceChecks[p]);
                }

                AddTexture(world.blockTypes[blockID].GetTextureID(p));

                float lighLevel = neighboor.globalLightPercent;
            /*
                int yPos = (int)pos.y + 1;
                bool inShade = false;

                while(yPos < VoxelData.ChunkHeight)
                {
                    if (voxelMap[(int)pos.x, yPos, (int)pos.z].id != 0)
                    {
                        inShade = true;
                        break;
                    }

                    yPos++;
                }

                if (inShade)
                {
                    lighLevel = 0.5f;
                }
                else
                {
                    lighLevel = 0f;
                }
            */

                colors.Add(new Color(0, 0, 0, lighLevel));
                colors.Add(new Color(0, 0, 0, lighLevel));
                colors.Add(new Color(0, 0, 0, lighLevel));
                colors.Add(new Color(0, 0, 0, lighLevel));


                if (!world.blockTypes[neighboor.id].renderNeighborFaces)
                {
                
                    triangles.Add(vertexIndex);
                    triangles.Add(vertexIndex + 1);
                    triangles.Add(vertexIndex + 2);
                    triangles.Add(vertexIndex + 2);
                    triangles.Add(vertexIndex + 1);
                    triangles.Add(vertexIndex + 3);
                }
                else
                {
                    transparentTriangles.Add(vertexIndex);
                    transparentTriangles.Add(vertexIndex + 1);
                    transparentTriangles.Add(vertexIndex + 2);
                    transparentTriangles.Add(vertexIndex + 2);
                    transparentTriangles.Add(vertexIndex + 1);
                    transparentTriangles.Add(vertexIndex + 3);
                }


                vertexIndex += 4;
            }
        }
        //---------------------------------//
    }

    public void Createmesh()
    {
        //-----Build Mesh-------------//
        Mesh mesh = new Mesh();
        mesh.vertices = vertices.ToArray();

        mesh.subMeshCount = 2;
        mesh.SetTriangles(triangles.ToArray(), 0);
        mesh.SetTriangles(transparentTriangles.ToArray(), 1);
        //mesh.triangles = triangles.ToArray();
        mesh.uv = Uvs.ToArray();
        mesh.colors = colors.ToArray();
        mesh.normals = normals.ToArray();
        //mesh.RecalculateNormals();

        meshFilter.mesh = mesh;
        //---------------------------//
    }

    public void AddTexture(int textureID)
    {
        float y = textureID / VoxelData.TextureAtlasSizeInBlocks;
        float x = textureID - (y * VoxelData.TextureAtlasSizeInBlocks);

        x *= VoxelData.NormalizedblockTextureSize;
        y *= VoxelData.NormalizedblockTextureSize;

        y = 1f - y - VoxelData.NormalizedblockTextureSize;

        Uvs.Add(new Vector2(x, y));
        Uvs.Add(new Vector2(x, y + VoxelData.NormalizedblockTextureSize));
        Uvs.Add(new Vector2(x + VoxelData.NormalizedblockTextureSize, y));
        Uvs.Add(new Vector2(x + VoxelData.NormalizedblockTextureSize, y + VoxelData.NormalizedblockTextureSize));

    }
}

public class ChunkCoord
{
    public int x;
    public int z;

    public ChunkCoord()
    {
        x = 0;
        z = 0;
    }

    public ChunkCoord(int _x, int _z)
    {
        x = _x;
        z = _z;
    }

    public ChunkCoord(Vector3 pos)
    {
        int xCheck = Mathf.FloorToInt(pos.x);
        int zCheck = Mathf.FloorToInt(pos.z);

        x = xCheck / VoxelData.ChunkWidth;
        z = zCheck / VoxelData.ChunkWidth;
    }

    public bool Equals(ChunkCoord others)
    {
        if(others == null)
        {
            return false;
        }
        else if(others.x == x && others.z == z)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
}

public class VoxelState
{
    public byte id;
    public float globalLightPercent;

    public VoxelState()
    {
        id = 0;
        globalLightPercent = 0f;
    }

    public VoxelState(byte _id)
    {
        id = _id;
        globalLightPercent = 0f;
    }
}



/*
 *              for (int i = 0; i < 6; i++)
                {
                    int triangleIndex = VoxelData.voxelTris[p, i];
                    vertices.Add(VoxelData.voxelVerts[triangleIndex] + pos);

                    triangles.Add(vertexIndex);


                    Uvs.Add(VoxelData.voxelUvs[i]);

                    vertexIndex++;
                } 
 */