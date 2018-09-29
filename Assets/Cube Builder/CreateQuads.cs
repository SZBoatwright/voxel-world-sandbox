using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreateQuads : MonoBehaviour
{
  public Material cubeMaterial;
  enum Cubeside { BOTTOM, TOP, LEFT, RIGHT, FRONT, BACK };

  private void Start()
  {
    CreateCube();
    CombineQuads();
  }

  private void CreateCube()
  {
    CreateQuad(Cubeside.TOP);
    CreateQuad(Cubeside.BOTTOM);
    CreateQuad(Cubeside.LEFT);
    CreateQuad(Cubeside.RIGHT);
    CreateQuad(Cubeside.FRONT);
    CreateQuad(Cubeside.BACK);
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

  void CreateQuad(Cubeside side)
  {
    Mesh mesh = new Mesh();
    mesh.name = "ScriptedMesh";

    Vector3[] vertices = new Vector3[4];
    Vector3[] normals = new Vector3[4];
    Vector2[] uvs = new Vector2[4];
    int[] triangles = new int[6];

    // all possible UVs
    Vector2 uv00 = new Vector2(0f, 0f);
    Vector2 uv10 = new Vector2(1f, 0f);
    Vector2 uv01 = new Vector2(0f, 1f);
    Vector2 uv11 = new Vector2(1f, 1f);

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
    mesh.triangles = triangles;

    mesh.RecalculateBounds();

    GameObject quad = new GameObject("quad");
    quad.transform.parent = this.gameObject.transform;
    MeshFilter meshFilter = (MeshFilter)quad.AddComponent(typeof(MeshFilter));
    meshFilter.mesh = mesh;
    MeshRenderer renderer = quad.AddComponent(typeof(MeshRenderer)) as MeshRenderer;
    renderer.material = cubeMaterial;
  }
}
