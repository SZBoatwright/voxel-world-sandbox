using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Realtime.Messaging.Internal;

public class World : MonoBehaviour
{
  public GameObject player;
  public Material textureAtlas;
  public static int columnHeight = 16;
  public static int chunkSize = 16;
  public static int worldSize = 2;
  public static int radius = 4; // how many chunks to generate around the player;
  public static ConcurrentDictionary<string, Chunk> chunks;
  bool firstBuild = true;

  private void Start()
  {
    // set player on the ground
    Vector3 playerPos = player.transform.position;
    player.transform.position = new Vector3(playerPos.x, Utils.GenerateHeight(playerPos.x, playerPos.z) + 1, playerPos.z);
    player.SetActive(false);

    firstBuild = true;
    chunks = new ConcurrentDictionary<string, Chunk>();
    this.transform.position = Vector3.zero;
    this.transform.rotation = Quaternion.identity;

    // build starting chunk
    BuildChunkAt((int)(player.transform.position.x / chunkSize), (int)(player.transform.position.y / chunkSize), (int)(player.transform.position.z / chunkSize));

    // draw the chunk
    StartCoroutine(DrawChunks());

    // build a bigger world
    StartCoroutine(
      BuildRecursiveWorld((int)(player.transform.position.x / chunkSize), (int)(player.transform.position.y / chunkSize), (int)(player.transform.position.z / chunkSize), radius)
    );
  }

  private void Update()
  {
    if (!player.activeSelf)
    {
      player.SetActive(true);
      firstBuild = false;
    }

    StartCoroutine(DrawChunks());
  }

  public static string BuildChunkName(Vector3 v)
  {
    return (int)v.x + "_" + (int)v.y + "_" + (int)v.z;
  }

  void BuildChunkAt(int x, int y, int z)
  {
    Vector3 chunkPosition = new Vector3(x * chunkSize, y * chunkSize, z * chunkSize);
    string name = BuildChunkName(chunkPosition);
    Chunk c;

    if (!chunks.TryGetValue(name, out c))
    {
      c = new Chunk(chunkPosition, textureAtlas);
      c.chunk.transform.parent = transform;
      chunks.TryAdd(c.chunk.name, c);
    }
  }

  IEnumerator BuildRecursiveWorld(int x, int y, int z, int rad)
  {
    Debug.Log(rad);
    if (rad <= 0) yield break;
    BuildChunkAt(x, y, z - 1);
    StartCoroutine(BuildRecursiveWorld(x, y, z - 1, rad - 1));

    yield return null;
  }

  IEnumerator DrawChunks()
  {
    foreach (KeyValuePair<string, Chunk> c in chunks)
    {
      if (c.Value.status == Chunk.ChunkStatus.DRAW)
      {
        c.Value.DrawChunk();
      }

      yield return null;
    }
  }
}
