using System;
using System.Text;

namespace XRL.World.Parts.Mutation;

[Serializable]
public class IrritableGenome : BaseMutation
{
	[NonSerialized]
	private bool insidetry;

	public int MPSpentMemory;

	public IrritableGenome()
	{
		DisplayName = "Irritable Genome ({{r|D}})";
	}

	public override bool CanLevel()
	{
		return false;
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "GainedMP");
		Object.RegisterPartEvent(this, "UsedMP");
		base.Register(Object);
	}

	public override string GetDescription()
	{
		return "Your genome is irritable and unpredictable.\n\nWhenever you spend a mutation point, the next mutation point you gain will be spent randomly.\nWhenever you buy a new mutation, you get a random one instead of a choice of three.";
	}

	public override string GetLevelText(int Level)
	{
		return "";
	}

	public void TrySpending()
	{
		if (MPSpentMemory <= 0)
		{
			return;
		}
		insidetry = true;
		try
		{
			int num = ParentObject.Stat("MP");
			int maxMPtospend = Math.Min(num, MPSpentMemory);
			StringBuilder stringBuilder = Event.NewStringBuilder();
			if (ParentObject.IsPlayer())
			{
				stringBuilder = new StringBuilder();
			}
			ParentObject.RandomlySpendPoints(0, 0, maxMPtospend, stringBuilder);
			int num2 = num - ParentObject.Stat("MP");
			if (num2 > 0)
			{
				IComponent<GameObject>.EmitMessage(ParentObject, ParentObject.Poss("irritable genome acts up." + stringBuilder.ToString()), FromDialog: true);
				MPSpentMemory -= num2;
			}
		}
		catch (Exception x)
		{
			MetricsManager.LogException("IrritableGenome::GainedMP", x);
		}
		insidetry = false;
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "UsedMP" && !insidetry && E.GetStringParameter("Context") != "BuyNew")
		{
			MPSpentMemory += E.GetIntParameter("Amount");
			TrySpending();
		}
		if (E.ID == "GainedMP")
		{
			TrySpending();
		}
		return true;
	}

	public override bool ChangeLevel(int NewLevel)
	{
		return base.ChangeLevel(NewLevel);
	}
}
