using System;
using System.Text;
using XRL.Language;
using XRL.UI;

namespace XRL.World.Parts;

[Serializable]
public class BioAmmoLoader : IActivePart
{
	public int MaxCapacity = 12;

	public int Available = 12;

	public int TurnsToGenerate = 5;

	public int ReloadEnergy = 1000;

	public string ProjectileObject;

	public string LiquidConsumed;

	public bool ConsumePure = true;

	public int ConsumeAmount = 1;

	public int ConsumeChance = 100;

	public float TurnsToGenerateComputePowerFactor;

	public int TurnsGenerating;

	public BioAmmoLoader()
	{
		WorksOnSelf = true;
		base.IsBioScannable = true;
	}

	public override bool SameAs(IPart p)
	{
		BioAmmoLoader bioAmmoLoader = p as BioAmmoLoader;
		if (bioAmmoLoader.MaxCapacity != MaxCapacity)
		{
			return false;
		}
		if (bioAmmoLoader.Available != Available)
		{
			return false;
		}
		if (bioAmmoLoader.TurnsToGenerate != TurnsToGenerate)
		{
			return false;
		}
		if (bioAmmoLoader.ProjectileObject != ProjectileObject)
		{
			return false;
		}
		if (bioAmmoLoader.LiquidConsumed != LiquidConsumed)
		{
			return false;
		}
		if (bioAmmoLoader.ConsumePure != ConsumePure)
		{
			return false;
		}
		if (bioAmmoLoader.ConsumeAmount != ConsumeAmount)
		{
			return false;
		}
		if (bioAmmoLoader.ConsumeChance != ConsumeChance)
		{
			return false;
		}
		if (bioAmmoLoader.TurnsToGenerateComputePowerFactor != TurnsToGenerateComputePowerFactor)
		{
			return false;
		}
		return base.SameAs(p);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && (ID != AllowLiquidCollectionEvent.ID || string.IsNullOrEmpty(LiquidConsumed)) && ID != CommandReloadEvent.ID && ID != EndTurnEvent.ID && ID != GetDisplayNameEvent.ID && ID != GetMissileWeaponProjectileEvent.ID && (ID != GetPreferredLiquidEvent.ID || string.IsNullOrEmpty(LiquidConsumed)) && ID != GetProjectileBlueprintEvent.ID && ID != GetShortDescriptionEvent.ID && ID != NeedsReloadEvent.ID)
		{
			if (ID == WantsLiquidCollectionEvent.ID)
			{
				return !string.IsNullOrEmpty(LiquidConsumed);
			}
			return false;
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

	public override bool HandleEvent(GetShortDescriptionEvent E)
	{
		if (TurnsToGenerateComputePowerFactor > 0f)
		{
			E.Postfix.AppendRules("Compute power on the local lattice decreases the time needed for this item to generate ammunition.");
		}
		else if (TurnsToGenerateComputePowerFactor < 0f)
		{
			E.Postfix.AppendRules("Compute power on the local lattice increases the time needed for this item to generate ammunition.");
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(NeedsReloadEvent E)
	{
		if (!string.IsNullOrEmpty(LiquidConsumed) && E.Skip != this && (E.Weapon == null || E.Weapon == ParentObject))
		{
			string liquidConsumed = LiquidConsumed;
			LiquidVolume liquidVolume = ParentObject.LiquidVolume;
			if (liquidVolume != null && (!CorrectLiquid(liquidConsumed, liquidVolume) || liquidVolume.Volume < liquidVolume.MaxVolume) && ParentObject.IsEquippedProperly())
			{
				return false;
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(CommandReloadEvent E)
	{
		if (!string.IsNullOrEmpty(LiquidConsumed) && (E.Weapon == null || E.Weapon == ParentObject) && !E.CheckedForReload.Contains(this))
		{
			E.CheckedForReload.Add(this);
			if (!ParentObject.IsEquippedProperly() || E.MinimumCharge > 0)
			{
				return true;
			}
			string liquidConsumed = LiquidConsumed;
			LiquidVolume liquidVolume = ParentObject.LiquidVolume;
			if (liquidVolume == null)
			{
				return true;
			}
			if (CorrectLiquid(liquidConsumed, liquidVolume) && liquidVolume.Volume >= liquidVolume.MaxVolume)
			{
				if (E.Actor.IsPlayer())
				{
					IComponent<GameObject>.AddPlayerMessage(ParentObject.The + ParentObject.ShortDisplayName + ParentObject.Is + " already full.");
				}
				return true;
			}
			E.NeededReload.Add(this);
			int freeDrams = E.Actor.GetFreeDrams(LiquidConsumed, ParentObject);
			if (freeDrams <= 0)
			{
				if (E.Actor.IsPlayer())
				{
					IComponent<GameObject>.AddPlayerMessage("You have no " + LiquidConsumed + " for " + ParentObject.the + ParentObject.ShortDisplayName + ".", 'r');
				}
				return true;
			}
			E.TriedToReload.Add(this);
			string displayNameOnly = ParentObject.DisplayNameOnly;
			if (liquidVolume.Volume > 0 && !CorrectLiquid(liquidConsumed, liquidVolume))
			{
				if (E.Actor.IsPlayer())
				{
					IComponent<GameObject>.AddPlayerMessage("You dump the " + liquidVolume.GetLiquidName() + " out of " + ParentObject.the + displayNameOnly + ".");
				}
				liquidVolume.EmptyIntoCell();
				displayNameOnly = ParentObject.DisplayNameOnly;
			}
			int val = liquidVolume.MaxVolume - liquidVolume.Volume;
			int num = Math.Min(freeDrams, val);
			E.Actor.UseDrams(num, LiquidConsumed, ParentObject);
			liquidVolume.MixWith(new LiquidVolume(liquidConsumed, num));
			E.Reloaded.Add(this);
			if (!E.ObjectsReloaded.Contains(ParentObject))
			{
				E.ObjectsReloaded.Add(ParentObject);
			}
			E.EnergyCost(ReloadEnergy);
			PlayWorldSound(ParentObject.GetTag("ReloadSound"));
			if (E.Actor.IsPlayer())
			{
				IComponent<GameObject>.AddPlayerMessage("You " + ((liquidVolume.Volume < liquidVolume.MaxVolume) ? "partially " : "") + "fill " + ParentObject.the + displayNameOnly + " with " + liquidVolume.GetLiquidName() + ".");
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
			E.Liquid = LiquidConsumed;
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

	public override bool HandleEvent(EndTurnEvent E)
	{
		if (Available < MaxCapacity && IsReady(UseCharge: true, IgnoreCharge: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
		{
			TurnsGenerating++;
			int num = GetAvailableComputePowerEvent.AdjustDown(this, TurnsToGenerate, TurnsToGenerateComputePowerFactor);
			if (TurnsGenerating >= num)
			{
				Available++;
				TurnsGenerating = 0;
				if (!string.IsNullOrEmpty(LiquidConsumed) && ConsumeChance.in100())
				{
					ConsumeLiquid(LiquidConsumed, ConsumeAmount, !ConsumePure);
				}
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetDisplayNameEvent E)
	{
		if (E.Understood())
		{
			E.AddMissileWeaponDamageTag(GetMissileWeaponPerformanceEvent.GetFor(ParentObject.Equipped, ParentObject));
			if (E.Context != "Tinkering")
			{
				if (Available <= 0)
				{
					E.AddTag("{{y|[{{K|empty}}]}}", -5);
				}
				else
				{
					E.AddTag("{{y|[" + Available + "]}}", -5);
				}
			}
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
		Object.RegisterPartEvent(this, "CheckLoadAmmo");
		Object.RegisterPartEvent(this, "CheckReadyToFire");
		Object.RegisterPartEvent(this, "GenerateIntegratedHostInitialAmmo");
		Object.RegisterPartEvent(this, "GetMissileWeaponStatus");
		Object.RegisterPartEvent(this, "GetNotReadyToFireMessage");
		Object.RegisterPartEvent(this, "LoadAmmo");
		Object.RegisterPartEvent(this, "PrepIntegratedHostToReceiveAmmo");
		Object.RegisterPartEvent(this, "SupplyIntegratedHostWithAmmo");
		base.Register(Object);
	}

	public override bool GetActivePartLocallyDefinedFailure()
	{
		if (!string.IsNullOrEmpty(LiquidConsumed))
		{
			return !LiquidAvailable(LiquidConsumed, 1, !ConsumePure);
		}
		return false;
	}

	public override string GetActivePartLocallyDefinedFailureDescription()
	{
		return "ProcessInputMissing";
	}

	private bool CorrectLiquid(string LiquidID, LiquidVolume Volume)
	{
		if (!Volume.ComponentLiquids.ContainsKey(LiquidID))
		{
			return false;
		}
		if (ConsumePure && !Volume.IsPureLiquid(LiquidID))
		{
			return false;
		}
		return true;
	}

	public bool IsLiquidCollectionCompatible(string LiquidType)
	{
		if (string.IsNullOrEmpty(LiquidConsumed))
		{
			return true;
		}
		if (LiquidConsumed == LiquidType)
		{
			return true;
		}
		if (!ConsumePure && LiquidType.IndexOf(',') != -1 && LiquidType.IndexOf(LiquidConsumed) != -1)
		{
			foreach (string item in LiquidType.CachedCommaExpansion())
			{
				if (item.Split('-')[0] == LiquidConsumed)
				{
					return true;
				}
			}
		}
		return false;
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "CheckReadyToFire")
		{
			if (Available <= 0)
			{
				return false;
			}
		}
		else if (E.ID == "GetNotReadyToFireMessage")
		{
			if (Available <= 0)
			{
				E.SetParameter("Message", ParentObject.The + ParentObject.ShortDisplayName + ParentObject.Is + " exhausted!");
			}
		}
		else if (E.ID == "AIWantUseWeapon")
		{
			if (Available <= 0)
			{
				return false;
			}
		}
		else if (E.ID == "GetMissileWeaponStatus")
		{
			if (!E.HasParameter("Override"))
			{
				StringBuilder stringBuilder = E.GetParameter("Items") as StringBuilder;
				if (Available <= 0)
				{
					stringBuilder.Append(" [{{K|empty}}]");
				}
				else
				{
					stringBuilder.Append(" [").Append(Available).Append(']');
				}
			}
		}
		else if (E.ID == "CheckLoadAmmo")
		{
			if (Available <= 0)
			{
				return false;
			}
		}
		else if (E.ID == "LoadAmmo")
		{
			if (Available <= 0)
			{
				if (E.GetGameObjectParameter("Loader").IsPlayer())
				{
					IComponent<GameObject>.AddPlayerMessage(ParentObject.The + ParentObject.ShortDisplayName + ParentObject.Is + " exhausted!", 'r');
				}
				E.SetParameter("Ammo", (object)null);
				return false;
			}
			if (ProjectileObject != null)
			{
				E.SetParameter("Ammo", GameObject.create(ProjectileObject));
				Available--;
			}
		}
		else if (E.ID == "PrepIntegratedHostToReceiveAmmo")
		{
			if (!string.IsNullOrEmpty(LiquidConsumed))
			{
				GameObject gameObjectParameter = E.GetGameObjectParameter("Host");
				if (gameObjectParameter != null)
				{
					Body body = gameObjectParameter.Body;
					if (body != null)
					{
						foreach (BodyPart item in body.GetPart("Arm"))
						{
							if (item.Equipped == null)
							{
								gameObjectParameter.ForceEquipObject(GameObject.create("Gourd"), item, Silent: true, 0);
							}
						}
					}
				}
			}
		}
		else if (E.ID == "SupplyIntegratedHostWithAmmo")
		{
			if (!string.IsNullOrEmpty(LiquidConsumed))
			{
				if (E.HasFlag("TrackSupply"))
				{
					E.SetFlag("AnySupplyHandler", State: true);
				}
				GameObject gameObjectParameter2 = E.GetGameObjectParameter("Host");
				GameObject gameObject = E.GetGameObjectParameter("Actor") ?? E.GetGameObjectParameter("Owner");
				if (gameObjectParameter2 != null && gameObject != null)
				{
					int freeDrams = gameObject.GetFreeDrams(LiquidConsumed, ParentObject);
					int storableDrams = gameObjectParameter2.GetStorableDrams(LiquidConsumed);
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
							int? num2 = Popup.AskNumber("Supply " + gameObjectParameter2.the + gameObjectParameter2.ShortDisplayName + " with how many drams of your " + LiquidConsumed + "? (max=" + num + ")", num, 0, num);
							int num3 = 0;
							try
							{
								num3 = Convert.ToInt32(num2);
							}
							catch
							{
								goto IL_059a;
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
							Popup.Show("You have no " + LiquidConsumed + " to supply " + gameObjectParameter2.the + gameObjectParameter2.ShortDisplayName + " with.");
						}
						else if (storableDrams <= 0)
						{
							Popup.Show(gameObjectParameter2.The + gameObjectParameter2.ShortDisplayName + " has no room for more " + LiquidConsumed + ".");
						}
					}
					if (num > 0)
					{
						IComponent<GameObject>.XDidYToZ(gameObject, "transfer", Grammar.Cardinal(num) + " " + ((num == 1) ? "dram" : "drams") + " of " + LiquidConsumed + " to", gameObjectParameter2, null, null, null, null, null, UseFullNames: false, IndefiniteSubject: false, IndefiniteObject: false, IndefiniteObjectForOthers: true, PossessiveObject: false, null, null, null, DescribeSubjectDirection: false, DescribeSubjectDirectionLate: false, AlwaysVisible: false, FromDialog: true);
						gameObject.UseDrams(num, LiquidConsumed);
						gameObjectParameter2.GiveDrams(num, LiquidConsumed);
						gameObject.UseEnergy(1000, "Ammo Liquid Transfer");
					}
				}
			}
		}
		else if (E.ID == "GenerateIntegratedHostInitialAmmo" && !string.IsNullOrEmpty(LiquidConsumed))
		{
			GameObject gameObjectParameter3 = E.GetGameObjectParameter("Host");
			if (gameObjectParameter3 != null)
			{
				int storableDrams2 = gameObjectParameter3.GetStorableDrams(LiquidConsumed);
				if (storableDrams2 > 0)
				{
					gameObjectParameter3.GiveDrams(storableDrams2, LiquidConsumed);
				}
			}
		}
		goto IL_059a;
		IL_059a:
		return base.FireEvent(E);
	}
}
