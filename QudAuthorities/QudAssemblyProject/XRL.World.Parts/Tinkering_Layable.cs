using System;

namespace XRL.World.Parts;

[Serializable]
public class Tinkering_Layable : IPart
{
	public string DetonationMessage = "AfterThrown";

	public GameObject ComponentOf;

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != GetContextEvent.ID && ID != RemoveFromContextEvent.ID && ID != ReplaceInContextEvent.ID)
		{
			return ID == TryRemoveFromContextEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetContextEvent E)
	{
		if (GameObject.validate(ref ComponentOf))
		{
			E.ObjectContext = ComponentOf;
			E.Relation = 6;
			E.RelationManager = this;
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(ReplaceInContextEvent E)
	{
		if (GameObject.validate(ref ComponentOf) && ComponentOf.GetPart("Tinkering_Mine") is Tinkering_Mine tinkering_Mine && tinkering_Mine.Explosive == ParentObject)
		{
			tinkering_Mine.SetExplosive(E.Replacement);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(RemoveFromContextEvent E)
	{
		if (GameObject.validate(ref ComponentOf) && ComponentOf.GetPart("Tinkering_Mine") is Tinkering_Mine tinkering_Mine && tinkering_Mine.Explosive == ParentObject)
		{
			tinkering_Mine.SetExplosive(null);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(TryRemoveFromContextEvent E)
	{
		if (GameObject.validate(ref ComponentOf))
		{
			return false;
		}
		return base.HandleEvent(E);
	}
}
