using UnityEngine;
using UnityEngine.Networking;
public class NetPlayerSetup : NetworkBehaviour
{
    public Behaviour[] componentsToDisable;
    public GameObject networkedCamera;

    // Use this for initialization
    void Start()
    {
        //spawn car camera and set the target of the camera
        if (networkedCamera != null)
        {
            GameObject carCamera = Instantiate(networkedCamera, Vector3.zero, Quaternion.identity) as GameObject;
            carCamera.SetActive(false);//default inactive
            if (isLocalPlayer)
            {
                //activate camera and initialize the settings
                carCamera.SetActive(true);
                carCamera.GetComponent<NetworkCarCamera>().setUpNetworkCamera(transform, GetComponent<Rigidbody>());
            }
        }
        //if the player is not local disable the components and camera
        if (!isLocalPlayer)
        {
            //enable all components in the array
            for (int i = 0; i < componentsToDisable.Length; i++)
            {
                componentsToDisable[i].enabled = false;
            }
        }
    }
}
