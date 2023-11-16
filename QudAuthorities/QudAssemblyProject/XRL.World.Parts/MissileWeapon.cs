using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using ConsoleLib.Console;
using XRL.Core;
using XRL.Language;
using XRL.Rules;
using XRL.UI;
using XRL.World.Capabilities;
using XRL.World.Effects;
using XRL.World.Parts.Skill;

namespace XRL.World.Parts;

[Serializable]
[UIView("FireMissileWeapon", true, false, false, "Menu,Targeting", null, false, 0, false)]
public class MissileWeapon : IPart
{
	public int AnimationDelay = 10;

	public int ShotsPerAction = 1;

	public int AmmoPerAction = 1;

	public int ShotsPerAnimation = 1;

	public int AimVarianceBonus;

	public int WeaponAccuracy;

	public int MaxRange = 999;

	public string VariableMaxRange;

	public string AmmoChar = "ù";

	public bool NoWildfire;

	public bool bShowShotsPerAction = true;

	public bool FiresManually = true;

	public string ProjectilePenetrationStat;

	public string SlotType = "Missile Weapon";

	public int EnergyCost = 1000;

	public int RangeIncrement = 3;

	public string Modifier = "Agility";

	public string Skill = "Rifle";

	[NonSerialized]
	private static Event eModifyAimVariance = new Event("ModifyAimVariance", "Amount", 0);

	[NonSerialized]
	private static Event eModifyIncomingAimVariance = new Event("ModifyIncomingAimVariance", "Amount", 0);

	[NonSerialized]
	private static Event eModifyMissileWeaponToHit = new Event("ModifyMissileWeaponToHit", "Amount", 0, "Target", (object)null, "Defender", (object)null);

	[NonSerialized]
	private static Event eModifyIncomingMissileWeaponToHit = new Event("ModifyIncomingMissileWeaponToHit", "Amount", 0, "Target", (object)null, "Defender", (object)null);

	[NonSerialized]
	private static DieRoll VarianceDieRoll = new DieRoll("2d20");

	private static bool bLocked = true;

	public static List<Pair> ListOfVisitedSquares(int x1, int y1, int x2, int y2)
	{
		List<Pair> list = new List<Pair>(Math.Abs(x2 - x1) + Math.Abs(y2 - y1));
		int num = y1;
		int num2 = x1;
		int num3 = x2 - x1;
		int num4 = y2 - y1;
		list.Add(new Pair(x1, y1));
		int num5;
		if (num4 < 0)
		{
			num5 = -1;
			num4 = -num4;
		}
		else
		{
			num5 = 1;
		}
		int num6;
		if (num3 < 0)
		{
			num6 = -1;
			num3 = -num3;
		}
		else
		{
			num6 = 1;
		}
		int num7 = 2 * num4;
		int num8 = 2 * num3;
		if (num8 >= num7)
		{
			int num9;
			int num10 = (num9 = num3);
			for (int i = 0; i < num3; i++)
			{
				num2 += num6;
				num9 += num7;
				if (num9 > num8)
				{
					num += num5;
					num9 -= num8;
					if (num9 + num10 < num8)
					{
						list.Add(new Pair(num2, num - num5));
					}
					else if (num9 + num10 > num8)
					{
						list.Add(new Pair(num2 - num6, num));
					}
				}
				list.Add(new Pair(num2, num));
				num10 = num9;
			}
		}
		else
		{
			int num9;
			int num10 = (num9 = num4);
			for (int i = 0; i < num4; i++)
			{
				num += num5;
				num9 += num8;
				if (num9 > num7)
				{
					num2 += num6;
					num9 -= num7;
					if (num9 + num10 < num7)
					{
						list.Add(new Pair(num2 - num6, num));
					}
					else if (num9 + num10 > num7)
					{
						list.Add(new Pair(num2, num - num5));
					}
				}
				list.Add(new Pair(num2, num));
				num10 = num9;
			}
		}
		return list;
	}

	public static bool CheckWallIntersection(Zone Z, int x0, int y0, int x1, int y1)
	{
		if (x0 == x1 && y0 == y1)
		{
			return false;
		}
		int num = (int)Math.Floor((float)x1 / 3f);
		int num2 = (int)Math.Floor((float)y1 / 3f);
		if (num < 80 && num2 < 25 && Z.MissileMap[num][num2] == MissileMapType.Wall && !Z.GetCell(num, num2).HasObjectWithPart("Combat"))
		{
			return true;
		}
		foreach (Pair item in ListOfVisitedSquares(x0, y0, x1, y1))
		{
			int num3 = (int)Math.Floor((float)item.x / 3f);
			int num4 = (int)Math.Floor((float)item.y / 3f);
			if (num3 == num && num4 == num2)
			{
				return false;
			}
			if (num3 >= 0 && num4 >= 0 && num3 <= 79 && num4 <= 24 && Z.MissileMap[num3][num4] == MissileMapType.Wall && !Z.GetCell(num3, num4).HasObjectWithPart("Combat"))
			{
				return true;
			}
		}
		return false;
	}

	public static MissilePath CalculateMissilePath(Zone Z, int x0, int y0, int x1, int y1)
	{
		MissilePath missilePath = new MissilePath();
		missilePath.Angle = (float)Math.Atan2(x1 - x0, y1 - y0);
		missilePath.Cover = 9999f;
		int num = 3;
		int i = x0 * num;
		for (int num2 = (x0 + 1) * num; i < num2; i++)
		{
			int j = y0 * num;
			for (int num3 = (y0 + 1) * num; j < num3; j++)
			{
				int num4 = 0;
				int num5 = 0;
				for (int k = x1 * num; k < (x1 + 1) * num; k++)
				{
					for (int l = y1 * num; l < (y1 + 1) * num; l++)
					{
						if (CheckWallIntersection(Z, i, j, k, l))
						{
							num5++;
						}
						num4++;
					}
				}
				if (!(missilePath.Cover > (float)num5))
				{
					continue;
				}
				missilePath.Cover = num5;
				missilePath.x0 = i;
				missilePath.y0 = j;
				missilePath.x1 = -1f;
				missilePath.y1 = -1f;
				int num6 = 0;
				int num7 = 0;
				int num8 = 0;
				if (missilePath.Cover == 0f)
				{
					missilePath.x1 = x1 * num + 1;
					missilePath.y1 = y1 * num + 1;
					continue;
				}
				int m = x1 * num;
				for (int num9 = (x1 + 1) * num; m < num9; m++)
				{
					int n = y1 * num;
					for (int num10 = (y1 + 1) * num; n < num10; n++)
					{
						if (!CheckWallIntersection(Z, i, j, m, n))
						{
							num6 += m;
							num7 += n;
							num8++;
						}
					}
				}
				if (num8 != 0)
				{
					num6 /= num8;
					num7 /= num8;
					missilePath.x1 = num6;
					missilePath.y1 = num7;
					continue;
				}
				goto IL_0167;
			}
			continue;
			IL_0167:
			missilePath.x1 = x1 * num + 1;
			missilePath.y1 = y1 * num + 1;
			break;
		}
		bool flag = false;
		if (x0 == x1 && y0 == y1)
		{
			missilePath.Path.Add(Z.GetCell(x0, y0));
		}
		else
		{
			bool flag2 = Math.Abs(y1 - y0) > Math.Abs(x1 - x0);
			if (flag2)
			{
				int num11 = x0;
				x0 = y0;
				y0 = num11;
				int num12 = x1;
				x1 = y1;
				y1 = num12;
			}
			if (x0 > x1)
			{
				flag = true;
				int num13 = x1;
				x1 = x0;
				x0 = num13;
				int num14 = y1;
				y1 = y0;
				y0 = num14;
			}
			int num15 = x1 - x0;
			int num16 = Math.Abs(y1 - y0);
			double num17 = 0.0;
			double num18 = (double)num16 / (double)num15;
			int num19 = 0;
			int num20 = y0;
			num19 = ((y0 < y1) ? 1 : (-1));
			int num21 = 0;
			for (int num22 = x0; num22 <= x1; num22++)
			{
				num21++;
				if (flag2)
				{
					missilePath.Path.Add(Z.GetCell(num20, num22));
				}
				else
				{
					missilePath.Path.Add(Z.GetCell(num22, num20));
				}
				num17 += num18;
				if (num17 >= 0.5)
				{
					num20 += num19;
					num17 -= 1.0;
					if (num19 >= 0)
					{
					}
				}
			}
		}
		if (flag)
		{
			missilePath.Path.Reverse();
		}
		return missilePath;
	}

	public static void GetObjectListCone(int StartX, int StartY, List<GameObject> ObjectList, string Direction)
	{
		Look.GetObjectListCone(StartX, StartY, ObjectList, Direction);
	}

	public static string GetRoundCooldown(int nCooldown)
	{
		int num = Math.Max((int)Math.Ceiling((double)nCooldown / 10.0), 1);
		if (num == 1)
		{
			return "({{C|1}} turn)";
		}
		return "({{C|" + num + "}} turns)";
	}

