using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chunk : MonoBehaviour
{
  public Material cubeMaterial;

  void Start()
  {
    StartCoroutine(BuildChunk(4, 4, 4));
  }

  IEnumerator BuildChunk(int sizeX, int sizeY, int sizeZ)
  {
    for (int z = 0; z < sizeZ; z++)
      for (int y = 0; y < sizeY; y++)
        for (int x = 0; x < sizeX; x++)
        {
          Vector3 pos = new Vector3(x, y, z);
          Block b = new Block(Block.BlockType.DIRT, pos, this.gameObject, cubeMaterial);
          b.Draw();
          yield return null;
        }
    CombineQuads();
  }

  void CombineQuads()
  {
    // combine all child meshes
    MeshFilter[] meshFilters = GetComponentsInChildren<MeshFilter>();
    CombineInstance[] combine = new CombineInstance[meshFilters.Length];
    int i = 0;
    while (i < meshFilters.Length)
    {
      combine[i].mesh = meshFilters[i].sharedMesh;
      combine[i].transform = meshFilters[i].transform.localToWorldMatrix;
      i++;
    }

    // create a new mesh on the parent object
    MeshFilter mf = (MeshFilter)this.gameObject.AddComponent(typeof(MeshFilter));
    mf.mesh = new Mesh();

    // add combined meshes on children as the parent's mesh
    mf.mesh.CombineMeshes(combine);

    // create a renderer for the parent
    MeshRenderer renderer = gameObject.AddComponent(typeof(MeshRenderer)) as MeshRenderer;
    renderer.material = cubeMaterial;

    // delete all uncombined children
    foreach (Transform quad in this.transform)
    {
      Destroy(quad.gameObject);
    }
  }
}
