using UnityEngine;
using System.Collections;

public class SuperLevelOptimizer : MonoBehaviour {

    [HideInInspector]
    public bool GroupingWithoutCombine = false;
    [HideInInspector]
    public bool SuperOptimization = false;
    [HideInInspector]
    public enum CombineState { CombineToScene, CombineToPrefab };
    [HideInInspector]
    public CombineState combineState;
    [HideInInspector]
    public string folderPatch = "Assets/";
}