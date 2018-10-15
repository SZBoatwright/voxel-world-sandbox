using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Block
{
  enum Cubeside { BOTTOM, TOP, LEFT, RIGHT, FRONT, BACK };
  public enum BlockType { GRASS, DIRT, STONE, DIAMOND, REDSTONE, BEDROCK, NOCRACK, CRACK1, CRACK2, CRACK3, CRACK4, AIR };

  public BlockType bType;
  public bool isSolid;
  public Chunk owner;
  GameObject parent;
  public Vector3 blockPosition;
  public Material cubeMaterial;

  public BlockType health;
  int currentHealth;
  int[] blockHealthMax = { 3, 3, 4, 4, 4, -1, 0, 0, 0, 0, 0, 0 }; // max health of each blocktype, in order

  Vector2[,] blockUVs =
  {
    /*GRASS TOP*/	{new Vector2( 0.125f, 0.375f ), new Vector2( 0.1875f, 0.375f),
                    new Vector2( 0.125f, 0.4375f ),new Vector2( 0.1875f, 0.4375f )},
    /*GRASS SIDE*/{new Vector2( 0.1875f, 0.9375f ), new Vector2( 0.25f, 0.9375f),
                    new Vector2( 0.1875f, 1.0f ),new Vector2( 0.25f, 1.0f )},
    /*DIRT*/			{new Vector2( 0.125f, 0.9375f ), new Vector2( 0.1875f, 0.9375f),
                    new Vector2( 0.125f, 1.0f ),new Vector2( 0.1875f, 1.0f )},
    /*STONE*/			{new Vector2( 0.5f, 0.6875f ), new Vector2( 0.5625f, 0.6875f),
                    new Vector2( 0.5f, 0.75f ),new Vector2( 0.5625f, 0.75f )},
    /*DIAMOND*/   {new Vector2( 0.125f, 0.75f ), new Vector2( 0.1875f, 0.75f),
                    new Vector2( 0.125f, 0.8125f ),new Vector2( 0.1875f, 0.8125f )},
    /*REDSTONE*/  {new Vector2( 0.1875f, 0.75f ), new Vector2( 0.25f, 0.75f),
                    new Vector2( 0.1875f, 0.8125f ),new Vector2( 0.25f, 0.8125f )},
    /*BEDROCK*/   {new Vector2( 0.0625f, 0.875f ), new Vector2( 0.125f, 0.875f),
                    new Vector2( 0.0625f, 0.9375f ),new Vector2( 0.125f, 0.9375f )},
		/*NOCRACK*/			{new Vector2( 0.6875f, 0f ), new Vector2( 0.75f, 0f),
                new Vector2( 0.6875f, 0.0625f ),new Vector2( 0.75f, 0.0625f )},
		/*CRACK1*/			{ new Vector2(0f,0f),  new Vector2(0.0625f,0f),
                 new Vector2(0f,0.0625f), new Vector2(0.0625f,0.0625f)},
 		/*CRACK2*/			{ new Vector2(0.0625f,0f),  new Vector2(0.125f,0f),
                 new Vector2(0.0625f,0.0625f), new Vector2(0.125f,0.0625f)},
 		/*CRACK3*/			{ new Vector2(0.125f,0f),  new Vector2(0.1875f,0f),
                 new Vector2(0.125f,0.0625f), new Vector2(0.1875f,0.0625f)},
 		/*CRACK4*/			{ new Vector2(0.1875f,0f),  new Vector2(0.25f,0f),
                 new Vector2(0.1875f,0.0625f), new Vector2(0.25f,0.0625f)}
  };

  public Block(BlockType type, Vector3 pos, GameObject parent, Chunk o)
  {
    bType = type;
    owner = o;
    this.parent = parent;
    blockPosition = pos;
    if (bType == BlockType.AIR)
      isSolid = false;
    else
      isSolid = true;

    health = BlockType.NOCRACK;
    currentHealth = blockHealthMax[(int)bType];
  }

  void CreateQuad(Cubeside side)
  {
    Mesh mesh = new Mesh();
    mesh.name = "ScriptedMesh";

    Vector3[] vertices = new Vector3[4];
    Vector3[] normals = new Vector3[4];
    Vector2[] uvs = new Vector2[4];
    List<Vector2> suvs = new List<Vector2>();
    int[] triangles = new int[6];

    // all possible UVs
    Vector2 uv00;
    Vector2 uv10;
    Vector2 uv01;
    Vector2 uv11;

    if (bType == BlockType.GRASS && side == Cubeside.TOP)
    {
      uv00 = blockUVs[0, 0];
      uv10 = blockUVs[0, 1];
      uv01 = blockUVs[0, 2];
      uv11 = blockUVs[0, 3];
    }
    else if (bType == BlockType.GRASS && side == Cubeside.BOTTOM)
    {
      uv00 = blockUVs[(int)(BlockType.DIRT + 1), 0];
      uv10 = blockUVs[(int)(BlockType.DIRT + 1), 1];
      uv01 = blockUVs[(int)(BlockType.DIRT + 1), 2];
      uv11 = blockUVs[(int)(BlockType.DIRT + 1), 3];
    }
    else
    {
      uv00 = blockUVs[(int)(bType + 1), 0];
      uv10 = blockUVs[(int)(bType + 1), 1];
      uv01 = blockUVs[(int)(bType + 1), 2];
      uv11 = blockUVs[(int)(bType + 1), 3];
    }

    // set crack uvs
    suvs.Add(blockUVs[(int)(health + 1), 3]);
    suvs.Add(blockUVs[(int)(health + 1), 2]);
    suvs.Add(blockUVs[(int)(health + 1), 0]);
    suvs.Add(blockUVs[(int)(health + 1), 1]);

    // all possible vertices
    Vector3 p0 = new Vector3(-0.5f, -0.5f, 0.5f);
    Vector3 p1 = new Vector3(0.5f, -0.5f, 0.5f);
    Vector3 p2 = new Vector3(0.5f, -0.5f, -0.5f);
    Vector3 p3 = new Vector3(-0.5f, -0.5f, -0.5f);
    Vector3 p4 = new Vector3(-0.5f, 0.5f, 0.5f);
    Vector3 p5 = new Vector3(0.5f, 0.5f, 0.5f);
    Vector3 p6 = new Vector3(0.5f, 0.5f, -0.5f);
    Vector3 p7 = new Vector3(-0.5f, 0.5f, -0.5f);

    switch (side)
    {
      case Cubeside.BOTTOM:
        vertices = new Vector3[] { p0, p1, p2, p3 };
        normals = new Vector3[] { Vector3.down, Vector3.down, Vector3.down, Vector3.down };
        break;
      case Cubeside.TOP:
        vertices = new Vector3[] { p7, p6, p5, p4 };
        normals = new Vector3[] { Vector3.up, Vector3.up, Vector3.up, Vector3.up };
        break;
      case Cubeside.LEFT:
        vertices = new Vector3[] { p7, p4, p0, p3 };
        normals = new Vector3[] { Vector3.left, Vector3.left, Vector3.left, Vector3.left };
        break;
      case Cubeside.RIGHT:
        vertices = new Vector3[] { p5, p6, p2, p1 };
        normals = new Vector3[] { Vector3.right, Vector3.right, Vector3.right, Vector3.right };
        break;
      case Cubeside.FRONT:
        vertices = new Vector3[] { p4, p5, p1, p0 };
        normals = new Vector3[] { Vector3.forward, Vector3.forward, Vector3.forward, Vector3.forward };
        break;
      case Cubeside.BACK:
        vertices = new Vector3[] { p6, p7, p3, p2 };
        normals = new Vector3[] { Vector3.back, Vector3.back, Vector3.back, Vector3.back };
        break;
    }

    uvs = new Vector2[] { uv11, uv01, uv00, uv10 };
    triangles = new int[] { 3, 1, 0, 3, 2, 1 };

    mesh.vertices = vertices;
    mesh.normals = normals;
    mesh.uv = uvs;
    mesh.SetUVs(1, suvs); // set uvs on channel 1 to suvs list
    mesh.triangles = triangles;

    mesh.RecalculateBounds();

    GameObject quad = new GameObject("quad");
    quad.transform.position = blockPosition;
    quad.transform.parent = parent.transform;

    MeshFilter meshFilter = (MeshFilter)quad.AddComponent(typeof(MeshFilter));
    meshFilter.mesh = mesh;

    // MeshRenderer renderer = quad.AddComponent(typeof(MeshRenderer)) as MeshRenderer;
    // renderer.material = cubeMaterial;
  }

  int ConvertBlockIndexToLocal(int i)
  {
    if (i == -1)
      i = World.chunkSize - 1;
    else if (i == World.chunkSize)
      i = 0;

    return i;
  }

  public bool HasSolidNeighbor(int x, int y, int z)
  {
    Block[,,] chunks;

    if (x < 0 || x >= World.chunkSize ||
        y < 0 || y >= World.chunkSize ||
        z < 0 || z >= World.chunkSize)
    { // the block is in a neighboring chunk

      Vector3 neighborChunkPosition = this.parent.transform.position +
                                      new Vector3((x - (int)blockPosition.x) * World.chunkSize,
                                                  (y - (int)blockPosition.y) * World.chunkSize,
                                                  (z - (int)blockPosition.z) * World.chunkSize);
      string neighborName = World.BuildChunkName(neighborChunkPosition);

      x = ConvertBlockIndexToLocal(x);
      y = ConvertBlockIndexToLocal(y);
      z = ConvertBlockIndexToLocal(z);

      Chunk neighborChunk;
      if (World.chunks.TryGetValue(neighborName, out neighborChunk))
      {
        chunks = neighborChunk.chunkData;
      }
      else
      {
        return false;
      }
    }
    else
    { // the block is in the current chunk
      chunks = owner.chunkData;
    }

    try
    {
      return chunks[x, y, z].isSolid;
    }
    catch (System.IndexOutOfRangeException ex) { };

    return false;
  }

  public void Draw()
  {
    if (bType == BlockType.AIR) return;

    if (!HasSolidNeighbor((int)blockPosition.x, (int)blockPosition.y, (int)blockPosition.z + 1))
      CreateQuad(Cubeside.FRONT);
    if (!HasSolidNeighbor((int)blockPosition.x, (int)blockPosition.y, (int)blockPosition.z - 1))
      CreateQuad(Cubeside.BACK);
    if (!HasSolidNeighbor((int)blockPosition.x, (int)blockPosition.y + 1, (int)blockPosition.z))
      CreateQuad(Cubeside.TOP);
    if (!HasSolidNeighbor((int)blockPosition.x, (int)blockPosition.y - 1, (int)blockPosition.z))
      CreateQuad(Cubeside.BOTTOM);
    if (!HasSolidNeighbor((int)blockPosition.x + 1, (int)blockPosition.y, (int)blockPosition.z))
      CreateQuad(Cubeside.RIGHT);
    if (!HasSolidNeighbor((int)blockPosition.x - 1, (int)blockPosition.y, (int)blockPosition.z))
      CreateQuad(Cubeside.LEFT);
  }

  public void SetType(BlockType b)
  {
    bType = b;
    if (bType == BlockType.AIR)
      isSolid = false;
    else
      isSolid = true;

    health = BlockType.NOCRACK;
    currentHealth = blockHealthMax[(int)bType];
  }

  public bool HitBlock()
  {
    if (currentHealth == -1) return false;
    currentHealth--;
    health++;

    if (currentHealth == (blockHealthMax[(int)bType] - 1))
    {
      owner.mb.StartCoroutine(owner.mb.HealBlock(blockPosition));
    }

    if (currentHealth <= 0)
    {
      SetType(BlockType.AIR);
      owner.RedrawChunk();
      return true;
    }
    owner.RedrawChunk();
    return false;
  }

  public bool BuildBlock(BlockType type)
  {
    SetType(type);
    owner.RedrawChunk();
    return true;
  }

  public void Reset()
  {
    health = BlockType.NOCRACK;
    currentHealth = blockHealthMax[(int)bType];
    owner.RedrawChunk();
  }
}
