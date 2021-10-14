using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using System.IO;


public class World : MonoBehaviour
{

    public Settings settings;

    [Header("World Generation Values")]
    public BiomeAttributes[] biomes;

    [Range(0.0f, 1.0f)]
    public float globalLightLevel;
    public Color day;
    public Color night;

    public Transform player;
    public Vector3 spanwPosition;

    [Header ("Materials")]

    public Material material;
    public Material transparentMaterial;

    [Header("All Block Types Here")]

    public BlockType[] blockTypes;

    Chunk[,] chunks = new Chunk[VoxelData.WorldSizeChunks, VoxelData.WorldSizeChunks];

    List<ChunkCoord> activeChunks = new List<ChunkCoord>();
    
    ChunkCoord playerChunkCoord;

    public ChunkCoord playerLastChunkCoord;

    List<ChunkCoord> chunksToCreate = new List<ChunkCoord>();

    public List<Chunk> chunksToUpdate = new List<Chunk>();

    public Queue<Chunk> chunksToDraw = new Queue<Chunk>();

    //private bool isCreatingChunks;
    private bool applyingModifications = false;

    Queue<Queue<VoxelMod>> modifications = new Queue<Queue<VoxelMod>>();

    private bool _inUI = false;

    public GameObject debugScreen;

    public GameObject creativeInventoryWindow;
    public GameObject cursorSlot;

    Thread ChunkUpdateThread;
    public object ChunkUpdateThreadLock = new object();


    private void Start()
    {

        VoxelData.seed = Random.Range(100000, 999999);
        Debug.Log("Generating New World using seed " + VoxelData.seed);
        //string jsonExport = JsonUtility.ToJson(settings);
        //Debug.Log(jsonExport);

        //File.WriteAllText(Application.dataPath + "/settings.cfg", jsonExport);

        //lock (ChunkUpdateThreadLock) { seed = Random.Range(100000, 999999999); }

        string jsonImport = File.ReadAllText(Application.dataPath + "/settings.cfg");
        settings = JsonUtility.FromJson<Settings>(jsonImport);

        Shader.SetGlobalFloat("minGlobalLightLevel", VoxelData.minLightLevel);
        Shader.SetGlobalFloat("maxGlobalLightLevel", VoxelData.maxLightLevel);

        if (settings.enableThreading)
        {
            ChunkUpdateThread = new Thread(new ThreadStart(ThreadUpdate));
            ChunkUpdateThread.Start();
        }

        SetGlobalLightValue();

        //Random.InitState(seed);
        spanwPosition = new Vector3(VoxelData.worldCentre, VoxelData.ChunkHeight - 50f, VoxelData.worldCentre);
        GenerateWorld();

        playerLastChunkCoord = GetChunkCoordFromVector3(player.position);

    }

    public void SetGlobalLightValue()
    {
        Shader.SetGlobalFloat("GlobalLightLevel", globalLightLevel);
        Camera.main.backgroundColor = Color.Lerp(night, day, globalLightLevel);
    }

    private void Update()
    {
        playerChunkCoord = GetChunkCoordFromVector3(player.position);


        if (!playerChunkCoord.Equals(playerLastChunkCoord))
        {
            CheckViewDistance();
        }

        if(chunksToCreate.Count > 0)
        {
            CreateChunck();
        }

        if(chunksToDraw.Count > 0)
        {
             if (chunksToDraw.Peek().isEditable)
             {
                 chunksToDraw.Dequeue().Createmesh();
             }
        }

        if (!settings.enableThreading)
        {
            if (!applyingModifications)
            {
                ApplyModifications();
            }

            if (chunksToUpdate.Count > 0)
            {
                UpdateChunks();
            }
        }

        if (Input.GetKeyDown(KeyCode.F3))
        {
            debugScreen.SetActive(!debugScreen.activeSelf);
        }
     
    }

    public void GenerateWorld()
    {
        for(int x = (VoxelData.WorldSizeChunks / 2) - settings.viewDistance; x < (VoxelData.WorldSizeChunks / 2) + settings.viewDistance; x++)
        {
            for (int z = (VoxelData.WorldSizeChunks / 2) - settings.viewDistance; z < (VoxelData.WorldSizeChunks / 2) + settings.viewDistance; z++)
            {
                ChunkCoord newChunk = new ChunkCoord(x, z);
                //CreateNewChunk(x, z);
                chunks[x, z] = new Chunk(newChunk, this);
                //activeChunks.Add(new ChunkCoord(x, z));
                //Yield

                chunksToCreate.Add(newChunk);
            }
        }

        player.position = spanwPosition;
        CheckViewDistance();

    }

