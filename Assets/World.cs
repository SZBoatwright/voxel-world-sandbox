using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class World : MonoBehaviour
{
  public GameObject player;
  public Material textureAtlas;
  public static int columnHeight = 16;
  public static int chunkSize = 16;
  public static int worldSize = 2;
  public static int radius = 1; // how many chunks to generate around the player;
  public static Dictionary<string, Chunk> chunks;
  public Slider loadAmount;
  public Camera cam;
  public Button playButton;
  bool firstBuild = true;
  bool building = false;

  private void Start()
  {
    player.SetActive(false);
    chunks = new Dictionary<string, Chunk>();
    transform.position = Vector3.zero;
    transform.rotation = Quaternion.identity;
  }

  private void Update()
  {
    if (!building && !firstBuild)
    {
      StartCoroutine(BuildWorld());
    }
  }

  public static string BuildChunkName(Vector3 v)
  {
    return (int)v.x + "_" + (int)v.y + "_" + (int)v.z;
  }

  public void StartBuild()
  {
    StartCoroutine(BuildWorld());
  }

  IEnumerator BuildWorld()
  {
    building = true;

    int playerChunkPosX = (int)Mathf.Floor(player.transform.position.x / chunkSize);
    int playerChunkPosZ = (int)Mathf.Floor(player.transform.position.z / chunkSize);

    float totalChunks = (Mathf.Pow(radius * 2 + 1, 2) * columnHeight) * 2;
    int processCount = 0;

    for (int z = -radius; z <= radius; z++)
      for (int x = -radius; x <= radius; x++)
        for (int y = 0; y < columnHeight; y++)
        {
          Vector3 chunkPosition = new Vector3((x + playerChunkPosX) * chunkSize, y * chunkSize, (z + playerChunkPosZ) * chunkSize);
          Chunk c;
          string name = BuildChunkName(chunkPosition);
          if (chunks.TryGetValue(name, out c))
          {
            c.status = Chunk.ChunkStatus.KEEP;
            break;
          }
          else
          {
            c = new Chunk(chunkPosition, textureAtlas);
            c.chunk.transform.parent = transform;
            chunks.Add(c.chunk.name, c);
          }

          if (firstBuild)
          {
            // update loader
            processCount++;
            loadAmount.value = processCount / totalChunks * 100;
          }
          yield return null;
        }

    foreach (KeyValuePair<string, Chunk> c in chunks)
    {
      if (c.Value.status == Chunk.ChunkStatus.DRAW)
      {
        c.Value.DrawChunk();
        c.Value.status = Chunk.ChunkStatus.KEEP;
      }

      // delete old chunks here

      c.Value.status = Chunk.ChunkStatus.DONE;

      if (firstBuild)
      {
        // update loader
        processCount++;
        loadAmount.value = processCount / totalChunks * 100;
      }
      yield return null;
    }

    if (firstBuild)
    {
      player.SetActive(true);
      loadAmount.gameObject.SetActive(false);
      cam.gameObject.SetActive(false);
      playButton.gameObject.SetActive(false);
      firstBuild = false;
    }
    building = false;
  }
}
