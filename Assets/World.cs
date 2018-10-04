using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class World : MonoBehaviour
{
  public Material textureAtlas;
  public static int columnHeight = 2;
  public static int chunkSize = 8;
  public static Dictionary<string, Chunk> chunks;

  private void Start()
  {
    chunks = new Dictionary<string, Chunk>();
    transform.position = Vector3.zero;
    transform.rotation = Quaternion.identity;
    StartCoroutine(BuildChunkColumn());
  }

  public static string BuildChunkName(Vector3 v)
  {
    return (int)v.x + "_" + (int)v.y + "_" + (int)v.z;
  }

  IEnumerator BuildChunkColumn()
  {
    for (int i = 0; i < columnHeight; i++)
    {
      Vector3 chunkPosition = new Vector3(transform.position.x, i * chunkSize, transform.position.z);
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
