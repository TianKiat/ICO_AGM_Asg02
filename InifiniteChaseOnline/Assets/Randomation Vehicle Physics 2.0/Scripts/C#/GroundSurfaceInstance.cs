using UnityEngine;
using System.Collections;
[RequireComponent (typeof(Collider))]

//Class for instances of surface types
public class GroundSurfaceInstance : MonoBehaviour
{
	[Tooltip("Which surface type to use from the GroundSurfaceMaster list of surface types")]
	public int surfaceType;
	[System.NonSerialized]
	public float friction;

	void Start()
	{
		//Set friction
		if (GroundSurfaceMaster.surfaceTypesStatic[surfaceType].useColliderFriction)
		{
			friction = GetComponent<Collider>().material.dynamicFriction * 2;
		}
		else
		{
			friction = GroundSurfaceMaster.surfaceTypesStatic[surfaceType].friction;
		}
	}
}
