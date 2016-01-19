//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
// Name: CameraFollow
// Description:
// 
// Author: Liao Keyi
// Date: //
//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
#pragma strict

	public var target : GameObject;
	public var smoothFollow : float;
	public var distanceAway : float;
	public var height : float;
	
	private var targetPosition : Vector3;

function Start () {

}

function Update () {

}

function LateUpdate	() {

	targetPosition = target.transform.position + (target.transform.up * height) + (target.transform.forward * -distanceAway);

	transform.position = Vector3.Lerp(transform.position, targetPosition, smoothFollow * Time.deltaTime);
	
	transform.LookAt(target.transform);

}