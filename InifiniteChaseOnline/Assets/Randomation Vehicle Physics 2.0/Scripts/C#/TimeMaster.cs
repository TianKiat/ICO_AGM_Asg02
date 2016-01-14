using UnityEngine;
using System.Collections;
using UnityEngine.Audio;

//Class for managing time
public class TimeMaster : MonoBehaviour
{
	float initialFixedTime;//Intial Time.fixedDeltaTime

	[Tooltip("Master audio mixer")]
	public AudioMixer masterMixer;
	public bool destroyOnLoad;

	void Awake()
	{
		initialFixedTime = Time.fixedDeltaTime;

		if (!destroyOnLoad)
		{
			DontDestroyOnLoad(gameObject);
		}
	}

	void Update()
	{
		//Set the pitch of all audio to the time scale
		if (masterMixer)
		{
			masterMixer.SetFloat("MasterPitch", Time.timeScale);
		}
	}

	void FixedUpdate()
	{
		//Set the fixed update rate based on time scale
		Time.fixedDeltaTime = Time.timeScale * initialFixedTime;
	}
}
