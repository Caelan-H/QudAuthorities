using System;
using System.Collections.Generic;
using System.Linq;
using XRL.Language;
using XRL.World.Parts;

namespace XRL.World.Skills.Cooking;

[Serializable]
public class PreparedCookingRecipieComponentDomain : ICookingRecipeComponent
{
	public string ingredientType;

	public int amount;

	public PreparedCookingRecipieComponentDomain(string ingredientType, int amount = 1)
	{
		this.ingredientType = ingredientType;
		this.amount = amount;
	}

	public string getIngredientId()
	{
		return "prepared-" + ingredientType;
	}

	public bool HasPlants()
	{
		return false;
	}

	public bool HasFungi()
	{
		return false;
	}

	public string getDisplayName()
	{
		string text = ((amount > 1) ? "servings" : "serving");
		if (doesPlayerHaveEnough())
		{
			return "&C" + amount + "&y " + text + " of " + ingredientType;
		}
		return "&r" + amount + "&y " + text + " of " + ingredientType;
	}

	public int PlayerHolding()
	{
		int num = 0;
		foreach (GameObject item in CookingGamestate.GetInventorySnapshot())
		{
			PreparedCookingIngredient part = item.GetPart<PreparedCookingIngredient>();
			if (part != null && part.type == ingredientType)
			{
				num += part.charges;
			}
		}
		return num;
	}

	public bool doesPlayerHaveEnough()
	{
		return amount <= CookingGamestate.GetIngredientQuantity(this);
	}

	public string createPlayerDoesNotHaveEnoughMessage()
	{
		return "You don't have enough " + ingredientType + ".";
	}

	public void use(List<GameObject> used)
	{
		int num = amount;
		Event e = Event.New("UsedAsIngredient", "Actor", The.Player);
		while (true)
		{
			using List<GameObject>.Enumerator enumerator = CookingGamestate.GetInventorySnapshot().GetEnumerator();
			GameObject current;
			PreparedCookingIngredient part;
			do
			{
				if (enumerator.MoveNext())
				{
					current = enumerator.Current;
					part = current.GetPart<PreparedCookingIngredient>();
					continue;
				}
				return;
			}
			while (part == null || !(part.type == ingredientType));
			used.Add(current);
			current.FireEvent(e);
			if (num > part.charges)
			{
				num -= part.charges;
				part.ParentObject.SplitFromStack();
				part.ParentObject.Destroy();
				continue;
			}
			part.ParentObject.SplitFromStack();
			part.charges -= num;
			if (part.charges == 0)
			{
				part.ParentObject.Destroy();
			}
			else
			{
				part.ParentObject.CheckStack();
			}
			num = 0;
			break;
		}
	}
}
[Serializable]
public class PreparedCookingRecipieComponentBlueprint : ICookingRecipeComponent
{
	public string ingredientBlueprint;

	public string ingredientDisplayName;

	public int amount;

	public PreparedCookingRecipieComponentBlueprint(string ingredientType, string displayName = null, int amount = 1)
	{
		ingredientBlueprint = ingredientType;
		if (displayName == null)
		{
			if (ingredientType.Contains('|'))
			{
				ingredientDisplayName = Grammar.MakeOrList(ingredientType.Split('|'));
			}
			else
			{
				ingredientDisplayName = GameObjectFactory.Factory.CreateObject(ingredientBlueprint).DisplayNameOnlyDirect;
			}
		}
		else
		{
			ingredientDisplayName = displayName;
		}
		this.amount = amount;
	}

	public string getIngredientId()
	{
		return "blueprint-" + ingredientBlueprint;
	}

	public bool HasPlants()
	{
		string[] array = ingredientBlueprint.Split('|');
		foreach (string key in array)
		{
			if (GameObjectFactory.Factory.Blueprints[key].HasTag("Plant"))
			{
				return true;
			}
		}
		return false;
	}

	public bool HasFungi()
	{
		string[] array = ingredientBlueprint.Split('|');
		foreach (string key in array)
		{
			if (GameObjectFactory.Factory.Blueprints[key].HasTag("Fungus"))
			{
				return true;
			}
		}
		return false;
	}

