using System;
using System.Collections.Generic;
using XRL.World.Skills.Cooking;

namespace XRL.World.Parts;

[Serializable]
public class Chef : IPart
{
	public bool bCreated;

	public List<CookingRecipe> signatureDishes = new List<CookingRecipe>();

	public override bool SameAs(IPart p)
	{
		return false;
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "EnteredCell");
		base.Register(Object);
	}

	public override IPart DeepCopy(GameObject Parent)
	{
		Chef obj = base.DeepCopy(Parent) as Chef;
		obj.signatureDishes = new List<CookingRecipe>(signatureDishes);
		return obj;
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "EnteredCell")
		{
			if (bCreated)
			{
				return true;
			}
			bCreated = true;
			int newTier = ParentObject.pPhysics.CurrentCell.ParentZone.NewTier;
			GameObject gameObject = GameObjectFactory.Factory.CreateObject("BaseCookbook");
			if (ParentObject.HasPart("GivesRep"))
			{
				gameObject.AddPart(new Cookbook("Generic_LegendaryChef", "3", newTier, ParentObject.DisplayNameOnlyStripped));
			}
			else
			{
				gameObject.AddPart(new Cookbook());
			}
			gameObject.GetPart<Cookbook>().GenerateCookbook();
			ParentObject.TakeObject(gameObject, Silent: false, 0);
			GameObject gameObject2 = GameObjectFactory.Factory.CreateObject("ChefOven");
			Cookbook cookbook = gameObject.GetPart("Cookbook") as Cookbook;
			if (ParentObject.HasPart("GivesRep"))
			{
				signatureDishes = cookbook.recipes;
			}
			else
			{
				signatureDishes.Add(cookbook.recipes.GetRandomElement());
			}
			if (ParentObject.HasTag("StiltChef"))
			{
				CookingRecipe item = Activator.CreateInstance(ModManager.ResolveType("XRL.World.Skills.Cooking.HotandSpiny")) as CookingRecipe;
				signatureDishes.Add(item);
			}
			if (gameObject2.HasPart("Campfire"))
			{
				gameObject2.RemovePart("Campfire");
			}
			gameObject2.AddPart(new Campfire(signatureDishes));
			(ParentObject.pPhysics.CurrentCell.GetFirstAdjacentCell((Cell OC) => !OC.HasObjectWithPart("Physics")) ?? ParentObject.pPhysics.CurrentCell.GetFirstAdjacentCell((Cell OC) => OC.IsEmpty()))?.AddObject(gameObject2);
		}
		return base.FireEvent(E);
	}
}