	public static MissilePath ShowPicker(int StartX, int StartY, bool Locked, AllowVis VisLevel, int Range, bool BowOrRifle, GameObject Projectile, ref FireType FireType)
	{
		GameManager.Instance.PushGameView("FireMissileWeapon");
		GameObject gameObject = null;
		if (Locked && The.Core.ConfusionLevel <= 0 && The.Core.FuriousConfusion <= 0)
		{
			gameObject = Sidebar.CurrentTarget ?? IComponent<GameObject>.ThePlayer.GetNearestVisibleObject(Hostile: true, "Combat");
		}
		MissilePath missilePath = new MissilePath();
		TextConsole textConsole = Popup._TextConsole;
		TextConsole.LoadScrapBuffers();
		ScreenBuffer scrapBuffer = TextConsole.ScrapBuffer;
		bool flag = false;
		bool flag2 = true;
		if (gameObject != null)
		{
			Cell cell = gameObject.CurrentCell;
			if (cell != null && IComponent<GameObject>.ThePlayer.InSameZone(cell))
			{
				StartX = cell.X;
				StartY = cell.Y;
			}
		}
		Cell cell2 = IComponent<GameObject>.ThePlayer.CurrentCell;
		if (cell2 != null)
		{
			int num = StartX;
			int num2 = StartY;
			while (!flag)
			{
				Event.ResetStringbuilderPool();
				Event.ResetGameObjectListPool();
				bool flag3 = false;
				bool flag4 = false;
				bool flag5 = false;
				bool flag6 = false;
				bool flag7 = false;
				bool flag8 = false;
				bool flag9 = false;
				bool flag10 = false;
				XRLCore.Core.RenderMapToBuffer(scrapBuffer);
				List<Point> list = Zone.Line(cell2.X, cell2.Y, num, num2);
				missilePath = CalculateMissilePath(cell2.ParentZone, cell2.X, cell2.Y, num, num2);
				string text = "&W";
				scrapBuffer.Goto(0, 2);
				Cell cell3 = cell2.ParentZone.GetCell(num, num2);
				if (list.Count == 0)
				{
					scrapBuffer.Goto(num, num2);
					scrapBuffer.Write("&WX");
				}
				else if (XRLCore.Core.ConfusionLevel <= 0)
				{
					for (int i = 1; i < list.Count; i++)
					{
						scrapBuffer.Goto(list[i].X, list[i].Y);
						Cell cell4 = cell2.ParentZone.GetCell(list[i].X, list[i].Y);
						if (i < list.Count - 1 || XRLCore.CurrentFrame % 32 < 16)
						{
							if (i > Range)
							{
								scrapBuffer.Write("&K" + list[i].DisplayChar);
							}
							else if (!cell4.IsVisible() || !cell4.IsLit())
							{
								scrapBuffer.Write("&K" + list[i].DisplayChar);
							}
							else if (cell4.IsVisible() && cell4.IsLit() && cell4.HasObjectWithPart("Combat"))
							{
								text = "&R";
								ConsoleChar currentChar = scrapBuffer.CurrentChar;
								scrapBuffer.Write("^R" + cell4.Render(scrapBuffer.CurrentChar, cell4.IsVisible(), cell4.GetLight(), cell4.IsExplored(), bAlt: false));
								currentChar.Detail = ColorUtility.ColorMap['R'];
							}
							else
							{
								text = "&W";
								scrapBuffer.Write(text + list[i].DisplayChar);
							}
						}
					}
				}
				List<Pair> list2 = ListOfVisitedSquares(cell2.X, cell2.Y, num, num2);
				int num3 = 0;
				foreach (Pair item in list2)
				{
					if (num3 != 0)
					{
						int num4 = (int)CalculateMissilePath(cell2.ParentZone, cell2.X, cell2.Y, item.x, item.y).Cover;
						if (XRLCore.Core.ConfusionLevel > 0)
						{
							num4 = 0;
						}
						string text2 = "&y";
						text2 = ((num4 >= 9) ? "&R" : ((num4 >= 6) ? "&r" : ((num4 >= 3) ? "&w" : ((num4 < 1) ? "&G" : "&g"))));
						scrapBuffer.Goto(item.x, item.y);
						if (num3 != list2.Count - 1)
						{
							scrapBuffer.Write(text2 + "ù");
						}
						else
						{
							string text3 = text2.Replace('&', '^');
							scrapBuffer.Write(text3 + scrapBuffer[item.x, item.y].Char);
						}
					}
					num3++;
				}
				int x = ((num >= 40) ? 1 : 43);
				scrapBuffer.Goto(x, 0);
				if (bLocked)
				{
					scrapBuffer.Write("{{W|space}}-select | {{W|u}}nlock (F1)");
				}
				else
				{
					scrapBuffer.Write("{{W|space}}-select | {{W|l}}ock (F1)");
				}
				if (XRLCore.Core.ConfusionLevel > 0 || XRLCore.Core.FuriousConfusion > 0)
				{
					BowOrRifle = false;
				}
				bool flag11 = false;
				GameObject combatTarget = cell3.GetCombatTarget(IComponent<GameObject>.ThePlayer, IgnoreFlight: true, IgnoreAttackable: false, IgnorePhase: false, 5, Projectile, null, AllowInanimate: false);
				int num5 = 1;
				List<(string, string)> list3 = new List<(string, string)>();
				if (BowOrRifle && IComponent<GameObject>.ThePlayer.HasSkill("Rifle_DrawABead"))
				{
					Rifle_DrawABead rifle_DrawABead = IComponent<GameObject>.ThePlayer.GetPart("Rifle_DrawABead") as Rifle_DrawABead;
					RifleMark rifleMark = ((combatTarget != null) ? (combatTarget.GetEffect("RifleMark") as RifleMark) : null);
					scrapBuffer.Goto(x, num5);
					num5++;
					if (rifleMark != null && rifleMark.Marker.IsPlayer())
					{
						scrapBuffer.Write("{{G|marked target}}");
						flag11 = true;
					}
					else if (rifle_DrawABead != null)
					{
						if (rifle_DrawABead.MarkCooldown <= 0)
						{
							flag3 = true;
							scrapBuffer.Write("{{W|m}} or {{W|+}} - mark target");
							list3.Add(("mark target", "MarkTarget"));
						}
						else
						{
							scrapBuffer.Write("{{K|m}} or {{K|+}} - mark target " + GetRoundCooldown(rifle_DrawABead.MarkCooldown) + "}}");
						}
					}
				}
				if (BowOrRifle && IComponent<GameObject>.ThePlayer.HasSkill("Rifle_DrawABead") && combatTarget != null)
				{
					int num6 = 0;
					if (IComponent<GameObject>.ThePlayer.HasSkill("Rifle_SuppressiveFire"))
					{
						scrapBuffer.Goto(x, num5);
						num5++;
						if (IComponent<GameObject>.ThePlayer.HasSkill("Rifle_FlatteningFire"))
						{
							flag5 = Rifle_FlatteningFire.MeetsCriteria(combatTarget);
						}
						if (!flag11)
						{
							if (flag5)
							{
								scrapBuffer.Write("{{K|1 - Flattening Fire (not marked)}}");
							}
							else
							{
								scrapBuffer.Write("{{K|1 - Suppressive Fire (not marked)}}");
							}
						}
						else if (num6 <= 0)
						{
							flag4 = true;
							if (flag5)
							{
								scrapBuffer.Write("{{W|1}} - {{W|Flattening Fire}}");
								list3.Add(("Flattening Fire", "SupressiveFire"));
							}
							else
							{
								scrapBuffer.Write("{{W|1}} - Suppressive Fire");
								list3.Add(("Suppressive Fire", "SupressiveFire"));
							}
						}
						else
						{
							scrapBuffer.Write("{{K|1 - Suppressive Fire " + GetRoundCooldown(num6) + "}}");
						}
					}
					if (IComponent<GameObject>.ThePlayer.HasSkill("Rifle_WoundingFire"))
					{
						scrapBuffer.Goto(x, num5);
						num5++;
						if (IComponent<GameObject>.ThePlayer.HasSkill("Rifle_DisorientingFire"))
						{
							flag7 = Rifle_DisorientingFire.MeetsCriteria(combatTarget);
						}
						if (!flag11)
						{
							if (flag7)
							{
								scrapBuffer.Write("&K2 - Disorienting Fire (not marked)");
							}
							else
							{
								scrapBuffer.Write("&K2 - Wounding Fire (not marked)");
							}
						}
						else if (num6 <= 0)
						{
							flag6 = true;
							if (flag7)
							{
								scrapBuffer.Write("{{W|2}} - {{W|Disorienting Fire}}");
								list3.Add(("Disorienting Fire", "WoundingFire"));
							}
							else
							{
								scrapBuffer.Write("{{W|2}} - Wounding Fire");
								list3.Add(("Wounding Fire", "WoundingFire"));
							}
						}
						else
						{
							scrapBuffer.Write("{{K|2 - Wounding Fire " + GetRoundCooldown(num6) + "}}");
						}
					}
					if (IComponent<GameObject>.ThePlayer.HasSkill("Rifle_SureFire"))
					{
						scrapBuffer.Goto(x, num5);
						num5++;
						if (IComponent<GameObject>.ThePlayer.HasSkill("Rifle_BeaconFire"))
						{
							flag9 = Rifle_BeaconFire.MeetsCriteria(combatTarget);
						}
						if (!flag11)
						{
							if (flag9)
							{
								scrapBuffer.Write("{{K|3 - Beacon Fire (not marked)}}");
							}
							else
							{
								scrapBuffer.Write("{{K|3 - Sure Fire (not marked)}}");
							}
						}
						else if (num6 <= 0)
						{
							flag8 = true;
							if (IComponent<GameObject>.ThePlayer.HasSkill("Rifle_BeaconFire"))
							{
								flag9 = Rifle_BeaconFire.MeetsCriteria(combatTarget);
							}
							if (flag9)
							{
								scrapBuffer.Write("{{W|3}} - {{W|Beacon Fire}}");
								list3.Add(("Beacon Fire", "SureFire"));
							}
							else
							{
								scrapBuffer.Write("{{W|3}} - Sure Fire");
								list3.Add(("Sure Fire", "SureFire"));
							}
						}
						else
						{
							scrapBuffer.Write("{{K|3 - Sure Fire " + GetRoundCooldown(num6) + "}}");
						}
					}
					if (IComponent<GameObject>.ThePlayer.HasSkill("Rifle_OneShot"))
					{
						scrapBuffer.Goto(x, num5);
						num5++;
						Rifle_OneShot rifle_OneShot = IComponent<GameObject>.ThePlayer.GetPart("Rifle_OneShot") as Rifle_OneShot;
						if (!flag11)
						{
							scrapBuffer.Write("{{K|4 - Ultra Fire (not marked)}}");
						}
						else if (rifle_OneShot.Cooldown <= 0)
						{
							flag10 = true;
							scrapBuffer.Write("{{W|4}} - Ultra Fire");
							list3.Add(("Ultra Fire", "UltraFire"));
						}
						else
						{
							scrapBuffer.Write("{{K|4 - Ultra Fire " + GetRoundCooldown(rifle_OneShot.Cooldown) + "}}");
						}
					}
				}
				if (list3.Count > 0)
				{
					scrapBuffer.Goto(x, num5);
					scrapBuffer.Write("[{{W|" + ControlManager.getCommandInputDescription("CmdMissileWeaponMenu") + "}}] Menu");
				}
				textConsole.DrawBuffer(scrapBuffer);
				if (!Keyboard.kbhit())
				{
					continue;
				}
				Keys keys = Keyboard.getvk(MapDirectionToArrows: true);
				string text4 = null;
				if (keys == Keys.MouseEvent && Keyboard.CurrentMouseEvent.Event == "Passthrough:CmdMissileWeaponMenu")
				{
					int num7 = Popup.ShowOptionList("Select Fire Mode", list3.Select(((string, string) m) => m.Item1).ToArray(), null, 0, null, 60, RespectOptionNewlines: false, AllowEscape: true);
					if (num7 >= 0)
					{
						text4 = list3[num7].Item2;
					}
					else
					{
						keys = Keys.None;
					}
				}
				if (text4 == "MarkTarget")
				{
					keys = Keys.M;
				}
				if (text4 == "SupressiveFire")
				{
					keys = Keys.D1;
				}
				if (text4 == "WoundingFire")
				{
					keys = Keys.D2;
				}
				if (text4 == "SureFire")
				{
					keys = Keys.D3;
				}
				if (text4 == "UltraFire")
				{
					keys = Keys.D4;
				}
				if (keys == Keys.MouseEvent)
				{
					if (Keyboard.CurrentMouseEvent.Event == "PointerOver" && !flag2)
					{
						num = Keyboard.CurrentMouseEvent.x;
						num2 = Keyboard.CurrentMouseEvent.y;
					}
					if (Keyboard.CurrentMouseEvent.Event == "PointerOver")
					{
						flag2 = false;
					}
				}
				if (keys == Keys.NumPad5 || keys == Keys.Escape || (keys == Keys.MouseEvent && Keyboard.CurrentMouseEvent.Event == "RightClick"))
				{
					flag = true;
					GameManager.Instance.PopGameView();
					return null;
				}
				if (keys == Keys.U || keys == Keys.L || keys == Keys.F1)
				{
					bLocked = !bLocked;
				}
				if (bLocked)
				{
					List<GameObject> list4 = new List<GameObject>();
					if (keys == Keys.NumPad1)
					{
						GetObjectListCone(num - 1, num2 + 1, list4, "sw");
					}
					if (keys == Keys.NumPad2)
					{
						GetObjectListCone(num, num2 + 1, list4, "s");
					}
					if (keys == Keys.NumPad3)
					{
						GetObjectListCone(num + 1, num2 + 1, list4, "se");
					}
					if (keys == Keys.NumPad4)
					{
						GetObjectListCone(num - 1, num2, list4, "w");
					}
					if (keys == Keys.NumPad6)
					{
						GetObjectListCone(num + 1, num2, list4, "e");
					}
					if (keys == Keys.NumPad7)
					{
						GetObjectListCone(num - 1, num2 - 1, list4, "nw");
					}
					if (keys == Keys.NumPad8)
					{
						GetObjectListCone(num, num2 - 1, list4, "n");
					}
					if (keys == Keys.NumPad9)
					{
						GetObjectListCone(num + 1, num2 - 1, list4, "ne");
					}
					if (list4.Count > 0 && XRLCore.Core.ConfusionLevel <= 0 && XRLCore.Core.FuriousConfusion <= 0)
					{
						Cell cell5 = list4[0].CurrentCell;
						if (Math.Abs(cell5.X - cell2.X) <= Range && Math.Abs(cell5.Y - cell2.Y) <= Range)
						{
							num = cell5.X;
							num2 = cell5.Y;
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
				if ((keys == Keys.Oemplus || keys == Keys.M || keys == Keys.Add) && flag3)
				{
					FireType = FireType.Mark;
					GameManager.Instance.PopGameView();
					return missilePath;
				}
				if (flag4 && (keys == Keys.Oem1 || keys == Keys.D1))
				{
					if (VisLevel == AllowVis.OnlyVisible && !cell2.ParentZone.GetCell(num, num2).IsVisible())
					{
						Popup.ShowBlock("You may only select a visible square!");
					}
					else
					{
						if (VisLevel != AllowVis.OnlyExplored || cell2.ParentZone.GetCell(num, num2).IsExplored())
						{
							(IComponent<GameObject>.ThePlayer.GetPart("Rifle_DrawABead") as Rifle_DrawABead).ClearMark();
							FireType = FireType.SuppressingFire;
							if (flag5)
							{
								FireType = FireType.FlatteningFire;
							}
							GameManager.Instance.PopGameView();
							return missilePath;
						}
						Popup.ShowBlock("You may only select an explored square!");
					}
				}
				if (flag6 && (keys == Keys.OemQuestion || keys == Keys.D2))
				{
					if (VisLevel == AllowVis.OnlyVisible && !cell2.ParentZone.GetCell(num, num2).IsVisible())
					{
						Popup.ShowBlock("You may only select a visible square!");
					}
					else
					{
						if (VisLevel != AllowVis.OnlyExplored || cell2.ParentZone.GetCell(num, num2).IsExplored())
						{
							(IComponent<GameObject>.ThePlayer.GetPart("Rifle_DrawABead") as Rifle_DrawABead).ClearMark();
							FireType = FireType.WoundingFire;
							if (flag7)
							{
								FireType = FireType.DisorientingFire;
							}
							GameManager.Instance.PopGameView();
							return missilePath;
						}
						Popup.ShowBlock("You may only select an explored square!");
					}
				}
				if (flag8 && (keys == Keys.Oemtilde || keys == Keys.D3))
				{
					if (VisLevel == AllowVis.OnlyVisible && !cell2.ParentZone.GetCell(num, num2).IsVisible())
					{
						Popup.ShowBlock("You may only select a visible square!");
					}
					else
					{
						if (VisLevel != AllowVis.OnlyExplored || cell2.ParentZone.GetCell(num, num2).IsExplored())
						{
							(IComponent<GameObject>.ThePlayer.GetPart("Rifle_DrawABead") as Rifle_DrawABead).ClearMark();
							FireType = FireType.SureFire;
							if (flag9)
							{
								FireType = FireType.BeaconFire;
							}
							GameManager.Instance.PopGameView();
							return missilePath;
						}
						Popup.ShowBlock("You may only select an explored square!");
					}
				}
				if (flag10 && (keys == Keys.Oem4 || keys == Keys.D4))
				{
					if (VisLevel == AllowVis.OnlyVisible && !cell2.ParentZone.GetCell(num, num2).IsVisible())
					{
						Popup.ShowBlock("You may only select a visible square!");
					}
					else
					{
						if (VisLevel != AllowVis.OnlyExplored || cell2.ParentZone.GetCell(num, num2).IsExplored())
						{
							Rifle_OneShot rifle_OneShot2 = IComponent<GameObject>.ThePlayer.GetPart("Rifle_OneShot") as Rifle_OneShot;
							Event @event = Event.New("BeforeCooldownActivatedAbility", "AbilityEntry", null, "Turns", 1010, "Tags", "Agility");
							if (IComponent<GameObject>.ThePlayer.FireEvent(@event) && @event.GetIntParameter("Turns") != 0)
							{
								rifle_OneShot2.Cooldown = 1010;
							}
							FireType = FireType.OneShot;
							(IComponent<GameObject>.ThePlayer.GetPart("Rifle_DrawABead") as Rifle_DrawABead).ClearMark();
							GameManager.Instance.PopGameView();
							return missilePath;
						}
						Popup.ShowBlock("You may only select an explored square!");
					}
				}
				if (keys == Keys.F || keys == Keys.Space || keys == Keys.Enter || (keys == Keys.MouseEvent && Keyboard.CurrentMouseEvent.Event == "Command:CmdFire") || (keys == Keys.MouseEvent && Keyboard.CurrentMouseEvent.Event == "LeftClick"))
				{
					if (VisLevel == AllowVis.OnlyVisible && !cell2.ParentZone.GetCell(num, num2).IsVisible())
					{
						Popup.ShowBlock("You may only select a visible square!");
					}
					else
					{
						if (VisLevel != AllowVis.OnlyExplored || cell2.ParentZone.GetCell(num, num2).IsExplored())
						{
							GameManager.Instance.PopGameView();
							return missilePath;
						}
						Popup.ShowBlock("You may only select an explored square!");
					}
				}
				if (num < 0)
				{
					num = 0;
				}
				if (num >= cell2.ParentZone.Width)
				{
					num = cell2.ParentZone.Width - 1;
				}
				if (num2 < 0)
				{
					num2 = 0;
				}
				if (num2 >= cell2.ParentZone.Height)
				{
					num2 = cell2.ParentZone.Height - 1;
				}
			}
		}
		GameManager.Instance.PopGameView();
		return null;
	}

	public void UpdateHeavyWeaponMovementPenalty(GameObject who = null, bool ForceRemove = false)
	{
		if (who == null)
		{
			who = ParentObject.Equipped;
			if (who == null)
			{
				return;
			}
		}
		if (!ForceRemove && Skill == "HeavyWeapons" && !who.HasSkill("HeavyWeapons_Tank"))
		{
			base.StatShifter.SetStatShift(who, "MoveSpeed", 25);
		}
		else
		{
			base.StatShifter.SetStatShift(who, "MoveSpeed", 0);
		}
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != AfterAddSkillEvent.ID && ID != AfterRemoveSkillEvent.ID && ID != EquippedEvent.ID && ID != GenericQueryEvent.ID && (ID != AdjustTotalWeightEvent.ID || !(Skill == "HeavyWeapons")) && (ID != GetEnergyCostEvent.ID || !(Skill == "HeavyWeapons")) && ID != GetShortDescriptionEvent.ID && ID != QueryEquippableListEvent.ID)
		{
			return ID == UnequippedEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(AfterAddSkillEvent E)
	{
		if (E.Skill.Name == "HeavyWeapons_Tank")
		{
			UpdateHeavyWeaponMovementPenalty();
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(AfterRemoveSkillEvent E)
	{
		if (E.Skill.Name == "HeavyWeapons_Tank")
		{
			UpdateHeavyWeaponMovementPenalty();
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EquippedEvent E)
	{
		UpdateHeavyWeaponMovementPenalty(E.Actor);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(UnequippedEvent E)
	{
		UpdateHeavyWeaponMovementPenalty(E.Actor, ForceRemove: true);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GenericQueryEvent E)
	{
		if (E.Query == "PhaseHarmonicEligible" && ModPhaseHarmonic.IsProjectileCompatible(GetProjectileBlueprintEvent.GetFor(ParentObject)))
		{
			E.Result = true;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(QueryEquippableListEvent E)
	{
		if (!E.List.Contains(ParentObject) && ValidSlotType(E.SlotType))
		{
			E.List.Add(ParentObject);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetShortDescriptionEvent E)
	{
		E.Postfix.Append(GetDetailedStats());
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(AdjustTotalWeightEvent E)
	{
		if (Skill == "HeavyWeapons")
		{
			GameObject gameObject = ParentObject.Equipped ?? ParentObject.InInventory;
			if (gameObject != null && gameObject.HasSkill("HeavyWeapons_StrappingShoulders"))
			{
				E.AdjustWeight(0.5);
			}
		}
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "CommandFireMissile");
		Object.RegisterPartEvent(this, "PerformFileMissile");
		base.Register(Object);
	}

	public string GetDetailedStats()
	{
		if (IComponent<GameObject>.ThePlayer == null)
		{
			return "";
		}
		StringBuilder stringBuilder = Event.NewStringBuilder();
		stringBuilder.Append("{{rules|");
		if (Skill == "Pistol")
		{
			stringBuilder.Append("\nWeapon Class: Pistol");
		}
		else if (Skill == "Rifle")
		{
			stringBuilder.Append("\nWeapon Class: Bows && Rifles");
		}
		else if (Skill == "HeavyWeapons")
		{
			stringBuilder.Append("\nWeapon Class: Heavy Weapon");
		}
		else if (!string.IsNullOrEmpty(Skill))
		{
			stringBuilder.Append("\nWeapon Class: ").Append(Skill);
		}
		if (WeaponAccuracy <= 0)
		{
			stringBuilder.Append("\nAccuracy: Very High");
		}
		else if (WeaponAccuracy < 5)
		{
			stringBuilder.Append("\nAccuracy: High");
		}
		else if (WeaponAccuracy < 10)
		{
			stringBuilder.Append("\nAccuracy: Medium");
		}
		else if (WeaponAccuracy < 25)
		{
			stringBuilder.Append("\nAccuracy: Low");
		}
		else
		{
			stringBuilder.Append("\nAccuracy: Very Low");
		}
		if (AmmoPerAction > 1)
		{
			stringBuilder.Append("\nMultiple ammo used per shot: " + AmmoPerAction);
		}
		if (ShotsPerAction > 1 && bShowShotsPerAction)
		{
			stringBuilder.Append("\nMultiple projectiles per shot: " + ShotsPerAction);
		}
		if (NoWildfire)
		{
			stringBuilder.Append("\nSpray fire: This item can be fired while adjacent to multiple enemies without risk of the shot going wild.");
		}
		if (Skill == "HeavyWeapons")
		{
			stringBuilder.Append("\n-25 move speed");
		}
		if (!string.IsNullOrEmpty(ProjectilePenetrationStat))
		{
			stringBuilder.Append("\nProjectiles fired with this weapon receive bonus penetration based on the wielder's ").Append(ProjectilePenetrationStat).Append('.');
		}
		stringBuilder.Append("}}");
		return stringBuilder.ToString();
	}

	private static int toCoord(float pos)
	{
		return (int)Math.Floor(pos / 3f);
	}

	public static List<Point> CalculateBulletTrajectory(out bool PlayerInvolved, out bool CameNearPlayer, out Cell NearPlayerCell, MissilePath Path, GameObject projectile = null, GameObject weapon = null, GameObject owner = null, Zone zone = null, string AimVariance = null, int FlatVariance = 0, int WeaponVariance = 0, bool IntendedPathOnly = false)
	{
		PlayerInvolved = false;
		CameNearPlayer = false;
		NearPlayerCell = null;
		double num = Math.Atan2((double)Path.x1 - (double)Path.x0, (double)Path.y1 - (double)Path.y0).normalizeRadians();
		List<Pair> list = new List<Pair>(32);
		int num2 = (int)(num * 57.324840764331213);
		Path.Angle = num2;
		int num3 = WeaponVariance + FlatVariance + ((!string.IsNullOrEmpty(AimVariance)) ? AimVariance.RollCached() : 0);
		if (weapon != null && weapon.HasRegisteredEvent("ModifyMissileWeaponAngle"))
		{
			Event @event = Event.New("ModifyMissileWeaponAngle", "Attacker", owner, "Projectile", projectile, "Angle", num, "Mod", num3);
			weapon.FireEvent(@event);
			num = (double)@event.GetParameter("Angle");
			num3 = @event.GetIntParameter("Mod");
		}
		num += (double)num3 * 0.0174532925;
		double num4 = Math.Sin(num);
		double num5 = Math.Cos(num);
		double num6 = Path.x0;
		double num7 = Path.y0;
		while (Math.Floor(num6) >= 0.0 && Math.Floor(num6) <= 237.0 && Math.Floor(num7) >= 0.0 && Math.Floor(num7) <= 72.0)
		{
			num6 += num4;
			num7 += num5;
		}
		list.AddRange(ListOfVisitedSquares((int)Path.x0, (int)Path.y0, (int)num6, (int)num7));
		if (zone != null && projectile != null && !IntendedPathOnly)
		{
			Cell cell = null;
			for (int i = 0; i < list.Count; i++)
			{
				int x = toCoord(list[i].x);
				int y = toCoord(list[i].y);
				Cell cell2 = zone.GetCell(x, y);
				if (cell2 == null || cell2 == cell)
				{
					continue;
				}
				cell = cell2;
				if (i == 0 || ((!cell2.HasObjectWithRegisteredEvent("RefractLight") || !projectile.HasTagOrProperty("Light")) && !cell2.HasObjectWithRegisteredEvent("ReflectProjectile")))
				{
					continue;
				}
				bool flag = true;
				GameObject obj = null;
				string clip = null;
				int num8 = -1;
				string verb = null;
				if (cell2.HasObjectWithRegisteredEvent("RefractLight") && projectile.HasTagOrProperty("Light"))
				{
					Event event2 = Event.New("RefractLight");
					event2.SetParameter("Projectile", projectile);
					event2.SetParameter("Attacker", owner);
					event2.SetParameter("Cell", cell2);
					event2.SetParameter("Angle", Path.Angle);
					event2.SetParameter("Direction", Stat.Random(0, 359));
					event2.SetParameter("Verb", null);
					event2.SetParameter("Sound", "refract");
					event2.SetParameter("By", (object)null);
					flag = cell2.FireEvent(event2);
					if (!flag)
					{
						obj = event2.GetGameObjectParameter("By");
						clip = event2.GetParameter<string>("Sound");
						verb = event2.GetStringParameter("Verb") ?? "refract";
						num8 = event2.GetIntParameter("Direction").normalizeDegrees();
					}
				}
				if (flag && cell2.HasObjectWithRegisteredEvent("ReflectProjectile"))
				{
					Event event3 = Event.New("ReflectProjectile");
					event3.SetParameter("Projectile", projectile);
					event3.SetParameter("Attacker", owner);
					event3.SetParameter("Cell", cell2);
					event3.SetParameter("Angle", Path.Angle);
					event3.SetParameter("Direction", Stat.Random(0, 359));
					event3.SetParameter("Verb", null);
					event3.SetParameter("Sound", "refract");
					event3.SetParameter("By", (object)null);
					flag = cell2.FireEvent(event3);
					if (!flag)
					{
						obj = event3.GetGameObjectParameter("By");
						clip = event3.GetParameter<string>("Sound");
						verb = event3.GetStringParameter("Verb") ?? "reflect";
						num8 = event3.GetIntParameter("Direction").normalizeDegrees();
					}
				}
				if (flag || !GameObject.validate(ref obj))
				{
					continue;
				}
				if (obj.IsPlayer())
				{
					PlayerInvolved = true;
				}
				else
				{
					GameObject gameObject = obj.Equipped ?? obj.InInventory ?? obj.Implantee;
					if (gameObject != null && gameObject.IsPlayer())
					{
						PlayerInvolved = true;
					}
				}
				obj?.pPhysics?.PlayWorldSound(clip, 0.5f, 0f, combat: true);
				IComponent<GameObject>.XDidYToZ(obj, verb, projectile, null, "!", null, obj);
				float num9 = list[i].x;
				float num10 = list[i].y;
				float num11 = num9;
				float num12 = num10;
				float num13 = (float)Math.Sin((float)num8 * ((float)Math.PI / 180f));
				float num14 = (float)Math.Cos((float)num8 * ((float)Math.PI / 180f));
				list.RemoveRange(i, list.Count - i);
				Cell cell3 = cell2;
				do
				{
					num11 += num13;
					num12 += num14;
					Cell cell4 = zone.GetCell(toCoord(num11), toCoord(num12));
					if (cell4 == null)
					{
						break;
					}
					if (cell4 == cell2)
					{
						continue;
					}
					list.Add(new Pair((int)num11, (int)num12));
					if (cell4 != cell3)
					{
						if (cell4.GetCombatTarget(owner, IgnoreFlight: true, IgnoreAttackable: false, IgnorePhase: false, 0, projectile, null, AllowInanimate: false) != null || cell4.HasSolidObjectForMissile(owner, projectile))
						{
							break;
						}
						cell3 = cell4;
					}
				}
				while (num11 > 0f && num11 < 237f && num12 > 0f && num12 < 72f);
			}
		}
		List<Point> list2 = new List<Point>(list.Count / 2);
		int num15 = int.MinValue;
		int j = 0;
		for (int count = list.Count; j < count; j++)
		{
			Pair pair = list[j];
			int num16 = toCoord(pair.x) + toCoord(pair.y) * 1000;
			if (num16 != num15)
			{
				list2.Add(new Point(toCoord(pair.x), toCoord(pair.y)));
				num15 = num16;
			}
		}
		if (IComponent<GameObject>.ThePlayer != null && zone != null && list2.Count > 0)
		{
			Cell cell5 = IComponent<GameObject>.ThePlayer.GetCurrentCell();
			if (cell5 != null)
			{
				Cell cell6 = zone.GetCell(list2[0]);
				if (cell6 != null && cell6.PathDistanceTo(cell5) >= 2)
				{
					bool flag2 = false;
					int k = 1;
					for (int count2 = list2.Count; k < count2; k++)
					{
						Cell cell7 = zone.GetCell(list2[k]);
						if (cell7 == cell5)
						{
							break;
						}
						if (flag2)
						{
							if (cell7.PathDistanceTo(cell5) >= 2)
							{
								CameNearPlayer = true;
								break;
							}
						}
						else if (cell7.PathDistanceTo(cell5) <= 1)
						{
							flag2 = true;
							NearPlayerCell = cell7;
						}
					}
				}
			}
		}
		return list2;
	}

	public static List<Point> CalculateBulletTrajectory(MissilePath Path, GameObject projectile = null, GameObject weapon = null, GameObject owner = null, Zone zone = null, string AimVariance = null, int FlatVariance = 0, int WeaponVariance = 0, bool IntendedPathOnly = false)
	{
		bool PlayerInvolved;
		bool CameNearPlayer;
		Cell NearPlayerCell;
		return CalculateBulletTrajectory(out PlayerInvolved, out CameNearPlayer, out NearPlayerCell, Path, projectile, weapon, owner, zone, AimVariance, FlatVariance, WeaponVariance, IntendedPathOnly);
	}

	public string GetSlotType()
	{
		string text = ParentObject.UsesSlots;
		if (string.IsNullOrEmpty(text))
		{
			text = SlotType;
		}
		if (text.IndexOf(',') != -1)
		{
			return text.CachedCommaExpansion()[0];
		}
		return text;
	}

	public bool ValidSlotType(string Type)
	{
		string text = ParentObject.UsesSlots;
		if (string.IsNullOrEmpty(text))
		{
			text = SlotType;
		}
		if (text.IndexOf(',') != -1)
		{
			List<string> list = text.CachedCommaExpansion();
			if (!list.Contains(Type))
			{
				return list.Contains("*");
			}
			return true;
		}
		if (!(text == Type))
		{
			return text == "*";
		}
		return true;
	}

	public static void SetupProjectile(GameObject Projectile, GameObject Attacker, GameObject Launcher = null, Projectile pProjectile = null)
	{
		Projectile.SetIntProperty("Primed", 1);
		if (pProjectile != null)
		{
			pProjectile.Launcher = Launcher;
		}
		if (Attacker.HasEffect("Phased") && !Projectile.HasTagOrProperty("IndependentPhaseProjectile") && Projectile.FireEvent("CanApplyPhased") && Projectile.ForceApplyEffect(new Phased(9999)))
		{
			Projectile.ModIntProperty("ProjectilePhaseAdded", 1);
		}
		if (Attacker.HasEffect("Omniphase") && !Projectile.HasTagOrProperty("IndependentOmniphaseProjectile") && Projectile.FireEvent("CanApplyOmniphase") && Projectile.ForceApplyEffect(new Omniphase(9999)))
		{
			Projectile.ModIntProperty("ProjectileOmniphaseAdded", 1);
		}
		if (Launcher != null && Launcher.HasRegisteredEvent("ProjectileSetup"))
		{
			Launcher.FireEvent(Event.New("ProjectileSetup", "Attacker", Attacker, "Launcher", Launcher, "Projectile", Projectile));
		}
	}

	public static void CleanupProjectile(GameObject Projectile)
	{
		if (!GameObject.validate(ref Projectile))
		{
			return;
		}
		if (Projectile.pPhysics.IsReal)
		{
			if (Projectile.GetIntProperty("ProjectilePhaseAdded") > 0)
			{
				Projectile.RemoveEffect("Phased");
				Projectile.ModIntProperty("ProjectilePhaseAdded", -1, RemoveIfZero: true);
			}
			if (Projectile.GetIntProperty("ProjectileOmniphaseAdded") > 0)
			{
				Projectile.RemoveEffect("Omniphase");
				Projectile.ModIntProperty("ProjectileOmniphaseAdded", -1, RemoveIfZero: true);
			}
		}
		else
		{
			Projectile.Obliterate();
		}
	}

	private void MissileHit(GameObject Attacker, GameObject Defender, GameObject Owner, GameObject Projectile, Projectile pProjectile, GameObject AimedAt, GameObject ApparentTarget, MissilePath MPath, FireType FireType, int AimLevel, int NaturalHitResult, bool PathInvolvesPlayer, GameObject MessageAsFrom, ref bool Done, ref bool PenetrateWalls)
	{
		bool flag = false;
		if (!Defender.FireEvent("DefenderMissileHit"))
		{
			return;
		}
		bool flag2 = Defender != ApparentTarget;
		string text = null;
		if (MessageAsFrom != null && MessageAsFrom != Owner)
		{
			text = (MessageAsFrom.IsPlayer() ? "You" : (MessageAsFrom.HasProperName ? ColorUtility.CapitalizeExceptFormatting(MessageAsFrom.ShortDisplayName) : ((MessageAsFrom.Equipped == Owner) ? Owner.Poss(MessageAsFrom) : ((MessageAsFrom.Equipped == null) ? ColorUtility.CapitalizeExceptFormatting(MessageAsFrom.The + MessageAsFrom.ShortDisplayName) : ColorUtility.CapitalizeExceptFormatting(Grammar.MakePossessive(MessageAsFrom.Equipped.The + MessageAsFrom.Equipped.DisplayNameOnly) + " " + MessageAsFrom.ShortDisplayName)))));
		}
		if (Defender == AimedAt && (FireType == FireType.BeaconFire || (FireType == FireType.OneShot && Owner.HasSkill("Rifle_BeaconFire") && Rifle_BeaconFire.MeetsCriteria(Defender))))
		{
			if (Owner.IsPlayer())
			{
				if (text != null)
				{
					IComponent<GameObject>.AddPlayerMessage(text + MessageAsFrom.GetVerb("hit") + " " + ((Defender == MessageAsFrom) ? MessageAsFrom.itself : (Defender.the + Defender.ShortDisplayName)) + " in a vital area.");
				}
				else
				{
					IComponent<GameObject>.AddPlayerMessage("You hit " + ((Defender == Owner) ? Owner.itself : (Defender.the + Defender.ShortDisplayName)) + " in a vital area.");
				}
			}
			Defender.BloodsplatterCone(bSelfsplatter: true, MPath.Angle, 45);
			flag = true;
		}
		if (Owner.HasEffect("Sniping"))
		{
			flag = true;
			Owner.RemoveEffect("Sniping");
		}
		if (!flag)
		{
			int num = GetCriticalThresholdEvent.GetFor(Attacker, Defender, ParentObject, Projectile, Skill);
			int @for = GetSpecialEffectChanceEvent.GetFor(Attacker, ParentObject, "Missile Critical", 5, Defender, Projectile);
			if (@for != 5)
			{
				num -= (@for - 5) / 5;
			}
			if (NaturalHitResult >= num)
			{
				flag = true;
			}
		}
		int num2 = pProjectile.BasePenetration;
		int num3 = pProjectile.BasePenetration + pProjectile.StrengthPenetration;
		if (flag)
		{
			BaseSkill genericSkill = Skills.GetGenericSkill(Skill, Attacker);
			if (genericSkill != null)
			{
				int weaponCriticalModifier = genericSkill.GetWeaponCriticalModifier(Attacker, Defender, ParentObject);
				if (weaponCriticalModifier != 0)
				{
					num2 += weaponCriticalModifier;
					num3 += weaponCriticalModifier;
				}
			}
		}
		if (!string.IsNullOrEmpty(ProjectilePenetrationStat) && Attacker != null)
		{
			num2 += Attacker.StatMod(ProjectilePenetrationStat);
		}
		Event @event = Event.New("WeaponMissileWeaponHit");
		@event.SetParameter("Attacker", Attacker);
		@event.SetParameter("Defender", Defender);
		@event.SetParameter("Weapon", ParentObject);
		@event.SetParameter("Penetrations", num2);
		@event.SetParameter("PenetrationCap", num3);
		@event.SetParameter("MessageAsFrom", MessageAsFrom);
		@event.SetFlag("Critical", flag);
		ParentObject.FireEvent(@event);
		num2 = @event.GetIntParameter("Penetrations");
		num3 = @event.GetIntParameter("PenetrationCap");
		flag = @event.HasFlag("Critical");
		@event.ID = "AttackerMissileWeaponHit";
		Attacker?.FireEvent(@event);
		@event.ID = "DefenderMissileWeaponHit";
		Defender?.FireEvent(@event);
		if (flag)
		{
			@event.ID = "MissileAttackerCriticalHit";
			Attacker.FireEvent(@event);
		}
		bool defenderIsCreature = Defender.HasTag("Creature");
		string blueprint = Defender.Blueprint;
		WeaponUsageTracking.TrackMissileWeaponHit(Owner, ParentObject, Projectile, defenderIsCreature, blueprint, flag2);
		GetMissileWeaponPerformanceEvent for2 = GetMissileWeaponPerformanceEvent.GetFor(Owner, ParentObject, Projectile, num2, num3, pProjectile.BaseDamage, null, null, pProjectile.PenetrateCreatures, pProjectile.PenetrateWalls, pProjectile.Quiet, null, null, Active: true);
		PenetrateWalls = for2.PenetrateWalls;
		Damage damage = new Damage(0);
		damage.AddAttributes(for2.Attributes);
		int num4 = 0;
		if (for2.Attributes.Contains("Mental"))
		{
			if (Defender.pBrain == null && (Defender.IsCreature ? for2.PenetrateCreatures : PenetrateWalls))
			{
				return;
			}
			num4 = Stats.GetCombatMA(Defender);
		}
		else
		{
			num4 = Stats.GetCombatAV(Defender);
		}
		int num5 = 0;
		num5 = (for2.Attributes.Contains("NonPenetrating") ? 1 : ((!for2.Attributes.Contains("Vorpal")) ? Stat.RollDamagePenetrations(num4, for2.BasePenetration, for2.PenetrationCap) : Stat.RollDamagePenetrations(0, 0, 0)));
		if (Skill == "Pistol" && Owner.HasSkill("Pistol_DisarmingShot") && Owner.StatMod("Agility").in100())
		{
			Disarming.Disarm(Defender, Attacker, 100, "Strength", "Agility", ParentObject);
		}
		if (num5 == 0)
		{
			Defender.ParticleBlip("&K\a");
			if (Owner.IsPlayer())
			{
				if (text != null)
				{
					IComponent<GameObject>.AddPlayerMessage(text + MessageAsFrom.GetVerb("fail") + " to penetrate " + Grammar.MakePossessive(Defender.the + Defender.ShortDisplayName) + " armor with " + MessageAsFrom.its_(Projectile) + "!", 'r');
				}
				else
				{
					IComponent<GameObject>.AddPlayerMessage(Owner.Poss(Projectile) + Projectile.GetVerb("fail") + " to penetrate " + Grammar.MakePossessive(Defender.the + Defender.ShortDisplayName) + " armor!", 'r');
				}
			}
			else if (Defender.IsPlayer())
			{
				if (text != null)
				{
					IComponent<GameObject>.AddPlayerMessage(text + MessageAsFrom.GetVerb("fail") + " to penetrate your armor with " + MessageAsFrom.its_(Projectile) + "!", 'g');
				}
				else
				{
					IComponent<GameObject>.AddPlayerMessage(Owner.Poss(Projectile) + Projectile.GetVerb("fail") + " to penetrate your armor!");
				}
			}
			Done = true;
			Event event2 = Event.New("ProjectileHit");
			event2.SetParameter("Attacker", Attacker);
			event2.SetParameter("Defender", Defender);
			event2.SetParameter("Skill", Skill);
			event2.SetParameter("Damage", damage);
			event2.SetParameter("AimLevel", AimLevel);
			event2.SetParameter("Owner", Attacker);
			event2.SetParameter("Launcher", ParentObject);
			event2.SetParameter("Projectile", Projectile);
			event2.SetParameter("Path", MPath);
			event2.SetParameter("Penetrations", 0);
			event2.SetParameter("ApparentTarget", ApparentTarget);
			event2.SetParameter("AimedAt", AimedAt);
			event2.SetFlag("Critical", flag);
			Projectile.FireEvent(event2);
			event2.ID = "DefenderProjectileHit";
			Defender.FireEvent(event2);
			event2.ID = "LauncherProjectileHit";
			ParentObject.FireEvent(event2);
			return;
		}
		if (Defender == AimedAt && Defender.IsCombatObject())
		{
			if (FireType == FireType.SuppressingFire || FireType == FireType.FlatteningFire || (FireType == FireType.OneShot && Attacker.HasSkill("Rifle_SuppressiveFire")))
			{
				if (Defender.ApplyEffect(new Suppressed(Stat.Random(3, 5))))
				{
					if (text != null)
					{
						IComponent<GameObject>.EmitMessage(MessageAsFrom, Grammar.MakePossessive(text) + " suppressive fire locks " + Defender.the + Defender.ShortDisplayName + " in place.");
					}
					else
					{
						IComponent<GameObject>.EmitMessage(Attacker, Attacker.Poss("suppressive fire locks ") + Defender.the + Defender.ShortDisplayName + " in place.");
					}
				}
				if (Attacker.HasSkill("Rifle_FlatteningFire") && Rifle_FlatteningFire.MeetsCriteria(Defender))
				{
					if (Defender.ApplyEffect(new Prone()))
					{
						if (text != null)
						{
							IComponent<GameObject>.EmitMessage(MessageAsFrom, Grammar.MakePossessive(text) + " flattening fire drops " + Defender.the + Defender.ShortDisplayName + " to the ground!");
						}
						else
						{
							IComponent<GameObject>.EmitMessage(Attacker, Attacker.Poss("flattening fire drops ") + Defender.the + Defender.ShortDisplayName + " to the ground!");
						}
					}
					Disarming.Disarm(Defender, Attacker, 100, "Strength", "Agility", ParentObject);
				}
			}
			if (FireType == FireType.WoundingFire || FireType == FireType.DisorientingFire || (FireType == FireType.OneShot && Attacker.HasSkill("Rifle_WoundingFire")))
			{
				string text2 = (Attacker.IsPlayer() ? "You" : (Attacker.The + Attacker.ShortDisplayName));
				if (Defender.ApplyEffect(new Bleeding(num5.ToString(), 20 + for2.BaseDamage.RollMaxCached(), Attacker, Stack: false)))
				{
					if (text != null)
					{
						IComponent<GameObject>.EmitMessage(MessageAsFrom, text + MessageAsFrom.GetVerb("wound") + " " + Defender.the + Defender.DisplayNameOnly + ".");
					}
					else
					{
						IComponent<GameObject>.EmitMessage(Attacker, text2 + Attacker.GetVerb("wound") + " " + Defender.the + Defender.DisplayNameOnly + ".");
					}
					Defender.BloodsplatterCone(bSelfsplatter: true, MPath.Angle, 45);
				}
				if (Attacker.HasSkill("Rifle_DisorientingFire") && Rifle_DisorientingFire.MeetsCriteria(Defender) && Defender.ApplyEffect(new Disoriented(Stat.Random(5, 7), 4)))
				{
					if (text != null)
					{
						IComponent<GameObject>.EmitMessage(MessageAsFrom, text + MessageAsFrom.GetVerb("disorient") + " " + Defender.the + Defender.DisplayNameOnly + ".");
					}
					else
					{
						IComponent<GameObject>.EmitMessage(Attacker, text2 + Attacker.GetVerb("disorient") + " " + Defender.the + Defender.DisplayNameOnly + ".");
					}
				}
			}
		}
		if (Options.ShowMonsterHPHearts)
		{
			Defender.ParticleBlip(Stat.GetResultColor(num5) + "\u0003");
		}
		bool flag3 = for2.BaseDamage != "0";
		string text3 = null;
		if (for2.Attributes.Contains("Mental") && Defender.pBrain == null)
		{
			flag3 = false;
			if (Attacker.IsPlayer())
			{
				text3 = ", but your mental attack has no effect";
			}
		}
		if (flag3)
		{
			DieRoll possiblyCachedDamageRoll = for2.GetPossiblyCachedDamageRoll();
			int num6 = 0;
			for (int i = 0; i < num5; i++)
			{
				num6 += possiblyCachedDamageRoll.Resolve();
			}
			damage.Amount = num6;
			if (flag2)
			{
				damage.AddAttribute("Accidental");
			}
			int phase = Projectile.GetPhase();
			if (damage.Amount > 0 && flag)
			{
				Defender.ParticleText("*critical hit*", IComponent<GameObject>.ConsequentialColorChar(null, Defender));
			}
			if (damage.Amount > 0)
			{
				Event event3 = Event.New("DealingMissileDamage");
				event3.SetParameter("Attacker", Attacker);
				event3.SetParameter("Defender", Defender);
				event3.SetParameter("Skill", Skill);
				event3.SetParameter("Damage", damage);
				event3.SetParameter("AimLevel", AimLevel);
				event3.SetParameter("Phase", phase);
				event3.SetFlag("Critical", flag);
				if (!Attacker.FireEvent(event3))
				{
					damage.Amount = 0;
				}
			}
			if (damage.Amount > 0)
			{
				Event event4 = Event.New("WeaponDealingMissileDamage");
				event4.SetParameter("Attacker", Attacker);
				event4.SetParameter("Defender", Defender);
				event4.SetParameter("Skill", Skill);
				event4.SetParameter("Damage", damage);
				event4.SetParameter("AimLevel", AimLevel);
				event4.SetParameter("Phase", AimLevel);
				event4.SetFlag("Critical", flag);
				if (!ParentObject.FireEvent(event4))
				{
					damage.Amount = 0;
				}
			}
			bool flag4 = false;
			if (damage.Amount > 0)
			{
				Defender.WillCheckHP(true);
				flag4 = true;
				Event event5 = Event.New("TakeDamage");
				event5.SetParameter("Damage", damage);
				event5.SetParameter("Owner", Attacker);
				event5.SetParameter("Attacker", Attacker);
				event5.SetParameter("Weapon", ParentObject);
				event5.SetParameter("Projectile", Projectile);
				event5.SetParameter("Phase", phase);
				event5.SetFlag("WillUseOutcomeMessageFragment", State: true);
				if (!Defender.FireEvent(event5))
				{
					damage.Amount = 0;
				}
				text3 = event5.GetStringParameter("OutcomeMessageFragment");
			}
			WeaponUsageTracking.TrackMissileWeaponDamage(Owner, ParentObject, Projectile, defenderIsCreature, blueprint, flag2, damage);
			if (damage.Amount > 0 && !base.juiceEnabled && Options.ShowMonsterHPHearts)
			{
				Defender.ParticleBlip(Defender.GetHPColor() + "\u0003");
			}
			if (Owner.IsPlayer())
			{
				if (text3 != null)
				{
					if (Defender.IsVisible())
					{
						if (text != null)
						{
							if (MessageAsFrom.IsVisible())
							{
								IComponent<GameObject>.AddPlayerMessage(text + (flag ? " critically" : "") + MessageAsFrom.GetVerb("hit") + " " + ((Defender == Owner) ? Owner.itself : (Defender.the + Defender.ShortDisplayName)) + " with " + Projectile.a + Projectile.ShortDisplayName + text3 + ".");
							}
							else
							{
								IComponent<GameObject>.AddPlayerMessage("Something hits " + ((Defender == Owner) ? Owner.itself : (Defender.the + Defender.ShortDisplayName)) + " with " + Projectile.a + Projectile.ShortDisplayName + text3 + ".");
							}
						}
						else
						{
							IComponent<GameObject>.AddPlayerMessage("You" + (flag ? " critically" : "") + " hit " + ((Defender == Owner) ? Owner.itself : (Defender.the + Defender.ShortDisplayName)) + " with " + Projectile.a + Projectile.ShortDisplayName + text3 + ".");
						}
					}
				}
				else if (damage.Amount > 0 || !damage.SuppressionMessageDone)
				{
					if (text != null)
					{
						if (Defender.IsVisible())
						{
							if (MessageAsFrom.IsVisible())
							{
								IComponent<GameObject>.AddPlayerMessage(text + (flag ? " critically" : "") + MessageAsFrom.GetVerb("hit") + " " + ((Defender == Owner) ? Owner.itself : (Defender.the + Defender.ShortDisplayName)) + " (x" + num5 + ") with " + Projectile.a + Projectile.ShortDisplayName + " for " + damage.Amount + " damage!", Stat.GetResultColor(num5));
							}
							else
							{
								IComponent<GameObject>.AddPlayerMessage("Something hits " + ((Defender == Owner) ? Owner.itself : (Defender.the + Defender.ShortDisplayName)) + " (x" + num5 + ") with " + Projectile.a + Projectile.ShortDisplayName + " for " + damage.Amount + " damage!", Stat.GetResultColor(num5));
							}
						}
						else if (Defender.IsAudible(IComponent<GameObject>.ThePlayer, 80))
						{
							IComponent<GameObject>.AddPlayerMessage(text + MessageAsFrom.GetVerb("hit") + " something " + Owner.DescribeDirectionToward(Defender) + "!");
						}
					}
					else if (Defender.IsVisible())
					{
						IComponent<GameObject>.AddPlayerMessage("You" + (flag ? " critically" : "") + " hit " + ((Defender == Owner) ? Owner.itself : (Defender.the + Defender.ShortDisplayName)) + " (x" + num5 + ") with " + Projectile.a + Projectile.ShortDisplayName + " for " + damage.Amount + " damage!", Stat.GetResultColor(num5));
					}
					else if (Defender.IsAudible(IComponent<GameObject>.ThePlayer, 80))
					{
						IComponent<GameObject>.AddPlayerMessage("You hit something " + Owner.DescribeDirectionToward(Defender) + "!");
					}
				}
			}
			else if (Defender.IsPlayer())
			{
				if (text3 != null)
				{
					if (text != null)
					{
						if (MessageAsFrom.IsVisible())
						{
							IComponent<GameObject>.AddPlayerMessage(text + (flag ? " critically" : "") + MessageAsFrom.GetVerb("hit") + " you with " + Projectile.a + Projectile.ShortDisplayName + text3 + ".");
						}
						else
						{
							IComponent<GameObject>.AddPlayerMessage(Projectile.A + Projectile.ShortDisplayName + (flag ? " critically" : "") + Projectile.GetVerb("hit") + " you" + text3 + ".");
						}
					}
					else if (Owner.IsVisible())
					{
						IComponent<GameObject>.AddPlayerMessage(Owner.The + Owner.ShortDisplayName + (flag ? " critically" : "") + Owner.GetVerb("hit") + " you with " + Projectile.a + Projectile.ShortDisplayName + text3 + ".");
					}
					else
					{
						IComponent<GameObject>.AddPlayerMessage(Projectile.A + Projectile.ShortDisplayName + (flag ? " critically" : "") + Projectile.GetVerb("hit") + " you " + Defender.DescribeDirectionFrom(Owner) + text3 + ".");
					}
				}
				else if (text != null)
				{
					if (MessageAsFrom.IsVisible())
					{
						IComponent<GameObject>.AddPlayerMessage(text + (flag ? " critically" : "") + MessageAsFrom.GetVerb("hit") + " you (x" + num5 + ") with " + Projectile.a + Projectile.ShortDisplayName + " for " + damage.Amount + " damage!", 'r');
					}
					else
					{
						IComponent<GameObject>.AddPlayerMessage(Projectile.A + Projectile.ShortDisplayName + (flag ? " critically" : "") + Projectile.GetVerb("hit") + " you (x" + num5 + ") " + Defender.DescribeDirectionFrom(Owner) + " for " + damage.Amount + " damage!", 'r');
					}
				}
				else if (Owner.IsVisible())
				{
					IComponent<GameObject>.AddPlayerMessage(Owner.The + Owner.ShortDisplayName + (flag ? " critically" : "") + Owner.GetVerb("hit") + " you (x" + num5 + ") with " + Projectile.a + Projectile.ShortDisplayName + " for " + damage.Amount + " damage!", 'r');
				}
				else
				{
					IComponent<GameObject>.AddPlayerMessage(Projectile.A + Projectile.ShortDisplayName + (flag ? " critically" : "") + Projectile.GetVerb("hit") + " you (x" + num5 + ") " + Defender.DescribeDirectionFrom(Owner) + " for " + damage.Amount + " damage!", 'r');
				}
			}
			else if (PathInvolvesPlayer && Defender.IsVisible())
			{
				if (text3 != null)
				{
					if (text != null)
					{
						if (MessageAsFrom.IsVisible())
						{
							IComponent<GameObject>.AddPlayerMessage(text + (flag ? " critically" : "") + MessageAsFrom.GetVerb("hit") + " " + ((Defender == Owner) ? Owner.itself : (Defender.the + Defender.ShortDisplayName)) + " with " + Projectile.a + Projectile.ShortDisplayName + text3 + ".");
						}
						else
						{
							IComponent<GameObject>.AddPlayerMessage(Projectile.A + Projectile.ShortDisplayName + (flag ? " critically" : "") + Projectile.GetVerb("hit") + " " + ((Defender == Owner) ? Owner.itself : (Defender.the + Defender.ShortDisplayName)) + " " + IComponent<GameObject>.ThePlayer.DescribeDirectionFrom(Owner) + text3 + ".");
						}
					}
					else if (Owner.IsVisible())
					{
						IComponent<GameObject>.AddPlayerMessage(Owner.The + Owner.ShortDisplayName + (flag ? " critically" : "") + Owner.GetVerb("hit") + " " + ((Defender == Owner) ? Owner.itself : (Defender.the + Defender.ShortDisplayName)) + " with " + Projectile.a + Projectile.ShortDisplayName + text3 + ".");
					}
					else
					{
						IComponent<GameObject>.AddPlayerMessage(Projectile.A + Projectile.ShortDisplayName + (flag ? " critically" : "") + Projectile.GetVerb("hit") + " " + ((Defender == Owner) ? Owner.itself : (Defender.the + Defender.ShortDisplayName)) + " " + IComponent<GameObject>.ThePlayer.DescribeDirectionFrom(Owner) + text3 + ".");
					}
				}
				else if (text != null)
				{
					if (MessageAsFrom.IsVisible())
					{
						IComponent<GameObject>.AddPlayerMessage(text + (flag ? " critically" : "") + MessageAsFrom.GetVerb("hit") + " " + ((Defender == Owner) ? Owner.itself : (Defender.the + Defender.ShortDisplayName)) + " (x" + num5 + ") with " + Projectile.a + Projectile.ShortDisplayName + " for " + damage.Amount + " damage!");
					}
					else
					{
						IComponent<GameObject>.AddPlayerMessage(Projectile.A + Projectile.ShortDisplayName + (flag ? " critically" : "") + Projectile.GetVerb("hit") + " " + ((Defender == Owner) ? Owner.itself : (Defender.the + Defender.ShortDisplayName)) + " (x" + num5 + ") " + IComponent<GameObject>.ThePlayer.DescribeDirectionFrom(Owner) + " for " + damage.Amount + " damage!");
					}
				}
				else if (Owner.IsVisible())
				{
					IComponent<GameObject>.AddPlayerMessage(Owner.The + Owner.ShortDisplayName + (flag ? " critically" : "") + Owner.GetVerb("hit") + " " + ((Defender == Owner) ? Owner.itself : (Defender.the + Defender.ShortDisplayName)) + " (x" + num5 + ") with " + Projectile.a + Projectile.ShortDisplayName + " for " + damage.Amount + " damage!");
				}
				else
				{
					IComponent<GameObject>.AddPlayerMessage(Projectile.A + Projectile.ShortDisplayName + (flag ? " critically" : "") + Projectile.GetVerb("hit") + " " + ((Defender == Owner) ? Owner.itself : (Defender.the + Defender.ShortDisplayName)) + " (x" + num5 + ") " + IComponent<GameObject>.ThePlayer.DescribeDirectionFrom(Owner) + " for " + damage.Amount + " damage!");
				}
			}
			if (flag4)
			{
				Defender.CheckHP(null, null, null, Preregistered: true);
			}
		}
		else if (!for2.Quiet)
		{
			if (Owner.IsPlayer())
			{
				if (text3 != null)
				{
					if (Defender.IsVisible())
					{
						if (text != null)
						{
							if (MessageAsFrom.IsVisible())
							{
								IComponent<GameObject>.AddPlayerMessage(text + (flag ? " critically" : "") + MessageAsFrom.GetVerb("hit") + " " + ((Defender == Owner) ? Owner.itself : (Defender.the + Defender.ShortDisplayName)) + " with " + Projectile.a + Projectile.ShortDisplayName + text3 + ".");
							}
							else
							{
								IComponent<GameObject>.AddPlayerMessage(Projectile.A + Projectile.ShortDisplayName + (flag ? " critically" : "") + Projectile.GetVerb("hit") + " " + ((Defender == Owner) ? Owner.itself : (Defender.the + Defender.ShortDisplayName)) + " " + Defender.DescribeDirectionFrom(Owner) + text3 + ".");
							}
						}
						else
						{
							IComponent<GameObject>.AddPlayerMessage("You" + (flag ? " critically" : "") + " hit " + ((Defender == Owner) ? Owner.itself : (Defender.the + Defender.ShortDisplayName)) + " with " + Projectile.a + Projectile.ShortDisplayName + text3 + ".");
						}
					}
				}
				else if (text != null)
				{
					if (Defender.IsVisible())
					{
						if (MessageAsFrom.IsVisible())
						{
							IComponent<GameObject>.AddPlayerMessage(text + (flag ? " critically" : "") + MessageAsFrom.GetVerb("hit") + " " + ((Defender == Owner) ? Owner.itself : (Defender.the + Defender.ShortDisplayName)) + " (x" + num5 + ") with " + Projectile.a + Projectile.ShortDisplayName + "!", Stat.GetResultColor(num5));
						}
						else
						{
							IComponent<GameObject>.AddPlayerMessage(Projectile.A + Projectile.ShortDisplayName + (flag ? " critically" : "") + Projectile.GetVerb("hit") + " " + ((Defender == Owner) ? Owner.itself : (Defender.the + Defender.ShortDisplayName)) + " (x" + num5 + ") " + Defender.DescribeDirectionFrom(Owner) + "!", Stat.GetResultColor(num5));
						}
					}
					else if (MessageAsFrom.IsVisible() && Defender.IsAudible(IComponent<GameObject>.ThePlayer, 80))
					{
						IComponent<GameObject>.AddPlayerMessage(text + MessageAsFrom.GetVerb("hit") + " something " + Owner.DescribeDirectionToward(Defender) + "!");
					}
				}
				else if (Defender.IsVisible())
				{
					IComponent<GameObject>.AddPlayerMessage("You" + (flag ? " critically" : "") + " hit " + ((Defender == Owner) ? Owner.itself : (Defender.the + Defender.ShortDisplayName)) + " (x" + num5 + ") with " + Projectile.a + Projectile.ShortDisplayName + "!", Stat.GetResultColor(num5));
				}
				else if (Defender.IsAudible(IComponent<GameObject>.ThePlayer, 80))
				{
					IComponent<GameObject>.AddPlayerMessage("You hit something " + Owner.DescribeDirectionToward(Defender) + "!");
				}
			}
			else if (Defender.IsPlayer())
			{
				if (text3 != null)
				{
					if (text != null)
					{
						if (MessageAsFrom.IsVisible())
						{
							IComponent<GameObject>.AddPlayerMessage(text + (flag ? " critically" : "") + MessageAsFrom.GetVerb("hit") + " you with " + Projectile.a + Projectile.ShortDisplayName + text3 + ".");
						}
						else
						{
							IComponent<GameObject>.AddPlayerMessage(Projectile.A + Projectile.ShortDisplayName + (flag ? " critically" : "") + Projectile.GetVerb("hit") + " you " + Defender.DescribeDirectionToward(Owner) + text3 + ".");
						}
					}
					else if (Owner.IsVisible())
					{
						IComponent<GameObject>.AddPlayerMessage(Owner.The + Owner.ShortDisplayName + (flag ? " critically" : "") + Owner.GetVerb("hit") + " you" + text3 + ".");
					}
					else
					{
						IComponent<GameObject>.AddPlayerMessage(Projectile.A + Projectile.ShortDisplayName + (flag ? " critically" : "") + Projectile.GetVerb("hit") + " you " + Defender.DescribeDirectionToward(Owner) + text3 + ".");
					}
				}
				else if (text != null)
				{
					if (MessageAsFrom.IsVisible())
					{
						IComponent<GameObject>.AddPlayerMessage(text + (flag ? " critically" : "") + MessageAsFrom.GetVerb("hit") + " you with " + Projectile.a + Projectile.ShortDisplayName + "! (x" + num5 + ")", 'r');
					}
					else
					{
						IComponent<GameObject>.AddPlayerMessage(Projectile.A + Projectile.ShortDisplayName + (flag ? " critically" : "") + Projectile.GetVerb("hit") + " you " + Defender.DescribeDirectionToward(Owner) + "! (x" + num5 + ")", 'r');
					}
				}
				else if (Owner.IsVisible())
				{
					IComponent<GameObject>.AddPlayerMessage(Owner.The + Owner.ShortDisplayName + (flag ? " critically" : "") + Owner.GetVerb("hit") + " you! (x" + num5 + ")", 'r');
				}
				else
				{
					IComponent<GameObject>.AddPlayerMessage(Projectile.A + Projectile.ShortDisplayName + (flag ? " critically" : "") + Projectile.GetVerb("hit") + " you " + Defender.DescribeDirectionToward(Owner) + "! (x" + num5 + ")", 'r');
				}
			}
			else if (PathInvolvesPlayer && Defender.IsVisible())
			{
				if (text3 != null)
				{
					if (text != null)
					{
						if (MessageAsFrom.IsVisible())
						{
							IComponent<GameObject>.AddPlayerMessage(text + (flag ? " critically" : "") + MessageAsFrom.GetVerb("hit") + " " + ((Defender == Owner) ? Owner.itself : (Defender.the + Defender.ShortDisplayName)) + text3 + ".");
						}
						else
						{
							IComponent<GameObject>.AddPlayerMessage(Projectile.A + Projectile.ShortDisplayName + (flag ? " critically" : "") + Projectile.GetVerb("hit") + " " + ((Defender == Owner) ? Owner.itself : (Defender.the + Defender.ShortDisplayName)) + " " + IComponent<GameObject>.ThePlayer.DescribeDirectionToward(Owner) + text3 + ".");
						}
					}
					else if (Owner.IsVisible())
					{
						IComponent<GameObject>.AddPlayerMessage(Owner.The + Owner.ShortDisplayName + (flag ? " critically" : "") + Owner.GetVerb("hit") + " " + ((Defender == Owner) ? Owner.itself : (Defender.the + Defender.ShortDisplayName)) + text3 + ".");
					}
					else
					{
						IComponent<GameObject>.AddPlayerMessage(Projectile.A + Projectile.ShortDisplayName + (flag ? " critically" : "") + Projectile.GetVerb("hit") + " " + ((Defender == Owner) ? Owner.itself : (Defender.the + Defender.ShortDisplayName)) + " " + IComponent<GameObject>.ThePlayer.DescribeDirectionToward(Owner) + text3 + ".");
					}
				}
				else if (text != null)
				{
					if (MessageAsFrom.IsVisible())
					{
						IComponent<GameObject>.AddPlayerMessage(text + (flag ? " critically" : "") + MessageAsFrom.GetVerb("hit") + " " + ((Defender == Owner) ? Owner.itself : (Defender.the + Defender.ShortDisplayName)) + "! (x" + num5 + ")");
					}
					else
					{
						IComponent<GameObject>.AddPlayerMessage(Projectile.A + Projectile.ShortDisplayName + (flag ? " critically" : "") + Projectile.GetVerb("hit") + " " + ((Defender == Owner) ? Owner.itself : (Defender.the + Defender.ShortDisplayName)) + " (x" + num5 + ") " + IComponent<GameObject>.ThePlayer.DescribeDirectionToward(Owner) + "!");
					}
				}
				else if (Owner.IsVisible())
				{
					IComponent<GameObject>.AddPlayerMessage(Owner.The + Owner.ShortDisplayName + (flag ? " critically" : "") + Owner.GetVerb("hit") + " " + ((Defender == Owner) ? Owner.itself : (Defender.the + Defender.ShortDisplayName)) + "! (x" + num5 + ")");
				}
				else
				{
					IComponent<GameObject>.AddPlayerMessage(Projectile.A + Projectile.ShortDisplayName + (flag ? " critically" : "") + Projectile.GetVerb("hit") + " " + ((Defender == Owner) ? Owner.itself : (Defender.the + Defender.ShortDisplayName)) + " (x" + num5 + ") " + IComponent<GameObject>.ThePlayer.DescribeDirectionToward(Owner) + "!");
				}
			}
		}
		if (Owner.IsPlayer() && Sidebar.CurrentTarget == null && Defender.IsHostileTowards(Owner) && Defender.IsVisible())
		{
			Sidebar.CurrentTarget = Defender;
		}
		Event event6 = Event.New("ProjectileHit");
		event6.SetParameter("Attacker", Attacker);
		event6.SetParameter("Defender", Defender);
		event6.SetParameter("Skill", Skill);
		event6.SetParameter("Damage", damage);
		event6.SetParameter("AimLevel", AimLevel);
		event6.SetParameter("Owner", Attacker);
		event6.SetParameter("Launcher", ParentObject);
		event6.SetParameter("Path", MPath);
		event6.SetParameter("Penetrations", num5);
		event6.SetParameter("ApparentTarget", ApparentTarget);
		event6.SetParameter("AimedAt", AimedAt);
		event6.SetFlag("Critical", flag);
		Projectile.FireEvent(event6);
		event6.ID = "DefenderProjectileHit";
		Defender.FireEvent(event6);
		event6.ID = "LauncherProjectileHit";
		ParentObject.FireEvent(event6);
		if (!for2.PenetrateCreatures)
		{
			Done = true;
		}
	}

	public bool IsSkilled(GameObject who)
	{
		if (who != null)
		{
			if (Skill == "Pistol")
			{
				return who.HasSkill("Pistol_SteadyHands");
			}
			if (Skill == "Rifle" || Skill == "Bow")
			{
				return who.HasSkill("Rifle_SteadyHands");
			}
		}
		return false;
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "CommandFireMissile")
		{
			ScreenBuffer scrapBuffer = ScreenBuffer.GetScrapBuffer1(bLoadFromCurrent: true);
			if (E.GetParameter("ScreenBuffer") is ScreenBuffer source)
			{
				scrapBuffer.Copy(source);
			}
			else
			{
				XRLCore.Core.RenderBaseToBuffer(scrapBuffer);
			}
			Cell cell = E.GetParameter("TargetCell") as Cell;
			GameObject gameObject = E.GetGameObjectParameter("Actor") ?? E.GetGameObjectParameter("Owner");
			GameObject gameObject2 = gameObject;
			FireType fireType = FireType.Normal;
			if (E.HasParameter("FireType"))
			{
				fireType = (FireType)E.GetParameter("FireType");
			}
			if (gameObject == null)
			{
				return true;
			}
			Cell cell2 = gameObject.CurrentCell;
			if (cell2 == null || cell2.IsGraveyard())
			{
				return true;
			}
			Zone parentZone = cell2.ParentZone;
			if (parentZone == null)
			{
				return true;
			}
			GameObject gameObject3 = null;
			int intParameter = E.GetIntParameter("AimLevel");
			MissilePath missilePath = E.GetParameter("Path") as MissilePath;
			if (missilePath == null)
			{
				cell.ParentZone.CalculateMissileMap(gameObject);
				missilePath = CalculateMissilePath(cell2.ParentZone, cell2.X, cell2.Y, cell.X, cell.Y);
				if (missilePath == null)
				{
					return false;
				}
			}
			int intParameter2 = E.GetIntParameter("FlatVariance");
			if (!gameObject2.FireEvent("BeginMissileAttack"))
			{
				return false;
			}
			bool flag = false;
			if (!NoWildfire)
			{
				int num = 0;
				foreach (Cell adjacentCell in gameObject2.CurrentCell.GetAdjacentCells())
				{
					foreach (GameObject item2 in adjacentCell.LoopObjectsWithPart("Combat"))
					{
						if (!item2.IsHostileTowards(gameObject2) || !item2.PhaseAndFlightMatches(gameObject2) || !item2.CanMoveExtremities(null, ShowMessage: false, Involuntary: false, AllowTelekinetic: true))
						{
							continue;
						}
						num++;
						if (num > 1)
						{
							if (50.in100())
							{
								flag = true;
							}
							goto end_IL_01dd;
						}
					}
					continue;
					end_IL_01dd:
					break;
				}
			}
			float num2 = missilePath.x1 - missilePath.x0;
			float num3 = missilePath.y1 - missilePath.y0;
			float num4 = 0f;
			string text = "-";
			num4 = ((num2 != 0f) ? (Math.Abs(num3) / Math.Abs(num2)) : 9999f);
			text = ((num4 >= 2f) ? "|" : ((!((double)num4 >= 0.5)) ? "-" : ((num2 < 0f) ? ((!(num3 > 0f)) ? "\\" : "/") : ((!(num3 > 0f)) ? "/" : "\\"))));
			ScreenBuffer scrapBuffer2 = ScreenBuffer.GetScrapBuffer2();
			Event e = Event.New("CheckLoadAmmo", "Loader", gameObject);
			if (!ParentObject.FireEvent(e))
			{
				return false;
			}
			int num5 = 0;
			List<GameObject> list = new List<GameObject>(ShotsPerAction);
			List<Projectile> list2 = new List<Projectile>(ShotsPerAction);
			GameObject gameObject4 = null;
			Event @event = Event.New("LoadAmmo", "Loader", gameObject, "Ammo", null, "AmmoObject", null);
			for (int i = 0; i < AmmoPerAction; i++)
			{
				if (!ParentObject.FireEvent(@event))
				{
					break;
				}
				num5++;
				GameObject obj = @event.GetGameObjectParameter("Ammo");
				if (GameObject.validate(ref obj))
				{
					list.Add(obj);
					list2.Add(obj.GetPart("Projectile") as Projectile);
					if (gameObject4 == null)
					{
						gameObject4 = obj;
					}
				}
			}
			gameObject3 = @event.GetGameObjectParameter("AmmoObject");
			for (int j = AmmoPerAction; j < ShotsPerAction; j++)
			{
				int num6 = j - AmmoPerAction;
				if (list.Count < num6)
				{
					num6 = 0;
				}
				if (list.Count > num6)
				{
					GameObject obj2 = list[num6].DeepCopy();
					if (GameObject.validate(ref obj2))
					{
						list.Add(obj2);
						list2.Add(obj2.GetPart("Projectile") as Projectile);
					}
				}
			}
			for (int num7 = list.Count - 1; num7 >= 0; num7--)
			{
				SetupProjectile(list[num7], gameObject2, ParentObject, list2[num7]);
			}
			if (num5 == 0)
			{
				if (gameObject2 != null && gameObject2.pBrain != null && ParentObject.FireEvent("ReloadPossible"))
				{
					gameObject2.pBrain.NeedToReload = true;
				}
				return false;
			}
			gameObject?.pPhysics?.PlayWorldSound(ParentObject.GetTag("MissileFireSound"), 0.5f, 0f, combat: true);
			if (flag)
			{
				if (gameObject2.IsPlayer())
				{
					IComponent<GameObject>.AddPlayerMessage("Your shot goes wild!", 'R');
				}
				else if (IComponent<GameObject>.Visible(gameObject2))
				{
					IComponent<GameObject>.AddPlayerMessage(Grammar.MakePossessive(gameObject2.The + gameObject2.ShortDisplayName) + " shot goes wild!", ColorCoding.ConsequentialColor(null, gameObject2));
				}
			}
			int num8 = 0;
			num8 = ((num5 < AmmoPerAction) ? ((int)Math.Ceiling((float)ShotsPerAction * ((float)num5 / (float)AmmoPerAction))) : ShotsPerAction);
			if (num8 > 0)
			{
				ParentObject.FireEvent("WeaponMissleWeaponFiring");
			}
			GameObject gameObject5 = cell.GetCombatTarget(gameObject2, IgnoreFlight: false, IgnoreAttackable: false, IgnorePhase: false, 0, gameObject4 ?? gameObject2, null, AllowInanimate: true, InanimateSolidOnly: true);
			if (gameObject5 == gameObject)
			{
				gameObject5 = null;
			}
			if (gameObject5 != null && gameObject5.IsPlayer() && AutoAct.IsActive() && IComponent<GameObject>.Visible(gameObject2))
			{
				AutoAct.Interrupt("something is shooting at you", null, gameObject);
			}
			GameObject gameObject6 = gameObject5;
			if (gameObject.IsPlayer())
			{
				MissilePath path = missilePath;
				GameObject owner = gameObject2;
				foreach (Point item3 in CalculateBulletTrajectory(path, gameObject4, null, owner, null, null, 0, 0, IntendedPathOnly: true))
				{
					GameObject combatTarget = parentZone.GetCell(item3.X, item3.Y).GetCombatTarget(gameObject2, IgnoreFlight: false, IgnoreAttackable: false, IgnorePhase: false, 0, gameObject4 ?? gameObject2, null, AllowInanimate: true, InanimateSolidOnly: true);
					if (combatTarget != null && combatTarget != gameObject6 && combatTarget != gameObject && !combatTarget.IsLedBy(gameObject))
					{
						if (gameObject6 == null)
						{
							gameObject6 = combatTarget;
						}
						if (combatTarget.pBrain != null && combatTarget.pBrain.FriendlyFireIncident(gameObject))
						{
							combatTarget.pBrain.AdjustFeeling(gameObject, -2);
							break;
						}
					}
				}
			}
			List<List<Point>> list3 = new List<List<Point>>();
			int num9 = 0;
			int num10 = -gameObject.StatMod(Modifier);
			if (IsSkilled(gameObject2))
			{
				num10 -= 2;
			}
			if (gameObject.IsPlayer() && ((gameObject5 != null && (gameObject5.IsCreature || Sidebar.CurrentTarget == null)) || (gameObject6 != null && Sidebar.CurrentTarget == null && gameObject6.IsHostileTowards(gameObject) && gameObject6.IsVisible())))
			{
				Sidebar.CurrentTarget = gameObject6;
			}
			if (gameObject5 != null)
			{
				num10 += gameObject6.GetIntProperty("IncomingAimModifier");
				if (gameObject5.HasEffect("RifleMark") && ((RifleMark)gameObject5.GetEffect("RifleMark")).Marker == gameObject2)
				{
					num10--;
				}
			}
			num10 -= intParameter;
			num10 -= AimVarianceBonus;
			num10 -= gameObject2.GetIntProperty("MissileWeaponAccuracyBonus");
			num10 -= ParentObject.GetIntProperty("MissileWeaponAccuracyBonus");
			eModifyAimVariance.SetParameter("Amount", 0);
			gameObject2.FireEvent(eModifyAimVariance);
			ParentObject.FireEvent(eModifyAimVariance);
			num10 += eModifyAimVariance.GetIntParameter("Amount");
			if (gameObject5 != null && gameObject5.HasRegisteredEvent("ModifyIncomingAimVariance"))
			{
				eModifyIncomingAimVariance.SetParameter("Amount", 0);
				gameObject5.FireEvent(eModifyIncomingAimVariance);
				num10 += eModifyIncomingAimVariance.GetIntParameter("Amount");
			}
			int num11 = VarianceDieRoll.Resolve();
			num9 = Math.Abs(num11 - 21) + num10;
			if (fireType == FireType.SureFire || fireType == FireType.BeaconFire || (fireType == FireType.OneShot && gameObject2.HasSkill("Rifle_SureFire")))
			{
				num9 = 0;
			}
			if (num9 < 0)
			{
				num9 = 0;
			}
			if (num11 < 25)
			{
				num9 = -num9;
			}
			num9 += intParameter2;
			if (gameObject2.HasEffect("Running") && (Skill != "Pistol" || !gameObject2.HasSkill("Pistol_SlingAndRun")) && !gameObject2.HasProperty("EnhancedSprint"))
			{
				num9 += Stat.Random(-23, 23);
			}
			if (flag)
			{
				num9 += Stat.Random(-23, 23);
			}
			bool flag2 = false;
			int Spread = 0;
			int num12 = 0;
			if (num8 > 1)
			{
				flag2 = GetFixedMissileSpreadEvent.GetFor(ParentObject, out Spread);
				if (flag2)
				{
					num12 = Stat.Random(-WeaponAccuracy, WeaponAccuracy);
				}
			}
			List<bool> list4 = new List<bool>(num8);
			List<bool> list5 = new List<bool>(num8);
			List<bool> list6 = new List<bool>(num8);
			List<Cell> list7 = new List<Cell>(num8);
			for (int k = 0; k < num8; k++)
			{
				int num13 = intParameter2;
				int num14 = num9;
				int value;
				if (flag2)
				{
					value = num12;
					int num15 = -Spread / 2 + Spread * k / (num8 - 1);
					num13 += num15;
					num14 += num15;
				}
				else
				{
					value = Stat.Random(-WeaponAccuracy, WeaponAccuracy);
				}
				Event event2 = Event.New("WeaponMissileWeaponShot");
				event2.SetParameter("AimVariance", num14);
				event2.SetParameter("FlatVariance", num13);
				event2.SetParameter("WeaponAccuracy", value);
				ParentObject.FireEvent(event2);
				GameObject gameObject7 = ((list.Count > k) ? list[k] : null);
				if (gameObject7 == null)
				{
					MetricsManager.LogError("had no projectile for shot " + k + " from " + ParentObject.DebugName);
					continue;
				}
				bool PlayerInvolved;
				bool CameNearPlayer;
				Cell NearPlayerCell;
				List<Point> item = CalculateBulletTrajectory(out PlayerInvolved, out CameNearPlayer, out NearPlayerCell, missilePath, gameObject7, ParentObject, gameObject2, gameObject2.CurrentZone, event2.GetIntParameter("AimVariance").ToString(), event2.GetIntParameter("FlatVariance"), event2.GetIntParameter("WeaponAccuracy"));
				list3.Add(item);
				list4.Add(item: false);
				list5.Add(PlayerInvolved);
				list6.Add(CameNearPlayer);
				list7.Add(NearPlayerCell);
			}
			scrapBuffer2.Copy(scrapBuffer);
			int num16 = Math.Min(num8, ShotsPerAnimation);
			Cell cell3 = IComponent<GameObject>.ThePlayer.GetCurrentCell();
			Event event3 = Event.New("ProjectileEntering", "Attacker", gameObject2);
			Event event4 = Event.New("ProjectileEnteringCell", "Attacker", gameObject2);
			List<GameObject> objectsThatWantEvent = cell.ParentZone.GetObjectsThatWantEvent(ProjectileMovingEvent.ID, ProjectileMovingEvent.CascadeLevel);
			ProjectileMovingEvent projectileMovingEvent = ((objectsThatWantEvent.Count > 0) ? ProjectileMovingEvent.FromPool(gameObject2, ParentObject, null, null, null, cell, null, -1, scrapBuffer2) : null);
			GameObject gameObjectParameter = E.GetGameObjectParameter("MessageAsFrom");
			for (int l = 0; l < list.Count; l += num16)
			{
				int num17 = 0;
				bool flag3 = false;
				for (int m = l; m < l + num16 && m < list3.Count; m++)
				{
					if (list3[m].Count > num17)
					{
						num17 = list3[m].Count;
					}
				}
				int num18 = cell2.X - cell.X;
				int num19 = cell2.Y - cell.Y;
				_ = (int)Math.Sqrt(num18 * num18 + num19 * num19) / RangeIncrement;
				int num20 = ((VariableMaxRange != null) ? Math.Min(VariableMaxRange.RollCached(), MaxRange) : MaxRange);
				bool flag4 = false;
				for (int n = 1; n < num17 && n <= num20; n++)
				{
					if (cell3 != null && gameObject2.CurrentCell != null && gameObject2.CurrentCell.ParentZone == cell3.ParentZone && AmmoChar != "f" && AmmoChar != "m" && AmmoChar != "e")
					{
						scrapBuffer2.Copy(scrapBuffer);
					}
					bool flag5 = true;
					for (int num21 = l; num21 < l + num16 && num21 < list4.Count; num21++)
					{
						if (!list4[num21])
						{
							flag5 = false;
						}
					}
					if (flag5)
					{
						break;
					}
					for (int num22 = l; num22 < l + num16 && num22 < list3.Count; num22++)
					{
						if (n >= list3[num22].Count)
						{
							list4[num22] = true;
						}
						if (list4[num22])
						{
							continue;
						}
						Projectile projectile = list2[num22];
						GameObject gameObject8 = list[num22];
						string text2 = projectile.RenderChar ?? AmmoChar;
						scrapBuffer2.Goto(list3[num22][n].X, list3[num22][n].Y);
						if (text2 == "sm")
						{
							scrapBuffer2.Goto(list3[num22][n].X, list3[num22][n].Y);
							int num23 = Stat.Random(1, 3);
							string s = "+";
							if (num23 == 1)
							{
								s = "&R*";
							}
							if (num23 == 2)
							{
								s = "&W*";
							}
							if (num23 == 3)
							{
								s = "&Y*";
							}
							scrapBuffer2.Write(s);
						}
						else if (text2 == "e")
						{
							float num24 = 0f;
							float num25 = 0f;
							float num26 = (float)Stat.Random(85, 185) / 58f;
							num24 = (float)Math.Sin(num26) / 6f;
							num25 = (float)Math.Cos(num26) / 6f;
							int num27 = Stat.Random(1, 3);
							string text3 = "";
							text3 = ((char)Stat.Random(191, 198)).ToString() ?? "";
							if (num27 == 1)
							{
								text3 = "&Y" + text3;
							}
							if (num27 == 2)
							{
								text3 = "&W*" + text3;
							}
							if (num27 == 3)
							{
								text3 = "&C*" + text3;
							}
							XRLCore.ParticleManager.Add(text3, (float)list3[num22][n].X + num24 * 2f, (float)list3[num22][n].Y + num25 * 2f, num24, num25, 2);
							XRLCore.ParticleManager.Frame();
							XRLCore.ParticleManager.Render(scrapBuffer2);
							scrapBuffer2.Goto(list3[num22][n].X, list3[num22][n].Y);
							if (num27 == 1)
							{
								text3 = "&Y" + text3;
							}
							if (num27 == 2)
							{
								text3 = "&W*" + text3;
							}
							if (num27 == 3)
							{
								text3 = "&C*" + text3;
							}
							scrapBuffer2.Write(text3);
						}
						else if (text2.Contains("-"))
						{
							scrapBuffer2.Write(text2.Replace("-", text) ?? "");
						}
						else
						{
							switch (text2)
							{
							case "m":
							{
								float num28 = 0f;
								float num29 = 0f;
								float num30 = (float)Stat.Random(85, 185) / 58f;
								num28 = (float)Math.Sin(num30) / 6f;
								num29 = (float)Math.Cos(num30) / 6f;
								int num31 = Stat.Random(1, 3);
								string text4 = "";
								if (num31 == 1)
								{
									text4 = "°";
								}
								if (num31 == 2)
								{
									text4 = "±";
								}
								if (num31 == 3)
								{
									text4 = "²";
								}
								XRLCore.ParticleManager.Add(text4, list3[num22][n].X, list3[num22][n].Y, num28, num29);
								XRLCore.ParticleManager.Frame();
								XRLCore.ParticleManager.Render(scrapBuffer2);
								scrapBuffer2.Goto(list3[num22][n].X, list3[num22][n].Y);
								if (num31 == 1)
								{
									text4 = "&R*";
								}
								if (num31 == 2)
								{
									text4 = "&W*";
								}
								if (num31 == 3)
								{
									text4 = "&Y*";
								}
								scrapBuffer2.Write(text4);
								break;
							}
							case "HR":
							{
								for (int num38 = 1; num38 < list3[num22].Count && num38 < n; num38++)
								{
									scrapBuffer2.Goto(list3[num22][num38].X, list3[num22][num38].Y);
									string text7 = "&b";
									int num39 = Stat.Random(1, 3);
									if (num39 == 1)
									{
										text7 = "&r";
									}
									if (num39 == 2)
									{
										text7 = "&b";
									}
									if (num39 == 3)
									{
										text7 = "&r";
									}
									int num40 = Stat.Random(1, 3);
									if (num40 == 1)
									{
										text7 += "^b";
									}
									if (num40 == 2)
									{
										text7 += "^r";
									}
									if (num40 == 3)
									{
										text7 += "^b";
									}
									Stat.Random(1, 3);
									scrapBuffer2.Write(text7 + " ");
								}
								break;
							}
							case "FR":
							{
								for (int num32 = 1; num32 < list3[num22].Count && num32 < n; num32++)
								{
									scrapBuffer2.Goto(list3[num22][num32].X, list3[num22][num32].Y);
									string text5 = "&C";
									int num33 = Stat.Random(1, 3);
									if (num33 == 1)
									{
										text5 = "&C";
									}
									if (num33 == 2)
									{
										text5 = "&B";
									}
									if (num33 == 3)
									{
										text5 = "&Y";
									}
									int num34 = Stat.Random(1, 3);
									if (num34 == 1)
									{
										text5 += "^C";
									}
									if (num34 == 2)
									{
										text5 += "^B";
									}
									if (num34 == 3)
									{
										text5 += "^Y";
									}
									Stat.Random(1, 3);
									scrapBuffer2.Write(text5 + (char)(219 + Stat.Random(0, 4)));
								}
								break;
							}
							case "f":
							{
								for (int num35 = 1; num35 < list3[num22].Count && num35 < n; num35++)
								{
									scrapBuffer2.Goto(list3[num22][num35].X, list3[num22][num35].Y);
									string text6 = "&R";
									int num36 = Stat.Random(1, 3);
									if (num36 == 1)
									{
										text6 = "&R";
									}
									if (num36 == 2)
									{
										text6 = "&W";
									}
									if (num36 == 3)
									{
										text6 = "&Y";
									}
									int num37 = Stat.Random(1, 3);
									if (num37 == 1)
									{
										text6 += "^R";
									}
									if (num37 == 2)
									{
										text6 += "^W";
									}
									if (num37 == 3)
									{
										text6 += "^Y";
									}
									Stat.Random(1, 3);
									scrapBuffer2.Write(text6 + (char)(219 + Stat.Random(0, 4)));
								}
								break;
							}
							default:
								scrapBuffer2.Write(text2 ?? "");
								break;
							}
						}
						Cell cell4 = parentZone.GetCell(list3[num22][n - 1].X, list3[num22][n - 1].Y);
						Cell cell5 = parentZone.GetCell(list3[num22][n].X, list3[num22][n].Y);
						if (cell5 != null && cell5.IsVisible())
						{
							flag3 = true;
						}
						cell5.FindSolidObjectForMissile(gameObject2, gameObject8, out var SolidObject, out var IsSolid);
						GameObject owner = gameObject8;
						GameObject combatTarget2 = cell5.GetCombatTarget(gameObject2, IgnoreFlight: true, IgnoreAttackable: false, IgnorePhase: false, 0, gameObject8, owner, AllowInanimate: true, InanimateSolidOnly: true);
						bool Done = false;
						if (!IsSolid)
						{
							event3.SetParameter("Cell", cell5);
							event3.SetParameter("Path", missilePath);
							event3.SetParameter("p", n);
							gameObject8.FireEvent(event3);
							event4.SetParameter("Projectile", gameObject8);
							event4.SetParameter("Cell", cell5);
							event4.SetParameter("Path", missilePath);
							event4.SetParameter("p", n);
							if (!cell5.FireEvent(event4))
							{
								Done = true;
							}
							else if (projectileMovingEvent != null)
							{
								projectileMovingEvent.Projectile = gameObject8;
								projectileMovingEvent.Defender = combatTarget2;
								projectileMovingEvent.Cell = cell5;
								projectileMovingEvent.Path = list3[num22];
								projectileMovingEvent.PathIndex = n;
								foreach (GameObject item4 in objectsThatWantEvent)
								{
									if (!item4.HandleEvent(projectileMovingEvent))
									{
										Done = true;
										break;
									}
								}
							}
						}
						bool flag6 = false;
						if (combatTarget2 != null && (!flag || cell5.DistanceTo(gameObject) >= 2))
						{
							if (AutoAct.IsActive())
							{
								if (combatTarget2.IsPlayerControlledAndPerceptible() && !combatTarget2.IsTrifling && !gameObject2.IsPlayerControlled())
								{
									AutoAct.Interrupt("you " + combatTarget2.GetPerceptionVerb() + " something shooting at " + combatTarget2.the + combatTarget2.BaseDisplayName + (combatTarget2.IsVisible() ? "" : (" " + The.Player.DescribeDirectionToward(combatTarget2))), null, combatTarget2);
								}
								else if (combatTarget2.DistanceTo(IComponent<GameObject>.ThePlayer) <= 1 && gameObject2.IsHostileTowards(IComponent<GameObject>.ThePlayer))
								{
									AutoAct.Interrupt("something is shooting at you or " + combatTarget2.the + combatTarget2.BaseDisplayName, null, combatTarget2);
								}
							}
							int num41 = Stat.Random(1, 20) + Math.Max(0, gameObject2.StatMod(Modifier));
							int num42 = num41;
							if (gameObject2.HasRegisteredEvent("ModifyMissileWeaponToHit") || ParentObject.HasRegisteredEvent("ModifyMissileWeaponToHit"))
							{
								eModifyMissileWeaponToHit.SetParameter("Amount", 0);
								eModifyMissileWeaponToHit.SetParameter("Target", gameObject6);
								eModifyMissileWeaponToHit.SetParameter("AimedAt", gameObject5);
								eModifyMissileWeaponToHit.SetParameter("Defender", combatTarget2);
								gameObject2.FireEvent(eModifyMissileWeaponToHit);
								ParentObject.FireEvent(eModifyMissileWeaponToHit);
								num41 += eModifyMissileWeaponToHit.GetIntParameter("Amount");
							}
							if (combatTarget2.HasRegisteredEvent("ModifyIncomingMissileWeaponToHit"))
							{
								eModifyIncomingMissileWeaponToHit.SetParameter("Amount", 0);
								eModifyIncomingMissileWeaponToHit.SetParameter("Target", gameObject6);
								eModifyIncomingMissileWeaponToHit.SetParameter("AimedAt", gameObject5);
								eModifyIncomingMissileWeaponToHit.SetParameter("Defender", combatTarget2);
								combatTarget2.FireEvent(eModifyIncomingMissileWeaponToHit);
								num41 += eModifyIncomingMissileWeaponToHit.GetIntParameter("Amount");
							}
							int combatDV = Stats.GetCombatDV(combatTarget2);
							Event event5 = Event.New("WeaponGetDefenderDV");
							event5.AddParameter("Weapon", ParentObject);
							event5.AddParameter("Defender", combatTarget2);
							event5.AddParameter("NaturalHitResult", num42);
							event5.AddParameter("Result", num41);
							event5.AddParameter("Skill", Skill);
							event5.AddParameter("DV", Stats.GetCombatDV(combatTarget2));
							combatTarget2?.FireEvent(event5);
							event5.ID = "ProjectileGetDefenderDV";
							projectile?.FireEvent(event5);
							combatDV = event5.GetIntParameter("DV");
							if (!combatTarget2.HasSkill("Acrobatics_SwiftReflexes"))
							{
								combatDV -= 5;
							}
							if (!combatTarget2.HasPart("Brain") || !combatTarget2.pBrain.Mobile)
							{
								combatDV = -100;
							}
							if (num41 > combatDV)
							{
								if (gameObject8.HasTagOrProperty("NoDodging"))
								{
									if (combatTarget2.IsPlayer())
									{
										if (combatTarget2.HasPart("Combat") && combatTarget2.CanChangeMovementMode("Dodging"))
										{
											IComponent<GameObject>.XDidYToZ(combatTarget2, "attempt", "to flinch away, but", gameObject8, "is too wide", "!", null, null, combatTarget2);
										}
									}
									else if (combatTarget2.IsVisible() && combatTarget2.HasPart("Combat") && combatTarget2.CanChangeMovementMode("Dodging"))
									{
										IComponent<GameObject>.XDidYToZ(combatTarget2, "attempt", "to flinch out of the way of", gameObject8, ", but it's too wide", "!", null, null, combatTarget2);
									}
								}
								if (IComponent<GameObject>.Visible(combatTarget2))
								{
									flag4 = true;
								}
								flag6 = true;
								bool PenetrateWalls = false;
								MissileHit(gameObject2, combatTarget2, gameObject, gameObject8, projectile, gameObject5, gameObject6, missilePath, fireType, intParameter, num42, list5[num22], gameObjectParameter, ref Done, ref PenetrateWalls);
							}
							else if (combatDV != -100 && combatTarget2.InActiveZone() && !gameObject8.HasTagOrProperty("NoDodging"))
							{
								string passByVerb = projectile.PassByVerb;
								combatTarget2.ParticleBlip("&K\t");
								if (combatTarget2.IsPlayer())
								{
									if (!string.IsNullOrEmpty(passByVerb))
									{
										if (combatTarget2.HasPart("Combat") && combatTarget2.CanChangeMovementMode("Dodging"))
										{
											IComponent<GameObject>.XDidYToZ(combatTarget2, "flinch", "away as", gameObject8, gameObject8.GetVerb(passByVerb, PrependSpace: false) + " past " + IComponent<GameObject>.ThePlayer.DescribeDirectionFrom(gameObject), "!", null, combatTarget2, null, UseFullNames: false, IndefiniteSubject: false, IndefiniteObject: true);
										}
										else
										{
											IComponent<GameObject>.XDidYToZ(gameObject8, passByVerb, "past " + IComponent<GameObject>.ThePlayer.DescribeDirectionFrom(gameObject), combatTarget2, null, "!", null, combatTarget2, null, UseFullNames: false, IndefiniteSubject: true);
										}
									}
								}
								else if (combatTarget2.IsVisible())
								{
									if (combatTarget2.HasPart("Combat") && combatTarget2.CanChangeMovementMode("Dodging"))
									{
										IComponent<GameObject>.XDidYToZ(combatTarget2, "flinch", "out of the way of", gameObject8, gameObject.IsPlayerControlled() ? null : IComponent<GameObject>.ThePlayer.DescribeDirectionFrom(gameObject), "!", null, combatTarget2, null, UseFullNames: false, IndefiniteSubject: false, IndefiniteObject: true);
									}
									else if (!string.IsNullOrEmpty(passByVerb))
									{
										IComponent<GameObject>.XDidYToZ(gameObject8, passByVerb, "past", combatTarget2, gameObject.IsPlayerControlled() ? null : IComponent<GameObject>.ThePlayer.DescribeDirectionFrom(gameObject), "!", null, combatTarget2, null, UseFullNames: false, IndefiniteSubject: true);
									}
								}
							}
							combatTarget2.StopMoving();
						}
						if (!(IsSolid || Done))
						{
							continue;
						}
						bool PenetrateWalls2 = false;
						if (IsSolid && !flag6)
						{
							if (SolidObject != null)
							{
								int naturalHitResult = Stat.Random(1, 20) + Math.Max(0, gameObject2.StatMod(Modifier));
								if (IComponent<GameObject>.Visible(SolidObject))
								{
									flag4 = true;
								}
								MissileHit(gameObject2, SolidObject, gameObject, gameObject8, projectile, gameObject5, gameObject6, missilePath, fireType, intParameter, naturalHitResult, list5[num22], gameObjectParameter, ref Done, ref PenetrateWalls2);
							}
							else
							{
								Event event6 = Event.New("ProjectileHit");
								event6.SetParameter("Attacker", gameObject2);
								event6.SetParameter("Defender", (object)null);
								event6.SetParameter("Skill", Skill);
								event6.SetParameter("Damage", (object)null);
								event6.SetParameter("Critical", 0);
								event6.SetParameter("AimLevel", intParameter);
								event6.SetParameter("Owner", gameObject2);
								event6.SetParameter("Launcher", ParentObject);
								event6.SetParameter("Path", missilePath);
								event6.SetParameter("Penetrations", 0);
								event6.SetParameter("ApparentTarget", gameObject6);
								event6.SetParameter("AimedAt", gameObject5);
								gameObject8.FireEvent(event6);
								event6.ID = "DefenderProjectileHit";
								combatTarget2.FireEvent(event6);
								event6.ID = "LauncherProjectileHit";
								ParentObject.FireEvent(event6);
							}
						}
						bool flag7 = !Done && IsSolid && PenetrateWalls2;
						if (!flag7)
						{
							list4[num22] = true;
						}
						if (IsSolid && !flag7)
						{
							cell4.AddObject(gameObject8);
						}
						else if (Done)
						{
							cell5.AddObject(gameObject8);
						}
						if (!flag7)
						{
							gameObject8.WasThrown(gameObject2, gameObject6);
							CleanupProjectile(gameObject8);
						}
					}
					if (flag3 && gameObject2.CurrentCell.ParentZone.IsActive())
					{
						XRLCore._Console.DrawBuffer(scrapBuffer2);
						if (AnimationDelay > 0)
						{
							Thread.Sleep(Math.Max(AnimationDelay - num17 / 5, 1));
						}
					}
				}
				if (!flag4 && list6[l] && !list5[l])
				{
					GameObject obj3 = list[l];
					Projectile projectile2 = list2[l];
					if (GameObject.validate(ref obj3) && !string.IsNullOrEmpty(projectile2.PassByVerb))
					{
						IComponent<GameObject>.AddPlayerMessage(obj3.A + obj3.ShortDisplayName + obj3.GetVerb(projectile2.PassByVerb) + " past " + IComponent<GameObject>.ThePlayer.DescribeDirectionFrom(gameObject) + ".");
					}
					if (!gameObject2.IsPlayerLed())
					{
						AutoAct.Interrupt(null, list7[l]);
					}
				}
			}
			for (int num43 = list.Count - 1; num43 >= 0; num43--)
			{
				GameObject obj4 = list[num43];
				if (GameObject.validate(ref obj4))
				{
					obj4.Obliterate();
				}
			}
			float num44 = 1f;
			if (E.HasParameter("EnergyMultiplier"))
			{
				num44 = (float)E.GetParameter("EnergyMultiplier");
			}
			if (Skill == "Pistol")
			{
				if (gameObject2.HasEffect("EmptyTheClips"))
				{
					num44 *= 0.5f;
				}
				if (gameObject2.HasSkill("Pistol_FastestGun"))
				{
					num44 *= 0.75f;
				}
				if (gameObject2.HasIntProperty("PistolEnergyModifier"))
				{
					float num45 = (100f - (float)gameObject2.GetIntProperty("PistolEnergyModifier", 100)) / 100f;
					num44 *= num45;
				}
			}
			if (ParentObject.HasRegisteredEvent("ShotComplete"))
			{
				ParentObject.FireEvent(Event.New("ShotComplete", "AmmoObject", gameObject3));
			}
			gameObject.UseEnergy((int)((float)EnergyCost * num44), "Combat Missile " + Skill);
		}
		return base.FireEvent(E);
	}

	public bool ReadyToFire()
	{
		return ParentObject.FireEvent("CheckReadyToFire");
	}

	public string GetNotReadyToFireMessage()
	{
		Event @event = Event.New("GetNotReadyToFireMessage");
		ParentObject.FireEvent(@event);
		return @event.GetStringParameter("Message");
	}

	public string Status()
	{
		StringBuilder stringBuilder = Event.NewStringBuilder();
		ParentObject.FireEvent(Event.New("GetMissileWeaponStatus", "Items", stringBuilder));
		string text = null;
		if (stringBuilder.Length > 0)
		{
			text = stringBuilder.ToString();
		}
		int num = 23;
		if (text != null)
		{
			num -= ColorUtility.LengthExceptFormatting(text);
		}
		string text2;
		if (num > 0)
		{
			text2 = ParentObject.ShortDisplayNameStripped;
			if (text2.Length > num)
			{
				text2 = text2.Substring(0, num).Trim();
			}
			if (text != null)
			{
				text2 += text;
			}
		}
		else
		{
			text2 = text ?? "";
		}
		return text2;
	}
}
