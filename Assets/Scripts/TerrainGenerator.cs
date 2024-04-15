using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class TerrainGenerator : MonoBehaviour
{
    [SerializeField] private int depth = 20;
    [SerializeField] private int width = 256;
    [SerializeField] private int height = 256;
    [SerializeField] private float scale = 20f;

    [SerializeField] private float offsetX;
    [SerializeField] private float offsetY;

    private void Start()
    {
        offsetX = Random.Range(0f, 9999f);
        offsetY = Random.Range(0f, 9999f);
    }

    private void Update()
    {
        Terrain terrain = GetComponent<Terrain>();

        GenerateTerrain(ref terrain);

        offsetX += Time.deltaTime * 5f;
    }

    private void GenerateTerrain(ref Terrain terrain)
    {
        var terrainData = terrain.terrainData;
        
        terrainData.heightmapResolution = width + 1;
        terrainData.size = new Vector3(width, depth, height);
        terrainData.SetHeights(0, 0, GenerateHeights());
    }

    private float[,] GenerateHeights()
    {
        float[,] heights = new float[width, height];

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                heights[x, y] = CalculateHeight(x, y);
            }
        }

        return heights;
    }

    private float CalculateHeight(int x, int y)
    {
        float xCoord = x * scale / width + offsetX;
        float yCoord = y * scale / width + offsetY;

        return Mathf.PerlinNoise(xCoord, yCoord);
    }
}
