using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChunkMB : MonoBehaviour
{

  Chunk owner;
  ChunkMB() { }
  public void SetOwner(Chunk o)
  {
    owner = o;
  }
  public IEnumerator HealBlock(Vector3 bPos)
  {
    yield return new WaitForSeconds(3);

    int x = (int)bPos.x;
    int y = (int)bPos.y;
    int z = (int)bPos.z;

    if (owner.chunkData[x, y, z].bType != Block.BlockType.AIR)
    {
      owner.chunkData[x, y, z].Reset();
    }
  }
}
