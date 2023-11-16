using System;
using XRL.Rules;

namespace XRL.World.Parts;

[Serializable]
public class LuminousInfection : IPart
{
	public int ShroomCounter = Stat.Random(300, 500);

	public string ManagerID => ParentObject.id + "::LuminousInfection";

	public override bool SameAs(IPart p)
	{
		return true;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != EndTurnEvent.ID && ID != EquippedEvent.ID && ID != UnequippedEvent.ID)
		{
			return ID == OnDestroyObjectEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(EndTurnEvent E)
	{
		ShroomCounter--;
		if (ShroomCounter < 0)
		{
			TryGrowMushroom();
			ShroomCounter = Stat.Random(300, 500);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EquippedEvent E)
	{
		BodyPart bodyPart = ParentObject?.EquippedOn();
		if (bodyPart != null)
		{
			bodyPart.AddPartAt("Icy Outcrop", 0, null, null, null, null, ManagerID, null, null, null, null, null, null, null, true, null, null, null, null, "Icy Outcrop", new string[2] { "Feet", "Roots" });
			TryGrowMushroom();
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(UnequippedEvent E)
	{
		E.Actor.RemoveBodyPartsByManager(ManagerID);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(OnDestroyObjectEvent E)
	{
		ParentObject?.Equipped?.RemoveBodyPartsByManager(ManagerID, EvenIfDismembered: true);
		return base.HandleEvent(E);
	}

	public void TryGrowMushroom()
	{
		BodyPart bodyPart = ParentObject?.Equipped?.Body?.FindEquippedItem(ParentObject);
		if (bodyPart == null)
		{
			return;
		}
		try
		{
			BodyPart bodyPart2 = bodyPart.FindByManager(ManagerID);
			if (bodyPart2 == null || bodyPart2.Equipped != null)
			{
				return;
			}
			GameObject gameObject = GameObject.createUnmodified("Hoarshroom");
			bodyPart2.Equip(gameObject, 0, Silent: true, ForDeepCopy: false, Forced: false, SemiForced: true);
			if (bodyPart2.Equipped == gameObject)
			{
				gameObject.RequirePart<Cursed>();
				gameObject.RequirePart<RemoveCursedOnUnequip>();
				if (ParentObject.Equipped.IsPlayer())
				{
					IComponent<GameObject>.AddPlayerMessage("You sprout a {{C|luminous hoarshroom}}.");
				}
			}
			else
			{
				gameObject.Obliterate();
			}
		}
		catch (Exception x)
		{
			MetricsManager.LogException("TryGrowMushroom", x);
		}
	}
}
