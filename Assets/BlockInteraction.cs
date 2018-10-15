using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockInteraction : MonoBehaviour
{

  public GameObject cam;
  [Tooltip("Will the raycast sent out for placing/deleting blocks be sent from camera or mouse position?")]
  [SerializeField] bool useMousePosiiton;

  void Update()
  {
    if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1))
    {
      RaycastHit raycastHit;
      Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

      if (useMousePosiiton ?
            Physics.Raycast(ray, out raycastHit, 10) :
            Physics.Raycast(cam.transform.position, cam.transform.forward, out raycastHit, 10)
         )
      {
        Chunk hitChunk;
        if (!World.chunks.TryGetValue(raycastHit.collider.gameObject.name, out hitChunk)) return;

        Vector3 hitBlock;
        if (Input.GetMouseButtonDown(0))
          hitBlock = raycastHit.point - raycastHit.normal / 2.0f;
        else
          hitBlock = raycastHit.point + raycastHit.normal / 2.0f;

        Block block = World.GetWorldBlock(hitBlock);
        hitChunk = block.owner;

        bool update = false;
        if (Input.GetMouseButtonDown(0))
          update = block.HitBlock();
        else
          update = block.BuildBlock(Block.BlockType.STONE);

        if (update)
        {
          List<string> updates = new List<string>();
          float thisChunkx = hitChunk.chunk.transform.position.x;
          float thisChunky = hitChunk.chunk.transform.position.y;
          float thisChunkz = hitChunk.chunk.transform.position.z;

          // updates.Add(raycastHit.collider.gameObject.name);

          // update neighbors?
          if (block.blockPosition.x == 0)
            updates.Add(World.BuildChunkName(new Vector3(thisChunkx - World.chunkSize, thisChunky, thisChunkz)));
          if (block.blockPosition.x == World.chunkSize - 1)
            updates.Add(World.BuildChunkName(new Vector3(thisChunkx + World.chunkSize, thisChunky, thisChunkz)));
          if (block.blockPosition.y == 0)
            updates.Add(World.BuildChunkName(new Vector3(thisChunkx, thisChunky - World.chunkSize, thisChunkz)));
          if (block.blockPosition.y == World.chunkSize - 1)
            updates.Add(World.BuildChunkName(new Vector3(thisChunkx, thisChunky + World.chunkSize, thisChunkz)));
          if (block.blockPosition.z == 0)
            updates.Add(World.BuildChunkName(new Vector3(thisChunkx, thisChunky, thisChunkz - World.chunkSize)));
          if (block.blockPosition.z == World.chunkSize - 1)
            updates.Add(World.BuildChunkName(new Vector3(thisChunkx, thisChunky, thisChunkz + World.chunkSize)));

          foreach (string chunkName in updates)
          {
            Chunk chunk;
            if (World.chunks.TryGetValue(chunkName, out chunk))
            {
              chunk.RedrawChunk();
            }
          }
        }
      }
    }
  }
}
