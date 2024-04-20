using UnityEngine;
using Random = System.Random;

public static class Noise
{
    public enum NoiseAlgorithmImplementation
    {
        BuiltIn,
        MyImplementation
    }

    private static Perlin2D _perlin2D = new Perlin2D();
    
    public static float[,] GenerateNoiseMap(int mapWidth, int mapHeight, int seed, float scale,
        int octaves, float persistence, float lacunarity, Vector2 offset, NoiseAlgorithmImplementation implementation)
    {
        float[,] noiseMap = new float[mapWidth, mapHeight];

        Random prng = new Random(seed);
        Vector2[] octaveOffsets = new Vector2[octaves];
        for (int i = 0; i < octaves; i++)
        {
            float offsetX = prng.Next(-100000, 100000) + offset.x;
            float offsetY = prng.Next(-100000, 100000) + offset.y;
            octaveOffsets[i] = new Vector2(offsetX, offsetY);
        }

        float maxNoiseHeight = float.MinValue;
        float minNoiseHeight = float.MaxValue;

        float halfWidth = mapWidth / 2f;
        float halfHeight = mapWidth / 2f;

        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                float amplitude = 1f;
                float frequency = 1f;
                float noiseHeight = 0f;

                for (int i = 0; i < octaves; i++)
                {
                    float sampleX = (x - halfWidth) / scale * frequency + octaveOffsets[i].x;
                    float sampleY = (y - halfHeight) / scale * frequency + octaveOffsets[i].y;
                    
                    float perlinValue = implementation switch
                    {
                        NoiseAlgorithmImplementation.BuiltIn => Mathf.PerlinNoise(sampleX, sampleY) * 2 - 1,
                        NoiseAlgorithmImplementation.MyImplementation => MyPerlinNoise(sampleX, sampleY),
                        _ => 0
                    };

                    noiseHeight += perlinValue * amplitude;

                    amplitude *= persistence;
                    frequency *= lacunarity;
                }

                if (noiseHeight > maxNoiseHeight)
                    maxNoiseHeight = noiseHeight;
                else if (noiseHeight < minNoiseHeight)
                    minNoiseHeight = noiseHeight;

                noiseMap[x, y] = noiseHeight;
            }
        }

        NormalizeNoiseMap(ref noiseMap, minNoiseHeight, maxNoiseHeight);

        return noiseMap;
    }

    private static void NormalizeNoiseMap(ref float[,] noiseMap, float minNoiseHeight, float maxNoiseHeight)
    {
        for (int y = 0; y < noiseMap.GetLength(1); y++)
        {
            for (int x = 0; x < noiseMap.GetLength(0); x++)
            {
                noiseMap[x, y] = Mathf.InverseLerp(minNoiseHeight, maxNoiseHeight, noiseMap[x, y]);
            }
        }
    }

    private static float MyPerlinNoise(float x, float y)
    {
        return Perlin2D.Noise(x, y);
    }

    private class Perlin2D
    {
        private static byte[] _permutationTable;

        public Perlin2D(int seed = 0)
        {
            var rand = new Random(seed);
            _permutationTable = new byte[256];
            rand.NextBytes(_permutationTable);
        }

        private static Vector2 GetPseudoRandomGradientVector(int x, int y)
        {
            int v = (int)(((x * 1836311903) ^ y * 2971215073 + 4807526976) & 255);
            v = _permutationTable[v] & 3;

            return v switch
            {
                0 => Vector2.right,
                1 => Vector2.left,
                2 => Vector2.up,
                _ => Vector2.down
            };
        }

        private static float QuinticCurve(float t)
        {
            return ((t * 6 - 15) * t + 10) * t * t * t;
        }
        
        public static float Noise(float fx, float fy)
        {
            int left = Mathf.FloorToInt(fx);
            int top = Mathf.FloorToInt(fy);
            float pointInQuadX = fx - left;
            float pointInQuadY = fy - top;

            Vector2 topLeftGradient = GetPseudoRandomGradientVector(left, top);
            Vector2 topRightGradient = GetPseudoRandomGradientVector(left + 1, top);
            Vector2 bottomLeftGradient = GetPseudoRandomGradientVector(left, top + 1);
            Vector2 bottomRightGradient = GetPseudoRandomGradientVector(left + 1, top + 1);

            Vector2 distanceToTopLeft = new Vector2( pointInQuadX, pointInQuadY);
            Vector2 distanceToTopRight = new Vector2(pointInQuadX - 1, pointInQuadY);
            Vector2 distanceToBottomLeft = new Vector2(pointInQuadX, pointInQuadY - 1);
            Vector2 distanceToBottomRight = new Vector2(pointInQuadX - 1, pointInQuadY - 1);

            float tx1 = Vector2.Dot(distanceToTopLeft, topLeftGradient);
            float tx2 = Vector2.Dot(distanceToTopRight, topRightGradient);
            float bx1 = Vector2.Dot(distanceToBottomLeft, bottomLeftGradient);
            float bx2 = Vector2.Dot(distanceToBottomRight, bottomRightGradient);

            pointInQuadX = QuinticCurve(pointInQuadX);
            pointInQuadY = QuinticCurve(pointInQuadY);

            float tx = Mathf.Lerp(tx1, tx2, pointInQuadX);
            float bx = Mathf.Lerp(bx1, bx2, pointInQuadX);
            float tb = Mathf.Lerp(tx, bx, pointInQuadY);

            return tb;
        }
    }
}