namespace XRL.World.Skills.Cooking;

public interface ICookingRecipeResult
{
	string GetCampfireDescription();

	string apply(GameObject eater);
}
