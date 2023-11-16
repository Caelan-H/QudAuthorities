using System;
using System.Collections.Generic;
using System.Text;
using ConsoleLib.Console;
using UnityEngine;
using XRL.Core;
using XRL.Rules;
using XRL.World;
using XRL.World.Capabilities;
using XRL.World.Parts;

namespace XRL.UI;

[UIView("Looker", false, false, false, "Looker,Menu", "Looker", false, 0, false)]
public class Look : IWantsTextConsoleInit
{
	public static TextConsole _TextConsole;

	public static ScreenBuffer _ScreenBuffer;

	[NonSerialized]
	private static StringBuilder LookSB = new StringBuilder();

	[NonSerialized]
	private static ScreenBuffer Buffer = ScreenBuffer.create(80, 25);

	[NonSerialized]
	private static ScreenBuffer OldBuffer = ScreenBuffer.create(80, 25);

	private static LookSorter lookSorter = new LookSorter();

	public static bool bLocked
	{
		get
		{
			return Options.LookLocked;
		}
		set
		{
			Options.LookLocked = value;
		}
	}

	public void Init(TextConsole TextConsole_, ScreenBuffer ScreenBuffer_)
	{
		Debug.LogWarning("Looker INIT!");
		_TextConsole = TextConsole_;
		_ScreenBuffer = ScreenBuffer_;
	}

	public static XRL.World.GameObject GetTargetAt(int X, int Y, Zone Z, Predicate<XRL.World.GameObject> ObjectTest = null, Predicate<XRL.World.GameObject> ExtraVisibility = null)
	{
		if (X < 0)
		{
			return null;
		}
		if (X > Z.Width - 1)
		{
			return null;
		}
		if (Y < 0)
		{
			return null;
		}
		if (Y > Z.Height - 1)
		{
			return null;
		}
		Cell cell = Z.GetCell(X, Y);
		XRL.World.GameObject result = null;
		if (Z.GetVisibility(X, Y))
		{
			result = ((ExtraVisibility == null) ? cell.GetFirstObjectWithPart("Brain", ObjectTest, GameObjectIsVisible) : cell.GetFirstObjectWithPart("Brain", ObjectTest, (XRL.World.GameObject o) => GameObjectIsVisible(o) || ExtraVisibility(o)));
		}
		else if (ExtraVisibility != null)
		{
			result = cell.GetFirstObjectWithPart("Brain", ObjectTest, ExtraVisibility);
		}
		return result;
	}

	private static bool GameObjectIsVisible(XRL.World.GameObject obj)
	{
		return obj.IsVisible();
	}

	public static void GetObjectListCone(int StartX, int StartY, List<XRL.World.GameObject> ObjectList, string Direction, Predicate<XRL.World.GameObject> ObjectTest = null, Predicate<XRL.World.GameObject> ExtraVisibility = null)
	{
		Zone currentZone = The.Player.CurrentZone;
		ObjectList.Clear();
		int num = 1;
		int num2 = 1;
		int num3 = 1;
		int num4 = 1;
		int num5 = 1;
		int num6 = 1;
		switch (Direction)
		{
		case "nw":
			num = -1;
			num2 = -1;
			num3 = 1;
			num4 = 0;
			num5 = 0;
			num6 = 1;
			break;
		case "n":
			num = 0;
			num2 = -1;
			num3 = -1;
			num4 = 0;
			num5 = 1;
			num6 = 0;
			break;
		case "ne":
			num = 1;
			num2 = -1;
			num3 = -1;
			num4 = 0;
			num5 = 0;
			num6 = 1;
			break;
		case "e":
			num = 1;
			num2 = 0;
			num3 = 0;
			num4 = 1;
			num5 = 0;
			num6 = -1;
			break;
		case "se":
			num = 1;
			num2 = 1;
			num3 = 0;
			num4 = -1;
			num5 = -1;
			num6 = 0;
			break;
		case "s":
			num = 0;
			num2 = 1;
			num3 = 1;
			num4 = 0;
			num5 = -1;
			num6 = 0;
			break;
		case "sw":
			num = -1;
			num2 = 1;
			num3 = 0;
			num4 = -1;
			num5 = 1;
			num6 = 0;
			break;
		case "w":
			num = -1;
			num2 = 0;
			num3 = 0;
			num4 = -1;
			num5 = 0;
			num6 = 1;
			break;
		}
		int num7 = StartX;
		int num8 = StartY;
		int num9 = 0;
		while (num7 >= 0 && num7 < currentZone.Width && num8 >= 0 && num8 < currentZone.Height)
		{
			XRL.World.GameObject targetAt = GetTargetAt(num7, num8, currentZone, ObjectTest, ExtraVisibility);
			if (targetAt != null && targetAt.pRender != null && targetAt.pRender.Visible && !targetAt.HasProperty("HideCon"))
			{
				ObjectList.Add(targetAt);
			}
			for (int i = 0; i <= num9; i++)
			{
				targetAt = GetTargetAt(num7 + num3 * i, num8 + num4 * i, currentZone, ObjectTest, ExtraVisibility);
				if (targetAt != null && targetAt.pRender != null && targetAt.pRender.Visible && !targetAt.HasProperty("HideCon"))
				{
					ObjectList.Add(targetAt);
				}
				targetAt = GetTargetAt(num7 + num5 * i, num8 + num6 * i, currentZone, ObjectTest, ExtraVisibility);
				if (targetAt != null && targetAt.pRender != null && targetAt.pRender.Visible && !targetAt.HasProperty("HideCon"))
				{
					ObjectList.Add(targetAt);
				}
			}
			num9++;
			num7 += num;
			num8 += num2;
		}
	}

