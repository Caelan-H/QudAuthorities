using System;
using System.Collections.Generic;
using System.Text;
using XRL.Language;
using XRL.Messages;
using XRL.UI;

namespace XRL.World.Parts;

[Serializable]
public class MagazineAmmoLoader : IPoweredPart
{
	public int MaxAmmo = 6;

	public string ID = "";

	public GameObject Ammo;

	public int ReloadEnergy = 1000;

	public string AmmoPart = "";

	public string ProjectileObject;

	[NonSerialized]
	private static Dictionary<string, List<GameObjectBlueprint>> AmmoBlueprints = new Dictionary<string, List<GameObjectBlueprint>>();

	public new int ChargeUse
	{
		get
		{
			return 0;
		}
		set
		{
			throw new Exception("cannot set ChargeUse on a MagazineAmmoLoader");
		}
	}

	public override IPart DeepCopy(GameObject Parent, Func<GameObject, GameObject> MapInv)
	{
		MagazineAmmoLoader magazineAmmoLoader = new MagazineAmmoLoader();
		magazineAmmoLoader.MaxAmmo = MaxAmmo;
		magazineAmmoLoader.ID = ID;
		if (Ammo != null)
		{
			magazineAmmoLoader.Ammo = MapInv?.Invoke(Ammo) ?? Ammo.DeepCopy(CopyEffects: false, CopyID: false, MapInv);
			if (magazineAmmoLoader.Ammo != null)
			{
				magazineAmmoLoader.Ammo.ForeachPartDescendedFrom(delegate(IAmmo p)
				{
					p.LoadedIn = Parent;
				});
			}
		}
		magazineAmmoLoader.ReloadEnergy = ReloadEnergy;
		magazineAmmoLoader.AmmoPart = AmmoPart;
		magazineAmmoLoader.ProjectileObject = ProjectileObject;
		magazineAmmoLoader.ParentObject = Parent;
		return magazineAmmoLoader;
	}

	public MagazineAmmoLoader()
	{
		base.ChargeUse = 0;
		IsBootSensitive = false;
		IsEMPSensitive = false;
		IsPowerSwitchSensitive = false;
		NameForStatus = "FiringMechanism";
		WorksOnEquipper = true;
	}

	public override bool SameAs(IPart p)
	{
		MagazineAmmoLoader magazineAmmoLoader = p as MagazineAmmoLoader;
		if (magazineAmmoLoader.MaxAmmo != MaxAmmo)
		{
			return false;
		}
		if (magazineAmmoLoader.ID != ID)
		{
			return false;
		}
		if (magazineAmmoLoader.Ammo != Ammo)
		{
			return false;
		}
		if (magazineAmmoLoader.ReloadEnergy != ReloadEnergy)
		{
			return false;
		}
		if (magazineAmmoLoader.AmmoPart != AmmoPart)
		{
			return false;
		}
		if (magazineAmmoLoader.ProjectileObject != ProjectileObject)
		{
			return false;
		}
		return base.SameAs(p);
	}

	public void SetAmmo(GameObject obj)
	{
		if (obj == Ammo)
		{
			return;
		}
		if (Ammo != null)
		{
			Ammo.ForeachPartDescendedFrom(delegate(IAmmo p)
			{
				p.LoadedIn = null;
			});
		}
		Ammo = obj;
		obj?.ForeachPartDescendedFrom(delegate(IAmmo p)
		{
			p.LoadedIn = ParentObject;
		});
		FlushWantTurnTickCache();
	}

