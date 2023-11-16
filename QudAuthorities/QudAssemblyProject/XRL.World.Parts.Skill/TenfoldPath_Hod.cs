using System;

namespace XRL.World.Parts.Skill;

/// This part is not used in the base game.
[Serializable]
public class TenfoldPath_Hod : BaseSkill
{
	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != GetRandomBuyChimericBodyPartRollsEvent.ID)
		{
			return ID == GetRandomBuyMutationCountEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetRandomBuyMutationCountEvent E)
	{
		E.Amount++;
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetRandomBuyChimericBodyPartRollsEvent E)
	{
		E.Amount++;
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "Regenerating");
		base.Register(Object);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "Regenerating")
		{
			E.SetParameter("Amount", E.GetIntParameter("Amount") * 21 / 20);
		}
		return base.FireEvent(E);
	}
}
