using UnityEngine;
using UnityEditor;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

public class TexturePacker : MonoBehaviour 
{
    Renderer[] allRenderers;
	List<ShadersGroup> shaderGroups = new List<ShadersGroup>();
    public bool superOptimization = false;

	public void StartPacking()
	{
		allRenderers = (Renderer[])FindObjectsOfType(typeof(Renderer));
        allRenderers = allRenderers.Where(r => r.gameObject.isStatic && r.enabled == true && r != null && r.sharedMaterials.Length > 0 && r.sharedMaterials != null).ToArray();

		for(int i = 0;i < allRenderers.Length;i++)
		{
            if(allRenderers[i] != null)
			    DeliveMesh(allRenderers[i]);
		}

        allRenderers = (Renderer[])FindObjectsOfType(typeof(Renderer));
        allRenderers = allRenderers.Where(r => r.gameObject.isStatic && r.enabled == true && r != null && r.sharedMaterials.Length > 0 && r.sharedMaterials != null).ToArray();

		InitializeComponents();
		GroupingShaders();

		foreach(var gs in shaderGroups)
		{
			CreateAtlases(gs);
		}
	}

	private void InitializeComponents()
	{
		for(int i = 0;i < allRenderers.Length;i++)
		{
			if((allRenderers[i] is MeshRenderer || allRenderers[i] is SkinnedMeshRenderer) && (allRenderers[i].sharedMaterials.Length == 1) && (allRenderers[i].gameObject.isStatic))
				if(allRenderers[i].gameObject.GetComponent<MeshFilter>() !=null)
					if(allRenderers[i].gameObject.GetComponent<MeshFilter>().sharedMesh !=null)
						if(allRenderers[i].gameObject.GetComponent<MeshFilter>().sharedMesh.subMeshCount == 1)
					{
                        try
                        {
                            ShadersGroup sg = new ShadersGroup();
                            sg.shader = allRenderers[i].sharedMaterial.shader;
                            sg.gameObjects.Add(allRenderers[i].gameObject);
                            shaderGroups.Add(sg);
                        }
                        catch
                        {
                            continue;
                        }
					}
		}
	}

	private void GroupingShaders()
	{
		try
		{
			for(int a = 0;a < shaderGroups.Count;a++)
			{
				for(int b = 0;b < shaderGroups.Count;b++)
				{
					if(shaderGroups[a].shader == shaderGroups[b].shader && a!=b)
					{
						shaderGroups[a].gameObjects.AddRange(shaderGroups[b].gameObjects);
						shaderGroups.Remove(shaderGroups[b]);
						throw new UnityException();
					}
				}
			}
		}
		catch
		{
			GroupingShaders();
			return;
		}
	}

	private void CreateAtlases(ShadersGroup sg)
	{
		List<Renderer> renderers = new List<Renderer>();

        for (int i = 0; i < sg.gameObjects.Count; i++)
		{
            if (sg.gameObjects[i].GetComponent<Renderer>() != null)
                renderers.Add(sg.gameObjects[i].GetComponent<Renderer>());
            else
                sg.gameObjects.Remove(sg.gameObjects[i]);
		}

        AssetsReimporter tr = new AssetsReimporter();
        tr.Reimport(GetTextures(renderers.ToArray()) as Texture[]);

        Texture2D[] textures = GetTextures(renderers.ToArray());

        if (textures == null || textures.Length == 0 || textures.Length == 1)
            return;

		Texture2D atlas = new Texture2D(32,32,TextureFormat.DXT1,true);
		Material newMaterial = new Material(sg.gameObjects[0].GetComponent<Renderer>().sharedMaterial);

		Rect[] rects = atlas.PackTextures(textures,0,4096,false);

        atlas.Compress(true);
        atlas.Apply();

        newMaterial.mainTexture = atlas as Texture;

		for(int i = 0;i < sg.gameObjects.Count;i++)
		{
            if(rects[i].x == 0 && rects[i].y == 0 && rects[i].width == 1 && rects[i].height == 1)
                continue;

			Vector2[] uv,uvs;

			uv = sg.gameObjects[i].GetComponent<MeshFilter>().sharedMesh.uv;
			uvs = uv;

            float offsetX = 0;
            float offsetY = 0;

            int scaleFactor = 1;

            #region Offset

            for (int c = 0; c < uv.Length; c++)
            {
                if (uv[c].x < offsetX)
                    offsetX = uv[c].x;

                if (uv[c].y < offsetY)
                    offsetY = uv[c].y;
            }

            #endregion

            if (!superOptimization && (offsetX < 0 || offsetY < 0))
                continue;

            #region SetOffset

            for (int c = 0; c < uv.Length; c++)
                uvs[c] = new Vector2(uv[c].x + (-(offsetX)), uv[c].y + (-(offsetY)));

            #endregion

            #region Scale

            for (int c = 0; c < uvs.Length; c++)
            {

                int rounded = 0;
                float value = 0;

                if (uvs[c].x < 0)
                    value = -uvs[c].x;
                else
                    value = uvs[c].x;

                rounded = Mathf.CeilToInt(value);

                if (rounded > scaleFactor)
                    scaleFactor = rounded;

                rounded = 0;
                value = 0;

                if (uv[c].y < 0)
                    value = -uvs[c].y;
                else
                    value = uvs[c].y;

                rounded = Mathf.CeilToInt(value);

                if (rounded > scaleFactor)
                    scaleFactor = rounded;
            }

            #endregion

            if (!superOptimization && scaleFactor > 1)
                continue;

            #region SetScale

            for (int c = 0; c < uvs.Length; c++)
                uvs[c] = new Vector2(uvs[c].x / scaleFactor, uvs[c].y / scaleFactor);

            #endregion

            for (int c = 0; c < uvs.Length; c++)
            {
                uvs[c] = new Vector2((float)(((float)uvs[c].x * (float)rects[i].width) + (float)rects[i].x), (float)(((float)uvs[c].y * (float)rects[i].height) + (float)rects[i].y));
            }

			sg.gameObjects[i].GetComponent<MeshFilter>().sharedMesh.uv = uvs;
            sg.gameObjects[i].GetComponent<Renderer>().sharedMaterial = newMaterial;
            sg.gameObjects[i].GetComponent<Renderer>().sharedMaterial.mainTextureOffset = new Vector2(0, 0);
            sg.gameObjects[i].GetComponent<Renderer>().sharedMaterial.mainTextureScale = new Vector2(1, 1);
		}
	}

