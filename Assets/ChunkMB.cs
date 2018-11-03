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
    InvokeRepeating("SaveProgress", 10, 1000);
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

  public IEnumerator Flow(Block block, Block.BlockType blockType, int strength, int maxSize)
  {

    // strength: how many times the function will run recursively before stopping
    // maxSize: how far down water will flow before stopping

    if (maxSize <= 0) yield break;
    if (block == null) yield break;
    if (strength <= 0) yield break;
    if (block.bType != Block.BlockType.AIR) yield break;

    block.SetType(blockType);
    block.currentHealth = strength;
    block.owner.RedrawChunk();
    yield return new WaitForSeconds(1);

    int x = (int)block.blockPosition.x;
    int y = (int)block.blockPosition.y;
    int z = (int)block.blockPosition.z;

    Block blockBelow = block.GetBlock(x, y - 1, z);
    if (blockBelow != null && blockBelow.bType == Block.BlockType.AIR)
    {
      StartCoroutine(Flow(block.GetBlock(x, y - 1, z), blockType, strength, --maxSize));
      yield break;
    }
    else
    {
      --strength;
      --maxSize;

      // flow left
      World.coroutineQueue.Run(Flow(block.GetBlock(x - 1, y, z), blockType, strength, maxSize));
      yield return new WaitForSeconds(1);

      // flow right
      World.coroutineQueue.Run(Flow(block.GetBlock(x + 1, y, z), blockType, strength, maxSize));
      yield return new WaitForSeconds(1);

      // flow backward
      World.coroutineQueue.Run(Flow(block.GetBlock(x, y, z - 1), blockType, strength, maxSize));
      yield return new WaitForSeconds(1);

      // flow forward
      World.coroutineQueue.Run(Flow(block.GetBlock(x, y, z + 1), blockType, strength, maxSize));
      yield return new WaitForSeconds(1);
    }
  }

  void SaveProgress()
  {
    if (owner.changed)
    {
      owner.Save();
      owner.changed = false;
    }
  }
}
