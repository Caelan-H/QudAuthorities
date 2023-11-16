using QupKit;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.QupKit.Components;

public class HotkeyCommand : MonoBehaviour
{
	public string CommandID = "";

	public KeyCode Hotkey;

	public bool HotkeyCapitalized;

	private Selectable parentControl;

	public void Awake()
	{
		if (parentControl == null)
		{
			parentControl = GetComponent<Button>();
		}
	}

	public void Update()
	{
		if ((parentControl == null || parentControl.interactable) && Hotkey != 0 && Input.GetKeyDown(Hotkey) && HotkeyCapitalized == (GameManager.capslock || Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)))
		{
			Input.ResetInputAxes();
			LegacyViewManager.Instance.OnCommand(CommandID);
		}
	}
}
