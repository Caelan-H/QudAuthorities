using UnityEngine;
using XRL;
using XRL.Core;
using XRL.UI;

namespace Qud.UI;

[ExecuteAlways]
[HasGameBasedStaticCache]
[UIView("MessageLog", false, false, false, null, null, false, 0, false, NavCategory = "Adventure", UICanvas = "MessageLog", UICanvasHost = 1)]
public class MessageLogWindow : MovableSceneFrameWindowBase<MessageLogWindow>
{
	public MessageLogPooledScrollRect messageLog;

	public new void Update()
	{
		if (Application.isPlaying)
		{
			if (ControlManager.isCommandDown("Toggle Message Log"))
			{
				toggle.SetActive(!toggle.activeSelf);
			}
			base.Update();
		}
	}

	[GameBasedCacheInit]
	public static void GameInit()
	{
		SingletonWindowBase<MessageLogWindow>.instance?.ClearMessageLog();
	}

	public void ClearMessageLog()
	{
		messageLog?.Clear();
	}

	public override void Init()
	{
		base.Init();
		XRLCore.RegisterNewMessageLogEntryCallback(AddMessage);
	}

	public void AddMessage(string log)
	{
		string i = ":: " + log;
		GameManager.Instance?.uiQueue?.queueTask(delegate
		{
			_AddMessage(i);
		});
	}

	private void _AddMessage(string log)
	{
		messageLog?.Add(RTF.FormatToRTF(log));
	}

	public override bool AllowPassthroughInput()
	{
		return true;
	}
}
