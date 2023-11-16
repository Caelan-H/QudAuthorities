using System;

namespace XRL.World.Parts;

[Serializable]
public class GraveMoss : IPart
{
	public bool triggered;

	public int turns = 20;

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "AppliedLiquidCovered");
		Object.RegisterPartEvent(this, "ApplyBloody");
		Object.RegisterPartEvent(this, "ApplyEffect");
		Object.RegisterPartEvent(this, "ForceApplyEffect");
		Object.RegisterPartEvent(this, "ObjectEnteredCell");
		base.Register(Object);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "ObjectEnteredCell")
		{
			GameObject gameObjectParameter = E.GetGameObjectParameter("Object");
			if (gameObjectParameter != null && gameObjectParameter.HasTag("Corpse"))
			{
				trigger();
			}
			return true;
		}
		if (E.ID == "EndTurn")
		{
			if (ParentObject.HasEffect("Bloody"))
			{
				trigger();
			}
			if (turns > 0)
			{
				turns--;
				if (turns <= 0)
				{
					if (ParentObject.pPhysics.CurrentCell.HasObjectWithTag("Corpse"))
					{
						ParentObject.pPhysics.CurrentCell.GetFirstObjectWithPropertyOrTag("Corpse")?.Destroy();
					}
					ParentObject.pPhysics.CurrentCell.AddObject("Gorged Growth").SetActive();
					ParentObject.Destroy();
				}
			}
		}
		else if (E.ID == "ApplyEffect" || E.ID == "ForceApplyEffect")
		{
			if (E.GetParameter<Effect>("Effect").ClassName.Contains("Blood"))
			{
				trigger();
			}
		}
		else if (E.ID == "ApplyBloody")
		{
			trigger();
		}
		else if (E.ID == "AppliedLiquidCovered" && E.GetParameter("Liquid") is LiquidVolume liquidVolume && liquidVolume.hasLiquid("blood"))
		{
			trigger();
		}
		return base.FireEvent(E);
	}

	public void trigger()
	{
		if (!triggered)
		{
			if (ParentObject.IsVisible())
			{
				IComponent<GameObject>.AddPlayerMessage("The grave moss starts to fizz hungrily.");
			}
			triggered = true;
			ParentObject.RegisterPartEvent(this, "EndTurn");
		}
	}
}
