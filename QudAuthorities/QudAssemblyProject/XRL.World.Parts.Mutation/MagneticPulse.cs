using System;
using System.Collections.Generic;
using System.Linq;
using XRL.Core;
using XRL.Rules;
using XRL.UI;

namespace XRL.World.Parts.Mutation;

[Serializable]
public class MagneticPulse : BaseMutation
{
	public new Guid ActivatedAbilityID = Guid.Empty;

	public int Cooldown = 100;

	public int PulseCharging;

	public MagneticPulse()
	{
		DisplayName = "Magnetic Pulse";
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != AIBoredEvent.ID && ID != BeginTakeActionEvent.ID)
		{
			return ID == CommandEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(AIBoredEvent E)
	{
		if (PulseCharging == 0 && IsMyActivatedAbilityAIUsable(ActivatedAbilityID))
		{
			CommandEvent.Send(E.Actor, "CommandMagneticPulse");
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(BeginTakeActionEvent E)
	{
		if (PulseCharging > 0)
		{
			PulseCharging++;
			ParentObject.UseEnergy(1000, "Physical Mutation");
			if (PulseCharging >= 2)
			{
				EmitMagneticPulse(ParentObject, GetRadius(base.Level));
				PulseCharging = 0;
			}
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(CommandEvent E)
	{
		if (E.Command == "CommandMagneticPulse")
		{
			PulseCharging = 1;
			ParentObject.UseEnergy(1000, "Physical Mutation");
			CooldownMyActivatedAbility(ActivatedAbilityID, Cooldown);
		}
		return base.HandleEvent(E);
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "AIGetOffensiveMutationList");
		base.Register(Object);
	}

	public override string GetDescription()
	{
		return "You emit powerful magnetic pulses.";
	}

	public override string GetLevelText(int Level)
	{
		return string.Concat("After a one turn charge time, you emit a pulse with radius " + Level + " that attempts to pull metal objects toward you, including metal gear equipped on creatures.\n", "Cooldown: ", Cooldown.ToString(), " rounds\n");
	}

	public override bool Render(RenderEvent E)
	{
		if (PulseCharging == 1)
		{
			int num = XRLCore.CurrentFrame % 60;
			if (num > 35 && num < 45)
			{
				E.Tile = null;
				E.RenderString = ">";
				E.ColorString = "&C";
			}
		}
		return base.Render(E);
	}

	public static bool IsFerromagnetic(GameObject obj)
	{
		if ((obj.HasPart("Metal") || obj.HasPart("ModMagnetized") || obj.HasPropertyOrTag("Metal")) && !obj.HasPart("ModHardened"))
		{
			return true;
		}
		return false;
	}

	private static int PullResistance(GameObject obj)
	{
		if (obj.HasStat("Strength"))
		{
			return obj.Stat("Strength");
		}
		return (int)Math.Round((double)obj.GetKineticResistance() * 0.1, MidpointRounding.AwayFromZero);
	}

	private static int PullStrengthInstance(int Radius)
	{
		return Radius * 10 - Stat.Random(1, 20);
	}

	private static int PullSquares(int PullStrength, int PullResistance)
	{
		return (PullStrength - PullResistance) / 5;
	}

	public static void EmitMagneticPulse(GameObject attacker, int Radius)
	{
		if (!GameObject.validate(attacker))
		{
			return;
		}
		Cell cell = attacker.CurrentCell;
		if (cell == null)
		{
			return;
		}
		IComponent<GameObject>.XDidY(attacker, "emit", "a powerful magnetic pulse", "!", null, attacker);
		if (IComponent<GameObject>.Visible(attacker))
		{
			for (int num = Radius; num > 0; num -= 2)
			{
				float num2 = 1f;
				for (int i = 0; i < 360; i++)
				{
					float num3 = (float)Math.Sin((double)(float)i * 0.017) / num2;
					float num4 = (float)Math.Cos((double)(float)i * 0.017) / num2;
					float num5 = (float)num / num2;
					XRLCore.ParticleManager.Add("@", (float)cell.X + num3 * num5, (float)cell.Y + num4 * num5, 0f - num3, 0f - num4, (int)num5);
				}
			}
		}
		List<Cell> realAdjacentCells = cell.GetRealAdjacentCells(Radius);
		List<Tuple<GameObject, int>> list = new List<Tuple<GameObject, int>>(Radius + 2);
		int num6 = 0;
		foreach (Cell item in realAdjacentCells)
		{
			foreach (GameObject @object in item.Objects)
			{
				if (IsFerromagnetic(@object) || @object.AnyInstalledCybernetics(IsFerromagnetic))
				{
					list.Add(new Tuple<GameObject, int>(@object, -1));
				}
			}
			num6 += item.Objects.Count;
		}
		List<GameObject> list2 = new List<GameObject>(num6);
		foreach (Cell item2 in realAdjacentCells)
		{
			list2.AddRange(item2.Objects);
		}
		list2.Sort((GameObject a, GameObject b) => a.DistanceTo(attacker).CompareTo(b.DistanceTo(attacker)));
		foreach (GameObject affectedObject in list2)
		{
			List<Tuple<Cell, char>> lineTo = affectedObject.GetLineTo(attacker);
			GameObject randomElement = (from o in affectedObject.GetInventoryAndEquipment()
				where IsFerromagnetic(o) && o.Implantee == null
				select o).GetRandomElement();
			GameObject gameObject = null;
			if (randomElement != null)
			{
				int pullStrength = PullStrengthInstance(Radius);
				int pullResistance = PullResistance(randomElement);
				int num7 = PullSquares(pullStrength, pullResistance);
				if (num7 > 0)
				{
					if (randomElement.Equipped != null && (randomElement.EquippedOn()?.Type == "Body" || !randomElement.UnequipAndRemove()))
					{
						if (!list.Any((Tuple<GameObject, int> e) => e.Item1 == affectedObject))
						{
							list.Add(new Tuple<GameObject, int>(affectedObject, num7));
						}
					}
					else
					{
						randomElement.RemoveFromContext();
						affectedObject.CurrentCell.AddObject(randomElement);
						Cell cell2 = null;
						for (int j = 1; j <= num7 && j < lineTo.Count - 1 && !lineTo[j].Item1.IsSolid(); j++)
						{
							if (!randomElement.HasPart("Combat") || lineTo[j].Item1.IsEmpty())
							{
								cell2 = lineTo[j].Item1;
							}
						}
						if (cell2 != null && randomElement.DirectMoveTo(cell2) && affectedObject.IsPlayer())
						{
							gameObject = randomElement;
						}
					}
				}
			}
			if (affectedObject.IsPlayer() && gameObject != null)
			{
				Popup.Show(affectedObject.Poss(gameObject) + gameObject.Is + " ripped from your body!");
			}
		}
		list.Sort((Tuple<GameObject, int> a, Tuple<GameObject, int> b) => a.Item1.DistanceTo(attacker).CompareTo(b.Item1.DistanceTo(attacker)));
		foreach (Tuple<GameObject, int> item3 in list)
		{
			int num8 = 0;
			if (item3.Item2 < 0)
			{
				int pullStrength2 = PullStrengthInstance(Radius);
				int pullResistance2 = PullResistance(item3.Item1);
				num8 = PullSquares(pullStrength2, pullResistance2);
			}
			else
			{
				num8 = item3.Item2;
			}
			if (num8 <= 0)
			{
				continue;
			}
			List<Tuple<Cell, char>> lineTo2 = item3.Item1.GetLineTo(attacker);
			Cell cell3 = null;
			for (int k = 1; k <= num8 && k < lineTo2.Count - 1 && !lineTo2[k].Item1.IsSolid(); k++)
			{
				if (!item3.Item1.HasPart("Combat") || lineTo2[k].Item1.IsEmpty())
				{
					cell3 = lineTo2[k].Item1;
				}
			}
			if (cell3 != null && item3.Item1.DirectMoveTo(cell3) && IComponent<GameObject>.Visible(item3.Item1))
			{
				if (IComponent<GameObject>.Visible(attacker))
				{
					IComponent<GameObject>.AddPlayerMessage((item3.Item1.IsPlayer() ? "You" : (item3.Item1.The + item3.Item1.ShortDisplayName)) + item3.Item1.Is + " pulled toward " + attacker.the + attacker.ShortDisplayName + ".");
				}
				else
				{
					IComponent<GameObject>.AddPlayerMessage((item3.Item1.IsPlayer() ? "You" : (item3.Item1.The + item3.Item1.ShortDisplayName)) + item3.Item1.Is + " pulled toward something.");
				}
			}
		}
	}

	public int GetRadius(int Level)
	{
		return 1 + Level;
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "AIGetOffensiveMutationList" && IsMyActivatedAbilityAIUsable(ActivatedAbilityID) && E.GetIntParameter("Distance") > 1)
		{
			E.AddAICommand("CommandMagneticPulse");
		}
		return base.FireEvent(E);
	}

	public override bool ChangeLevel(int NewLevel)
	{
		return base.ChangeLevel(NewLevel);
	}

	private void AddAbility(GameObject GO)
	{
		ActivatedAbilityID = AddMyActivatedAbility("Magnetic Pulse", "CommandMagneticPulse", "Physical Mutation", null, "æ");
	}

	public override bool Mutate(GameObject GO, int Level)
	{
		AddAbility(GO);
		return base.Mutate(GO, Level);
	}

	public override bool Unmutate(GameObject GO)
	{
		RemoveMyActivatedAbility(ref ActivatedAbilityID);
		return base.Unmutate(GO);
	}
}
