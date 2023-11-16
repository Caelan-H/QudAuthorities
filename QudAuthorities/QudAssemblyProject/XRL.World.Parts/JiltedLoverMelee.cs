using System;
using XRL.World.Effects;

namespace XRL.World.Parts;

[Serializable]
public class JiltedLoverMelee : IPart
{
	public int Chance = 15;

	public int StuckDuration = 10;

	public int SaveTarget = 20;

	public string GrabbedText = "grabbed by a jilted lover";

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "WeaponHit");
		base.Register(Object);
	}

	public override bool SameAs(IPart p)
	{
		JiltedLoverMelee jiltedLoverMelee = p as JiltedLoverMelee;
		if (jiltedLoverMelee.Chance != Chance)
		{
			return false;
		}
		if (jiltedLoverMelee.StuckDuration != StuckDuration)
		{
			return false;
		}
		if (jiltedLoverMelee.SaveTarget != SaveTarget)
		{
			return false;
		}
		if (jiltedLoverMelee.GrabbedText != GrabbedText)
		{
			return false;
		}
		return base.SameAs(p);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "WeaponHit" && ParentObject.hitpoints > 0 && ParentObject.Equipped != null && ParentObject.Equipped.CurrentCell != null && !ParentObject.Equipped.CurrentCell.IsGraveyard() && Chance.in100())
		{
			GameObject gameObjectParameter = E.GetGameObjectParameter("Defender");
			if (gameObjectParameter.FireEvent("BeforeGrabbed"))
			{
				Stuck e = new Stuck(StuckDuration, SaveTarget, "Grab Stuck Restraint", null, GrabbedText, ParentObject.Equipped.id);
				gameObjectParameter.ApplyEffect(e);
			}
		}
		return base.FireEvent(E);
	}
}
