using ConsoleLib.Console;
using QupKit;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.QupKit.Components;

public class ButtonKeyShortcut : MonoBehaviour
{
	public string CommandID = string.Empty;

	public UnityEngine.KeyCode Hotkey;

	private Button parentButton;

	public bool AllowWithRewired = true;

	public bool init;

	public void Update()
	{
		if ((AllowWithRewired || !GameManager.Instance.PrereleaseInput) && parentButton != null && parentButton.interactable && Hotkey != 0 && Input.GetKeyDown(Hotkey))
		{
			Keyboard.ClearInput();
			Input.ResetInputAxes();
			LegacyViewManager.Instance.OnCommand(CommandID);
		}
	}

	public void Awake()
	{
		if (init)
		{
			return;
		}
		init = true;
		parentButton = GetComponent<Button>();
		GetComponent<Button>().onClick.RemoveAllListeners();
		GetComponent<Button>().onClick.AddListener(delegate
		{
			if (parentButton.enabled)
			{
				LegacyViewManager.Instance.OnCommand(CommandID);
			}
		});
	}
}
