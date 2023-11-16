using System.Collections.Generic;
using System.Text;
using ConsoleLib.Console;
using QupKit;
using UnityEngine;
using UnityEngine.UI;
using XRL.Core;
using XRL.Help;
using XRL.UI;

[UIView("Help", true, true, false, "Menu", "Help", false, 0, false)]
public class HelpView : BaseView
{
	private ScrollRect scrollView;

	private UnityEngine.UI.Text helpText;

	private UnityEngine.UI.Text topicText;

	public int CurrentTopic;

	private XRLManual manual;

	public Dictionary<int, string> RenderedTopic = new Dictionary<int, string>();

	public override void OnAttach(GameObject Canvas)
	{
		scrollView = Canvas.transform.Find("Scroll View").GetComponent<ScrollRect>();
		helpText = Canvas.transform.Find("Scroll View/Viewport/Content/Text").GetComponent<UnityEngine.UI.Text>();
		topicText = Canvas.transform.Find("Topic").GetComponent<UnityEngine.UI.Text>();
	}

	public override void BeforeEnter()
	{
		if (manual == null)
		{
			manual = XRLCore.Manual;
			CurrentTopic = 0;
			RenderTopic(CurrentTopic);
		}
	}

	public override void Enter()
	{
		base.Enter();
		GetChild("NextButton").Select();
	}

	public void RenderTopic(int topic)
	{
		if (!RenderedTopic.ContainsKey(topic))
		{
			StringBuilder stringBuilder = new StringBuilder();
			for (int i = 0; i < manual.Page[topic].Lines.Count; i++)
			{
				if (i != 0)
				{
					stringBuilder.Append("\n");
				}
				Sidebar.FormatToRTF(manual.Page[topic].LinesStripped[i], stringBuilder);
			}
			RenderedTopic.Add(topic, stringBuilder.ToString());
		}
		topicText.text = manual.Page[topic].Topic;
		if (helpText.text.Length > 0)
		{
			_ = helpText.text[0];
		}
		helpText.text = RenderedTopic[topic];
	}

	public override void OnCommand(string Command)
	{
		if (Command == "NextTopic")
		{
			CurrentTopic++;
			if (CurrentTopic >= manual.Page.Count)
			{
				CurrentTopic = 0;
			}
			RenderTopic(CurrentTopic);
			scrollView.verticalNormalizedPosition = 1f;
			Canvas.ForceUpdateCanvases();
		}
		if (Command == "PreviousTopic")
		{
			CurrentTopic--;
			if (CurrentTopic < 0)
			{
				CurrentTopic = manual.Page.Count - 1;
			}
			RenderTopic(CurrentTopic);
			scrollView.verticalNormalizedPosition = 1f;
			Canvas.ForceUpdateCanvases();
		}
		if (Command == "Back")
		{
			Keyboard.PushKey(new Keyboard.XRLKeyEvent(UnityEngine.KeyCode.Escape));
		}
	}
}
