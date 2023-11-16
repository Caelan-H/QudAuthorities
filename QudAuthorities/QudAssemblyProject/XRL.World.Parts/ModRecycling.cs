using System;
using XRL.Language;
using XRL.Rules;

namespace XRL.World.Parts;

[Serializable]
public class ModRecycling : IModification
{
	public ModRecycling()
	{
	}

	public ModRecycling(int Tier)
		: base(Tier)
	{
	}

	public override void Configure()
	{
		WorksOnSelf = true;
	}

	public override bool ModificationApplicable(GameObject Object)
	{
		if (Object.HasPart("LiquidProducer"))
		{
			return false;
		}
		if (!(Object.GetPart("Armor") is Armor armor))
		{
			return false;
		}
		if (armor.WornOn != "Body" && armor.WornOn != "Back" && armor.WornOn != "*")
		{
			return false;
		}
		string usesSlots = Object.UsesSlots;
		if (!string.IsNullOrEmpty(usesSlots) && !usesSlots.Contains("Body") && !usesSlots.Contains("Back"))
		{
			return false;
		}
		return true;
	}

	public override void ApplyModification(GameObject Object)
	{
		if (!Object.HasPart("LiquidVolume"))
		{
			LiquidVolume liquidVolume = new LiquidVolume();
			Object.AddPart(liquidVolume);
			liquidVolume.MaxVolume = 8;
			liquidVolume.Volume = Stat.Random(0, liquidVolume.MaxVolume);
			liquidVolume.ComponentLiquids.Clear();
			if (liquidVolume.Volume > 0)
			{
				liquidVolume.ComponentLiquids.Add("water", 1000);
			}
			liquidVolume.Update();
		}
		if (!Object.HasPart("LiquidProducer"))
		{
			LiquidProducer liquidProducer = new LiquidProducer();
			liquidProducer.Liquid = "water";
			liquidProducer.Rate = 10000;
			liquidProducer.WorksOnSelf = false;
			liquidProducer.WorksOnEquipper = true;
			Object.AddPart(liquidProducer);
		}
		IncreaseDifficultyIfComplex(1);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != GetDisplayNameEvent.ID && ID != GetItemElementsEvent.ID)
		{
			return ID == GetShortDescriptionEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetDisplayNameEvent E)
	{
		if (E.Understood() && !E.Object.HasProperName)
		{
			E.AddAdjective("{{B|recycling}}");
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetShortDescriptionEvent E)
	{
		E.Postfix.Append("\n{{rules|Recycling: This item collects");
		LiquidVolume liquidVolume = ParentObject.LiquidVolume;
		if (liquidVolume != null)
		{
			E.Postfix.Append(", purifies, and stores up to ").Append(Grammar.Cardinal(liquidVolume.MaxVolume)).Append(' ')
				.Append((liquidVolume.MaxVolume == 1) ? "dram" : "drams")
				.Append(" of the wearer's wastewater.");
		}
		else
		{
			E.Postfix.Append(" and purifies the wearer's wastewater.");
		}
		AddStatusSummary(E.Postfix);
		E.Postfix.Append("}}");
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetItemElementsEvent E)
	{
		E.Add("travel", 1);
		E.Add("salt", 3);
		return base.HandleEvent(E);
	}
}
