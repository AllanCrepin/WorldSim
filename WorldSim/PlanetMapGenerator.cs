using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static FastNoiseLite;

namespace WorldSim
{
    public class PlanetMapGenerator
    {
        public float[] temperatureData;
        public float[] humidityData;
        public float[] heightData;

        public Texture2D GeneratePlanet(GraphicsDevice graphicsDevice, int width, int height)
        {
            Random rnd = new Random();
            var noise = new FastNoiseLite(rnd.Next());
            noise.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2);
            noise.SetFrequency(1.0f);

            Color[] colorData = new Color[width * height];

            temperatureData = new float[width * height];
            humidityData = new float[width * height];
            heightData = new float[width * height];

            GenerateTemperature(temperatureData, noise, rnd, width, height);
            GenerateHumidity(humidityData, noise, rnd, width, height);

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    float nx = (float)x / width;
                    float ny = (float)y / height;

                    float noiseValue = GetOctaveNoise(noise, nx * width, ny * height, 0.0020f, 7, 2.0f, 0.60f);
                    heightData[y * width + x] = noiseValue;
                }
            }

            ColorizeMap(width, height, heightData, temperatureData, humidityData, colorData);

            Texture2D texture = new Texture2D(graphicsDevice, width, height);
            texture.SetData(colorData);

            return texture;
        }

        public void GenerateTemperature(float[] data, FastNoiseLite noise, Random rnd, int width, int height)
        {
            noise.SetSeed(rnd.Next());

            float[,] dataPoints = {
                { 0.0f, -50.0f},
                { 0.1f,  -15.0f},
                { 0.2f,  0.0f},
                { 0.5f,  30.0f},
                { 0.8f,  0.0f},
                { 0.9f,  -15.0f},
                { 1.0f, -50.0f}
            };

            int numPoints = dataPoints.GetLength(0);

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    float nx = (float)x / width;
                    float ny = (float)y / height;

                    float normalizedLatitude = ReRangeFloatToFloat((float)y, 0.0f, (float)height, 0.0f, 1.0f);
                    float temperature = LinearInterpolation(normalizedLatitude, dataPoints, numPoints);
                    float normalizedTemp = ReRangeFloatToFloat(temperature, -50.0f, 30.0f, -1.0f, 1.0f);

                    float noisedTemp = normalizedTemp + (GetOctaveNoise(noise, nx * width, ny * height, 0.0030f, 6, 1.9f, 0.75f) * 0.22f);

                    data[y * width + x] = noisedTemp;
                }
            }
        }

        public void GenerateHumidity(float[] data, FastNoiseLite noise, Random rnd, int width, int height)
        {
            noise.SetSeed(rnd.Next());

            float[,] dataPoints = {
                {0.0f,  5.0f},
                {0.15f,  75.0f},
                {0.30f, 10.0f},
                {0.35f, 00.0f},
                {0.40f, 30.0f},
                {0.45f, 35.0f},
                {0.48f, 130.0f},
                {0.5f,  160.0f}, // equator
                {0.52f, 130.0f},
                {0.55f, 35.0f},
                {0.60f, 30.0f},
                {0.65f, 00.0f},
                {0.70f, 10.0f},
                {0.85f,  75.0f},
                {1.0f,  5.0f}
            };

            int numPoints = dataPoints.GetLength(0);

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    float nx = (float)x / width;
                    float ny = (float)y / height;

                    float normalizedLatitude = ReRangeFloatToFloat((float)y, 0.0f, (float)height, 0.0f, 1.0f);
                    float humidity = LinearInterpolation(normalizedLatitude, dataPoints, numPoints);
                    float normalizedHum = ReRangeFloatToFloat(humidity, 0.0f, 160.0f, -1.0f, 1.0f);

                    float noisedHum = normalizedHum + (GetOctaveNoise(noise, nx * width, ny * height, 0.0020f, 7, 2f, 0.6f) * 0.40f);

                    data[y * width + x] = noisedHum;
                }
            }
        }

        public float ReRangeFloatToFloat(float value, float oldMin, float oldMax, float newMin, float newMax)
        {
            float oldRange = oldMax - oldMin;
            float newRange = newMax - newMin;
            return ((value - oldMin) * newRange / oldRange) + newMin;
        }

        public float LinearInterpolation(float x, float[,] dataPoints, int numPoints)
        {
            int i;
            for (i = 0; i < numPoints - 1; ++i)
            {
                if (x <= dataPoints[i + 1, 0])
                {
                    break;
                }
            }

            float x0 = dataPoints[i, 0];
            float y0 = dataPoints[i, 1];
            float x1 = dataPoints[i + 1, 0];
            float y1 = dataPoints[i + 1, 1];

            return y0 + (x - x0) * (y1 - y0) / (x1 - x0);
        }

        float GetOctaveNoise(FastNoiseLite noise, float x, float y, float baseFrequency, int octaves, float lacunarity, float gain)
        {
            float frequency = baseFrequency;
            float amplitude = 1.0f;
            float total = 0.0f;

            for (int i = 0; i < octaves; i++)
            {
                total += noise.GetNoise(x * frequency, y * frequency) * amplitude;
                frequency *= lacunarity;
                amplitude *= gain;
            }

            return total;
        }

        public void ColorizeMap(int mapWidth, int mapHeight, float[] elevationMap, float[] temperatureMap, float[] humidityMap, Color[] buffer)
        {
            Random rnd = new Random();
            var noise = new FastNoiseLite(rnd.Next());
            noise.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2);
            noise.SetFrequency(1.0f);

            Color gMapsPrairie = new Color(192, 238, 217);
            Color gMapsLightwoods = new Color(174, 233, 206);
            Color gMapsForest = new Color(153, 228, 193);
            Color gMapsArid = new Color(245, 240, 228);
            Color gMapsDesert = new Color(251, 248, 243);
            Color gMapsColdGrey = new Color(204, 208, 203);
            Color gMapsColdBeige = new Color(245, 240, 228);
            Color gMapsSnow = Color.White;
            Color gMapsWater = new Color(119, 212, 232);

            // Base terrain: water and land
            for (int j = 0; j < mapHeight; j++)
            {
                for (int i = 0; i < mapWidth; i++)
                {
                    int index = j * mapWidth + i;

                    if (elevationMap[index] > 0.0f)
                    {
                        buffer[index] = gMapsPrairie;
                    }
                    else if (elevationMap[index] < 0.0f)
                    {
                        buffer[index] = gMapsWater;
                    }

                    // Snow on high elevations and cold regions
                    if (elevationMap[index] > -0.1f && temperatureMap[index] < -0.4f)
                    {
                        buffer[index] = gMapsSnow;
                    }
                }
            }

            // Cold-grey regions
            for (int j = 0; j < mapHeight; j++)
            {
                for (int i = 0; i < mapWidth; i++)
                {
                    int index = j * mapWidth + i;

                    if (elevationMap[index] > 0.0f && temperatureMap[index] > -0.4f && temperatureMap[index] < -0.12f)
                    {
                        buffer[index] = gMapsColdGrey;
                    }
                }
            }

            // grassy patches in cold-grey regions
            noise.SetSeed(rnd.Next());
            for (int j = 0; j < mapHeight; j++)
            {
                for (int i = 0; i < mapWidth; i++)
                {
                    int index = j * mapWidth + i;
                    float nx = (float)i / mapWidth;
                    float ny = (float)j / mapHeight;

                    float noiseValue = GetOctaveNoise(noise, nx * mapWidth, ny * mapHeight, 0.012f, 4, 2.0f, 0.90f);

                    if (noiseValue > 0.5f && elevationMap[index] > 0.0f && temperatureMap[index] > -0.25
                        && temperatureMap[index] < -0.12f && buffer[index] == gMapsColdGrey)
                    {
                        buffer[index] = gMapsPrairie;
                    }
                }
            }

            // Add beige spots near coastlines in cold regions
            noise.SetSeed(rnd.Next());
            for (int j = 0; j < mapHeight; j++)
            {
                for (int i = 0; i < mapWidth; i++)
                {
                    int index = j * mapWidth + i;
                    float nx = (float)i / mapWidth;
                    float ny = (float)j / mapHeight;

                    float noiseValue = GetOctaveNoise(noise, nx * mapWidth, ny * mapHeight, 0.052f, 2, 2.1f, 0.95f);

                    if (noiseValue > 0.5f && elevationMap[index] > 0.0f && elevationMap[index] < 0.3f
                        && temperatureMap[index] > -0.30 && temperatureMap[index] < -0.05f)
                    {
                        buffer[index] = gMapsColdBeige;
                    }
                }
            }

            // Forests in temperate regions
            for (int j = 0; j < mapHeight; j++)
            {
                for (int i = 0; i < mapWidth; i++)
                {
                    int index = j * mapWidth + i;

                    if (elevationMap[index] > 0.0f && temperatureMap[index] > 0.08
                        && temperatureMap[index] < 0.40f && humidityMap[index] > -0.3f)
                    {
                        buffer[index] = gMapsLightwoods;
                    }
                }
            }

            // Dense forests in suitable regions
            noise.SetSeed(rnd.Next());
            for (int j = 0; j < mapHeight; j++)
            {
                for (int i = 0; i < mapWidth; i++)
                {
                    int index = j * mapWidth + i;
                    float nx = (float)i / mapWidth;
                    float ny = (float)j / mapHeight;

                    float noiseValue = GetOctaveNoise(noise, nx * mapWidth, ny * mapHeight, 0.002f, 6, 2f, 0.90f);

                    if (noiseValue > 0.0f && elevationMap[index] > 0.0f && temperatureMap[index] > 0.12
                        && temperatureMap[index] < 0.33f && humidityMap[index] > -0.3f)
                    {
                        buffer[index] = gMapsForest;
                    }
                }
            }

            // Arid and desert regions
            noise.SetSeed(rnd.Next());
            for (int j = 0; j < mapHeight; j++)
            {
                for (int i = 0; i < mapWidth; i++)
                {
                    int index = j * mapWidth + i;
                    float nx = (float)i / mapWidth;
                    float ny = (float)j / mapHeight;

                    float noiseValue = GetOctaveNoise(noise, nx * mapWidth, ny * mapHeight, 0.015f, 6, 2f, 0.60f);
                    float largeChunk = GetOctaveNoise(noise, nx * mapWidth, ny * mapHeight, 0.002f, 7, 2.0f, 0.50f);

                    if (elevationMap[index] > 0f && humidityMap[index] < -0.45f
                        && temperatureMap[index] > 0.25f && largeChunk < 0.6f)
                    {
                        buffer[index] = gMapsArid;
                    }

                    if (elevationMap[index] > 0.0f && humidityMap[index] < -0.3f
                        && temperatureMap[index] > 0.25f && noiseValue > 0.6f)
                    {
                        buffer[index] = gMapsArid;
                    }

                    if (elevationMap[index] > 0f && humidityMap[index] < -0.55f
                        && temperatureMap[index] > 0.25f && largeChunk < 0.5f)
                    {
                        buffer[index] = gMapsDesert;
                    }
                }
            }

            // Tropical regions
            for (int j = 0; j < mapHeight; j++)
            {
                for (int i = 0; i < mapWidth; i++)
                {
                    int index = j * mapWidth + i;

                    if (elevationMap[index] > 0.0f && humidityMap[index] > 0.9f)
                    {
                        buffer[index] = gMapsLightwoods;
                    }

                    if (elevationMap[index] > 0.0f && humidityMap[index] > -0.2f && temperatureMap[index] > 0.95f)
                    {
                        buffer[index] = gMapsLightwoods;
                    }

                    if (elevationMap[index] > 0.0f && temperatureMap[index] > 1.2f)
                    {
                        buffer[index] = gMapsForest;
                    }
                }
            }

            // Mountains with elevation-based coloring
            noise.SetSeed(rnd.Next());
            for (int j = 0; j < mapHeight; j++)
            {
                for (int i = 0; i < mapWidth; i++)
                {
                    int index = j * mapWidth + i;
                    float nx = (float)i / mapWidth;
                    float ny = (float)j / mapHeight;

                    float noiseValue = GetOctaveNoise(noise, nx * mapWidth, ny * mapHeight, 0.002f, 7, 2f, 0.90f);
                    float alteredElevation = elevationMap[index] + 0.2f;

                    if (elevationMap[index] > 0 && (alteredElevation * noiseValue) > 1.2 && temperatureMap[index] > 0)
                    {
                        buffer[index] = gMapsColdGrey;
                    }

                    if (elevationMap[index] > 0 && (elevationMap[index] * noiseValue) > 2.8)
                    {
                        buffer[index] = Color.White;
                    }
                }
            }
        }

        public Color Blend(Color color1, Color color2, float factor)
        {
            factor = MathHelper.Clamp(factor, 0f, 1f);

            int r = MathHelper.Clamp((int)(color1.R + (color2.R - color1.R) * factor), 0, 255);
            int g = MathHelper.Clamp((int)(color1.G + (color2.G - color1.G) * factor), 0, 255);
            int b = MathHelper.Clamp((int)(color1.B + (color2.B - color1.B) * factor), 0, 255);
            int a = MathHelper.Clamp((int)(color1.A + (color2.A - color1.A) * factor), 0, 255);

            return new Color(r, g, b, a);
        }

        public Color Darken(Color color, float factor)
        {
            factor = MathHelper.Clamp(factor, 0f, 1f);

            int r = MathHelper.Clamp((int)(color.R * (1 - factor)), 0, 255);
            int g = MathHelper.Clamp((int)(color.G * (1 - factor)), 0, 255);
            int b = MathHelper.Clamp((int)(color.B * (1 - factor)), 0, 255);

            return new Color(r, g, b, color.A);
        }
    }
}