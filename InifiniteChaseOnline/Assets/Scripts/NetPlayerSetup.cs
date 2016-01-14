using UnityEngine;
using UnityEngine.Networking;
public class NetPlayerSetup : NetworkBehaviour {
    public Behaviour[] componentsToEnable;
	// Use this for initialization
	void Start () {
	   if(isLocalPlayer)
       {
           for (int i = 0; i < componentsToEnable.Length; i++)
           {
                componentsToEnable[i].enabled = true;
           }
       }
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
