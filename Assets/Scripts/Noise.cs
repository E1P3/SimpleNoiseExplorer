using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//just a placeholder noise function made by this guy -> https://github.com/SebLague/Procedural-Landmass-Generation
public static class Noise
{

	public static float[] GenerateNoisemap(int chunkx, int chunky, int chunkSize, float magnitude, float frequency, float layers, float seed) {



        float[] noiseMap = new float[Mathf.RoundToInt(Mathf.Pow(chunkSize+1, 2))];

		int offsetx = chunkx * chunkSize;
		int offsety = chunky * chunkSize;
        float noise = 0;

        for (int i = 0, y = 0; y <= chunkSize; y++)
        {
            for (int x = 0; x <= chunkSize; x++, i++)
            {
                float newx = (offsetx + seed + x)/frequency;
                float newy = (offsety + seed + y)/frequency;

                for (int j = 1; j <= layers; j++) {
                    //noise += (1 - Mathf.Abs((Mathf.PerlinNoise((seed + offsetx + x) * j / frequency, (seed + offsety + y) * j / frequency) * 2 - 1) * magnitude * j)) * (1/j);

                    noise = (noise + j)*((Mathf.PerlinNoise(newx, newy)*2 - 1)* j) * Mathf.Pow(-1,j);
                    newx *= 2;
                    newy *= 2;
                }

                int factorial = 0;

                for (int d = 0; d <= layers; d++) {
                    factorial += d;
                }


                noiseMap[i] = (noise - factorial)*magnitude;
                noise = 0;
            }
        }

		return noiseMap;
	}

}

