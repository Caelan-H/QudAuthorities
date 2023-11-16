using System;
using System.Collections.Generic;
using System.Linq;
using HistoryKit;
using Qud.API;
using XRL.Core;
using XRL.Language;
using XRL.Rules;
using XRL.UI;
using XRL.World.Skills.Cooking;

namespace XRL.World.Parts;

[Serializable]
public class Cookbook : IPart
{
	public int Tier = 1;

	public string NumberOfIngredients = "2-4";

	public Guid id;

	public string Style = "Generic";

	public string ChefName;

	public List<CookingRecipe> recipes = new List<CookingRecipe>();

	public List<bool> readPage = new List<bool>();

	[NonSerialized]
	public List<string> bookText;

	public Cookbook()
	{
	}

	public Cookbook(string Style, string NumberOfIngredients, int Tier, string ChefName)
		: this()
	{
		this.Style = Style;
		this.NumberOfIngredients = NumberOfIngredients;
		this.Tier = Tier;
		this.ChefName = ChefName;
	}

	public override bool SameAs(IPart p)
	{
		return false;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != GetInventoryActionsEvent.ID && ID != InventoryActionEvent.ID)
		{
			return ID == ObjectCreatedEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetInventoryActionsEvent E)
	{
		E.AddAction("Read", "read", "Read", null, 'r', FireOnActor: false, (!GetHasBeenRead()) ? 100 : 0);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(InventoryActionEvent E)
	{
		if (E.Command == "Read")
		{
			if (!E.Actor.IsPlayer())
			{
				return false;
			}
			if (bookText == null)
			{
				bookText = new List<string>();
				foreach (CookingRecipe recipe in recipes)
				{
					bookText.Add(recipe.GetDisplayName() + "\n\n" + recipe.GetIngredients() + "\n\n" + recipe.GetDescription());
				}
			}
			List<Action> afterClosed = new List<Action>();
			BookUI.ShowBook(bookText, ParentObject.DisplayName, delegate(int p)
			{
				if (!readPage[p])
				{
					readPage[p] = true;
					afterClosed.Add(delegate
					{
						CookingGamestate.LearnRecipe(recipes[p]);
					});
				}
			});
			foreach (Action item in afterClosed)
			{
				item();
			}
			if (!GetHasBeenRead())
			{
				SetHasBeenRead(flag: true);
				JournalAPI.AddAccomplishment("You read " + ParentObject.DisplayName + ".", "In the month of " + Calendar.getMonth() + " of " + Calendar.getYear() + ", =name= penned the influential cookbook, " + ParentObject.a + ParentObject.pRender.DisplayName + ".", "general", JournalAccomplishment.MuralCategory.CreatesSomething, JournalAccomplishment.MuralWeight.VeryLow, null, -1L);
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(ObjectCreatedEvent E)
	{
		GenerateCookbook();
		return base.HandleEvent(E);
	}

	public void GenerateCookbook()
	{
		if (Tier == -1)
		{
			Tier = 0;
			Tier = ZoneManager.zoneGenerationContextTier;
		}
		id = Guid.NewGuid();
		if (Style == "Generic")
		{
			ParentObject.pRender.DisplayName = HistoricStringExpander.ExpandString("<spice.cooking.cookbooks.genericName.!random>");
		}
		if (Style == "Generic_LegendaryChef")
		{
			ParentObject.pRender.DisplayName = "Chef " + Grammar.MakePossessive(ChefName.Split(',')[0]) + " " + HistoricStringExpander.ExpandString("<spice.cooking.cookbooks.genericName.!random>");
		}
		if (Style == "Focal")
		{
			ParentObject.pRender.DisplayName = HistoricStringExpander.ExpandString("<spice.cooking.cookbooks.focalName.!random>");
		}
		int num = Stat.Roll(NumberOfIngredients);
		GameObject gameObject = null;
		if (Style == "Focal")
		{
			string newValue = "";
			gameObject = GameObjectFactory.Factory.CreateObject(CookingRecipe.RollOvenSafeIngredient("Ingredients" + Tier));
			if (gameObject.HasPart("LiquidVolume"))
			{
				if (gameObject.LiquidVolume.GetPrimaryLiquid() != null)
				{
					newValue = gameObject.LiquidVolume.GetPreparedCookingIngredientLiquidDomainPairs().Split(',').GetRandomElement()
						.Split(':')[0];
				}
			}
			else
			{
				newValue = gameObject.DisplayNameOnlyStripped;
			}
			if (!gameObject.HasTag("Preservable") && !gameObject.HasPart("LiquidVolume"))
			{
				ParentObject.pRender.DisplayName = Grammar.Pluralize(ParentObject.pRender.DisplayName.Replace("$focus", newValue));
			}
			else
			{
				ParentObject.pRender.DisplayName = ParentObject.pRender.DisplayName.Replace("$focus", newValue);
			}
		}
		ParentObject.pRender.DisplayName = "&g" + Grammar.MakeTitleCase(ParentObject.pRender.DisplayName.Replace("$markovTitle", GameObjectFactory.Factory.CreateObject("StandaloneMarkovBook").DisplayNameOnlyDirect));
		for (int i = 0; i < num; i++)
		{
			List<GameObject> list = new List<GameObject>();
			int num2 = 0;
			int num3 = 25;
			int num4 = 50;
			if (Tier >= 0 && Tier <= 1)
			{
				num3 = 25;
				num4 = 85;
			}
			if (Tier >= 2 && Tier <= 3)
			{
				num3 = 15;
				num4 = 65;
			}
			if (Tier >= 4 && Tier <= 5)
			{
				num3 = 5;
				num4 = 52;
			}
			if (Tier >= 6 && Tier <= 7)
			{
				num3 = 0;
				num4 = 35;
			}
			if (Tier >= 8)
			{
				num3 = 0;
				num4 = 25;
			}
			int num5 = Stat.Random(1, 100);
			num2 = ((num5 <= num3) ? 1 : ((num5 > num4) ? 3 : 2));
			Commerce part = ParentObject.GetPart<Commerce>();
			if (part != null)
			{
				part.Value = 50 + 10 * Tier;
			}
			if (Style == "Focal")
			{
				list.Add(gameObject);
				num2--;
			}
			int num6 = 0;
			for (int j = 0; j < num2; j++)
			{
				string ingredientBlueprint = CookingRecipe.RollOvenSafeIngredient("Ingredients" + Tier);
				if (num6 < 50 && list.Any((GameObject o) => o.Blueprint == ingredientBlueprint))
				{
					j--;
				}
				else
				{
					list.Add(GameObject.create(ingredientBlueprint));
				}
				num6++;
			}
			CookingRecipe item = ((ChefName != null) ? CookingRecipe.FromIngredients(list, null, "Chef " + ChefName.Split(',')[0]) : CookingRecipe.FromIngredients(list));
			recipes.Add(item);
			readPage.Add(item: false);
		}
	}

	public string GetBookKey()
	{
		Guid guid = id;
		return "AlreadyRead_" + guid.ToString();
	}

	public bool GetHasBeenRead()
	{
		return XRLCore.Core.Game.HasStringGameState(GetBookKey());
	}

	public void SetHasBeenRead(bool flag)
	{
		if (flag)
		{
			XRLCore.Core.Game.SetStringGameState(GetBookKey(), "Yes");
		}
		else
		{
			XRLCore.Core.Game.SetStringGameState(GetBookKey(), "");
		}
	}
}