	public static string GenerateTooltipContent(XRL.World.GameObject O)
	{
		Description part = O.GetPart<Description>();
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendLine(O.GetDisplayName(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: false, ColorOnly: false, Visible: true, WithoutEpithet: false, Short: false, BaseOnly: false, WithIndefiniteArticle: false, O.HasProperName));
		stringBuilder.AppendLine();
		part.GetLongDescription(stringBuilder);
		stringBuilder.AppendLine();
		stringBuilder.AppendLine();
		string feelingDescription = part.GetFeelingDescription();
		if (!string.IsNullOrEmpty(feelingDescription))
		{
			stringBuilder.AppendLine(feelingDescription);
		}
		stringBuilder.Append(Strings.WoundLevel(O));
		string difficultyDescription = part.GetDifficultyDescription();
		if (!string.IsNullOrEmpty(difficultyDescription))
		{
			stringBuilder.Append(", ").Append(difficultyDescription);
		}
		return Markup.Transform(stringBuilder.ToString());
	}

	public static void QueueItemTooltip(Vector3 screenPos, XRL.World.GameObject O, bool stayOpen)
	{
		if (O == null)
		{
			return;
		}
		GameManager.Instance.gameQueue.queueSingletonTask("QueueItemTooltip" + O.GetHashCode(), delegate
		{
			string contents = GenerateTooltipContent(O);
			GameManager.Instance.uiQueue.queueTask(delegate
			{
				if (!(GameManager.Instance.generalTooltip == null))
				{
					GameManager.Instance.generalTooltip.parameterizedTextFields[0].value = Sidebar.FormatToRTF(contents.ToString());
					GameManager.Instance.generalTooltip.ShowManually(bForceDisplay: true, screenPos, usePosOverride: false, stayOpen);
					if (O != null)
					{
						GameManager.Instance.generalTooltip.onHideAction = delegate
						{
							GameManager.Instance.gameQueue.queueSingletonTask("LookedAt" + O.GetHashCode(), delegate
							{
								O.FireEvent(XRL.World.Event.New("AfterLookedAt", "Looker", XRLCore.Core.Game.Player.Body));
								if (XRLCore.Core.Game.Player.Body != null)
								{
									XRLCore.Core.Game.Player.Body.FireEvent(XRL.World.Event.New("LookedAt", "Object", O));
								}
							});
						};
					}
				}
			});
		});
	}

