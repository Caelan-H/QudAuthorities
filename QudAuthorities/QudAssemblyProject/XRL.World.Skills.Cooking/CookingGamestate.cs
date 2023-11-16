using System;
using System.Collections.Generic;
using System.Linq;
using Qud.API;
using XRL.Core;

namespace XRL.World.Skills.Cooking;

[Serializable]
[GamestateSingleton("cookingGamestate")]
public class CookingGamestate : IGamestateSingleton
{
	public List<CookingRecipe> knownRecipies = new List<CookingRecipe>();

	[NonSerialized]
	public static List<GameObject> inventorySnapshot;

	[NonSerialized]
	public static Dictionary<string, int> ingredientQuantity;

	public static CookingGamestate instance => XRLCore.Core.Game.GetObjectGameState("cookingGamestate") as CookingGamestate;

	public static List<GameObject> GetInventorySnapshot()
	{
		if (inventorySnapshot == null)
		{
			bool carnivorous = XRLCore.Core.Game.Player.Body.HasPart("Carnivorous");
			inventorySnapshot = XRLCore.Core.Game.Player.Body.GetInventoryDirectAndEquipment((GameObject go) => (!carnivorous || (!go.HasTag("Plant") && !go.HasTag("Fungus"))) ? true : false);
		}
		return inventorySnapshot;
	}

	public static void ResetInventorySnapshot()
	{
		inventorySnapshot = null;
		if (ingredientQuantity != null)
		{
			ingredientQuantity.Clear();
		}
	}

	public static int GetIngredientQuantity(ICookingRecipeComponent component)
	{
		if (ingredientQuantity == null)
		{
			ingredientQuantity = new Dictionary<string, int>();
		}
		string ingredientId = component.getIngredientId();
		if (!ingredientQuantity.ContainsKey(ingredientId))
		{
			ingredientQuantity[ingredientId] = component.PlayerHolding();
		}
		return ingredientQuantity[ingredientId];
	}

	public static bool KnowsRecipe(CookingRecipe newRecipe)
	{
		return instance.knownRecipies.Any((CookingRecipe i) => newRecipe != null && i != null && newRecipe.GetDisplayName() == i.GetDisplayName());
	}

	public static bool KnowsRecipe(string ClassName)
	{
		if (ClassName.IsNullOrEmpty())
		{
			return false;
		}
		return instance.knownRecipies.Any((CookingRecipe i) => i.GetType().Name == ClassName);
	}

	public static CookingRecipe LearnRecipe(CookingRecipe newRecipe)
	{
		JournalAPI.AddRecipeNote(newRecipe);
		return newRecipe;
	}

	public void worldBuild()
	{
	}

	public void init()
	{
	}
}
