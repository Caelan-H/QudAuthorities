using System;
using System.Text;
using XRL.Language;
using XRL.UI;

namespace XRL.World.Parts;

[Serializable]
public class LiquidAmmoLoader : IActivePart
{
	public string ID = "";

	public string Liquid = "water";

	public int ReloadEnergy = 1000;

	public string ProjectileObject;

	public bool ShowDamage;

	public int ShotsPerDram = 1;

	public int ShotsTakenOnCurrentDram;

	public LiquidAmmoLoader()
	{
		WorksOnEquipper = true;
		base.IsTechScannable = true;
		NameForStatus = "LiquidProjector";
	}

	public override bool SameAs(IPart p)
	{
		LiquidAmmoLoader liquidAmmoLoader = p as LiquidAmmoLoader;
		if (liquidAmmoLoader.ID != ID)
		{
			return false;
		}
		if (liquidAmmoLoader.Liquid != Liquid)
		{
			return false;
		}
		if (liquidAmmoLoader.ReloadEnergy != ReloadEnergy)
		{
			return false;
		}
		if (liquidAmmoLoader.ProjectileObject != ProjectileObject)
		{
			return false;
		}
		if (liquidAmmoLoader.ShowDamage != ShowDamage)
		{
			return false;
		}
		if (liquidAmmoLoader.ShotsPerDram != ShotsPerDram)
		{
			return false;
		}
		if (liquidAmmoLoader.ShotsTakenOnCurrentDram != ShotsTakenOnCurrentDram)
		{
			return false;
		}
		return base.SameAs(p);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != AllowLiquidCollectionEvent.ID && ID != CommandReloadEvent.ID && (ID != GetDisplayNameEvent.ID || !ShowDamage) && ID != GetMissileWeaponProjectileEvent.ID && ID != GetPreferredLiquidEvent.ID && ID != GetProjectileBlueprintEvent.ID && ID != NeedsReloadEvent.ID)
		{
			return ID == WantsLiquidCollectionEvent.ID;
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
		if (E.Skip != this && (E.Weapon == null || E.Weapon == ParentObject))
		{
			LiquidVolume liquidVolume = ParentObject.LiquidVolume;
			if (liquidVolume != null && (!liquidVolume.IsPureLiquid(Liquid) || liquidVolume.Volume < liquidVolume.MaxVolume) && ParentObject.IsEquippedProperly())
			{
				return false;
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(CommandReloadEvent E)
	{
		if ((E.Weapon == null || E.Weapon == ParentObject) && !E.CheckedForReload.Contains(this))
		{
			E.CheckedForReload.Add(this);
			if (!ParentObject.IsEquippedProperly() || E.MinimumCharge > 0)
			{
				return true;
			}
			LiquidVolume liquidVolume = ParentObject.LiquidVolume;
			if (liquidVolume.IsPureLiquid(Liquid) && liquidVolume.Volume >= liquidVolume.MaxVolume)
			{
				if (E.Actor.IsPlayer())
				{
					IComponent<GameObject>.AddPlayerMessage(ParentObject.The + ParentObject.ShortDisplayName + ParentObject.Is + " already full of " + liquidVolume.GetLiquidName() + ".");
				}
				return true;
			}
			E.NeededReload.Add(this);
			int freeDrams = E.Actor.GetFreeDrams(Liquid, ParentObject);
			if (freeDrams <= 0)
			{
				if (E.Actor.IsPlayer())
				{
					IComponent<GameObject>.AddPlayerMessage("You have no " + Liquid + " for " + ParentObject.the + ParentObject.ShortDisplayName + ".", 'r');
				}
				return true;
			}
			E.TriedToReload.Add(this);
			string shortDisplayName = ParentObject.ShortDisplayName;
			if (liquidVolume.Volume > 0 && !liquidVolume.IsPureLiquid(Liquid))
			{
				if (E.Actor.IsPlayer())
				{
					IComponent<GameObject>.AddPlayerMessage("You dump the " + liquidVolume.GetLiquidName() + " out of " + ParentObject.the + shortDisplayName + ".");
				}
				liquidVolume.EmptyIntoCell();
				shortDisplayName = ParentObject.ShortDisplayName;
			}
			int val = liquidVolume.MaxVolume - liquidVolume.Volume;
			int num = Math.Min(freeDrams, val);
			E.Actor.UseDrams(num, Liquid, ParentObject);
			liquidVolume.MixWith(new LiquidVolume(Liquid, num));
			E.Reloaded.Add(this);
			if (!E.ObjectsReloaded.Contains(ParentObject))
			{
				E.ObjectsReloaded.Add(ParentObject);
			}
			E.EnergyCost(ReloadEnergy);
			PlayWorldSound(ParentObject.GetTag("ReloadSound"));
			if (E.Actor.IsPlayer())
			{
				IComponent<GameObject>.AddPlayerMessage("You " + ((liquidVolume.Volume < liquidVolume.MaxVolume) ? "partially " : "") + "fill " + ParentObject.the + shortDisplayName + " with " + liquidVolume.GetLiquidName() + ".");
			}
		}
		return base.HandleEvent(E);
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

	public override bool HandleEvent(GetDisplayNameEvent E)
	{
		if (ShowDamage && E.Understood())
		{
			E.AddMissileWeaponDamageTag(GetMissileWeaponPerformanceEvent.GetFor(ParentObject.Equipped, ParentObject));
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(AllowLiquidCollectionEvent E)
	{
		if (!IsLiquidCollectionCompatible(E.Liquid))
		{
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetPreferredLiquidEvent E)
	{
		if (E.Liquid == null)
		{
			E.Liquid = Liquid;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(WantsLiquidCollectionEvent E)
	{
		if (IsLiquidCollectionCompatible(E.Liquid))
		{
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "AIWantUseWeapon");
		Object.RegisterPartEvent(this, "CheckReadyToFire");
		Object.RegisterPartEvent(this, "GenerateIntegratedHostInitialAmmo");
		Object.RegisterPartEvent(this, "GetMissileWeaponStatus");
		Object.RegisterPartEvent(this, "GetNotReadyToFireMessage");
		Object.RegisterPartEvent(this, "LoadAmmo");
		Object.RegisterPartEvent(this, "PrepIntegratedHostToReceiveAmmo");
		Object.RegisterPartEvent(this, "SupplyIntegratedHostWithAmmo");
		base.Register(Object);
	}

	public string GetBadFuelMessage()
	{
		LiquidVolume liquidVolume = ParentObject.LiquidVolume;
		if (!liquidVolume.IsPureLiquid(Liquid))
		{
			return ParentObject.The + ParentObject.ShortDisplayName + ParentObject.Is + " not loaded with the correct liquid.";
		}
		if (liquidVolume.Volume <= 0)
		{
			return ParentObject.The + ParentObject.ShortDisplayName + ParentObject.Is + " empty.";
		}
		return ParentObject.The + ParentObject.ShortDisplayName + ParentObject.Is + " dysfunctional.";
	}

	public bool IsLiquidCollectionCompatible(string LiquidType)
	{
		return Liquid == LiquidType;
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "CheckReadyToFire")
		{
			LiquidVolume liquidVolume = ParentObject.LiquidVolume;
			if (!liquidVolume.IsPureLiquid(Liquid))
			{
				return false;
			}
			if (liquidVolume.Volume <= 0)
			{
				return false;
			}
		}
		else if (E.ID == "GetNotReadyToFireMessage")
		{
			LiquidVolume liquidVolume2 = ParentObject.LiquidVolume;
			if (!liquidVolume2.IsPureLiquid(Liquid) || liquidVolume2.Volume <= 0)
			{
				E.SetParameter("Message", GetBadFuelMessage());
			}
		}
		else if (E.ID == "AIWantUseWeapon")
		{
			LiquidVolume liquidVolume3 = ParentObject.LiquidVolume;
			if (liquidVolume3.IsPureLiquid(Liquid) && liquidVolume3.Volume > 0)
			{
				return true;
			}
			Inventory inventory = E.GetGameObjectParameter("Object").Inventory;
			bool flag = false;
			if (inventory != null)
			{
				foreach (GameObject item in inventory.GetObjectsDirect())
				{
					LiquidVolume liquidVolume4 = item.LiquidVolume;
					if (liquidVolume4 != null && liquidVolume4.IsPureLiquid(Liquid) && liquidVolume4.Volume > 0)
					{
						flag = true;
						break;
					}
				}
			}
			if (!flag)
			{
				return false;
			}
		}
		else if (E.ID == "GetMissileWeaponStatus")
		{
			if (!E.HasParameter("Override"))
			{
				StringBuilder stringBuilder = E.GetParameter("Items") as StringBuilder;
				LiquidVolume liquidVolume5 = ParentObject.LiquidVolume;
				string primaryLiquidColor = liquidVolume5.GetPrimaryLiquidColor();
				if (liquidVolume5.Volume == 0)
				{
					if (primaryLiquidColor != null)
					{
						stringBuilder.Append(" [{{").Append(primaryLiquidColor).Append('|')
							.Append("empty")
							.Append("}}]");
					}
					else
					{
						stringBuilder.Append(" [empty]");
					}
				}
				else if (liquidVolume5.IsPureLiquid(Liquid))
				{
					if (primaryLiquidColor != null)
					{
						stringBuilder.Append(" [{{").Append(primaryLiquidColor).Append('|')
							.Append(liquidVolume5.Volume)
							.Append("}}]");
					}
					else
					{
						stringBuilder.Append(" [").Append(liquidVolume5.Volume).Append("]");
					}
				}
				else if (primaryLiquidColor != null)
				{
					stringBuilder.Append(" [{{").Append(primaryLiquidColor).Append('|')
						.Append("?")
						.Append("}}]");
				}
				else
				{
					stringBuilder.Append(" [?]");
				}
			}
		}
		else if (E.ID == "CheckLoadAmmo")
		{
			GameObject gameObjectParameter = E.GetGameObjectParameter("Loader");
			LiquidVolume liquidVolume6 = ParentObject.LiquidVolume;
			if (!liquidVolume6.IsPureLiquid(Liquid))
			{
				if (gameObjectParameter.IsPlayer())
				{
					IComponent<GameObject>.AddPlayerMessage(ParentObject.The + ParentObject.ShortDisplayName + ParentObject.Is + " not loaded with the correct liquid.", 'r');
				}
				return false;
			}
			if (liquidVolume6.Volume <= 0)
			{
				if (gameObjectParameter.IsPlayer())
				{
					IComponent<GameObject>.AddPlayerMessage(ParentObject.The + ParentObject.ShortDisplayName + ParentObject.Is + " empty.", 'r');
				}
				return false;
			}
		}
		else if (E.ID == "LoadAmmo")
		{
			GameObject gameObjectParameter2 = E.GetGameObjectParameter("Loader");
			LiquidVolume liquidVolume7 = ParentObject.LiquidVolume;
			if (!liquidVolume7.IsPureLiquid(Liquid))
			{
				if (gameObjectParameter2.IsPlayer())
				{
					IComponent<GameObject>.AddPlayerMessage(ParentObject.The + ParentObject.ShortDisplayName + ParentObject.Is + " not loaded with the correct liquid.", 'r');
				}
				return false;
			}
			if (liquidVolume7.Volume <= 0)
			{
				if (gameObjectParameter2.IsPlayer())
				{
					IComponent<GameObject>.AddPlayerMessage(ParentObject.The + ParentObject.ShortDisplayName + ParentObject.Is + " empty.", 'r');
				}
				return false;
			}
			if (++ShotsTakenOnCurrentDram >= ShotsPerDram)
			{
				liquidVolume7.UseDram();
				ShotsTakenOnCurrentDram = 0;
			}
			if (ProjectileObject != null)
			{
				E.SetParameter("Ammo", GameObject.create(ProjectileObject));
			}
		}
		else if (E.ID == "PrepIntegratedHostToReceiveAmmo")
		{
			GameObject gameObjectParameter3 = E.GetGameObjectParameter("Host");
			if (gameObjectParameter3 != null)
			{
				Body body = gameObjectParameter3.Body;
				if (body != null)
				{
					foreach (BodyPart item2 in body.GetPart("Arm"))
					{
						if (item2.Equipped == null)
						{
							gameObjectParameter3.ForceEquipObject(GameObject.create("StorageTank"), item2, Silent: true, 0);
						}
					}
				}
			}
		}
		else if (E.ID == "SupplyIntegratedHostWithAmmo")
		{
			if (E.HasFlag("TrackSupply"))
			{
				E.SetFlag("AnySupplyHandler", State: true);
			}
			GameObject gameObjectParameter4 = E.GetGameObjectParameter("Host");
			GameObject gameObject = E.GetGameObjectParameter("Actor") ?? E.GetGameObjectParameter("Owner");
			if (gameObjectParameter4 != null && gameObject != null)
			{
				int freeDrams = gameObject.GetFreeDrams(Liquid, ParentObject);
				int storableDrams = gameObjectParameter4.GetStorableDrams(Liquid);
				int num = Math.Min(freeDrams, storableDrams);
				if (gameObject.IsPlayer())
				{
					if (num > 0)
					{
						if (E.HasFlag("TrackSupply"))
						{
							E.SetFlag("AnySupplies", State: true);
						}
						num.ToString();
						int? num2 = Popup.AskNumber("Supply " + gameObjectParameter4.the + gameObjectParameter4.ShortDisplayName + " with how many drams of your " + Liquid + "? (max=" + num + ")", num, 0, num);
						int num3 = 0;
						try
						{
							num3 = Convert.ToInt32(num2);
						}
						catch
						{
							goto IL_0832;
						}
						if (num3 > num)
						{
							num3 = num;
						}
						if (num3 < 0)
						{
							num3 = 0;
						}
						num = num3;
					}
					else if (freeDrams <= 0)
					{
						Popup.Show("You have no " + Liquid + " to supply " + gameObjectParameter4.the + gameObjectParameter4.ShortDisplayName + " with.");
					}
					else if (storableDrams <= 0)
					{
						Popup.Show(gameObjectParameter4.The + gameObjectParameter4.ShortDisplayName + gameObjectParameter4.GetVerb("have") + " no room for more " + Liquid + ".");
					}
				}
				if (num > 0)
				{
					IComponent<GameObject>.XDidYToZ(gameObject, "transfer", Grammar.Cardinal(num) + " " + ((num == 1) ? "dram" : "drams") + " of " + Liquid + " to", gameObjectParameter4, null, null, null, null, null, UseFullNames: false, IndefiniteSubject: false, IndefiniteObject: false, IndefiniteObjectForOthers: true, PossessiveObject: false, null, null, null, DescribeSubjectDirection: false, DescribeSubjectDirectionLate: false, AlwaysVisible: false, FromDialog: true);
					gameObject.UseDrams(num, Liquid);
					gameObjectParameter4.GiveDrams(num, Liquid);
					gameObject.UseEnergy(1000, "Ammo Liquid Transfer");
				}
			}
		}
		else if (E.ID == "GenerateIntegratedHostInitialAmmo")
		{
			GameObject gameObjectParameter5 = E.GetGameObjectParameter("Host");
			if (gameObjectParameter5 != null)
			{
				int storableDrams2 = gameObjectParameter5.GetStorableDrams(Liquid);
				if (storableDrams2 > 0)
				{
					gameObjectParameter5.GiveDrams(storableDrams2, Liquid);
				}
			}
		}
		goto IL_0832;
		IL_0832:
		return base.FireEvent(E);
	}
}
