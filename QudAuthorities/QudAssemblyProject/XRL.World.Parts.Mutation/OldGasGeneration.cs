using System;
using System.Collections.Generic;
using System.Threading;
using ConsoleLib.Console;
using XRL.Core;
using XRL.Rules;
using XRL.UI;

namespace XRL.World.Parts.Mutation;

[Serializable]
public class OldGasGeneration : BaseMutation
{
	public string GasObject = "Gas";

	public Guid SprayActivatedAbilityID = Guid.Empty;

	public Guid GasBoltActivatedAbilityID = Guid.Empty;

	public Guid GasCloudActivatedAbilityID = Guid.Empty;

	public Guid GasArmorActivatedAbilityID = Guid.Empty;

	public Guid GasFieldActivatedAbilityID = Guid.Empty;

	public Guid GasBillowsActivatedAbilityID = Guid.Empty;

	public int BillowsTimer = 20;

	public OldGasGeneration()
	{
		DisplayName = "OldGasGeneration";
	}

	public OldGasGeneration(string GasObject)
		: this()
	{
		this.GasObject = GasObject;
		if (GasObject == "PoisonGas")
		{
			DisplayName = "Poison Gas Generation";
		}
		if (GasObject == "BlindGas")
		{
			DisplayName = "Blinding Gas Generation";
		}
		if (GasObject == "StunGas")
		{
			DisplayName = "Stun Gas Generation";
		}
		if (GasObject == "StinkGas")
		{
			DisplayName = "Foul Odor Gas Generation";
		}
		if (GasObject == "AcidGas")
		{
			DisplayName = "Corrosive Gas Generation";
		}
		if (GasObject == "SleepGas")
		{
			DisplayName = "Sleep Gas Generation";
		}
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "CommandSpray");
		Object.RegisterPartEvent(this, "CommandGasBolt");
		Object.RegisterPartEvent(this, "CommandGasCloud");
		Object.RegisterPartEvent(this, "CommandGasField");
		Object.RegisterPartEvent(this, "CommandGasBillows");
		Object.RegisterPartEvent(this, "EndTurn");
		Object.RegisterPartEvent(this, "AIGetOffensiveMutationList");
		Object.RegisterPartEvent(this, "BeforeApplyDamage");
		Object.RegisterPartEvent(this, "ApplyPoisonGasPoison");
		Object.RegisterPartEvent(this, "ApplyStunGasStun");
		base.Register(Object);
	}

	public void Spray(Cell C)
	{
		TextConsole textConsole = Look._TextConsole;
		ScreenBuffer scrapBuffer = TextConsole.ScrapBuffer;
		XRLCore.Core.RenderMapToBuffer(scrapBuffer);
		if (C != null)
		{
			GameObject gameObject = GameObject.create(GasObject);
			Gas obj = gameObject.GetPart("Gas") as Gas;
			if (GasObject == "PoisonGas")
			{
				(gameObject.GetPart("GasPoison") as GasPoison).GasLevel = base.Level;
			}
			obj.Density = 20 * base.Level;
			C.AddObject(gameObject);
		}
		if (C.IsVisible())
		{
			scrapBuffer.Goto(C.X, C.Y);
			switch (Stat.Random(1, 3))
			{
			case 1:
				scrapBuffer.Write("&G*");
				break;
			case 2:
				scrapBuffer.Write("&G*");
				break;
			default:
				scrapBuffer.Write("&G*");
				break;
			}
			textConsole.DrawBuffer(scrapBuffer);
			Thread.Sleep(25);
		}
	}

	public override bool Render(RenderEvent E)
	{
		if (IsMyActivatedAbilityToggledOn(GasBillowsActivatedAbilityID))
		{
			E.ColorString = "&G";
		}
		return true;
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "EndTurn")
		{
			if (IsMyActivatedAbilityToggledOn(GasBillowsActivatedAbilityID))
			{
				BillowsTimer--;
				if (BillowsTimer < 0)
				{
					ToggleMyActivatedAbility(GasBillowsActivatedAbilityID);
				}
				else
				{
					foreach (Cell adjacentCell in ParentObject.CurrentCell.GetAdjacentCells())
					{
						GameObject gameObject = GameObject.create(GasObject);
						(gameObject.GetPart("Gas") as Gas).Density = base.Level * 20;
						adjacentCell.AddObject(gameObject);
					}
				}
			}
		}
		else if (E.ID == "AIGetOffensiveMutationList")
		{
			int intParameter = E.GetIntParameter("Distance");
			if (intParameter <= 1 && IsMyActivatedAbilityAIUsable(SprayActivatedAbilityID))
			{
				E.AddAICommand("CommandSpray");
			}
			if (intParameter <= 6 && IsMyActivatedAbilityAIUsable(GasCloudActivatedAbilityID))
			{
				E.AddAICommand("CommandGasCloud");
			}
			if (!IsMyActivatedAbilityToggledOn(GasBillowsActivatedAbilityID))
			{
				E.AddAICommand("CommandGasBillows");
			}
		}
		else if (E.ID == "BeforeApplyDamage")
		{
			if (GasObject == "PoisonGas")
			{
				if ((E.GetParameter("Damage") as Damage).HasAttribute("Poison"))
				{
					return false;
				}
			}
			else if (GasObject == "AcidGas" && (E.GetParameter("Damage") as Damage).HasAttribute("Acid"))
			{
				return false;
			}
		}
		else if (E.ID == "ApplyPoisonGasPoison")
		{
			if (GasObject == "PoisonGas")
			{
				return false;
			}
		}
		else if (E.ID == "ApplyStunGasStun")
		{
			if (GasObject == "StunGas")
			{
				return false;
			}
		}
		else if (E.ID == "CommandSpray")
		{
			Cell cell = PickDirection();
			if (cell == null)
			{
				return true;
			}
			if (cell != null)
			{
				if (base.Level < 6)
				{
					CooldownMyActivatedAbility(SprayActivatedAbilityID, 20 - base.Level);
				}
				UseEnergy(1000);
				Spray(cell);
			}
		}
		else if (E.ID == "CommandGasBolt")
		{
			List<Cell> list = PickLine(5, AllowVis.Any);
			if (list == null)
			{
				return true;
			}
			if (base.Level < 7)
			{
				CooldownMyActivatedAbility(GasBoltActivatedAbilityID, 20 - base.Level);
			}
			UseEnergy(1000);
			int num = 0;
			foreach (Cell item in list)
			{
				if (item != (ParentObject.GetPart("Physics") as Physics).CurrentCell)
				{
					Spray(item);
					num++;
					if (num >= 5)
					{
						break;
					}
				}
			}
		}
		else if (E.ID == "CommandGasCloud")
		{
			List<Cell> list2 = PickCloud(1);
			if (list2 == null)
			{
				return true;
			}
			if (base.Level < 7)
			{
				CooldownMyActivatedAbility(GasCloudActivatedAbilityID, 20 - base.Level);
			}
			UseEnergy(1000);
			foreach (Cell item2 in list2)
			{
				if (item2 != ParentObject.CurrentCell)
				{
					Spray(item2);
				}
			}
		}
		else if (E.ID == "CommandGasField")
		{
			List<Cell> list3 = PickField(9);
			if (list3 == null || list3.Count == 0)
			{
				return true;
			}
			if (base.Level < 8)
			{
				CooldownMyActivatedAbility(GasFieldActivatedAbilityID, 20 - base.Level);
			}
			UseEnergy(1000);
			foreach (Cell item3 in list3)
			{
				Spray(item3);
			}
		}
		else if (E.ID == "CommandGasBillows")
		{
			CooldownMyActivatedAbility(GasBillowsActivatedAbilityID, 100 - 2 * base.Level);
			ToggleMyActivatedAbility(GasBillowsActivatedAbilityID);
			if (IsMyActivatedAbilityToggledOn(GasBillowsActivatedAbilityID))
			{
				BillowsTimer = 21;
			}
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
		if (Level >= 1)
		{
			SprayActivatedAbilityID = AddMyActivatedAbility("Spray", "CommandSpray", "Physical Mutation");
		}
		_ = 2;
		if (Level >= 3)
		{
			GasCloudActivatedAbilityID = AddMyActivatedAbility("Gas Cloud", "CommandGasCloud", "Physical Mutation");
		}
		_ = 4;
		if (Level >= 5)
		{
			GasBillowsActivatedAbilityID = AddMyActivatedAbility("Gas Billows", "CommandGasBillows", "Physical Mutation", null, "\a", null, Toggleable: true);
		}
		return base.Mutate(GO, Level);
	}

	public override bool Unmutate(GameObject GO)
	{
		RemoveMyActivatedAbility(ref SprayActivatedAbilityID);
		RemoveMyActivatedAbility(ref GasBoltActivatedAbilityID);
		RemoveMyActivatedAbility(ref GasCloudActivatedAbilityID);
		RemoveMyActivatedAbility(ref GasArmorActivatedAbilityID);
		RemoveMyActivatedAbility(ref GasFieldActivatedAbilityID);
		RemoveMyActivatedAbility(ref GasBillowsActivatedAbilityID);
		return base.Unmutate(GO);
	}
}
