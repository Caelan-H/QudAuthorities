using System;
using System.Text;
using ConsoleLib.Console;
using XRL.Language;
using XRL.UI;

namespace XRL.World.Parts;

[Serializable]
public class ModLiquidCooled : IModification
{
	public const int DEFAULT_LIQUID_CONSUMPTION_CHANCE_BASE = 15;

	public string PercentBonusRange = "30-60";

	public int PercentBonus;

	public int AmountBonus;

	public string LiquidConsumed = "water";

	public bool RequiresPureLiquid = true;

	public int LiquidConsumptionChanceBase = 15;

	public ModLiquidCooled()
	{
		base.IsTechScannable = true;
		NameForStatus = "CoolantSystem";
	}

	public ModLiquidCooled(int Tier)
		: base(Tier)
	{
	}

	public override void Configure()
	{
		WorksOnSelf = true;
	}

	public override bool ModificationApplicable(GameObject Object)
	{
		if (!(Object.GetPart("MissileWeapon") is MissileWeapon missileWeapon))
		{
			return false;
		}
		if (missileWeapon.ShotsPerAction <= 1)
		{
			return false;
		}
		if (missileWeapon.ShotsPerAction != missileWeapon.AmmoPerAction)
		{
			return false;
		}
		return true;
	}

	public override bool BeingAppliedBy(GameObject Object, GameObject Who)
	{
		if (Who.IsPlayer())
		{
			EnforceLiquidVolume(Object).Empty();
		}
		return true;
	}