	public static void QueueLookerTooltip(int x, int y, string mode = "rightclick", int pickObject = 0)
	{
		if (The.Player == null || !The.Player.InACell())
		{
			return;
		}
		Cell cell = The.Player.CurrentZone.GetCell(x, y);
		XRL.World.GameObject O = null;
		int num = 0;
		List<XRL.World.GameObject> list = new List<XRL.World.GameObject>(cell.Objects);
		list.Sort(lookSorter);
		foreach (XRL.World.GameObject item in list)
		{
			if (item.HasPart("Description") && item.IsVisible())
			{
				O = item;
				num++;
				if (num > pickObject)
				{
					break;
				}
			}
		}
		if (O == null)
		{
			return;
		}
		string contents = GenerateTooltipContent(O);
		GameManager.Instance.uiQueue.queueTask(delegate
		{
			if (mode == "looker")
			{
				if (!GameManager.Instance.lookerTooltip.transform.parent.gameObject.activeInHierarchy)
				{
					GameManager.Instance.lookerTooltip.transform.parent.gameObject.SetActive(value: true);
				}
				GameManager.Instance.CenterOnCell(x, y);
				Vector3 cellCenter = GameManager.Instance.GetCellCenter(x, y);
				LookerView.instance.GetChild("InteractButton").rootObject.SetActive(value: true);
				GameManager.Instance.lookerTooltip.parameterizedTextFields[0].value = Sidebar.FormatToRTF(contents.ToString());
				GameManager.Instance.lookerTooltip.ShowManually(bForceDisplay: true, cellCenter, usePosOverride: true);
			}
			else
			{
				GameManager.Instance.tileTooltip.parameterizedTextFields[0].value = Sidebar.FormatToRTF(contents.ToString());
				GameManager.Instance.tileTooltip.ShowManually(bForceDisplay: true);
			}
			if (O != null)
			{
				if (mode == "looker")
				{
					GameManager.Instance.lookerTooltip.onHideAction = delegate
					{
						GameManager.Instance.gameQueue.queueSingletonTask("LookedAt" + O.GetHashCode(), delegate
						{
							O.FireEvent(XRL.World.Event.New("AfterLookedAt", "Looker", The.Player));
							The.Player?.FireEvent(XRL.World.Event.New("LookedAt", "Object", O));
						});
					};
				}
				else
				{
					GameManager.Instance.tileTooltip.onHideAction = delegate
					{
						GameManager.Instance.gameQueue.queueSingletonTask("LookedAt" + O.GetHashCode(), delegate
						{
							O.FireEvent(XRL.World.Event.New("AfterLookedAt", "Looker", The.Player));
							The.Player?.FireEvent(XRL.World.Event.New("LookedAt", "Object", O));
						});
					};
				}
			}
		});
	}

