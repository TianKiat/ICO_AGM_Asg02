using UnityEngine;
using UnityEditor;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

public class MaterialStructurizer : ScriptableObject  {

	Renderer[] allRenderers;
    public GameObject gameObject;
	List<GroupRenderers> groupRenderers = new List<GroupRenderers>();
	[HideInInspector]public bool GroupingWithoutCombine = false;
    [HideInInspector]public enum CombineState { CombineToScene, CombineToPrefab };
    [HideInInspector]public CombineState combineState;
    [HideInInspector]public string folderPatch = "Assets/";

	public void FindRenderers()
	{
		allRenderers = (Renderer[])FindObjectsOfType(typeof(Renderer));
        allRenderers = allRenderers.Where(r => r.gameObject.isStatic && r.enabled == true && r.sharedMaterial !=null).ToArray();
		
		InitializeGroupRenderers();
		
		GroupingRenderers();
	}

	public void Remove()
	{
		allRenderers = new Renderer[0];
		groupRenderers = new List<GroupRenderers>();
	}

	public void InitializeGroupRenderers()
	{
		foreach(Renderer _renderer in allRenderers)
		{
			if( (_renderer is MeshRenderer || _renderer is SkinnedMeshRenderer) && (_renderer.gameObject.isStatic) && (_renderer.sharedMaterial !=null) && (_renderer.enabled))
			{
				GroupRenderers curGroupRenderers = new GroupRenderers();
				
				curGroupRenderers.gameObjects.Add(_renderer.gameObject);
                curGroupRenderers.materials = _renderer.sharedMaterials;
				
				groupRenderers.Add(curGroupRenderers);
			}
		}

		if(groupRenderers.Count == 0)
		{
			Debug.LogError("Not found static objects in the scene.");
			throw new UnityException();
		}
	}

	public void GroupingRenderers()
	{
        for (int a = 0; a < groupRenderers.Count; a++)
        {
            for (int b = 0; b < groupRenderers.Count; b++)
            {
                if ((isMatch(groupRenderers[a], groupRenderers[b]) == true) && (a != b))
                {
                    groupRenderers[a].gameObjects.AddRange(groupRenderers[b].gameObjects);
                    groupRenderers[b].gameObjects = new List<GameObject>();
                }
            }
        }

        groupRenderers = groupRenderers.Where(g => g.gameObjects.Count > 0).ToList();
	}

    public bool isMatch(GroupRenderers gRenderersA, GroupRenderers gRenderersB)
    {
        return gRenderersA.GetName() == gRenderersB.GetName();
    }

    public void CreateObjects()
    {
        string thisName = this.gameObject.name;

        foreach (var gRenderers in groupRenderers)
        {
            GameObject _gameObject = new GameObject(name = "GroupingRenderers - " + gRenderers.materials[0].name + "_" +  gRenderers.gameObjects.Count);

            foreach (var instObjects in gRenderers.gameObjects)
            {
                instObjects.transform.parent = _gameObject.transform;
				if(instObjects.transform.parent.gameObject.GetComponent<MeshCombiner>() == null && !GroupingWithoutCombine)
					instObjects.transform.parent.gameObject.AddComponent<MeshCombiner>();
            }

            this.gameObject.name = thisName;
			if(!GroupingWithoutCombine)
				BakeMeshCombiners();
        }
    }

	public void BakeMeshCombiners()
	{
		MeshCombiner[] meshCombiners = (MeshCombiner[])GameObject.FindObjectsOfType(typeof(MeshCombiner));
        AssetsReimporter reimporter = new AssetsReimporter();

        if (folderPatch[folderPatch.Length - 1] != '/')
            folderPatch = folderPatch + "/";

		foreach(var combine in meshCombiners)
		{
            if (combineState == CombineState.CombineToPrefab)
            {
                combine.combineToPrefab = true;
                combine.folderPatch = folderPatch;
                combine.createEvent += reimporter.CreateMesh;
            }

			combine.CombineChildren();
		}
	}

	public void DestroyChildren()
	{
		MeshCombiner[] meshCombiners = (MeshCombiner[])FindObjectsOfType(typeof(MeshCombiner));

		for(int i = 0;i < meshCombiners.Length;i++)
		{
			meshCombiners[i].DestroyChildren();
		}

		foreach(var combiner in meshCombiners)
		{
			DestroyImmediate(combiner);
		}
	}
}


[System.Serializable]
public class GroupRenderers
{
	public Material[] materials = new Material[0];
	public List<GameObject> gameObjects = new List<GameObject>();

    public string GetName()
    {
        string _name = "";

        foreach(var material in materials)
        {
            _name = _name + material.name + material.shader.name;

            if (material.HasProperty("_MainTex"))
                _name = _name + material.mainTexture.name;
            else
                _name = _name + "null";
        }

        return _name;
    }
}