	public override void ApplyModification(GameObject Object)
	{
		if (Object.GetPart("MissileWeapon") is MissileWeapon missileWeapon)
		{
			if (AmountBonus == 0)
			{
				if (PercentBonus == 0)
				{
					PercentBonus = PercentBonusRange.RollCached();
				}
				AmountBonus = Math.Max(missileWeapon.ShotsPerAction * PercentBonus / 100, 1);
			}
			if (missileWeapon.ShotsPerAnimation == missileWeapon.ShotsPerAction)
			{
				missileWeapon.ShotsPerAnimation += AmountBonus;
			}
			missileWeapon.ShotsPerAction += AmountBonus;
			missileWeapon.AmmoPerAction += AmountBonus;
		}
		EnforceLiquidVolume(Object);
		IncreaseDifficultyAndComplexity(2, 2);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != AllowLiquidCollectionEvent.ID && ID != CommandReloadEvent.ID && ID != GetDisplayNameEvent.ID && ID != GetPreferredLiquidEvent.ID && ID != GetShortDescriptionEvent.ID && ID != NeedsReloadEvent.ID)
		{
			return ID == WantsLiquidCollectionEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(NeedsReloadEvent E)
	{
		if (E.Skip != this && (E.Weapon == null || E.Weapon == ParentObject))
		{
			LiquidVolume liquidVolume = ParentObject.LiquidVolume;
			if (liquidVolume != null && (!liquidVolume.IsPureLiquid(LiquidConsumed) || liquidVolume.Volume < liquidVolume.MaxVolume) && ParentObject.IsEquippedProperly())
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
			if (liquidVolume.IsPureLiquid(LiquidConsumed) && liquidVolume.Volume >= liquidVolume.MaxVolume)
			{
				if (E.Actor.IsPlayer())
				{
					IComponent<GameObject>.AddPlayerMessage(ParentObject.The + ParentObject.ShortDisplayName + ParentObject.Is + " already full of " + liquidVolume.GetLiquidName() + ".");
				}
				return true;
			}
			E.NeededReload.Add(this);
			int freeDrams = E.Actor.GetFreeDrams(LiquidConsumed, ParentObject, null, (GameObject o) => !o.HasPart("ModLiquidCooled"));
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
			if (liquidVolume.Volume > 0 && !liquidVolume.IsPureLiquid(LiquidConsumed))
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
			E.Actor.UseDrams(num, LiquidConsumed, ParentObject, null, (GameObject o) => !o.HasPart("ModLiquidCooled"));
			liquidVolume.MixWith(new LiquidVolume(LiquidConsumed, num));
			E.Reloaded.Add(this);
			if (!E.ObjectsReloaded.Contains(ParentObject))
			{
				E.ObjectsReloaded.Add(ParentObject);
			}
			E.EnergyCost(1000);
			if (E.Actor.IsPlayer())
			{
				IComponent<GameObject>.PlayUISound("SplashStep1");
			}
			if (E.Actor.IsPlayer())
			{
				IComponent<GameObject>.AddPlayerMessage("You " + ((liquidVolume.Volume < liquidVolume.MaxVolume) ? "partially " : "") + "fill " + ParentObject.the + displayNameOnly + " with " + liquidVolume.GetLiquidName() + ".");
			}
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

	public override bool HandleEvent(GetDisplayNameEvent E)
	{
		if (E.Understood() && !E.Object.HasProperName)
		{
			E.AddAdjective("{{" + LiquidVolume.getLiquid(LiquidConsumed).GetColor() + "|liquid-cooled}}");
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetShortDescriptionEvent E)
	{
		E.Base.Compound(GetPhysicalDescription());
		E.Postfix.AppendRules(GetInstanceDescription());
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
		Object.RegisterPartEvent(this, "CheckLoadAmmo");
		Object.RegisterPartEvent(this, "LoadAmmo");
		Object.RegisterPartEvent(this, "PrepIntegratedHostToReceiveAmmo");
		Object.RegisterPartEvent(this, "SupplyIntegratedHostWithAmmo");
		base.Register(Object);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "CheckReadyToFire")
		{
			if (!LiquidAvailable(LiquidConsumed, 1, !RequiresPureLiquid))
			{
				return false;
			}
		}
		else if (E.ID == "CheckLoadAmmo")
		{
			if (!LiquidAvailable(LiquidConsumed, 1, !RequiresPureLiquid))
			{
				GameObject gameObjectParameter = E.GetGameObjectParameter("Loader");
				if (gameObjectParameter != null && gameObjectParameter.IsPlayer())
				{
					IComponent<GameObject>.AddPlayerMessage(GetEmptyMessage());
				}
				return false;
			}
		}
		else if (E.ID == "LoadAmmo")
		{
			if (GetLiquidConsumptionChance().in100())
			{
				ConsumeLiquid(LiquidConsumed, 1, !RequiresPureLiquid);
			}
		}
		else if (E.ID == "GetNotReadyToFireMessage")
		{
			if (!LiquidAvailable(LiquidConsumed, 1, !RequiresPureLiquid))
			{
				E.SetParameter("Message", GetEmptyMessage());
			}
		}
		else if (E.ID == "AIWantUseWeapon")
		{
			LiquidVolume liquidVolume = ParentObject.LiquidVolume;
			if (liquidVolume != null && liquidVolume.IsPureLiquid(LiquidConsumed) && liquidVolume.Volume > 0)
			{
				return true;
			}
			Inventory inventory = E.GetGameObjectParameter("Object").Inventory;
			bool flag = false;
			if (inventory != null)
			{
				foreach (GameObject item in inventory.GetObjectsDirect())
				{
					LiquidVolume liquidVolume2 = item.LiquidVolume;
					if (liquidVolume2 != null && liquidVolume2.IsPureLiquid(LiquidConsumed) && liquidVolume2.Volume > 0)
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
				LiquidVolume liquidVolume3 = ParentObject.LiquidVolume;
				string primaryLiquidColor = liquidVolume3.GetPrimaryLiquidColor();
				if (liquidVolume3.Volume == 0)
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
				else if (liquidVolume3.IsPureLiquid(LiquidConsumed))
				{
					if (primaryLiquidColor != null)
					{
						stringBuilder.Append(" [{{").Append(primaryLiquidColor).Append('|')
							.Append(liquidVolume3.Volume)
							.Append("}}]");
					}
					else
					{
						stringBuilder.Append(" [").Append(liquidVolume3.Volume).Append("]");
					}
				}
				else if (primaryLiquidColor != null)
				{
					stringBuilder.Append(" [{{").Append(primaryLiquidColor).Append("|?}}]");
				}
				else
				{
					stringBuilder.Append(" [?]");
				}
			}
		}
		else if (E.ID == "PrepIntegratedHostToReceiveAmmo")
		{
			GameObject gameObjectParameter2 = E.GetGameObjectParameter("Host");
			if (gameObjectParameter2 != null)
			{
				Body body = gameObjectParameter2.Body;
				if (body != null)
				{
					foreach (BodyPart item2 in body.GetPart("Arm"))
					{
						if (item2.Equipped == null)
						{
							gameObjectParameter2.ForceEquipObject(GameObject.create("StorageTank"), item2, Silent: true, 0);
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
			GameObject gameObjectParameter3 = E.GetGameObjectParameter("Host");
			GameObject gameObject = E.GetGameObjectParameter("Actor") ?? E.GetGameObjectParameter("Owner");
			if (gameObjectParameter3 != null && gameObject != null)
			{
				int freeDrams = gameObject.GetFreeDrams(LiquidConsumed, ParentObject);
				int storableDrams = gameObjectParameter3.GetStorableDrams(LiquidConsumed);
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
						int? num2 = Popup.AskNumber("Supply " + gameObjectParameter3.the + gameObjectParameter3.ShortDisplayName + " with how many drams of your " + LiquidConsumed + "? (max=" + num + ")", num, 0, num);
						int num3 = 0;
						try
						{
							num3 = Convert.ToInt32(num2);
						}
						catch
						{
							goto IL_06ba;
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
						Popup.Show("You have no " + LiquidConsumed + " to supply " + gameObjectParameter3.the + gameObjectParameter3.ShortDisplayName + " with.");
					}
					else if (storableDrams <= 0)
					{
						Popup.Show(gameObjectParameter3.The + gameObjectParameter3.ShortDisplayName + gameObjectParameter3.GetVerb("have") + " no room for more " + LiquidConsumed + ".");
					}
				}
				if (num > 0)
				{
					IComponent<GameObject>.XDidYToZ(gameObject, "transfer", Grammar.Cardinal(num) + " " + ((num == 1) ? "dram" : "drams") + " of " + LiquidConsumed + " to", gameObjectParameter3, null, null, null, null, null, UseFullNames: false, IndefiniteSubject: false, IndefiniteObject: false, IndefiniteObjectForOthers: true, PossessiveObject: false, null, null, null, DescribeSubjectDirection: false, DescribeSubjectDirectionLate: false, AlwaysVisible: false, FromDialog: true);
					gameObject.UseDrams(num, LiquidConsumed);
					gameObjectParameter3.GiveDrams(num, LiquidConsumed);
					gameObject.UseEnergy(1000, "Ammo Liquid Transfer");
				}
			}
		}
		else if (E.ID == "GenerateIntegratedHostInitialAmmo")
		{
			GameObject gameObjectParameter4 = E.GetGameObjectParameter("Host");
			if (gameObjectParameter4 != null)
			{
				int storableDrams2 = gameObjectParameter4.GetStorableDrams(LiquidConsumed);
				if (storableDrams2 > 0)
				{
					gameObjectParameter4.GiveDrams(storableDrams2, LiquidConsumed);
				}
			}
		}
		goto IL_06ba;
		IL_06ba:
		return base.FireEvent(E);
	}

	public string GetPhysicalDescription()
	{
		return "";
	}

	public static string GetDescription(int Tier)
	{
		return "Liquid-cooled: This weapon's rate of fire is increased, but it requires pure water to function. When fired, there's " + Grammar.AOrAnBeforeNumber(GetLiquidConsumptionChance(Tier)) + " " + GetLiquidConsumptionChance(Tier) + "% chance that 1 dram is consumed.";
	}

	public string GetInstanceDescription()
	{
		return "Liquid-cooled: This weapon's rate of fire is increased by " + AmountBonus + ", but " + ParentObject.it + " requires " + (RequiresPureLiquid ? "pure " : "") + " " + ColorUtility.StripFormatting(LiquidVolume.getLiquid(LiquidConsumed).GetName(null)) + " to function. When fired, there's " + Grammar.AOrAnBeforeNumber(GetLiquidConsumptionChance()) + " " + GetLiquidConsumptionChance() + "% chance that 1 dram is consumed.";
	}

	public string GetEmptyMessage()
	{
		return ParentObject.The + ParentObject.DisplayNameOnly + ParentObject.GetVerb("emit") + " a grinding noise.";
	}

	public override bool GetActivePartLocallyDefinedFailure()
	{
		if (!string.IsNullOrEmpty(LiquidConsumed))
		{
			return !LiquidAvailable(LiquidConsumed, 1, !RequiresPureLiquid);
		}
		return false;
	}

	public override string GetActivePartLocallyDefinedFailureDescription()
	{
		return "ProcessInputMissing";
	}

	public bool IsLiquidCollectionCompatible(string LiquidType)
	{
		return LiquidConsumed == LiquidType;
	}

	public LiquidVolume EnforceLiquidVolume(GameObject Object)
	{
		int liquidVolumeSize = GetLiquidVolumeSize(Object);
		if (liquidVolumeSize == 0)
		{
			return null;
		}
		LiquidVolume liquidVolume = Object.LiquidVolume;
		if (liquidVolume == null)
		{
			liquidVolume = new LiquidVolume();
			liquidVolume.MaxVolume = liquidVolumeSize;
			liquidVolume.Volume = liquidVolumeSize;
			liquidVolume.SetComponent(LiquidConsumed, 1000);
			Object.AddPart(liquidVolume);
		}
		else if (liquidVolume.MaxVolume < liquidVolumeSize)
		{
			liquidVolume.MaxVolume = liquidVolumeSize;
		}
		return liquidVolume;
	}

	public LiquidVolume EnforceLiquidVolume()
	{
		return EnforceLiquidVolume(ParentObject);
	}

	public static int GetLiquidVolumeSizeForShotsPerAction(int shots)
	{
		if (shots <= 1)
		{
			return 0;
		}
		int num = shots * 2;
		int num2 = num % 8;
		if (num2 != 0)
		{
			num += 8 - num2;
		}
		return num;
	}

	public static int GetLiquidVolumeSize(GameObject Object)
	{
		if (!(Object.GetPart("MissileWeapon") is MissileWeapon missileWeapon))
		{
			return 0;
		}
		return GetLiquidVolumeSizeForShotsPerAction(missileWeapon.ShotsPerAction);
	}

	public int GetLiquidVolumeSize()
	{
		return GetLiquidVolumeSize(ParentObject);
	}

	public static int GetLiquidConsumptionChance(int Tier)
	{
		return 15 - Tier;
	}

	public int GetLiquidConsumptionChance()
	{
		return LiquidConsumptionChanceBase - Tier;
	}
}
