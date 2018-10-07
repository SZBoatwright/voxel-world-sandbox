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
    float offset = 32000f;

    for (int i = 0; i < octaves; i++)
    {
      total += Mathf.PerlinNoise((x + offset) * frequency, (z + offset) * frequency) * amplitude;
      maxValue += amplitude;
      amplitude *= persistence;
      frequency *= 2;
    }
    return total / maxValue;
  }

  public static float fBM3D(float x, float y, float z, float smooth, int octaves)
  {
    float XY = fBM(x * smooth, y * smooth, octaves, 0.5f);
    float YZ = fBM(y * smooth, z * smooth, octaves, 0.5f);
    float XZ = fBM(x * smooth, z * smooth, octaves, 0.5f);

    float YX = fBM(y * smooth, x * smooth, octaves, 0.5f);
    float ZY = fBM(z * smooth, y * smooth, octaves, 0.5f);
    float ZX = fBM(z * smooth, x * smooth, octaves, 0.5f);

    return (XY + YZ + XZ + YX + ZY + ZX) / 6.0f;
  }

  static float Map(float newMin, float newMax, float origMin, float origMax, float value)
  {
    return Mathf.Lerp(newMin, newMax, Mathf.InverseLerp(origMin, origMax, value));
  }
}
