#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UIElements;
using System;
using System.Reflection;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class MapDisplay : MonoBehaviour
{
    public enum DrawMode {NoiseMap, ColourMap, Mesh, CityMap}
    public DrawMode drawMode;
    public int mapWidth;
    public int mapHeight;


    public float noiseScale;
    public int octaves;
    [Range(0,1)]
    public float persistance;
    public float lacunarity;

    public int seed;
    public Vector2 offsets;

    public bool autoUpdate;

    public Renderer textureRender;

    public TerrainType[] regions;

    public AnimationCurve heightCurve;

    public void GenerateMap()
    {
        TileBuilder tile = new TileBuilder();
        
        //tile.GenerateHeightMap(mapWidth, mapHeight, noiseScale, octaves, persistance, lacunarity, offsets);
        float[,] noiseMap = GenerateCityNoiseMap(mapWidth,mapHeight,offsets);

        Texture2D texture = new Texture2D(mapWidth, mapHeight);

        Color[] noiseMapC = new Color[mapWidth * mapHeight];
        Color[] colourMapC = new Color[mapWidth * mapHeight];
        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                float currentHeight = noiseMap[x, y];
                for(int i =0; i< regions.Length; i++)
                {
                    if(currentHeight <= regions[i].height)
                    {
                        colourMapC[y * mapWidth + x] = regions[i].color;
                        break;
                    }
                }
                noiseMapC[y * mapWidth + x] = Color.Lerp(Color.black, Color.white, noiseMap[x, y]);
            }
        }
        if(drawMode == DrawMode.NoiseMap) texture.SetPixels(noiseMapC);
        if(drawMode == DrawMode.ColourMap) texture.SetPixels(colourMapC);
        // Prevents blurryness and makes the colours hold on to points
        texture.filterMode = FilterMode.Point;
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.Apply();
        textureRender.sharedMaterial.mainTexture = texture;
        textureRender.transform.localScale = new Vector3(mapWidth, 1, mapHeight);
    }

    public void CopyCurve()
    {
        string result = "this.heightCurve = new AnimationCurve(";
        string split = "f, ";

        foreach (Keyframe keyframe in heightCurve.keys)
        {
            result += "new Keyframe(" + keyframe.time + split + keyframe.value;

            result += split + keyframe.inTangent + split + keyframe.outTangent;

            result += split + keyframe.inWeight + split + keyframe.outWeight;

            result += "f),";
        }

        result = result.Remove(result.Length - 1, 1);
        result += ");";
        Debug.Log(result);
        //EditorGUIUtility.systemCopyBuffer = result;
    }

    public void GetCurve()
    {
        ///string animationCurveString = EditorGUIUtility.systemCopyBuffer.Trim();

       /* if (animationCurveString.Contains("AnimationCurve"))
        {
            // Strip and make animation curve
            String[] sep = { "AnimationCurve(", ");" };
            animationCurveString = animationCurveString.Split(sep, 3, StringSplitOptions.None)[1].Trim();

            // Regex the values of the key frames
            Regex rgx = new Regex("-?[0-9]\\.?[0-9]*");
            MatchCollection matches = rgx.Matches(animationCurveString);
            Keyframe[] keys = new Keyframe[matches.Count / 6];
            for (int i = 0; i < matches.Count; i+=6)
            {
                keys[i / 6] = new Keyframe(
                    float.Parse(matches[i].Value),
                    float.Parse(matches[i + 1].Value),
                    float.Parse(matches[i + 2].Value),
                    float.Parse(matches[i + 3].Value),
                    float.Parse(matches[i + 4].Value),
                    float.Parse(matches[i + 5].Value)
                    );

                *//*
                Debug.Log(float.Parse(matches[i].Value) + " " +
                    float.Parse(matches[i + 1].Value) + " " +
                    float.Parse(matches[i + 2].Value) + " " +
                    float.Parse(matches[i + 3].Value) + " " +
                    float.Parse(matches[i + 4].Value) + " " +
                    float.Parse(matches[i + 5].Value));
                *//*
            }

            // Set AnimationCurve
            this.heightCurve = new AnimationCurve(keys);
        }
        else
        {
            Debug.Log("You didn't copy the correct string!");
        }*/

    }

    private float[,] GenerateCityNoiseMap(int mapWidth, int mapHeight, Vector2 offsets)
    {
        float scale = noiseScale;
        mapWidth *= 5;
        mapWidth *= 5;
        float maxNoiseHeight = float.MinValue;
        float minNoiseHeight = float.MaxValue;

        float[,] noiseMap = new float[mapWidth, mapHeight];
        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {

                float amplitude = 1;
                float noiseHeight = 0;

                for (int i = 0; i < octaves; i++)
                {
                    float sampleX = (x + offsets.x) / scale;
                    float sampleY = (y + offsets.y) / scale;

                    float perlinValue = Mathf.PerlinNoise(sampleX, sampleY) * 2 - 1;
                    noiseHeight += perlinValue * amplitude;
                }

                if (noiseHeight > maxNoiseHeight)
                {
                    maxNoiseHeight = noiseHeight;
                }
                else if (noiseHeight < minNoiseHeight)
                {
                    minNoiseHeight = noiseHeight;
                }
                noiseMap[x, y] = noiseHeight;
            }
        }

        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                noiseMap[x, y] = Mathf.InverseLerp(minNoiseHeight, maxNoiseHeight, noiseMap[x, y]);
            }
        }

        return noiseMap;
    }

    private void OnValidate()
    {
        if (mapWidth < 1) mapWidth = 1;
        if (mapHeight < 1) mapHeight = 1;
        if (octaves < 0) octaves = 0;
        if (lacunarity < 0) lacunarity = 0;
    }
}

[System.Serializable]
public struct TerrainType
{
    public string name;
    public float height;
    public Color color;

}




