using ConsoleLib.Console;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Qud.UI;

public class AbilityBarButton : MonoBehaviour
{
	public TextMeshProUGUI Text;

	public UIThreeColorProperties Icon;

	public string command;

	public bool disabled
	{
		get
		{
			return !base.gameObject.GetComponent<Button>().interactable;
		}
		set
		{
			base.gameObject.GetComponent<Button>().interactable = !value;
		}
	}

	public void OnClick()
	{
		if (!disabled)
		{
			Keyboard.PushMouseEvent("Command:" + command);
		}
	}
}
