using System;
using XRL.World.Effects;

namespace XRL.World.Parts.Mutation;

[Serializable]
public class Astral : BaseMutation
{
	public Astral()
	{
		DisplayName = "Astral";
	}

	public override bool CanLevel()
	{
		return false;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == GetItemElementsEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetItemElementsEvent E)
	{
		E.Add("stars", 1);
		return base.HandleEvent(E);
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "BeginTakeAction");
		Object.RegisterPartEvent(this, "EnteredCell");
		base.Register(Object);
	}

	public override string GetLevelText(int Level)
	{
		return "";
	}

	public override string GetDescription()
	{
		return "You live in an alternate plane of reality.";
	}

	public override bool FireEvent(Event E)
	{
		if ((E.ID == "BeginTakeAction" || E.ID == "EnteredCell") && !ParentObject.HasEffect("Phased") && ParentObject.FireEvent("CheckRealityDistortionUsability"))
		{
			ParentObject.ApplyEffect(new Phased(9999));
		}
		return base.FireEvent(E);
	}
}
