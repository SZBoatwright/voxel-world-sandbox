using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Utils : MonoBehaviour
{

  static int maxHeight = 150;
  static float smooth = 0.01f;
  static int octaves = 4;
  static float persistence = 0.5f;

  public static int GenerateHeight(float x, float z)
  {
    float height = Map(0, maxHeight, 0, 1, fBM(x * smooth, z * smooth, octaves, persistence));
    return (int)height;
  }

  public static int GenerateStoneHeight(float x, float z)
  {
    float height = Map(0, maxHeight - 5, 0, 1, fBM(x * smooth * 2, z * smooth * 2, octaves + 1, persistence));
    return (int)height;
  }

  static float fBM(float x, float z, int octaves, float persistence)
  {
    float total = 0;
    float frequency = 1;
    float amplitude = 1;
    float maxValue = 0;

    for (int i = 0; i < octaves; i++)
    {
      total += Mathf.PerlinNoise(x * frequency, z * frequency) * amplitude;
      maxValue += amplitude;
      amplitude *= persistence;
      frequency *= 2;
    }
    return total / maxValue;
  }

  static float Map(float newMin, float newMax, float origMin, float origMax, float value)
  {
    return Mathf.Lerp(newMin, newMax, Mathf.InverseLerp(origMin, origMax, value));
  }
}
