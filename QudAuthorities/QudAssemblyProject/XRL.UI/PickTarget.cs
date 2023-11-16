using System;
using System.Collections.Generic;
using ConsoleLib.Console;
using Genkit;
using XRL.Core;
using XRL.Rules;
using XRL.World;
using XRL.World.Parts;

namespace XRL.UI;

[UIView("PickTarget", true, false, false, "Menu,Targeting", null, false, 0, false)]
public class PickTarget : IWantsTextConsoleInit
{
	public enum PickStyle
	{
		Cone,
		Line,
		Burst,
		Circle,
		EmptyCell
	}

	public static TextConsole _TextConsole;

	public static ScreenBuffer _ScreenBuffer;

	public static ScreenBuffer OldBuffer = ScreenBuffer.create(80, 25);

	public static ScreenBuffer Buffer = ScreenBuffer.create(80, 25);

	public static bool bLocked
	{
		get
		{
			return Options.PickTargetLocked;
		}
		set
		{
			Options.PickTargetLocked = value;
		}
	}

	public void Init(TextConsole TextConsole_, ScreenBuffer ScreenBuffer_)
	{
		_TextConsole = TextConsole_;
		_ScreenBuffer = ScreenBuffer_;
	}

	public static GameObject GetCombatObjectAt(int X, int Y, Zone Z)
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
		if (!cell.IsExplored() || !cell.IsLit())
		{
			return null;
		}
		return cell.GetFirstObjectWithPart("Combat");
	}

	public static void GetObjectListCone(int StartX, int StartY, List<GameObject> ObjectList, string Direction, Predicate<GameObject> ObjectTest = null, Predicate<GameObject> ExtraVisibility = null)
	{
		Look.GetObjectListCone(StartX, StartY, ObjectList, Direction, ObjectTest, ExtraVisibility);
	}

	public static List<Cell> ShowFieldPicker(int Range, int Size, int StartX, int StartY, string What = "Wall", bool StartAdjacent = false, bool ReturnNullForAbort = false, bool AllowDiagonals = false, bool AllowDiagonalStart = true, bool RequireVisibility = false)
	{
		GameManager.Instance.PushGameView("PickTarget");
		OldBuffer.Copy(TextConsole.CurrentBuffer);
		Buffer.Copy(TextConsole.CurrentBuffer);
		List<Cell> list = new List<Cell>();
		Physics pPhysics = XRLCore.Core.Game.Player.Body.pPhysics;
		int num = Range;
		bool flag = false;
		bool flag2 = true;
		bool flag3 = false;
		bool flag4 = false;
		if (pPhysics != null)
		{
			Cell currentCell = pPhysics.CurrentCell;
			int num2 = StartX;
			int num3 = StartY;
			while (!flag3)
			{
				Event.ResetPool(resetMinEventPools: false);
				XRLCore.Core.RenderMapToBuffer(Buffer);
				string s = ((flag || !StartAdjacent || num2 != StartX || num3 != StartY) ? ("{{W|space}} {{W|5}}-" + (flag ? "End " : "Start") + " " + What + " {{C|" + num + "}}") : (num.ToString() ?? "")) + " " + ((num == 1) ? "square" : "squares") + " left  {{W|Escape}}-" + (flag ? "Clear" : "Cancel");
				if (num2 < 40)
				{
					Buffer.Goto(79 - ColorUtility.LengthExceptFormatting(s), 0);
				}
				else
				{
					Buffer.Goto(1, 0);
				}
				Buffer.Write(s);
				foreach (Cell item in list)
				{
					Buffer.WriteAt(item, "{{G|#}}");
				}
				Buffer.Goto(num2, num3);
				if (XRLCore.CurrentFrame % 32 < 16)
				{
					Buffer.Write("{{Y|X}}");
				}
				_TextConsole.DrawBuffer(Buffer);
				if (!Keyboard.kbhit())
				{
					continue;
				}
				Keys keys = Keyboard.getvk(MapDirectionToArrows: true);
				if (keys == Keys.Escape)
				{
					if (flag)
					{
						flag = false;
						list = new List<Cell>();
						num = Range;
						if (StartAdjacent)
						{
							num2 = StartX;
							num3 = StartY;
						}
					}
					else
					{
						flag3 = true;
						if (ReturnNullForAbort)
						{
							flag4 = true;
						}
					}
				}
				if (keys == Keys.U)
				{
					bLocked = false;
				}
				if (keys == Keys.L)
				{
					bLocked = true;
				}
				int num4 = num2;
				int num5 = num3;
				if (keys == Keys.MouseEvent)
				{
					Keyboard.MouseEvent currentMouseEvent = Keyboard.CurrentMouseEvent;
					if (currentMouseEvent.Event == "PointerOver")
					{
						if (flag2)
						{
							flag2 = false;
						}
						else
						{
							num2 = currentMouseEvent.x;
							num3 = currentMouseEvent.y;
						}
					}
					else if (currentMouseEvent.Event == "RightClick")
					{
						flag3 = true;
					}
				}
				if (keys == Keys.NumPad1)
				{
					num2--;
					num3++;
				}
				if (keys == Keys.NumPad2)
				{
					num3++;
				}
				if (keys == Keys.NumPad3)
				{
					num2++;
					num3++;
				}
				if (keys == Keys.NumPad4)
				{
					num2--;
				}
				if (keys == Keys.NumPad6)
				{
					num2++;
				}
				if (keys == Keys.NumPad7)
				{
					num2--;
					num3--;
				}
				if (keys == Keys.NumPad8)
				{
					num3--;
				}
				if (keys == Keys.NumPad9)
				{
					num2++;
					num3--;
				}
				if (num2 < 0)
				{
					num2 = 0;
				}
				if (num2 >= currentCell.ParentZone.Width)
				{
					num2 = currentCell.ParentZone.Width - 1;
				}
				if (num3 < 0)
				{
					num3 = 0;
				}
				if (num3 >= currentCell.ParentZone.Height)
				{
					num3 = currentCell.ParentZone.Height - 1;
				}
				if (flag)
				{
					if ((!AllowDiagonals && num2 != num4 && num3 != num5) || Math.Abs(num4 - num2) > 1 || Math.Abs(num5 - num3) > 1)
					{
						num2 = num4;
						num3 = num5;
					}
				}
				else if (StartAdjacent)
				{
					if (num2 > StartX + 1)
					{
						num2 = StartX + 1;
					}
					else if (num2 < StartX - 1)
					{
						num2 = StartX - 1;
					}
					if (num3 > StartY + 1)
					{
						num3 = StartY + 1;
					}
					else if (num3 < StartY - 1)
					{
						num3 = StartY - 1;
					}
					if (!AllowDiagonalStart && num2 != StartX && num3 != StartY)
					{
						if (num4 == StartX)
						{
							num2 = num4;
						}
						else if (num5 == StartY)
						{
							num3 = num5;
						}
						else
						{
							num2 = num4;
							num3 = num5;
						}
					}
				}
				if (RequireVisibility && (num2 != num4 || num3 != num5) && !currentCell.ParentZone.GetCell(num2, num3).IsVisible())
				{
					num2 = num4;
					num3 = num5;
				}
				if (keys == Keys.NumPad5 || keys == Keys.Space || keys == Keys.Enter || (keys == Keys.MouseEvent && Keyboard.CurrentMouseEvent.Event == "LeftClick"))
				{
					if (!flag)
					{
						if (!StartAdjacent || num2 != StartX || num3 != StartY)
						{
							Cell cell = currentCell.ParentZone.GetCell(num2, num3);
							list.Add(cell);
							num--;
							flag = true;
						}
					}
					else
					{
						flag3 = true;
					}
				}
				if ((num2 != num4 || num3 != num5) && flag)
				{
					Cell cell2 = currentCell.ParentZone.GetCell(num2, num3);
					if (list.CleanContains(cell2))
					{
						if (list.Count > 1 && cell2 == list[list.Count - 2])
						{
							list.Remove(currentCell.ParentZone.GetCell(num4, num5));
							num++;
						}
						else
						{
							num2 = num4;
							num3 = num5;
						}
					}
					else if (num <= 0)
					{
						num2 = num4;
						num3 = num5;
					}
					else
					{
						list.Add(currentCell.ParentZone.GetCell(num2, num3));
						num--;
					}
				}
				if (keys == Keys.F || (keys == Keys.MouseEvent && Keyboard.CurrentMouseEvent.Event == "Command:CmdFire"))
				{
					flag3 = true;
				}
			}
		}
		_TextConsole.DrawBuffer(OldBuffer);
		GameManager.Instance.PopGameView();
		if (ReturnNullForAbort && flag4 && list.Count == 0)
		{
			return null;
		}
		return list;
	}

	public static bool IsVisibleCombatObject(GameObject Object)
	{
		if (Object.pRender != null && Object.pRender.Visible)
		{
			return Object.IsCombatObject();
		}
		return false;
	}

	public static Cell ShowPicker(PickStyle Style, int Radius, int Range, int StartX, int StartY, bool Locked, AllowVis VisLevel, Predicate<GameObject> ExtraVisibility = null, Predicate<GameObject> ObjectTest = null, GameObject UsePassability = null, Point2D? Origin = null, string Label = null, bool EnforceRange = false, bool UseTarget = true)
	{
		GameManager.Instance.PushGameView("PickTarget");
		bool flag = Locked;
		OldBuffer.Copy(TextConsole.CurrentBuffer);
		Buffer.Copy(TextConsole.CurrentBuffer);
		GameObject body = XRLCore.Core.Game.Player.Body;
		Cell cell = body?.GetCurrentCell();
		Cell result = null;
		if (body != null)
		{
			bool flag2 = false;
			Point2D point2D = Origin ?? cell.Pos2D;
			if (UseTarget && Sidebar.CurrentTarget != null && body.GetConfusion() <= 0 && (ObjectTest == null || ObjectTest(Sidebar.CurrentTarget)))
			{
				Cell currentCell = Sidebar.CurrentTarget.CurrentCell;
				if (currentCell != null && body.InSameZone(currentCell))
				{
					StartX = currentCell.X;
					StartY = currentCell.Y;
				}
			}
			int x = StartX;
			int y = StartY;
			bool flag3 = true;
			List<Location2D> list = new List<Location2D>();
			while (!flag2)
			{
				Event.ResetStringbuilderPool();
				Event.ResetGameObjectListPool();
				XRLCore.Core.RenderMapToBuffer(Buffer);
				List<Point> list2 = Zone.Line(point2D.x, point2D.y, x, y);
				string text = "&W";
				switch (Style)
				{
				case PickStyle.EmptyCell:
				{
					if (list2.Count == 0)
					{
						Buffer.Goto(x, y);
						Buffer.Write("{{W|X}}");
						break;
					}
					Buffer.Goto(x, y);
					int num = Math.Max(Math.Abs(point2D.x - x), Math.Abs(point2D.y - y));
					Cell cell4 = cell.ParentZone.GetCell(x, y);
					if (num < list2.Count - 1 || XRLCore.CurrentFrame % 32 < 16)
					{
						if (num > Range)
						{
							Buffer.Write("&K" + list2[num].DisplayChar);
						}
						else if (((!cell4.IsVisible() || !cell4.IsLit()) && (ExtraVisibility == null || !cell4.HasObject(ExtraVisibility))) || XRLCore.player.GetConfusion() > 0)
						{
							Buffer.Write("&Y" + list2[num].DisplayChar);
						}
						else if (((cell4.IsVisible() && cell4.IsLit()) || (ExtraVisibility != null && cell4.HasObject(ExtraVisibility))) && (cell4.IsEmpty() || (UsePassability != null && cell4.IsPassable(UsePassability))))
						{
							text = "&G";
							Buffer.Write("X");
						}
						else
						{
							text = "&R";
							Buffer.Write("X");
						}
					}
					break;
				}
				case PickStyle.Cone:
				{
					XRL.Rules.Geometry.GetCone(Location2D.get(point2D.x, point2D.y), Location2D.get(x, y), Range, Radius, list);
					for (int l = 0; l < list.Count; l++)
					{
						Buffer.Goto(list[l].x, list[l].y);
						Cell cell3 = cell.ParentZone.GetCell(list[l].x, list[l].y);
						if (l < list.Count - 1)
						{
							if (list[l].Distance(Location2D.get(StartX, StartY)) > Range)
							{
								Buffer.Write("&KX");
							}
							else if ((!cell3.IsVisible() || !cell3.IsLit()) && (ExtraVisibility == null || !cell3.HasObject(ExtraVisibility)))
							{
								Buffer.Write("&KX");
							}
							else if (((cell3.IsVisible() && cell3.IsLit()) || (ExtraVisibility != null && cell3.HasObject(ExtraVisibility))) && cell3.HasObject(IsVisibleCombatObject) && XRLCore.player.GetConfusion() <= 0)
							{
								text = "&R";
								ConsoleChar currentChar2 = Buffer.CurrentChar;
								Buffer.Write("^R" + ColorUtility.StripBackgroundFormatting(cell3.Render(Buffer.CurrentChar, cell3.IsVisible(), cell3.GetLight(), cell3.IsExplored(), bAlt: false)));
								currentChar2.Detail = ColorUtility.ColorMap['R'];
							}
							else
							{
								text = "&yX";
								Buffer.Write(text);
							}
						}
					}
					if (XRLCore.CurrentFrame % 32 < 16)
					{
						Buffer.Goto(x, y);
						Buffer.Write("{{W|X}}");
					}
					break;
				}
				case PickStyle.Line:
				{
					if (list2.Count == 0)
					{
						Buffer.Goto(x, y);
						Buffer.Write("{{W|X}}");
						break;
					}
					for (int k = ((!Origin.HasValue) ? 1 : 0); k < list2.Count; k++)
					{
						Buffer.Goto(list2[k].X, list2[k].Y);
						Cell cell2 = cell.ParentZone.GetCell(list2[k].X, list2[k].Y);
						if (k < list2.Count - 1 || XRLCore.CurrentFrame % 32 < 16)
						{
							if (k > Radius)
							{
								Buffer.Write("&K" + list2[k].DisplayChar);
							}
							else if ((!cell2.IsVisible() || !cell2.IsLit()) && (ExtraVisibility == null || !cell2.HasObject(ExtraVisibility)))
							{
								Buffer.Write("&K" + list2[k].DisplayChar);
							}
							else if (((cell2.IsVisible() && cell2.IsLit()) || (ExtraVisibility != null && cell2.HasObject(ExtraVisibility))) && cell2.HasObject(IsVisibleCombatObject) && XRLCore.player.GetConfusion() <= 0)
							{
								text = "&R";
								ConsoleChar currentChar = Buffer.CurrentChar;
								Buffer.Write("^R" + ColorUtility.StripBackgroundFormatting(cell2.Render(Buffer.CurrentChar, cell2.IsVisible(), cell2.GetLight(), cell2.IsExplored(), bAlt: false)));
								currentChar.Detail = ColorUtility.ColorMap['R'];
							}
							else
							{
								text = "&W";
								Buffer.Write(text + list2[k].DisplayChar);
							}
						}
					}
					break;
				}
				case PickStyle.Circle:
				{
					int x4 = x - Radius;
					int x5 = x + Radius;
					int y4 = y - Radius;
					int y5 = y + Radius;
					cell.ParentZone.Constrain(ref x4, ref y4, ref x5, ref y5);
					for (int m = x4; m <= x5; m++)
					{
						for (int n = y4; n <= y5; n++)
						{
							if (Math.Sqrt((m - x) * (m - x) + (n - y) * (n - y)) <= (double)Radius)
							{
								Buffer.Goto(m, n);
								if (m == x && n == y)
								{
									Buffer.Write("{{W|X}}");
								}
								else
								{
									Buffer.Write("{{w|X}}");
								}
							}
						}
					}
					break;
				}
				case PickStyle.Burst:
				{
					int x2 = x - Radius;
					int x3 = x + Radius;
					int y2 = y - Radius;
					int y3 = y + Radius;
					cell.ParentZone.Constrain(ref x2, ref y2, ref x3, ref y3);
					string s = "&WX";
					string s2 = "&wX";
					if (point2D.ManhattanDistance(new Point2D(x, y)) > Range)
					{
						s = "&KX";
						s2 = "&KX";
					}
					for (int i = x2; i <= x3; i++)
					{
						for (int j = y2; j <= y3; j++)
						{
							Buffer.Goto(i, j);
							if (i == x && j == y)
							{
								Buffer.Write(s);
							}
							else
							{
								Buffer.Write(s2);
							}
						}
					}
					break;
				}
				}
				if (x < 40)
				{
					if (string.IsNullOrEmpty(Label))
					{
						Buffer.Goto(54, 0);
					}
					else
					{
						Buffer.Goto(52 - ColorUtility.LengthExceptFormatting(Label), 0);
					}
				}
				else
				{
					Buffer.Goto(1, 0);
				}
				if (!string.IsNullOrEmpty(Label))
				{
					Buffer.Write(Label);
					Buffer.Write("  ");
				}
				if (flag)
				{
					Buffer.Write("{{W|" + ControlManager.getCommandInputDescription("Accept", mapGlyphs: false) + "}}-select" + (CapabilityManager.AllowKeyboardHotkeys ? " | {{W|u}}nlock (F1))" : ""));
				}
				else
				{
					Buffer.Write("{{W|" + ControlManager.getCommandInputDescription("Accept", mapGlyphs: false) + "}}-select" + (CapabilityManager.AllowKeyboardHotkeys ? " | {{W|l}}ock (F1)" : ""));
				}
				_TextConsole.DrawBuffer(Buffer);
				if (!Keyboard.kbhit())
				{
					continue;
				}
				Keys keys = Keyboard.getvk(MapDirectionToArrows: true);
				if (keys == Keys.MouseEvent)
				{
					if (Keyboard.CurrentMouseEvent.Event == "PointerOver" && !flag3)
					{
						x = Keyboard.CurrentMouseEvent.x;
						y = Keyboard.CurrentMouseEvent.y;
					}
					if (Keyboard.CurrentMouseEvent.Event == "RightClick")
					{
						flag2 = true;
					}
					if (Keyboard.CurrentMouseEvent.Event == "PointerOver")
					{
						flag3 = false;
					}
				}
				if (keys == Keys.NumPad5 || keys == Keys.Escape)
				{
					flag2 = true;
				}
				if (keys == Keys.U || keys == Keys.L || keys == Keys.F1)
				{
					flag = !flag;
				}
				if (flag && XRLCore.player.GetConfusion() <= 0)
				{
					List<GameObject> list3 = new List<GameObject>();
					if (keys == Keys.NumPad1)
					{
						GetObjectListCone(x - 1, y + 1, list3, "sw", ObjectTest, ExtraVisibility);
					}
					if (keys == Keys.NumPad2)
					{
						GetObjectListCone(x, y + 1, list3, "s", ObjectTest, ExtraVisibility);
					}
					if (keys == Keys.NumPad3)
					{
						GetObjectListCone(x + 1, y + 1, list3, "se", ObjectTest, ExtraVisibility);
					}
					if (keys == Keys.NumPad4)
					{
						GetObjectListCone(x - 1, y, list3, "w", ObjectTest, ExtraVisibility);
					}
					if (keys == Keys.NumPad6)
					{
						GetObjectListCone(x + 1, y, list3, "e", ObjectTest, ExtraVisibility);
					}
					if (keys == Keys.NumPad7)
					{
						GetObjectListCone(x - 1, y - 1, list3, "nw", ObjectTest, ExtraVisibility);
					}
					if (keys == Keys.NumPad8)
					{
						GetObjectListCone(x, y - 1, list3, "n", ObjectTest, ExtraVisibility);
					}
					if (keys == Keys.NumPad9)
					{
						GetObjectListCone(x + 1, y - 1, list3, "ne", ObjectTest, ExtraVisibility);
					}
					if (list3.Count > 0)
					{
						Cell currentCell2 = list3[0].CurrentCell;
						if (Math.Abs(currentCell2.X - point2D.x) <= Range && Math.Abs(currentCell2.Y - point2D.y) <= Range)
						{
							x = currentCell2.X;
							y = currentCell2.Y;
						}
					}
					else
					{
						if (keys == Keys.NumPad1)
						{
							x--;
							y++;
						}
						if (keys == Keys.NumPad2)
						{
							y++;
						}
						if (keys == Keys.NumPad3)
						{
							x++;
							y++;
						}
						if (keys == Keys.NumPad4)
						{
							x--;
						}
						if (keys == Keys.NumPad6)
						{
							x++;
						}
						if (keys == Keys.NumPad7)
						{
							x--;
							y--;
						}
						if (keys == Keys.NumPad8)
						{
							y--;
						}
						if (keys == Keys.NumPad9)
						{
							x++;
							y--;
						}
					}
				}
				else
				{
					if (keys == Keys.NumPad1)
					{
						x--;
						y++;
					}
					if (keys == Keys.NumPad2)
					{
						y++;
					}
					if (keys == Keys.NumPad3)
					{
						x++;
						y++;
					}
					if (keys == Keys.NumPad4)
					{
						x--;
					}
					if (keys == Keys.NumPad6)
					{
						x++;
					}
					if (keys == Keys.NumPad7)
					{
						x--;
						y--;
					}
					if (keys == Keys.NumPad8)
					{
						y--;
					}
					if (keys == Keys.NumPad9)
					{
						x++;
						y--;
					}
				}
				if (EnforceRange)
				{
					if (x < StartX - Range)
					{
						x = StartX - Range;
					}
					else if (x > StartX + Range)
					{
						x = StartX + Range;
					}
					if (y < StartY - Range)
					{
						y = StartY - Range;
					}
					else if (y > StartY + Range)
					{
						y = StartY + Range;
					}
				}
				if (keys == Keys.F || keys == Keys.Space || keys == Keys.Enter || (keys == Keys.MouseEvent && Keyboard.CurrentMouseEvent.Event == "Command:CmdFire") || (keys == Keys.MouseEvent && Keyboard.CurrentMouseEvent.Event == "LeftClick"))
				{
					if (point2D.ManhattanDistance(new Point2D(x, y)) > Range && Style == PickStyle.Burst)
					{
						Popup.ShowBlock("You must select a location within " + Range + " tiles!");
					}
					else if (VisLevel == AllowVis.OnlyVisible && !cell.ParentZone.GetCell(x, y).IsVisible() && (ExtraVisibility == null || !cell.ParentZone.GetCell(x, y).HasObject(ExtraVisibility)))
					{
						Popup.ShowBlock("You may only select a visible square!");
					}
					else if (VisLevel == AllowVis.OnlyExplored && !cell.ParentZone.GetCell(x, y).IsExplored())
					{
						Popup.ShowBlock("You may only select an explored square!");
					}
					else
					{
						flag2 = true;
						result = cell.ParentZone.GetCell(x, y);
					}
				}
				cell.ParentZone.Constrain(ref x, ref y);
			}
		}
		_TextConsole.DrawBuffer(OldBuffer);
		GameManager.Instance.PopGameView();
		return result;
	}
}
