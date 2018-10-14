﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

[Serializable]
class BlockData
{
  public Block.BlockType[,,] matrix;

  public BlockData() { }

  public BlockData(Block[,,] b)
  {
    matrix = new Block.BlockType[World.chunkSize, World.chunkSize, World.chunkSize];
    for (int z = 0; z < World.chunkSize; z++)
      for (int y = 0; y < World.chunkSize; y++)
        for (int x = 0; x < World.chunkSize; x++)
        {
          matrix[x, y, z] = b[x, y, z].bType;
        }
  }
}

public class Chunk
{
  public Material cubeMaterial;
  public Block[,,] chunkData;
  public GameObject chunk;
  public enum ChunkStatus { DRAW, DONE, KEEP };
  public ChunkStatus status;
  public ChunkMB mb;
  BlockData blockData;

  public Chunk(Vector3 position, Material c)
  {
    chunk = new GameObject(World.BuildChunkName(position));
    chunk.transform.position = position;
    mb = chunk.AddComponent<ChunkMB>();
    mb.SetOwner(this);
    cubeMaterial = c;
    BuildChunk();
  }

  string BuildChunkFileName(Vector3 v)
  {
    return Application.persistentDataPath + "/savedata/Chunk_" +
                                            (int)v.x + (int)v.y + (int)v.z + "_" +
                                            World.chunkSize + "_" +
                                            World.radius +
                                            ".dat";
  }

  bool Load()
  {
    string fileName = BuildChunkFileName(chunk.transform.position);
    if (File.Exists(fileName))
    {
      BinaryFormatter binaryFormatter = new BinaryFormatter();
      FileStream file = File.Open(fileName, FileMode.Open);
      blockData = new BlockData();
      blockData = (BlockData)binaryFormatter.Deserialize(file);
      file.Close();
      // Debug.Log("Loading chunk from file: " + fileName);
      return true;
    }
    return false;
  }

  public void Save()
  {
    string fileName = BuildChunkFileName(chunk.transform.position);

    if (!File.Exists(fileName))
    {
      Directory.CreateDirectory(Path.GetDirectoryName(fileName));
    }
    BinaryFormatter binaryFormatter = new BinaryFormatter();
    FileStream file = File.Open(fileName, FileMode.OpenOrCreate);
    blockData = new BlockData(chunkData);
    binaryFormatter.Serialize(file, blockData);
    file.Close();
    // Debug.Log("Saving chunk from: " + fileName);
  }

  void BuildChunk()
  {

    bool dataFromFile = false;
    dataFromFile = Load();

    chunkData = new Block[World.chunkSize, World.chunkSize, World.chunkSize];

    for (int z = 0; z < World.chunkSize; z++)
      for (int y = 0; y < World.chunkSize; y++)
        for (int x = 0; x < World.chunkSize; x++)
        {
          Vector3 pos = new Vector3(x, y, z);

          int worldX = (int)(x + chunk.transform.position.x);
          int worldY = (int)(y + chunk.transform.position.y);
          int worldZ = (int)(z + chunk.transform.position.z);

          if (dataFromFile)
          {
            chunkData[x, y, z] = new Block(blockData.matrix[x, y, z], pos, chunk.gameObject, this);
            continue;
          }

          // generate bedrock
          if (worldY <= 1)
            chunkData[x, y, z] = new Block(Block.BlockType.BEDROCK, pos, chunk.gameObject, this);

          // generate caves
          else if (Utils.fBM3D(worldX, worldY, worldZ, 0.1f, 3) < 0.43f)
            chunkData[x, y, z] = new Block(Block.BlockType.AIR, pos, chunk.gameObject, this);

          // generate stone layer
          else if (worldY <= Utils.GenerateStoneHeight(worldX, worldZ))
          {
            if (Utils.fBM3D(worldX, worldY, worldZ, 0.03f, 3) < 0.41f && worldY < 20)
              chunkData[x, y, z] = new Block(Block.BlockType.REDSTONE, pos, chunk.gameObject, this);

            if (Utils.fBM3D(worldX, worldY, worldZ, 0.01f, 2) < 0.38f && worldY < 40)
              chunkData[x, y, z] = new Block(Block.BlockType.DIAMOND, pos, chunk.gameObject, this);
            else
              chunkData[x, y, z] = new Block(Block.BlockType.STONE, pos, chunk.gameObject, this);
          }

          // generate grass layer
          else if (worldY == Utils.GenerateHeight(worldX, worldZ))
            chunkData[x, y, z] = new Block(Block.BlockType.GRASS, pos, chunk.gameObject, this);

          // generate soil layer
          else if (worldY < Utils.GenerateHeight(worldX, worldZ))
            chunkData[x, y, z] = new Block(Block.BlockType.DIRT, pos, chunk.gameObject, this);

          // everything not yet set is air
          else
            chunkData[x, y, z] = new Block(Block.BlockType.AIR, pos, chunk.gameObject, this);
        }
    status = ChunkStatus.DRAW;
  }

  public void DrawChunk()
  {
    for (int z = 0; z < World.chunkSize; z++)
      for (int y = 0; y < World.chunkSize; y++)
        for (int x = 0; x < World.chunkSize; x++)
        {
          chunkData[x, y, z].Draw();
        }

    CombineQuads();
    MeshCollider collider = chunk.gameObject.AddComponent<MeshCollider>();
    collider.sharedMesh = chunk.transform.GetComponent<MeshFilter>().mesh;
    status = ChunkStatus.DONE;
  }

  public void RedrawChunk()
  {
    GameObject.DestroyImmediate(chunk.GetComponent<MeshFilter>());
    GameObject.DestroyImmediate(chunk.GetComponent<MeshRenderer>());
    GameObject.DestroyImmediate(chunk.GetComponent<Collider>());
    DrawChunk();
  }

  void CombineQuads()
  {
    // combine all child meshes
    MeshFilter[] meshFilters = chunk.GetComponentsInChildren<MeshFilter>();
    CombineInstance[] combine = new CombineInstance[meshFilters.Length];
    int i = 0;
    while (i < meshFilters.Length)
    {
      combine[i].mesh = meshFilters[i].sharedMesh;
      combine[i].transform = meshFilters[i].transform.localToWorldMatrix;
      i++;
    }

    // create a new mesh on the parent object
    MeshFilter mf = (MeshFilter)chunk.gameObject.AddComponent(typeof(MeshFilter));
    mf.mesh = new Mesh();

    // add combined meshes on children as the parent's mesh
    mf.mesh.CombineMeshes(combine);

    // create a renderer for the parent
    MeshRenderer renderer = chunk.gameObject.AddComponent(typeof(MeshRenderer)) as MeshRenderer;
    renderer.material = cubeMaterial;

    // delete all uncombined children
    foreach (Transform quad in chunk.transform)
    {
      GameObject.Destroy(quad.gameObject);
    }
  }
}
