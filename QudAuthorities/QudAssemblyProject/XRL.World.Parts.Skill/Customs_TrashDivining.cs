using System;

namespace XRL.World.Parts.Skill;

[Serializable]
public class Customs_TrashDivining : BaseSkill
{
	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == PartSupportEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(PartSupportEvent E)
	{
		if (E.Skip != this && E.Type == "TrashRifling")
		{
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public override bool AddSkill(GameObject GO)
	{
		GO.RequirePart<TrashRifling>();
		return base.AddSkill(GO);
	}

	public override bool RemoveSkill(GameObject GO)
	{
		NeedPartSupportEvent.Send(GO, "TrashRifling", this);
		return base.RemoveSkill(GO);
	}
}
