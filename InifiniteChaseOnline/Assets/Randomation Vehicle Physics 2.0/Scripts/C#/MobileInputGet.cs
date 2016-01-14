using UnityEngine;
using System.Collections;
[RequireComponent (typeof(VehicleParent))]

//Class for getting mobile input
public class MobileInputGet : MonoBehaviour
{
	VehicleParent vp;
	MobileInput setter;
	public float steerFactor = 1;
	public float flipFactor = 1;

	[Tooltip("Multiplier for input addition based on rate of change of input")]
	public float deltaFactor = 10;
	Vector3 accelerationPrev;
	Vector3 accelerationDelta;

	void Start()
	{
		vp = GetComponent<VehicleParent>();
		setter = FindObjectOfType<MobileInput>();
	}

	void FixedUpdate()
	{
		if (setter)
		{
			accelerationDelta = Input.acceleration - accelerationPrev;
			accelerationPrev = Input.acceleration;
			vp.SetAccel(setter.accel);
			vp.SetBrake(setter.brake);
			vp.SetSteer((Input.acceleration.x + accelerationDelta.x * deltaFactor) * steerFactor);
			vp.SetEbrake(setter.ebrake);
			vp.SetBoost(setter.boost);
			vp.SetYaw(Input.acceleration.x * flipFactor);
			vp.SetPitch(-Input.acceleration.z * flipFactor);
		}
	}
}