	private Texture2D[] GetTextures(Renderer renderer)
	{
        if (renderer == null)
            return null;

		List<Texture2D> textures = new List<Texture2D>();

		for(int i = 0;i < renderer.sharedMaterials.Length;i++)
		{
			textures.Add(renderer.sharedMaterials[i].mainTexture as Texture2D);
		}

		return textures.ToArray();
	}

	private Texture2D[] GetTextures(Renderer[] renderer)
	{
        if (renderer == null)
            return null;

		List<Texture2D> textures = new List<Texture2D>();

		for(int c = 0;c < renderer.Length;c++)
		{
            if (renderer == null)
                continue;

            for (int i = 0; i < renderer[c].sharedMaterials.Length; i++)
            {
                if (i >= renderer.Length)
                    break;

                textures.Add(renderer[c].sharedMaterials[i].mainTexture as Texture2D);
            }
		}
		
		return textures.ToArray();
	}

	private void DeliveMesh(Renderer source)
	{
        if (source == null)
            return;

		if(source.gameObject.GetComponent<MeshFilter>() == null)
			return;

		if(!source.gameObject.isStatic)
			return;

        if (source.gameObject.GetComponent<MeshFilter>() == null)
            return;

        if (source.gameObject.GetComponent<MeshFilter>().sharedMesh == null)
            return;

		if(source.gameObject.GetComponent<MeshFilter>().sharedMesh.subMeshCount == 1)
		{
			Mesh mesh = new Mesh();
			mesh.vertices = source.gameObject.GetComponent<MeshFilter>().sharedMesh.vertices;
			mesh.normals = source.gameObject.GetComponent<MeshFilter>().sharedMesh.normals;
			mesh.tangents = source.gameObject.GetComponent<MeshFilter>().sharedMesh.tangents;
			mesh.uv = source.gameObject.GetComponent<MeshFilter>().sharedMesh.uv;
			mesh.uv2 = source.gameObject.GetComponent<MeshFilter>().sharedMesh.uv2;
			mesh.colors = source.gameObject.GetComponent<MeshFilter>().sharedMesh.colors;
			mesh.subMeshCount = 1;
			mesh.SetTriangles(source.gameObject.GetComponent<MeshFilter>().sharedMesh.GetTriangles(0),0);

			source.gameObject.GetComponent<MeshFilter>().mesh = mesh;
			source.gameObject.GetComponent<MeshRenderer>().material = source.sharedMaterial;
			return;
		}

		if(source.gameObject.GetComponent<MeshFilter>().sharedMesh.subMeshCount !=source.sharedMaterials.Length)
			return;


		for(int i = 0;i < source.gameObject.GetComponent<MeshFilter>().sharedMesh.subMeshCount;i++)
		{
			Mesh mesh = new Mesh();
			mesh.vertices = source.gameObject.GetComponent<MeshFilter>().sharedMesh.vertices;
			mesh.normals = source.gameObject.GetComponent<MeshFilter>().sharedMesh.normals;
			mesh.tangents = source.gameObject.GetComponent<MeshFilter>().sharedMesh.tangents;
			mesh.uv = source.gameObject.GetComponent<MeshFilter>().sharedMesh.uv;
			mesh.uv2 = source.gameObject.GetComponent<MeshFilter>().sharedMesh.uv2;
			mesh.colors = source.gameObject.GetComponent<MeshFilter>().sharedMesh.colors;
			mesh.subMeshCount = 1;
			mesh.SetTriangles(source.gameObject.GetComponent<MeshFilter>().sharedMesh.GetTriangles(i),0);

			GameObject go = new GameObject(source.gameObject.name);
			go.transform.position = source.transform.position;
			go.transform.localScale = source.transform.lossyScale;
			go.transform.rotation = source.transform.rotation;
			go.transform.parent = source.transform.parent;
			go.AddComponent<MeshRenderer>();
			go.AddComponent<MeshFilter>();
			go.GetComponent<MeshFilter>().mesh = mesh;
			go.GetComponent<MeshRenderer>().material = source.sharedMaterials[i];
			go.isStatic = true;
		}
		DestroyImmediate(source.gameObject);
	}
}

[System.Serializable]
public class ShadersGroup
{
	public List<GameObject> gameObjects = new List<GameObject>();
	public Shader shader;
}








