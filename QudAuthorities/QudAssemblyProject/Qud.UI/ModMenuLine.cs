using System;
using System.Collections.Generic;
using ConsoleLib.Console;
using UnityEngine;
using UnityEngine.UI;
using XRL;

namespace Qud.UI;

public class ModMenuLine : ControlledSelectable
{
	public ModInfo modInfo;

	public Text titleText;

	public GameObject authorSpacer;

	public Text authorText;

	public InfoChip version;

	public InfoChip size;

	public InfoChip tags;

	public InfoChip location;

	public ImageTinyFrame imageFrame;

	public RectTransform taggedArea;

	public GameObject taggedPrefab;

	private string _lastAuthor = "\0";

	private string _lastTitle = "\0";

	private string _lastPath = "\0";

	private long _lastSize;

	private List<GameObject> _tagged = new List<GameObject>();

	private ModState? _lastState;

	private Sprite _sprite;

	private string _imgPath;

	public override void Update()
	{
		if (modInfo != data)
		{
			modInfo = data as ModInfo;
		}
		base.Update();
		if (modInfo == null)
		{
			return;
		}
		if (modInfo.DisplayTitle != _lastTitle)
		{
			_lastTitle = modInfo.DisplayTitle;
			titleText.text = RTF.FormatToRTF("{{Y|" + modInfo.DisplayTitle + "}}");
		}
		if (modInfo.Manifest.Author != _lastAuthor)
		{
			_lastAuthor = modInfo.Manifest.Author;
			if (string.IsNullOrEmpty(modInfo.Manifest.Author))
			{
				authorSpacer.SetActive(value: false);
				authorText.gameObject.SetActive(value: false);
			}
			else
			{
				authorSpacer.SetActive(value: true);
				authorText.gameObject.SetActive(value: true);
				authorText.text = RTF.FormatToRTF("{{y|by " + _lastAuthor + "}}");
			}
		}
		if (version != null && modInfo.Manifest.Version != version.value)
		{
			version.value = modInfo.Manifest.Version;
			version.gameObject.SetActive(!string.IsNullOrEmpty(version.value));
		}
		if (tags != null && modInfo.Manifest.Tags != tags.value)
		{
			tags.value = modInfo.Manifest.Tags;
			tags.gameObject.SetActive(!string.IsNullOrEmpty(tags.value));
		}
		if (modInfo.Path != _lastPath)
		{
			_lastPath = modInfo.Path;
			location.value = DataManager.SanitizePathForDisplay(modInfo.Path);
		}
		if (modInfo.Size != _lastSize)
		{
			_lastSize = modInfo.Size;
			double num = _lastSize;
			if (num >= 1048576.0)
			{
				num /= 1048576.0;
				size.value = $"{num:0.00} MB";
			}
			else if (_lastSize >= 1024)
			{
				num /= 1024.0;
				size.value = $"{num:0} KB";
			}
			else
			{
				size.value = _lastSize + " bytes";
			}
		}
		if (_lastState == modInfo.State)
		{
			return;
		}
		if (_tagged.Count < 1)
		{
			List<GameObject> list = new List<GameObject>();
			foreach (Transform item in taggedArea)
			{
				list.Add(item.gameObject);
			}
			try
			{
				list.ForEach(delegate(GameObject o)
				{
					o.DestroyImmediate();
				});
			}
			catch (Exception)
			{
			}
			GameObject gameObject = taggedPrefab.Instantiate();
			gameObject.transform.SetParent(taggedArea, worldPositionStays: false);
			_tagged.Add(gameObject);
		}
		_lastState = modInfo.State;
		Text componentInChildren = _tagged[0].GetComponentInChildren<Text>();
		switch (modInfo.State)
		{
		case ModState.NeedsApproval:
			componentInChildren.text = RTF.FormatToRTF("{{W|NEEDS APPROVAL}}");
			imageFrame.borderColor = ConsoleLib.Console.ColorUtility.ColorMap['W'];
			break;
		case ModState.Enabled:
			componentInChildren.text = RTF.FormatToRTF("{{green|ENABLED}}");
			imageFrame.borderColor = ConsoleLib.Console.ColorUtility.ColorMap['g'];
			break;
		case ModState.Disabled:
			componentInChildren.text = RTF.FormatToRTF("{{red|DISABLED}}");
			imageFrame.borderColor = ConsoleLib.Console.ColorUtility.ColorMap['r'];
			break;
		case ModState.Failed:
			componentInChildren.text = RTF.FormatToRTF("{{red|FAILED}}");
			imageFrame.borderColor = ConsoleLib.Console.ColorUtility.ColorMap['r'];
			break;
		}
		if (modInfo.IsScripting && _tagged.Count < 2)
		{
			GameObject gameObject2 = taggedPrefab.Instantiate();
			gameObject2.transform.SetParent(taggedArea, worldPositionStays: false);
			_tagged.Add(gameObject2);
			gameObject2.GetComponentInChildren<Text>().text = RTF.FormatToRTF("{{w|# SCRIPTING}}");
		}
		if (modInfo.GetSprite() != null && imageFrame.sprite != modInfo.GetSprite())
		{
			imageFrame.sprite = modInfo.GetSprite();
		}
	}
}
