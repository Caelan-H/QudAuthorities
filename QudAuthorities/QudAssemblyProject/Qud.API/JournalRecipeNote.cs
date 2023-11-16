using System;
using XRL.World.Skills.Cooking;

namespace Qud.API;

[Serializable]
public class JournalRecipeNote : IBaseJournalEntry
{
	public CookingRecipe recipe;

	public override void Reveal(bool silent = false)
	{
		if (!base.revealed)
		{
			base.Reveal();
			CookingGamestate.instance.knownRecipies.Add(recipe);
			if (!silent)
			{
				IBaseJournalEntry.DisplayMessage("You learn to cook " + recipe.GetDisplayName() + "!");
			}
		}
	}
}
