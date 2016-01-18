using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

[CustomEditor(typeof(SuperLevelOptimizer))]
public class SuperLevelOptimizerEditor : Editor {

    MaterialStructurizer teb = new MaterialStructurizer();

    [MenuItem("Super Level Optimizer/Create Optimizer")]
	[MenuItem("GameObject/Create Other/Super Level Optimizer")]
	private static void CreateObjectFromEditor()
	{
		GameObject combiner = new GameObject();
		combiner.name = "Super Level Optimizer";
		combiner.AddComponent<SuperLevelOptimizer>();
	}

    [MenuItem("Super Level Optimizer/CreatePrefab")]
    private static void CreatePrefab()
    {
        if (Selection.activeGameObject != null)
        {
            if (Selection.activeGameObject.GetComponent<MeshFilter>() != null)
            {
                AssetsReimporter ar = new AssetsReimporter();
                ar.CreateMesh(Selection.activeGameObject,"Assets/SLOMeshes/");
            }
        }
    }

	public override void OnInspectorGUI ()
	{
        SuperLevelOptimizer SLO = (SuperLevelOptimizer)target;

		EditorGUILayout.HelpBox("SuperLevelOptimizer only works with static objects."
		                        ,UnityEditor.MessageType.None);

        SLO.GroupingWithoutCombine = GUILayout.Toggle(SLO.GroupingWithoutCombine, "GroupingWithoutCombine");
        SLO.SuperOptimization = GUILayout.Toggle(SLO.SuperOptimization, "SuperOptimization");
        SLO.combineState = (SuperLevelOptimizer.CombineState)EditorGUILayout.EnumPopup("CombineState :", SLO.combineState);

        if (SLO.combineState == SuperLevelOptimizer.CombineState.CombineToPrefab)
        {
            SLO.folderPatch = EditorGUILayout.TextField("Folder patch", SLO.folderPatch);
        }

        teb.combineState = (MaterialStructurizer.CombineState)SLO.combineState;
        teb.folderPatch = SLO.folderPatch;
        teb.GroupingWithoutCombine = SLO.GroupingWithoutCombine;

		if(GUILayout.Button("Create atlases"))
		{
			TexturePacker tp = new TexturePacker();
            tp.superOptimization = SLO.SuperOptimization;
			tp.StartPacking();
		}


		if( GUILayout.Button("Combine Meshes") )
		{
            teb.gameObject = SLO.gameObject;
			teb.Remove();
			teb.FindRenderers();
            teb.CreateObjects();
		}

		if( GUILayout.Button("Destroy sources") )
		{
			teb.DestroyChildren();
		}
	}
}