    public void CreateChunck()
    {
        ChunkCoord c = chunksToCreate[0];
        chunksToCreate.RemoveAt(0);
        //activeChunks.Add(c);
        chunks[c.x, c.z].Init();
    }

    public void UpdateChunks()
    {
        bool updated = false;
        int index = 0;

        lock (ChunkUpdateThreadLock)
        {
            while (!updated && index < chunksToUpdate.Count - 1)
            {
                if (chunksToUpdate[index].isEditable)
                {
                    chunksToUpdate[index].UpdateChunk();
                    if (!activeChunks.Contains(chunksToUpdate[index].coord))
                    {
                        activeChunks.Add(chunksToUpdate[index].coord);
                    }
                    chunksToUpdate.RemoveAt(index);
                    updated = true;
                }
                else
                {
                    index++;
                }
            }
        }
    }

    public void ThreadUpdate()
    {
        while (true)
        {
            if (!applyingModifications)
            {
                ApplyModifications();
            }

            if (chunksToUpdate.Count > 0)
            {
                UpdateChunks();
            }
        }
    }

    private void OnDisable()
    {
        if (settings.enableThreading)
        {
            ChunkUpdateThread.Abort();
        }
    }

    void ApplyModifications()
    {
        applyingModifications = true;

        while (modifications.Count > 0)
        {
            Queue<VoxelMod> queue = modifications.Dequeue();

            while (queue.Count > 0)
            {
                VoxelMod v = queue.Dequeue();

                ChunkCoord c = GetChunkCoordFromVector3(v.position);

                if (chunks[c.x, c.z] == null)
                {
                    chunks[c.x, c.z] = new Chunk(c, this);
                    chunksToCreate.Add(c);
                    //activeChunks.Add(c);
                }

                chunks[c.x, c.z].modifications.Enqueue(v);
                /*if (!chunksToUpdate.Contains(chunks[c.x, c.z]))
                  {
                      chunksToUpdate.Add(chunks[c.x, c.z]);
                  }
                 */

            }

        }

        applyingModifications = false;

    } 
/*
    IEnumerator CreateChunks()
    {
        isCreatingChunks = true;

        while(chunksToCreate.Count > 0)
        {
            chunks[chunksToCreate[0].x, chunksToCreate[0].z].Init();
            chunksToCreate.RemoveAt(0);
            yield return null;
        }

        isCreatingChunks = false;
    }
*/
    ChunkCoord GetChunkCoordFromVector3(Vector3 pos)
    {
        int x = Mathf.FloorToInt(pos.x / VoxelData.ChunkWidth);
        int z = Mathf.FloorToInt(pos.z / VoxelData.ChunkWidth);

        return new ChunkCoord(x, z);
    }

    public Chunk GetChunkFromVector3(Vector3 pos)
    {
        int x = Mathf.FloorToInt(pos.x / VoxelData.ChunkWidth);
        int z = Mathf.FloorToInt(pos.z / VoxelData.ChunkWidth);

        return chunks[x, z];
    }

    public void CheckViewDistance()
    {
        ChunkCoord coord = GetChunkCoordFromVector3(player.position);
        playerLastChunkCoord = playerChunkCoord;

        List<ChunkCoord> previouslyActiveChunks = new List<ChunkCoord>(activeChunks);

        activeChunks.Clear();

        for(int x = coord.x - settings.viewDistance; x < coord.x + settings.viewDistance; x++)
        {
            for (int z = coord.z - settings.viewDistance; z < coord.z + settings.viewDistance; z++)
            {
                ChunkCoord thisChunkCoord = new ChunkCoord(x, z);

                if (IsChunkInWorld(thisChunkCoord))
                {
                    //Debug.Log(x + ", " + z);
                    if(chunks[x, z] == null)
                    {
                        //CreateNewChunk(x, z);
                        chunks[x, z] = new Chunk(thisChunkCoord, this);
                        chunksToCreate.Add(thisChunkCoord);
                    }
                    else if (!chunks[x, z].IsActive)
                    {
                        chunks[x, z].IsActive = true;
                    }
                    activeChunks.Add(thisChunkCoord);
                }

                for (int i = 0; i < previouslyActiveChunks.Count; i++)
                {
                    if (previouslyActiveChunks[i].Equals(thisChunkCoord))
                    {
                        previouslyActiveChunks.RemoveAt(i);
                    }
                }
            }
        }

        foreach(ChunkCoord c in previouslyActiveChunks)
        {
            chunks[c.x, c.z].IsActive = false;
        }
    }

