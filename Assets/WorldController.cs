using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldController : MonoBehaviour
{

  public GameObject block;
  public int worldWidth = 10;
  public int worldDepth = 10;
  public int worldHeight = 2;

  // Use this for initialization
  void Start()
  {
    StartCoroutine(BuildWorld());
  }

  public IEnumerator BuildWorld()
  {
    for (int z = 0; z < worldWidth; z++)
      for (int y = 0; y < worldHeight; y++)
      {
        for (int x = 0; x < worldDepth; x++)
        {
          Vector3 position = new Vector3(x, y, z);
          GameObject cube = Instantiate(block, position, Quaternion.identity);
          cube.name = x + " " + y + " " + z;
        }
        yield return null;
      }
  }
}
