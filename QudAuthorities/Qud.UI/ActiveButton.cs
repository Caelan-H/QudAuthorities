using UnityEngine;
using UnityEngine.UI;

namespace Qud.UI;

[ExecuteAlways]
public class ActiveButton : MonoBehaviour
{
	public Color InactiveColor;

	public Color ActiveColor;

	public bool IsActive;

	public bool? _wasActive;

	public void Update()
	{
		if (_wasActive != IsActive)
		{
			_wasActive = IsActive;
			Button component = base.gameObject.GetComponent<Button>();
			if (component != null)
			{
				ColorBlock colors = component.colors;
				colors.normalColor = (IsActive ? ActiveColor : InactiveColor);
				colors.selectedColor = (IsActive ? ActiveColor : InactiveColor);
				component.colors = colors;
			}
		}
	}
}