	private bool AmmoWantsEvent(int ID, int cascade)
	{
		if (!GameObject.validate(ref Ammo))
		{
			return false;
		}
		if (!MinEvent.CascadeTo(cascade, 4))
		{
			return false;
		}
		return Ammo.WantEvent(ID, cascade);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != CommandReloadEvent.ID && ID != GetContentsEvent.ID && ID != GetDisplayNameEvent.ID && ID != GetExtrinsicValueEvent.ID && ID != GetExtrinsicWeightEvent.ID && ID != GetInventoryActionsEvent.ID && ID != GetMissileWeaponProjectileEvent.ID && ID != GetProjectileBlueprintEvent.ID && ID != InventoryActionEvent.ID && ID != NeedsReloadEvent.ID && ID != StripContentsEvent.ID)
		{
			return AmmoWantsEvent(ID, cascade);
		}
		return true;
	}

	public override bool HandleEvent(GetProjectileBlueprintEvent E)
	{
		if (!string.IsNullOrEmpty(ProjectileObject))
		{
			E.Blueprint = ProjectileObject;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(NeedsReloadEvent E)
	{
		if (E.Skip != this && (E.Weapon == null || E.Weapon == ParentObject) && (Ammo == null || Ammo.Count < MaxAmmo) && ParentObject.IsEquippedProperly())
		{
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(CommandReloadEvent E)
	{
		if (E.Pass >= 2 && (E.Weapon == null || E.Weapon == ParentObject) && !E.CheckedForReload.Contains(this))
		{
			E.CheckedForReload.Add(this);
			if (E.Weapon == null && !ParentObject.IsEquippedProperly())
			{
				return true;
			}
			if (E.MinimumCharge > 0)
			{
				return true;
			}
			bool flag = Ammo == null || Ammo.Count < MaxAmmo;
			if (flag)
			{
				E.NeededReload.Add(this);
			}
			if (flag || (E.NeededReload.Count <= 0 && !NeedsReloadEvent.Check(E.Actor, this)))
			{
				List<GameObject> list = Event.NewGameObjectList();
				foreach (GameObject item in E.Actor.GetInventory())
				{
					if (item.HasPart(AmmoPart) || item.HasTag(AmmoPart))
					{
						list.Add(item);
					}
				}
				if (list.Count == 0)
				{
					if (E.Actor.IsPlayer())
					{
						IComponent<GameObject>.EmitMessage(E.Actor, "You have no more ammo for " + ParentObject.t() + ".", E.FromDialog);
					}
					return true;
				}
				if (!flag && list.Count == 1 && Ammo != null && Ammo.Blueprint == list[0].Blueprint)
				{
					if (E.Actor.IsPlayer())
					{
						IComponent<GameObject>.EmitMessage(E.Actor, ParentObject.Does("are") + " already fully loaded.", E.FromDialog);
					}
					return true;
				}
				GameObject gameObject = null;
				if (E.LastAmmo != null)
				{
					for (int i = 0; i < list.Count; i++)
					{
						if (list[i].Blueprint == E.LastAmmo.Blueprint)
						{
							gameObject = list[i];
							break;
						}
					}
					if (gameObject == null)
					{
						return false;
					}
				}
				if (gameObject == null)
				{
					if (list.Count > 1)
					{
						if (E.Actor.IsPlayer())
						{
							gameObject = PickItem.ShowPicker(list, null, PickItem.PickItemDialogStyle.SelectItemDialog, E.Actor);
							if (gameObject == null)
							{
								goto IL_0371;
							}
						}
						else
						{
							List<GameObject> list2 = Event.NewGameObjectList();
							foreach (GameObject item2 in list)
							{
								if (!item2.IsImportant())
								{
									int j = 0;
									for (int num = Math.Min(item2.Count, 10); j < num; j++)
									{
										list2.Add(item2);
									}
								}
							}
							gameObject = list2.GetRandomElement();
						}
					}
					else
					{
						gameObject = list[0];
					}
				}
				if (!gameObject.ConfirmUseImportant(E.Actor, "load"))
				{
					return true;
				}
				E.TriedToReload.Add(this);
				Unload(E.Actor);
				if (Load(E.Actor, gameObject, E.FromDialog))
				{
					E.Reloaded.Add(this);
					if (!E.ObjectsReloaded.Contains(ParentObject))
					{
						E.ObjectsReloaded.Add(ParentObject);
					}
					E.EnergyCost(ReloadEnergy);
				}
			}
		}
		goto IL_0371;
		IL_0371:
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(MinEvent E)
	{
		if (!base.HandleEvent(E))
		{
			return false;
		}
		if (E.CascadeTo(4) && GameObject.validate(ref Ammo) && !Ammo.HandleEvent(E))
		{
			return false;
		}
		return true;
	}

	public override bool HandleEvent(GetMissileWeaponProjectileEvent E)
	{
		if (!string.IsNullOrEmpty(ProjectileObject))
		{
			E.Blueprint = ProjectileObject;
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetExtrinsicValueEvent E)
	{
		if (GameObject.validate(ref Ammo))
		{
			E.Value += Ammo.Value;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetExtrinsicWeightEvent E)
	{
		if (GameObject.validate(ref Ammo))
		{
			E.Weight += Ammo.Weight;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(StripContentsEvent E)
	{
		if (GameObject.validate(ref Ammo) && (!E.KeepNatural || !Ammo.IsNatural()))
		{
			Ammo.Obliterate();
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetContentsEvent E)
	{
		if (GameObject.validate(ref Ammo))
		{
			E.Objects.Add(Ammo);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetInventoryActionsEvent E)
	{
		E.AddAction("Load Ammo", "load", "LoadMagazineAmmo", null, 'o');
		if (GameObject.validate(ref Ammo))
		{
			E.AddAction("Unload Ammo", "unload", "UnloadMagazineAmmo", null, 'u');
			GetSlottedInventoryActionsEvent.Send(Ammo, E);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(InventoryActionEvent E)
	{
		if (E.Command == "LoadMagazineAmmo")
		{
			CommandReloadEvent.Execute(E.Actor, ParentObject, null, FreeAction: false, FromDialog: true);
		}
		else if ((E.Command == "UnloadMagazineAmmo" || E.Command == "EmptyForDisassemble") && GameObject.validate(ref Ammo))
		{
			ParentObject.GetContext(out var ObjectContext, out var CellContext);
			if (ObjectContext != null)
			{
				ObjectContext.TakeObject(Ammo, Silent: false, 0);
			}
			else if (CellContext != null)
			{
				CellContext.AddObject(Ammo);
			}
			else
			{
				E.Actor.TakeObject(Ammo, Silent: false, 0);
			}
			SetAmmo(null);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetDisplayNameEvent E)
	{
		if (E.Understood())
		{
			if (E.Cutoff >= 1100)
			{
				E.AddMissileWeaponDamageTag(GetMissileWeaponPerformanceEvent.GetFor(ParentObject.Equipped, ParentObject, (Ammo != null && string.IsNullOrEmpty(ProjectileObject)) ? GetProjectileObjectEvent.GetFor(Launcher: ParentObject, Ammo: Ammo) : null));
			}
			if (E.Context != "Tinkering")
			{
				if (GameObject.validate(ref Ammo))
				{
					E.AddTag("{{y|[" + Ammo.DisplayNameOnly + "]}}", -5);
				}
				else
				{
					E.AddTag("{{y|[{{K|empty}}]}}", -5);
				}
			}
		}
		return base.HandleEvent(E);
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "AIWantUseWeapon");
		Object.RegisterPartEvent(this, "CheckReadyToFire");
		Object.RegisterPartEvent(this, "GenerateIntegratedHostInitialAmmo");
		Object.RegisterPartEvent(this, "GetMissileWeaponStatus");
		Object.RegisterPartEvent(this, "LoadAmmo");
		Object.RegisterPartEvent(this, "LoadSpecificAmmo");
		Object.RegisterPartEvent(this, "ShotComplete");
		Object.RegisterPartEvent(this, "SupplyIntegratedHostWithAmmo");
		base.Register(Object);
	}

	public override bool WantTurnTick()
	{
		if (GameObject.validate(ref Ammo))
		{
			return Ammo.WantTurnTick();
		}
		return false;
	}

	public override bool WantTenTurnTick()
	{
		if (GameObject.validate(ref Ammo))
		{
			return Ammo.WantTenTurnTick();
		}
		return false;
	}

	public override bool WantHundredTurnTick()
	{
		if (GameObject.validate(ref Ammo))
		{
			return Ammo.WantTenTurnTick();
		}
		return false;
	}

	public override void TurnTick(long TurnNumber)
	{
		if (GameObject.validate(ref Ammo) && Ammo.WantTurnTick())
		{
			Ammo.TurnTick(TurnNumber);
		}
	}

	public override void TenTurnTick(long TurnNumber)
	{
		if (GameObject.validate(ref Ammo) && Ammo.WantTenTurnTick())
		{
			Ammo.TenTurnTick(TurnNumber);
		}
	}

	public override void HundredTurnTick(long TurnNumber)
	{
		if (GameObject.validate(ref Ammo) && Ammo.WantHundredTurnTick())
		{
			Ammo.HundredTurnTick(TurnNumber);
		}
	}

	public void Unload(GameObject Loader)
	{
		try
		{
			ParentObject.SplitFromStack();
			MessageQueue.Suppress = true;
			InventoryActionEvent.Check(ParentObject, Loader, ParentObject, "UnloadMagazineAmmo");
			MessageQueue.Suppress = false;
		}
		catch
		{
			MessageQueue.Suppress = false;
		}
	}

	public bool Load(GameObject Loader, GameObject ChosenAmmo, bool FromDialog = false)
	{
		try
		{
			ParentObject.SplitFromStack();
			MessageQueue.Suppress = true;
			ChosenAmmo.SplitStack(MaxAmmo, Loader);
			Event @event = Event.New("CommandRemoveObject");
			@event.SetParameter("Object", ChosenAmmo);
			@event.SetFlag("ForEquip", State: true);
			if (Loader.FireEvent(@event))
			{
				SetAmmo(ChosenAmmo);
				ParentObject.FireEvent("MagazineAmmoLoaderReloaded");
				MessageQueue.Suppress = false;
				PlayWorldSound(ParentObject.GetPropertyOrTag("ReloadSound"));
				if (Loader.IsPlayer())
				{
					IComponent<GameObject>.EmitMessage(Loader, "You reload " + ParentObject.t() + " with " + ((Ammo.Count == 1) ? Ammo.an() : Ammo.ShortDisplayName) + ".", FromDialog);
				}
				return true;
			}
			return false;
		}
		catch (Exception message)
		{
			MetricsManager.LogError(message);
			return false;
		}
		finally
		{
			MessageQueue.Suppress = false;
		}
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "ShotComplete")
		{
			if (ReloadEnergy == 0 && ParentObject.Equipped != null && ParentObject.Equipped.CanMoveExtremities(null, ShowMessage: false, Involuntary: false, AllowTelekinetic: true))
			{
				CommandReloadEvent.Execute(ParentObject.Equipped, ParentObject, E.GetGameObjectParameter("AmmoObject"));
			}
		}
		else if (E.ID == "CheckReadyToFire")
		{
			if (Ammo == null || IsDisabled(UseCharge: false, IgnoreCharge: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
			{
				return false;
			}
		}
		else if (E.ID == "AIWantUseWeapon")
		{
			if (IsDisabled(UseCharge: false, IgnoreCharge: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
			{
				return false;
			}
			if (Ammo == null)
			{
				Inventory inventory = E.GetParameter<GameObject>("Object").Inventory;
				bool flag = false;
				foreach (GameObject item in inventory.GetObjectsDirect())
				{
					if (item.HasPart(AmmoPart) || item.HasTag(AmmoPart))
					{
						flag = true;
						break;
					}
				}
				if (!flag)
				{
					return false;
				}
			}
		}
		else if (E.ID == "GetMissileWeaponStatus")
		{
			if (!E.HasParameter("Override"))
			{
				StringBuilder parameter = E.GetParameter<StringBuilder>("Items");
				if (Ammo == null)
				{
					parameter.Append(" [{{K|empty}}]");
				}
				else
				{
					int count = Ammo.Count;
					string @for = GetMissileStatusColorEvent.GetFor(Ammo);
					parameter.Append(" [");
					if (!string.IsNullOrEmpty(@for))
					{
						parameter.Append("{{").Append(@for).Append('|');
					}
					parameter.Append(count);
					if (!string.IsNullOrEmpty(@for))
					{
						parameter.Append("}}");
					}
					parameter.Append("]");
				}
			}
		}
		else if (E.ID == "CheckLoadAmmo")
		{
			GameObject gameObjectParameter = E.GetGameObjectParameter("Loader");
			if (Ammo == null)
			{
				if (gameObjectParameter.IsPlayer())
				{
					IComponent<GameObject>.AddPlayerMessage(ParentObject.Does("have") + " no more ammo!", 'r');
				}
				return false;
			}
		}
		else if (E.ID == "LoadAmmo")
		{
			GameObject gameObjectParameter2 = E.GetGameObjectParameter("Loader");
			if (Ammo == null)
			{
				if (gameObjectParameter2.IsPlayer())
				{
					IComponent<GameObject>.AddPlayerMessage(ParentObject.Does("have") + " no more ammo!", 'r');
				}
				E.SetParameter("Ammo", null);
				return false;
			}
			if (!IsReady(UseCharge: false, IgnoreCharge: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
			{
				if (gameObjectParameter2.IsPlayer())
				{
					IComponent<GameObject>.AddPlayerMessage(ParentObject.Does("are") + " jammed!", 'r');
				}
				E.SetParameter("Ammo", null);
				return false;
			}
			GameObject gameObject = Ammo.RemoveOne();
			if (gameObject == Ammo)
			{
				SetAmmo(null);
			}
			if (string.IsNullOrEmpty(ProjectileObject))
			{
				E.SetParameter("Ammo", GetProjectileObjectEvent.GetFor(gameObject, ParentObject));
			}
			else
			{
				E.SetParameter("Ammo", GameObject.create(ProjectileObject));
			}
			E.SetParameter("AmmoObject", gameObject);
		}
		else if (E.ID == "SupplyIntegratedHostWithAmmo")
		{
			if (E.HasFlag("TrackSupply"))
			{
				E.SetFlag("AnySupplyHandler", State: true);
			}
			GameObject gameObjectParameter3 = E.GetGameObjectParameter("Host");
			GameObject gameObject2 = E.GetGameObjectParameter("Actor") ?? E.GetGameObjectParameter("Owner");
			if (gameObjectParameter3 != null && gameObject2 != null)
			{
				int desiredAmmoCount = GetDesiredAmmoCount();
				int num = GetAccessibleAmmoCount();
				Inventory inventory2 = gameObject2.Inventory;
				if (inventory2 != null)
				{
					List<GameObject> list = Event.NewGameObjectList();
					inventory2.GetObjects(list, ObjectHasAmmoPart);
					foreach (GameObject item2 in list)
					{
						int count2 = item2.Count;
						int num2 = 0;
						if (gameObject2.IsPlayer())
						{
							if (E.HasFlag("TrackSupply"))
							{
								E.SetFlag("AnySupplies", State: true);
							}
							Math.Min(desiredAmmoCount - num, count2);
							int? num3 = Popup.AskNumber("Supply " + gameObjectParameter3.t() + " with how many " + (item2.HasProperName ? ("of " + item2.DisplayNameSingle) : Grammar.Pluralize(item2.DisplayNameSingle)) + "? (max=" + count2 + ")", count2, 0, count2);
							int num4 = 0;
							try
							{
								num4 = Convert.ToInt32(num3);
							}
							catch
							{
								break;
							}
							if (num4 > count2)
							{
								num4 = count2;
							}
							if (num4 < 0)
							{
								num4 = 0;
							}
							num2 = num4;
						}
						else if (desiredAmmoCount > num)
						{
							num2 = Math.Min(desiredAmmoCount - num, count2);
						}
						if (num2 > 0)
						{
							IComponent<GameObject>.XDidYToZ(gameObject2, "transfer", item2.HasProperName ? item2.DisplayNameOnly : (((num2 == 1) ? item2.an(int.MaxValue, null, null, AsIfKnown: false, Single: true) : (Grammar.Cardinal(num2) + " " + Grammar.Pluralize(item2.ShortDisplayNameSingle))) + " to"), gameObjectParameter3, null, null, null, null, null, UseFullNames: false, IndefiniteSubject: false, IndefiniteObject: false, IndefiniteObjectForOthers: true, PossessiveObject: false, null, null, null, DescribeSubjectDirection: false, DescribeSubjectDirectionLate: false, AlwaysVisible: false, FromDialog: true);
							if (num2 < count2)
							{
								item2.Split(num2);
							}
							gameObjectParameter3.ReceiveObject(item2);
							gameObject2.UseEnergy(1000, "Ammo Magazine Transfer");
							num += num2;
						}
					}
				}
			}
		}
		else if (E.ID == "GenerateIntegratedHostInitialAmmo")
		{
			GameObject gameObjectParameter4 = E.GetGameObjectParameter("Host");
			if (gameObjectParameter4 != null && gameObjectParameter4.Inventory != null)
			{
				int desiredAmmoCount2 = GetDesiredAmmoCount();
				int num5 = GetAccessibleAmmoCount();
				if (num5 < desiredAmmoCount2)
				{
					List<GameObjectBlueprint> ammoBlueprints = GetAmmoBlueprints();
					if (ammoBlueprints.Count == 1)
					{
						num5 += gameObjectParameter4.TakeObject(ammoBlueprints[0].Name, desiredAmmoCount2 - num5, Silent: true, 0);
					}
					else if (ammoBlueprints.Count > 1)
					{
						int num6 = (desiredAmmoCount2 - num5) / ammoBlueprints.Count;
						if (num6 > 0)
						{
							foreach (GameObjectBlueprint item3 in ammoBlueprints)
							{
								num5 += gameObjectParameter4.TakeObject(item3.Name, num6, Silent: true, 0);
							}
						}
						if (num5 < desiredAmmoCount2)
						{
							int num7 = 0;
							int num8 = desiredAmmoCount2 - num5 + 100;
							while (num5 < desiredAmmoCount2 && ++num7 < num8)
							{
								if (gameObjectParameter4.ReceiveObject(ammoBlueprints.GetRandomElement().Name))
								{
									num5++;
								}
							}
						}
					}
				}
			}
		}
		return base.FireEvent(E);
	}

	private bool ObjectHasAmmoPart(GameObject obj)
	{
		return obj.HasPart(AmmoPart);
	}

	private int GetDesiredAmmoCount()
	{
		MissileWeapon part = ParentObject.GetPart<MissileWeapon>();
		int num = ParentObject.GetIntProperty("IntegratedWeaponHostShots");
		if (num <= 0)
		{
			num = ((MaxAmmo == 1) ? 50 : ((MaxAmmo != 2) ? 200 : 100));
		}
		return part.AmmoPerAction * num;
	}

	private int GetAccessibleAmmoCount()
	{
		int num = ((Ammo != null) ? Ammo.Count : 0);
		GameObject gameObject = ParentObject.Equipped ?? ParentObject.InInventory;
		if (gameObject != null)
		{
			Inventory inventory = gameObject.Inventory;
			if (inventory != null)
			{
				foreach (GameObject @object in inventory.Objects)
				{
					if (@object.HasPart(AmmoPart))
					{
						num += @object.Count;
					}
				}
				return num;
			}
		}
		return num;
	}

	private static List<GameObjectBlueprint> GetAmmoBlueprints(string ForAmmoPart)
	{
		if (!AmmoBlueprints.ContainsKey(ForAmmoPart))
		{
			bool flag = false;
			List<GameObjectBlueprint> list = new List<GameObjectBlueprint>();
			{
				foreach (GameObjectBlueprint value in GameObjectFactory.Factory.Blueprints.Values)
				{
					if (!value.HasPart(ForAmmoPart) || value.HasTag("ExcludeFromDynamicEncounters") || value.HasTag("BaseObject") || value.HasTag("ExcludeFromTurretStock"))
					{
						continue;
					}
					if (flag)
					{
						if (value.HasTag("TurretStockExclusive"))
						{
							AddAmmoBlueprint(list, value);
						}
					}
					else if (value.HasTag("TurretStockExclusive"))
					{
						list.Clear();
						AddAmmoBlueprint(list, value);
						flag = true;
					}
					else
					{
						AddAmmoBlueprint(list, value);
					}
				}
				return list;
			}
		}
		return AmmoBlueprints[ForAmmoPart];
	}

	private static void AddAmmoBlueprint(List<GameObjectBlueprint> List, GameObjectBlueprint BP)
	{
		string tag = BP.GetTag("TurretStockWeight");
		if (string.IsNullOrEmpty(tag))
		{
			List.Add(BP);
			return;
		}
		try
		{
			int num = Convert.ToInt32(tag);
			for (int i = 0; i < num; i++)
			{
				List.Add(BP);
			}
		}
		catch
		{
		}
	}

	private List<GameObjectBlueprint> GetAmmoBlueprints()
	{
		return GetAmmoBlueprints(AmmoPart);
	}
}
