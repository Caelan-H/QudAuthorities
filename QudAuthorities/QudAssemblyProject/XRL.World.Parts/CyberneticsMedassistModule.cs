using System;
using System.Collections.Generic;
using System.Text;
using ConsoleLib.Console;
using XRL.UI;

namespace XRL.World.Parts;

[Serializable]
public class CyberneticsMedassistModule : IPoweredPart
{
	public Guid ActivatedAbilityID = Guid.Empty;

	public int TonicCapacity = 8;

	public CyberneticsMedassistModule()
	{
		ChargeUse = 0;
		WorksOnImplantee = true;
		NameForStatus = "DeploymentExpertSystem";
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != CommandEvent.ID && ID != GetInventoryActionsEvent.ID && ID != GetShortDescriptionEvent.ID && ID != ImplantedEvent.ID && ID != InventoryActionEvent.ID && ID != OwnerGetInventoryActionsEvent.ID)
		{
			return ID == UnimplantedEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(ImplantedEvent E)
	{
		E.Implantee.RegisterPartEvent(this, "BeforeApplyDamage");
		E.Implantee.RegisterPartEvent(this, "CommandToggleMedassistModule");
		E.Implantee.RegisterPartEvent(this, "OwnerGetInventoryActions");
		ActivatedAbilityID = E.Implantee.AddActivatedAbility("Medassist Module", "CommandToggleMedassistModule", "Cybernetics", null, "\a", null, Toggleable: true, DefaultToggleState: true);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(UnimplantedEvent E)
	{
		E.Implantee.UnregisterPartEvent(this, "BeforeApplyDamage");
		E.Implantee.UnregisterPartEvent(this, "CommandToggleMedassistModule");
		E.Implantee.UnregisterPartEvent(this, "OwnerGetInventoryActions");
		E.Implantee.RemoveActivatedAbility(ref ActivatedAbilityID);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(CommandEvent E)
	{
		if (E.Command == "CommandToggleMedassistModule" && E.Actor == ParentObject.Implantee)
		{
			ParentObject.Implantee.ToggleActivatedAbility(ActivatedAbilityID);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetShortDescriptionEvent E)
	{
		Inventory inventory = ParentObject.Inventory;
		if (inventory != null)
		{
			E.Infix.Append("\n{{c|Current loadout:}}");
			if (inventory.Objects.Count > 0)
			{
				foreach (GameObject @object in inventory.Objects)
				{
					E.Infix.Append("\n ").Append('ú').Append(' ')
						.Append(@object.DisplayName);
				}
			}
			else
			{
				E.Infix.Append(" {{y|no injectors}}");
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetInventoryActionsEvent E)
	{
		E.AddAction("Load Tonics", "load tonics", "LoadTonics", null, 'o');
		E.AddAction("Eject Tonics", "eject tonics", "EjectTonics", null, 'j');
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(OwnerGetInventoryActionsEvent E)
	{
		if (CanBeLoaded(E.Object))
		{
			E.AddAction("Load Into Medassist Module", "load into medassist module", "LoadTonic", null, 'm', FireOnActor: false, 0, 0, Override: false, WorksAtDistance: false, WorksTelekinetically: false, WorksTelepathically: false, AsMinEvent: true, ParentObject);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(InventoryActionEvent E)
	{
		if (E.Command == "LoadTonic")
		{
			Inventory inventory = ParentObject.Inventory;
			if (inventory == null)
			{
				throw new Exception("inventory missing from " + ParentObject.DebugName);
			}
			if (!CanBeLoaded(E.Item))
			{
				throw new Exception("received invalid item " + E.Item.DebugName);
			}
			if (GetLoadedCount() >= TonicCapacity)
			{
				FullMessage(E.Actor);
			}
			else
			{
				GameObject gameObject = E.Item.RemoveOne();
				gameObject.RemoveFromContext();
				inventory.AddObject(gameObject);
				if (E.Actor.IsPlayer())
				{
					Popup.Show("You slot " + gameObject.a + gameObject.ShortDisplayName + " into" + (ParentObject.HasProperName ? " " : " your ") + ParentObject.ShortDisplayName + ".");
				}
				E.Actor.UseEnergy(1000);
				E.RequestInterfaceExit();
			}
		}
		else if (E.Command == "LoadTonics")
		{
			Inventory inventory2 = ParentObject.Inventory;
			if (inventory2 == null)
			{
				throw new Exception("inventory missing from " + ParentObject.DebugName);
			}
			if (GetLoadedCount() >= TonicCapacity)
			{
				FullMessage(E.Actor);
			}
			else
			{
				List<GameObject> list = Event.NewGameObjectList();
				foreach (GameObject content in E.Actor.GetContents())
				{
					if (CanBeLoaded(content))
					{
						int Relation;
						GameObject objectContext = content.GetObjectContext(out Relation);
						if (objectContext != null && objectContext != ParentObject && Relation != 4 && Relation != 6)
						{
							list.Add(content);
						}
					}
				}
				if (list.Count <= 0)
				{
					if (E.Actor.IsPlayer())
					{
						Popup.Show("You have no tonics to load.");
					}
				}
				else
				{
					GameObject gameObject2 = PickItem.ShowPicker(list, null, PickItem.PickItemDialogStyle.SelectItemDialog, E.Actor, null, null, null, PreserveOrder: false, null, ShowContext: true);
					if (gameObject2 != null)
					{
						gameObject2.SplitFromStack();
						gameObject2.RemoveFromContext();
						inventory2.AddObject(gameObject2);
						if (E.Actor.IsPlayer())
						{
							Popup.Show("You slot " + gameObject2.a + gameObject2.ShortDisplayName + " into" + (ParentObject.HasProperName ? " " : " your ") + ParentObject.ShortDisplayName + ".");
						}
						E.Actor.UseEnergy(1000);
						E.RequestInterfaceExit();
					}
				}
			}
		}
		else if (E.Command == "EjectTonics")
		{
			Inventory inventory3 = ParentObject.Inventory;
			if (inventory3 == null)
			{
				throw new Exception("inventory missing from " + ParentObject.DebugName);
			}
			int num = 0;
			foreach (GameObject @object in inventory3.Objects)
			{
				num += @object.Count;
			}
			if (num <= 0)
			{
				if (E.Actor.IsPlayer())
				{
					Popup.Show("Your " + ParentObject.ShortDisplayName + ParentObject.Is + " empty.");
				}
			}
			else
			{
				int num2 = TonicCapacity * 3;
				GameObject gameObject3 = null;
				while (inventory3.Objects.Count > 0 && --num2 > 0)
				{
					gameObject3 = inventory3.Objects[0];
					E.Actor.TakeObject(gameObject3, Silent: false, 0);
				}
				if (E.Actor.IsPlayer())
				{
					Popup.Show("You eject " + ((num == 1) ? (gameObject3.the + gameObject3.ShortDisplayName) : "the injectors") + " from" + (ParentObject.HasProperName ? "" : " your") + ParentObject.ShortDisplayName + ".");
					E.Actor.UseEnergy(1000);
					E.RequestInterfaceExit();
				}
			}
		}
		return base.HandleEvent(E);
	}

	public override bool SameAs(IPart p)
	{
		return false;
	}

	public override bool WantTurnTick()
	{
		return true;
	}

	public override bool WantTenTurnTick()
	{
		return true;
	}

	public override bool WantHundredTurnTick()
	{
		return true;
	}

	public override void TurnTick(long TurnNumber)
	{
		AttemptMedicalAssistance();
	}

	public override void TenTurnTick(long TurnNumber)
	{
		AttemptMedicalAssistance();
	}

	public override void HundredTurnTick(long TurnNumber)
	{
		AttemptMedicalAssistance();
	}

	public override bool GetActivePartLocallyDefinedFailure()
	{
		return !ParentObject.Implantee.IsActivatedAbilityToggledOn(ActivatedAbilityID);
	}

	public override string GetActivePartLocallyDefinedFailureDescription()
	{
		return "Deactivated";
	}

	public void AttemptMedicalAssistance(Damage damage = null)
	{
		if (IsDisabled(UseCharge: false, IgnoreCharge: true, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
		{
			return;
		}
		Inventory inventory = ParentObject.Inventory;
		if (inventory == null || inventory.Objects.Count <= 0)
		{
			return;
		}
		GameObject implantee = ParentObject.Implantee;
		if (implantee.GetTonicEffectCount() >= implantee.GetTonicCapacity())
		{
			return;
		}
		int num = 0;
		GameObject gameObject = null;
		int i = 0;
		for (int count = inventory.Objects.Count; i < count; i++)
		{
			GameObject gameObject2 = inventory.Objects[i];
			if (!gameObject2.IsBroken() && !gameObject2.IsRusted())
			{
				int utilityScore = GetUtilityScore(implantee, gameObject2, damage);
				if (utilityScore > num)
				{
					gameObject = gameObject2;
					num = utilityScore;
				}
			}
		}
		if (gameObject == null || IsDisabled(UseCharge: true, IgnoreCharge: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
		{
			return;
		}
		List<Effect> tonicEffects = implantee.GetTonicEffects();
		if (implantee.IsPlayer())
		{
			IComponent<GameObject>.AddPlayerMessage("Your " + ParentObject.ShortDisplayName + ParentObject.GetVerb("inject") + " you with " + gameObject.a + gameObject.DisplayNameSingle + ".");
		}
		Event e = Event.New("ApplyTonic", "Owner", ParentObject, "Target", implantee, "Overdose", "No", "Attacker", null);
		if (gameObject.FireEvent(e))
		{
			gameObject.Destroy();
			Event @event = Event.New("TonicAutoApplied");
			@event.SetParameter("Subject", implantee);
			@event.SetParameter("By", ParentObject);
			@event.SetParameter("Damage", damage);
			{
				foreach (Effect tonicEffect in implantee.GetTonicEffects())
				{
					if (!tonicEffects.Contains(tonicEffect))
					{
						tonicEffect.FireEvent(@event);
					}
				}
				return;
			}
		}
		if (implantee.IsPlayer())
		{
			IComponent<GameObject>.AddPlayerMessage("The injection fails.");
		}
	}

	public static int GetUtilityScore(GameObject who, GameObject tonic, Damage damage = null)
	{
		return GetUtilityScoreEvent.GetFor(who, tonic, damage);
	}

	public override bool AllowStaticRegistration()
	{
		return false;
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "BeforeApplyDamage" && E.GetParameter("Damage") is Damage damage)
		{
			GameObject implantee = ParentObject.Implantee;
			GameObject gameObjectParameter = E.GetGameObjectParameter("Attacker");
			if (E.HasParameter("Phase"))
			{
				int intParameter = E.GetIntParameter("Phase");
				bool num = implantee.PhaseMatches(intParameter);
				AttemptMedicalAssistance(damage);
				if (num && !implantee.PhaseMatches(intParameter))
				{
					return false;
				}
			}
			else if (gameObjectParameter != null)
			{
				bool num2 = implantee.PhaseMatches(gameObjectParameter);
				AttemptMedicalAssistance(damage);
				if (num2 && !implantee.PhaseMatches(gameObjectParameter))
				{
					return false;
				}
			}
			else
			{
				int phase = implantee.GetPhase();
				AttemptMedicalAssistance(damage);
				if (!implantee.PhaseMatches(phase))
				{
					return false;
				}
			}
		}
		return base.FireEvent(E);
	}

	public static bool CanBeLoaded(GameObject obj)
	{
		if (!(obj.GetPart("Tonic") is Tonic tonic))
		{
			return false;
		}
		if (tonic.Eat)
		{
			return false;
		}
		return true;
	}

	public int GetLoadedCount()
	{
		int num = 0;
		Inventory inventory = ParentObject.Inventory;
		if (inventory != null)
		{
			foreach (GameObject @object in inventory.Objects)
			{
				num += @object.Count;
			}
			return num;
		}
		return num;
	}

	public void FullMessage(GameObject who)
	{
		if (who.IsPlayer())
		{
			StringBuilder stringBuilder = Event.NewStringBuilder();
			if (ParentObject.HasProperName)
			{
				stringBuilder.Append(ColorUtility.CapitalizeExceptFormatting(ParentObject.ShortDisplayName));
			}
			else
			{
				stringBuilder.Append("Your ").Append(ParentObject.ShortDisplayName);
			}
			stringBuilder.Append(ParentObject.Is).Append(" full.");
			Popup.Show(stringBuilder.ToString());
		}
	}
}
