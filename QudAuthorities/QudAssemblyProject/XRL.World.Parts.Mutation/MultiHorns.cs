using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using ConsoleLib.Console;
using XRL.Language;
using XRL.Rules;
using XRL.World.Capabilities;

namespace XRL.World.Parts.Mutation;

[Serializable]
public class MultiHorns : BaseMutation
{
	public new Guid ActivatedAbilityID;

	[NonSerialized]
	public List<GameObject> Horns = new List<GameObject>(2);

	public int ExtraHeads = 2;

	public bool bSetHeads;

	public string HornsName;

	public int Charging;

	[NonSerialized]
	private List<Cell> chargeCells;

	public string ManagerID => ParentObject.id + "::MultiHorns";

	public MultiHorns()
	{
		DisplayName = "Multiple Horns";
	}

	public override bool AffectsBodyParts()
	{
		return true;
	}

	public override bool GeneratesEquipment()
	{
		return true;
	}

	public override void SaveData(SerializationWriter Writer)
	{
		Writer.WriteGameObjectList(Horns);
		base.SaveData(Writer);
	}

	public override void LoadData(SerializationReader Reader)
	{
		Horns = new List<GameObject>();
		Reader.ReadGameObjectList(Horns);
		base.LoadData(Reader);
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "AIGetOffensiveMutationList");
		Object.RegisterPartEvent(this, "BeginTakeAction");
		Object.RegisterPartEvent(this, "CommandMassiveCharge");
		base.Register(Object);
	}

	private bool ValidChargeTarget(GameObject obj)
	{
		return obj?.HasPart("Combat") ?? false;
	}

	public void PickChargeTarget()
	{
		if (Charging > 0)
		{
			return;
		}
		chargeCells = PickLine(GetChargeDistance(base.Level), AllowVis.OnlyVisible, ValidChargeTarget);
		if (chargeCells == null)
		{
			return;
		}
		if (chargeCells != null)
		{
			chargeCells = new List<Cell>(chargeCells);
		}
		if (chargeCells.FirstOrDefault() != ParentObject.CurrentCell)
		{
			chargeCells.Insert(0, ParentObject.CurrentCell);
		}
		if (chargeCells.Count <= 1 || chargeCells[0].ParentZone != chargeCells[1].ParentZone)
		{
			return;
		}
		while (chargeCells.Count < GetChargeDistance(base.Level) + 1)
		{
			for (int i = 0; i < chargeCells.Count - 1; i++)
			{
				if (chargeCells.Count >= GetChargeDistance(base.Level) + 1)
				{
					break;
				}
				if (chargeCells[i].ParentZone != chargeCells[i + 1].ParentZone)
				{
					break;
				}
				string directionFromCell = chargeCells[i].GetDirectionFromCell(chargeCells[i + 1]);
				chargeCells.Add(chargeCells[chargeCells.Count - 1].GetCellFromDirection(directionFromCell, BuiltOnly: false));
			}
		}
	}

	public string pathDirectionAtStep(int n, List<Cell> path)
	{
		if (path.Count <= 1)
		{
			return ".";
		}
		n %= path.Count;
		return path[n].GetDirectionFromCell(path[(n + 1) % path.Count]);
	}

	public Cell cellAtStep(int n, List<Cell> path)
	{
		if (path.Count <= 1)
		{
			return path[0];
		}
		if (n < path.Count)
		{
			return path[n];
		}
		Cell cell = path[path.Count - 1];
		for (int i = path.Count; i <= n; i++)
		{
			cell = cell.GetCellFromDirection(pathDirectionAtStep(n, path), BuiltOnly: false);
		}
		return cell;
	}

	public void performCharge(List<Cell> chargePath, bool bDoEffect = true)
	{
		Charging = 0;
		Dictionary<GameObject, int> dictionary = new Dictionary<GameObject, int>();
		List<GameObject> list = new List<GameObject>();
		if (!chargePath.Any((Cell c) => c.IsVisible()))
		{
			bDoEffect = false;
		}
		ScreenBuffer scrapBuffer = ScreenBuffer.GetScrapBuffer1();
		DidX("charge", null, "!");
		int num = ParentObject.Stat("Strength") + base.Level * 2 - 2;
		int num2 = num;
		int num3 = (num - 5) * (num - 5);
		for (int i = 0; i < chargePath.Count; i++)
		{
			int num4 = i + 1 + list.Count;
			while (num4 != -1)
			{
				int num5 = num4;
				num4 = -1;
				Cell cell = cellAtStep(num5, chargePath);
				foreach (GameObject item in from go in cell.GetObjectsWithTagOrProperty("Wall")
					where go.PhaseAndFlightMatches(ParentObject)
					select go)
				{
					foreach (GameObject item2 in list)
					{
						if (!dictionary.ContainsKey(item2))
						{
							dictionary.Add(item2, 0);
						}
						dictionary[item2]++;
					}
					if (IComponent<GameObject>.Visible(item))
					{
						CombatJuice._cameraShake(0.25f);
					}
					if (num2 > Stats.GetCombatAV(item) + 20)
					{
						item.Destroy();
						continue;
					}
					DidXToY("are", "stopped in " + ParentObject.its + " tracks by", item, null, "!", null, null, ParentObject);
					goto end_IL_0443;
				}
				foreach (GameObject item3 in from go in cell.LoopObjectsWithPart("Combat")
					where go.PhaseAndFlightMatches(ParentObject)
					select go)
				{
					if (!list.Contains(item3) && item3 != ParentObject)
					{
						if (num <= item3.Stat("Strength") || !item3.CanBeInvoluntarilyMoved())
						{
							DidXToY("are", "stopped in " + ParentObject.its + " tracks by", item3, null, "!", null, null, ParentObject);
							goto end_IL_0443;
						}
						list.Add(item3);
						num4 = num5 + 1;
					}
					if (item3.IsPlayer())
					{
						CombatJuice._cameraShake(0.25f);
					}
				}
				foreach (GameObject item4 in from go in cell.LoopObjectsWithPart("Physics")
					where go.PhaseAndFlightMatches(ParentObject)
					select go)
				{
					if (item4.pPhysics.Solid && !item4.HasTagOrProperty("Wall") && !item4.HasPart("Combat") && !list.Contains(item4) && item4 != ParentObject)
					{
						if (num3 <= item4.Weight)
						{
							DidXToY("are", "stopped in " + ParentObject.its + " tracks by", item4, null, "!", null, null, ParentObject);
							goto end_IL_0443;
						}
						list.Add(item4);
						num4 = num5 + 1;
					}
				}
			}
			list.RemoveAll((GameObject O) => O.IsInvalid() || O.IsNowhere());
			for (int num6 = list.Count - 1; num6 >= 0; num6--)
			{
				list[num6].DirectMoveTo(cellAtStep(i + num6 + 1, chargePath));
			}
			ParentObject.DirectMoveTo(cellAtStep(i, chargePath));
			scrapBuffer.RenderBase();
			scrapBuffer.Draw();
			Thread.Sleep(10);
			continue;
			end_IL_0443:
			break;
		}
		foreach (GameObject item5 in list)
		{
			if (item5 == ParentObject)
			{
				continue;
			}
			int num7 = 0;
			if (dictionary.ContainsKey(item5))
			{
				num7 = dictionary[item5];
			}
			int num8 = ExtraHeads + 1 + (ExtraHeads + 1) * Math.Min(3, num7);
			string damageIncrement = GetDamageIncrement(base.Level);
			int num9 = damageIncrement.RollCached();
			for (int j = 0; j < num8; j++)
			{
				num9 += damageIncrement.RollCached();
			}
			if (num9 > 0)
			{
				string message = "from %t charge!";
				if (num7 == 1)
				{
					message = "from being slammed into a wall by %t charge!";
				}
				else if (num7 == 2)
				{
					message = "from being slammed into &Wtwo&y walls by %t charge!";
				}
				else if (num7 >= 3)
				{
					message = "from being slammed into &r" + Grammar.Cardinal(num7) + "&y walls by %t charge!";
				}
				int amount = num9;
				GameObject parentObject = ParentObject;
				item5.TakeDamage(amount, message, null, null, null, null, parentObject);
			}
			else
			{
				IComponent<GameObject>.XDidY(item5, "are", "shoved by " + Grammar.MakePossessive(ParentObject.the + ParentObject.DisplayNameOnly) + " charge!", null, null, null, item5);
			}
		}
		CooldownMyActivatedAbility(ActivatedAbilityID, GetChargeCooldown(base.Level));
		chargePath?.Clear();
	}

	public int GetChargeCooldown(int Level)
	{
		return Math.Max(42 - Level * 3, 5);
	}

	public int GetTurnsToCharge()
	{
		return 1;
	}

	public override bool Render(RenderEvent E)
	{
		if (chargeCells != null && chargeCells.Count > 0 && ParentObject.CurrentCell != null)
		{
			int num = 500;
			if (IComponent<GameObject>.frameTimerMS % num < num / 2)
			{
				E.ColorString = "&r^R";
			}
			E.WantsToPaint = true;
		}
		return true;
	}

	public override void OnPaint(ScreenBuffer buffer)
	{
		if (chargeCells != null)
		{
			int num = 1000;
			int val = num / Math.Max(chargeCells.Count, 1);
			int num2 = (int)(IComponent<GameObject>.frameTimerMS % num / Math.Max(val, 1));
			if (num2 > 0 && num2 < chargeCells.Count && chargeCells[num2].ParentZone == ParentObject.pPhysics.CurrentCell.ParentZone && chargeCells[num2].IsVisible())
			{
				buffer.Goto(chargeCells[num2].X, chargeCells[num2].Y);
				buffer.Write(ParentObject.pRender.RenderString);
				buffer.Buffer[chargeCells[num2].X, chargeCells[num2].Y].Tile = ParentObject.pRender.Tile;
				buffer.Buffer[chargeCells[num2].X, chargeCells[num2].Y].TileForeground = ColorUtility.ColorMap['r'];
				buffer.Buffer[chargeCells[num2].X, chargeCells[num2].Y].Detail = ColorUtility.ColorMap['R'];
				buffer.Buffer[chargeCells[num2].X, chargeCells[num2].Y].SetForeground('r');
			}
			base.OnPaint(buffer);
		}
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "BeginTakeAction")
		{
			if (Charging > 0)
			{
				if (chargeCells == null || chargeCells.Count == 0)
				{
					PickChargeTarget();
				}
				if (chargeCells == null || chargeCells.Count == 0)
				{
					Charging = 0;
				}
				else
				{
					Charging--;
					ParentObject.UseEnergy(1000, "Physical Mutation Massive Charge");
					if (Charging > 0)
					{
						return false;
					}
					performCharge(chargeCells);
				}
			}
		}
		else if (E.ID == "CommandMassiveCharge")
		{
			if (ParentObject.GetCurrentCell() == null || ParentObject.GetCurrentCell().OnWorldMap())
			{
				return ParentObject.ShowFailure("You can't do that here.");
			}
			PickChargeTarget();
			if (chargeCells == null || chargeCells.Count <= 0)
			{
				return false;
			}
			Charging = GetTurnsToCharge();
			ParentObject.UseEnergy(1000, "Physical Mutation Massive Charge");
			DidX("stomp", "with bestial fury", "!", null, ParentObject);
			if (Visible() && AutoAct.IsInterruptable())
			{
				AutoAct.Interrupt(null, null, ParentObject);
			}
		}
		else if (E.ID == "AIGetOffensiveMutationList" && Charging <= 0 && E.GetIntParameter("Distance") <= GetChargeDistance(base.Level) && IsMyActivatedAbilityAIUsable(ActivatedAbilityID) && ParentObject.HasLOSTo(ParentObject.Target))
		{
			E.AddAICommand("CommandMassiveCharge");
		}
		return base.FireEvent(E);
	}

	public override string GetDescription()
	{
		return "Several horns jut out of your head.";
	}

	public int GetChargeDistance(int Level)
	{
		return 8 + Level;
	}

	public string GetDamageIncrement(int Level)
	{
		return "2d" + (Level / 2 + 3);
	}

	public override string GetLevelText(int Level)
	{
		string text = "";
		int num = 0;
		if (Level == 1)
		{
			text = "2d3";
			num = 0;
		}
		if (Level == 2)
		{
			text = "2d4";
			num = 0;
		}
		if (Level == 3)
		{
			text = "2d4";
			num = 1;
		}
		if (Level == 4)
		{
			text = "2d5";
			num = 1;
		}
		if (Level == 5)
		{
			text = "2d5";
			num = 1;
		}
		if (Level == 6)
		{
			text = "2d6";
			num = 1;
		}
		if (Level == 7)
		{
			text = "2d6";
			num = 2;
		}
		if (Level == 8)
		{
			text = "2d7";
			num = 2;
		}
		if (Level == 9)
		{
			text = "2d7";
			num = 2;
		}
		if (Level >= 10)
		{
			text = "2d8";
			num = 2;
		}
		string text2 = "20% chance on melee attack to gore your opponent\n";
		text2 = text2 + "Damage increment: " + text + "\n";
		text2 = ((Level != base.Level) ? (text2 + "{{rules|Increased bleeding save difficulty and intensity}}\n") : (text2 + "Goring attacks may cause bleeding\n"));
		text2 = text2 + "+" + num + " AV\n";
		text2 += "Cannot wear helmets\n";
		text2 += "Can launch into a destructive charge after a one turn warm-up.\n";
		return text2 + "Charge distance: " + GetChargeDistance(Level);
	}

	public override bool ChangeLevel(int NewLevel)
	{
		Body body = ParentObject.Body;
		BodyPart body2 = body.GetBody();
		if (!bSetHeads)
		{
			for (int i = 0; i < ExtraHeads; i++)
			{
				body2.AddPartAt("Head", 0, null, null, null, null, Category: body2.Category, Manager: ManagerID, RequiresLaterality: null, Mobility: null, Appendage: null, Integral: null, Mortal: null, Abstract: null, Extrinsic: null, Plural: null, Mass: null, Contact: null, IgnorePosition: null, InsertAfter: "Head", OrInsertBefore: "Back").AddPart("Face", 0, null, null, null, null, Category: body2.Category, Manager: ManagerID);
			}
		}
		bSetHeads = true;
		foreach (GameObject horn in Horns)
		{
			horn.Destroy();
		}
		foreach (BodyPart item in body.GetPart("Head"))
		{
			item.ForceUnequip(Silent: true);
			GameObject gameObject = GameObjectFactory.Factory.CreateObject("Horns");
			MeleeWeapon part = gameObject.GetPart<MeleeWeapon>();
			Armor part2 = gameObject.GetPart<Armor>();
			part.HitBonus = Math.Max(NewLevel - 4, 0);
			if (string.IsNullOrEmpty(HornsName))
			{
				HornsName = DisplayName.ToLower();
			}
			gameObject.pRender.DisplayName = HornsName;
			part.Skill = "Cudgel";
			part.MaxStrengthBonus = 10;
			if (base.Level == 1)
			{
				part.BaseDamage = "2d3";
				part2.AV = 0;
			}
			if (base.Level == 2)
			{
				part.BaseDamage = "2d4";
				part2.AV = 0;
			}
			if (base.Level == 3)
			{
				part.BaseDamage = "2d5";
				part2.AV = 1;
			}
			if (base.Level == 4)
			{
				part.BaseDamage = "2d5";
				part2.AV = 1;
			}
			if (base.Level == 5)
			{
				part.BaseDamage = "2d5";
				part2.AV = 1;
			}
			if (base.Level == 6)
			{
				part.BaseDamage = "2d6";
				part2.AV = 1;
			}
			if (base.Level == 7)
			{
				part.BaseDamage = "2d6";
				part2.AV = 2;
			}
			if (base.Level == 8)
			{
				part.BaseDamage = "2d7";
				part2.AV = 2;
			}
			if (base.Level == 9)
			{
				part.BaseDamage = "2d7";
				part2.AV = 2;
			}
			if (base.Level >= 10)
			{
				part.BaseDamage = "2d8";
				part2.AV = 2;
			}
			ParentObject.ForceEquipObject(gameObject, item, Silent: true, 0);
			Horns.Add(gameObject);
		}
		return base.ChangeLevel(NewLevel);
	}

	public override bool Mutate(GameObject GO, int Level)
	{
		if (string.IsNullOrEmpty(HornsName))
		{
			if (GO.HasTagOrProperty("HasHorns"))
			{
				DisplayName = "Triple Horn";
				HornsName = "horn";
			}
			else if (GO.HasTagOrProperty("HasLittleHorns"))
			{
				DisplayName = "Triple Horn";
				HornsName = "little horn";
			}
			else if (GO.Blueprint.Contains("Goat"))
			{
				DisplayName = "Horns";
				HornsName = "horns";
			}
			else if (GO.Blueprint.Contains("Rhino"))
			{
				DisplayName = "Horn";
				HornsName = "horn";
			}
			else
			{
				int num = Stat.Random(1, 100);
				if (num <= 35)
				{
					DisplayName = "Horns";
					HornsName = "horns";
				}
				else if (num <= 65)
				{
					DisplayName = "Antlers";
					HornsName = "antlers";
				}
				else
				{
					DisplayName = "Horn";
					HornsName = "horn";
				}
			}
		}
		ActivatedAbilityID = AddMyActivatedAbility("Wrecking Charge", "CommandMassiveCharge", "Physical Mutation", null, "\u00af");
		return base.Mutate(GO, Level);
	}

	public override bool Unmutate(GameObject GO)
	{
		RemoveMyActivatedAbility(ref ActivatedAbilityID);
		if (Horns.Count > 0)
		{
			foreach (GameObject horn in Horns)
			{
				CleanUpMutationEquipment(GO, horn);
			}
			Horns.Clear();
		}
		GO.RemoveBodyPartsByManager(ManagerID, EvenIfDismembered: true);
		return base.Unmutate(GO);
	}
}
