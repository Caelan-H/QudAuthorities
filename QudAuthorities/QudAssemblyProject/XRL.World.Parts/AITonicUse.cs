using System;

namespace XRL.World.Parts;

[Serializable]
public class AITonicUse : IPart
{
	public override bool SameAs(IPart p)
	{
		return true;
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "AIGetDefensiveItemList");
		base.Register(Object);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "AIGetDefensiveItemList" && !ParentObject.IsBroken() && !ParentObject.IsRusted() && !ParentObject.IsImportant())
		{
			GameObject gameObjectParameter = E.GetGameObjectParameter("User");
			if (gameObjectParameter.FireEvent("ApplyingTonic") && gameObjectParameter.GetTonicEffectCount() < gameObjectParameter.GetTonicCapacity())
			{
				E.AddAICommand("Apply", 100, ParentObject, Inv: true);
			}
		}
		return base.FireEvent(E);
	}
}
