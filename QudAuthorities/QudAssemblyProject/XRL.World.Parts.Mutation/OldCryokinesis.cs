using System;
using System.Collections.Generic;
using System.Threading;
using ConsoleLib.Console;
using XRL.Core;
using XRL.Rules;
using XRL.UI;

namespace XRL.World.Parts.Mutation;

[Serializable]
public class OldCryokinesis : BaseMutation
{
	public Guid FreezeActivatedAbilityID = Guid.Empty;

	public Guid IceBoltActivatedAbilityID = Guid.Empty;

	public Guid IceBurstActivatedAbilityID = Guid.Empty;

	public Guid FrostArmorActivatedAbilityID = Guid.Empty;

	public Guid IceFieldActivatedAbilityID = Guid.Empty;

	public Guid FrostAuraActivatedAbilityID = Guid.Empty;

	private int OldFreeze = -1;

	private int OldBrittle = -1;

	public OldCryokinesis()
	{
		DisplayName = "OldCryokinesis";
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
		E.Add("ice", 1);
		return base.HandleEvent(E);
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "AIGetOffensiveMutationList");
		Object.RegisterPartEvent(this, "CommandFreeze");
		Object.RegisterPartEvent(this, "CommandFrostArmor");
		Object.RegisterPartEvent(this, "CommandFrostAura");
		Object.RegisterPartEvent(this, "CommandIceBolt");
		Object.RegisterPartEvent(this, "CommandIceBurst");
		Object.RegisterPartEvent(this, "CommandIceField");
		Object.RegisterPartEvent(this, "EndTurn");
		base.Register(Object);
	}

	public void Freeze(Cell C)
	{
		TextConsole textConsole = Look._TextConsole;
		ScreenBuffer scrapBuffer = TextConsole.ScrapBuffer;
		if (C.IsVisible())
		{
			XRLCore.Core.RenderMapToBuffer(scrapBuffer);
		}
		if (C != null)
		{
			foreach (GameObject item in C.GetObjectsInCell())
			{
				item.TemperatureChange(-20 + -20 * Stat.Random(1, base.Level), ParentObject);
				if (C.IsVisible())
				{
					for (int i = 0; i < 5; i++)
					{
						item.ParticleText("&C" + (char)(219 + Stat.Random(0, 4)), 4.9f, 5);
					}
					for (int j = 0; j < 5; j++)
					{
						item.ParticleText("&c" + (char)(219 + Stat.Random(0, 4)), 4.9f, 5);
					}
					for (int k = 0; k < 5; k++)
					{
						item.ParticleText("&Y" + (char)(219 + Stat.Random(0, 4)), 4.9f, 5);
					}
				}
			}
		}
		if (C.IsVisible())
		{
			scrapBuffer.Goto(C.X, C.Y);
			scrapBuffer.Write("&Y*");
			textConsole.DrawBuffer(scrapBuffer);
			Thread.Sleep(50);
		}
	}

	public override bool Render(RenderEvent E)
	{
		if (IsMyActivatedAbilityToggledOn(FrostAuraActivatedAbilityID))
		{
			E.ColorString = "&C";
		}
		else if (IsMyActivatedAbilityToggledOn(FrostArmorActivatedAbilityID))
		{
			E.ColorString = "&c";
		}
		return true;
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "EndTurn")
		{
			if (IsMyActivatedAbilityToggledOn(FrostAuraActivatedAbilityID))
			{
				ParentObject.pPhysics.Temperature = -500;
			}
			else if (IsMyActivatedAbilityToggledOn(FrostArmorActivatedAbilityID))
			{
				ParentObject.pPhysics.Temperature = 0;
			}
		}
		else if (E.ID == "AIGetOffensiveMutationList")
		{
			int intParameter = E.GetIntParameter("Distance");
			if (intParameter <= 1 && IsMyActivatedAbilityAIUsable(FreezeActivatedAbilityID))
			{
				E.AddAICommand("CommandFreeze");
			}
			if (intParameter <= 5 && IsMyActivatedAbilityAIUsable(IceBoltActivatedAbilityID))
			{
				E.AddAICommand("CommandIceBolt");
			}
			if (intParameter <= 6 && IsMyActivatedAbilityAIUsable(IceBurstActivatedAbilityID))
			{
				E.AddAICommand("CommandIceBurst");
			}
			if (IsMyActivatedAbilityAIUsable(FrostArmorActivatedAbilityID) && !IsMyActivatedAbilityToggledOn(FrostArmorActivatedAbilityID))
			{
				E.AddAICommand("CommandFrostArmor");
			}
			if (IsMyActivatedAbilityAIUsable(FrostAuraActivatedAbilityID) && !IsMyActivatedAbilityToggledOn(FrostAuraActivatedAbilityID))
			{
				E.AddAICommand("CommandFrostAura");
			}
		}
		else if (E.ID == "CommandFreeze")
		{
			Cell cell = PickDirection();
			if (cell == null)
			{
				return false;
			}
			if (base.Level < 6)
			{
				CooldownMyActivatedAbility(FreezeActivatedAbilityID, 20 - base.Level);
			}
			UseEnergy(1000, "Mental Mutation Old Cryokinesis Freeze");
			Freeze(cell);
		}
		else if (E.ID == "CommandIceBolt")
		{
			List<Cell> list = PickLine(5, AllowVis.Any);
			if (list == null)
			{
				return false;
			}
			if (base.Level < 7)
			{
				CooldownMyActivatedAbility(IceBoltActivatedAbilityID, 20 - base.Level);
			}
			UseEnergy(1000, "Mental Mutation Old Cryokinesis Ice Bolt");
			int num = 0;
			foreach (Cell item in list)
			{
				if (item != ParentObject.CurrentCell)
				{
					Freeze(item);
					num++;
					if (num >= 5)
					{
						break;
					}
				}
			}
		}
		else if (E.ID == "CommandIceBurst")
		{
			List<Cell> list2 = PickBurst(1, 5, bLocked: false, AllowVis.OnlyVisible);
			if (list2 == null)
			{
				return false;
			}
			if (base.Level < 7)
			{
				CooldownMyActivatedAbility(IceBurstActivatedAbilityID, 20 - base.Level);
			}
			UseEnergy(1000, "Mental Mutation Old Cryokinesis Ice Burst");
			foreach (Cell item2 in list2)
			{
				if (item2 != ParentObject.CurrentCell)
				{
					Freeze(item2);
				}
			}
		}
		else if (E.ID == "CommandIceField")
		{
			List<Cell> list3 = PickField(9);
			if (list3 == null || list3.Count == 0)
			{
				return false;
			}
			if (base.Level < 8)
			{
				CooldownMyActivatedAbility(IceFieldActivatedAbilityID, 20 - base.Level);
			}
			UseEnergy(1000, "Mental Mutation Old Cryokinesis Ice Field");
			foreach (Cell item3 in list3)
			{
				Freeze(item3);
			}
		}
		else if (E.ID == "CommandFrostArmor")
		{
			ToggleMyActivatedAbility(FrostArmorActivatedAbilityID);
			if (IsMyActivatedAbilityToggledOn(FrostArmorActivatedAbilityID))
			{
				ParentObject.Statistics["AV"].Bonus += 2;
			}
			else
			{
				ParentObject.Statistics["AV"].Bonus -= 2;
			}
			UseEnergy(1000, "Mental Mutation Old Cryokinesis Frost Armor");
		}
		else if (E.ID == "CommandFrostAura")
		{
			ToggleMyActivatedAbility(FrostAuraActivatedAbilityID);
			UseEnergy(1000, "Mental Mutation Old Cryokinesis Frost Aura");
		}
		return base.FireEvent(E);
	}

	public override bool ChangeLevel(int NewLevel)
	{
		return base.ChangeLevel(NewLevel);
	}

	public override bool Mutate(GameObject GO, int Level)
	{
		if (GO.pPhysics != null)
		{
			OldFreeze = GO.pPhysics.FreezeTemperature;
			OldBrittle = GO.pPhysics.BrittleTemperature;
			GO.pPhysics.FreezeTemperature = -600 + -300 * Level;
			GO.pPhysics.BrittleTemperature = -1000 + -500 * Level;
		}
		if (Level >= 1)
		{
			FreezeActivatedAbilityID = AddMyActivatedAbility("Freeze", "CommandFreeze", "Mutation");
		}
		if (Level >= 2)
		{
			IceBoltActivatedAbilityID = AddMyActivatedAbility("Ice Bolt", "CommandIceBolt", "Mutation");
			IceBurstActivatedAbilityID = AddMyActivatedAbility("Ice Burst", "CommandIceBurst", "Mutation");
		}
		if (Level >= 3)
		{
			FrostArmorActivatedAbilityID = AddMyActivatedAbility("Frost Armor", "CommandFrostArmor", "Mutation", null, "\a", null, Toggleable: true);
		}
		if (Level >= 4)
		{
			IceFieldActivatedAbilityID = AddMyActivatedAbility("Ice Field", "CommandIceField", "Mutation");
		}
		if (Level >= 5)
		{
			FrostAuraActivatedAbilityID = AddMyActivatedAbility("Frost Aura", "CommandFrostAura", "Mutation", null, "\a", null, Toggleable: true);
		}
		return base.Mutate(GO, Level);
	}

	public override bool Unmutate(GameObject GO)
	{
		if (GO.pPhysics != null)
		{
			if (OldFreeze != -1)
			{
				GO.pPhysics.FreezeTemperature = OldFreeze;
			}
			if (OldBrittle != -1)
			{
				GO.pPhysics.BrittleTemperature = OldBrittle;
			}
			OldFreeze = -1;
			OldBrittle = -1;
			GO.pPhysics.Temperature = 25;
		}
		RemoveMyActivatedAbility(ref FreezeActivatedAbilityID);
		RemoveMyActivatedAbility(ref IceBoltActivatedAbilityID);
		RemoveMyActivatedAbility(ref IceBurstActivatedAbilityID);
		RemoveMyActivatedAbility(ref FrostArmorActivatedAbilityID);
		RemoveMyActivatedAbility(ref IceFieldActivatedAbilityID);
		RemoveMyActivatedAbility(ref FrostAuraActivatedAbilityID);
		return base.Unmutate(GO);
	}
}
