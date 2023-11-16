using System;
using System.Collections.Generic;

namespace XRL.World.Parts;

[Serializable]
public class CyberneticsFistReplacement : IPart
{
	public string FistObject;

	private List<int> ReplacedFists = new List<int>();

	private List<string> OriginalFistBlueprints = new List<string>();

	public string ImplantDependency;

	public override bool SameAs(IPart p)
	{
		return false;
	}

	public override bool AllowStaticRegistration()
	{
		return false;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != ImplantedEvent.ID)
		{
			return ID == UnimplantedEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(ImplantedEvent E)
	{
		ImplantDependency = E.Part.DependsOn;
		ApplyFists(E.Part.ParentBody);
		E.Implantee.RegisterPartEvent(this, "RegenerateDefaultEquipment");
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(UnimplantedEvent E)
	{
		E.Implantee.UnregisterPartEvent(this, "RegenerateDefaultEquipment");
		E.Part?.ParentBody?.UpdateBodyParts();
		return base.HandleEvent(E);
	}

	public void ApplyFists(Body pBody)
	{
		if (string.IsNullOrEmpty(ImplantDependency))
		{
			return;
		}
		foreach (BodyPart item in pBody.GetPart("Hand", EvenIfDismembered: true))
		{
			if (!item.Extrinsic && item.SupportsDependent == ImplantDependency)
			{
				ReplacedFists.Add(item.ID);
				if (item.DefaultBehavior != null)
				{
					OriginalFistBlueprints.Add(item.DefaultBehavior.Blueprint);
				}
				else
				{
					OriginalFistBlueprints.Add(null);
				}
				item.DefaultBehavior = GameObject.create(FistObject);
			}
		}
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "RegenerateDefaultEquipment")
		{
			ApplyFists(E.GetParameter<Body>("Body"));
		}
		return base.FireEvent(E);
	}
}
