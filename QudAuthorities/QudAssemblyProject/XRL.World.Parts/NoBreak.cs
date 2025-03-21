using System;

namespace XRL.World.Parts;

[Serializable]
public class NoBreak : IPart
{
	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != CanApplyEffectEvent.ID)
		{
			return ID == ApplyEffectEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(CanApplyEffectEvent E)
	{
		if (E.Name == "Broken")
		{
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(ApplyEffectEvent E)
	{
		if (E.Name == "Broken" || E.Effect.ClassName == "Broken")
		{
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}
}
