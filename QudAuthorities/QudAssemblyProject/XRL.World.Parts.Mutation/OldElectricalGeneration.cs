using System;
using System.Collections.Generic;
using System.Threading;
using ConsoleLib.Console;
using XRL.Core;
using XRL.Rules;
using XRL.UI;
using XRL.World.Effects;

namespace XRL.World.Parts.Mutation;

[Serializable]
public class OldElectricalGeneration : BaseMutation
{
	public int nCharges = 5;

	public int nTurnCounter;

	public Guid DischargeActivatedAbilityID = Guid.Empty;

	public Guid SynapseSnapActivatedAbilityID = Guid.Empty;

	public Guid ElectromagneticPulseActivatedAbilityID = Guid.Empty;

	public Guid ElectricalShieldActivatedAbilityID = Guid.Empty;

	public Guid ShockActivatedAbilityID = Guid.Empty;

	public Guid ArcLightningActivatedAbilityID = Guid.Empty;

	public int OldConductivity;

	public OldElectricalGeneration()
	{
		DisplayName = "Electrified Carapace";
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "AIGetOffensiveMutationList");
		Object.RegisterPartEvent(this, "CommandArcLightning");
		Object.RegisterPartEvent(this, "CommandDischarge");
		Object.RegisterPartEvent(this, "CommandElectricalShield");
		Object.RegisterPartEvent(this, "CommandElectromagneticPulse");
		Object.RegisterPartEvent(this, "CommandShock");
		Object.RegisterPartEvent(this, "CommandSynapseSnap");
		Object.RegisterPartEvent(this, "DefendMeleeHit");
		Object.RegisterPartEvent(this, "EndTurn");
		base.Register(Object);
	}

	public void Discharge(Cell C, int Voltage)
	{
		string damage = ((base.Level < 3) ? (nCharges + "d4") : (nCharges + "d4+1"));
		nCharges = 0;
		ParentObject.Discharge(C, Voltage, damage, ParentObject);
	}

	public override bool Render(RenderEvent E)
	{
		if (IsMyActivatedAbilityToggledOn(ElectricalShieldActivatedAbilityID))
		{
			int num = Stat.RandomCosmetic(1, 15) + XRLCore.CurrentFrame;
			if (num < 15)
			{
				E.ColorString = "&W";
			}
			else if (num < 30)
			{
				E.ColorString = "&Y";
			}
			else if (num < 45)
			{
				E.ColorString = "&W";
			}
			else
			{
				E.ColorString = "&Y";
			}
		}
		return true;
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "EndTurn")
		{
			nTurnCounter++;
			int num = 40 - base.Level;
			if (base.Level >= 4)
			{
				num = 35 - base.Level;
			}
			if (base.Level >= 5)
			{
				num = 30 - base.Level;
			}
			if (nTurnCounter >= num)
			{
				nTurnCounter = 0;
				int num2 = 5;
				if (base.Level >= 3)
				{
					num2 = 10;
				}
				if (base.Level >= 4)
				{
					num2 = 20;
				}
				if (nCharges < num2)
				{
					nCharges++;
				}
			}
			ActivatedAbilityEntry activatedAbilityEntry = MyActivatedAbility(DischargeActivatedAbilityID);
			if (activatedAbilityEntry != null)
			{
				activatedAbilityEntry.DisplayName = "Discharge [" + nCharges + " charges]";
			}
			if (IsMyActivatedAbilityAIUsable(ArcLightningActivatedAbilityID))
			{
				ParentObject.pPhysics.Temperature = 2500;
			}
		}
		else if (E.ID == "AIGetOffensiveMutationList")
		{
			int intParameter = E.GetIntParameter("Distance");
			if (nCharges > 0 && intParameter <= 1 && IsMyActivatedAbilityAIUsable(DischargeActivatedAbilityID))
			{
				E.AddAICommand("CommandDischarge");
			}
			if (intParameter <= 5 && IsMyActivatedAbilityAIUsable(SynapseSnapActivatedAbilityID))
			{
				E.AddAICommand("CommandSynapseSnap");
			}
			if (!IsMyActivatedAbilityToggledOn(ElectricalShieldActivatedAbilityID))
			{
				E.AddAICommand("CommandElectricalShield");
			}
			if (nCharges >= 5 && IsMyActivatedAbilityAIUsable(ShockActivatedAbilityID))
			{
				E.AddAICommand("CommandShock");
			}
			if (nCharges >= 15 && !IsMyActivatedAbilityToggledOn(ArcLightningActivatedAbilityID))
			{
				E.AddAICommand("CommandArcLightning");
			}
		}
		else if (E.ID == "DefendMeleeHit")
		{
			if (nCharges > 0 && IsMyActivatedAbilityToggledOn(ElectricalShieldActivatedAbilityID))
			{
				GameObject gameObjectParameter = E.GetGameObjectParameter("Attacker");
				if (ParentObject.IsPlayer())
				{
					IComponent<GameObject>.AddPlayerMessage("Your Electrical Shield discharges into " + gameObjectParameter.the + gameObjectParameter.DisplayNameOnly + "&y!");
				}
				Discharge(gameObjectParameter.CurrentCell, 1);
			}
		}
		else if (E.ID == "CommandDischarge")
		{
			if (nCharges == 0)
			{
				return false;
			}
			Cell cell = PickDirection();
			if (cell == null)
			{
				return false;
			}
			if (base.Level < 6)
			{
				CooldownMyActivatedAbility(DischargeActivatedAbilityID, 20 - base.Level);
			}
			UseEnergy(1000, "Physical Mutation Old Electrical Generation Discharge");
			Discharge(cell, 1);
		}
		else if (E.ID == "CommandSynapseSnap")
		{
			CooldownMyActivatedAbility(SynapseSnapActivatedAbilityID, 200);
			ParentObject.ApplyEffect(new SynapseSnap(50));
		}
		else if (E.ID == "CommandElectromagneticPulse")
		{
			if (nCharges < 20)
			{
				if (ParentObject.IsPlayer())
				{
					Popup.ShowFail("You must have 20 charges to use Electromagnetic Pulse.");
				}
				return false;
			}
			List<Cell> list = PickBurst(10, 0, bLocked: false, AllowVis.OnlyVisible);
			if (list == null || list.Count <= 0 || list[0] == null)
			{
				return false;
			}
			UseEnergy(1000, "Physical Mutation Old Electrical Generation Electromagnetic Pulse");
			nCharges = 0;
			TextConsole textConsole = Look._TextConsole;
			ScreenBuffer scrapBuffer = TextConsole.ScrapBuffer;
			XRLCore.Core.RenderMapToBuffer(scrapBuffer);
			for (int i = 0; i < 10; i++)
			{
				bool flag = false;
				foreach (Cell item in list)
				{
					if (item.ParentZone == ParentObject.CurrentCell.ParentZone && item != ParentObject.CurrentCell)
					{
						if (item.IsVisible())
						{
							flag = true;
						}
						scrapBuffer.Goto(item.X, item.Y);
						if (Stat.Random(1, 2) == 1)
						{
							scrapBuffer.Write("&W" + (char)Stat.Random(191, 198));
						}
						else
						{
							scrapBuffer.Write("&Y" + (char)Stat.Random(191, 198));
						}
					}
				}
				if (flag)
				{
					textConsole.DrawBuffer(scrapBuffer);
					Thread.Sleep(50);
				}
			}
		}
		else if (E.ID == "CommandShock")
		{
			if (nCharges < 5)
			{
				if (ParentObject.IsPlayer())
				{
					Popup.Show("You must have 5 charges to use Shock.");
				}
				return false;
			}
			Cell cell2 = PickDirection();
			if (cell2 == null)
			{
				return false;
			}
			UseEnergy(1000, "Physical Mutation Old Electrical Generation Sbock");
			Discharge(cell2, 2);
		}
		else if (E.ID == "CommandElectricalShield")
		{
			ToggleMyActivatedAbility(ElectricalShieldActivatedAbilityID);
			UseEnergy(1000);
		}
		else if (E.ID == "CommandArcLightning")
		{
			if (nCharges < 15)
			{
				if (ParentObject.IsPlayer())
				{
					Popup.Show("You must have 15 charges to use Arc Lightning.");
				}
				return false;
			}
			List<Cell> list2 = PickLine(80, AllowVis.Any);
			if (list2 == null || list2.Count == 0)
			{
				return false;
			}
			if (base.Level < 8)
			{
				CooldownMyActivatedAbility(ShockActivatedAbilityID, 20 - base.Level);
			}
			UseEnergy(1000, "Physical Mutation Old Electrical Generation Arc Lightning");
			Discharge(list2[list2.Count - 1], base.Level);
		}
		return base.FireEvent(E);
	}

	public override bool ChangeLevel(int NewLevel)
	{
		return base.ChangeLevel(NewLevel);
	}

	public override bool Mutate(GameObject GO, int Level)
	{
		nCharges = 5 * (Level - 1);
		if (GO.pPhysics != null)
		{
			OldConductivity = GO.pPhysics.Conductivity;
			GO.pPhysics.Conductivity = 0;
		}
		if (Level >= 1)
		{
			DischargeActivatedAbilityID = AddMyActivatedAbility("Discharge", "CommandDischarge", "Mutation");
			ElectricalShieldActivatedAbilityID = AddMyActivatedAbility("Electrical Shield", "CommandElectricalShield", "Mutation", null, "\a", null, Toggleable: true);
		}
		if (Level >= 2)
		{
			SynapseSnapActivatedAbilityID = AddMyActivatedAbility("Synapse Snap", "CommandSynapseSnap", "Mutation");
		}
		if (Level >= 3)
		{
			ShockActivatedAbilityID = AddMyActivatedAbility("Shock", "CommandShock", "Mutation");
		}
		if (Level >= 4)
		{
			ElectromagneticPulseActivatedAbilityID = AddMyActivatedAbility("Electromagnetc Pulse", "CommandElectromagneticPulse", "Mutation");
		}
		if (Level >= 5)
		{
			ArcLightningActivatedAbilityID = AddMyActivatedAbility("Arc Lightning", "CommandArcLightning", "Mutation");
		}
		return base.Mutate(GO, Level);
	}

	public override bool Unmutate(GameObject GO)
	{
		if (GO.pPhysics != null)
		{
			GO.pPhysics.Conductivity = OldConductivity;
			OldConductivity = 0;
		}
		RemoveMyActivatedAbility(ref DischargeActivatedAbilityID);
		RemoveMyActivatedAbility(ref SynapseSnapActivatedAbilityID);
		RemoveMyActivatedAbility(ref ElectromagneticPulseActivatedAbilityID);
		RemoveMyActivatedAbility(ref ElectricalShieldActivatedAbilityID);
		RemoveMyActivatedAbility(ref ShockActivatedAbilityID);
		RemoveMyActivatedAbility(ref ArcLightningActivatedAbilityID);
		return base.Unmutate(GO);
	}
}
