using UnityEngine;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

public delegate void CreateMesh(GameObject mesh, string folderPatch);

public class MeshCombiner : MonoBehaviour {

    public bool combineToPrefab = false;
    public string folderPatch = "";

    public event CreateMesh createEvent;

	public void CombineChildren() 
	{
		if(this.GetComponent<MeshFilter>() == null)
			this.gameObject.AddComponent<MeshFilter>();

		if(this.GetComponent<MeshRenderer>() == null)
			this.gameObject.AddComponent<MeshRenderer>();

		MeshFilter[] meshFilters = GetComponentsInChildren<MeshFilter>();
		CombineInstance[] combine = new CombineInstance[meshFilters.Length - 1];
		int vertexCount = 0;

		if(meshFilters.Length == 2)
		{
			DestroyImmediate(this.gameObject.GetComponent<MeshCombiner>());
			return;
		}

        try
        {
            if (meshFilters[1].GetComponent<MeshRenderer>() != null)
                this.gameObject.GetComponent<MeshRenderer>().sharedMaterials = meshFilters[1].GetComponent<MeshRenderer>().sharedMaterials;

            else if (meshFilters[1].GetComponent<SkinnedMeshRenderer>() != null)
                this.gameObject.GetComponent<MeshRenderer>().sharedMaterials = meshFilters[1].GetComponent<SkinnedMeshRenderer>().sharedMaterials;
        }
        catch
        {
            return;
        }

        for (int i = 0; i < meshFilters.Length - 1; i++)
        {
            combine[i].mesh = meshFilters[i + 1].sharedMesh;
            combine[i].transform = meshFilters[i + 1].transform.localToWorldMatrix;
            if (combine[i].mesh)
                combine[i].subMeshIndex = combine[i].mesh.subMeshCount;

            if (meshFilters[i + 1].sharedMesh)
                vertexCount = vertexCount + meshFilters[i + 1].sharedMesh.vertexCount;
        }

        if (vertexCount < 65000)
        {
            this.gameObject.GetComponent<MeshFilter>().mesh = new Mesh();

            Mesh mesh = GetMesh(combine, vertexCount);

            if(mesh !=null)
                this.gameObject.GetComponent<MeshFilter>().mesh = mesh;

            if (combineToPrefab)
            {
               createEvent(gameObject, folderPatch);
            }
        }
        else
        {
            Split(meshFilters);
        }
	}

	public Mesh GetMesh(CombineInstance[] combines,int vertexCount)
	{
        combines = combines.Where(c => c.mesh !=null).ToArray();

        if (combines.Length == 0)
            return null;

		Vector3[] vertices = new Vector3[vertexCount];
		Vector3[] normals = new Vector3[vertexCount];
		Vector4[] tangents = new Vector4[vertexCount];
		List<Vector2> uv =  new List<Vector2>();
		List<Vector2> uv1 = new List<Vector2>();
		List<Vector2> uv2 = new List<Vector2>();
		List<Color> colors = new List<Color>();

		int offset = 0;

		#region vertices
		offset = 0;
		foreach(var combine in combines)
		{
			GetVertices(combine.mesh.vertexCount,combine.mesh.vertices,vertices,ref offset,combine.transform);
		}
		#endregion

		#region normals
		offset = 0;
		foreach(var combine in combines)
		{
			GetNormal(combine.mesh.vertexCount,combine.mesh.normals,normals,ref offset,combine.transform);
		}
		#endregion

		#region tangents
		offset = 0;
		foreach(var combine in combines)
		{
			GetTangents(combine.mesh.vertexCount,combine.mesh.tangents,tangents,ref offset,combine.transform);
		}
		#endregion

		#region triangles
		List<int[]> triangles = new List<int[]>();

		for(int i = 0;i < combines[0].subMeshIndex;i++)
		{
            int curTrianglesCount = 0;
            foreach (var combine in combines)
            {
                curTrianglesCount = curTrianglesCount + combine.mesh.GetTriangles(i).Length;
            }

            int[] curTriangles = new int[curTrianglesCount];

            int triangleOffset = 0;
            int vertexOffset = 0;
            foreach (var combine in combines)
            {
                int[] inputtriangles = combine.mesh.GetTriangles(i);
                for (int c = 0; c < inputtriangles.Length; c++)
                {
                    curTriangles[c + triangleOffset] = inputtriangles[c] + vertexOffset;
                }

                triangleOffset += inputtriangles.Length;
                vertexOffset += combine.mesh.vertexCount;
            }

            triangles.Add(curTriangles);
		}
		#endregion

		foreach(var combine in combines)
		{
			uv.AddRange(new Queue<Vector2>(combine.mesh.uv));
			uv1.AddRange(new Queue<Vector2>(combine.mesh.uv2));
			uv2.AddRange(new Queue<Vector2>(combine.mesh.uv2));
			colors.AddRange(new Queue<Color>(combine.mesh.colors));
		}


		Mesh mesh = new Mesh();
		mesh.name = "CombineMesh";
		mesh.vertices = vertices;
		mesh.normals = normals;
		mesh.tangents = tangents;
		mesh.subMeshCount = combines[0].subMeshIndex;
		mesh.uv = uv.ToArray();
		mesh.RecalculateBounds();

		if(triangles.Count !=1)
		{
			for(int i = 0;i < combines[0].subMeshIndex;i++)
			{
				mesh.SetTriangles(triangles[i],i);
			}
		}
		else
			mesh.SetTriangles(triangles[0],0);

        if (uv1.Count == vertices.Length)
            mesh.uv2 = uv1.ToArray();
        if (uv2.Count == vertices.Length)
            mesh.uv2 = uv2.ToArray();
        if (colors.Count == vertices.Length)
            mesh.colors = colors.ToArray();

        mesh.RecalculateNormals();
		mesh.RecalculateBounds();
		mesh.Optimize();

		return mesh;
	}

