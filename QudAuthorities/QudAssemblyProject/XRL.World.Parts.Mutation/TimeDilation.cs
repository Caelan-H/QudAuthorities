using System;
using System.Collections.Generic;
using XRL.Core;
using XRL.Rules;
using XRL.UI;
using XRL.World.Effects;

namespace XRL.World.Parts.Mutation;

[Serializable]
public class TimeDilation : BaseMutation
{
	public const int DEFAULT_RANGE = 9;

	public const int DEFAULT_DURATION = 15;

	public new Guid ActivatedAbilityID;

	public int Duration;

	public int Range = 9;

	public int nFrame;

	public TimeDilation()
	{
		DisplayName = "Time Dilation";
		Type = "Mental";
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == GetItemElementsEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetItemElementsEvent E)
	{
		E.Add("time", 1);
		return base.HandleEvent(E);
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "AIGetOffensiveMutationList");
		Object.RegisterPartEvent(this, "CommandTimeDilation");
		Object.RegisterPartEvent(this, "EndTurn");
		base.Register(Object);
	}

	public override string GetDescription()
	{
		return "You distort time around your person in order to slow down your enemies.";
	}

	public override string GetLevelText(int Level)
	{
		return string.Concat(string.Concat(string.Concat(string.Concat(string.Concat("Creatures within " + Range + " tiles are slowed according to how close they are to you.\n", "Duration: 15 rounds\n"), "Cooldown: ", GetCooldown(Level).ToString(), " rounds\n"), "Distance 1: creatures receive a {{rules|", ((int)(CalculateQuicknessPenaltyMultiplier(1.0, Range, Level) * 100.0)).ToString(), "%}} Quickness penalty\n"), "Distance 4: creatures receive a {{rules|", ((int)(CalculateQuicknessPenaltyMultiplier(4.0, Range, Level) * 100.0)).ToString(), "%}} Quickness penalty\n"), "Distance 7: creatures receive a {{rules|", ((int)(CalculateQuicknessPenaltyMultiplier(7.0, Range, Level) * 100.0)).ToString(), "%}} Quickness penalty");
	}

	public static double CalculateQuicknessPenaltyMultiplier(double Distance, int Range, int MutationLevel)
	{
		return Math.Pow((double)(float)Range - Distance, 2.0) * (double)(0.0005f * (float)MutationLevel + 0.0085f);
	}

	public static void ApplyField(GameObject src, int Range = 9, bool Independent = false, int Duration = 15, int Level = 1)
	{
		if (src.OnWorldMap() || src.IsInGraveyard())
		{
			return;
		}
		Cell cell = src.CurrentCell;
		if (cell == null)
		{
			return;
		}
		List<GameObject> list = cell.ParentZone.FastSquareVisibility(cell.X, cell.Y, Range, "Combat", src, VisibleToPlayerOnly: false, IncludeWalls: true);
		Event e = Event.New("CheckRealityDistortionAccessibility");
		foreach (GameObject item in list)
		{
			if (item == src || !item.FireEvent("CanApplyTimeDilated") || !item.CurrentCell.FireEvent(e))
			{
				continue;
			}
			if (Independent)
			{
				double num = src.RealDistanceTo(item);
				if (num <= (double)Range)
				{
					double speedPenaltyMultiplier = CalculateQuicknessPenaltyMultiplier(num, Range, Level);
					item.ApplyEffect(new TimeDilatedIndependent(Duration, speedPenaltyMultiplier));
				}
			}
			else
			{
				item.ApplyEffect(new TimeDilated(src));
			}
		}
	}

	public void ApplyField()
	{
		ApplyField(ParentObject, Range);
	}

	public override bool Render(RenderEvent E)
	{
		if (Duration > 0)
		{
			Cell cell = ParentObject.CurrentCell;
			nFrame--;
			if (nFrame < 0)
			{
				nFrame = 180;
				for (int i = 0; i < Stat.RandomCosmetic(1, 3); i++)
				{
					float num = (float)Stat.RandomCosmetic(4, 14) / 3f;
					for (int j = 0; j < 360; j++)
					{
						XRLCore.ParticleManager.Add("@", cell.X, cell.Y, (float)Math.Sin((double)(float)j * 0.017) / num, (float)Math.Cos((double)(float)j * 0.017) / num);
					}
				}
			}
		}
		return true;
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "AIGetOffensiveMutationList")
		{
			if (E.GetIntParameter("Distance") <= 10 && IsMyActivatedAbilityAIUsable(ActivatedAbilityID) && ParentObject.FireEvent("CheckRealityDistortionUsability"))
			{
				E.AddAICommand("CommandTimeDilation");
			}
		}
		else if (E.ID == "EndTurn")
		{
			if (Duration > 0)
			{
				Duration--;
				if (Duration > 0)
				{
					ApplyField();
				}
			}
		}
		else if (E.ID == "CommandTimeDilation")
		{
			if (ParentObject.OnWorldMap())
			{
				if (ParentObject.IsPlayer())
				{
					Popup.ShowFail("You cannot do that on the world map.");
				}
				return false;
			}
			if (!ParentObject.FireEvent(Event.New("InitiateRealityDistortionLocal", "Object", ParentObject, "Mutation", this), E))
			{
				return false;
			}
			CooldownMyActivatedAbility(ActivatedAbilityID, GetCooldown(base.Level));
			Duration = 16;
			PlayWorldSound("time_dilation", 0.5f, 0f, combat: true);
			ApplyField();
			UseEnergy(1000, "Mental Mutation");
		}
		return base.FireEvent(E);
	}

	public static int GetCooldown(int Level)
	{
		return 150;
	}

	public override bool ChangeLevel(int NewLevel)
	{
		return base.ChangeLevel(NewLevel);
	}

	public override bool Mutate(GameObject GO, int Level)
	{
		ActivatedAbilityID = AddMyActivatedAbility("Time Dilation", "CommandTimeDilation", "Mental Mutation", null, "ä", null, Toggleable: false, DefaultToggleState: false, ActiveToggle: false, IsAttack: false, IsRealityDistortionBased: true);
		return base.Mutate(GO, Level);
	}

	public override bool Unmutate(GameObject GO)
	{
		RemoveMyActivatedAbility(ref ActivatedAbilityID);
		return base.Unmutate(GO);
	}
}
