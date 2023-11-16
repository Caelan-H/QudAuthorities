using System;

namespace XRL.World.Parts;

[Serializable]
public class CyberneticsUnimplantUnequipsTwoSlotItems : IPart
{
	public override bool SameAs(IPart p)
	{
		return true;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == UnimplantedEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(UnimplantedEvent E)
	{
		if (E.Part?.ParentBody != null)
		{
			foreach (BodyPart part in E.Part.ParentBody.GetParts())
			{
				if (part.Equipped != null && part.Equipped.UsesTwoSlots)
				{
					part.TryUnequip(Silent: false, SemiForced: true);
				}
			}
		}
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}
}
