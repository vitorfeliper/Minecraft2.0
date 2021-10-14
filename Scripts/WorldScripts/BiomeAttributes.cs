using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "BiomeAttributes", menuName = "MINECRAFT2.0_BY_GANJGAMES_STUDIOS/Biome Attribute")]
public class BiomeAttributes : ScriptableObject
{
    [Header("NA...")]
    public string biomeName;
    public int offset;
    public float scale;

    //public int solidGroundHeight;
    public int terrainHeight;
    public float terrainsScale;

    public byte surfaceBlock;
    public byte subSurfaceBlock;

    [Header("Major Flora")]
    public int majorFloraIndex;

    public float majorFloraZoneScale = 1.3f;
    [Range(0.1f, 2f)]
    public float majorFloraZoneThreshould = 0.6f;
    public float majorFloraPlacementScale = 15f;
    [Range(0.1f, 2f)]
    public float majorFloraPlacementThreshould = 0.8f;
    public bool placeMajorFlora = true;

    public int maxHeight = 12;
    public int minHeight = 5;

    public Lode[] lodes;
}

[System.Serializable]
public class Lode
{
    public string nodeName;
    public byte blockID;
    public int minHeight;
    public int maxHeight;
    public float scale;
    public float threShould;
    public float noiseOffset;
}