    public bool CheckForVoxel(Vector3 pos)
    {
        ChunkCoord thisChunck = new ChunkCoord(pos);
        if (!IsChunkInWorld(thisChunck) || pos.y < 0 || pos.y > VoxelData.ChunkHeight)
        {
            return false;
        }

        if (chunks[thisChunck.x, thisChunck.z] != null && chunks[thisChunck.x, thisChunck.z].isEditable)
        {
            return blockTypes[chunks[thisChunck.x, thisChunck.z].GetVOxelFromGlobalVector3(pos).id].isSolid;
        }

        return blockTypes[GetVoxel(pos)].isSolid;
    }

    public VoxelState GetVoxelState(Vector3 pos)
    {
        ChunkCoord thisChunck = new ChunkCoord(pos);
        if (!IsChunkInWorld(thisChunck) || pos.y < 0 || pos.y > VoxelData.ChunkHeight)
        {
            return null;
        }

        if (chunks[thisChunck.x, thisChunck.z] != null && chunks[thisChunck.x, thisChunck.z].isEditable)
        {
            return chunks[thisChunck.x, thisChunck.z].GetVOxelFromGlobalVector3(pos);
        }

        return new VoxelState(GetVoxel(pos));
    }

    public bool inUI
    {
        get { return _inUI; }
        set
        {
            _inUI = value;
            if (_inUI)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                
                creativeInventoryWindow.SetActive(true);
                cursorSlot.SetActive(true);
            }
            else
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
                creativeInventoryWindow.SetActive(false);
                cursorSlot.SetActive(false);
            }
        }
    }


    public byte GetVoxel(Vector3 pos)
    {
        int yPos = Mathf.FloorToInt(pos.y);

        /* IMMUTABLE PASS */

        //If outside world, return air
        if (!IsVoxelInWorld(pos))
        {
            return 0;
        }

        //If bottom block of chunk, return bedrock
        if (yPos == 0)
        {
            return 1;
        }

        /* Biome Selection PASS */

        int solidGroundHeight = 42;
        float sumOfHeights = 0f;
        int count = 0;

        float strongestWeight = 0f;
        int strongestBiomeIndex = 0;

        for(int i = 0; i < biomes.Length; i++)
        {
            float weight = Noise.Get2DPerlin(new Vector2(pos.x, pos.z), biomes[i].offset, biomes[i].scale);

            //Keep track of which weight is strongeest.

            if(weight > strongestWeight)
            {
                strongestWeight = weight;
                strongestBiomeIndex = i;
            }

            // Get the height of the terrain (for the current biome) and multiply it by its weight.

            float height = biomes[i].terrainHeight * Noise.Get2DPerlin(new Vector2(pos.x, pos.z), 0, biomes[i].terrainsScale) * weight;

            //If the height value is > 0 add it to the sum of heights.

            if(height > 0)
            {
                sumOfHeights += height;
                count++;
            }

        }

        //Set  biome to the one with the strongest weight.

        BiomeAttributes biome = biomes[strongestBiomeIndex];

        //Get the average of the heights.

        sumOfHeights /= count;
        int terrainHeight = Mathf.FloorToInt(sumOfHeights + solidGroundHeight);


        //BiomeAttributes biome = biomes[index];

        /* BASIC TERRAIN PASS */
        byte voxelValue = 0;



        if (yPos == terrainHeight)
        {
            voxelValue = biome.surfaceBlock;
        }
        else if (yPos < terrainHeight && yPos > terrainHeight - 4)
        {
            voxelValue = biome.subSurfaceBlock;
        }
        else if (yPos > terrainHeight)
        {
            return 0;
        }
        else
        {
            voxelValue = 2;
        }

        /*  SECOND PASS  */

        if(voxelValue == 2)
        {
            foreach (Lode lode in biome.lodes)
            {
                if(yPos > lode.minHeight && yPos < lode.maxHeight)
                {
                    if (Noise.Get3DPerlin(pos, lode.noiseOffset, lode.scale, lode.threShould))
                    {
                        voxelValue = lode.blockID;
                    }
                }
            }
        }

        /* TREE PASS */

        if (yPos == terrainHeight && biome.placeMajorFlora)
        {
            if (Noise.Get2DPerlin(new Vector3(pos.x, pos.z), 0, biome.majorFloraZoneScale) > biome.majorFloraZoneThreshould) 
            {
                //voxelValue = 3;
                if (Noise.Get2DPerlin(new Vector3(pos.x, pos.z), 0, biome.majorFloraPlacementScale) > biome.majorFloraPlacementThreshould)
                {
                    //voxelValue = 5;
                    modifications.Enqueue(Structure.GenerateMajorFlora(biome.majorFloraIndex, pos, biome.minHeight, biome.maxHeight));
                }
            }
        }

        return voxelValue;

    }

