using System;
using System.Collections.Generic;
using System.Text;
using XRL.Language;
using XRL.Rules;
using XRL.UI;

namespace XRL.World.Parts;

[Serializable]
public class DecoyHologramEmitter : IPoweredPart
{
	public int MaxDecoys = 1;

	public int Difficulty = 15;

	public bool HologramActive;

	[NonSerialized]
	public List<GameObject> Holograms = new List<GameObject>();

	public Guid ActivatedAbilityID;

	public DecoyHologramEmitter()
	{
		ChargeUse = 2;
		WorksOnHolder = true;
		WorksOnWearer = true;
		IsPowerLoadSensitive = true;
	}

	public override bool SameAs(IPart p)
	{
		return false;
	}

	public override void SaveData(SerializationWriter Writer)
	{
		base.SaveData(Writer);
		Writer.WriteGameObjectList(Holograms);
	}

	public override void LoadData(SerializationReader Reader)
	{
		base.LoadData(Reader);
		Reader.ReadGameObjectList(Holograms);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != CheckExistenceSupportEvent.ID && ID != EquippedEvent.ID && ID != GetInventoryActionsEvent.ID && ID != InventoryActionEvent.ID && ID != ObjectCreatedEvent.ID && ID != OnDestroyObjectEvent.ID)
		{
			return ID == UnequippedEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(ObjectCreatedEvent E)
	{
		if (WorksOnSelf)
		{
			SetUpActivatedAbility(ParentObject);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(CheckExistenceSupportEvent E)
	{
		if (HologramActive && Holograms.Contains(E.Object) && IsReady(UseCharge: false, IgnoreCharge: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
		{
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EquippedEvent E)
	{
		DestroyHolograms(null, E.Actor, FromDialog: true);
		E.Actor.RegisterPartEvent(this, "AfterMoved");
		E.Actor.RegisterPartEvent(this, "BeginTakeAction");
		E.Actor.RegisterPartEvent(this, "ToggleHologramEmitter");
		if (ParentObject.Understood())
		{
			SetUpActivatedAbility(E.Actor);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(UnequippedEvent E)
	{
		E.Actor.UnregisterPartEvent(this, "AfterMoved");
		E.Actor.UnregisterPartEvent(this, "BeginTakeAction");
		E.Actor.UnregisterPartEvent(this, "ToggleHologramEmitter");
		E.Actor.RemoveActivatedAbility(ref ActivatedAbilityID);
		if (HologramActive)
		{
			DestroyHolograms(null, E.Actor, FromDialog: true);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(OnDestroyObjectEvent E)
	{
		DestroyHolograms();
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetInventoryActionsEvent E)
	{
		if (HologramActive)
		{
			if (GetActivePartStatus(UseCharge: false, IgnoreCharge: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L) != ActivePartStatus.NeedsSubject)
			{
				E.AddAction("Deactivate", "deactivate", "DeactivateHologramBracelet", null, 'a', FireOnActor: false, 10);
			}
		}
		else if (GetActivePartStatus(UseCharge: false, IgnoreCharge: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L) != ActivePartStatus.NeedsSubject)
		{
			E.AddAction("Activate", "activate", "ActivateHologramBracelet", null, 'a', FireOnActor: false, 10);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(InventoryActionEvent E)
	{
		if (E.Command == "ActivateHologramBracelet")
		{
			ActivateHologramBracelet(E.Actor, E);
		}
		else if (E.Command == "DeactivateHologramBracelet" && HologramActive)
		{
			DestroyHolograms(null, E.Actor, FromDialog: true);
			E.Actor.UseEnergy(1000, "Item Deactivation");
			E.RequestInterfaceExit();
		}
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return false;
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "AIGetDefensiveItemList");
		Object.RegisterPartEvent(this, "BeginTakeAction");
		Object.RegisterPartEvent(this, "BootSequenceInitialized");
		Object.RegisterPartEvent(this, "EffectApplied");
		Object.RegisterPartEvent(this, "ExamineSuccess");
		base.Register(Object);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "BeginTakeAction")
		{
			if (HologramActive)
			{
				int value = (int)Math.Ceiling((double)ChargeUse * (1.0 * (double)Holograms.Count / (double)MaxDecoys));
				if (IsDisabled(UseCharge: true, IgnoreCharge: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, value, UseChargeIfUnpowered: false, 0L))
				{
					DestroyHolograms();
				}
			}
		}
		else if (E.ID == "AIGetDefensiveItemList")
		{
			if (!HologramActive && !IsDisabled(UseCharge: false, IgnoreCharge: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L) && (E.GetIntParameter("Distance") > 1 || Stat.Random(1, 5) == 1))
			{
				E.AddAICommand("ActivateHologramBracelet", 2, ParentObject, Inv: true);
			}
		}
		else if (E.ID == "AfterMoved")
		{
			if (HologramActive)
			{
				for (int i = 0; i < Holograms.Count; i++)
				{
					GameObject gameObject = Holograms[i];
					if (gameObject.IsInvalid())
					{
						DestroyHolograms(gameObject);
					}
					else
					{
						gameObject.Move(Directions.GetOppositeDirection(E.GetStringParameter("Direction")), Forced: true);
					}
				}
			}
		}
		else if (E.ID == "BootSequenceInitialized")
		{
			DestroyHolograms();
		}
		else if (E.ID == "EffectApplied")
		{
			if (HologramActive && IsDisabled(UseCharge: false, IgnoreCharge: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
			{
				DestroyHolograms();
			}
		}
		else if (E.ID == "ToggleHologramEmitter")
		{
			GameObject activePartFirstSubject = GetActivePartFirstSubject();
			if (activePartFirstSubject != null)
			{
				if (HologramActive)
				{
					DestroyHolograms();
					activePartFirstSubject.UseEnergy(1000, "Item Deactivation");
					E.RequestInterfaceExit();
				}
				else
				{
					ActivateHologramBracelet(GetActivePartFirstSubject(), E);
				}
			}
		}
		else if (E.ID == "ExamineSuccess")
		{
			SetUpActivatedAbility(ParentObject.Equipped);
		}
		return base.FireEvent(E);
	}

	public override bool GetActivePartLocallyDefinedFailure()
	{
		if (ParentObject.OnWorldMap())
		{
			return true;
		}
		return false;
	}

	public override string GetActivePartLocallyDefinedFailureDescription()
	{
		return "InvalidContext";
	}

	public void PlaceHologram(Cell C, GameObject Who, int Load)
	{
		GameObject gameObject = GameObject.create("Hologram Distraction");
		Distraction part = gameObject.GetPart<Distraction>();
		gameObject.GetPart<HologramMaterial>();
		gameObject.pRender.Tile = Who.pRender.Tile;
		gameObject.pRender.RenderString = Who.pRender.RenderString;
		gameObject.pRender.DisplayName = Who.pRender.DisplayName;
		if (Who.HasProperName)
		{
			gameObject.SetIntProperty("ProperNoun", 1);
		}
		part.DistractionFor = Who;
		part.DistractionGeneratedBy = ParentObject;
		part.Difficulty = Difficulty + IComponent<GameObject>.PowerLoadBonus(Load);
		gameObject.RequirePart<ExistenceSupport>().SupportedBy = ParentObject;
		string text = Who.a + Who.DisplayNameOnly;
		gameObject.GetPart<Description>().Short = "A holographic image of " + text + ".";
		gameObject.SetStringProperty("HologramOf", text);
		C.AddObject(gameObject);
		Holograms.Add(gameObject);
		IComponent<GameObject>.EmitMessage(gameObject, "An image of " + gameObject.GetStringProperty("HologramOf") + " appears.");
	}

	public ActivePartStatus CreateHolograms(GameObject Who = null)
	{
		if (Holograms.Count > 0)
		{
			DestroyHolograms(null, Who);
		}
		int num = MyPowerLoadLevel();
		ActivePartStatus activePartStatus = GetActivePartStatus(UseCharge: false, IgnoreCharge: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L, num);
		if (activePartStatus != 0)
		{
			return activePartStatus;
		}
		if (Who == null)
		{
			Who = ParentObject.Equipped ?? ParentObject.GetCurrentCell()?.GetCombatObject();
			if (Who == null)
			{
				return ActivePartStatus.LocallyDefinedFailure;
			}
		}
		if (Who.IsPlayer())
		{
			int num2 = 0;
			while (num2 < MaxDecoys)
			{
				Cell cell = Who.pPhysics.PickDestinationCell(10, AllowVis.OnlyVisible, Locked: false);
				if (cell == null)
				{
					break;
				}
				if (Who.DistanceTo(cell) > 10)
				{
					Popup.Show("That is out of range (10 squares)");
					continue;
				}
				PlaceHologram(cell, Who, num);
				num2++;
			}
		}
		else
		{
			List<Cell> list = Who.CurrentCell.GetAdjacentCells(2).ShuffleInPlace();
			for (int i = 0; i < MaxDecoys && i < list.Count; i++)
			{
				if (list[i].IsEmpty())
				{
					PlaceHologram(list[i], Who, num);
				}
			}
		}
		HologramActive = Holograms.Count > 0;
		return activePartStatus;
	}

	private bool ActivateHologramBracelet(GameObject Who, IEvent E = null)
	{
		if (Who.OnWorldMap())
		{
			if (Who.IsPlayer())
			{
				Popup.ShowFail("You cannot do that on the world map.");
			}
			return false;
		}
		if (!HologramActive)
		{
			ActivePartStatus activePartStatus = CreateHolograms(Who);
			if (HologramActive)
			{
				Who.UseEnergy(1000, "Item Activation");
				E?.RequestInterfaceExit();
				SyncActivatedAbilityName(Who);
				return true;
			}
			if (Who.IsPlayer())
			{
				if (activePartStatus == ActivePartStatus.Booting && ParentObject.GetPart<BootSequence>().IsObvious())
				{
					Popup.Show(ParentObject.T() + ParentObject.Is + " still starting up.");
				}
				else
				{
					switch (activePartStatus)
					{
					case ActivePartStatus.Unpowered:
						Popup.Show(ParentObject.T() + ParentObject.GetVerb("do") + " not have enough charge to sustain the hologram.");
						break;
					default:
						Popup.Show(ParentObject.T() + ParentObject.Is + " unresponsive.");
						break;
					case ActivePartStatus.Operational:
						break;
					}
				}
			}
		}
		return false;
	}

	public void DestroyHolograms(GameObject Hologram = null, GameObject Who = null, bool FromDialog = false)
	{
		if (Hologram != null)
		{
			if (!Hologram.IsInvalid())
			{
				IComponent<GameObject>.EmitMessage(Hologram, "An image of " + Hologram.GetStringProperty("HologramOf") + " disappears.", FromDialog);
				Hologram.Destroy();
			}
			Holograms.Remove(Hologram);
		}
		else
		{
			for (int num = Holograms.Count - 1; num >= 0; num--)
			{
				Hologram = Holograms[num];
				if (!Hologram.IsInvalid())
				{
					IComponent<GameObject>.EmitMessage(Hologram, "An image of " + Hologram.GetStringProperty("HologramOf") + " disappears.", FromDialog);
					Hologram.Destroy();
				}
				Holograms.RemoveAt(num);
			}
		}
		HologramActive = Holograms.Count > 0;
		SyncActivatedAbilityName(Who);
	}

	public void SetUpActivatedAbility(GameObject Who)
	{
		if (Who != null)
		{
			ActivatedAbilityID = Who.AddActivatedAbility(GetActivatedAbilityName(Who), "ToggleHologramEmitter", (Who == ParentObject) ? "Maneuvers" : "Items", null, "\u0001");
		}
	}

	public string GetActivatedAbilityName(GameObject Who = null)
	{
		if (Who == null)
		{
			Who = ParentObject.Equipped ?? ParentObject;
		}
		StringBuilder stringBuilder = Event.NewStringBuilder();
		stringBuilder.Append(HologramActive ? "Deactivate" : "Activate").Append(' ').Append((Who == null || Who == ParentObject) ? "Holographic Decoy" : Grammar.MakeTitleCase(ParentObject.BaseDisplayNameStripped));
		return stringBuilder.ToString();
	}

	public void SyncActivatedAbilityName(GameObject Who = null)
	{
		if (!(ActivatedAbilityID == Guid.Empty))
		{
			if (Who == null)
			{
				Who = ParentObject.Equipped ?? ParentObject;
			}
			Who.SetActivatedAbilityDisplayName(ActivatedAbilityID, GetActivatedAbilityName(Who));
		}
	}
}
