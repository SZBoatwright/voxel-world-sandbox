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

        int x = (int)(Mathf.Round(hitBlock.x) - raycastHit.collider.gameObject.transform.position.x);
        int y = (int)(Mathf.Round(hitBlock.y) - raycastHit.collider.gameObject.transform.position.y);
        int z = (int)(Mathf.Round(hitBlock.z) - raycastHit.collider.gameObject.transform.position.z);

        bool update = false;
        if (Input.GetMouseButtonDown(0))
          update = hitChunk.chunkData[x, y, z].HitBlock();
        else
          update = hitChunk.chunkData[x, y, z].BuildBlock(Block.BlockType.STONE);

        if (update)
        {
          List<string> updates = new List<string>();
          float thisChunkx = hitChunk.chunk.transform.position.x;
          float thisChunky = hitChunk.chunk.transform.position.y;
          float thisChunkz = hitChunk.chunk.transform.position.z;

          // updates.Add(raycastHit.collider.gameObject.name);

          // update neighbors?
          if (x == 0)
            updates.Add(World.BuildChunkName(new Vector3(thisChunkx - World.chunkSize, thisChunky, thisChunkz)));
          if (x == World.chunkSize - 1)
            updates.Add(World.BuildChunkName(new Vector3(thisChunkx + World.chunkSize, thisChunky, thisChunkz)));
          if (y == 0)
            updates.Add(World.BuildChunkName(new Vector3(thisChunkx, thisChunky - World.chunkSize, thisChunkz)));
          if (y == World.chunkSize - 1)
            updates.Add(World.BuildChunkName(new Vector3(thisChunkx, thisChunky + World.chunkSize, thisChunkz)));
          if (z == 0)
            updates.Add(World.BuildChunkName(new Vector3(thisChunkx, thisChunky, thisChunkz - World.chunkSize)));
          if (z == World.chunkSize - 1)
            updates.Add(World.BuildChunkName(new Vector3(thisChunkx, thisChunky, thisChunkz + World.chunkSize)));

          foreach (string chunkName in updates)
          {
            Chunk chunk;
            if (World.chunks.TryGetValue(chunkName, out chunk))
            {
              DestroyImmediate(chunk.chunk.GetComponent<MeshFilter>());
              DestroyImmediate(chunk.chunk.GetComponent<MeshRenderer>());
              DestroyImmediate(chunk.chunk.GetComponent<MeshCollider>());
              chunk.chunkData[x, y, z].SetType(Block.BlockType.AIR);
              chunk.DrawChunk();
            }
          }
        }
      }
    }
  }
}
