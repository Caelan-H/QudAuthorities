using System;
using System.Collections.Generic;
using System.Linq;
using ConsoleLib.Console;
using Qud.UI;
using UnityEngine;
using XRL.Language;
using XRL.World;
using XRL.World.Conversations;
using XRL.World.Parts;

namespace XRL.UI;

[UIView("OldConversation", false, false, false, "Menu,Conversation", null, false, 0, false)]
public class OldConversationUI : IWantsTextConsoleInit
{
	public static TextConsole _TextConsole;

	public static ScreenBuffer _ScreenBuffer;

	private static XRL.World.Event eShowConversationChoices = new XRL.World.Event("ShowConversationChoices", "Choices", null, "FirstNode", null, "CurrentNode", null, "Speaker", null);

	public void Init(TextConsole TextConsole_, ScreenBuffer ScreenBuffer_)
	{
		_TextConsole = TextConsole_;
		_ScreenBuffer = ScreenBuffer_;
	}

	public static XRL.World.Event ShowConversationChoices(XRL.World.GameObject Speaker, List<ConversationChoice> Choices, ConversationNode FirstNode, ConversationNode CurrentNode)
	{
		eShowConversationChoices.SetParameter("Choices", Choices);
		eShowConversationChoices.SetParameter("FirstNode", FirstNode);
		eShowConversationChoices.SetParameter("CurrentNode", CurrentNode);
		eShowConversationChoices.SetParameter("Speaker", Speaker);
		if (Speaker.FireEvent(eShowConversationChoices) && The.Player.FireEvent(eShowConversationChoices))
		{
			The.Game.FireSystemsEvent(eShowConversationChoices);
		}
		return eShowConversationChoices;
	}

	private static string PronounExchangeDescription(XRL.World.GameObject Player, XRL.World.GameObject Speaker, bool SpeakerGivePronouns, bool SpeakerGetPronouns, bool SpeakerGetNewPronouns)
	{
		return ConversationScript.PronounExchangeDescription(Player, Speaker, SpeakerGivePronouns, SpeakerGetPronouns, SpeakerGetNewPronouns);
	}

	public static void HaveConversation(string ConversationID, XRL.World.GameObject Speaker = null, bool TradeEnabled = true, bool bCheckObjectTalking = true, string startNode = null, IEvent ParentEvent = null, string Filter = null, string FilterExtras = null, string Append = null, string Color = null, bool Physical = false, bool Mental = false)
	{
		if (ConversationLoader.Loader.ConversationsByID.TryGetValue(ConversationID, out var value))
		{
			HaveConversation(value, Speaker, TradeEnabled, bCheckObjectTalking, startNode, ParentEvent, Filter, FilterExtras, Append, Color, Physical, Mental);
		}
		else
		{
			MetricsManager.LogError("Unknown conversation '" + ConversationID + "'");
		}
	}

	public static UnityEngine.KeyCode GetUnityKeycodeForChoice(int c)
	{
		if (c < 9)
		{
			return (UnityEngine.KeyCode)(49 + c);
		}
		return (UnityEngine.KeyCode)(97 + (c - 9));
	}

	public static Keys GetKeycodeForChoice(int c)
	{
		if (c < 9)
		{
			return (Keys)(49 + c);
		}
		return (Keys)(65 + (c - 9));
	}

	public static int GetChoiceNumber(Keys k)
	{
		if (k >= Keys.D1 && k <= Keys.D9)
		{
			return (int)(k - 49);
		}
		return (int)(9 + (k - 65));
	}

	public static string GetChoiceDisplayChar(int n)
	{
		return n switch
		{
			0 => "1", 
			1 => "2", 
			2 => "3", 
			3 => "4", 
			4 => "5", 
			5 => "6", 
			6 => "7", 
			7 => "8", 
			8 => "9", 
			_ => ((char)(65 + (n - 9))).ToString() ?? "", 
		};
	}

	public static void HaveConversation(Conversation CurrentConversation, XRL.World.GameObject Speaker = null, bool TradeEnabled = true, bool bCheckObjectTalking = true, string startNode = null, IEvent ParentEvent = null, string Filter = null, string FilterExtras = null, string Append = null, string Color = null, bool Physical = false, bool Mental = false)
	{
		try
		{
			_HaveConversation(CurrentConversation, Speaker, TradeEnabled, bCheckObjectTalking, startNode, ParentEvent, Filter, Filter, Append, Color, Physical, Mental);
		}
		finally
		{
			eShowConversationChoices.SetParameter("Speaker", null);
			eShowConversationChoices.SetParameter("FirstNode", null);
			eShowConversationChoices.SetParameter("CurrentNode", null);
			eShowConversationChoices.SetParameter("Choices", null);
			Conversation.Speaker = null;
		}
	}

