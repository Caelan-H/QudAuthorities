using System;
using XRL.UI;

namespace XRL.World.Parts;

[Serializable]
public class Pettable : IPart
{
	public string useFactionForFeelingFloor;

	public bool pettableIfPositiveFeeling;

	private bool bOnlyAllowIfLiked = true;

	public override bool SameAs(IPart p)
	{
		return false;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != CanSmartUseEvent.ID && ID != CommandSmartUseEvent.ID && ID != GetInventoryActionsEvent.ID)
		{
			return ID == InventoryActionEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetInventoryActionsEvent E)
	{
		E.AddAction("Pet", "pet", "Pet", null, 'p', FireOnActor: false, 5);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(InventoryActionEvent E)
	{
		if (E.Command == "Pet" && Pet(E.Actor))
		{
			E.RequestInterfaceExit();
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(CanSmartUseEvent E)
	{
		if ((!E.Actor.IsPlayer() || !ParentObject.IsPlayerLed() || !ParentObject.IsMobile()) && !PreferConversation())
		{
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(CommandSmartUseEvent E)
	{
		if ((!E.Actor.IsPlayer() || !ParentObject.IsPlayerLed() || !ParentObject.IsMobile()) && !PreferConversation())
		{
			Pet(E.Actor);
		}
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "CanHaveSmartUseConversation");
		base.Register(Object);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "CanHaveSmartUseConversation" && !PreferConversation())
		{
			return false;
		}
		return base.FireEvent(E);
	}

	public bool Pet(GameObject who)
	{
		if (pettableIfPositiveFeeling && ParentObject.pBrain.GetFeeling(who) < 0)
		{
			if (who.IsPlayer())
			{
				Popup.Show(ParentObject.The + ParentObject.ShortDisplayName + ParentObject.GetVerb("shy") + " away from you.");
			}
			return true;
		}
		if (useFactionForFeelingFloor == null)
		{
			if (bOnlyAllowIfLiked && who != null && (ParentObject.pBrain?.GetFeeling(who) ?? 0) < 50)
			{
				if (who.IsPlayer())
				{
					Popup.Show(ParentObject.The + ParentObject.ShortDisplayName + ParentObject.GetVerb("shy") + " away from you.");
				}
				return true;
			}
		}
		else if (!pettableIfPositiveFeeling && (ParentObject.pBrain.GetFeeling(who) < 0 || (who.IsPlayer() && Math.Max(The.Game.PlayerReputation.getFeeling(useFactionForFeelingFloor), ParentObject.pBrain?.GetFeeling(who) ?? 0) < 50)))
		{
			if (who.IsPlayer())
			{
				Popup.Show(ParentObject.The + ParentObject.ShortDisplayName + ParentObject.GetVerb("shy") + " away from you.");
			}
			return true;
		}
		IComponent<GameObject>.XDidYToZ(who, "pet", ParentObject, null, null, null, null, null, UseFullNames: false, IndefiniteSubject: false, IndefiniteObject: false, IndefiniteObjectForOthers: false, PossessiveObject: false, null, null, null, DescribeSubjectDirection: false, DescribeSubjectDirectionLate: false, AlwaysVisible: false, FromDialog: false, UsePopup: true);
		if (who.IsPlayer())
		{
			if (ParentObject.HasPropertyOrTag("SpecialPetResponse"))
			{
				Popup.Show(GameText.VariableReplace(ParentObject.GetTag("SpecialPetResponse"), ParentObject));
			}
			else
			{
				Popup.Show(ParentObject.The + ParentObject.ShortDisplayName + " " + ParentObject.GetPropertyOrTag("PetResponse", "stares at you blankly").Split(',').GetRandomElement() + ".");
			}
		}
		who.UseEnergy(1000, "Petting");
		ParentObject.FireEvent(Event.New("ObjectPetted", "Object", ParentObject, "Petter", who));
		return true;
	}

	public static bool PreferConversation(GameObject who)
	{
		if (who.GetIntProperty("NamedVillager") > 0)
		{
			return true;
		}
		if (who.GetIntProperty("ParticipantVillager") > 0)
		{
			return true;
		}
		if (who.GetIntProperty("Hero") > 0)
		{
			return true;
		}
		if (who.GetIntProperty("Librarian") > 0)
		{
			return true;
		}
		if (who.HasTagOrProperty("PreferChatToPet"))
		{
			return true;
		}
		return false;
	}

	public bool PreferConversation()
	{
		return PreferConversation(ParentObject);
	}
}