	public void GetVertices (int vertexcount, Vector3[] sources, Vector3[] main, ref int offset, Matrix4x4 transform)
	{
		for (int i = 0;i < sources.Length;i++)
			main[i+offset] = transform.MultiplyPoint(sources[i]);
		offset += vertexcount;
	}
	
	public void GetNormal (int vertexcount, Vector3[] sources, Vector3[] main, ref int offset, Matrix4x4 transform)
	{
		for (int i = 0;i < sources.Length;i++)
			main[i+offset] = transform.MultiplyVector(sources[i]).normalized;
		offset += vertexcount;
	}

	public void GetTangents (int vertexcount, Vector4[] sources, Vector4[] main, ref int offset, Matrix4x4 transform)
	{
		for (int i = 0;i < sources.Length;i++)
		{
			Vector4 p4 = sources[i];
			Vector3 p = new Vector3(p4.x, p4.y, p4.z);
			p = transform.MultiplyVector(p).normalized;
			main[i+offset] = new Vector4(p.x, p.y, p.z, p4.w);
		}
		
		offset += vertexcount;
	}

	public void Split(MeshFilter[] _meshFilters)
	{
		int count = (int)_meshFilters.Length/2;

		GameObject _firstObject = new GameObject(name = _meshFilters[0].gameObject.name);
		_firstObject.AddComponent<MeshCombiner>();

		GameObject _secondObject = new GameObject(name = _meshFilters[0].gameObject.name);
		_secondObject.AddComponent<MeshCombiner>();

		for(int i = 1;i < _meshFilters.Length;i++)
		{
			if( i < count )
			{
				_meshFilters[i].transform.parent = _firstObject.transform;

				if( _firstObject.GetComponent<MeshCombiner>() == null)
					_firstObject.AddComponent<MeshCombiner>();
			}
			else
			{
				_meshFilters[i].transform.parent = _secondObject.transform;

				if( _secondObject.GetComponent<MeshCombiner>() == null)
					_secondObject.AddComponent<MeshCombiner>();
			}
		}

		_firstObject.GetComponent<MeshCombiner>().CombineChildren();

		_secondObject.GetComponent<MeshCombiner>().CombineChildren();

		DestroyImmediate(gameObject);
	}

	public void DestroyChildren()
	{
		MeshFilter[] meshFilters = GetComponentsInChildren<MeshFilter>();

		for(int i = 1;i < meshFilters.Length;i++)
        {
            try
            {
                DestroyImmediate(meshFilters[i].gameObject);
            }
            catch
            {
                continue;
            }
        }
	}
}

















