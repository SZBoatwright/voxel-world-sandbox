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

  CoroutineQueue queue;
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
    queue = new CoroutineQueue(maxCoroutines, StartCoroutine);

    // build starting chunk
    BuildChunkAt((int)(player.transform.position.x / chunkSize), (int)(player.transform.position.y / chunkSize), (int)(player.transform.position.z / chunkSize));

    // draw the chunk
    queue.Run(DrawChunks());

    // build a bigger world
    queue.Run(
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

    queue.Run(DrawChunks());
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

  public void BuildNearPlayer()
  {
    StopCoroutine("BuildRecursiveWorld");
    queue.Run(
      BuildRecursiveWorld((int)(player.transform.position.x / chunkSize), (int)(player.transform.position.y / chunkSize), (int)(player.transform.position.z / chunkSize), radius)
    );
  }

  IEnumerator BuildRecursiveWorld(int x, int y, int z, int rad)
  {
    rad--;
    if (rad <= 0) yield break;

    // build chunk front
    BuildChunkAt(x, y, z + 1);
    queue.Run(BuildRecursiveWorld(x, y, z + 1, rad));
    // build chunk back
    BuildChunkAt(x, y, z - 1);
    queue.Run(BuildRecursiveWorld(x, y, z - 1, rad));

    // build chunk left
    BuildChunkAt(x - 1, y, z);
    queue.Run(BuildRecursiveWorld(x - 1, y, z, rad));
    // build chunk back
    BuildChunkAt(x + 1, y, z);
    queue.Run(BuildRecursiveWorld(x + 1, y, z, rad));

    // build chunk top
    BuildChunkAt(x, y + 1, z);
    queue.Run(BuildRecursiveWorld(x, y + 1, z, rad));
    // build chunk bottom
    BuildChunkAt(x, y - 1, z);
    queue.Run(BuildRecursiveWorld(x, y - 1, z, rad));

    yield return null;
  }

  IEnumerator DrawChunks()
  {
    foreach (KeyValuePair<string, Chunk> c in chunks)
    {
      if (c.Value.status == Chunk.ChunkStatus.DRAW)
      {
        Debug.Log("status is draw");
        c.Value.DrawChunk();
      }

      yield return null;
    }
  }
}
