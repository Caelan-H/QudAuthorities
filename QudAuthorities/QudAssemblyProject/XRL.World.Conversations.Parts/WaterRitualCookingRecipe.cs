using System;
using Qud.API;
using XRL.UI;
using XRL.World.Parts;
using XRL.World.Skills.Cooking;

namespace XRL.World.Conversations.Parts;

public class WaterRitualCookingRecipe : IWaterRitualPart
{
	public CookingRecipe Recipe;

	public string Text;

	public bool Truekin = true;

	public bool Chef;

	public override bool WantEvent(int ID, int Propagation)
	{
		if (!base.WantEvent(ID, Propagation) && ID != PrepareTextEvent.ID && ID != GetChoiceTagEvent.ID)
		{
			return ID == EnteredElementEvent.ID;
		}
		return true;
	}

	public Type GetRecipeType(string Name)
	{
		return ModManager.ResolveType("XRL.World.Skills.Cooking." + Name);
	}

	public void GetSpecificRecipe()
	{
		string propertyOrTag = The.Speaker.GetPropertyOrTag("SharesRecipe");
		if (!propertyOrTag.IsNullOrEmpty())
		{
			Truekin = The.Speaker.GetPropertyOrTag("SharesRecipeWithTrueKin") != "false";
			Recipe = Activator.CreateInstance(GetRecipeType(propertyOrTag)) as CookingRecipe;
			Text = The.Speaker.GetPropertyOrTag("SharesRecipeText");
		}
		else if (The.Speaker.GetPart("TeachesDish") is TeachesDish teachesDish)
		{
			Recipe = teachesDish.Recipe;
			Text = teachesDish.Text;
		}
		else if (!WaterRitual.RecordFaction.WaterRitualRecipe.IsNullOrEmpty())
		{
			Recipe = Activator.CreateInstance(GetRecipeType(WaterRitual.RecordFaction.WaterRitualRecipe)) as CookingRecipe;
			Text = WaterRitual.RecordFaction.WaterRitualRecipeText;
		}
	}

	public void GetChefRecipe()
	{
		Chef part = The.Speaker.GetPart<Chef>();
		if (part != null)
		{
			Random r = new Random(WaterRitual.Record.mySeed);
			Recipe = part.signatureDishes.GetRandomElement(r);
		}
	}

	public override void Awake()
	{
		if (!(The.Speaker.GetxTag("WaterRitual", "SellCookingRecipe") == "false"))
		{
			if (Chef)
			{
				GetChefRecipe();
			}
			else
			{
				GetSpecificRecipe();
			}
			if (Recipe != null && !CookingGamestate.KnowsRecipe(Recipe))
			{
				Reputation = GetWaterRitualCostEvent.GetFor(The.Player, The.Speaker, "CookingRecipe", 50);
				Visible = true;
			}
		}
	}

	public override bool HandleEvent(PrepareTextEvent E)
	{
		if (!Text.IsNullOrEmpty())
		{
			E.Text.Clear();
			E.Text.Append(Text);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EnteredElementEvent E)
	{
		if (!Truekin && The.Player.IsTrueKin())
		{
			Popup.ShowFail("True kin cannot digest this meal.");
		}
		else if (UseReputation())
		{
			Popup.Show(The.Speaker.Does("share") + " the recipe for {{W|" + Recipe.GetDisplayName() + "}}!");
			JournalAPI.AddRecipeNote(Recipe, revealed: true, silent: true);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetChoiceTagEvent E)
	{
		E.Tag = "{{" + Lowlight + "|[learn to cook {{W|" + Recipe.GetDisplayName() + "}}: {{" + Numeric + "|-" + Reputation + "}} reputation]}}";
		return false;
	}
}
