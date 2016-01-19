using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class TOD_RenderAtDay : MonoBehaviour
{
	private Renderer rendererComponent;

	protected void Start()
	{
		rendererComponent = GetComponent<Renderer>();

		rendererComponent.enabled = TOD_Sky.Instance.IsDay;
	}

	protected void Update()
	{
		rendererComponent.enabled = TOD_Sky.Instance.IsDay;
	}
}
