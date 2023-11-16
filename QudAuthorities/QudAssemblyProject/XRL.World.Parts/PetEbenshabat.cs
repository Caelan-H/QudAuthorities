using System;
using System.Collections.Generic;
using XRL.Rules;
using XRL.UI;
using XRL.World.Skills.Cooking;

namespace XRL.World.Parts;

[Serializable]
public class PetEbenshabat : IPart
{
	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "PlayerGainedLevel");
		base.Register(Object);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "PlayerGainedLevel" && ParentObject.PartyLeader != null && ParentObject.PartyLeader.IsPlayer() && Stat.Random(1, 100) <= 40)
		{
			int tier = ParentObject.PartyLeader.GetTier();
			List<string> list = new List<string>();
			list.Add("Starapple Preserves");
			string text;
			do
			{
				text = CookingRecipe.RollOvenSafeIngredient("Ingredients" + tier);
			}
			while (text == "Starapple Preserves");
			list.Add(text);
			if (Stat.Random(1, 100) <= 50)
			{
				string text2;
				do
				{
					text2 = CookingRecipe.RollOvenSafeIngredient("Ingredients" + tier);
				}
				while (text2 == "Starapple Preserves" || text2 == text);
				list.Add(text2);
			}
			Popup.Show(ParentObject.DisplayName + "&y teaches you " + CookingGamestate.LearnRecipe(CookingRecipe.FromIngredients(list, null, ParentObject.DisplayNameOnlyDirectAndStripped)).GetDisplayName() + "&y!");
		}
		return true;
	}
}
