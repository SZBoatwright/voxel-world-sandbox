using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Realtime.Messaging.Internal;

public class World : MonoBehaviour
{
  public GameObject player;
  public Material textureAtlas;
  public Material fluidTexture;
  public static int columnHeight = 16;
  public static int chunkSize = 16;
  public static int worldSize = 2;
  public static int radius = 4; // how many chunks to generate around the player;
  public static ConcurrentDictionary<string, Chunk> chunks;
  bool firstBuild = true;
  public static List<string> toRemove = new List<string>();

  public static CoroutineQueue coroutineQueue;
  public static uint maxCoroutines = 1000;

  public Vector3 lastBuildPos;

  private void Start()
  {
    // set player on the ground
    Vector3 playerPos = player.transform.position;
    player.transform.position = new Vector3(playerPos.x, Utils.GenerateHeight(playerPos.x, playerPos.z) + 1, playerPos.z);
    lastBuildPos = player.transform.position;
    player.SetActive(false);

    firstBuild = true;
    chunks = new ConcurrentDictionary<string, Chunk>();
    this.transform.position = Vector3.zero;
    this.transform.rotation = Quaternion.identity;
    coroutineQueue = new CoroutineQueue(maxCoroutines, StartCoroutine);

    // build starting chunk
    BuildChunkAt((int)(player.transform.position.x / chunkSize), (int)(player.transform.position.y / chunkSize), (int)(player.transform.position.z / chunkSize));

    // draw the chunk
    coroutineQueue.Run(DrawChunks());

    // build a bigger world
    coroutineQueue.Run(
      BuildRecursiveWorld((int)(player.transform.position.x / chunkSize), (int)(player.transform.position.y / chunkSize), (int)(player.transform.position.z / chunkSize), radius)
    );
  }

  private void Update()
  {
    Vector3 movement = lastBuildPos - player.transform.position;

    if (movement.magnitude > chunkSize)
    {
      lastBuildPos = player.transform.position;
      BuildNearPlayer();
    }

    if (!player.activeSelf)
    {
      player.SetActive(true);
      firstBuild = false;
    }

    coroutineQueue.Run(DrawChunks());
    coroutineQueue.Run(RemoveOldChunks());
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
      c = new Chunk(chunkPosition, textureAtlas, fluidTexture);
      c.chunk.transform.parent = transform;
      c.fluid.transform.parent = this.transform;
      chunks.TryAdd(c.chunk.name, c);
    }
  }

  public void BuildNearPlayer()
  {
    StopCoroutine("BuildRecursiveWorld");
    coroutineQueue.Run(
      BuildRecursiveWorld((int)(player.transform.position.x / chunkSize), (int)(player.transform.position.y / chunkSize), (int)(player.transform.position.z / chunkSize), radius)
    );
  }

  IEnumerator BuildRecursiveWorld(int x, int y, int z, int rad)
  {
    rad--;
    if (rad <= 0) yield break;

    // build chunk front
    BuildChunkAt(x, y, z + 1);
    coroutineQueue.Run(BuildRecursiveWorld(x, y, z + 1, rad));
    // build chunk back
    BuildChunkAt(x, y, z - 1);
    coroutineQueue.Run(BuildRecursiveWorld(x, y, z - 1, rad));

    // build chunk left
    BuildChunkAt(x - 1, y, z);
    coroutineQueue.Run(BuildRecursiveWorld(x - 1, y, z, rad));
    // build chunk back
    BuildChunkAt(x + 1, y, z);
    coroutineQueue.Run(BuildRecursiveWorld(x + 1, y, z, rad));

    // build chunk top
    BuildChunkAt(x, y + 1, z);
    coroutineQueue.Run(BuildRecursiveWorld(x, y + 1, z, rad));
    // build chunk bottom
    BuildChunkAt(x, y - 1, z);
    coroutineQueue.Run(BuildRecursiveWorld(x, y - 1, z, rad));

    yield return null;
  }

  public static Block GetWorldBlock(Vector3 pos)
  {
    int chunkx, chunky, chunkz;

    if (pos.x < 0)
      chunkx = (int)(Mathf.Round(pos.x - chunkSize) / (float)chunkSize) * chunkSize;
    else
      chunkx = (int)(Mathf.Round(pos.x) / (float)chunkSize) * chunkSize;

    if (pos.y < 0)
      chunky = (int)(Mathf.Round(pos.y - chunkSize) / (float)chunkSize) * chunkSize;
    else
      chunky = (int)(Mathf.Round(pos.y) / (float)chunkSize) * chunkSize;

    if (pos.z < 0)
      chunkz = (int)(Mathf.Round(pos.z - chunkSize) / (float)chunkSize) * chunkSize;
    else
      chunkz = (int)(Mathf.Round(pos.z) / (float)chunkSize) * chunkSize;

    int blockx = (int)Mathf.Abs((float)Mathf.Round(pos.x) - chunkx);
    int blocky = (int)Mathf.Abs((float)Mathf.Round(pos.y) - chunky);
    int blockz = (int)Mathf.Abs((float)Mathf.Round(pos.z) - chunkz);

    string chunkName = BuildChunkName(new Vector3(chunkx, chunky, chunkz));
    Chunk chunk;
    if (chunks.TryGetValue(chunkName, out chunk))
      return chunk.chunkData[blockx, blocky, blockz];
    else
      return null;
  }

  IEnumerator DrawChunks()
  {
    foreach (KeyValuePair<string, Chunk> c in chunks)
    {
      if (c.Value.status == Chunk.ChunkStatus.DRAW)
      {
        c.Value.DrawChunk();
      }
      if (c.Value.chunk && Vector3.Distance(player.transform.position, c.Value.chunk.transform.position) > radius * chunkSize)
        toRemove.Add(c.Key);

      yield return null;
    }
  }

  IEnumerator RemoveOldChunks()
  {
    for (int i = 0; i < toRemove.Count; i++)
    {
      string n = toRemove[i];
      Chunk c;
      if (chunks.TryGetValue(n, out c))
      {
        Destroy(c.chunk);
        c.Save();
        chunks.TryRemove(n, out c);
        yield return null;
      }
    }
  }
}
