using System;
using System.Collections.Generic;
using System.Threading;
using ConsoleLib.Console;
using XRL.Core;
using XRL.Rules;
using XRL.UI;

namespace XRL.World.Parts.Mutation;

[Serializable]
public class OldPyrokinesis : BaseMutation
{
	public Guid HeatActivatedAbilityID = Guid.Empty;

	public Guid FireBoltActivatedAbilityID = Guid.Empty;

	public Guid FireBurstActivatedAbilityID = Guid.Empty;

	public Guid FlameArmorActivatedAbilityID = Guid.Empty;

	public Guid FireFieldActivatedAbilityID = Guid.Empty;

	public Guid FlameAuraActivatedAbilityID = Guid.Empty;

	private int OldFlame = -1;

	private int OldVapor = -1;

	public OldPyrokinesis()
	{
		DisplayName = "OldPyrokinesis";
		Type = "Mental";
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "CommandHeat");
		Object.RegisterPartEvent(this, "CommandFireBolt");
		Object.RegisterPartEvent(this, "CommandFireBurst");
		Object.RegisterPartEvent(this, "CommandFlameArmor");
		Object.RegisterPartEvent(this, "CommandFireField");
		Object.RegisterPartEvent(this, "CommandFlameAura");
		Object.RegisterPartEvent(this, "EndTurn");
		Object.RegisterPartEvent(this, "AIGetOffensiveMutationList");
		base.Register(Object);
	}

	public void Heat(Cell C)
	{
		TextConsole textConsole = Look._TextConsole;
		ScreenBuffer scrapBuffer = TextConsole.ScrapBuffer;
		XRLCore.Core.RenderMapToBuffer(scrapBuffer);
		if (C != null)
		{
			foreach (GameObject item in C.GetObjectsInCell())
			{
				item.TemperatureChange(300 + 50 * base.Level, ParentObject);
				if (C.IsVisible())
				{
					for (int i = 0; i < 5; i++)
					{
						item.ParticleText("&r" + (char)(219 + Stat.Random(0, 4)), 4.9f, 5);
					}
					for (int j = 0; j < 5; j++)
					{
						item.ParticleText("&R" + (char)(219 + Stat.Random(0, 4)), 4.9f, 5);
					}
					for (int k = 0; k < 5; k++)
					{
						item.ParticleText("&W" + (char)(219 + Stat.Random(0, 4)), 4.9f, 5);
					}
				}
			}
		}
		scrapBuffer.Goto(C.X, C.Y);
		switch (Stat.Random(1, 3))
		{
		case 1:
			scrapBuffer.Write("&R*");
			break;
		case 2:
			scrapBuffer.Write("&W*");
			break;
		default:
			scrapBuffer.Write("&r*");
			break;
		}
		if (C.IsVisible())
		{
			textConsole.DrawBuffer(scrapBuffer);
			Thread.Sleep(50);
		}
	}

	public override bool Render(RenderEvent E)
	{
		if (IsMyActivatedAbilityToggledOn(FlameAuraActivatedAbilityID))
		{
			if (XRLCore.CurrentFrame % 3 == 0)
			{
				E.ColorString = "&R";
			}
			else if (XRLCore.CurrentFrame % 7 == 0)
			{
				E.ColorString = "&W";
			}
			else
			{
				E.ColorString = "&r";
			}
		}
		else if (IsMyActivatedAbilityToggledOn(FlameArmorActivatedAbilityID))
		{
			if (XRLCore.CurrentFrame % 3 == 0)
			{
				E.ColorString = "&r";
			}
			else if (XRLCore.CurrentFrame % 7 == 0)
			{
				E.ColorString = "&W";
			}
			else
			{
				E.ColorString = "&r";
			}
		}
		return true;
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "EndTurn")
		{
			if (IsMyActivatedAbilityToggledOn(FlameAuraActivatedAbilityID))
			{
				ParentObject.pPhysics.Temperature = 2500;
			}
			else if (IsMyActivatedAbilityToggledOn(FlameArmorActivatedAbilityID))
			{
				ParentObject.pPhysics.Temperature = 450;
			}
		}
		else if (E.ID == "AIGetOffensiveMutationList")
		{
			int intParameter = E.GetIntParameter("Distance");
			if (intParameter <= 1 && IsMyActivatedAbilityAIUsable(HeatActivatedAbilityID))
			{
				E.AddAICommand("CommandHeat");
			}
			if (intParameter <= 5 && IsMyActivatedAbilityAIUsable(FireBoltActivatedAbilityID))
			{
				E.AddAICommand("CommandFireBolt");
			}
			if (intParameter <= 6 && IsMyActivatedAbilityAIUsable(FireBurstActivatedAbilityID))
			{
				E.AddAICommand("CommandFireBurst");
			}
			if (IsMyActivatedAbilityAIUsable(FireBurstActivatedAbilityID) && !IsMyActivatedAbilityToggledOn(FireBurstActivatedAbilityID))
			{
				E.AddAICommand("CommandFlameArmor");
			}
			if (IsMyActivatedAbilityAIUsable(FlameAuraActivatedAbilityID) && !IsMyActivatedAbilityToggledOn(FlameAuraActivatedAbilityID))
			{
				E.AddAICommand("CommandFlameAura");
			}
		}
		else if (E.ID == "CommandHeat")
		{
			Cell cell = PickDirection();
			if (cell != null)
			{
				if (base.Level < 6)
				{
					CooldownMyActivatedAbility(HeatActivatedAbilityID, 20 - base.Level);
				}
				UseEnergy(1000);
				Heat(cell);
			}
		}
		if (E.ID == "CommandFireBolt")
		{
			List<Cell> list = PickLine(5, AllowVis.Any);
			if (list == null)
			{
				return true;
			}
			if (base.Level < 7)
			{
				CooldownMyActivatedAbility(FireBoltActivatedAbilityID, 20 - base.Level);
			}
			UseEnergy(1000);
			int num = 0;
			foreach (Cell item in list)
			{
				if (item != ParentObject.CurrentCell)
				{
					Heat(item);
					num++;
					if (num >= 5)
					{
						break;
					}
				}
			}
		}
		else if (E.ID == "CommandFireBurst")
		{
			List<Cell> list2 = PickBurst(1, 5, bLocked: false, AllowVis.OnlyVisible);
			if (list2 == null)
			{
				return true;
			}
			if (base.Level < 7)
			{
				CooldownMyActivatedAbility(FireBurstActivatedAbilityID, 20 - base.Level);
			}
			UseEnergy(1000);
			foreach (Cell item2 in list2)
			{
				if (item2 != ParentObject.CurrentCell)
				{
					Heat(item2);
				}
			}
		}
		else if (E.ID == "CommandFireField")
		{
			List<Cell> list3 = PickField(9);
			if (list3 == null || list3.Count == 0)
			{
				return true;
			}
			if (base.Level < 8)
			{
				CooldownMyActivatedAbility(FireFieldActivatedAbilityID, 20 - base.Level);
			}
			UseEnergy(1000);
			foreach (Cell item3 in list3)
			{
				Heat(item3);
			}
		}
		else if (E.ID == "CommandFlameArmor")
		{
			ToggleMyActivatedAbility(FlameArmorActivatedAbilityID);
			if (IsMyActivatedAbilityToggledOn(FlameArmorActivatedAbilityID))
			{
				ParentObject.Statistics["AV"].Bonus += 2;
			}
			else
			{
				ParentObject.Statistics["AV"].Bonus -= 2;
			}
			UseEnergy(1000);
		}
		else if (E.ID == "CommandFlameAura")
		{
			ToggleMyActivatedAbility(FlameAuraActivatedAbilityID);
			UseEnergy(1000);
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
			OldFlame = GO.pPhysics.FlameTemperature;
			OldVapor = GO.pPhysics.VaporTemperature;
			GO.pPhysics.FlameTemperature = 2500 + 250 * Level;
			GO.pPhysics.VaporTemperature = 4000 + 400 * Level;
		}
		if (Level >= 1)
		{
			HeatActivatedAbilityID = AddMyActivatedAbility("Heat", "CommandHeat", "Mutation");
		}
		if (Level >= 2)
		{
			FireBoltActivatedAbilityID = AddMyActivatedAbility("Fire Bolt", "CommandFireBolt", "Mutation");
			FireBurstActivatedAbilityID = AddMyActivatedAbility("Fire Burst", "CommandFireBurst", "Mutation");
		}
		if (Level >= 3)
		{
			FlameArmorActivatedAbilityID = AddMyActivatedAbility("Flame Armor", "CommandFlameArmor", "Mutation", null, "\a", null, Toggleable: true);
		}
		if (Level >= 4)
		{
			FireFieldActivatedAbilityID = AddMyActivatedAbility("Fire Field", "CommandFireField", "Mutation");
		}
		if (Level >= 5)
		{
			FlameAuraActivatedAbilityID = AddMyActivatedAbility("Flame Aura", "CommandFlameAura", "Mutation", null, "\a", null, Toggleable: true);
		}
		return base.Mutate(GO, Level);
	}

	public override bool Unmutate(GameObject GO)
	{
		if (GO.pPhysics != null)
		{
			if (OldFlame != -1)
			{
				GO.pPhysics.FlameTemperature = OldFlame;
			}
			if (OldVapor != -1)
			{
				GO.pPhysics.BrittleTemperature = OldVapor;
			}
			OldFlame = -1;
			OldVapor = -1;
			GO.pPhysics.Temperature = 25;
		}
		RemoveMyActivatedAbility(ref HeatActivatedAbilityID);
		RemoveMyActivatedAbility(ref FireBoltActivatedAbilityID);
		RemoveMyActivatedAbility(ref FireBurstActivatedAbilityID);
		RemoveMyActivatedAbility(ref FlameArmorActivatedAbilityID);
		RemoveMyActivatedAbility(ref FlameAuraActivatedAbilityID);
		return base.Unmutate(GO);
	}
}
