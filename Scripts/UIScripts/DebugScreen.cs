using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DebugScreen : MonoBehaviour
{
    World world;
    Text text;

    float frameRate;
    float timer;

    int halfWorldSizeInVoxels;
    int halfWorldSizeInChunks;

    private void Start()
    {
        world = GameObject.Find("World").GetComponent<World>();
        text = GetComponent<Text>();

        halfWorldSizeInChunks = VoxelData.WorldSizeChunks / 2;
        halfWorldSizeInVoxels = VoxelData.WorldSizeInVoxels / 2;
    }

    private void Update()
    {
        string debugText = "GANJGAME_STUDIOS ' MINECRAFT2.0 PROGRAMED BY VITORDEV";
        debugText += "\nMADE IN UNITY ENGINE";
        debugText += "\n";
        debugText += frameRate + " FPS";
        debugText += "\n";
        debugText += "API: " + SystemInfo.graphicsDeviceType;
        debugText += "\n";
        debugText += "Specs: " + SystemInfo.processorType + "\nOperation System: " + SystemInfo.operatingSystem + "\nGraphics Memory Size: " + SystemInfo.graphicsMemorySize + "\nSystem Memory Size: " + SystemInfo.systemMemorySize;
        debugText += "\n\n";
        debugText += "XYZ: " + (Mathf.FloorToInt(world.player.transform.position.x) - halfWorldSizeInVoxels) + " / " + (Mathf.FloorToInt(world.player.transform.position.y) + " / " + (Mathf.FloorToInt(world.player.transform.position.z) - halfWorldSizeInVoxels));
        debugText += "\n";
        debugText += "Chunk: " + (world.playerLastChunkCoord.x - halfWorldSizeInChunks) + " / " + (world.playerLastChunkCoord.z - halfWorldSizeInChunks);
        debugText += "\n";
        debugText += "Seed: " + VoxelData.seed;




        text.text = debugText;

        if (timer > 1f)
        {
            frameRate = (int)(1f / Time.unscaledDeltaTime);
            timer = 0;
        }
        else
        {
            timer += Time.deltaTime;
        }
    }
}
