using UnityEngine;
using System.Collections;

public class NetworkCarCamera : MonoBehaviour {
    //properties
	public Transform target;//target transform
	public float distance = 20.0f;//default distance between camera and car
	public float height = 5.0f;//height above car
	public float heightDamping = 2.0f;//dampening for height
 
	public float lookAtHeight = 0.0f;
 
	public Rigidbody parentRigidbody;//rigidbody of target
 
	public float rotationSnapTime = 0.3F;
 
	public float distanceSnapTime;
	public float distanceMultiplier;
 
	private Vector3 lookAtVector;
 
	private float usedDistance;
 
	float wantedRotationAngle;
	float wantedHeight;
 
	float currentRotationAngle;
	float currentHeight;
 
	Quaternion currentRotation;
	Vector3 wantedPosition;
 
	private float yVelocity = 0.0F;
	private float zVelocity = 0.0F;
 
	void Start () {
        //initialize lookAtVector
		lookAtVector =  new Vector3(0,lookAtHeight,0);
 
	}
 
	void LateUpdate () {
        //look at the car
		wantedHeight = target.position.y + height;
		currentHeight = transform.position.y;
 
		wantedRotationAngle = target.eulerAngles.y;
		currentRotationAngle = transform.eulerAngles.y;
 
		currentRotationAngle = Mathf.SmoothDampAngle(currentRotationAngle, wantedRotationAngle, ref yVelocity, rotationSnapTime);
 
		currentHeight = Mathf.Lerp(currentHeight, wantedHeight, heightDamping * Time.deltaTime);
 
		wantedPosition = target.position;
		wantedPosition.y = currentHeight;
 
		usedDistance = Mathf.SmoothDampAngle(usedDistance, distance + (parentRigidbody.velocity.magnitude * distanceMultiplier), ref zVelocity, distanceSnapTime); 
 
		wantedPosition += Quaternion.Euler(0, currentRotationAngle, 0) * new Vector3(0, 0, -usedDistance);
 
		transform.position = wantedPosition;
 
		transform.LookAt(target.position + lookAtVector);
	}
    //method for setting camera properties
    public void setUpNetworkCamera(Transform carTransform, Rigidbody carRigidbody)
    {
        target = carTransform;
        parentRigidbody = carRigidbody;
    }
    
}
