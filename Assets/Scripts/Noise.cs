using UnityEngine;
using Random = System.Random;

public static class Noise
{
    public enum NoiseAlgorithmImplementation
    {
        BuiltIn,
        MyImplementation
    }

    private static int[] _permutationTable =
    {
        151, 160, 137, 91, 90, 15, 131, 13, 201, 95, 96, 53, 194, 233, 7, 225, 140, 36, 103, 30, 69, 142, 8, 99, 37,
        240, 21, 10, 23, 190, 6, 148, 247, 120, 234, 75, 0, 26, 197, 62, 94, 252, 219, 203, 117, 35, 11, 32, 57, 177,
        33, 88, 237, 149, 56, 87, 174, 20, 125, 136, 171, 168, 68, 175, 74, 165, 71, 134, 139, 48, 27, 166, 77, 146,
        158, 231, 83, 111, 229, 122, 60, 211, 133, 230, 220, 105, 92, 41, 55, 46, 245, 40, 244, 102, 143, 54, 65, 25,
        63, 161, 1, 216, 80, 73, 209, 76, 132, 187, 208, 89, 18, 169, 200, 196, 135, 130, 116, 188, 159, 86, 164, 100,
        109, 198, 173, 186, 3, 64, 52, 217, 226, 250, 124, 123, 5, 202, 38, 147, 118, 126, 255, 82, 85, 212, 207, 206,
        59, 227, 47, 16, 58, 17, 182, 189, 28, 42, 223, 183, 170, 213, 119, 248, 152, 2, 44, 154, 163, 70, 221, 153,
        101, 155, 167, 43, 172, 9, 129, 22, 39, 253, 19, 98, 108, 110, 79, 113, 224, 232, 178, 185, 112, 104, 218, 246,
        97, 228, 251, 34, 242, 193, 238, 210, 144, 12, 191, 179, 162, 241, 81, 51, 145, 235, 249, 14, 239, 107, 49, 192,
        214, 31, 181, 199, 106, 157, 184, 84, 204, 176, 115, 121, 50, 45, 127, 4, 150, 254, 138, 236, 205, 93, 222, 114,
        67, 29, 24, 72, 243, 141, 128, 195, 78, 66, 215, 61, 156, 180
    };

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
        int left = Mathf.FloorToInt(x);
        int top = Mathf.FloorToInt(y);
        float pointInQuadX = x - left;
        float pointInQuadY = y - top;

        Vector2 topLeftGradient = GetPseudoRandomGradientVector(left, top);
        Vector2 topRightGradient = GetPseudoRandomGradientVector(left + 1, top);
        Vector2 bottomLeftGradient = GetPseudoRandomGradientVector(left, top + 1);
        Vector2 bottomRightGradient = GetPseudoRandomGradientVector(left + 1, top + 1);

        Vector2 distanceToTopLeft = new Vector2(pointInQuadX, pointInQuadY);
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

    private static Vector2 GetPseudoRandomGradientVector(int x, int y)
    {
        int v = (int)(((x * 1836311903) ^ y * 2971215073 + 4807526976) & (_permutationTable.Length - 1));
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
}