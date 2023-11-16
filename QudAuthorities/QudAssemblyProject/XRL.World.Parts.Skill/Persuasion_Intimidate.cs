using System;
using System.Threading;
using ConsoleLib.Console;
using XRL.Core;
using XRL.UI;
using XRL.World.Capabilities;
using XRL.World.Effects;

namespace XRL.World.Parts.Skill;

[Serializable]
public class Persuasion_Intimidate : BaseSkill
{
	public Guid ActivatedAbilityID = Guid.Empty;

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "CommandIntimidate");
		base.Register(Object);
	}

	public static void ApplyIntimidate(Cell cell, GameObject ParentObject, bool bEnergyFree = false)
	{
		foreach (Cell localAdjacentCell in cell.GetLocalAdjacentCells())
		{
			TextConsole textConsole = Look._TextConsole;
			ScreenBuffer scrapBuffer = TextConsole.ScrapBuffer;
			XRLCore.Core.RenderMapToBuffer(scrapBuffer);
			if (localAdjacentCell != null)
			{
				foreach (GameObject item in localAdjacentCell.GetObjectsInCell())
				{
					if (item.IsHostileTowards(ParentObject))
					{
						if (!bEnergyFree)
						{
							ParentObject.UseEnergy(1000, "Persuasion Skill Intimidate");
							bEnergyFree = true;
						}
						int attackModifier = ParentObject.StatMod("Ego") + ParentObject.GetIntProperty("Persuasion_Intimidate") * 2;
						Mental.PerformAttack(Terrified.OfAttacker, ParentObject, item, null, "Terrify Intimidate", "1d8", 8388610, "6d4".RollCached(), int.MinValue, attackModifier);
					}
				}
			}
			if (localAdjacentCell.IsVisible())
			{
				scrapBuffer.WriteAt(localAdjacentCell, "&Y#");
				textConsole.DrawBuffer(scrapBuffer);
				Thread.Sleep(50);
			}
		}
	}

	public static bool Terrify(MentalAttackEvent E)
	{
		if (E.Penetrations <= 0 || !E.Defender.ApplyEffect(new Terrified("6d4".RollCached(), E.Attacker)))
		{
			IComponent<GameObject>.XDidY(E.Defender, "resist", "becoming afraid", null, null, E.Defender);
			return false;
		}
		return true;
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "CommandIntimidate")
		{
			int num = 50;
			if (ParentObject.GetIntProperty("Horrifying") > 0)
			{
				num -= 10;
			}
			CooldownMyActivatedAbility(ActivatedAbilityID, num);
			ApplyIntimidate(ParentObject.GetCurrentCell(), ParentObject);
		}
		return base.FireEvent(E);
	}

	public override bool AddSkill(GameObject GO)
	{
		ActivatedAbilityID = AddMyActivatedAbility("Intimidate", "CommandIntimidate", "Skill", null, "\u0005");
		return base.AddSkill(GO);
	}

	public override bool RemoveSkill(GameObject GO)
	{
		RemoveMyActivatedAbility(ref ActivatedAbilityID);
		return base.RemoveSkill(GO);
	}
}
