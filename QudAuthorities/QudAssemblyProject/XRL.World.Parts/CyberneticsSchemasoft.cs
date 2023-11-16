using System;
using System.Collections.Generic;
using XRL.Language;
using XRL.World.Tinkering;

namespace XRL.World.Parts;

[Serializable]
public class CyberneticsSchemasoft : IPart
{
	public int MaxTier = 3;

	public string RecipiesAdded;

	public string Category;

	public override bool SameAs(IPart p)
	{
		return false;
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != ImplantedEvent.ID && ID != ObjectCreatedEvent.ID)
		{
			return ID == UnimplantedEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(ImplantedEvent E)
	{
		GameObject implantee = E.Implantee;
		if (implantee != null && implantee.IsPlayer())
		{
			RecipiesAdded = "";
			foreach (TinkerData tinkerRecipe in TinkerData.TinkerRecipes)
			{
				if (tinkerRecipe.Category == Category && tinkerRecipe.Tier <= MaxTier && !TinkerData.KnownRecipes.Contains(tinkerRecipe))
				{
					if (RecipiesAdded != "")
					{
						RecipiesAdded += ",";
					}
					RecipiesAdded += tinkerRecipe.Blueprint;
					TinkerData.KnownRecipes.Add(tinkerRecipe);
				}
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(UnimplantedEvent E)
	{
		if (!string.IsNullOrEmpty(RecipiesAdded))
		{
			string[] array = RecipiesAdded.Split(',');
			foreach (string text in array)
			{
				if (!string.IsNullOrEmpty(text))
				{
					TinkerData.UnlearnRecipe(text);
				}
			}
			RecipiesAdded = "";
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(ObjectCreatedEvent E)
	{
		if (Category == null)
		{
			List<string> list = new List<string>(new string[8] { "ammo and energy cells", "pistols", "rifles", "melee weapons", "grenades", "tonics", "utility", "armor" });
			Category = list.GetRandomElement();
			string text = "Low Tier";
			if (MaxTier >= 6)
			{
				text = "High Tier";
			}
			else if (MaxTier >= 4)
			{
				text = "Mid Tier";
			}
			ParentObject.pRender.DisplayName = "{{Y|Schemasoft [{{C|" + Grammar.MakeTitleCase(Category) + ", " + text + "}}]}}";
		}
		return base.HandleEvent(E);
	}
}