	public static Cell ShowLooker(int Range, int StartX, int StartY)
	{
		GameManager.Instance.PushGameView("Looker");
		Buffer.Copy(TextConsole.CurrentBuffer);
		OldBuffer.Copy(TextConsole.CurrentBuffer);
		Cell currentCell = The.Player.CurrentCell;
		Zone zone = currentCell?.ParentZone;
		bool flag = false;
		Cell result = null;
		if (zone != null)
		{
			int num = StartX;
			int num2 = StartY;
			int num3 = 0;
			XRL.World.GameObject gameObject = null;
			IRenderable renderable = null;
			TextBlock textBlock = null;
			int num4 = 0;
			int num5 = 0;
			string s = "";
			int num6 = 3;
			int num7 = 3;
			int num8 = 0;
			bool flag2 = true;
			Cell cell = null;
			while (!flag)
			{
				XRL.World.Event.ResetPool(resetMinEventPools: false);
				XRLCore.Core.RenderMapToBuffer(Buffer);
				List<XRL.World.GameObject> list = XRL.World.Event.NewGameObjectList();
				List<Point> list2 = Zone.Line(currentCell.X, currentCell.Y, num, num2);
				Cell cell2 = null;
				if (list2.Count == 0 || list2.Count == 1)
				{
					Buffer.Goto(num, num2);
					Buffer.Write("&WX");
					cell2 = zone.GetCell(num, num2);
				}
				else
				{
					for (int i = 1; i < list2.Count; i++)
					{
						Buffer.Goto(list2[i].X, list2[i].Y);
						Cell cell3 = zone.GetCell(list2[i].X, list2[i].Y);
						if (i == list2.Count - 1)
						{
							ConsoleChar currentChar = Buffer.CurrentChar;
							if (XRLCore.CurrentFrame % 16 <= 8)
							{
								Buffer.Write("^K ");
								currentChar.Detail = ConsoleLib.Console.ColorUtility.ColorMap['R'];
							}
							cell2 = cell3;
						}
					}
				}
				if (cell2 != cell)
				{
					num8 = 0;
					cell = cell2;
				}
				List<XRL.World.GameObject> objectsInCell = cell2.GetObjectsInCell();
				objectsInCell.Sort(lookSorter);
				XRL.World.GameObject gameObject2 = null;
				Description description = null;
				int num9 = 0;
				foreach (XRL.World.GameObject item in objectsInCell)
				{
					if (item.GetPart("Description") is Description description2 && item.IsVisible())
					{
						gameObject2 = item;
						description = description2;
						num9++;
						if (num9 > num8)
						{
							break;
						}
					}
				}
				if (!GameManager.Instance.OverlayUIEnabled || !Options.OverlayTooltips)
				{
					string text = ((!bLocked) ? "{{W|ESC}} | {{hotkey|(F1)}} {{W|l}}ock" : "{{W|ESC}} | {{hotkey|(F1)}} {{W|u}}nlock");
					if (gameObject2 != null)
					{
						text += " | {{hotkey|space}} interact";
					}
					int keyFromCommand = LegacyKeyMapping.GetKeyFromCommand("CmdWalk");
					if (keyFromCommand != 0)
					{
						text = text + " | {{hotkey|" + Keyboard.MetaToStringWithLower(keyFromCommand) + "}} walk";
					}
					if (Options.DebugInternals)
					{
						text += " | {{hotkey|n}} show navweight";
					}
					int x2 = ((num >= 40) ? 1 : (79 - ConsoleLib.Console.ColorUtility.LengthExceptFormatting(text)));
					Buffer.WriteAt(x2, 0, text);
					if (gameObject2 != null && description != null)
					{
						if (gameObject != gameObject2)
						{
							if (gameObject != null)
							{
								gameObject.FireEvent(XRL.World.Event.New("AfterLookedAt", "Looker", The.Player));
								if (The.Player != null)
								{
									The.Player.FireEvent(XRL.World.Event.New("LookedAt", "Object", gameObject));
								}
							}
							num6 = num;
							num7 = num2;
							gameObject = gameObject2;
							renderable = gameObject2.RenderForUI();
							int adjustFirstLine = ((renderable != null) ? (-2) : 0);
							s = Strings.WoundLevel(gameObject2);
							string displayName = gameObject2.GetDisplayName(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: false, ColorOnly: false, Visible: true, WithoutEpithet: false, Short: false, BaseOnly: false, WithIndefiniteArticle: false, gameObject2.HasProperName);
							LookSB.Clear().Append(" {{Y|").Append(displayName)
								.Append("}} \n\n");
							description.GetLongDescription(LookSB);
							int num10 = ConsoleLib.Console.ColorUtility.LengthExceptFormatting(displayName) + 2;
							if (renderable != null)
							{
								num10 += 4;
							}
							num4 = Math.Max(35, num10);
							int num11 = ((num < 40) ? (80 - num - 3) : (num - 3));
							if (num4 > num11)
							{
								num4 = num11;
							}
							textBlock = new TextBlock(LookSB, num4, 100, ReverseBlocks: false, adjustFirstLine);
							while (textBlock.Lines.Count > 22 && num4 < num11)
							{
								num4 += Math.Max(2, (num4 - num11) / 2);
								if (num4 > num11)
								{
									num4 = num11;
								}
								textBlock = new TextBlock(LookSB, num4, 100, ReverseBlocks: false, adjustFirstLine);
							}
							num5 = Math.Min(textBlock.Lines.Count, 22);
							num6 = ((num < 40) ? (num6 + 1) : (num - num4 - 2));
							num7 = ((num7 < 12) ? (num7 + 1) : (num7 - (num5 + 2)));
							if (num7 < 0)
							{
								num7 = 0;
							}
							if (num7 + num5 > 24)
							{
								num7 -= num7 + num5 - 23;
							}
							num3 = 0;
						}
						Buffer.Fill(num6, num7, num6 + num4 + 1, num7 + num5 + 1, 32, ConsoleLib.Console.ColorUtility.MakeColor(TextColor.Black, TextColor.Black));
						Buffer.SingleBox(num6, num7, num6 + num4 + 1, num7 + num5 + 1, ConsoleLib.Console.ColorUtility.MakeColor(TextColor.Cyan, TextColor.Black));
						for (int j = 0; j < num5 && j + num3 < textBlock.Lines.Count; j++)
						{
							if (j == 1 && num3 != 0)
							{
								Buffer.Goto(num6 + 1, num7 + j);
								Buffer.Write("<MORE - use {{W|-}} to scroll up>");
							}
							else
							{
								Buffer.Goto(num6 + 1, num7 + j);
								Buffer.Write(textBlock.Lines[j + ((j != 0) ? num3 : 0)]);
							}
							if (j == 0 && num3 == 0 && renderable != null)
							{
								Buffer.Goto(num6 + num4 - 2, num7 + j);
								Buffer.Write("   ");
								Buffer.Goto(num6 + num4 - 1, num7 + j);
								Buffer.Write(renderable);
							}
						}
						LookSB.Length = 0;
						string difficultyDescription = description.GetDifficultyDescription();
						if (!string.IsNullOrEmpty(difficultyDescription))
						{
							LookSB.Append(difficultyDescription);
						}
						string feelingDescription = description.GetFeelingDescription();
						if (!string.IsNullOrEmpty(feelingDescription))
						{
							LookSB.Compound(feelingDescription, ", ");
						}
						int num12 = num7 + num5 + 1;
						if (num12 > 24)
						{
							num12 = 24;
						}
						if (LookSB.Length > 0)
						{
							Buffer.Goto(num6 + num4 - ConsoleLib.Console.ColorUtility.LengthExceptFormatting(LookSB), num12);
							Buffer.Write(LookSB);
						}
						Buffer.Goto(num6 + 1, num12);
						Buffer.Write(s);
						if (textBlock != null && num3 + num5 < textBlock.Lines.Count)
						{
							Buffer.Goto(num6 + 1, num7 + num5);
							Buffer.Write("<MORE - use {{W|+}} to scroll down>");
						}
					}
					else if (gameObject != null)
					{
						gameObject.FireEvent(XRL.World.Event.New("AfterLookedAt", "Looker", The.Player));
						if (The.Player != null)
						{
							The.Player.FireEvent(XRL.World.Event.New("LookedAt", "Object", gameObject));
						}
						gameObject = null;
					}
				}
				if (GameManager.Instance.OverlayUIEnabled && Options.OverlayTooltips)
				{
					if (flag2)
					{
						if (gameObject2 == null || gameObject2.pPhysics == null)
						{
							GameManager.Instance.uiQueue.queueTask(delegate
							{
								GameManager.Instance.tileTooltip.ForceHideTooltip();
							});
						}
						else
						{
							QueueLookerTooltip(gameObject2.CurrentCell.X, gameObject2.CurrentCell.Y, "looker", num8);
							flag2 = false;
						}
					}
					_TextConsole.DrawBuffer(Buffer);
				}
				else
				{
					_TextConsole.DrawBuffer(Buffer);
				}
				if (Keyboard.kbhit())
				{
					ScreenBuffer.ClearImposterSuppression();
					flag2 = true;
					Keys keys = Keyboard.getvk(Options.MapDirectionsToKeypad);
					if (keys == Keys.MouseEvent)
					{
						if (Keyboard.CurrentMouseEvent.Event == "RightClick")
						{
							flag = true;
						}
						if (Keyboard.CurrentMouseEvent.Event == "LeftClick")
						{
							num = Keyboard.CurrentMouseEvent.x;
							num2 = Keyboard.CurrentMouseEvent.y;
						}
					}
					if ((keys == Keys.Space || (keys == Keys.MouseEvent && Keyboard.CurrentMouseEvent.Event == "Interact") || (keys == Keys.MouseEvent && Keyboard.CurrentMouseEvent.Event == "Passthrough:Interact")) && gameObject2 != null && gameObject2.Twiddle())
					{
						flag = true;
					}
					if (keys == Keys.Enter && !GameManager.Instance.PrereleaseInput)
					{
						flag = true;
					}
					if (keys == Keys.Next)
					{
						num8++;
					}
					if (keys == Keys.Prior)
					{
						num8--;
					}
					if (Keyboard.RawCode == (Keys)LegacyKeyMapping.GetKeyFromCommand("CmdWalk"))
					{
						AutoAct.Setting = "M" + num + "," + num2;
						ActionManager.SkipPlayerTurn = true;
						flag = true;
					}
					if (keys == Keys.N && Options.DebugInternals)
					{
						Popup.Show(cell.X + ", " + cell.Y + ": " + cell.GetNavigationWeightFor(The.Player));
					}
					if (num8 < 0)
					{
						num8 = 0;
					}
					if (objectsInCell != null && num8 > objectsInCell.Count)
					{
						num8 = list.Count;
					}
					if (objectsInCell == null)
					{
						num8 = 0;
					}
					if (keys == Keys.NumPad5 || keys == Keys.Escape)
					{
						flag = true;
					}
					if (keys == Keys.U || keys == Keys.L || keys == Keys.F1)
					{
						bLocked = !bLocked;
					}
					if ((LegacyKeyMapping.GetCommandFromKey((int)keys) == "CmdMoveD" || keys == Keys.Oemplus) && textBlock != null && num3 + num5 < textBlock.Lines.Count)
					{
						num3++;
					}
					if ((LegacyKeyMapping.GetCommandFromKey((int)keys) == "CmdMoveU" || keys == Keys.OemMinus) && num3 > 0)
					{
						num3--;
					}
					if (bLocked && !The.Player.OnWorldMap())
					{
						list.Clear();
						if (keys == Keys.NumPad1)
						{
							GetObjectListCone(num - 1, num2 + 1, list, "sw");
						}
						if (keys == Keys.NumPad2)
						{
							GetObjectListCone(num, num2 + 1, list, "s");
						}
						if (keys == Keys.NumPad3)
						{
							GetObjectListCone(num + 1, num2 + 1, list, "se");
						}
						if (keys == Keys.NumPad4)
						{
							GetObjectListCone(num - 1, num2, list, "w");
						}
						if (keys == Keys.NumPad6)
						{
							GetObjectListCone(num + 1, num2, list, "e");
						}
						if (keys == Keys.NumPad7)
						{
							GetObjectListCone(num - 1, num2 - 1, list, "nw");
						}
						if (keys == Keys.NumPad8)
						{
							GetObjectListCone(num, num2 - 1, list, "n");
						}
						if (keys == Keys.NumPad9)
						{
							GetObjectListCone(num + 1, num2 - 1, list, "ne");
						}
						if (list.Count > 0)
						{
							Cell currentCell2 = list[0].CurrentCell;
							num = currentCell2.X;
							num2 = currentCell2.Y;
						}
						else
						{
							if (keys == Keys.NumPad1)
							{
								num--;
								num2++;
							}
							if (keys == Keys.NumPad2)
							{
								num2++;
							}
							if (keys == Keys.NumPad3)
							{
								num++;
								num2++;
							}
							if (keys == Keys.NumPad4)
							{
								num--;
							}
							if (keys == Keys.NumPad6)
							{
								num++;
							}
							if (keys == Keys.NumPad7)
							{
								num--;
								num2--;
							}
							if (keys == Keys.NumPad8)
							{
								num2--;
							}
							if (keys == Keys.NumPad9)
							{
								num++;
								num2--;
							}
						}
					}
					else
					{
						if (keys == Keys.NumPad1)
						{
							num--;
							num2++;
						}
						if (keys == Keys.NumPad2)
						{
							num2++;
						}
						if (keys == Keys.NumPad3)
						{
							num++;
							num2++;
						}
						if (keys == Keys.NumPad4)
						{
							num--;
						}
						if (keys == Keys.NumPad6)
						{
							num++;
						}
						if (keys == Keys.NumPad7)
						{
							num--;
							num2--;
						}
						if (keys == Keys.NumPad8)
						{
							num2--;
						}
						if (keys == Keys.NumPad9)
						{
							num++;
							num2--;
						}
					}
					if (keys == Keys.F || (keys == Keys.MouseEvent && Keyboard.CurrentMouseEvent.Event == "Command:CmdFire"))
					{
						flag = true;
						result = zone.GetCell(num, num2);
					}
					if (num < 0)
					{
						num = 0;
					}
					if (num >= zone.Width)
					{
						num = zone.Width - 1;
					}
					if (num2 < 0)
					{
						num2 = 0;
					}
					if (num2 >= zone.Height)
					{
						num2 = zone.Height - 1;
					}
				}
				else
				{
					Keyboard.IdleWait();
				}
			}
			if (flag && gameObject != null)
			{
				gameObject.FireEvent(XRL.World.Event.New("AfterLookedAt", "Looker", XRLCore.Core.Game.Player.Body));
				if (XRLCore.Core.Game.Player.Body != null)
				{
					XRLCore.Core.Game.Player.Body.FireEvent(XRL.World.Event.New("LookedAt", "Object", gameObject));
				}
			}
		}
		if (GameManager.Instance.OverlayUIEnabled && Options.OverlayTooltips)
		{
			Cell currentCell3 = XRLCore.Core.Game.Player.Body.GetCurrentCell();
			int x = currentCell3.X;
			int y = currentCell3.Y;
			GameManager.Instance.uiQueue.queueTask(delegate
			{
				GameManager.Instance.tileTooltip.ForceHideTooltip();
				GameManager.Instance.CenterOnCell(x, y);
			});
		}
		_TextConsole.DrawBuffer(OldBuffer, null, bSkipIfOverlay: true);
		GameManager.Instance.PopGameView();
		return result;
	}
}
