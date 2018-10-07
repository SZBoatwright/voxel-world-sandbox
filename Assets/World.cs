using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class World : MonoBehaviour
{
  public GameObject player;
  public Material textureAtlas;
  public static int columnHeight = 16;
  public static int chunkSize = 16;
  public static int worldSize = 2;
  public static int radius = 1; // how many chunks to generate around the player;
  public static Dictionary<string, Chunk> chunks;

  private void Start()
  {
    player.SetActive(false);
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
    int playerChunkPosX = (int)Mathf.Floor(player.transform.position.x / chunkSize);
    int playerChunkPosZ = (int)Mathf.Floor(player.transform.position.z / chunkSize);

    for (int z = -radius; z <= radius; z++)
      for (int x = -radius; x <= radius; x++)
        for (int y = 0; y < columnHeight; y++)
        {
          Vector3 chunkPosition = new Vector3((x + playerChunkPosX) * chunkSize, y * chunkSize, (z + playerChunkPosZ) * chunkSize);
          Chunk c = new Chunk(chunkPosition, textureAtlas);
          c.chunk.transform.parent = transform;
          chunks.Add(c.chunk.name, c);
        }
    foreach (KeyValuePair<string, Chunk> c in chunks)
    {
      c.Value.DrawChunk();
      yield return null;
    }
    player.SetActive(true);
  }
}