	public string getDisplayName()
	{
		string text = ((amount > 1) ? "servings" : "serving");
		if (doesPlayerHaveEnough())
		{
			return "&C" + amount + "&y " + text + " of " + ingredientDisplayName;
		}
		return "&r" + amount + "&y " + text + " of " + ingredientDisplayName;
	}

	public int PlayerHolding()
	{
		int num = 0;
		string[] source = ingredientBlueprint.Split('|');
		foreach (GameObject item in CookingGamestate.GetInventorySnapshot())
		{
			if (source.Contains(item.Blueprint))
			{
				num += item.Count;
			}
		}
		return num;
	}

	public bool doesPlayerHaveEnough()
	{
		return amount <= CookingGamestate.GetIngredientQuantity(this);
	}

	public string createPlayerDoesNotHaveEnoughMessage()
	{
		return "You don't have enough servings of " + ingredientDisplayName + ".";
	}

	public void use(List<GameObject> used)
	{
		int num = amount;
		while (true)
		{
			IL_0007:
			string[] source = ingredientBlueprint.Split('|');
			foreach (GameObject item in CookingGamestate.GetInventorySnapshot())
			{
				if (!source.Contains(item.Blueprint))
				{
					continue;
				}
				used.Add(item);
				PreparedCookingIngredient part = item.GetPart<PreparedCookingIngredient>();
				if (num > part.charges)
				{
					num -= part.charges;
					part.ParentObject.SplitFromStack();
					part.ParentObject.Destroy();
					goto IL_0007;
				}
				part.ParentObject.SplitFromStack();
				part.charges -= num;
				if (part.charges == 0)
				{
					part.ParentObject.Destroy();
				}
				else
				{
					part.ParentObject.CheckStack();
				}
				num = 0;
				break;
			}
			break;
		}
		CookingGamestate.ResetInventorySnapshot();
	}
}
[Serializable]
public class PreparedCookingRecipieComponentLiquid : ICookingRecipeComponent
{
	public string liquid;

	public int amount;

	public PreparedCookingRecipieComponentLiquid(string liquid, int amount = 1)
	{
		this.liquid = liquid;
		this.amount = amount;
	}

	public string getIngredientId()
	{
		return "liquid-" + liquid;
	}

	public bool HasPlants()
	{
		if (liquid == "sap")
		{
			return true;
		}
		if (liquid == "cider")
		{
			return true;
		}
		return false;
	}

	public bool HasFungi()
	{
		return false;
	}

	public string getDisplayName()
	{
		string text = ((amount > 1) ? "drams" : "dram");
		if (doesPlayerHaveEnough())
		{
			return "&y" + amount + "&y " + text + " of " + LiquidVolume.getLiquid(liquid).GetName();
		}
		return "&r" + amount + "&y " + text + " of " + LiquidVolume.getLiquid(liquid).GetName();
	}

	public int PlayerHolding()
	{
		int num = 0;
		foreach (GameObject item in CookingGamestate.GetInventorySnapshot())
		{
			LiquidVolume part = item.GetPart<LiquidVolume>();
			if (part != null && part.IsPureLiquid(liquid))
			{
				num += part.Volume;
			}
		}
		return num;
	}

	public bool doesPlayerHaveEnough()
	{
		return amount <= CookingGamestate.GetIngredientQuantity(this);
	}

	public string createPlayerDoesNotHaveEnoughMessage()
	{
		return "You don't have enough " + LiquidVolume.getLiquid(liquid).GetName(null) + "&Y.";
	}

	public void use(List<GameObject> used)
	{
		int num = amount;
		while (true)
		{
			IL_0007:
			foreach (GameObject item in CookingGamestate.GetInventorySnapshot())
			{
				LiquidVolume part = item.GetPart<LiquidVolume>();
				if (part != null && part.IsPureLiquid(liquid))
				{
					used.Add(item);
					if (num > part.Volume)
					{
						num -= part.Volume;
						part.Volume = 0;
						part.Empty();
						goto IL_0007;
					}
					part.UseDrams(num);
					break;
				}
			}
			break;
		}
		CookingGamestate.ResetInventorySnapshot();
	}
}