/*    public void CreateNewChunk(int x, int z)
    {
        chunks[x, z] = new Chunk(new ChunkCoord(x, z), this);
        activeChunks.Add(new ChunkCoord(x, z));
    }
*/

    public bool IsChunkInWorld(ChunkCoord coord)
    {
        if (coord.x > 0 && coord.x < VoxelData.WorldSizeChunks - 1 && coord.z > 0 && coord.z <  VoxelData.WorldSizeChunks - 1)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public bool IsVoxelInWorld(Vector3 pos)
    {
        if (pos.x >= 0 && pos.x < VoxelData.WorldSizeInVoxels && pos.y >= 0 && pos.y < VoxelData.ChunkHeight && pos.z >= 0 && pos.z < VoxelData.WorldSizeInVoxels)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
}

//Memory buffer, for block generation//
[System.Serializable]
public class BlockType
{

    //Back, Front, Top, Bottom, Left, Right

    [Header ("Texture values")]


    public string blockName;
    public bool isSolid;

    public bool renderNeighborFaces;
    public float transparency;

    public Sprite icon;



    public int backFaceTexture;
    public int frontFaceTexture;
    public int topFaceTexture;
    public int bottomFaceTexture;
    public int leftFaceTexture;
    public int rightFaceTexture;

    public int GetTextureID(int faceIndex)
    {
        switch (faceIndex)
        {
            case 0:
                return backFaceTexture;
            case 1:
                return frontFaceTexture;
            case 2:
                return topFaceTexture;
            case 3:
                return bottomFaceTexture;
            case 4:
                return leftFaceTexture;
            case 5:
                return rightFaceTexture;
            default:
                Debug.Log("ERROR IN getTextureID; {Invalid face index} ERROR IN LINE 35 OF World.cs");
                return 0;
        }
    }
}

public class VoxelMod
{
    public Vector3 position;
    public byte id;
    
    public VoxelMod()
    {
        position = new Vector3();
        id = 0;
    }

    public VoxelMod (Vector3 _position, byte _id)
    {
        position = _position;
        id = _id;
    }
}

[System.Serializable]
public class Settings
{
    [Header("Game Data")]
    public string version = "0.0.0.01";

    [Header("Performance")]
    public int viewDistance = 5;
    public bool enableThreading = true;
    public bool enableAnimatedChunks = true;

    [Header("Controls")]
    [Range(0.1f, 10f)]
    public float mouseSensitivity = 3.0f;
}




/*
 * 
 * 
 *         int xCheck = Mathf.FloorToInt(_x);
        int yCheck = Mathf.FloorToInt(_y);
        int zCheck = Mathf.FloorToInt(_z);

        int xChunk = xCheck / VoxelData.ChunkWidth;
        int zChunk = zCheck / VoxelData.ChunkWidth;

        xCheck -= (xChunk * VoxelData.ChunkWidth);
        zCheck -= (zChunk * VoxelData.ChunkWidth);

        return blockTypes[chunks[xChunk, zChunk].voxelMap[xCheck, yCheck, zCheck]].isSolid;
 */
