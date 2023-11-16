using UnityEngine;

public class GUITest : MonoBehaviour
{
	private void OnGUI()
	{
		if (GameManager.Instance != null && GameManager.Instance.Views != null)
		{
			GameManager.Instance.OnGUIExternal();
		}
	}

	private void Start()
	{
	}

	private void Update()
	{
	}
}
