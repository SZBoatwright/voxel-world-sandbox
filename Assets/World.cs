using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class World : MonoBehaviour
{
  public Material textureAtlas;
  public static int columnHeight = 2;
  public static int chunkSize = 8;
  public static int worldSize = 2;
  public static Dictionary<string, Chunk> chunks;

  private void Start()
  {
    chunks = new Dictionary<string, Chunk>();
    transform.position = Vector3.zero;
    transform.rotation = Quaternion.identity;
    StartCoroutine(BuildWorld());
  }

  public static string BuildChunkName(Vector3 v)
  {
    return (int)v.x + "_" + (int)v.y + "_" + (int)v.z;
  }

  IEnumerator BuildWorld()
  {
    for (int z = 0; z < worldSize; z++)
      for (int x = 0; x < worldSize; x++)
        for (int y = 0; y < columnHeight; y++)
        {
          Vector3 chunkPosition = new Vector3(x * chunkSize, y * chunkSize, z * chunkSize);
          Chunk c = new Chunk(chunkPosition, textureAtlas);
          c.chunk.transform.parent = transform;
          chunks.Add(c.chunk.name, c);
        }
    foreach (KeyValuePair<string, Chunk> c in chunks)
    {
      c.Value.DrawChunk();
      yield return null;
    }
  }
}