	private static void _HaveConversation(Conversation CurrentConversation, XRL.World.GameObject Speaker = null, bool TradeEnabled = true, bool bCheckObjectTalking = true, string startNode = null, IEvent ParentEvent = null, string Filter = null, string FilterExtras = null, string Append = null, string Color = null, bool Physical = false, bool Mental = false)
	{
		XRL.World.GameObject player = The.Player;
		if (player == null)
		{
			MetricsManager.LogError("Player missing");
			return;
		}
		Conversation.Speaker = Speaker;
		CurrentConversation = CurrentConversation.CloneDeep();
		ConversationNode conversationNode = null;
		foreach (ConversationNode startNode2 in CurrentConversation.StartNodes)
		{
			if (startNode != null || startNode2.Test())
			{
				conversationNode = startNode2;
				if (startNode == null || !startNode2.ID.EqualsNoCase(startNode))
				{
					CurrentConversation.NodesByID.Remove("Start");
					CurrentConversation.NodesByID.Add("Start", startNode2);
					break;
				}
			}
		}
		if (!CanHaveConversationEvent.Check(The.Player, Speaker, CurrentConversation, TradeEnabled, Physical, Mental))
		{
			return;
		}
		if (TradeEnabled)
		{
			if (Speaker == null)
			{
				TradeEnabled = false;
			}
			else
			{
				if (!The.Player.PhaseMatches(Speaker))
				{
					TradeEnabled = false;
				}
				else if (Speaker.HasTagOrProperty("NoTrade"))
				{
					TradeEnabled = false;
				}
				else if (Speaker.DistanceTo(The.Player) > 1)
				{
					TradeEnabled = false;
				}
				XRL.World.GameObject player2 = The.Player;
				Conversation conversation = CurrentConversation;
				bool physical = Physical;
				bool mental = Mental;
				TradeEnabled = CanTradeEvent.Check(player2, Speaker, conversation, TradeEnabled, physical, mental);
			}
		}
		if (TradeEnabled && conversationNode != null && conversationNode.bCloseable)
		{
			conversationNode.Choices.Add(new ConversationChoice
			{
				ParentNode = conversationNode,
				Ordinal = 990,
				Text = "Let's trade. {{g|[begin trade]}}",
				GotoID = "*trade"
			});
		}
		if (conversationNode != null && conversationNode.bCloseable && GlobalConfig.GetBoolSetting("GeneralAskName") && Speaker.IsCreature && !Speaker.HasProperName && !Speaker.HasPropertyOrTag("NoAskName") && ConversationLoader.Loader.ConversationsByID.TryGetValue("GenericAskNameOption", out var value))
		{
			ConversationChoice conversationChoice = new ConversationChoice();
			conversationChoice.Copy(value.StartNodes[0].Choices[0]);
			conversationChoice.ParentNode = conversationNode;
			conversationChoice.Ordinal = 10000;
			if (conversationChoice.Text.Contains("~"))
			{
				conversationChoice.Text = conversationChoice.Text.Split('~').GetRandomElement();
			}
			conversationNode.Choices.Add(conversationChoice);
			if (value.NodesByID.TryGetValue("TellName", out var value2))
			{
				ConversationNode conversationNode2 = new ConversationNode();
				conversationNode2.Copy(value2);
				int i = 0;
				for (int count = conversationNode2.Choices.Count; i < count; i++)
				{
					if (conversationNode2.Choices[i].Text.Contains("~"))
					{
						conversationNode2.Choices[i].Text = conversationNode2.Choices[i].Text.Split('~').GetRandomElement();
					}
				}
				CurrentConversation.AddNode(conversationNode2);
			}
		}
		GameManager.Instance.PushGameView("OldConversation");
		try
		{
			if (!BeforeConversationEvent.Check(The.Player, Speaker, CurrentConversation, TradeEnabled, Physical, Mental) || !BeginConversationEvent.Check(The.Player, Speaker, CurrentConversation, TradeEnabled, Physical, Mental) || (bCheckObjectTalking && (Speaker == null || !Speaker.FireEvent("ObjectTalking"))))
			{
				return;
			}
			if (The.Player.HasEffect("Lost") && CanGiveDirectionsEvent.Check(The.Player, Speaker, CurrentConversation, TradeEnabled, Physical, Mental))
			{
				Popup.Show("You ask about your location, and are no longer lost!");
				The.Player.RemoveEffect("Lost");
				XRL.World.Event e = XRL.World.Event.New("GaveDirections", "Speaker", Speaker);
				Speaker?.FireEvent(e);
				The.Game.FireSystemsEvent(e);
			}
			TextConsole.LoadScrapBuffers();
			int num = 0;
			int num2 = 0;
			ConversationNode conversationNode3 = CurrentConversation.NodesByID["Start"];
			ConversationNode firstNode = conversationNode3;
			List<ConversationChoice> list = null;
			string text = null;
			string text2 = null;
			bool flag = false;
			while (conversationNode3 != null)
			{
				XRL.World.Event.ResetPool(resetMinEventPools: false);
				conversationNode3.Visit(Speaker, player);
				conversationNode3.Choices.Sort();
				XRL.World.Event @event = ShowConversationChoices(Speaker, conversationNode3.Choices, firstNode, conversationNode3);
				flag = TradeEnabled;
				if (flag && !conversationNode3.bCloseable)
				{
					flag = false;
				}
				list = @event.GetParameter<List<ConversationChoice>>("Choices");
				if (text == null)
				{
					text = conversationNode3.Text;
					if (text.Contains('~'))
					{
						text = text.Split('~').GetRandomElement();
					}
					text = GameText.VariableReplace(text, Speaker).Trim();
					text2 = text;
					if (!string.IsNullOrEmpty(Append))
					{
						string text3 = Append;
						if (text3.Contains('~'))
						{
							text3 = text3.Split('~').GetRandomElement();
						}
						text2 += GameText.VariableReplace(text3, Speaker);
					}
					if (!string.IsNullOrEmpty(Filter))
					{
						text2 = TextFilters.Filter(text2, Filter, FilterExtras);
					}
					if (!string.IsNullOrEmpty(Color))
					{
						text2 = "{{" + Color + "|" + text2 + "}}";
					}
					if (!string.IsNullOrEmpty(CurrentConversation.Introduction))
					{
						text2 = GameText.VariableReplace(CurrentConversation.Introduction, Speaker) + text2;
						CurrentConversation.Introduction = "";
					}
					if (!string.IsNullOrEmpty(conversationNode3.PrependUnspoken))
					{
						if (!conversationNode3.PrependUnspoken.EndsWith("\n"))
						{
							text2 = "\n" + text2;
						}
						text2 = GameText.VariableReplace(conversationNode3.PrependUnspoken, Speaker) + text2;
					}
					if (!string.IsNullOrEmpty(conversationNode3.AppendUnspoken))
					{
						if (text2 != "" && !text2.EndsWith("\n"))
						{
							text2 += "\n";
						}
						text2 += GameText.VariableReplace(conversationNode3.AppendUnspoken, Speaker);
					}
					if (conversationNode3.TradeNote && flag)
					{
						if (text2 != "" && !text2.EndsWith("\n"))
						{
							text2 += "\n";
						}
						text2 += "\n[Press Tab or T to open trade]";
					}
				}
				_ScreenBuffer.Clear();
				_ScreenBuffer.SingleBox(0, 0, 79, 24, ConsoleLib.Console.ColorUtility.MakeColor(TextColor.Grey, TextColor.Black));
				_ScreenBuffer.Goto(2, 0);
				string text4 = (Options.DebugShowConversationNode ? ((Speaker == null) ? ("{{W|" + CurrentConversation.ID + " - " + conversationNode3.ID + "}}") : ("{{W|" + Speaker.DisplayName + " - " + CurrentConversation.ID + " - " + conversationNode3.ID + "}}")) : ((Speaker == null) ? "" : ("{{W|" + Speaker.DisplayName + "}}")));
				if (!string.IsNullOrEmpty(text4))
				{
					_ScreenBuffer.Write("{{y|[ " + text4 + " ]}}");
				}
				if (Speaker != null && Speaker.HasPart("Brain") && player.GetPart<Mutations>().HasMutation("Empathy"))
				{
					_ScreenBuffer.Goto(45, 0);
					_ScreenBuffer.Write("[ Feels " + Speaker.pBrain.GetFeeling(player) + " towards player ]");
				}
				if (flag)
				{
					_ScreenBuffer.Goto(60, 24);
					_ScreenBuffer.Write("[ {{W|Tab}} - {{W|T}}rade ]");
				}
				int MaxClippedWidth;
				List<string> list2 = new List<string>(StringFormat.ClipTextToArray(text2, 76, out MaxClippedWidth, KeepNewlines: true));
				list2.Add("");
				List<int> list3 = new List<int>(8);
				List<int> list4 = new List<int>(8);
				List<string> list5 = new List<string>(8);
				list3.Add(list2.Count);
				int num3 = 0;
				for (int j = 0; j < list.Count; j++)
				{
					ConversationChoice conversationChoice2 = list[j];
					if (conversationChoice2.Test())
					{
						string text5 = "y";
						ConversationNode conversationNode4 = conversationChoice2.Goto(Speaker, peekOnly: true, CurrentConversation);
						if (conversationNode4 != null)
						{
							conversationNode4.ParentConversation = CurrentConversation;
						}
						text5 = ((conversationNode4 == null) ? "G" : (conversationNode4.alwaysShowAsNotVisted ? "G" : ((conversationNode4.ID == conversationNode3.ID && !conversationChoice2.Visited) ? "G" : ((!conversationNode4.Visited) ? "G" : "g"))));
						string text6 = "{{" + text5 + "|" + GameText.VariableReplace(list[j].GetDisplayText(), Speaker) + "}}";
						List<string> collection = StringFormat.ClipTextToArray(text6, 71, out MaxClippedWidth);
						list2.AddRange(collection);
						list2.Add(" ");
						list3.Add(list2.Count);
						list5.Add(text6);
						list4.Add(j);
						num3++;
					}
				}
				GameManager.Instance.ClearRegions();
				if (num2 > 0)
				{
					_ScreenBuffer.Goto(2, 1);
					_ScreenBuffer.Write("&W<more...>");
				}
				for (int k = num2; k < list3[0]; k++)
				{
					_ScreenBuffer.Goto(2, 2 + k - num2);
					_ScreenBuffer.Write("&y");
					_ScreenBuffer.Write(list2[k]);
				}
				num3 = 2 + list3[0] - num2;
				for (int l = 0; l < list3.Count - 1; l++)
				{
					int num4 = 2 + list3[l] - num2;
					string s = ((num != l) ? ("&y  &W" + GetChoiceDisplayChar(l) + "&y) ") : ("&Y>&y^k &W" + GetChoiceDisplayChar(l) + "&y) "));
					if (num4 > 22)
					{
						_ScreenBuffer.Goto(2, 23);
						_ScreenBuffer.Write("&W<more...>");
					}
					else if (num4 >= 2)
					{
						_ScreenBuffer.Goto(2, num4);
						_ScreenBuffer.Write(s);
						_ScreenBuffer.Goto(7, num4);
						string[] array = list2.GetRange(list3[l], list3[l + 1] - list3[l]).ToArray();
						int num5 = 23 - num4;
						_ScreenBuffer.WriteBlockWithNewlines(array, num5);
						GameManager.Instance.AddRegion(2, num4, 70, array.Count() - 1 + num4, "Click:" + l, "Close", "Hover:" + l);
						if (num5 < array.Count())
						{
							_ScreenBuffer.Goto(2, 23);
							_ScreenBuffer.Write("&W<more...>");
						}
					}
					else if (list3[l + 1] - num2 >= 2)
					{
						_ScreenBuffer.Goto(7, 2);
						string[] array2 = list2.GetRange(list3[l], list3[l + 1] - list3[l]).ToArray();
						_ScreenBuffer.WriteBlockWithNewlines(array2, 21, 2 - num4);
						GameManager.Instance.AddRegion(2, 2, 70, array2.Count() - 1 + num4, "Click:" + l, "Close", "Hover:" + l);
					}
				}
				if (num2 > 0 || list2.Count > 21)
				{
					ScrollbarHelper.Paint(_ScreenBuffer, 2, 79, 20, ScrollbarHelper.Orientation.Vertical, 0, list2.Count() - 1, num2, num2 + 21);
				}
				int num6 = -1;
				Keys keys;
				if (UIManager.UseNewPopups)
				{
					num6 = Popup.ShowConversation(text4, Speaker, text2, list5, flag, conversationNode3.bCloseable);
					switch (num6)
					{
					case -1:
						keys = Keys.Escape;
						break;
					case -2:
						keys = Keys.T;
						break;
					default:
						num = num6;
						keys = Keys.Space;
						break;
					}
				}
				else
				{
					_TextConsole.DrawBuffer(_ScreenBuffer, null, bSkipIfOverlay: true);
					keys = Keyboard.getvk(Options.MapDirectionsToKeypad);
				}
				if (keys == Keys.MouseEvent && Keyboard.CurrentMouseEvent.Event != null)
				{
					if (Keyboard.CurrentMouseEvent.Event == "Close")
					{
						keys = Keys.Escape;
					}
					else if (Keyboard.CurrentMouseEvent.Event.Contains("Click:"))
					{
						num6 = Convert.ToInt32(Keyboard.CurrentMouseEvent.Event.Split(':')[1]);
					}
					else if (Keyboard.CurrentMouseEvent.Event.Contains("Hover:"))
					{
						num = Convert.ToInt32(Keyboard.CurrentMouseEvent.Event.Split(':')[1]);
					}
				}
				if (keys == Keys.MouseEvent && Keyboard.CurrentMouseEvent.Event == "RightClick")
				{
					keys = Keys.Escape;
				}
				if (keys == Keys.Escape && conversationNode3.bCloseable)
				{
					if (conversationNode3 != null)
					{
						Speaker?.FireEvent(XRL.World.Event.New("LeaveConversationNode", "CurrentNode", conversationNode3, "GotoID", conversationNode3.ID));
						if (conversationNode3.OnLeaveNode != null)
						{
							conversationNode3.OnLeaveNode();
						}
					}
					break;
				}
				if (flag && (keys == Keys.T || keys == Keys.Tab))
				{
					TradeUI.ShowTradeScreen(Speaker);
				}
				if (keys == Keys.Space || keys == Keys.Enter)
				{
					num6 = num;
				}
				else if (LegacyKeyMapping.MapKeyToCommand(Keyboard.MetaKey) == "CmdMoveS" && list4.Count <= GetChoiceNumber(keys))
				{
					if (num < list4.Count - 1)
					{
						num++;
					}
				}
				else if (LegacyKeyMapping.MapKeyToCommand(Keyboard.MetaKey) == "CmdMoveN" && list4.Count <= GetChoiceNumber(keys))
				{
					if (num > 0)
					{
						num--;
					}
					else
					{
						num2 = 0;
					}
				}
				else if (((keys >= Keys.D1 && keys <= Keys.D9) || (keys >= Keys.A && keys <= Keys.Z)) && list4.Count > GetChoiceNumber(keys))
				{
					num6 = GetChoiceNumber(keys);
				}
				if (list3[num] < num2)
				{
					num2 = list3[num];
				}
				if (list3[num + 1] > 21 + num2)
				{
					num2 = list3[num + 1] - 21;
				}
				if (num6 < 0 || num6 >= list4.Count)
				{
					continue;
				}
				ConversationChoice conversationChoice3 = list[list4[num6]];
				bool removeChoice = false;
				bool terminateConversation = false;
				bool flag2 = conversationChoice3.Visit(Speaker, player, out removeChoice, out terminateConversation);
				if (removeChoice)
				{
					list.Remove(conversationChoice3);
				}
				if (terminateConversation)
				{
					break;
				}
				if (flag2)
				{
					Speaker.FireEvent(XRL.World.Event.New("LeaveConversationNode", "CurrentNode", conversationNode3, "GotoID", conversationNode3.ID));
					if (conversationNode3 != null && conversationNode3.OnLeaveNode != null)
					{
						conversationNode3.OnLeaveNode();
					}
					ConversationNode previous = conversationNode3;
					conversationNode3 = conversationChoice3.Goto(Speaker);
					num2 = 0;
					if (conversationNode3 != null)
					{
						conversationNode3.ParentConversation = CurrentConversation;
						Speaker.FireEvent(XRL.World.Event.New("VisitConversationNode", "CurrentNode", conversationNode3, "GotoID", conversationNode3.ID));
						conversationNode3 = conversationNode3.Enter(previous, Speaker);
					}
					num = 0;
					text = null;
				}
			}
		}
		finally
		{
			Keyboard.ClearMouseEvents();
			GameManager.Instance.PopGameView();
			_TextConsole.DrawBuffer(TextConsole.ScrapBuffer2, null, bSkipIfOverlay: true);
			AfterConversationEvent.Send(The.Player, Speaker, CurrentConversation, TradeEnabled, Physical, Mental);
		}
	}
}
