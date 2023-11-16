using System;
using XRL.UI;

namespace XRL.World.Parts;

[Serializable]
public class RebornOnDeathInThinWorld : IPart
{
	public bool reborn;

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "BeforeTakeAction");
		Object.RegisterPartEvent(this, "BeforeDie");
		base.Register(Object);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "BeforeTakeAction" && reborn)
		{
			reborn = false;
			if (ParentObject.pBrain != null)
			{
				ParentObject.pBrain.Goals.Clear();
				ParentObject.pBrain.ObjectMemory.Clear();
			}
		}
		if (E.ID == "BeforeDie" && ParentObject.Statistics.ContainsKey("Hitpoints"))
		{
			ParentObject.Statistics["Hitpoints"].Penalty = 0;
			if (ParentObject.pPhysics != null)
			{
				ParentObject.pPhysics.Temperature = 30;
			}
			ParentObject.DilationSplat();
			ParentObject.SmallTeleportSwirl();
			if (ParentObject.IsPlayer())
			{
				Popup.Show("Death has no meaning here.");
			}
			else if (ParentObject.pBrain != null)
			{
				ParentObject.pBrain.Goals.Clear();
				ParentObject.pBrain.ObjectMemory.Clear();
				reborn = true;
			}
			DidX("continue", "being", ".", null, ParentObject);
			return false;
		}
		return base.FireEvent(E);
	}
}
