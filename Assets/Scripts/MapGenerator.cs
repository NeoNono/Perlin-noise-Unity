using System;
using UnityEngine;
using UnityEngine.Serialization;

public class MapGenerator : MonoBehaviour
{
    private enum DrawMode
    {
        NoiseMap, 
        ColorMap,
        Mesh
    }
    
    private const int MapChunkSize = 241;
    
    [Header("Main settings")]
    [SerializeField] private float noiseScale;
    [SerializeField] private int octaves;
    [Range(0f, 1f)]
    [SerializeField] private float persistance;
    [SerializeField] private float lacunarity;
    [SerializeField] private Noise.NoiseAlgorithmImplementation implementation;
    public bool autoUpdate = true;

    [Header("Mesh settings")]
    [Range(0, 6)]
    [SerializeField] private int levelOfDetail;
    [SerializeField] private float meshHeightMultiplier;
    [SerializeField] private AnimationCurve meshHeightCurve;

    [Header("Randomness settings")]
    [SerializeField] private int seed;
    [SerializeField] private Vector2 offset;

    [Header("Drawing settings")]
    [SerializeField] private TerrainType[] regions;
    [SerializeField] private DrawMode drawMode;
    
    public void GenerateMap()
    {
        float[,] noiseMap = Noise.GenerateNoiseMap(MapChunkSize, MapChunkSize, seed, noiseScale, octaves, persistance, lacunarity, offset, implementation);

        Color[] colorMap = GenerateColorMap(noiseMap);

        MapDisplay display = FindObjectOfType<MapDisplay>();

        switch (drawMode)
        {
            case DrawMode.NoiseMap:
                display.DrawTexture(TextureGenerator.TextureFromHeightMap(noiseMap));
                break;
            case DrawMode.ColorMap:
                display.DrawTexture(TextureGenerator.TextureFromColorMap(colorMap, MapChunkSize, MapChunkSize));
                break;
            case DrawMode.Mesh:
                display.DrawMesh(MeshGenerator.GenerateTerrainMesh(noiseMap, meshHeightMultiplier, meshHeightCurve, levelOfDetail), TextureGenerator.TextureFromColorMap(colorMap, MapChunkSize, MapChunkSize));
                break;
        }
    }
    
    private void OnValidate()
    {
        if (noiseScale < 0.1f)
            noiseScale = 0.1f;
        if (octaves < 0)
            octaves = 0;
        if (octaves > 29)
            octaves = 29;
        if (lacunarity < 1)
            lacunarity = 1;
    }
    
    private Color[] GenerateColorMap(float[,] noiseMap)
    {
        Color[] colorMap = new Color[MapChunkSize * MapChunkSize];
        for (int y = 0; y < MapChunkSize; y++)
        {
            for (int x = 0; x < MapChunkSize; x++)
            {
                float currentHeight = noiseMap[x, y];
                for (int i = 0; i < regions.Length; i++)
                {
                    if (currentHeight <= regions[i].height)
                    {
                        colorMap[y * MapChunkSize + x] = regions[i].color;
                        break;
                    }
                }
            }
        }

        return colorMap;
    }
}

[Serializable]
public struct TerrainType
{
    public string name;
    public float height;
    public Color color;
}