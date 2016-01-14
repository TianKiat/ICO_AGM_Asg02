using UnityEngine;
using System.Collections;
using UnityEngine.UI;

//Class for displaying the framerate
public class PerformanceStats : MonoBehaviour
{
	public Text fpsText;
	float fpsUpdateTime;
	int frames;
	
	void Update()
	{
		fpsUpdateTime = Mathf.Max(0, fpsUpdateTime - Time.deltaTime);

		if (fpsUpdateTime == 0)
		{
			fpsText.text = "FPS: " + frames.ToString();
			fpsUpdateTime = 1;
			frames = 0;
		}
		else
		{
			frames ++;
		}
	}

	public void Restart()
	{
		Application.LoadLevel(Application.loadedLevel);
		Time.timeScale = 1;
	}
}
